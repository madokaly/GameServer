using GameServer.Entities;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    /// <summary>
    /// 状态管理器
    /// </summary>
    class StatusManager
    {
        /// <summary>
        /// 所属角色
        /// </summary>
        Character Owner;
        /// <summary>
        /// 状态集合
        /// </summary>
        private List<NStatus> Status { get; set; }

        public bool HasStatus
        {
            get { return this.Status.Count > 0; }
        }

        public StatusManager(Character owner)
        {
            this.Owner = owner;
            this.Status = new List<NStatus>();
        }

        public void AddStatus(StatusType type, int id, int value, StatusAction action)
        {
            this.Status.Add(new NStatus()
            {
                Type = type,
                Id = id,
                Value = value,
                Action = action
            });
        }
        /// <summary>
        /// 金币变化
        /// </summary>
        /// <param name="goldDelta"></param>
        public void AddGoldChange(int goldDelta)
        {
            if (goldDelta > 0)
            {
                this.AddStatus(StatusType.Money, 0, goldDelta, StatusAction.Add);
            }
            if (goldDelta < 0)
            {
                this.AddStatus(StatusType.Money, 0, -goldDelta, StatusAction.Delete);
            }
        }
        /// <summary>
        /// 经验变化
        /// </summary>
        /// <param name="expDelta"></param>
        public void AddExpChange(int expDelta)
        {
            this.AddStatus(StatusType.Exp, 0, expDelta, StatusAction.Add);
        }
        /// <summary>
        /// 等级变化
        /// </summary>
        /// <param name="levelDelta"></param>
        public void AddLevelUp(int levelDelta)
        {
            this.AddStatus(StatusType.Level, 0, levelDelta, StatusAction.Add);
        }
        /// <summary>
        /// 道具变化
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <param name="action"></param>
        public void AddItemChange(int id, int count, StatusAction action)
        {
            this.AddStatus(StatusType.Item, id, count, action);
        }
        /// <summary>
        /// 状态变化后处理
        /// </summary>
        /// <param name="message"></param>
        public void PostProcess(NetMessageResponse message)
        {
            if (message.statusNotify == null)
                message.statusNotify = new StatusNotify();
            foreach(var status in this.Status)
            {
                message.statusNotify.Status.Add(status);
            }
            this.Status.Clear();
        }
    }
}
