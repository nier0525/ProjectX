using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core
{
    public class SendBuffer
    {
        private byte[] _buffer;
        int usedSize = 0;

        public SendBuffer(int capacity)
        {
            _buffer = new byte[capacity];
        }

        public int FreeSize { get { return _buffer.Length - usedSize; } }

        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > FreeSize)
                return null;
            return new ArraySegment<byte>(_buffer, usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSIze)
        {
            var segment = new ArraySegment<byte>(_buffer, usedSize, usedSIze);
            usedSize += usedSize;

            return segment;
        }
    }

    public class SendBufferManager
    {
        public static ThreadLocal<SendBuffer> currentSendBuffer = new ThreadLocal<SendBuffer>(() => { return null; });

        public static int ChunkSize { get; set; } = Define.BUFFER_SIZE * 10;

        public static ArraySegment<byte> Open(int reserveSize = Define.BUFFER_SIZE)
        {
            if (null == currentSendBuffer.Value)
                currentSendBuffer.Value = new SendBuffer(ChunkSize);

            if (currentSendBuffer.Value.FreeSize < reserveSize)
                currentSendBuffer.Value = new SendBuffer(ChunkSize);

            var segment = currentSendBuffer.Value.Open(reserveSize);
            if (null == segment)
                NetworkLogger.Write("Overflow");

            return segment;
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return currentSendBuffer.Value.Close(usedSize);
        }
    }
}
