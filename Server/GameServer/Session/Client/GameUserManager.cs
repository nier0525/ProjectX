using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class GameUserManager : Singleton<GameUserManager>
    {
        private JobQueue jobQueue = new JobQueue();
        private List<GameUserSession> sessions = new List<GameUserSession>();

        public void PushJob(Action job) { jobQueue.Push(job); }

        public GameUserSession NewSession()
        {
            var session = new GameUserSession();
            PushJob(() => { AddSession(session); });
            return session;
        }

        public void AddSession(GameUserSession session)
        {
            if (true == sessions.Contains(session))
                session.Disconnect("Duplicate Session..");
            else
                sessions.Add(session);
        }

        public void RemoveSession(GameUserSession session)
        {            
            if (true == sessions.Contains(session))
                sessions.Remove(session);
        }

        public void Broadcast<T>(GameProtocol protocol, T packet) where T : class, new()
        {
            foreach (var session in sessions)
                session.Send(protocol, packet);
        }
    }
}