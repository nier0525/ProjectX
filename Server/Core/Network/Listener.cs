using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Listener
    {
        private Socket          _listenSocket;
        private Func<Session>   _sessionGenerator;

        private bool _isOpen = false;

        public void Open(IPEndPoint endPoint, Func<Session> sessionGenerator, bool isMultiService = false)
        {
            _listenSocket       = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionGenerator   = sessionGenerator;

            while (true)
            {
                try
                {
                    _listenSocket.Bind(endPoint);
                }
                catch (Exception exception)
                {
                    if (false == isMultiService)
                        throw new ArgumentException(exception.Message);

                    NetworkLogger.Write($"ReBind Port {endPoint.Port} -> {endPoint.Port + 1}");
                    ++endPoint.Port;
                    continue;
                }
                break;
            }
            
            _isOpen = true;
        }

        public void ReBind(IPEndPoint endPoint)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(endPoint);
        }

        public bool Listen(int registerCount = 1, int backLogCount = 100)
        {
            if (false == _isOpen)
                return false;

            _listenSocket.Listen(backLogCount);
            for (int i = 0; i < registerCount; ++i)
            {
                var args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(ProcessAccept);

                RegisterAccept(args);
            }
            return true;
        }

        private void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            var isPending = _listenSocket.AcceptAsync(args);
            if (false == isPending)
                ProcessAccept(null, args);
        }

        private void ProcessAccept(object sender, SocketAsyncEventArgs args)
        {
            try
            {
                if (SocketError.Success != args.SocketError)
                    throw new Exception(args.SocketError.ToString());

                var clientSocket = args.AcceptSocket;
                if (null == clientSocket)
                    throw new Exception("Invalid Socket");

                if (null == _sessionGenerator)
                    throw new Exception("Invalid Session Generator");

                var newSession = _sessionGenerator.Invoke();
                if (null == newSession)                                    
                    throw new Exception("Invalid Session");
                
                newSession.Open(clientSocket);
                newSession.OnAccepted(clientSocket.RemoteEndPoint);
            }
            catch (Exception exception)
            {
                NetworkLogger.Write(exception.Message);
            }
            finally
            {
                RegisterAccept(args);
            }
        }
    }
}
