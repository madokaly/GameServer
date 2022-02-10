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
    /// 剧情副本
    /// </summary>
    class Story
    {
        const float READY_TIME = 11f;
        const float ROUND_TIME = 60f;
        const float RESULT_TIME = 5f;

        /// <summary>
        /// 所属地图
        /// </summary>
        public Map Map;
        /// <summary>
        /// 地图Id
        /// </summary>
        public int StoryId;
        /// <summary>
        /// 副本Id
        /// </summary>
        public int InstanceId;
        /// <summary>
        /// 玩家链接
        /// </summary>
        public NetConnection<NetSession> Player;
        /// <summary>
        /// 原地图
        /// </summary>
        private Map SourceMap;
        /// <summary>
        /// 传送点
        /// </summary>
        private int startPoint = 12;


        /// <summary>
        /// 计时器
        /// </summary>
        private float timer = 0f;

        public Story(Map map, int storyId, int instanceId, NetConnection<NetSession> owner)
        {
            this.Map = map;
            this.StoryId = storyId;
            this.InstanceId = instanceId;
            this.Player = owner;
        }

        /// <summary>
        /// 进入剧情副本
        /// </summary>
        internal void PlayerEnter()
        {
            this.SourceMap = this.PlayerLeaveMap(this.Player);
            this.PlayerEnterStory();
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
        private void PlayerEnterStory()
        {
            TeleporterDefine redTeleporter = DataManager.Instance.Teleporters[this.startPoint];
            this.Player.Session.Character.Position = redTeleporter.Position;
            this.Player.Session.Character.Direction = redTeleporter.Direction;
            //客户端被动切换地图
            this.Map.AddCharacter(this.Player, this.Player.Session.Character);
            this.Map.CharacterEnter(this.Player, this.Player.Session.Character);
            EntityManager.Instance.AddMapEntity(this.Map.ID, this.Map.InstanceId, this.Player.Session.Character);
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void Update()
        {
            
        }

        /// <summary>
        /// 结束
        /// </summary>
        internal void End()
        {
            
        }
    }
}
