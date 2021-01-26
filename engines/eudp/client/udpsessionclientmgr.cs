using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Engine
{
    public class UdpClientSessionMgr
    {        
        private Dictionary<uint, UdpClientSession> dict = new Dictionary<uint,UdpClientSession>();  //conv -  UdpClientSession        

        public UdpClientSession CreateSession(uint conv,IUdpMsgHandler _handler,Socket sendSocket,IPEndPoint remoteIEP,long heartBeatTime)
        {
            UdpClientSession session = new UdpClientSession(conv,sendSocket, remoteIEP, _handler, heartBeatTime);
            dict[session.GetConv()] = session;
            Log.InfoAf("[Udp] UdpClientSessionMgr  Conv={0} Add UdpClientSession RemoteIp ={1} RemotePort = {2}", conv,session.GetRemoteIp(),session.GetRemotePort());
            return session;
        }

        public UdpClientSession FindSession(uint conv)
        {
            if (dict.ContainsKey(conv))
            {
                return dict[conv];
            }

            return null;
        }

        public void DelSession(uint conv)
        {
            if (dict.ContainsKey(conv))
            {
                UdpClientSession session = dict[conv];
                Log.InfoAf("[Udp] UdpClientSessionMgr Del Conv ={0} UdpSession RemoteIp ={1} RemotePort = {2}", session.GetConv(), session.GetRemoteIp(), session.GetRemotePort());
                dict.Remove(conv);
            }
        }

        public void SendKcpMessage(uint conv, byte[] datas)
        {
            if (dict.ContainsKey(conv))
            {
                dict[conv].KcpSend(datas);
            }
        }

        public void BroadCastMessage(byte[] datas)
        {
            foreach (KeyValuePair<uint, UdpClientSession> kv in dict)
            {
                kv.Value.KcpSend(datas);
            }
        }

        public bool Update(int loopCount)
        {
            bool busy = false;
            List<uint> delList = null;
            foreach (KeyValuePair<uint, UdpClientSession> kv in dict)
            {
                if (kv.Value.KcpUpdate(loopCount))
                {
                    busy = true;
                }

                if (!kv.Value.IsNormalHeartBeatFlag())
                {
                    if (delList == null)
                    {
                        delList = new List<uint>();
                    }

                    Log.ErrorAf("[Udp] Conv = {0} HeartBeat Error", kv.Value.GetConv());
                    delList.Add(kv.Key);
                }
            }

            if (delList != null)
            {
                for (int i = 0; i < delList.Count; ++i)
                {
                    DelSession(delList[i]);
                }
            }

            return busy;
        }
    }
}
