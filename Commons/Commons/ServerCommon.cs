using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum ErrorUIType
{
    MessageBox,
    Toast
}

[ProtoContract]
public class Vector3Int
{
    [ProtoMember(1)] public int x;
    [ProtoMember(2)] public int y;
    [ProtoMember(3)] public int z;
}
