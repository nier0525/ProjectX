using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient
{
    public class DummyClient
    {
        public static Connector LobbyServerConnector = new Connector();
        public static IPEndPoint LobbyServerAddress = null;
        
        public static void ConnectLobbyServer(int connectCount = 1)
        {
            LobbyServerConnector.Connect(LobbyServerAddress, connectCount);
        }

        private static void Main(string[] args)
        {
            NetworkLogger.Open(NetworkLogger.ConsoleLog, false);

            {
                ClientToLobby.Initialize();

                var hostName = Dns.GetHostEntry(Dns.GetHostName());
                LobbyServerAddress = new IPEndPoint(hostName.AddressList[0], 39001);

                //LobbyServerConnector.Open(() => { return LobbyDummyUserManager.Instance.NewSession(); });
            }            

            ConnectLobbyServer(2000);

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
