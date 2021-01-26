using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Engine
{
    public interface IUdpEvent
    {        
        UdpEventType GetEvtType();

        uint GetConv();

        byte[] GetMessage();

        UdpReceiver GetUdpReceiver();

        IPEndPoint GetIPEndPoint();

        bool IsServerFlag();
    }
}
