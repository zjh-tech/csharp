using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.IO;
using System.Diagnostics;

namespace Engine
{
    public class UdpClientSession : UdpSession
    {                                                       
        public UdpClientSession(uint _conv,Socket _sendSocket, IPEndPoint _remoteIEP, IUdpMsgHandler _handler, long _heartBeatTime)
        {
            clientFlag = true;
            conv = _conv;
            handler = _handler;
            remoteIEP = _remoteIEP;

            maxHeartBeatTime = _heartBeatTime;            

            kcp = new Kcp(conv, kcpSender);
            kcp.NoDelay(1, 10, 2, 1);
            kcpSender.Init(_sendSocket, kcp, remoteIEP);
        }
    }
}
