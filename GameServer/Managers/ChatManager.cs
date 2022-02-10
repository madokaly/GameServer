using Common;
using Common.Utils;
using GameServer.Entities;
using Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    /// <summary>
    /// 聊天管理器
    /// </summary>
    class ChatManager : Singleton<ChatManager>
    {
        /// <summary>
        /// 系统消息列表
        /// </summary>
        public List<ChatMessage> System = new List<ChatMessage>();
        /// <summary>
        /// 世界消息列表
        /// </summary>
        public List<ChatMessage> World = new List<ChatMessage>();
        /// <summary>
        /// 地图对应的本地消息列表
        /// </summary>
        public Dictionary<int, List<ChatMessage>> Local = new Dictionary<int, List<ChatMessage>>();
        /// <summary>
        /// 队伍对应的队伍消息列表
        /// </summary>
        public Dictionary<int, List<ChatMessage>> Team = new Dictionary<int, List<ChatMessage>>();
        /// <summary>
        /// 公会对应的公会消息列表
        /// </summary>
        public Dictionary<int, List<ChatMessage>> Guild = new Dictionary<int, List<ChatMessage>>();

        public void Init()
        {

        }
        /// <summary>
        /// 添加消息
        /// </summary>
        /// <param name="from"></param>
        /// <param name="message"></param>
        public void AddMessage(Character from, ChatMessage message)
        {
            message.FromId = from.Id;
            message.FromName = from.Name;
            message.Time = TimeUtil.timestamp;
            //根据消息频道加入到不同的缓存中
            switch (message.Channel)
            {
                case ChatChannel.Local:
                    this.AddLocalMessage(from.Info.mapId, message);
                    break;
                case ChatChannel.World:
                    this.AddWorldMessage(message);
                    break;
                case ChatChannel.System:
                    this.AddSystemMessage(message);
                    break;
                case ChatChannel.Team:
                    this.AddTeamMessage(from.Team.Id, message);
                    break;
                case ChatChannel.Guild:
                    this.AddGuildMessage(from.Guild.Id, message);
                    break;
            }
        }

        /// <summary>
        /// 添加本地消息
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="message"></param>
        private void AddLocalMessage(int mapId, ChatMessage message)
        {
            if(!this.Local.TryGetValue(mapId, out List<ChatMessage> messages))
            {
                messages = new List<ChatMessage>();
                this.Local.Add(mapId, messages);
            }
            messages.Add(message);
        }
        /// <summary>
        /// 添加世界消息
        /// </summary>
        /// <param name="message"></param>
        private void AddWorldMessage(ChatMessage message)
        {
            this.World.Add(message);
        }
        /// <summary>
        /// 添加系统消息
        /// </summary>
        /// <param name="message"></param>
        private void AddSystemMessage(ChatMessage message)
        {
            this.System.Add(message);
        }
        /// <summary>
        /// 添加队伍消息
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="message"></param>
        private void AddTeamMessage(int teamId, ChatMessage message)
        {
            if (!this.Team.TryGetValue(teamId, out List<ChatMessage> messages))
            {
                messages = new List<ChatMessage>();
                this.Team.Add(teamId, messages);
            }
            messages.Add(message);
        }
        /// <summary>
        /// 添加公会消息
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="message"></param>
        private void AddGuildMessage(int guildId, ChatMessage message)
        {
            if (!this.Guild.TryGetValue(guildId, out List<ChatMessage> messages))
            {
                messages = new List<ChatMessage>();
                this.Guild.Add(guildId, messages);
            }
            messages.Add(message);
        }
        /// <summary>
        /// 获得本地消息
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="idx"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public int GetLocalMessages(int mapId, int idx, List<ChatMessage> result)
        {
            if(!this.Local.TryGetValue(mapId, out List<ChatMessage> messages))
            {
                return 0;
            }
            return GetNewMessages(idx, result, messages);
        }
        /// <summary>
        /// 获得世界消息
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public int GetWorldMessages(int idx, List<ChatMessage> result)
        {
            return GetNewMessages(idx, result, this.World);
        }
        /// <summary>
        /// 获得系统消息
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public int GetSystemMessages(int idx, List<ChatMessage> result)
        {
            return GetNewMessages(idx, result, this.System);
        }
        /// <summary>
        /// 获得队伍消息
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="idx"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public int GetTeamMessages(int teamId, int idx, List<ChatMessage> result)
        {
            if (!this.Team.TryGetValue(teamId, out List<ChatMessage> messages))
            {
                return 0;
            }
            return GetNewMessages(idx, result, messages);
        }
        /// <summary>
        /// 获得公会消息
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="idx"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public int GetGuildMessages(int guildId, int idx, List<ChatMessage> result)
        {
            if (!this.Guild.TryGetValue(guildId, out List<ChatMessage> messages))
            {
                return 0;
            }
            return GetNewMessages(idx, result, messages);
        }
        /// <summary>
        /// 获得最新消息
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="result"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        private int GetNewMessages(int idx, List<ChatMessage> result, List<ChatMessage> messages)
        {
            if(idx == 0)
            {
                if(messages.Count > GameDefine.MaxChatRecoredNums)
                {
                    idx = messages.Count - GameDefine.MaxChatRecoredNums;
                }
            }
            for(;idx < messages.Count; idx++)
            {
                result.Add(messages[idx]);
            }
            return idx;
        }
    }
}
