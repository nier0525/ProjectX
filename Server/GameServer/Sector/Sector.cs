using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class Sector
    {
        private List<GameUserSession> sessions = new List<GameUserSession>();

        public void Enter(GameUserSession session)
        {
            sessions.Add(session);
        }

        public void Leave(GameUserSession session)
        {
            sessions.Remove(session);
        }

        public void Broadcast<T>(GameProtocol protocol, T packet) where T : class, new()
        {
            foreach (var session in sessions)
                session.Send(protocol, packet);
        }
    }
}
