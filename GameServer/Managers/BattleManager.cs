using Common;
using GameServer.Entities;
using Models;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    /// <summary>
    /// 战斗管理器
    /// </summary>
    class BattleManager : Singleton<BattleManager>
    {
        static long bid = 0;

        public void Init()
        {

        }
        /// <summary>
        /// 分发技能释放的请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        public void ProcessBattleMessage(NetConnection<NetSession> sender, SkillCastRequest request)
        {
            Log.InfoFormat("BattleManager:ProcessBattleMessage::skill:{0} caster:{1} target:{2} pos:{3}", 
                request.castInfo.skillId, request.castInfo.casterId, request.castInfo.targetId, request.castInfo.Position.String());
            Character character = sender.Session.Character;
            var battle = MapManager.Instance[character.Info.mapId].Battle;
            //分发技能释放到具体的战斗
            battle.ProcessBattleMessage(sender, request);
        }
    }
}
