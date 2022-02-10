using GameServer.Entities;
using GameServer.Managers;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    public class SkillManager
    {
        /// <summary>
        /// 所有者
        /// </summary>
        public Creature Owner { get; private set; }
        /// <summary>
        /// 技能列表
        /// </summary>
        public List<Skill> Skills { get; private set; }
        /// <summary>
        /// 协议技能信息列表
        /// </summary>
        public List<NSkillInfo> Infos { get; private set; }

        public Skill NormalSkill { get; private set; }

        public SkillManager(Creature owner)
        {
            this.Owner = owner;
            this.Skills = new List<Skill>();
            this.Infos = new List<NSkillInfo>();
            this.InitSkills();
        }
        /// <summary>
        /// 初始化技能列表
        /// </summary>
        private void InitSkills()
        {
            this.Skills.Clear();
            this.Infos.Clear();
            //----
            if (!DataManager.Instance.Skills.ContainsKey(this.Owner.Define.TID))
            {
                return;
            }
            foreach (var define in DataManager.Instance.Skills[this.Owner.Define.TID])
            {
                NSkillInfo info = new NSkillInfo();
                info.Id = define.Key;
                if (this.Owner.Info.Level >= define.Value.UnlockLevel)
                {
                    info.Level = 5;
                }
                else
                {
                    info.Level = 1;
                }
                this.Infos.Add(info);
                Skill skill = new Skill(info, this.Owner);
                if(define.Value.Type == Common.Battle.SkillType.Normal)
                {
                    NormalSkill = skill;
                }
                this.AddSkill(skill);
            }
        }
        /// <summary>
        /// 添加技能
        /// </summary>
        /// <param name="skill"></param>
        private void AddSkill(Skill skill)
        {
            this.Skills.Add(skill);
        }
        /// <summary>
        /// 获得指定技能
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        internal Skill GetSkill(int skillId)
        {
            for(int i = 0; i < Skills.Count; i++)
            {
                if(this.Skills[i].Define.ID == skillId)
                {
                    return this.Skills[i];
                }
            }
            return null;
        }

        internal void Update()
        {
            for (int i = 0; i < Skills.Count; i++)
            {
                this.Skills[i].Update();
            }
        }
    }
}
