using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Battle;
using GameServer.Entities;

namespace GameServer.AI
{
    /// <summary>
    /// AI被动类
    /// </summary>
    public class AIMonsterPassive : AIBase
    {
        public const string ID = "AIMonsterPassive";

        public AIMonsterPassive(Monster monster) : base(monster)
        {

        }
    }
}
