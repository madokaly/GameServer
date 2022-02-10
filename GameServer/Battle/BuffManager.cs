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
    /// Buff管理器
    /// </summary>
    public class BuffManager
    {
        /// <summary>
        /// buff所有者
        /// </summary>
        private Creature Owner;
        /// <summary>
        /// buff列表
        /// </summary>
        private List<Buff> Buffs = new List<Buff>();

        private int idx = 1;
        /// <summary>
        /// 唯一id
        /// </summary>
        private int BuffId
        {
            get { return this.idx++; }
        }

        public BuffManager(Creature owner)
        {
            this.Owner = owner;
        }
        /// <summary>
        /// 增加BUFF
        /// </summary>
        /// <param name="context"></param>
        /// <param name="buffDefine"></param>
        internal void AddBuff(BattleContext context, BuffDefine buffDefine)
        {
            Buff buff = new Buff(this.BuffId, this.Owner, buffDefine, context);
            this.Buffs.Add(buff);
        }
        /// <summary>
        /// 更新
        /// </summary>
        internal void Update()
        {
            for(int i = 0; i < this.Buffs.Count; i++)
            {
                if(!this.Buffs[i].Stoped)
                {
                    this.Buffs[i].Update();
                }
            }
            this.Buffs.RemoveAll((buff) => buff.Stoped);
        }
    }
}
