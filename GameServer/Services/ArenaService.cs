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
    /// 挑战服务类
    /// </summary>
    class ArenaService : Singleton<ArenaService>
    {

        public ArenaService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ArenaChallengeRequest>(this.OnArenaChallengeRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ArenaChallengeResponse>(this.OnArenaChallengeResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ArenaReadyRequest>(this.OnArenaReady);
        }

        public void Dispose()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Unsubscribe<ArenaChallengeRequest>(this.OnArenaChallengeRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Unsubscribe<ArenaChallengeResponse>(this.OnArenaChallengeResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Unsubscribe<ArenaReadyRequest>(this.OnArenaReady);
        }

        public void Init()
        {
            ArenaManager.Instance.Init();
        }

        /// <summary>
        /// 收到发起挑战请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnArenaChallengeRequest(NetConnection<NetSession> sender, ArenaChallengeRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnArenaChallengeRequest:  RedId:{0} RedName:{1} BlueId:{2} BlueName:{3}", request.ArenaInfo.Red.EntityId, request.ArenaInfo.Red.Name, request.ArenaInfo.Blue.EntityId,
                request.ArenaInfo.Blue.Name);
            NetConnection<NetSession> blue = null;
            if(request.ArenaInfo.Blue.EntityId > 0)
            {
                blue = SessionManager.Instance.GetSession(request.ArenaInfo.Blue.EntityId);
            }
            if(blue == null)
            {
                //被挑战者离线
                sender.Session.Response.arenaChallengeRes = new ArenaChallengeResponse();
                sender.Session.Response.arenaChallengeRes.Result = Result.Failed;
                sender.Session.Response.arenaChallengeRes.Errormsg = "好友不存在或不在线";
                sender.SendResponse();
                return;
            }
            //好友在线，将请求转发
            Log.InfoFormat("ForwardArenaChallengeRequest:  RedId:{0} RedName:{1} BlueId:{2} BlueName:{3}", request.ArenaInfo.Red.EntityId, request.ArenaInfo.Red.Name, request.ArenaInfo.Blue.EntityId,
                request.ArenaInfo.Blue.Name);
            blue.Session.Response.arenaChallengeReq = request;
            blue.SendResponse();
        }

        /// <summary>
        /// 收到被挑战者的回应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnArenaChallengeResponse(NetConnection<NetSession> sender, ArenaChallengeResponse response)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnArenaChallengeResponse:  character:{0} Result:{1} FromId:{2} ToId:{3}", character.Id, response.Result, response.ArenaInfo.Red.EntityId,
                response.ArenaInfo.Blue.EntityId);
            var requester = SessionManager.Instance.GetSession(response.ArenaInfo.Red.EntityId);
            if(requester == null)
            {
                //挑战者已离线
                sender.Session.Response.arenaChallengeRes = new ArenaChallengeResponse();
                sender.Session.Response.arenaChallengeRes.Result = Result.Failed;
                sender.Session.Response.arenaChallengeRes.Errormsg = "挑战者已离线";
                sender.SendResponse();
                return;
            }
            if(response.Result == Result.Failed)
            {
                requester.Session.Response.arenaChallengeRes = response;
                requester.Session.Response.arenaChallengeRes.Result = Result.Failed;
                requester.SendResponse();
                return;
            }
            Arena arena = ArenaManager.Instance.NewArena(response.ArenaInfo, requester, sender);
            this.SendArenaBegin(arena);
        }

        /// <summary>
        /// 发送挑战开始的回应
        /// </summary>
        /// <param name="arena"></param>
        private void SendArenaBegin(Arena arena)
        {
            var arenaBegin = new ArenaBeginResponse();
            arenaBegin.Result = Result.Failed;
            arenaBegin.Errormsg = "对方不在线";
            arenaBegin.ArenaInfo = arena.ArenaInfo;
            arena.Red.Session.Response.arenaBegin = arenaBegin;
            arena.Red.SendResponse();
            arena.Blue.Session.Response.arenaBegin = arenaBegin;
            arena.Blue.SendResponse();
        }

        /// <summary>
        /// 收到准备完成的请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnArenaReady(NetConnection<NetSession> sender, ArenaReadyRequest request)
        {
            Arena arena = ArenaManager.Instance.GetArena(request.arenaId);
            arena.EntityReady(request.entityId);
        }

        /// <summary>
        /// 发送回合准备完成的响应
        /// </summary>
        /// <param name="arena"></param>
        internal void SendArenaReady(Arena arena)
        {
            ArenaReadyResponse arenaReady = new ArenaReadyResponse();
            arenaReady.Round = arena.Round;
            arenaReady.ArenaInfo = arena.ArenaInfo;
            //发给双方
            arena.Red.Session.Response.arenaReady = arenaReady;
            arena.Red.SendResponse();
            arena.Blue.Session.Response.arenaReady = arenaReady;
            arena.Blue.SendResponse();
        }

        /// <summary>
        /// 发送回合开始的响应
        /// </summary>
        /// <param name="arena"></param>
        internal void SendArenaRoundStart(Arena arena)
        {
            ArenaRoundStartResponse roundStart = new ArenaRoundStartResponse();
            roundStart.Round = arena.Round;
            roundStart.ArenaInfo = arena.ArenaInfo;
            //发给双方
            arena.Red.Session.Response.arenaRoundStart = roundStart;
            arena.Red.SendResponse();
            arena.Blue.Session.Response.arenaRoundStart = roundStart;
            arena.Blue.SendResponse();
        }

        /// <summary>
        /// 发送回合结束的响应
        /// </summary>
        /// <param name="arena"></param>
        internal void SendArenaRoundEnd(Arena arena)
        {
            ArenaRoundEndResponse roundEnd = new ArenaRoundEndResponse();
            roundEnd.Round = arena.Round;
            roundEnd.ArenaInfo = arena.ArenaInfo;
            //发给双方
            arena.Red.Session.Response.arenaRoundEnd = roundEnd;
            arena.Red.SendResponse();
            arena.Blue.Session.Response.arenaRoundEnd = roundEnd;
            arena.Blue.SendResponse();
        }
    }
}
