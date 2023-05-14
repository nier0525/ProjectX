using Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class GameSession : PacketSession
{
    public Action OnConnectedTask       = null;
    public Action OnDisconnectedTask    = null;

    public long AccountID;

    public GameSession(Action onConnectedTask, Action onDisconnectedTask)
    {
        OnConnectedTask     = onConnectedTask;
        OnDisconnectedTask  = onDisconnectedTask;
    }

    public override void OnConnected(EndPoint endPoint)
    {
        if (null != OnConnectedTask)
            OnConnectedTask.Invoke();
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        if (null != OnDisconnectedTask)
            OnDisconnectedTask.Invoke();
    }

    public override void OnReceivePacket(ArraySegment<byte> buffer)
    {
        PacketUtil.OnReceivePacket(this, buffer);
    }

    public void Send<T>(GameProtocol protocol, T packet) where T : class, new()
    {
        Send(PacketUtil.Pack(protocol, packet));
    }

    public bool IsConnected()
    {
        if (null == _socket)
            return false;
        return _socket.Connected;
    }
}
