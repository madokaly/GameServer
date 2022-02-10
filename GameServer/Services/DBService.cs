using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;

namespace GameServer.Services
{
    /// <summary>
    /// DB服务类
    /// </summary>
    class DBService : Singleton<DBService>
    {
        ExtremeWorldEntities entities;



        public ExtremeWorldEntities Entities
        {
            get { return this.entities; }
        }

        public void Init()
        {
            entities = new ExtremeWorldEntities();

           
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="async"></param>
        public void Save(bool async = false)
        {
            if (async)
                entities.SaveChangesAsync();
            else
                entities.SaveChanges();
        }
    }
}
