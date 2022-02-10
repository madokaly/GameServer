using Common;
using Common.Data;
using GameServer.Models;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    /// <summary>
    /// 生成器类
    /// </summary>
    class Spawner
    {
        /// <summary>
        /// 生成规则
        /// </summary>
        public SpawnRuleDefine Define { get; set; }
        /// <summary>
        /// 地图
        /// </summary>
        private Map Map;
        /// <summary>
        /// 刷新时间
        /// </summary>
        private float spawnTime = 0;
        /// <summary>
        /// 消失时间
        /// </summary>
        private float unspawnTime = 0;

        private bool spawned=false;
        /// <summary>
        /// 生成点
        /// </summary>
        private SpawnPointDefine spawnPoint = null;

        public Spawner(SpawnRuleDefine define,Map map)
        {
            this.Map = map;
            this.Define = define;

            if (DataManager.Instance.SpawnPoints.ContainsKey(this.Map.ID))
            {
                if (DataManager.Instance.SpawnPoints[this.Map.ID].ContainsKey(this.Define.SpawnPoint))
                {
                    spawnPoint = DataManager.Instance.SpawnPoints[this.Map.ID][this.Define.SpawnPoint];
                }
                else
                {
                    Log.ErrorFormat("SpawnRule[{0}] SpawnPoint[{1}] not existed",this.Define.ID,this.Define.SpawnPoint);
                }
            }
        }
        /// <summary>
        /// 更新1秒10次
        /// </summary>
        public void Update()
        {
            if (this.CanSpawn())
            {
                this.Spawn();
            }
        }

        bool CanSpawn()
        {
            if (this.spawned)
                return false;
            if (this.unspawnTime + this.Define.SpawnPeriod > Time.time)
                return false;
            return true;

        }
        /// <summary>
        /// 生成怪物
        /// </summary>
        public void Spawn()
        {
            this.spawned = true;
            Log.InfoFormat("Map[{0}] Spawn[{1}]:Mon{2},Lv{3},At Point{4}",this.Define.MapID,this.Define.ID,this.Define.SpawnMonID,this.Define.SpawnLevel,this.Define.SpawnPoints);
            this.Map.MonsterManager.Create(this.Define.SpawnMonID,this.Define.SpawnLevel,this.spawnPoint.Position,this.spawnPoint.Direction);
        }
    }
}
