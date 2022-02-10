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
    /// 战斗服务类
    /// </summary>
    class BattleService : Singleton<BattleService>
    {
        public BattleService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<SkillCastRequest>(this.OnSkillCast);
        }

        public void Init()
        {

        }
        /// <summary>
        /// 收到技能释放的请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void OnSkillCast(NetConnection<NetSession> sender, SkillCastRequest request)
        {
            Log.InfoFormat("SkillCastRequest::skill:{0} caster:{1} target:{2} pos:{3}", request.castInfo.skillId, request.castInfo.casterId, request.castInfo.targetId, request.castInfo.Position.ToString());
            //分发技能释放的请求
            BattleManager.Instance.ProcessBattleMessage(sender, request);
        }
    }
}
