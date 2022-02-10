using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkillBridge.Message;

using Common;
using Common.Data;

using Network;
using GameServer.Managers;
using GameServer.Entities;
using GameServer.Services;

namespace GameServer.Models
{
    /// <summary>
    /// 地图类
    /// </summary>
    public class Map
    {
        /// <summary>
        /// 地图角色
        /// </summary>
        internal class MapCharacter
        {
            /// <summary>
            /// 网络连接
            /// </summary>
            public NetConnection<NetSession> connection;
            /// <summary>
            /// 角色
            /// </summary>
            public Character character;

            public MapCharacter(NetConnection<NetSession> conn, Character cha)
            {
                this.connection = conn;
                this.character = cha;
            }
        }
        /// <summary>
        /// 地图id
        /// </summary>
        public int ID
        {
            get { return this.Define.ID; }
        }
        /// <summary>
        /// 副本id
        /// </summary>
        public int InstanceId { get; set; }
        /// <summary>
        /// 数据库地图信息
        /// </summary>
        internal MapDefine Define;
        /// <summary>
        /// [角色唯一ID,地图角色类]字典
        /// </summary>
        Dictionary<int, MapCharacter> MapCharacters = new Dictionary<int, MapCharacter>();
        /// <summary>
        /// 刷怪管理器
        /// </summary>
        SpawnManager SpawnManager = new SpawnManager();
        /// <summary>
        /// 怪物管理器
        /// </summary>
        public MonsterManager MonsterManager = new MonsterManager();
        /// <summary>
        /// 战斗类
        /// </summary>
        public Battle.Battle Battle;

        internal Map(MapDefine define, int instanceId)
        {
            this.Define = define;
            this.InstanceId = instanceId;
            this.SpawnManager.Init(this);
            this.MonsterManager.Init(this);
            this.Battle = new Battle.Battle(this);
        }

        internal void Update()
        {
            SpawnManager.Update();
            this.Battle.Update();
        }

        /// <summary>
        /// 角色进入地图
        /// </summary>
        /// <param name="character"></param>
        internal void CharacterEnter(NetConnection<NetSession> conn, Character character)
        {
            Log.InfoFormat("CharacterEnter: Map:{0} characterId:{1}", this.Define.ID, character.Id);
            this.AddCharacter(conn, character);

            conn.Session.Response.mapCharacterEnter = new MapCharacterEnterResponse();
            conn.Session.Response.mapCharacterEnter.mapId = this.Define.ID;
            foreach (var kv in this.MapCharacters)
            {
                //地图已存在的角色
                conn.Session.Response.mapCharacterEnter.Characters.Add(kv.Value.character.Info);
                if (kv.Value.character != character)
                    //群发角色进入地图
                    this.AddCharacterEnterMap(kv.Value.connection, character.Info);
            }
            foreach (var kv in this.MonsterManager.Monsters)
            {
                conn.Session.Response.mapCharacterEnter.Characters.Add(kv.Value.Info);
            }
            conn.SendResponse();
        }

        /// <summary>
        /// 地图字典增加角色
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="character"></param>
        internal void AddCharacter(NetConnection<NetSession> conn, Character character)
        {
            Log.InfoFormat("AddCharacter: Map:{0} characterId:{1}", this.Define.ID, character.Id);
            character.Info.mapId = this.ID;
            character.OnEnterMap(this);
            if(!this.MapCharacters.ContainsKey(character.Id))
                this.MapCharacters[character.Id] = new MapCharacter(conn, character);
        }

        /// <summary>
        /// 角色离开地图
        /// </summary>
        /// <param name="cha"></param>
        internal void CharacterLeave(Character cha)
        {
            cha.OnLeaveMap(this);
            Log.InfoFormat("CharacterLeave: Map:{0} characterId:{1}", this.Define.ID, cha.Id);
            foreach (var kv in this.MapCharacters)
            {
                this.SendCharacterLeaveMap(kv.Value.connection, cha);
            }
            this.MapCharacters.Remove(cha.Id);
        }
        /// <summary>
        /// 对地图内所有玩家发送角色进入地图
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="character"></param>
        void AddCharacterEnterMap(NetConnection<NetSession> conn, NCharacterInfo character)
        {
            if (conn.Session.Response.mapCharacterEnter == null)
            {
                conn.Session.Response.mapCharacterEnter = new MapCharacterEnterResponse();
                conn.Session.Response.mapCharacterEnter.mapId = this.Define.ID;
            }
            conn.Session.Response.mapCharacterEnter.Characters.Add(character);
            conn.SendResponse();
        }
        /// <summary>
        /// 对地图内所有玩家发送角色离开地图
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="character"></param>
        void SendCharacterLeaveMap(NetConnection<NetSession> conn, Character character)
        {
            Log.InfoFormat("SendCharacterLeaveMap To {0}:{1} : Map:{2} Character:{3}:{4}", conn.Session.Character.Id, conn.Session.Character.Info.Name, this.Define.ID, character.Id, character.Info.Name);
            conn.Session.Response.mapCharacterLeave = new MapCharacterLeaveResponse();
            conn.Session.Response.mapCharacterLeave.entityId = character.entityId;
            conn.SendResponse();
        }
        /// <summary>
        /// 更新实体信息
        /// </summary>
        /// <param name="entity"></param>
        internal void UpdateEntity(NEntitySync entity)
        {
            foreach (var kv in this.MapCharacters)
            {
                if (kv.Value.character.entityId == entity.Id)
                {
                    kv.Value.character.Position = entity.Entity.Position;
                    kv.Value.character.Direction = entity.Entity.Direction;
                    kv.Value.character.Speed = entity.Entity.Speed;
                    if (entity.Event == EntityEvent.Ride)
                    {
                        kv.Value.character.Ride = entity.Param;
                    }
                }
                else
                {
                    MapService.Instance.SendEntityUpdate(kv.Value.connection, entity);
                }
            }
        }
        /// <summary>
        /// 怪物进入地图
        /// </summary>
        /// <param name="character"></param>
        internal void MonsterEnter(Monster monster)
        {
            Log.InfoFormat("MonsterEnter: Map:{0} monsterId:{1}", this.Define.ID, monster.Id);
            monster.OnEnterMap(this);
            foreach (var kv in this.MapCharacters)
            {
                this.AddCharacterEnterMap(kv.Value.connection, monster.Info);
            }
        }
        /// <summary>
        /// 广播技能释放或技能击中
        /// </summary>
        /// <param name="response"></param>
        internal void BroadcastBattleResponse(NetMessageResponse response)
        {
            foreach(var kv in this.MapCharacters)
            {
                if(response.skillCast != null)
                    kv.Value.connection.Session.Response.skillCast = response.skillCast;
                if(response.skillHits != null)
                    kv.Value.connection.Session.Response.skillHits = response.skillHits;
                if (response.buffRes != null)
                    kv.Value.connection.Session.Response.buffRes = response.buffRes;
                kv.Value.connection.SendResponse();
            }
        }
    }
}
