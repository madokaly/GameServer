using Common;
using Common.Battle;
using Common.Data;
using Common.Utils;
using GameServer.Core;
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
    public class Skill
    {
        /// <summary>
        /// 协议技能信息
        /// </summary>
        public NSkillInfo Info;
        /// <summary>
        /// 所有者
        /// </summary>
        public Creature Owner;
        /// <summary>
        /// 数据库技能定义
        /// </summary>
        public SkillDefine Define;
        /// <summary>
        /// 技能状态
        /// </summary>
        public SkillStatus Status;
        /// <summary>
        /// 战斗上下文
        /// </summary>
        private BattleContext Context;
        /// <summary>
        /// 子弹列表
        /// </summary>
        private List<Bullet> Bullets = new List<Bullet>();
        /// <summary>
        /// 蓄力时间
        /// </summary>
        private float castingTime = 0;
        /// <summary>
        /// 技能持续时间
        /// </summary>
        private float skillTime = 0;
        /// <summary>
        /// 技能击中次数
        /// </summary>
        private int Hit = 0;
        /// <summary>
        /// 技能cd
        /// </summary>
        private float cd = 0;
        public float CD
        {
            get { return cd; }
        }
        /// <summary>
        /// 是否瞬发
        /// </summary>
        public bool Instant
        {
            get
            {
                if (this.Define.CastTime > 0) return false;
                if (this.Define.Bullet) return false;
                if (this.Define.Duration > 0) return false;
                if (this.Define.HitTimes != null && this.Define.HitTimes.Count > 0) return false;
                return true;
            }
        }

        public Skill(NSkillInfo info, Creature owner)
        {
            this.Info = info;
            this.Owner = owner;
            this.Define = DataManager.Instance.Skills[(int)this.Owner.Define.TID][this.Info.Id];
        }
        /// <summary>
        /// 是否能释放
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public SkillResult CanCast(BattleContext context)
        {
            if(this.Status != SkillStatus.None)
            {
                //正在释放
                return SkillResult.Casting;
            }
            if (this.Define.CastTarget == TargetType.Target)
            {
                if (context.Target == null || context.Target == this.Owner)
                    //目标无效
                    return SkillResult.InvalidTarget;
                int distance = this.Owner.Distance(context.Target);
                if (distance > this.Define.CastRange)
                {
                    //超出范围
                    return SkillResult.OutOfRange;
                }
            }
            if (this.Define.CastTarget == TargetType.Position)
            {
                if(context.CastSkill.Position == null)
                {
                    //目标无效
                    return SkillResult.InvalidTarget;
                }
                if(this.Owner.Distance(context.Position) > this.Define.CastRange)
                {
                    //超出范围
                    return SkillResult.OutOfRange;
                }
            }
            if (this.Owner.Attributes.MP < this.Define.MPCost)
            {
                //蓝量不足
                return SkillResult.OutOfMp;
            }
            if (this.cd > 0)
            {
                //正在冷却
                return SkillResult.CoolDown;
            }
            return SkillResult.Ok;
        }
        /// <summary>
        /// 准备释放技能
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal SkillResult Cast(BattleContext context)
        {
            SkillResult result = this.CanCast(context);
            if(result == SkillResult.Ok)
            {
                //初始化技能状态
                this.castingTime = 0;
                this.skillTime = 0;
                this.Hit = 0;
                this.cd = this.Define.CD;
                this.Context = context;
                this.Bullets.Clear();

                this.AddBuff(TriggerType.SkillCast, this.Context.Target);

                if (this.Instant)
                {
                    //瞬发
                    this.DoHit();
                }
                else
                {
                    if(this.Define.CastTime > 0)
                    {
                        //蓄力状态
                        this.Status = SkillStatus.Casting;
                    }
                    else
                    {
                        //持续状态
                        this.Status = SkillStatus.Running;
                    }
                }
            }
            Log.InfoFormat("Skill[{0}].Cast Result:[{1}] Status:[{2}]", this.Define.Name, result, this.Status);
            return result;
        }
        /// <summary>
        /// 更新
        /// </summary>
        internal void Update()
        {
            UpdateCD();
            if (this.Status == SkillStatus.Casting)
            {
                this.UpdateCasting();
            }
            else if (this.Status == SkillStatus.Running)
            {
                this.UpdateSkill();
            }
        }
        /// <summary>
        /// 更新技能蓄力状态
        /// </summary>
        private void UpdateCasting()
        {
            if (this.castingTime < this.Define.CastTime)
            {
                this.castingTime += Time.deltaTime;
            }
            else
            {
                this.castingTime = 0;
                this.Status = SkillStatus.Running;
                Log.InfoFormat("Skill[{0}].UpdateCasting Finish", this.Define.Name);
            }
        }
        /// <summary>
        /// 更新技能持续状态
        /// </summary>
        private void UpdateSkill()
        {
            this.skillTime += Time.deltaTime;
            if (this.Define.Duration > 0)
            {
                //时间持续技能
                if (this.skillTime > this.Define.Interval * (this.Hit + 1))
                {
                    //达到造成伤害时间
                    this.DoHit();
                }
                if (this.skillTime >= this.Define.Duration)
                {
                    this.Status = SkillStatus.None;
                    Log.InfoFormat("Skill[{0}].UpdateSkill Duration Finish", this.Define.Name);
                }
            }
            else if (this.Define.HitTimes != null && this.Define.HitTimes.Count > 0)
            {
                //次数持续技能
                if (this.Hit < this.Define.HitTimes.Count)
                {
                    if (this.skillTime > this.Define.HitTimes[this.Hit])
                    {
                        //达到造成伤害时间
                        this.DoHit();
                    }
                }
                else
                {
                    if (!this.Define.Bullet)
                    {
                        this.Status = SkillStatus.None;
                        Log.InfoFormat("Skill[{0}].UpdateSkill HitTimes Finish", this.Define.Name);
                    }
                }
            }
            if (this.Define.Bullet)
            {
                //子弹技能
                bool finish = true;
                foreach (var bullet in Bullets)
                {
                    bullet.Update();
                    if (!bullet.Stoped) finish = false;
                }
                if (finish && this.Hit >= this.Define.HitTimes.Count)
                {
                    this.Status = SkillStatus.None;
                    Log.InfoFormat("Skill[{0}].UpdateSkill BulletHitTimes Finish", this.Define.Name);
                }
            }
        }
        /// <summary>
        /// 初始化打击信息
        /// </summary>
        /// <param name="isBullet">是否子弹伤害</param>
        /// <returns></returns>
        private NSkillHitInfo InitHitInfo(bool isBullet)
        {
            NSkillHitInfo hitInfo = new NSkillHitInfo();
            hitInfo.casterId = Context.Caster.entityId;
            hitInfo.skillId = this.Info.Id;
            hitInfo.hitId = this.Hit;
            hitInfo.isBullet = isBullet;
            return hitInfo;
        }
        /// <summary>
        /// 打击（判断是否为子弹技能）
        /// </summary>
        private void DoHit()
        {
            NSkillHitInfo hitInfo = this.InitHitInfo(false);
            Log.InfoFormat("Skill[{0}].DoHit[{1}]", this.Define.Name, this.Hit);
            this.Hit++;
            if (this.Define.Bullet)
            {
                //子弹类型
                this.CastBullet(hitInfo);
                return;
            }
            DoHit(hitInfo);
        }
        /// <summary>
        /// 打击（造成伤害）
        /// </summary>
        public void DoHit(NSkillHitInfo hitInfo)
        {
            Context.Battle.AddHitInfo(hitInfo);
            Log.InfoFormat("Skill[{0}].DoHit[{1}] IsBullet[{2}]", this.Define.Name, this.Hit, hitInfo.isBullet);
            if (this.Define.AOERange > 0)
            {
                //AOE
                this.HitRange(hitInfo);
                return;
            }
            if(this.Define.CastTarget == TargetType.Target)
            {
                //目标类型
                this.HitTarget(Context.Target, hitInfo);
            }

        }
 
        /// <summary>
        /// 释放子弹
        /// </summary>
        private void CastBullet(NSkillHitInfo hitInfo)
        {
            Context.Battle.AddHitInfo(hitInfo);
            Log.InfoFormat("Skill[{0}].CastBullet[{1}] Target[{2}]", this.Define.Name, this.Define.BulletResource, this.Context.Target);
            Bullet bullet = new Bullet(this, this.Context.Target, hitInfo);
            Bullets.Add(bullet);
        }
        /// <summary>
        /// 范围类型的技能攻击
        /// </summary>
        private void HitRange(NSkillHitInfo hitInfo)
        {
            //技能中心
            Vector3Int pos;
            if(this.Define.CastTarget == TargetType.Target)
            {
                pos = Context.Target.Position;
            }
            else if(this.Define.CastTarget == TargetType.Position)
            {
                pos = Context.Position;
            }
            else
            {
                pos = this.Owner.Position;
            }
            //查找范围中的目标生物
            List<Creature> units = this.Context.Battle.FindUnitsInMapRange(pos, this.Define.AOERange);
            foreach(var target in units)
            {
                this.HitTarget(target, hitInfo);
            }
        }
        /// <summary>
        /// 目标类型的技能攻击
        /// </summary>
        /// <param name="target"></param>
        private void HitTarget(Creature target, NSkillHitInfo hitInfo)
        {

            if (this.Define.CastTarget == TargetType.Self && (target != Context.Caster)) return;
            else if (target == Context.Caster) return;
            NDamageInfo damage = this.CalcSkillDamage(Context.Caster, target);
            Log.InfoFormat("Skill[{0}].HitTarget[{1}] Damage:{2} Crit:{3}", this.Define.Name, target.Name, damage.Damage, damage.Crit);
            target.DoDamage(damage, Context.Caster);
            hitInfo.Damages.Add(damage);

            this.AddBuff(TriggerType.SkillHit, target);
        }
        /// <summary>
        /// 添加BUFF
        /// </summary>
        /// <param name="trigger"></param>
        private void AddBuff(TriggerType trigger, Creature target)
        {
            if (this.Define.Buff == null || this.Define.Buff.Count == 0) return;
            foreach(var buffId in this.Define.Buff)
            {
                var buffDefine = DataManager.Instance.Buffs[buffId];
                if (buffDefine.Trigger != trigger) continue;
                if(buffDefine.Target == TargetType.Self)
                {
                    this.Owner.AddBuff(this.Context, buffDefine);
                }
                else if(buffDefine.Target == TargetType.Target)
                {
                    target.AddBuff(this.Context, buffDefine);
                }
            }
        }

        /// <summary>
        /// 计算技能伤害
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private NDamageInfo CalcSkillDamage(Creature caster, Creature target)
        {
            //ad
            float ad = this.Define.AD + caster.Attributes.AD * this.Define.ADFactor;
            //ap
            float ap = this.Define.AP + caster.Attributes.AP * this.Define.APFactor;
            //ad伤害
            float addmg = ad * (1 - target.Attributes.DEF / (target.Attributes.DEF + 100));
            //ap伤害
            float apdmg = ad * (1 - target.Attributes.MDEF / (target.Attributes.MDEF + 100));
            //总伤害
            float final = addmg + apdmg;
            //是否暴击
            bool isCrit = IsCrit(caster.Attributes.CRI);
            if (isCrit) final = final * 2f;
            //随机浮动上下5%
            final = final * (float)MathUtil.Random.NextDouble() * 0.1f - 0.05f;
            NDamageInfo damage = new NDamageInfo();
            damage.Damage = Math.Max(1, (int)final);
            damage.entityId = target.entityId;
            damage.Crit = isCrit;
            return damage;
        }
        /// <summary>
        /// 是否暴击
        /// </summary>
        /// <param name="crit"></param>
        /// <returns></returns>
        private bool IsCrit(float crit)
        {
            return MathUtil.Random.NextDouble() < crit;
        }

        /// <summary>
        /// 更新cd
        /// </summary>
        private void UpdateCD()
        {
            if(this.cd > 0)
            {
                this.cd -= Time.deltaTime;
            }
            if(this.cd < 0)
            {
                this.cd = 0;
            }
        }
    }
}
