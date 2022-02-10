using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using GameServer.Entities;
using GameServer.Models;
using Models;
using SkillBridge.Message;

namespace GameServer.Managers
{
    /// <summary>
    /// 怪物管理器
    /// </summary>
    public class MonsterManager
    {
        /// <summary>
        /// 所属地图
        /// </summary>
        private Map Map;
        /// <summary>
        /// [唯一ID，怪物]字典
        /// </summary>
        public Dictionary<int, Monster> Monsters = new Dictionary<int, Monster>();

        public void Init(Map map)
        {
            this.Map = map;
        }

        /// <summary>
        /// 生成怪物
        /// </summary>
        /// <param name="spawnMonID"></param>
        /// <param name="spawnLevel"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Monster Create(int spawnMonID, int spawnLevel, NVector3 position, NVector3 direction)
        {
            Monster monster = new Monster(spawnMonID, spawnLevel, position, direction);
            EntityManager.Instance.AddEntity(this.Map.ID, this.Map.InstanceId, monster);
            monster.Id = monster.entityId;
            monster.Info.EntityId = monster.entityId;
            monster.Info.mapId = this.Map.ID;
            Monsters[monster.Id] = monster;

            this.Map.MonsterEnter(monster);
            return monster;
        }
    }
}
