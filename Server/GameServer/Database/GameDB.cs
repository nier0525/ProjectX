using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class DBConfig
    {
        public string HostName { get; set; }
        public int PortNumber { get; set; }
        public string Database { get; set; }
        public string ID { get; set; }
        public string Passward { get; set; }
    }

    public class GameDB : Singleton<GameDB>
    {
        private const int CONNECTION_POOL_COUNT = 10;
        private MSSQLManager SQLManager = new MSSQLManager();

        public void Initialize()
        {
            var config = JsonHelper.ReadFrom<DBConfig>("Jsons\\GameDB.json");
            SQLManager.Initialize(config.HostName, config.Database, config.ID, config.Passward, CONNECTION_POOL_COUNT);
        }

        public MSSQLConnection GetDB()
        {
            return SQLManager.GetDB();
        }       
    }
}
