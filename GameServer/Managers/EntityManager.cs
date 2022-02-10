using Common;
using GameServer.Core;
using GameServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    /// <summary>
    /// 实体管理器
    /// </summary>
    class EntityManager : Singleton<EntityManager>
    {
        private int idx = 0;
        /// <summary>
        /// 实体类列表
        /// </summary>
        public Dictionary<int, Entity> AllEntities = new Dictionary<int, Entity>();
        /// <summary>
        /// 地图实体类字典
        /// </summary>
        public Dictionary<int, List<Entity>> MapEntities = new Dictionary<int, List<Entity>>();

        public int GetMapIndex(int mapId, int instanceId)
        {
            return mapId * 1000 + instanceId;
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="entity"></param>
        public void AddEntity(int mapId, int instanceId, Entity entity)
        {
            //加入管理器生成唯一ID
            entity.EntityData.Id = ++this.idx;
            AllEntities.Add(entity.EntityData.Id, entity);
            this.AddMapEntity(mapId, instanceId, entity);
        }

        /// <summary>
        /// 向地图添加实体
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="entity"></param>
        public void AddMapEntity(int mapId, int instanceId, Entity entity)
        {
            List<Entity> entities = null;
            int index = this.GetMapIndex(mapId, instanceId);
            if (!MapEntities.TryGetValue(index, out entities))
            {
                entities = new List<Entity>();
                MapEntities[index] = entities;
            }
            entities.Add(entity);
        }

        /// <summary>
        /// 移除实体
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="entity"></param>
        public void RemoveEntity(int mapId, int instanceId, Entity entity)
        {
            this.AllEntities.Remove(entity.entityId);
            this.RemoveMapEntity(mapId, instanceId, entity);
        }

        /// <summary>
        /// 移除地图保存实体
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="instanceId"></param>
        /// <param name="character"></param>
        internal void RemoveMapEntity(int mapId, int instanceId, Entity entity)
        {
            int index = this.GetMapIndex(mapId, instanceId);
            this.MapEntities[index].Remove(entity);
        }

        /// <summary>
        /// 查找实体类
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public Entity GetEntity(int entityId)
        {
            Entity result = null;
            this.AllEntities.TryGetValue(entityId, out result);
            return result;
        }

        /// <summary>
        /// 查找生物类
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public Creature GetCreature(int entityId)
        {
            return GetEntity(entityId) as Creature;
        }

        /// <summary>
        /// 得到指定的实体类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public List<T> GetMapEntities<T>(int index, Predicate<Entity> match) where T : Creature
        {
            List<T> result = new List<T>();
            foreach(var entity in this.MapEntities[index])
            {
                if(entity is T && match.Invoke(entity))
                {
                    result.Add((T)entity);
                }
            }
            return result;
        }
        /// <summary>
        /// 得到范围中的生物类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public List<T> GetMapEntitiesInRange<T>(int index, Vector3Int pos, int range) where T : Creature
        {
            return this.GetMapEntities<T>(index, (entity) =>
            {
                T creature = entity as T;
                return creature.Distance(pos) < range;
            });
        }
    }
}
