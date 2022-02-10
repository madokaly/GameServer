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
    /// 战斗上下文类
    /// </summary>
    public class BattleContext
    {
        /// <summary>
        /// 战斗
        /// </summary>
        public Battle Battle;
        /// <summary>
        /// 释放者
        /// </summary>
        public Creature Caster;
        /// <summary>
        /// 目标
        /// </summary>
        public Creature Target;
        /// <summary>
        /// 释放位置
        /// </summary>
        public Vector3Int Position { get { return this.CastSkill.Position; } }
        /// <summary>
        /// 协议释放技能信息
        /// </summary>
        public NSkillCastInfo CastSkill;
        /// <summary>
        /// 技能释放结果
        /// </summary>
        public SkillResult Result;

        public BattleContext(Battle battle)
        {
            this.Battle = battle;
        }
    }
}
