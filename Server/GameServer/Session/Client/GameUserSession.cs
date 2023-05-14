using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public class GameUserSession : ProtoSession
    {
        public long Ping;
        public long AccountID;

        public override void OnAccepted(EndPoint endPoint)
        {
            var address = ((IPEndPoint)endPoint).Address.MapToIPv4();
            NetworkLogger.Write($"[{address}] Connected User");

            if (true == GameServer.Config.UseKeepAlive)
                JobTimer.Push(() => { SendToHeartBeat(); }, GameServer.Config.KeepAliveIntervalTick);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            var address = ((IPEndPoint)endPoint).Address.MapToIPv4();
            NetworkLogger.Write($"[{address}] Disconnected User");

            GameUserManager.Instance.PushJob(() => { GameUserManager.Instance.RemoveSession(this); });
        }

        public void SendToHeartBeat()
        {
            Send(GameProtocol.S_HEART_BEAT, new HeartBeat() { ping = Environment.TickCount64 });
        }

        public void SendToErrorMessageBox(ServerErrorCode errorCode, bool isQuit)
        {
            Send(GameProtocol.S_ERROR_CODE, new ErrorCode()
            {
                errorCode = errorCode,
                UIType = ErrorUIType.MessageBox,
                isQuit = isQuit
            });
        }

        public void SendToErrorToast(ServerErrorCode errorCode, bool isQuit)
        {
            Send(GameProtocol.S_ERROR_CODE, new ErrorCode()
            {
                errorCode = errorCode,
                UIType = ErrorUIType.Toast,
                isQuit = isQuit
            });
        }
    }
}
