using Common;
using GameServer.Entities;
using GameServer.Managers;
using Models;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Services
{
    /// <summary>
    /// 剧情副本服务类
    /// </summary>
    class StoryService : Singleton<StoryService>
    {

        public StoryService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<StoryStartRequest>(this.OnStoryStart);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<StoryEndRequest>(this.OnStoryEnd);
        }

        public void Dispose()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Unsubscribe<StoryStartRequest>(this.OnStoryStart);
            MessageDistributer<NetConnection<NetSession>>.Instance.Unsubscribe<StoryEndRequest>(this.OnStoryEnd);
        }

        public void Init()
        {
            StoryManager.Instance.Init();
        }

        /// <summary>
        /// 收到剧情副本开始的请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        private void OnStoryStart(NetConnection<NetSession> sender, StoryStartRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnStoryStart : storyId:{0}", request.storyId);
            Story story = StoryManager.Instance.NewStory(request.storyId, sender);
            sender.Session.Response.storyStart = new StoryStartResponse();
            sender.Session.Response.storyStart.storyId = story.StoryId;
            sender.Session.Response.storyStart.instanceId = story.InstanceId;
            sender.Session.Response.storyStart.Result = Result.Success;
            sender.Session.Response.storyStart.Errormsg = "";
            sender.SendResponse();
        }

        /// <summary>
        /// 收到剧情副本结束的请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnStoryEnd(NetConnection<NetSession> sender, StoryEndRequest message)
        {
            Log.InfoFormat("OnStoryEnd : storyId:{0}", message.storyId);
            Story story = StoryManager.Instance.GetStory(message.storyId, message.instanceId);
            story.End();
            sender.Session.Response.storyEnd = new StoryEndResponse();
            sender.Session.Response.storyEnd.storyId = story.StoryId;
            sender.Session.Response.storyEnd.instanceId = story.InstanceId;
            sender.Session.Response.storyEnd.Result = Result.Success;
            sender.SendResponse();
        }
    }
}
