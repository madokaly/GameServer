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
    /// AI代理类
    /// </summary>
    public class AIAgent
    {
        private Monster monster;

        private AIBase ai;

        public AIAgent(Monster monster)
        {
            this.monster = monster;
            string aiName = monster.Define.AI;
            if (string.IsNullOrEmpty(aiName)) aiName = AIMonsterPassive.ID;
            //选择使用AI
            switch (aiName)
            {
                case AIMonsterPassive.ID:
                    this.ai = new AIMonsterPassive(monster);
                    break;
                case AIBoss.ID:
                    this.ai = new AIBoss(monster);
                    break;
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void Update()
        {
           if(this.ai != null)
            {
                this.ai.Update();
            }
        }

        internal void Init()
        {
            
        }

        /// <summary>
        /// 收到伤害
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="source"></param>
        internal void OnDamage(NDamageInfo damage, Creature source)
        {
            if (this.ai != null)
            {
                this.ai.OnDamage(damage, source);
            }
        }
    }
}
