using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.Diagnostics;

namespace Engine
{    
    public class UdpServerSession : UdpSession
    {                
        private int iepHashCode;
        private UInt64 serverId;
        private UdpDisConnDele disConnDele;               
                
        private long nextCreateSessionTick;
        
        public UdpServerSession(UInt64 _serverId,uint _conv,Socket _sendSocket, IPEndPoint _remoteIEP, IUdpMsgHandler _handler, long _maxHeartBeatTime)
        {
            serverId = _serverId;
            handler = _handler;
            remoteIEP = _remoteIEP;
            iepHashCode = _remoteIEP.GetHashCode();
            conv = _conv;
                 
            maxHeartBeatTime = _maxHeartBeatTime;
            nextCreateSessionTick = clock.ElapsedMilliseconds + KcpDef.KcpMaxVerifyDiffTime;

            kcp = new Kcp(conv, kcpSender);
            kcp.NoDelay(1, 10, 2, 1);
            kcpSender.Init(_sendSocket, kcp,remoteIEP);
            remoteIEP = _remoteIEP;            
        }

        
        public int GetIepHashCode()
        {
            return iepHashCode;
        }

        public bool IsRepeatedVerify(int iepHashCode)
        {            
            if(remoteIEP.GetHashCode() == iepHashCode && nextCreateSessionTick > clock.ElapsedMilliseconds)
            {
                return true; 
            }

            return false; 
        }              
    }
}
