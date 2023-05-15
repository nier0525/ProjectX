using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GameServer.PacketHandler
{
    public class GameReceiver
    {
        public static void Registers()
        {
            ProtoBufferManager.Instance.RegisterCallBack(GameProtocol.C_HEART_BEAT, OnHeartBeat);
            ProtoBufferManager.Instance.RegisterCallBack(GameProtocol.C_LOGIN, OnLogin);
        }

        private static void OnHeartBeat(ProtoSession session, ArraySegment<byte> buffer)
        {
            var packet  = ProtoBuffer.UnPack<HeartBeat>(buffer);
            var user    = session as GameUserSession;

            user.Ping = Environment.TickCount64 - packet.ping;
            JobTimer.Push(() => { user.SendToHeartBeat(); }, GameServer.Config.KeepAliveIntervalTick);
        }

        private static void OnLogin(ProtoSession session, ArraySegment<byte> buffer)
        {
            var packet = ProtoBuffer.UnPack<CLoginInfo>(buffer);
            var user = session as GameUserSession;

            var errorCode = user.OnReceiveLogin(packet);
            if (ServerErrorCode.SUCCESS != errorCode)
                user.SendToErrorMessageBox(errorCode);
            else
                user.SendToAccountInfo();
        }
    }
}
