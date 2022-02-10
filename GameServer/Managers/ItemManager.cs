using Common;
using GameServer.Entities;
using Models;
using GameServer.Services;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    /// <summary>
    /// 道具管理器
    /// </summary>
    class ItemManager
    {
        /// <summary>
        /// 拥有角色
        /// </summary>
        Character Owner;
        /// <summary>
        /// [道具id，道具]字典
        /// </summary>
        public Dictionary<int, Item> Items = new Dictionary<int, Item>();

        public ItemManager(Character owner)
        {
            this.Owner = owner;

            foreach (var item in owner.Data.Items)
            {
                this.Items.Add(item.ItemID, new Item(item));
            }
        }

        /// <summary>
        /// 使用道具
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool UserItem(int itemId, int count = 1)
        {
            Log.InfoFormat("[{0}]UseItem[{1}:{2}]", this.Owner.Data.ID, itemId, count);
            Item item = null;
            if (Items.TryGetValue(itemId, out item))
            {
                if (item.Count < count)
                {
                    if (item.Count < count)
                    {
                        return false;
                    }

                    //TODO:增加使用逻辑

                    item.Remove(count);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否拥有道具
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public bool HasItem(int itemId)
        {
            Item item = null;
            if (this.Items.TryGetValue(itemId, out item))
            {
                return item.Count > 0;
            }
            return false;
        }

        /// <summary>
        /// 获得指定道具
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public Item GetItem(int itemId)
        {
            Item item = null;
            this.Items.TryGetValue(itemId, out item);

            return item;
        }

        /// <summary>
        /// 添加道具
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool AddItem(int itemId, int count)
        {
            Item item = null;
            if (this.Items.TryGetValue(itemId, out item))
            {
                item.Add(count);
            }
            else
            {
                TCharacterItem dbitem = new TCharacterItem();
                dbitem.CharacterID = Owner.Data.ID;
                dbitem.Owner = Owner.Data;
                dbitem.ItemID = itemId;
                dbitem.ItemCount = count;
                Owner.Data.Items.Add(dbitem);
                item = new Item(dbitem);
                this.Items.Add(itemId, item);
            }
            this.Owner.StatusManager.AddItemChange(itemId, count, StatusAction.Add);
            Log.InfoFormat("[{0}]AddItem[{1}] addCount:{2}", this.Owner.Data.ID, item, count);
            DBService.Instance.Save();
            return true;
        }

        /// <summary>
        /// 移除道具
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool RemoveItem(int itemId, int count)
        {
            if (!Items.ContainsKey(itemId))
            {
                return false;
            }
            Item item = this.Items[itemId];
            if (item.Count < count)
            {
                return false;
            }
            item.Remove(count);
            this.Owner.StatusManager.AddItemChange(itemId, count, StatusAction.Delete);
            Log.InfoFormat("[{0}]RemoveItem[{1}] addCount:{2}", this.Owner.Data.ID, item, count);
            DBService.Instance.Save();
            return true;
        }

        /// <summary>
        /// 获得协议道具信息
        /// </summary>
        /// <param name="list"></param>
        public void GetItemInfos(List<NItemInfo> list)
        {
            foreach (var item in this.Items)
            {
                list.Add(new NItemInfo()
                    {
                        Id = item.Value.ItemID,
                        Count = item.Value.Count
                    }
                );
            }
        }

    }
}
