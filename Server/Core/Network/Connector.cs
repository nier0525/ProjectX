using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Connector
    {
        private Func<Session>   _sessionGenerator = null;
        private bool            _useReConnect = false;

        public void Open(Func<Session> sessionGenerator, bool useReConnect = false)
        {
            _sessionGenerator = sessionGenerator;
            _useReConnect = useReConnect;
        }

        public void Connect(IPEndPoint endPoint, int connectCount = 1)
        {
            if (null == _sessionGenerator)
                return;

            for (int i = 0; i < connectCount; ++i)
            {
                var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                var args = new SocketAsyncEventArgs();

                args.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessConnect);
                args.RemoteEndPoint = endPoint;
                args.UserToken = socket;

                RegisterConnect(args);
            }
        }

        public bool DirectConnect(IPEndPoint endPoint)
        {
            if (null == _sessionGenerator)
                return false;

            try
            {
                var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(endPoint);

                var newSession = _sessionGenerator.Invoke();
                newSession.Open(socket);
                newSession.OnConnected(endPoint);

                return true;
            }
            catch (SocketException exception)
            {
                NetworkLogger.Write(exception.ToString());
                return false;
            }
        }

        private void RegisterConnect(SocketAsyncEventArgs args)
        {
            var socket = args.UserToken as Socket;
            if (null == socket)
                return;

            var isPending = socket.ConnectAsync(args);
            if (false == isPending)
                ProcessConnect(null, args);
        }

        private void ProcessConnect(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                if (SocketError.Success != args.SocketError)
                    throw new Exception(args.SocketError.ToString());

                var newSession = _sessionGenerator.Invoke();
                if (null == newSession)
                    throw new ArgumentException("Invalid Session!! Please Declare Session");

                newSession.Open(args.ConnectSocket);
                newSession.OnConnected(args.RemoteEndPoint);

                args = null;
            }
            catch (Exception exception)
            {
                NetworkLogger.Write($"Connect Failed.. {exception.Message}");

                if (true == _useReConnect)                
                    JobTimer.Push(() => { RegisterConnect(args); }, 5000);
            }
        }
    }
}
