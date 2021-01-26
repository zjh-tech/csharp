using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.Security.Cryptography;
using System.IO;

namespace Engine
{
    public class UdpServerReceiver : UdpReceiver
    {        
        private UInt64 serverId;
        private UdpServerSessionMgr sessionMgr;
        private UdpVerfiyReqDele verifyDele;    
        private byte[] kcpVerifyReqBytes;
        private int kcpVerifyReqTotalLen;

        public UdpServerReceiver(UInt64 _serverId,UdpVerfiyReqDele _dele, uint _convCapacity, IUdpMsgHandler _handler, long _maxHeartBeatTime)
        {
            serverId = _serverId;
            verifyDele = _dele;
            sessionMgr = new UdpServerSessionMgr(_serverId,_convCapacity, _handler, _maxHeartBeatTime);

            kcpVerifyReqBytes = System.Text.Encoding.Default.GetBytes(KcpDef.KcpVerifyReq);
            kcpVerifyReqTotalLen = kcpVerifyReqBytes.Length + KcpDef.KcpVerifyTokenLen;
        }
                
        public UdpServerSessionMgr GetUdpSessionMgr()
        {
            return sessionMgr;
        }

        public UInt64 GetServerId()
        {
            return serverId;
        }

        private bool IsVerifyReqMsg(in byte[] datas)
        {
            if(datas.Length != kcpVerifyReqTotalLen)
            {
                return false; 
            }

            for(int i =0; i< kcpVerifyReqBytes.Length; ++i)
            {
                if(kcpVerifyReqBytes[i] != datas[i])
                {
                    return false; 
                }
            }

            return true; 
        }

        protected override void ProcessUdpMsg(byte[] datas)
        {                                 
            if (IsVerifyReqMsg(datas))
            {                          
                if (verifyDele(datas, 1))
                {
                    UdpEvent evt = new UdpEvent(this, datas, remoteIEP as IPEndPoint, 0, UdpEventType.VerifyReq, true);
                    UdpNet.Instance.PushEvent(evt);
                }
            }            
            else
            {
                //kcp message 
                uint conv = 0;
                if (Kcp.GetConv(datas, ref conv))
                {
                    UdpEvent evt = new UdpEvent(this,datas, remoteIEP as IPEndPoint,conv,UdpEventType.RecvMsg,true);
                    UdpNet.Instance.PushEvent(evt);
                }
            }            
        }

    }
}
