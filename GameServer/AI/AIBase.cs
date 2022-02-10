using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Battle;
using GameServer.Battle;
using GameServer.Entities;
using SkillBridge.Message;

namespace GameServer.AI
{
    /// <summary>
    /// AI基础类
    /// </summary>
    public class AIBase
    {
        /// <summary>
        /// 所属怪物
        /// </summary>
        private Monster owner;
        /// <summary>
        /// 目标
        /// </summary>
        private Creature target;
        /// <summary>
        /// 普通攻击
        /// </summary>
        private Skill normalSkill;

        public AIBase(Monster monster)
        {
            this.owner = monster;
            this.normalSkill = this.owner.SkillMgr.NormalSkill;
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void Update()
        {
            if (this.owner.State == BattleState.InBattle)
            {
                this.UpdateBattle();
            }
        }

        /// <summary>
        /// 更新战斗
        /// </summary>
        private void UpdateBattle()
        {
            if (this.target == null)
            {
                this.owner.State = BattleState.Idle;
                return;
            }
            if (!TryCastSkill())
            {
                if (!TryCastNormalSkill())
                {
                    this.FollowTarget();
                }
            }

        }

        /// <summary>
        /// 尝试释放技能
        /// </summary>
        /// <returns></returns>
        private bool TryCastSkill()
        {
            if (this.target != null)
            {
                BattleContext context = new BattleContext(this.owner.Map.Battle)
                {
                    Target = this.target,
                    Caster = this.owner
                };
                Skill skill = this.owner.FindSkill(context, SkillType.Skill);
                if (skill != null)
                {
                    this.owner.CastSkill(context, skill.Define.ID);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 尝试普通攻击
        /// </summary>
        /// <returns></returns>
        private bool TryCastNormalSkill()
        {
            if (this.target != null)
            {
                BattleContext context = new BattleContext(this.owner.Map.Battle)
                {
                    Target = this.target,
                    Caster = this.owner
                };
                var result = normalSkill.CanCast(context);
                if (result == SkillResult.Ok)
                {
                    this.owner.CastSkill(context, normalSkill.Define.ID);
                }
                if(result == SkillResult.OutOfRange)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 追击目标
        /// </summary>
        private void FollowTarget()
        {
            int distance = this.owner.Distance(this.target);
            if(distance > this.normalSkill.Define.CastRange - 50)
            {
                this.owner.MoveTo(this.target.Position);
            }
            else
            {
                this.owner.StopMove();
            }
        }
        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="source"></param>
        internal void OnDamage(NDamageInfo damage, Creature source)
        {
            this.target = source;
        }
    }
}

