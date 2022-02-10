using Common;
using Common.Data;
using Common.Utils;
using GameServer.Entities;
using GameServer.Managers;
using GameServer.Models;
using GameServer.Services;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    /// <summary>
    /// 竞技场
    /// </summary>
    class Arena
    {
        const float READY_TIME = 11f;
        const float ROUND_TIME = 60f;
        const float RESULT_TIME = 5f;

        /// <summary>
        /// 所属地图
        /// </summary>
        public Map Map;
        /// <summary>
        /// 竞技信息
        /// </summary>
        public ArenaInfo ArenaInfo;
        /// <summary>
        /// 挑战者
        /// </summary>
        public NetConnection<NetSession> Red;
        /// <summary>
        /// 被挑战者
        /// </summary>
        public NetConnection<NetSession> Blue;
        /// <summary>
        /// 挑战者原地图
        /// </summary>
        private Map SourceMapRed;
        /// <summary>
        /// 被挑战者原地图
        /// </summary>
        private Map SourceMapBlue;
        /// <summary>
        /// 红方传送点
        /// </summary>
        private int redPoint = 9;
        /// <summary>
        /// 蓝方传送点
        /// </summary>
        private int bluePoint = 10;
        /// <summary>
        /// 红方是否准备完成
        /// </summary>
        private bool redReady;
        /// <summary>
        /// 蓝方是否准备完成
        /// </summary>
        private bool blueReady;
        /// <summary>
        /// 双方是否准备完成
        /// </summary>
        public bool Ready { get { return this.redReady && this.blueReady; } }
        /// <summary>
        /// 计时器
        /// </summary>
        private float timer = 0f;

        public int Round { get; internal set; }
        /// <summary>
        /// 竞技场状态
        /// </summary>
        private ArenaStatus ArenaStatus;
        /// <summary>
        /// 竞技场回合状态
        /// </summary>
        private ArenaRoundStatus RoundStatus;

        public Arena(Map map, ArenaInfo info, NetConnection<NetSession> red, NetConnection<NetSession> blue)
        {
            info.ArenaId = map.InstanceId;
            this.Map = map;
            this.ArenaInfo = info;
            this.Red = red;
            this.Blue = blue;
            this.ArenaStatus = ArenaStatus.Wait;
            this.RoundStatus = ArenaRoundStatus.None;
        }

        /// <summary>
        /// 双方进入竞技场
        /// </summary>
        internal void PlayerEnter()
        {
            this.SourceMapRed = this.PlayerLeaveMap(this.Red);
            this.SourceMapBlue = this.PlayerLeaveMap(this.Blue);
            this.PlayerEnterArena();
        }

        /// <summary>
        /// 玩家离开地图
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private Map PlayerLeaveMap(NetConnection<NetSession> player)
        {
            var currentMap = MapManager.Instance[player.Session.Character.Info.mapId];
            currentMap.CharacterLeave(player.Session.Character);
            EntityManager.Instance.RemoveMapEntity(currentMap.ID, currentMap.InstanceId, player.Session.Character);
            return currentMap; 
        }

        /// <summary>
        /// 进入竞技场
        /// </summary>
        private void PlayerEnterArena()
        {
            TeleporterDefine redTeleporter = DataManager.Instance.Teleporters[this.redPoint];
            this.Red.Session.Character.Position = redTeleporter.Position;
            this.Red.Session.Character.Direction = redTeleporter.Direction;
            TeleporterDefine blueTeleporter = DataManager.Instance.Teleporters[this.bluePoint];
            this.Blue.Session.Character.Position = blueTeleporter.Position;
            this.Blue.Session.Character.Direction = blueTeleporter.Direction;
            //客户端被动切换地图，保证双方都能收到进入地图的响应
            this.Map.AddCharacter(this.Red, this.Red.Session.Character);
            this.Map.AddCharacter(this.Blue, this.Blue.Session.Character);
            this.Map.CharacterEnter(this.Red, this.Red.Session.Character);
            this.Map.CharacterEnter(this.Blue, this.Blue.Session.Character);
            EntityManager.Instance.AddMapEntity(this.Map.ID, this.Map.InstanceId, this.Red.Session.Character);
            EntityManager.Instance.AddMapEntity(this.Map.ID, this.Map.InstanceId, this.Blue.Session.Character);
        }

        /// <summary>
        /// 双方准备
        /// </summary>
        /// <param name="entityId"></param>
        internal void EntityReady(int entityId)
        {
            if(this.Red.Session.Character.entityId == entityId)
            {
                this.redReady = true;
            }
            if(this.Blue.Session.Character.entityId == entityId)
            {
                this.blueReady = true;
            }

            if (this.Ready)
            {
                this.ArenaStatus = ArenaStatus.Game;
                this.Round = 0;
                this.NextRound();
            }
        }

        /// <summary>
        /// 进入下一回合
        /// </summary>
        private void NextRound()
        {
            this.Round++;
            this.timer = READY_TIME;
            this.RoundStatus = ArenaRoundStatus.Ready;
            Log.InfoFormat("Arena:[{0}] Round[{1}] Ready", this.ArenaInfo.ArenaId, this.Round);
            ArenaService.Instance.SendArenaReady(this);
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void Update()
        {
            if(this.ArenaStatus == ArenaStatus.Game)
            {
                UpdateRound();
            }
        }

        /// <summary>
        /// 更新回合
        /// </summary>
        private void UpdateRound()
        {
            if(this.RoundStatus == ArenaRoundStatus.Ready)
            {
                this.timer -= Time.deltaTime;
                if(timer < 0)
                {
                    this.RoundStatus = ArenaRoundStatus.Fight;
                    this.timer = ROUND_TIME;
                    Log.InfoFormat("Arena:[{0}] Round Start", this.ArenaInfo.ArenaId);
                    ArenaService.Instance.SendArenaRoundStart(this);
                }
            }
            else if (this.RoundStatus == ArenaRoundStatus.Fight)
            {
                this.timer -= Time.deltaTime;
                if (timer < 0)
                {
                    this.RoundStatus = ArenaRoundStatus.Result;
                    this.timer = RESULT_TIME;
                    Log.InfoFormat("Arena:[{0}] Round End", this.ArenaInfo.ArenaId);
                    ArenaService.Instance.SendArenaRoundEnd(this);
                }
            }
            else if (this.RoundStatus == ArenaRoundStatus.Result)
            {
                this.timer -= Time.deltaTime;
                if (timer < 0)
                {
                    if(this.Round >= 3)
                    {
                        ArenaResult();
                    }
                    else
                    {
                        NextRound();
                    }
                }
            }
        }

        /// <summary>
        /// 竞技场结果
        /// </summary>
        private void ArenaResult()
        {
            this.ArenaStatus = ArenaStatus.Result;
            this.RoundStatus = ArenaRoundStatus.None;
            //执行结算
        }
    }
}
