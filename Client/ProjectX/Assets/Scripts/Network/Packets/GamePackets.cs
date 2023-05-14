﻿using ProtoBuf;
using System.Collections.Generic;

[ProtoContract]
public class HeartBeat
{
    [ProtoMember(1)] public long ping { get; set; }
}

[ProtoContract]
public class ErrorCode
{
    [ProtoMember(1)] public ServerErrorCode errorCode { get; set; }
    [ProtoMember(2)] public ErrorUIType UIType { get; set; }
    [ProtoMember(3)] public bool isQuit { get; set; }
}

[ProtoContract]
public class LoginInfo
{
    [ProtoMember(1)] public string  id { get; set; }
    [ProtoMember(2)] public string  password { get; set; }
    [ProtoMember(3)] public bool    isSignUp { get; set; }
}

[ProtoContract]
public class AccountInfo
{
    [ProtoMember(1)] public long accountID { get; set; }
}