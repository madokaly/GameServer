using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class CommandHelper
    {
        public static void Run()
        {
            bool run = true;
            while (run)
            {
                Console.Write(">");
                string line = Console.ReadLine().ToLower().Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    Help();
                    continue;
                }
                try
                {
                    string[] cmd = line.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    switch (cmd[0])
                    {
                        case "addexp":
                            AddExp(int.Parse(cmd[1]), int.Parse(cmd[2]));
                            break;
                        case "exit":
                            run = false;
                            break;
                        default:
                            Help();
                            break;
                    }
                }catch(Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
            }
        }
        /// <summary>
        /// 对在线玩家添加经验
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="exp"></param>
        public static void AddExp(int characterId, int exp = 1000)
        {
            var cha = Managers.CharacterManager.Instance.GetCharacter(characterId);
            if(cha == null)
            {
                Console.WriteLine("characterId:{0} not found", characterId);
                return;
            }
            Console.WriteLine("characterId:{0} addExp :{1}", characterId, exp);
            cha.AddExp(exp);
        }

        public static void Help()
        {
            Console.Write(@"
Help:
    exit    Exit Game Server
    help    Show Help
");
        }
    }
}
