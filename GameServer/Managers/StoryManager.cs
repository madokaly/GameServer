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
    /// 剧情副本管理器
    /// </summary>
    class StoryManager : Singleton<StoryManager>
    {
        /// <summary>
        /// 最大实例数
        /// </summary>
        public const int MaxInstance = 100;

        public class StoryMap
        {
            public Queue<int> InstanceIndexes = new Queue<int>();

            public Story[] Storys = new Story[MaxInstance];
        }

        Dictionary<int, StoryMap> Storys = new Dictionary<int, StoryMap>();

        public void Init()
        {
            foreach(var story in DataManager.Instance.Storys)
            {
                StoryMap map = new StoryMap();
                for(int i = 0; i < MaxInstance; i++)
                {
                    map.InstanceIndexes.Enqueue(i);
                }
                this.Storys[story.Key] = map;
            }
        }
        
        public Story NewStory(int storyId, NetConnection<NetSession> owner)
        {
            int storyMap = DataManager.Instance.Storys[storyId].MapId;
            int instance = this.Storys[storyId].InstanceIndexes.Dequeue();
            Map map = MapManager.Instance.GetInstance(storyMap, instance);
            Story story = new Story(map, storyId, instance, owner);
            this.Storys[storyId].Storys[instance] = story;
            story.PlayerEnter();
            return story;
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void Update()
        {
            
        }

        /// <summary>
        /// 获得指定竞技场
        /// </summary>
        /// <param name="arenaId"></param>
        /// <returns></returns>
        public Story GetStory(int storyId, int instanceId)
        {
            return this.Storys[storyId].Storys[instanceId];
        }
    }
}
