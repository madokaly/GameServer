using Common;
using Common.Battle;
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
    /// 影响管理器
    /// </summary>
    public class EffectManager
    {
        /// <summary>
        /// 所有者
        /// </summary>
        private Creature Owner;
        /// <summary>
        /// [影响类型，次数]字典
        /// </summary>
        private Dictionary<BuffEffect, int> Effects = new Dictionary<BuffEffect, int>();

        public EffectManager(Creature owner)
        {
            this.Owner = owner;
        }
        /// <summary>
        /// 是否有指定影响类型
        /// </summary>
        /// <param name="effect"></param>
        /// <returns></returns>
        internal bool HasEffect(BuffEffect effect)
        {
            if(this.Effects.TryGetValue(effect, out int val))
            {
                return val > 0;
            }
            return false;
        }
        /// <summary>
        /// 添加buff影响
        /// </summary>
        /// <param name="effect"></param>
        internal void AddBuffEffect(BuffEffect effect)
        {
            Log.InfoFormat("[{0}.AddEffect {1}]", this.Owner.Name, effect);
            if (!this.Effects.ContainsKey(effect))
            {
                this.Effects[effect] = 1;
            }
            else
            {
                this.Effects[effect]++;
            }
        }
        /// <summary>
        /// 移除buff影响
        /// </summary>
        /// <param name="effect"></param>
        internal void RemoveBuffEffect(BuffEffect effect)
        {
            Log.InfoFormat("[{0}.RemoveEffect {1}]", this.Owner.Name, effect);
            if (this.Effects[effect] > 0)
            {
                this.Effects[effect]--;
            }
        }
    }
}
