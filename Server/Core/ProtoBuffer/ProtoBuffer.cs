using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Core
{    
    public class ProtoBuffer
    {
        public static ArraySegment<byte> Serialize<T>(T packet) where T : class, new()
        {
            var serialize = new MemoryStream();
            ProtoBuf.Serializer.SerializeWithLengthPrefix(serialize, packet, ProtoBuf.PrefixStyle.Base128);
            var buffer = serialize.ToArray();

            return new ArraySegment<byte>(buffer, 0, buffer.Length);
        }

        public static T Parse<T>(byte[] buffer) where T : class, new()
        {
            var deserialize = new MemoryStream(buffer);
            return ProtoBuf.Serializer.DeserializeWithLengthPrefix<T>(deserialize, ProtoBuf.PrefixStyle.Base128);
        }

        public static ArraySegment<byte> Pack<E, T>(E protocol, T data) where T : class, new()
        {
            if (false == typeof(E).IsEnum)
                return null;

            var segment = SendBufferManager.Open();
            var buffer = new Span<byte>(segment.Array, segment.Offset, segment.Count);
            var isSuccess = true;

            ushort size = sizeof(ushort);

            isSuccess &= BitConverter.TryWriteBytes(buffer.Slice(size, buffer.Length - size), (ushort)(object)protocol);
            size += sizeof(ushort);

            if (null != data)
            {
                var protoBuffer = Serialize(data);
                Array.Copy(protoBuffer.Array, 0, segment.Array, size, protoBuffer.Count);
                size += (ushort)protoBuffer.Count;
            }
            
            isSuccess &= BitConverter.TryWriteBytes(buffer, size);

            if (false == isSuccess)
                return null;
            return SendBufferManager.Close(size);
        }

        public static ushort GetProtocol(ArraySegment<byte> buffer)
        {
            return BitConverter.ToUInt16(buffer.Array, sizeof(ushort));
        }

        public static T UnPack<T>(ArraySegment<byte> buffer) where T : class, new()
        {
            var size = BitConverter.ToUInt16(buffer.Array, 0);

            var data = new byte[size - Define.PACKETHEADER_SIZE];
            Array.Copy(buffer.Array, Define.PACKETHEADER_SIZE, data, 0, size - Define.PACKETHEADER_SIZE);

            return Parse<T>(data);
        }
    }

    public class ProtoBufferManager : Singleton<ProtoBufferManager>
    {
        private Dictionary<ushort, Action<ProtoSession, ArraySegment<byte>>> _callBacks = new Dictionary<ushort, Action<ProtoSession, ArraySegment<byte>>>();

        public bool RegisterCallBack<E>(E protocol, Action<ProtoSession, ArraySegment<byte>> callBack)
        {
            if (false == typeof(E).IsEnum)
                return false;

            if (true == _callBacks.ContainsKey((ushort)(object)protocol))
                return false;

            _callBacks[(ushort)(object)protocol] = callBack;
            return true;
        }

        public void OnReceiveProtoBuffer(ProtoSession session, ArraySegment<byte> buffer)
        {
            var protocol = ProtoBuffer.GetProtocol(buffer);
            Action<ProtoSession, ArraySegment<byte>> callBack;

            if (true == _callBacks.TryGetValue(protocol, out callBack))
                callBack(session, buffer);
            else
                session.Disconnect($"Not Registered Packet Info.. {protocol}");
        }
    }
}
