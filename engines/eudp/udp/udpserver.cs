using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Engine
{
    public class UdpServer : IUdpServer
    {
        private UdpServerReceiver udpReceiver;
        private UInt64 serverId = 0;

        public UdpServer(UInt64 _serverId)
        {
            serverId = _serverId;
        }

        public UInt64 GetServerId()
        {
            return serverId;
        }

        public Socket GetSocket()
        {
            return udpReceiver.GetSocket();
        }
            
       
        public void Start(IPEndPoint localIEP, UdpVerfiyReqDele _dele, uint _convCapacity, IUdpMsgHandler _handler, long _maxHeartBeatTime)
        {
            udpReceiver = new UdpServerReceiver(serverId, _dele, _convCapacity, _handler, _maxHeartBeatTime);
            udpReceiver.StartReceive(localIEP);
        }

        public void Stop()
        {
            udpReceiver.Terminate();            
        }       
        
        public void SendKcpMessage(int iepHashCode, byte[] datas)
        {
            UdpSession session = udpReceiver.GetUdpSessionMgr().FindSession(iepHashCode);
            if(session != null)
            {
                session.KcpSend(datas);
            }            
        }

        public bool Update(int loopCount)
        {
            return udpReceiver.GetUdpSessionMgr().Update(loopCount);            
        }
    }
}
