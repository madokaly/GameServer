using Common;
using GameServer.Models;
using Models;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;

namespace GameServer.Managers
{
    /// <summary>
    /// 挑战管理器
    /// </summary>
    class ArenaManager : Singleton<ArenaManager>
    {
        /// <summary>
        /// 地图ID
        /// </summary>
        public const int ArenaMapId = 5;
        /// <summary>
        /// 最大实例数
        /// </summary>
        public const int MaxInstance = 100;
        /// <summary>
        /// 实例索引
        /// </summary>
        Queue<int> InstanceIndexes = new Queue<int>();
        /// <summary>
        /// 实例数组
        /// </summary>
        Arena[] Arenas = new Arena[MaxInstance];

        public void Init()
        {
            for(int i = 0; i < MaxInstance; i++)
            {
                InstanceIndexes.Enqueue(i);
            }
        }
        
        /// <summary>
        /// 构建竞技场
        /// </summary>
        /// <param name="info"></param>
        /// <param name="red"></param>
        /// <param name="blue"></param>
        /// <returns></returns>
        public Arena NewArena(ArenaInfo info, NetConnection<NetSession> red, NetConnection<NetSession> blue)
        {
            int instance = InstanceIndexes.Dequeue();
            Map map = MapManager.Instance.GetInstance(ArenaMapId, instance);
            Arena arena = new Arena(map, info, red, blue);
            this.Arenas[instance] = arena;
            arena.PlayerEnter();
            return arena;
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void Update()
        {
            for(int i = 0; i < Arenas.Length; i++)
            {
                if(Arenas[i] != null)
                {
                    Arenas[i].Update();
                }
            }
        }

        /// <summary>
        /// 获得指定竞技场
        /// </summary>
        /// <param name="arenaId"></param>
        /// <returns></returns>
        public Arena GetArena(int arenaId)
        {
            if(arenaId >= 0 && arenaId < MaxInstance)
            {
                return Arenas[arenaId];
            }
            return null;
        }
    }
}
