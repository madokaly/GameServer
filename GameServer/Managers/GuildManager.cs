using Common;
using Common.Utils;
using GameServer.Entities;
using GameServer.Services;
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
    /// 公会管理器
    /// </summary>
    class GuildManager : Singleton<GuildManager>
    {
        /// <summary>
        /// [公会id，公会]字典
        /// </summary>
        public Dictionary<int, Guild> Guilds = new Dictionary<int, Guild>();
        /// <summary>
        /// 公会名字哈希表
        /// </summary>
        private HashSet<string> GuildNames = new HashSet<string>();

        public void Init()
        {
            this.Guilds.Clear();
            foreach(var guild in DBService.Instance.Entities.Guilds)
            {
                AddGuild(new Guild(guild));
            }
        }
        /// <summary>
        /// 增加公会
        /// </summary>
        /// <param name="guild"></param>
        private void AddGuild(Guild guild)
        {
            this.Guilds.Add(guild.Id, guild);
            this.GuildNames.Add(guild.Name);
            guild.timestamp = TimeUtil.timestamp;
        }
        /// <summary>
        /// 检查公会名字是否已存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool CheckNameExisted(string name)
        {
            return GuildNames.Contains(name);
        }
        /// <summary>
        /// 创建公会
        /// </summary>
        /// <param name="name"></param>
        /// <param name="notice"></param>
        /// <param name="leader"></param>
        /// <returns></returns>
        public bool CreateGuild(string name, string notice, Character leader)
        {
            DateTime now = DateTime.Now;
            TGuild dbGuild = DBService.Instance.Entities.Guilds.Create();
            dbGuild.Name = name;
            dbGuild.Notice = notice;
            dbGuild.LeaderID = leader.Id;
            dbGuild.LeaderName = leader.Name;
            dbGuild.CreateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            DBService.Instance.Entities.Guilds.Add(dbGuild);

            Guild guild = new Guild(dbGuild);
            guild.AddMember(leader.Id, leader.Name, leader.Data.Class, leader.Data.Level, GuildTitle.President);
            leader.Guild = guild;
            DBService.Instance.Save();
            leader.Data.GuildId = dbGuild.Id;
            DBService.Instance.Save();
            this.AddGuild(guild);
            return true;
        }
        /// <summary>
        /// 获得公会
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        internal Guild GetGuild(int guildId)
        {
            if(guildId == 0)
            {
                return null;
            }
            Guild guild = null;
            this.Guilds.TryGetValue(guildId, out guild);
            return guild;
        }
        /// <summary>
        /// 获得公会信息列表
        /// </summary>
        /// <returns></returns>
        internal List<NGuildInfo> GetGuildsInfo()
        {
            List<NGuildInfo> result = new List<NGuildInfo>();
            foreach(var kv in this.Guilds)
            {
                result.Add(kv.Value.GuildInfo(null));
            }
            return result;
        }
        /// <summary>
        /// 移除无人公会
        /// </summary>
        /// <param name="guildId"></param>
        /// <param name="guildName"></param>
        public void RemoveGuild(int guildId, string guildName)
        {
            if (this.Guilds.ContainsKey(guildId))
            {
                this.Guilds.Remove(guildId);
                this.GuildNames.Remove(guildName); 
            }
        }
    }
}
