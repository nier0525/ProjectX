using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ReceiveBuffer
    {
        private ArraySegment<byte> _buffer;
        private int _readOffset;
        private int _writeOffset;

        public ReceiveBuffer(int capacity)
        {
            _buffer = new ArraySegment<byte>(new byte[capacity], 0, capacity);
            _readOffset = 0;
            _writeOffset = 0;
        }

        public int UseSize { get { return _writeOffset - _readOffset; } }
        public int FreeSize { get { return _buffer.Count - _writeOffset; } }

        public ArraySegment<byte> Read { get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readOffset, UseSize); } }
        public ArraySegment<byte> Write { get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writeOffset, FreeSize); } }

        public bool OnRead(int bytes)
        {
            if (bytes > UseSize)
                return false;

            _readOffset += bytes;
            return true;
        }

        public bool OnWrite(int bytes)
        {
            if (bytes > FreeSize)
                return false;

            _writeOffset += bytes;
            return true;
        }

        public void Clear()
        {
            var useSize = UseSize;
            if (0 == useSize)
            {
                _readOffset = 0;
                _writeOffset = 0;
            }
            else
            {
                Array.Copy(_buffer.Array, _buffer.Offset + _readOffset, _buffer.Array, _buffer.Offset, useSize);
                _readOffset = 0;
                _writeOffset = useSize;
            }
        }
    }
}
