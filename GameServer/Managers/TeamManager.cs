using Common;
using GameServer.Entities;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    /// <summary>
    /// 队伍管理器
    /// </summary>
    class TeamManager : Singleton<TeamManager>
    {
        /// <summary>
        /// 队伍列表
        /// </summary>
        public List<Team> Teams = new List<Team>();
        /// <summary>
        /// 队伍字典
        /// </summary>
        public Dictionary<int, Team> CharacterTeams = new Dictionary<int, Team>();

        public void Init()
        {

        }
        /// <summary>
        /// 获得对应队伍
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public Team GetTeamByCharacter(int characterId)
        {
            Team team = null;
            this.CharacterTeams.TryGetValue(characterId, out team);
            return team;
        }
        /// <summary>
        /// 添加队伍成员
        /// </summary>
        /// <param name="leader"></param>
        /// <param name="member"></param>
        public void AddTeamMember(Character leader, Character member)
        {
            if (leader.Team == null)
            {
                leader.Team = CreateTeam(leader);
            }
            leader.Team.AddMember(member);

        }
        /// <summary>
        /// 创建队伍，查看队伍列表中是否有空队伍
        /// </summary>
        /// <param name="leader"></param>
        /// <returns></returns>
        Team CreateTeam(Character leader)
        {
            Team team = null;
            for (int i = 0; i < Teams.Count; i++)
            {
                team = this.Teams[i];
                if (team.Members.Count == 0)
                {
                    team.AddMember(leader);
                    return team;
                }
            }
            team = new Team(leader);
            this.Teams.Add(team);
            team.Id = this.Teams.Count;
            return team;
        }
    }
}
