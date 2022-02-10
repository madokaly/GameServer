using Common.Data;
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
    /// 战斗类
    /// </summary>
    public class Battle
    {
        /// <summary>
        /// 属于哪张地图中
        /// </summary>
        public Map Map;
        /// <summary>
        /// 战斗中的生物字典
        /// </summary>
        Dictionary<int, Creature> AllUnits = new Dictionary<int, Creature>();
        /// <summary>
        /// 技能释放信息列表
        /// </summary>
        Queue<NSkillCastInfo> Actions = new Queue<NSkillCastInfo>();
        /// <summary>
        /// 技能释放信息列表
        /// </summary>
        List<NSkillCastInfo> CastSkills = new List<NSkillCastInfo>();
        /// <summary>
        /// 技能打击信息列表
        /// </summary>
        List<NSkillHitInfo> Hits = new List<NSkillHitInfo>();
        /// <summary>
        /// buff信息列表
        /// </summary>
        List<NBuffInfo> BuffActions = new List<NBuffInfo>();
        /// <summary>
        /// 死亡生物
        /// </summary>
        List<Creature> DeahPool = new List<Creature>();

        public Battle(Map map)
        {
            this.Map = map;
        }
        /// <summary>
        /// 转发技能释放信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        internal void ProcessBattleMessage(NetConnection<NetSession> sender, SkillCastRequest request)
        {
            Character character = sender.Session.Character;
            if(request.castInfo != null)
            {
                //效验是否合法
                if (character.entityId != request.castInfo.casterId)
                    return;
                this.Actions.Enqueue(request.castInfo);
            }
        }
        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
            //初始化
            this.CastSkills.Clear();
            this.Hits.Clear();
            this.BuffActions.Clear();
            if(Actions.Count > 0)
            {
                NSkillCastInfo skillCast = this.Actions.Dequeue();
                //执行技能释放action
                this.ExecuteAction(skillCast);
            }
            this.UpdateUnits();
            this.BroadcastHitsMessage();
        }
        /// <summary>
        /// 加入战斗
        /// </summary>
        /// <param name="unit"></param>
        public void JoinBattle(Creature unit)
        {
            this.AllUnits[unit.entityId] = unit;
        }
        /// <summary>
        /// 离开战斗
        /// </summary>
        /// <param name="unit"></param>
        public void LeaveBattle(Creature unit)
        {
            this.AllUnits.Remove(unit.entityId);
        }
        /// <summary>
        /// 处理战斗行为
        /// </summary>
        /// <param name="cast"></param>
        private void ExecuteAction(NSkillCastInfo cast)
        {
            BattleContext context = new BattleContext(this);
            context.Caster = EntityManager.Instance.GetCreature(cast.casterId);
            context.Target = EntityManager.Instance.GetCreature(cast.targetId);
            context.CastSkill = cast;
            //加入战斗
            if(context.Caster != null)
            {
                this.JoinBattle(context.Caster);
            }
            if(context.Target != null)
            {
                this.JoinBattle(context.Target);
            }
            //释放技能
            context.Caster.CastSkill(context, cast.skillId);

        }
        /// <summary>
        /// 广播技能和buff打击消息
        /// </summary>
        private void BroadcastHitsMessage()
        {
            if (this.Hits.Count == 0 && this.BuffActions.Count == 0 && this.CastSkills.Count == 0) return;
            NetMessageResponse message = new NetMessageResponse();
            if (this.CastSkills.Count > 0)
            {
                message.skillCast = new SkillCastResponse();
                message.skillCast.Result = Result.Success;
                message.skillCast.castInfoes.AddRange(this.CastSkills);
                message.skillCast.Errormsg = "";
            }
            if (this.Hits.Count > 0)
            {
                message.skillHits = new SkillHitResponse();
                message.skillHits.Result = Result.Success;
                message.skillHits.Hits.AddRange(this.Hits);
                message.skillHits.Errormsg = "";
            }
            if (this.BuffActions.Count > 0)
            {
                message.buffRes = new BuffResponse();
                message.buffRes.Result = Result.Success;
                message.buffRes.Buffs.AddRange(this.BuffActions);
                message.buffRes.Errormsg = "";
            }
            this.Map.BroadcastBattleResponse(message);
        }      
        /// <summary>
        /// 更新战斗角色
        /// </summary>
        private void UpdateUnits()
        {
            this.DeahPool.Clear();
            foreach(var kv in this.AllUnits)
            {
                //如果没有打到生物
                if (kv.Value == null) continue;
                kv.Value.Update();
                if (kv.Value.IsDeath)
                {
                    this.DeahPool.Add(kv.Value);
                }
                foreach(var death in this.DeahPool)
                {
                    this.LeaveBattle(death);
                }
            }
        }
        /// <summary>
        /// 查找技能范围中的生物
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        internal List<Creature> FindUnitsInRange(Vector3Int pos, int range)
        {
            List<Creature> result = new List<Creature>();
            foreach(var unit in this.AllUnits)
            {
                if(unit.Value.Distance(pos) < range)
                {
                    result.Add(unit.Value);
                }
            }
            return result;
        }
        /// <summary>
        /// 查找技能范围中的生物
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        internal List<Creature> FindUnitsInMapRange(Vector3Int pos, int range)
        {
            return EntityManager.Instance.GetMapEntitiesInRange<Creature>(this.Map.ID * 1000 + this.Map.InstanceId, pos, range);//instanceId---id
        }
        /// <summary>
        /// 添加技能释放信息
        /// </summary>
        /// <param name="castInfo"></param>
        public void AddCastSkillInfo(NSkillCastInfo castInfo)
        {
            this.CastSkills.Add(castInfo);
        }
        /// <summary>
        /// 添加打击信息
        /// </summary>
        /// <param name="hitInfo"></param>
        public void AddHitInfo(NSkillHitInfo hitInfo)
        {
            this.Hits.Add(hitInfo);
        }
        /// <summary>
        /// 添加BUFF信息
        /// </summary>
        /// <param name="buffInfo"></param>
        internal void AddBuffAction(NBuffInfo buffInfo)
        {
            this.BuffActions.Add(buffInfo);
        }
    }
}
