using Common.Battle;
using Common.Data;
using GameServer.Battle;
using GameServer.Core;
using GameServer.Managers;
using GameServer.Models;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Entities
{
    /// <summary>
    /// 生物类
    /// </summary>
    public class Creature : Entity
    {

        public int Id
        {
            get;set;
        }
        public string Name { get { return this.Info.Name; } }
        /// <summary>
        /// 协议信息
        /// </summary>
        public NCharacterInfo Info;
        /// <summary>
        /// 数据库信息
        /// </summary>
        public CharacterDefine Define;
        /// <summary>
        /// 战斗属性
        /// </summary>
        public Attributes Attributes;
        /// <summary>
        /// 所属地图
        /// </summary>
        public Map Map;
        /// <summary>
        /// 技能管理器
        /// </summary>
        public SkillManager SkillMgr;
        /// <summary>
        /// Buff管理器
        /// </summary>
        public BuffManager BuffMgr;
        /// <summary>
        /// 影响管理器
        /// </summary>
        public EffectManager EffectMgr;
        /// <summary>
        /// 是否死亡
        /// </summary>
        public bool IsDeath = false;
        /// <summary>
        /// 战斗状态
        /// </summary>
        public BattleState State;
        /// <summary>
        /// 角色状态
        /// </summary>
        public CharacterState CharacterState;

        public Creature(CharacterType type, int configId, int level, Vector3Int pos, Vector3Int dir) :
           base(pos, dir)
        {
            this.Define = DataManager.Instance.Characters[configId];

            this.Info = new NCharacterInfo();
            this.Info.Type = type;
            this.Info.Level = level;
            this.Info.ConfigId = configId;
            this.Info.Entity = this.EntityData;
            this.Info.EntityId = this.entityId;
            this.Define = DataManager.Instance.Characters[configId];
            this.Info.Name = this.Define.Name;
            this.InitSkills();
            this.InitBuffs();
            this.Attributes = new Attributes();
            this.Attributes.Init(this.Define, this.Info.Level, this.GetEquips(), this.Info.attrDynamic);
            this.Info.attrDynamic = this.Attributes.DynamicAttr;
        }

        /// <summary>
        /// 初始化技能
        /// </summary>
        private void InitSkills()
        {
            SkillMgr = new SkillManager(this);
            this.Info.Skills.AddRange(this.SkillMgr.Infos);
        }
        /// <summary>
        /// 初始化buff
        /// </summary>
        private void InitBuffs()
        {
            BuffMgr = new BuffManager(this);
            EffectMgr = new EffectManager(this);
        }
        /// <summary>
        /// 获得装备列表虚函数
        /// </summary>
        /// <returns></returns>
        public virtual List<EquipDefine> GetEquips()
        {
            return null;
        }
        /// <summary>
        /// 释放技能
        /// </summary>
        /// <param name="context"></param>
        /// <param name="skillId"></param>
        internal void CastSkill(BattleContext context, int skillId)
        {
            Skill skill = this.SkillMgr.GetSkill(skillId);
            context.Result = skill.Cast(context);
            if(context.Result == SkillResult.Ok)
            {
                //战斗状态
                this.State = BattleState.InBattle;
            }
            if(context.CastSkill == null)
            {
                if (context.Result == SkillResult.Ok)
                {
                    //怪物释放技能
                    context.CastSkill = new NSkillCastInfo()
                    {
                        casterId = this.entityId,
                        targetId = context.Target.entityId,
                        skillId = skillId,
                        Position = new NVector3(),
                        Result = context.Result
                    };
                    context.Battle.AddCastSkillInfo(context.CastSkill);
                } 
            }
            else
            {
                //玩家释放技能
                context.CastSkill.Result = context.Result;
                context.Battle.AddCastSkillInfo(context.CastSkill);
            }
        }
        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="damage"></param>
        internal void DoDamage(NDamageInfo damage, Creature source)
        {
            //战斗状态
            this.State = BattleState.InBattle;
            this.Attributes.HP -= damage.Damage;
            if(this.Attributes.HP < 0)
            {
                this.IsDeath = true;
                damage.WillDead = true;
            }
            this.OnDamage(damage, source);
        }

        /// <summary>
        /// 计算距离
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        internal int Distance(Creature target)
        {
            return (int)Vector3Int.Distance(this.Position, target.Position);
        }
        internal int Distance(Vector3Int position)
        {
            return (int)Vector3Int.Distance(this.Position, position);
        }
        /// <summary>
        /// 更新
        /// </summary>
        public override void Update()
        {
            this.SkillMgr.Update();
            this.BuffMgr.Update();
        }
        /// <summary>
        /// 添加BUFF
        /// </summary>
        /// <param name="context"></param>
        /// <param name="buffDefine"></param>
        internal void AddBuff(BattleContext context, BuffDefine buffDefine)
        {
            this.BuffMgr.AddBuff(context, buffDefine);
        }

        /// <summary>
        /// 受到伤害时
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="source">来源</param>
        protected virtual void OnDamage(NDamageInfo damage, Creature source)
        {
            
        }

        public virtual void OnEnterMap(Map map)
        {
            this.Map = map;
        }

        public void OnLeaveMap(Map map)
        {
            this.Map = null;
        }
    }
}
