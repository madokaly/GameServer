using Common;
using GameServer.Entities;
using GameServer.Network;
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
    /// 好友管理器
    /// </summary>
    class FriendManager
    {
        /// <summary>
        /// 玩家
        /// </summary>
        Character Owner;
        /// <summary>
        /// 好友列表
        /// </summary>
        List<NFriendInfo> friends = new List<NFriendInfo>();
        /// <summary>
        /// 是否改变
        /// </summary>
        bool friendChanged = false;

        public FriendManager(Character owner)
        {
            this.Owner = owner;
            this.InitFriends();
        }
        /// <summary>
        /// 获得好友列表
        /// </summary>
        /// <param name="list"></param>
        public void GetFriendInfos(List<NFriendInfo> list)
        {
            foreach (var f in this.friends)
            {
                list.Add(f);
            }
        }
        /// <summary>
        /// 从数据库中初始化好友列表
        /// </summary>
        private void InitFriends()
        {
            this.friends.Clear();
            foreach (var friend in this.Owner.Data.Friends)
            {
                this.friends.Add(GetFriendInfo(friend));
            }
        }
        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="friend"></param>
        public void AddFriend(Character friend)
        {
            TCharacterFriend tf = new TCharacterFriend()
            {
                FriendID = friend.Id,
                FriendName = friend.Data.Name,
                Class = friend.Data.Class,
                Level = friend.Data.Level
            };
            this.Owner.Data.Friends.Add(tf);
            friendChanged = true;
        }
        /// <summary>
        /// 移除好友
        /// </summary>
        /// <param name="friendId">好友Id</param>
        /// <returns></returns>
        public bool RemoveFriendByFriendId(int friendId)
        {
            var removeItem = this.Owner.Data.Friends.FirstOrDefault(v => v.FriendID == friendId);
            if (removeItem != null)
            {
                DBService.Instance.Entities.CharacterFriends.Remove(removeItem);
            }
            friendChanged = true;
            return true;
        }
        /// <summary>
        /// 移除好友
        /// </summary>
        /// <param name="Id">角色Id</param>
        /// <returns></returns>
        public bool RemoveFriendById(int Id)
        {
            var removeItem = this.Owner.Data.Friends.FirstOrDefault(v => v.Id == Id);
            if (removeItem != null)
            {
                DBService.Instance.Entities.CharacterFriends.Remove(removeItem);
            }
            friendChanged = true;
            return true;
        }

        /// <summary>
        /// 获得好友信息
        /// </summary>
        /// <param name="friend"></param>
        /// <returns></returns>
        public NFriendInfo GetFriendInfo(TCharacterFriend friend)
        {
            NFriendInfo friendInfo = new NFriendInfo();
            var character = CharacterManager.Instance.GetCharacter(friend.FriendID);
            friendInfo.friendInfo = new NCharacterInfo();
            friendInfo.Id = friend.Id;
            if (character == null)
            {
                friendInfo.friendInfo.Id = friend.FriendID;
                friendInfo.friendInfo.Name = friend.FriendName;
                friendInfo.friendInfo.Class = (CharacterClass)friend.Class;
                friendInfo.friendInfo.Level = friend.Level;
                friendInfo.Status = 0;
            }
            else
            {
                friendInfo.friendInfo = character.GetBasicInfo();//friendInfo.friendInfo = GetBasicInfo(character.info);
                friendInfo.friendInfo.Name = character.Info.Name;
                friendInfo.friendInfo.Class = character.Info.Class;
                friendInfo.friendInfo.Level = character.Info.Level;
                if (friend.Level != character.Info.Level)
                {
                    friend.Level = character.Info.Level;
                }
                character.FriendManager.UpdateFriendInfo(this.Owner.Info, 1);
                friendInfo.Status = 1;
            }

            return friendInfo;
        }
        /// <summary>
        /// 获得好友信息
        /// </summary>
        /// <param name="friendId"></param>
        /// <returns></returns>
        public NFriendInfo GetFriendInfo(int friendId)
        {
            foreach (var f in this.friends)
            {
                if (f.friendInfo.Id == friendId)
                {
                    return f;
                }
            }
            return null;
        }

        /// <summary>
        /// 更新好友信息
        /// </summary>
        /// <param name="friendInfo"></param>
        /// <param name="status"></param>
        public void UpdateFriendInfo(NCharacterInfo friendInfo, int status)
        {
            foreach (var f in this.friends)
            {
                if (f.friendInfo.Id == friendInfo.Id)
                {
                    f.Status = status;
                    break;
                }
            }
            this.friendChanged = true;
        }
        /// <summary>
        /// 下线通知
        /// </summary>
        public void OfflineNotify()
        {
            foreach (var frienfInfo in this.friends)
            {
                var friend = CharacterManager.Instance.GetCharacter(frienfInfo.friendInfo.Id);
                if (friend != null)
                {
                    friend.FriendManager.UpdateFriendInfo(this.Owner.Info, 0);
                }
            }
        }

        /// <summary>
        /// 好友状态变化后处理
        /// </summary>
        /// <param name="message"></param>
        public void PostProcess(NetMessageResponse message)
        {
            //发生变化会顺道发送
            if (friendChanged)
            {
                Log.InfoFormat("PostProcess > FriendManager:characterID:{0}:{1}", this.Owner.Id, this.Owner.Info.Name);
                this.InitFriends();
                if (message.friendList == null)
                {
                    message.friendList = new FriendListResponse();
                    message.friendList.Friends.AddRange(this.friends);
                }
                friendChanged = false;
            }
        }
    }
}
