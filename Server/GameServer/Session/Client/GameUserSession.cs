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
    public enum GMLevel : byte
    {
        NORMAL,     // 일반 유저
        GUEST,      // 게스트
        GM,         // 운영자
        DEVELOPER   // 개발팀
    }

    public enum UserState : byte 
    {
        ACCEPTED,
        DISCONNECTED,
        LOBBY,
        INGAME,
    }

    public class GameUserSession : ProtoSession
    {
        public long         Ping;
        public UserState    UserState;
        public long         AccountID;
        public GMLevel      GMLevel;
        public int          Cash;

        public override void OnAccepted(EndPoint endPoint)
        {
            UserState = UserState.ACCEPTED;

            var address = ((IPEndPoint)endPoint).Address.MapToIPv4();
            NetworkLogger.Write($"[{address}] Connected User");

            if (true == GameServer.Config.UseKeepAlive)
                JobTimer.Push(() => { SendToHeartBeat(); }, GameServer.Config.KeepAliveIntervalTick);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            UserState = UserState.DISCONNECTED;

            var address = ((IPEndPoint)endPoint).Address.MapToIPv4();
            NetworkLogger.Write($"[{address}] Disconnected User");

            GameUserManager.Instance.PushJob(() => { GameUserManager.Instance.RemoveSession(this); });
        }

        public void SendToHeartBeat()
        {
            Send(GameProtocol.S_HEART_BEAT, new HeartBeat() { ping = Environment.TickCount64 });
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

        public ServerErrorCode OnReceiveLogin(CLoginInfo packet)
        {
            int result;
            if (true == packet.isSignUp)
            {
                result = GameDBToAccount.AccountInsert(this, packet.id, packet.password);
                if (-1 == result)
                    return ServerErrorCode.ALREADY_USED_ACCOUNT_ID;                
            }
            else
            {
                result = GameDBToAccount.AccountSelect(this, packet.id, packet.password);
                if (-1 == result)
                    return ServerErrorCode.NOT_FOUND_ACCOUNT_INFO;
            }

            if (ProcedureResult.SUCCESS != result)
                return ServerErrorCode.DB_ERROR;

            var characterList = GameDBToCharacter.GetCharacterListInfo(this);
            if (null == characterList)
                return ServerErrorCode.DB_ERROR;
            
            Send(GameProtocol.S_ACCOUNT_INFO, new SAccountInfo() { accountID = AccountID, characterList = characterList });

            UserState = UserState.LOBBY;
            return ServerErrorCode.SUCCESS;
        }
    }
}
