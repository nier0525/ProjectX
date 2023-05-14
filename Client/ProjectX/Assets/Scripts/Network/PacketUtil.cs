using Core;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class PacketUtil
{
    /* Header : [Size(2)][Protocol[2] */
    private static Dictionary<GameProtocol, Action<PacketSession, ArraySegment<byte>>> callbacks = new Dictionary<GameProtocol, Action<PacketSession, ArraySegment<byte>>>();

    public static ArraySegment<byte> Serialize<T>(T packet) where T : class, new()
    {
        var stream = new MemoryStream();
        Serializer.SerializeWithLengthPrefix(stream, packet, PrefixStyle.Base128);

        var buffer = stream.ToArray();
        return new ArraySegment<byte>(buffer, 0, buffer.Length);
    }

    public static T Parse<T>(byte[] buffer) where T : class, new()
    {
        var stream = new MemoryStream(buffer);
        return Serializer.DeserializeWithLengthPrefix<T>(stream, PrefixStyle.Base128);
    }

    public static ushort GetProtocol(ArraySegment<byte> buffer)
    {
        return BitConverter.ToUInt16(buffer.Array, sizeof(ushort));
    }

    public static ArraySegment<byte> Pack<T>(GameProtocol protocol, T data) where T : class, new()
    {
        var segment = new byte[Define.BUFFER_SIZE];
        ushort size = sizeof(ushort);

        Array.Copy(BitConverter.GetBytes((ushort)protocol), 0, segment, size, sizeof(ushort));
        size += sizeof(ushort);

        if (null != data)
        {
            var serialize = Serialize(data);
            Array.Copy(serialize.Array, 0, segment, size, serialize.Count);
            size += (ushort)serialize.Count;
        }

        Array.Copy(BitConverter.GetBytes(size), 0, segment, 0, sizeof(ushort));
        return new ArraySegment<byte>(segment, 0, size);
    }

    public static T UnPack<T>(ArraySegment<byte> buffer) where T : class, new()
    {
        var size = BitConverter.ToUInt16(buffer.Array, 0);
        var data = new byte[size - Define.PACKETHEADER_SIZE];

        Array.Copy(buffer.Array, Define.PACKETHEADER_SIZE, data, 0, size - Define.PACKETHEADER_SIZE);
        return Parse<T>(data);
    }

    public static void RegisterCallback(GameProtocol protocol, Action<PacketSession, ArraySegment<byte>> callback)
    {
        if (false == callbacks.ContainsKey(protocol))
            callbacks[protocol] = callback;
        else
            Debug.LogWarning($"Already Registered Receive CallBack.. {protocol}");
    }

    public static void OnReceivePacket(PacketSession session, ArraySegment<byte> buffer)
    {
        var protocol = GetProtocol(buffer);
        Action<PacketSession, ArraySegment<byte>> callback;

        if (true == callbacks.TryGetValue((GameProtocol)protocol, out callback))
            ReceiveQueue.Instance.Enqueue(callback, session, buffer);
        else
            Debug.LogWarning($"Not Found Registered Callback.. {protocol}");
    }
}
