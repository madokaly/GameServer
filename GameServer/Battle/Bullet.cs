using Common;
using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.Battle
{
    public class Bullet
    {
        /// <summary>
        /// 所有者技能
        /// </summary>
        private Skill skill;
        /// <summary>
        /// 打击信息
        /// </summary>
        private NSkillHitInfo hitInfo;
        /// <summary>
        /// 时间模式
        /// </summary>
        private bool TimeMode = true;
        /// <summary>
        /// 当前飞行时间
        /// </summary>
        private float flyTime = 0;
        /// <summary>
        /// 当前位置
        /// </summary>
        private NVector3 pos;
        /// <summary>
        /// 持续时间
        /// </summary>
        private float duration = 0;
        /// <summary>
        /// 是否到达
        /// </summary>
        public bool Stoped = false;

        public Bullet(Skill skill, Creature target, NSkillHitInfo hitInfo)
        {
            this.skill = skill;
            this.hitInfo = hitInfo;
            int distance = skill.Owner.Distance(target);
            if (TimeMode)
            {
                duration = distance / this.skill.Define.BulletSpeed;
            }
            Log.InfoFormat("Bullet[{0}].CastBullet[{1}] Target:{2} Distance:{3} Time:{4}", this.skill.Define.Name, this.skill.Define.BulletResource, target.Name, distance, this.duration);
        }
        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            if (Stoped) return;
            if (TimeMode)
            {
                //更新时间
                this.UpdateTime();
            }
            else
            {
                //更新位置
                this.UpdatePos();
            }
        }
        /// <summary>
        /// 更新时间
        /// </summary>
        public void UpdateTime()
        {
            this.flyTime += Time.deltaTime;
            if(this.flyTime > duration)
            {
                this.hitInfo.isBullet = true;
                this.skill.DoHit(this.hitInfo);
                this.Stoped = true;
            }
        }
        /// <summary>
        /// 更新位置
        /// </summary>
        private void UpdatePos()
        {

        }
    }
}
