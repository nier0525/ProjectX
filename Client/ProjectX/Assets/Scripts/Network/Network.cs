using Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Network : Singleton<Network>
{
    public string HostName;
    public int PortNumber;
    private Connector connector;

    [HideInInspector] public GameSession Session;
    [HideInInspector] public int Ping;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        NetworkLogger.Open(Debug.Log, false);
        connector = new Connector();

        PacketUtil.RegisterCallback(GameProtocol.S_HEART_BEAT, OnHeartBeat);
        PacketUtil.RegisterCallback(GameProtocol.S_ERROR_CODE, OnErrorCode);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Connect(HostName, PortNumber);
    }

    private void Update()
    {
        NetworkLogger.Flush_Short();        
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    private void OnHeartBeat(PacketSession session, ArraySegment<byte> buffer)
    {
        FetchPing();
        Send(GameProtocol.C_HEART_BEAT, PacketUtil.UnPack<HeartBeat>(buffer));
    }

    private void OnErrorCode(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = PacketUtil.UnPack<ErrorCode>(buffer);
        switch (packet.UIType)
        {
            case ErrorUIType.MessageBox:
                UIManager.Instance.ShowMessageBox($"{packet.errorCode}", () => { if (true == packet.isQuit) CommonUtils.ForceDestroy(); });
                break;

            case ErrorUIType.Toast:
                UIManager.Instance.ShowToast($"{packet.errorCode}", packet.isQuit);
                break;
        }
    }

    public int FetchPing()
    {
        var ping = 0 == Ping ? 0 : (Environment.TickCount - Ping) - 1000;
        Ping = Environment.TickCount;

        return ping;
    }

    public void Connect(string hostName, int portNumber)
    {
        var entry = Dns.GetHostEntry(hostName);
        var endPoint = new IPEndPoint(entry.AddressList[0], portNumber);

        Session = new GameSession(null, null);
        connector.Open(() => { return Session; });

        if (false == connector.DirectConnect(endPoint))
            UIManager.Instance.ShowMessageBox("Connect Failed.. Closed Server", () => { CommonUtils.ForceDestroy(); });
    }

    public void Disconnect(string reason = null)
    {
        if (null == Session)
            return;

        if (false == Session.IsConnected())
            return;

        if (null != reason)
            Debug.Log(reason);

        Session.Disconnect();
    }

    public void Send(GameProtocol protocol)
    {
        if (null != Session)
            Session.Send(protocol);
    }

    public void Send<T>(GameProtocol protocol, T packet) where T : class, new()
    {
        if (null != Session)
            Session.Send(protocol, packet);
    }
}
