using Common;
using Common.Battle;
using Common.Data;
using Common.Utils;
using GameServer.Core;
using GameServer.Entities;
using GameServer.Managers;
using GameServer.Models;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    /// <summary>
    /// Buff类
    /// </summary>
    public class Buff
    {
        /// <summary>
        /// 唯一Id
        /// </summary>
        public int BuffId;
        /// <summary>
        /// 所有者
        /// </summary>
        private Creature Owner;
        /// <summary>
        /// 数据buff信息
        /// </summary>
        private BuffDefine Define;
        /// <summary>
        /// 战斗上下文
        /// </summary>
        private BattleContext Context;
        /// <summary>
        /// 是否停止
        /// </summary>
        public bool Stoped;
        /// <summary>
        /// 持续时间
        /// </summary>
        private float time;
        /// <summary>
        /// 打击次数
        /// </summary>
        private int hit;

        public Buff(int buffId, Creature owner, BuffDefine buffDefine, BattleContext context)
        {
            this.BuffId = buffId;
            this.Owner = owner;
            this.Define = buffDefine;
            this.Context = context;
            this.OnAdd();
        }
        /// <summary>
        /// 添加BUFF影响
        /// </summary>
        private void OnAdd()
        {
            if(this.Define.Effect != BuffEffect.None)
            {
                this.Owner.EffectMgr.AddBuffEffect(this.Define.Effect);
            }
            this.AddAttr();
            NBuffInfo buffInfo = new NBuffInfo()
            {
                buffId = this.BuffId,
                buffType = this.Define.ID,
                casterId = this.Context.Caster.entityId,
                ownerId = this.Owner.entityId,
                Action = BuffAction.Add
            };
            Context.Battle.AddBuffAction(buffInfo);
        }
        /// <summary>
        /// 移除BUFF影响
        /// </summary>
        private void OnRemove()
        {
            RemoveAttr();
            this.Stoped = true;
            if (this.Define.Effect != BuffEffect.None)
            {
                this.Owner.EffectMgr.RemoveBuffEffect(this.Define.Effect);
            }
            NBuffInfo buffInfo = new NBuffInfo()
            {
                buffId = this.BuffId,
                buffType = this.Define.ID,
                casterId = this.Context.Caster.entityId,
                ownerId = this.Owner.entityId,
                Action = BuffAction.Remove
            };
            Context.Battle.AddBuffAction(buffInfo);
        }
        /// <summary>
        /// 增加属性
        /// </summary>
        private void AddAttr()
        {
            if(this.Define.DEFRatio != 0)
            {
                this.Owner.Attributes.Buff.DEF += this.Owner.Attributes.Basic.DEF * this.Define.DEFRatio;
                this.Owner.Attributes.InitFinalAttributes();
            }
        }
        /// <summary>
        /// 移除属性
        /// </summary>
        private void RemoveAttr()
        {
            if (this.Define.DEFRatio != 0)
            {
                this.Owner.Attributes.Buff.DEF -= this.Owner.Attributes.Basic.DEF * this.Define.DEFRatio;
                this.Owner.Attributes.InitFinalAttributes();
            }
        }
        /// <summary>
        /// 更新
        /// </summary>
        internal void Update()
        {
            if (Stoped) return;
            this.time += Time.deltaTime;
            if(this.Define.Interval > 0)
            {
                //执行伤害
                if(this.time > this.Define.Interval * (this.hit + 1))
                {
                    this.DoBuffDamage();
                }
            }
            if(this.time > this.Define.Duration)
            {
                //移除buff
                this.OnRemove();
            }
        }
        /// <summary>
        /// 产生buff伤害
        /// </summary>
        private void DoBuffDamage()
        {
            this.hit++;
            NDamageInfo damage = this.CalcBuffDamage(Context.Caster);
            Log.InfoFormat("Buff[{0}].DoBuffDamage[{1}] Damage:{2} Crit:{3}", this.Define.Name, this.Owner.Name, damage.Damage, damage.Crit);
            this.Owner.DoDamage(damage, Context.Caster);
            NBuffInfo buffInfo = new NBuffInfo()
            {
                buffId = this.BuffId,
                buffType = this.Define.ID,
                casterId = this.Context.Caster.entityId,
                ownerId = this.Owner.entityId,
                Action = BuffAction.Hit,
                Damage = damage
            };
            Context.Battle.AddBuffAction(buffInfo);
        }
        /// <summary>
        /// 计算Buff伤害
        /// </summary>
        /// <param name="caster"></param>
        /// <returns></returns>
        private NDamageInfo CalcBuffDamage(Creature caster)
        {
            //ad
            float ad = this.Define.AD + caster.Attributes.AD * this.Define.ADFactor;
            //ap
            float ap = this.Define.AP + caster.Attributes.AP * this.Define.APFactor;
            //ad伤害
            float addmg = ad * (1 - this.Owner.Attributes.DEF / (this.Owner.Attributes.DEF + 100));
            //ap伤害
            float apdmg = ad * (1 - this.Owner.Attributes.MDEF / (this.Owner.Attributes.MDEF + 100));
            //总伤害
            float final = addmg + apdmg;

            NDamageInfo damage = new NDamageInfo();
            damage.Damage = Math.Max(1, (int)final);
            damage.entityId = this.Owner.entityId;
            return damage;
        }
    }
}
