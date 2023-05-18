using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoginServer
{
    public class WorldServer
    {
        public string   WorldName { get; set; }
        public string   HostName { get; set; }
        public int      PortNumber { get; set; }
    }

    public class ServerInfo
    { 
        public string   HostName { get; set; }
        public int      PortNumber { get; set; }

        public List<WorldServer> WorldServers { get; set; }
    }

    public class LoginServer
    {
        public static Listener      ClientListener;
        public static ServerInfo    Config;
        
        public static SWorldServerList MakePacketToWorldServerList()
        {
            var packet = new SWorldServerList() { worldServers = new List<SWorldServerInfo>() };
            foreach (var info in Config.WorldServers)
            {
                var world = new SWorldServerInfo()
                {
                    worldName = info.WorldName,
                    hostName = info.HostName,
                    portNumber = info.PortNumber
                };
                packet.worldServers.Add(world);
            }
            return packet;
        }

        public static void Main(string[] args)
        {
            NetworkLogger.Open(NetworkLogger.ConsoleLog, false);
            Config = JsonHelper.ReadFrom<ServerInfo>("Jsons\\LoginServer.json");

            GameDB.Instance.Initialize();

            {   // Client Listen

                var hostName = Dns.GetHostEntry(Config.HostName);
                var endPonit = new IPEndPoint(hostName.AddressList[0], Config.PortNumber);

                ClientListener = new Listener();
                ClientListener.Open(endPonit, () => { return new LoginUserSession(); });
                ClientListener.Listen(5);
            }

            Console.Title = $"Login Server [{Config.HostName}:{Config.PortNumber}]";
            NetworkLogger.Write("Login Server Open");

            while (true)
            {
                JobTimer.Flush();
                GlobalJobQueue.Instance.Flush();
                NetworkLogger.Flush();

                Thread.Yield();
            }
        }
    }
}
