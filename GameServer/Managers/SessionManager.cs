using Common;
using Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    /// <summary>
    /// 会话管理器
    /// </summary>
    class SessionManager:Singleton<SessionManager>
    {
        /// <summary>
        /// [角色id，角色session]字典
        /// </summary>
        public Dictionary<int,NetConnection<NetSession>> Sessions = new Dictionary<int, NetConnection<NetSession>>();

        /// <summary>
        /// 添加角色session
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="session"></param>
        public void AddSession(int characterId,NetConnection<NetSession> session)
        {
            this.Sessions[characterId] = session;
        }

        /// <summary>
        /// 移除指定角色的session
        /// </summary>
        /// <param name="characterId"></param>
        public void RemoveSession(int characterId)
        {
            this.Sessions.Remove(characterId);
        }

        /// <summary>
        /// 获得指定角色的session
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public NetConnection<NetSession> GetSession(int characterId)
        {
            NetConnection<NetSession> session = null;
            this.Sessions.TryGetValue(characterId,out session);
            return session;
        }

    }
}
