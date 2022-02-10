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
    /// 刷怪管理器
    /// </summary>
    class SpawnManager
    {
        /// <summary>
        /// 生成器集合
        /// </summary>
        private List<Spawner> Rules = new List<Spawner>();
        /// <summary>
        /// 所属地图
        /// </summary>
        private Map Map;

        public void Init(Map map)
        {
            this.Map = map;
            if (DataManager.Instance.SpawnRules.ContainsKey(map.Define.ID))
            {
                foreach (var define in DataManager.Instance.SpawnRules[map.Define.ID].Values)
                {
                    this.Rules.Add(new Spawner(define,this.Map));
                }
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            if (Rules.Count==0)
                return;
            for (int i = 0; i < Rules.Count; i++)
            {
                this.Rules[i].Update();
            }
        }
    }
}
