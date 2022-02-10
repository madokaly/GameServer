using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using GameServer.Models;

namespace GameServer.Managers
{
    /// <summary>
    /// 地图管理器
    /// </summary>
    class MapManager : Singleton<MapManager>
    {
        /// <summary>
        /// [地图id,[副本id，副本地图]]字典
        /// </summary>
        Dictionary<int, Dictionary<int, Map>> Maps = new Dictionary<int, Dictionary<int, Map>>();

        public void Init()
        {
            foreach (var mapdefine in DataManager.Instance.Maps.Values)
            {
                int instanceCount = 1;
                if(mapdefine.Type == Common.Data.MapType.Arena)
                {
                    instanceCount = ArenaManager.MaxInstance;
                }
                Log.InfoFormat("MapManager.Init > Map:{0}:{1}:{2}", mapdefine.ID, mapdefine.Name, instanceCount);
                this.Maps[mapdefine.ID] = new Dictionary<int, Map>();
                for(int i = 0; i < instanceCount; i++)
                {
                    this.Maps[mapdefine.ID][i] = new Map(mapdefine, i);
                }
            }
        }

        /// <summary>
        /// 普通地图
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Map this[int key]
        {
            get
            {
                return this.Maps[key][0];
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            foreach(var maps in this.Maps.Values)
            {
                foreach(var instance in maps.Values)
                {
                    instance.Update();
                }
            }
        }

        /// <summary>
        /// 竞技场副本
        /// </summary>
        /// <param name="arenaMapId"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        internal Map GetInstance(int mapId, int instance)
        {
            return this.Maps[mapId][instance];
        }
    }
}
