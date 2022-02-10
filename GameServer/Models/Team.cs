using Common;
using Common.Utils;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    class Team
    {
        /// <summary>
        /// 队伍序号
        /// </summary>
        public int Id;
        /// <summary>
        /// 队长
        /// </summary>
        public Character Leader;
        /// <summary>
        /// 队伍成员列表
        /// </summary>
        public List<Character> Members = new List<Character>();
        /// <summary>
        /// 队伍变化时间
        /// </summary>
        public double timestamp;

        public Team(Character leader)
        {
            this.AddMember(leader);
        }
        /// <summary>
        /// 进入队伍
        /// </summary>
        /// <param name="member"></param>
        public void AddMember(Character member)
        {
            if (this.Members.Count == 0)
            {
                this.Leader = member;
            }
            this.Members.Add(member);
            member.Team = this;
            timestamp = TimeUtil.timestamp;

        }
        /// <summary>
        /// 离开队伍
        /// </summary>
        /// <param name="member"></param>
        public void Leave(Character member)
        {
            Log.InfoFormat("Leave Team: {0} : {1}", member.Id, member.Info.Name);
            this.Members.Remove(member);
            if (member == this.Leader)
            {
                if (this.Members.Count > 0)
                {
                    this.Leader = this.Members[0];
                }
                else
                {
                    this.Leader = null;
                }
            }
            member.Team = null;
            timestamp = TimeUtil.timestamp;
        }
        /// <summary>
        /// 队伍成员变化的后处理
        /// </summary>
        /// <param name="message"></param>
        public void PostProcess(NetMessageResponse message)
        {
            if (message.teamInfo == null)
            {
                message.teamInfo = new TeamInfoResponse();
                message.teamInfo.Result = Result.Success;
                message.teamInfo.Team = new NTeamInfo();
                message.teamInfo.Team.Id = this.Id;
                message.teamInfo.Team.Leader = this.Leader.Id;
                foreach (var member in this.Members)
                {
                    message.teamInfo.Team.Members.Add(member.GetBasicInfo());
                }
            }
        }
    }
}
