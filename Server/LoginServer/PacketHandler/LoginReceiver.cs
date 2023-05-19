using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer
{
    public class LoginReceiver
    {
        public static void Registers()
        {
            ProtoBufferManager.Instance.RegisterCallBack(GameProtocol.C_LOGIN, OnLogin);
            ProtoBufferManager.Instance.RegisterCallBack(GameProtocol.C_WORLD_SELECT, OnWorldSelect);
        }        

        private static void OnLogin(ProtoSession session, ArraySegment<byte> buffer)
        {
            var packet = ProtoBuffer.UnPack<CLoginInfo>(buffer);
            var user = session as LoginUserSession;

            ServerErrorCode errorCode;
            long accountID;

            if (true == packet.isSignUp)            
                errorCode = GameDB.Instance.AccountCreate(packet.id, packet.password, out accountID);            
            else            
                errorCode = GameDB.Instance.AccountLogin(packet.id, packet.password, out accountID);

            if (ServerErrorCode.SUCCESS != errorCode)
            {
                user.SendToErrorMessageBox(errorCode);
                return;
            }

        }

        private static void OnWorldSelect(ProtoSession session, ArraySegment<byte> buffer)
        {

        }
    }
}
