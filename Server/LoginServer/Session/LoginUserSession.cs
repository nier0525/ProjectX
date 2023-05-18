using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer
{
    public class LoginUserSession : ProtoSession
    {
        public override void OnAccepted(EndPoint endPoint)
        {
            var address = ((IPEndPoint)endPoint).Address.MapToIPv4();
            NetworkLogger.Write($"[{address}] Connected");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            var address = ((IPEndPoint)endPoint).Address.MapToIPv4();
            NetworkLogger.Write($"[{address}] Disconnected");
        }

        public void SendToErrorMessageBox(ServerErrorCode errorCode, bool isQuit = false)
        {
            Send(GameProtocol.S_ERROR_CODE, new SErrorCode()
            {
                errorCode = errorCode,
                UIType = ErrorUIType.MessageBox,
                isQuit = isQuit
            });
        }

        public void SendToErrorToast(ServerErrorCode errorCode, bool isQuit = false)
        {
            Send(GameProtocol.S_ERROR_CODE, new SErrorCode()
            {
                errorCode = errorCode,
                UIType = ErrorUIType.Toast,
                isQuit = isQuit
            });
        }
    }
}
