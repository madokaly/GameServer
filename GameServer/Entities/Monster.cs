using Common.Battle;
using GameServer.AI;
using GameServer.Battle;
using GameServer.Core;
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
    /// 怪物类
    /// </summary>
    public class Monster : Creature
    {

        /// <summary>
        /// AI代理
        /// </summary>
        private AIAgent AI;
        /// <summary>
        /// 移动目标
        /// </summary>
        private Vector3Int moveTarget;
        /// <summary>
        /// 移动时坐标
        /// </summary>
        private Vector3 movePosition;

        public Monster(int tid, int level, Vector3Int pos, Vector3Int dir) : base(CharacterType.Monster, tid, level, pos, dir)
        {
            this.AI = new AIAgent(this);
        }

        public override void Update()
        {
            base.Update();
            //移动
            this.UpdateMovement();
            this.AI.Update();
        }

        /// <summary>
        /// 进入地图时
        /// </summary>
        /// <param name="map"></param>
        public override void OnEnterMap(Map map)
        {
            base.OnEnterMap(map);
            this.AI.Init();
        }

        /// <summary>
        /// 寻找可用技能
        /// </summary>
        /// <returns></returns>
        public Skill FindSkill(BattleContext context, SkillType type)
        {
            Skill cancast = null;
            foreach(var skill in this.SkillMgr.Skills)
            {
                if ((skill.Define.Type & type) != skill.Define.Type) continue;
                var result = skill.CanCast(context);
                if(result == SkillResult.Casting)
                {
                    return null;
                }
                if(result == SkillResult.Ok)
                {
                    cancast = skill;
                }
            }
            return cancast;
        }

        /// <summary>
        /// 受到伤害时
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="source"></param>
        protected override void OnDamage(NDamageInfo damage, Creature source)
        {
            if(this.AI != null)
            {
                this.AI.OnDamage(damage, source);
            }
        }

        /// <summary>
        /// 更新自身移动
        /// </summary>
        private void UpdateMovement()
        {
            if(this.CharacterState == CharacterState.Move)
            {
                if(this.Distance(this.moveTarget) < 50)
                {
                    this.StopMove();
                }
                if(this.Speed > 0)
                {
                    Vector3 dir = this.Direction;
                    this.movePosition += dir * Speed * Time.deltaTime / 100f;
                    this.Position = this.movePosition;
                }
            }
        }

        /// <summary>
        /// 同步向目标移动
        /// </summary>
        /// <param name="position"></param>
        internal void MoveTo(Vector3Int position)
        {
            if(CharacterState == CharacterState.Idle)
            {
                CharacterState = CharacterState.Move;
            }
            if(this.moveTarget != position)
            {
                this.moveTarget = position;
                this.movePosition = this.Position;
                var dist = this.moveTarget - this.Position;
                this.Direction = dist.normalized;
                this.Speed = this.Define.Speed;
                //同步
                NEntitySync sync = new NEntitySync();
                sync.Entity = this.EntityData;
                sync.Event = EntityEvent.MoveFwd;
                sync.Id = this.entityId;
                this.Map.UpdateEntity(sync);
            }
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        internal void StopMove()
        {
            this.CharacterState = CharacterState.Idle;
            this.moveTarget = Vector3Int.zero;
            this.Speed = 0;
            //同步
            NEntitySync sync = new NEntitySync();
            sync.Entity = this.EntityData;
            sync.Event = EntityEvent.Idle;
            sync.Id = this.entityId;
            this.Map.UpdateEntity(sync);
        }
    }
}
