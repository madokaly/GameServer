using Common;
using GameServer.Entities;
using GameServer.Managers;
using Managers;
using Network;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// 道具服务类
    /// </summary>
    class ItemService : Singleton<ItemService>
    {
        public ItemService()
        {
            //监听购买请求
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ItemBuyRequest>(this.OnItemBuy);
            //监听装备请求
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ItemEquipRequest>(this.OnItemEquip);
        }



        public void Init()
        {

        }

        /// <summary>
        /// 收到购买道具请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        private void OnItemBuy(NetConnection<NetSession> sender, ItemBuyRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnItemBuy: character:{0} Shop:{1} ShopItem: {2}", character.Id,request.shopId,request.shopItemId);
            //购买道具
            var result = ShopManager.Instance.BuyItem(sender,request.shopId,request.shopItemId);
            sender.Session.Response.itemBuy = new ItemBuyResponse();
            sender.Session.Response.itemBuy.Result = result;
            sender.SendResponse();

        }

        /// <summary>
        /// 收到穿脱道具请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        private void OnItemEquip(NetConnection<NetSession> sender, ItemEquipRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("ItemEquip: character:{0}  Slot:{1} Item: {2} Equip:{3}", character.Id, request.Slot, request.itemId,request.isEquip);
            //穿脱装备
            var result = EquipManager.Instance.EquipItem(sender,request.Slot, request.itemId, request.isEquip);
            sender.Session.Response.itemEquip = new ItemEquipResponse();
            sender.Session.Response.itemEquip.Result = result;
            sender.SendResponse();
        }

    }
}
