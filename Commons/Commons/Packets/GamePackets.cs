using ProtoBuf;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

[ProtoContract]
public class HeartBeat
{
    [ProtoMember(1)] public long ping { get; set; }
}

[ProtoContract]
public class SErrorCode
{
    [ProtoMember(1)] public ServerErrorCode errorCode { get; set; }
    [ProtoMember(2)] public ErrorUIType UIType { get; set; }
    [ProtoMember(3)] public bool isQuit { get; set; }
}

[ProtoContract]
public class CLoginInfo
{
    [ProtoMember(1)] public string id { get; set; }
    [ProtoMember(2)] public string password { get; set; }
    [ProtoMember(3)] public bool isSignUp { get; set; }
}

[ProtoContract]
public class SWorldServerInfo
{
    [ProtoMember(1)] public string worldName { get; set; }
    [ProtoMember(2)] public string hostName { get; set; }
    [ProtoMember(3)] public int portNumber { get; set; }
}

[ProtoContract]
public class SWorldServerList
{
    [ProtoMember(1)] public List<SWorldServerInfo> worldServers { get; set; }
}

[ProtoContract]
public class SCharacterListInfo
{
    [ProtoMember(1)] public long index { get; set; }
    [ProtoMember(2)] public string name { get; set; }
    [ProtoMember(3)] public int level { get; set; }
}

[ProtoContract]
public class SAccountInfo
{
    [ProtoMember(1)] public long accountID { get; set; }
    [ProtoMember(2)] public List<SCharacterListInfo> characterList { get; set; }
}
