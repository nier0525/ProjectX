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
    public class ServerInfo
    { 
        public string LoginHostName { get; set; }
        public int LoginPortNumber { get; set; }
        public string WorldHostName { get; set; }
        public int WorldPortNumber { get; set; }
    }

    public class LoginServer
    {
        public static Listener      ClientListener;
        public static Listener      WorldListener;
        public static ServerInfo    Config;


        public static void Main(string[] args)
        {
            NetworkLogger.Open(NetworkLogger.ConsoleLog, false);
            Config = JsonHelper.ReadFrom<ServerInfo>("Jsons\\LoginServer.json");

            GameDB.Instance.Initialize();

            {   // Client Listen
                LoginReceiver.Registers();

                var hostName = Dns.GetHostEntry(Config.LoginHostName);
                var endPonit = new IPEndPoint(hostName.AddressList[0], Config.LoginPortNumber);

                ClientListener = new Listener();
                ClientListener.Open(endPonit, () => { return new LoginUserSession(); });
                ClientListener.Listen(5);
            }

            {   // World Listen
                WorldServerReceiver.Registers();

                var hostName = Dns.GetHostEntry(Config.WorldHostName);
                var endPoint = new IPEndPoint(hostName.AddressList[0], Config.WorldPortNumber);

                WorldListener = new Listener();
                WorldListener.Open(endPoint, () => { return WorldServerManager.Instance.NewSession(); });
                WorldListener.Listen();
            }

            Console.Title = $"Login Server [{Config.LoginHostName}:{Config.LoginPortNumber}]";
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
