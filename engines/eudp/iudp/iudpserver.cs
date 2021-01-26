using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Engine
{    
    public interface IUdpServer
    {
        void Start(IPEndPoint localIEP, UdpVerfiyReqDele _dele, uint _convCapacity, IUdpMsgHandler _handler, long _maxHeartBeatTime);

        UInt64 GetServerId();

        Socket GetSocket();                       
    }
}
