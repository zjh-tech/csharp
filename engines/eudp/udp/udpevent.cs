using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Engine
{
    public enum UdpEventType
    {
        VerifyReq = 1,
        VerifyRes = 2,
        RecvMsg = 3,
    }

    public class UdpEvent : IUdpEvent
    {
        private UdpReceiver receiver;
        private IPEndPoint remoteIEP;
        private UdpEventType evtType;        
        private uint conv;
        private byte[] datas;
        private bool serverflag;

        public UdpEvent(UdpReceiver _receiver,byte[] _datas, IPEndPoint _remoteIEP, uint _conv, UdpEventType _evtType, bool _serverflag)
        {
            receiver = _receiver;
            conv = _conv;
            remoteIEP = _remoteIEP;
            evtType = _evtType;
            datas = _datas;
            serverflag = _serverflag;
        }        
        public UdpEventType GetEvtType()
        {
            return evtType;
        }

        public uint GetConv()
        {
            return conv;
        }

        public byte[] GetMessage()
        {
            return datas; 
        }

        public UdpReceiver GetUdpReceiver()
        {
            return receiver;
        }

        public IPEndPoint GetIPEndPoint()
        {
            return remoteIEP;
        }

        public bool IsServerFlag()
        {
            return serverflag;
        }
    }
}
