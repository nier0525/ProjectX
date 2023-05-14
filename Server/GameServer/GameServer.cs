using System;
using System.Threading;
using Core;
using System.Net;
using GameServer.PacketHandler;

namespace GameServer
{
    public class ServerInfo
    {
        public string   GameHostName { get; set; }
        public int      GamePortNumber { get; set; }
        public bool     UseKeepAlive { get; set; }
        public int      KeepAliveIntervalTick { get; set; }
    }

    public class GameServer
    {
        public enum ServerType { GAME, MAX }

        public static Listener      ClientListener = new Listener();
        public static ServerInfo    Config;        
      
        private static void Main(string[] args)
        {
            NetworkLogger.Open(NetworkLogger.ConsoleLog, false);
            Config = JsonHelper.ReadFrom<ServerInfo>("Jsons\\GameServer.json");

            {   // Client Listen Open
                GameReceiver.Registers();

                var hostName = Dns.GetHostEntry(Config.GameHostName);
                var endPoint = new IPEndPoint(hostName.AddressList[0], Config.GamePortNumber);

                ClientListener.Open(endPoint, () => { return GameUserManager.Instance.NewSession(); });
                ClientListener.Listen(5);
            }

            GameDB.Instance.Initialize();

            Console.Title = $"Game Server [{Config.GameHostName}:{Config.GamePortNumber}]";
            NetworkLogger.Write("Game Server Open");

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
