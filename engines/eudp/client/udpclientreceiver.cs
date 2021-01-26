using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Engine
{
    public class UdpClientReceiver : UdpReceiver
    {        
        private UdpClientSessionMgr sessionMgr = new UdpClientSessionMgr();
        private byte[] kcpVerifyResBytes;
        private int kcpVerifyResTotalLen;
        private UdpVerfiyResDele dele;                
        public UdpClientReceiver(UdpVerfiyResDele _dele)
        {
            dele = _dele;
            kcpVerifyResBytes = System.Text.Encoding.Default.GetBytes(KcpDef.KcpVerifyRes);
            kcpVerifyResTotalLen = kcpVerifyResBytes.Length + KcpDef.KcpConvLen;
        }

        public UdpClientSessionMgr GetUdpClientSessionMgr()
        {
            return sessionMgr;
        }

        public UdpVerfiyResDele GetUdpVerfiyResDele()
        {
            return dele;
        }

        private bool IsVerifyResMsg(in byte[] datas)
        {
            if(datas.Length != kcpVerifyResTotalLen)
            {
                return false; 
            }

            for(int i = 0; i < kcpVerifyResBytes.Length;++i)
            {
                if(kcpVerifyResBytes[i] != datas[i])
                {
                    return false; 
                }
            }

            return true; 
        }

        protected override void ProcessUdpMsg(byte[] datas)
        {
            uint conv = 0;
            if (IsVerifyResMsg(datas))
            {
                conv = BitConverter.ToUInt32(datas, kcpVerifyResBytes.Length);
                UdpEvent evt = new UdpEvent(this, datas, remoteIEP as IPEndPoint, conv, UdpEventType.VerifyRes,false);
                UdpNet.Instance.PushEvent(evt);                            
            }
            else
            {
                //kcp message                 
                if (Kcp.GetConv(datas, ref conv))
                {
                    UdpEvent evt = new UdpEvent(this, datas, remoteIEP as IPEndPoint, conv, UdpEventType.RecvMsg,false);
                    UdpNet.Instance.PushEvent(evt);                    
                }
            }
        }
    }
}
