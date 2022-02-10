using Common;
using Common.Utils;
using GameServer.Entities;
using GameServer.Managers;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    /// <summary>
    /// 聊天
    /// </summary>
    class Chat
    {
        /// <summary>
        /// 所有者
        /// </summary>
        Character Owner;
        /// <summary>
        /// 已同步的本地信息索引
        /// </summary>
        public int localIdx;
        /// <summary>
        /// 已同步的世界信息索引
        /// </summary>
        public int worldIdx;
        /// <summary>
        /// 已同步的系统信息索引
        /// </summary>
        public int systemIdx;
        /// <summary>
        /// 已同步的队伍信息索引
        /// </summary>
        public int teamIdx;
        /// <summary>
        /// 已同步的公会信息索引
        /// </summary>
        public int guildIdx;

        public Chat(Character owner)
        {
            this.Owner = owner;
        }
        /// <summary>
        /// 消息后处理
        /// </summary>
        /// <param name="message"></param>
        public void PostProcess(NetMessageResponse message)
        {
            if(message.Chat == null)
            {
                message.Chat = new ChatResponse();
                message.Chat.Result = Result.Success;
            }
            //获得本地，世界，系统最新信息
            this.localIdx = ChatManager.Instance.GetLocalMessages(this.Owner.Info.mapId, this.localIdx, message.Chat.localMessages);
            this.worldIdx = ChatManager.Instance.GetWorldMessages(this.worldIdx, message.Chat.worldMessages);
            this.systemIdx = ChatManager.Instance.GetSystemMessages(this.systemIdx, message.Chat.systemMssages);
            //获得队伍最新信息
            if(Owner.Team != null)
            {
                this.teamIdx = ChatManager.Instance.GetTeamMessages(this.Owner.Team.Id, this.teamIdx, message.Chat.teamMessages);
            }
            //获得公会最新信息
            if(Owner.Guild != null)
            {
                this.guildIdx = ChatManager.Instance.GetGuildMessages(this.Owner.Guild.Id, this.guildIdx, message.Chat.guildMessages);
            }
        }
    }
}
