using Common;
using GameServer.Entities;
using GameServer.Managers;
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
    /// 聊天服务类
    /// </summary>
    class ChatService : Singleton<ChatService>
    {
        public ChatService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ChatRequest>(this.OnChat);
        }

        public void Init()
        {
            ChatManager.Instance.Init();
        }
        /// <summary>
        /// 收到聊天请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnChat(NetConnection<NetSession> sender, ChatRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnChat: :Character:{0} :Channel:{1} :Message:{2}", character.Id, request.Message.Channel, request.Message.Message);
            
            if(request.Message.Channel == ChatChannel.Private)
            {
                //私聊
                var charTo = SessionManager.Instance.GetSession(request.Message.ToId);
                if(charTo == null)
                {
                    //私聊对象离线
                    sender.Session.Response.Chat = new ChatResponse();
                    sender.Session.Response.Chat.Result = Result.Failed;
                    sender.Session.Response.Chat.Errormsg = "对方已经离线";
                    sender.Session.Response.Chat.privateMessages.Add(request.Message);
                    sender.SendResponse();
                }
                else
                {
                    //私聊对象在线
                    if(charTo.Session.Response.Chat == null)
                    {
                        charTo.Session.Response.Chat = new ChatResponse();
                    }
                    request.Message.FromId = character.Id;
                    request.Message.FromName = character.Name;
                    charTo.Session.Response.Chat.Result = Result.Success;
                    charTo.Session.Response.Chat.privateMessages.Add(request.Message);
                    charTo.SendResponse();

                    if(sender.Session.Response.Chat == null)
                    {
                        sender.Session.Response.Chat = new ChatResponse();
                    }
                    sender.Session.Response.Chat.Result = Result.Success;
                    sender.Session.Response.Chat.privateMessages.Add(request.Message);
                    sender.SendResponse();
                }
            }
            else
            {
                //非私聊
                sender.Session.Response.Chat = new ChatResponse();
                sender.Session.Response.Chat.Result = Result.Success;
                ChatManager.Instance.AddMessage(character, request.Message);
                sender.SendResponse();
            }
        }

    }
}
