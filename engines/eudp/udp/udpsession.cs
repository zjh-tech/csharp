using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.Diagnostics;
using System.IO;

namespace Engine
{
    public class UdpSession : IUdpSession
    {
        protected uint conv;
        protected IPEndPoint remoteIEP;
        protected Kcp kcp;
        protected KcpSender kcpSender = new KcpSender();
        protected IUdpMsgHandler handler;

        protected bool clientFlag = false; 
        protected long nextHeartBeatSendTick;        
        protected long nextHeartBeatCheckTick;
        protected long maxHeartBeatTime;
        protected Stopwatch clock = Stopwatch.StartNew();

        public uint GetConv()
        {
            return conv;
        }

        public string GetRemoteIp()
        {
            return remoteIEP.Address.ToString();
        }

        public int GetRemotePort()
        {
            return remoteIEP.Port;
        }

        public void KcpSend(byte[] datas)
        {
            kcpSender.Send(datas);
        }

        public void KcpInput(byte[] datas)
        {
            kcp.Input(datas);
        }

        private void SendKcpHeartBeatReqMsg()
        {            
            KcpHeartBeatReqMsg req = new KcpHeartBeatReqMsg();
            byte[] datas = null;
            req.Serialize(out datas);            
            KcpSend(datas);            
        }

        private void SendKcpHeartBeatResMsg()
        {            
            KcpHeartBeatResMsg res = new KcpHeartBeatResMsg();
            byte[] datas = null;
            res.Serialize(out datas);
            KcpSend(datas);            
        }

        public bool IsNormalHeartBeatFlag()
        {            
            return (nextHeartBeatCheckTick > clock.ElapsedMilliseconds) ? true : false;
        }

        public bool KcpUpdate(int loopCount)
        {
            kcp.Update(DateTime.UtcNow);

            if(clientFlag && nextHeartBeatSendTick == 0)
            {
                nextHeartBeatSendTick = clock.ElapsedMilliseconds + maxHeartBeatTime / KcpDef.KcpHeartBeatCount;
            }

            if (clientFlag && clock.ElapsedMilliseconds > nextHeartBeatSendTick)
            {
                SendKcpHeartBeatReqMsg();
                nextHeartBeatSendTick = clock.ElapsedMilliseconds + maxHeartBeatTime / KcpDef.KcpHeartBeatCount;
            }

            if(nextHeartBeatCheckTick == 0)
            {
                nextHeartBeatCheckTick = clock.ElapsedMilliseconds + maxHeartBeatTime ;
            }

            bool busy = false;
            for (int i = 0; i < loopCount; ++i)
            {
                int len = kcp.PeekSize();
                if (len <= 0)
                {
                    break;
                }

                byte[] datas = new byte[len];
                if (kcp.Recv(datas) >= 0)
                {
                    uint msgid = BitConverter.ToUInt32(datas, 0);
                    if(msgid == KcpDef.KcpHeartBeatReqId)
                    {
                        Log.InfoAf("[Udp]  HeartBeat Normal  Conv = {0},RemoteIp = {1} RemotePort = {2}", GetConv(), GetRemoteIp(), GetRemotePort());
                        nextHeartBeatCheckTick = clock.ElapsedMilliseconds + maxHeartBeatTime;
                        SendKcpHeartBeatResMsg();
                    }
                    else if(msgid == KcpDef.KcpHeartBeatResId)
                    {
                        nextHeartBeatCheckTick = clock.ElapsedMilliseconds + maxHeartBeatTime;
                        Log.InfoAf("[Udp]  HeartBeat Normal  Conv = {0},RemoteIp = {1} RemotePort = {2}", GetConv(), GetRemoteIp(), GetRemotePort());                        
                    }
                    else
                    {
                        handler.OnHandle(msgid, datas, this);
                    }
                    
                    busy = true;
                }
            }

            return busy;
        }
    }
}
