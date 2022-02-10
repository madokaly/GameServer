using Common;
using Common.Data;
using GameServer.Managers;
using GameServer.Services;
using Network;
using SkillBridge.Message;

namespace Managers
{
    /// <summary>
    /// 商店管理器
    /// </summary>
    class ShopManager:Singleton<ShopManager>
    {
        /// <summary>
        /// 购买物品
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="shopId"></param>
        /// <param name="shopItemId"></param>
        /// <returns></returns>
        public Result BuyItem(NetConnection<NetSession> sender, int shopId, int shopItemId)
        {
            if (!DataManager.Instance.Shops.ContainsKey(shopId))
            {
                return Result.Failed;
            }
            ShopItemDefine shopItem;
            if (DataManager.Instance.ShopItems[shopId].TryGetValue(shopItemId, out shopItem))
            {
                if (sender.Session.Character.Gold >= shopItem.Price)
                {
                    sender.Session.Character.ItemManager.AddItem(shopItem.ItemID, shopItem.Count);
                    sender.Session.Character.Gold -= shopItem.Price;
                    DBService.Instance.Save();
                    return Result.Success;
                }
            }
            return Result.Failed;
        }
    }
}
