using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer
{
    public class WorldServerSession : ProtoSession
    {
        public string Name { get; set; }

        public override void OnDisconnected(EndPoint endPoint)
        {
            WorldServerManager.Instance.DeleteSession(this);
        }
    }
}
