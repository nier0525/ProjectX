using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core
{
    public class Empty {}

    public abstract class Session
    {
        protected Socket _socket;
        protected int _isDisconnected = Define.EMPTY;

        private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        private List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        private Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();

        private SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();
        private ReceiveBuffer _receiveBuffer = new ReceiveBuffer(Define.BUFFER_SIZE);

        public void Open(Socket socket)
        {
            _socket = socket;

            _sendArgs.Completed     += new EventHandler<SocketAsyncEventArgs>(ProcessSend);
            _receiveArgs.Completed  += new EventHandler<SocketAsyncEventArgs>(ProcessReceive);

            RegisterReceive();
        }

        private void Close()
        {
            OnDisconnected(_socket.RemoteEndPoint);

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();

            Clear();
        }

        private void Clear()
        {            
            lock (_sendQueue)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Send<E>(E protocol)
        {
            if (false == typeof(E).IsEnum)
                return;

            var     segment = SendBufferManager.Open();
            ushort  size    = sizeof(ushort);

            Array.Copy(BitConverter.GetBytes((ushort)(object)protocol), 0, segment.Array, size, sizeof(ushort));
            size += sizeof(short);
            
            Array.Copy(BitConverter.GetBytes(size), 0, segment.Array, 0, sizeof(ushort));
            
            Send(SendBufferManager.Close(size));
        }

        public void Send(ArraySegment<byte> buffer)
        {
            lock (_sendQueue)
            {
                _sendQueue.Enqueue(buffer);
                if (0 == _pendingList.Count)
                    RegisterSend();
            }
        }

        public void Send(List<ArraySegment<byte>> buffers)
        {
            if (0 == buffers.Count)
                return;

            lock (_sendQueue)
            {
                foreach (var buffer in buffers)
                    _sendQueue.Enqueue(buffer);

                if (0 == _pendingList.Count)
                    RegisterSend();
            }
        }

        public void Disconnect(string reason = null)
        {
            if (Define.USED == Interlocked.Exchange(ref _isDisconnected, Define.USED))
                return;

            if (null != reason)
                NetworkLogger.Write($"[Disconnect] {reason}");

            Close();
        }


        #region Network
        public virtual void OnAccepted(EndPoint endPoint) { }
        public virtual void OnConnected(EndPoint endPoint) { }
        public virtual void OnDisconnected(EndPoint endPoint) { }
        public virtual void OnSend(int bytes) { }
        public abstract int OnReceive(ArraySegment<byte> buffer);

        private void RegisterSend()
        {
            if (Define.USED == _isDisconnected)
                return;

            while (0 != _sendQueue.Count)
                _pendingList.Add(_sendQueue.Dequeue());

            _sendArgs.BufferList = _pendingList;

            try
            {
                var isPending = _socket.SendAsync(_sendArgs);
                if (false == isPending)
                    ProcessSend(null, _sendArgs);
            }
            catch (Exception exception)
            {
                NetworkLogger.Write($"Send Failed.. {exception.Message}");
            }
        }

        private void ProcessSend(object sender, SocketAsyncEventArgs args)
        {
            lock (_sendQueue)
            {
                if (0 < args.BytesTransferred && SocketError.Success == args.SocketError)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();

                        OnSend(args.BytesTransferred);

                        if (0 < _sendQueue.Count)
                            RegisterSend();
                    }
                    catch (Exception exception)
                    {
                        Disconnect($"Send Failed.. {exception.Message}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        private void RegisterReceive()
        {
            if (Define.USED == _isDisconnected)
                return;

            _receiveBuffer.Clear();
            var segment = _receiveBuffer.Write;

            _receiveArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
            try
            {
                var isPending = _socket.ReceiveAsync(_receiveArgs);
                if (false == isPending)
                    ProcessReceive(null, _receiveArgs);
            }
            catch (Exception exception)
            {
                NetworkLogger.Write($"Receive Failed.. {exception.Message}");
            }
        }

        private void ProcessReceive(object sender, SocketAsyncEventArgs args)
        {
            if (0 < args.BytesTransferred && SocketError.Success == args.SocketError)
            {
                try
                {
                    if (false == _receiveBuffer.OnWrite(args.BytesTransferred))
                        throw new Exception("Overflow");

                    var processLength = OnReceive(_receiveBuffer.Read);
                    if (0 > processLength || _receiveBuffer.UseSize < processLength)
                        throw new Exception("Invalid Receive Data");

                    if (false == _receiveBuffer.OnRead(processLength))
                        throw new Exception("Underflow");

                    RegisterReceive();
                }
                catch (Exception exception)
                {
                    Disconnect($"Receive Failed.. {exception.Message}");
                }
            }
            else
            {
                Disconnect();
            }
        }
        #endregion
    }

    public abstract class PacketSession : Session
    {
        public sealed override int OnReceive(ArraySegment<byte> buffer)
        {
            var processLength = 0;
            while (true)
            {
                if (Define.PACKETHEADER_SIZE > buffer.Count)
                    break;

                var size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < size)
                    break;

                OnReceivePacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, size));

                processLength += size;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + size, buffer.Count - size);
            }
            return processLength;
        }

        public abstract void OnReceivePacket(ArraySegment<byte> buffer);
    }

    public abstract class ProtoSession : PacketSession
    {
        public override void OnReceivePacket(ArraySegment<byte> buffer)
        {
            ProtoBufferManager.Instance.OnReceiveProtoBuffer(this, buffer);
        }

        public void Send<E, T>(E protocol, T packet) where T : class, new()
        {
            if (false == typeof(E).IsEnum)
                return;

            Send(ProtoBuffer.Pack(protocol, packet));
        }

        public void Send<E, T>(E protocol, List<T> packets) where T : class, new()
        {
            if (false == typeof(E).IsEnum)
                return;

            List<ArraySegment<byte>> buffers = new List<ArraySegment<byte>>();
            foreach (var packet in packets)
                buffers.Add(ProtoBuffer.Pack(protocol, packet));

            Send(buffers);
        }
    }
}
