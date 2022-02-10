using Common;
using GameServer.Entities;
using GameServer.Services;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    /// <summary>
    /// 装备管理器
    /// </summary>
    class EquipManager : Singleton<EquipManager>
    {
        /// <summary>
        /// 穿脱装备
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="slot"></param>
        /// <param name="itemId"></param>
        /// <param name="isEquip"></param>
        /// <returns></returns>
        public Result EquipItem(NetConnection<NetSession> sender, int slot, int itemId, bool isEquip)
        {
            Character character = sender.Session.Character;
            //效验装备是否存在
            if (!character.ItemManager.Items.ContainsKey(itemId))
            {
                return Result.Failed;
            }
            UpdateEquip(character.Data.Equips, slot, itemId, isEquip);
            DBService.Instance.Save();
            return Result.Success;

        }
        /// <summary>
        /// 穿脱装备
        /// </summary>
        /// <param name="equipData"></param>
        /// <param name="slot"></param>
        /// <param name="itemId"></param>
        /// <param name="isEquip"></param>
        unsafe void UpdateEquip(byte[] equipData, int slot, int itemId, bool isEquip)
        {
            fixed (byte* pt = equipData)
            {
                int* slotid = (int*)(pt + sizeof(int) * slot);
                if (isEquip)
                {
                    //穿
                    *slotid = itemId;
                }
                else
                    //脱
                    *slotid = 0;
            }
        }
    }
}
