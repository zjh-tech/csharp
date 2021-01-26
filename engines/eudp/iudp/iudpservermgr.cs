using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


namespace Engine
{    
    public interface IUdpServerMgr
    {
        UdpServer CreateUdpServer();

        void DestoryUdpServer(UInt64 serverId);

        UdpServer FindUdpServer(UInt64 serverId);

        void SendKcpMessage(UInt64 serverId, int iepHashCode, byte[] datas);

        bool Update(int loopCount);
    }
}
