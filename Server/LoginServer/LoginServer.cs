using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer
{
    public class ServerInfo
    { 
        public string   HostName { get; set; }
        public int      PortNumber { get; set; }

        public string   WorldHostName { get; set; }
        public int      WorldPortNumber { get; set; }
    }

    public class LoginServer
    {
        public static Listener      ClientListener;
        public static ServerInfo    Config;

        public static void Main(string[] args)
        {
            NetworkLogger.Open(NetworkLogger.ConsoleLog, false);
            Config = JsonHelper.ReadFrom<ServerInfo>("Jsons\\LoginServer.json");

            {   // Client Listen

                var hostName = Dns.GetHostEntry(Config.HostName);
                var endPonit = new IPEndPoint(hostName.AddressList[0], Config.PortNumber);

                ClientListener = new Listener();
                ClientListener.Open(endPonit, () => { return null; });
                ClientListener.Listen(5);
            }

            
        }
    }
}
