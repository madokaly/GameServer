using Common.Utils;
using GameServer;
using GameServer.Entities;
using GameServer.Managers;
using GameServer.Services;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    class Guild
    {
        public int Id { get { return this.Data.Id; } }

        public string Name { get { return this.Data.Name; } }

        /// <summary>
        /// 变化的时间
        /// </summary>
        public double timestamp;

        public TGuild Data;

        public Guild(TGuild guild)
        {
            this.Data = guild;
        }
        /// <summary>
        /// 加入公会申请
        /// </summary>
        /// <param name="apply"></param>
        /// <returns></returns>
        internal bool JoinApply(NGuildApplyInfo apply)
        {
            //是否已经存在
            var oldApply = this.Data.Applies.FirstOrDefault(v => v.CharacterId == apply.characterId);
            if(oldApply != null)
            {
                return false;
            }
            //新增
            DateTime now = DateTime.Now;
            var dbApply = DBService.Instance.Entities.GuildApplies.Create();
            dbApply.GuildId = apply.GuildId;
            dbApply.CharacterId = apply.characterId;
            dbApply.Name = apply.Name;
            dbApply.Class = apply.Class;
            dbApply.Level = apply.Level;
            dbApply.ApplyTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            DBService.Instance.Entities.GuildApplies.Add(dbApply);
            Data.Applies.Add(dbApply);
            DBService.Instance.Save();
            timestamp = TimeUtil.timestamp;
            return true;
        }
        /// <summary>
        /// 审批加入公会请求
        /// </summary>
        /// <param name="apply"></param>
        /// <returns></returns>
        internal bool JoinAppove(NGuildApplyInfo apply)
        {
            var oldApply = this.Data.Applies.FirstOrDefault(v => v.CharacterId == apply.characterId && v.Result == 0);
            if(oldApply == null)
            {
                return false;
            }
            oldApply.Result = (int)apply.Result;
            if(apply.Result == ApplyResult.Accept)
            {
                AddMember(apply.characterId, apply.Name, apply.Class, apply.Level, GuildTitle.None);
            }
            DBService.Instance.Save();
            timestamp = TimeUtil.timestamp;
            return true;
        }
        /// <summary>
        /// 添加公会成员
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="name"></param>
        /// <param name="class"></param>
        /// <param name="level"></param>
        /// <param name="title"></param>
        public void AddMember(int characterId, string name, int @class, int level, GuildTitle title)
        {
            DateTime now = DateTime.Now;
            TGuildMember dbMember = new TGuildMember()
            {
                CharacterId = characterId,
                Name = name,
                Class = @class,
                Level = level,
                Title = (int)title,
                JoinTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second),
                LastTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second)
            };
            Data.Members.Add(dbMember);
            var character = CharacterManager.Instance.GetCharacter(characterId);
            if(character != null)
            {
                character.Data.GuildId = this.Id;
            }
            else
            {
                TCharacter dbChar = DBService.Instance.Entities.Characters.SingleOrDefault(c => c.ID == character.Id);
                dbChar.GuildId = this.Id;
            }
            timestamp = TimeUtil.timestamp;
        }
        /// <summary>
        /// 离开公会
        /// </summary>
        /// <param name="member"></param>
        public void Leave(Character member)
        {
            TGuildMember guildMember = GetDBMember(member.Id);
                //DBService.Instance.Entities.GuildMembers.SingleOrDefault(c => c.CharacterId == member.Id);
            if (this.Data.Members.Contains(guildMember))
            {
                this.Data.Members.Remove(guildMember);
                if(member.Id == this.Data.LeaderID)
                {
                    if(this.Data.Members.Count > 0)
                    {
                        this.Data.LeaderID = Data.Members.First().Id;
                        this.Data.LeaderName = Data.Members.First().Name;
                    }
                    else
                    {
                        GuildManager.Instance.RemoveGuild(this.Id, this.Name);
                        DBService.Instance.Entities.Guilds.Remove(this.Data);
                    }
                }
                member.Guild = null;
                member.Data.GuildId = 0;
                DBService.Instance.Save();
                timestamp = TimeUtil.timestamp;
            }
        }
        /// <summary>
        /// 公会变化后处理
        /// </summary>
        /// <param name="from"></param>
        /// <param name="message"></param>
        public void PostProcess(Character from, NetMessageResponse message)
        {
            if(message.Guild == null)
            {
                message.Guild = new GuildResponse();
                message.Guild.Result = Result.Success;
                message.Guild.guildInfo = GuildInfo(from);
            }
        }
        /// <summary>
        /// 向from发送后处理信息
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        internal NGuildInfo GuildInfo(Character from)
        {
            NGuildInfo info = new NGuildInfo()
            {
                Id = this.Id,
                GuildName = this.Name,
                Notice = this.Data.Notice,
                leaderId = this.Data.LeaderID,
                leaderName = this.Data.LeaderName,
                createTime = (long)TimeUtil.GetTimestamp(this.Data.CreateTime),
                memberCount = this.Data.Members.Count
            };
            if(from != null)
            {
                info.Members.AddRange(GetMemberInfos());
                if(from.Id == this.Data.LeaderID)
                {
                    info.Applies.AddRange(GetApplyInfos());
                }
            }
            return info;
        }
        /// <summary>
        /// 获得成员信息列表
        /// </summary>
        /// <returns></returns>
        private List<NGuildMemberInfo> GetMemberInfos()
        {
            List<NGuildMemberInfo> members = new List<NGuildMemberInfo>();
            foreach(var member in this.Data.Members)
            {
                var memberInfo = new NGuildMemberInfo()
                {
                    Id = member.Id,
                    characterId = member.CharacterId,
                    Title = (GuildTitle)member.Title,
                    joinTime = (long)TimeUtil.GetTimestamp(member.JoinTime),
                    lastTime = (long)TimeUtil.GetTimestamp(member.LastTime)
                };
                //效验
                var character = CharacterManager.Instance.GetCharacter(member.CharacterId);
                if(character != null)
                {
                    DateTime now = DateTime.Now;
                    memberInfo.Info = character.GetBasicInfo();
                    memberInfo.Status = 1;
                    member.Level = character.Data.Level;
                    member.Name = character.Data.Name;
                    member.LastTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                }
                else
                {
                    memberInfo.Info = this.GetMemberInfo(member);
                    memberInfo.Status = 0;
                }
                members.Add(memberInfo);
            }
            return members;
        }
        /// <summary>
        /// 获得成员信息
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        private NCharacterInfo GetMemberInfo(TGuildMember member)
        {
            return new NCharacterInfo()
            {
                Id = member.Id,
                Name = member.Name,
                Class = (CharacterClass)member.Class,
                Level = member.Level
            };
        }
        /// <summary>
        /// 获得申请信息列表
        /// </summary>
        /// <returns></returns>
        private List<NGuildApplyInfo> GetApplyInfos()
        {
            List<NGuildApplyInfo> applies = new List<NGuildApplyInfo>();
            foreach(var apply in this.Data.Applies)
            {
                //删选掉已审批的
                if (apply.Result != (int)ApplyResult.None) continue;
                applies.Add(new NGuildApplyInfo()
                {
                    characterId = apply.CharacterId,
                    GuildId = apply.GuildId,
                    Class = apply.Class,
                    Level = apply.Level,
                    Name = apply.Name,
                    Result = (ApplyResult)apply.Result,
                });
            }
            return applies;
        }
        /// <summary>
        /// 执行管理操作
        /// </summary>
        /// <param name="command"></param>
        /// <param name="targetId"></param>
        /// <param name="sourceId"></param>
        internal void ExecuteAdmin(GuildAdminCommand command, int targetId, int sourceId)
        {
            var target = GetDBMember(targetId);
            var source = GetDBMember(sourceId);
            switch (command)
            {
                case GuildAdminCommand.Promote:
                    target.Title = (int)GuildTitle.VicePresident;
                    break;
                case GuildAdminCommand.Depost:
                    target.Title = (int)GuildTitle.None;
                    break;
                case GuildAdminCommand.Transfer:
                    target.Title = (int)GuildTitle.President;
                    source.Title = (int)GuildTitle.None;
                    this.Data.LeaderID = targetId;
                    this.Data.LeaderName = target.Name;
                    break;
                case GuildAdminCommand.Kickout:
                    var character = CharacterManager.Instance.GetCharacter(targetId);
                    if (character != null)
                    {
                        character.Data.GuildId = 0;
                        character.Guild = null;
                    }
                    else
                    {
                        TCharacter dbChar = DBService.Instance.Entities.Characters.SingleOrDefault(c => c.ID == character.Id);
                        dbChar.GuildId = 0;
                    }
                    break;
            }
            DBService.Instance.Save();
            timestamp = TimeUtil.timestamp;
        }
        /// <summary>
        /// 获得数据公会成员
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        public TGuildMember GetDBMember(int characterId)
        {
            foreach(var member in this.Data.Members)
            {
                if(member.CharacterId == characterId)
                {
                    return member;
                }
            }
            return null;
        }
    }
}
