using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SQLServer
{
    public class Config
    {
        public string   HostName { get; set; }
        public int      PortNumber { get; set; }
        public string   Database { get; set; }
        public string   ID { get; set; }
        public string   Password { get; set; }
    }

    private MSSQLManager SQLManager = null;

    public void Initialize(string path, int poolCount = 10)
    {
        var config = JsonHelper.ReadFrom<Config>(path);
        
        SQLManager = new MSSQLManager();
        SQLManager.Initialize(config.HostName, config.Database, config.ID, config.Password, poolCount);
    }

    public MSSQLConnection Get()
    {
        return SQLManager.GetDB();
    }
}
