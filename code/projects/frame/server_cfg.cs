using System;
using System.Collections.Generic;
using System.Text;

public class ServerCfg
{
    public string ServerName = "";
    public UInt64 ServerId = 0;
    public UInt32 ServerType = 0;
    public string Token = "";
    public string LogDir = "";
    public int LogLevel = 0;
    public UInt32 ThreadNum = 0;

    public string SDListenIp = "";
    public UInt32 SDListenPort = 0;

    public string SDConnectIp = "";
    public UInt32 SDConnectPort = 0;

    public string C2SInterListen = "";
    public string C2SOuterListen = "";
    public UInt32 C2SListenMaxCount = 0;
}