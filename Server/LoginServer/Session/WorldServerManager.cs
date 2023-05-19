using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer
{
    public class WorldServerManager : Singleton<WorldServerManager>
    {
        private List<WorldServerSession> worldServers = new List<WorldServerSession>();
        
        public WorldServerSession NewSession()
        {
            var session = new WorldServerSession();
            lock (worldServers)
                worldServers.Add(session);
            return session;
        }

        public void DeleteSession(WorldServerSession server)
        {
            lock (worldServers)
                worldServers.Remove(server);
        }

        public WorldServerSession FindWorldServer(string name)
        {
            lock (worldServers)
            {
                foreach (var server in worldServers)
                {

                }
                return null;
            }
        }
    }
}
