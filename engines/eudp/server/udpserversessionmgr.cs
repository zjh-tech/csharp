using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Engine
{
    public class UdpServerSessionMgr 
    {
        IUdpMsgHandler handler = null;
        private Dictionary<int, UdpServerSession>  dict = new Dictionary<int, UdpServerSession>(); //iepHashCode - UdpServerSession        
        private UInt64 serverId = 0;        
        private uint convCapacity = 0;
        private long maxHeartBeatTime = 0;
        private Queue<uint> freeConvQueue = new Queue<uint>();
        private List<uint> usedConvList = new List<uint>();
                
        public UdpServerSessionMgr(UInt64 _serverId,uint _convCapacity, IUdpMsgHandler _handler, long _maxHeartBeatTime)
        {
            serverId = _serverId;
            convCapacity = _convCapacity;
            handler = _handler;
            maxHeartBeatTime = _maxHeartBeatTime;

            for (uint i = 1; i <= convCapacity; ++i)
            {
                freeConvQueue.Enqueue(i);
            }            
        }        

        public bool IsBusyState()
        {
            return (freeConvQueue.Count == 0) ? true : false; 
        }

        public int GetFreeConvCount()
        {
            return freeConvQueue.Count;
        }

        public uint PopFreeConv()
        {
            return (freeConvQueue.Count != 0) ? freeConvQueue.Dequeue(): 0;
        }
        
        public UdpServerSession CreateSession(Socket sendSocket,IPEndPoint remoteIEP)
        {
            uint conv = PopFreeConv();
            if(conv == 0)
            {
                return null;
            }
            
            usedConvList.Add(conv);
            UdpServerSession session = new UdpServerSession(serverId,conv,sendSocket, remoteIEP,handler,maxHeartBeatTime);            
            dict[session.GetIepHashCode()] = session;            
            Log.InfoAf("[Udp] UdpServer Add UdpSession ServerId={0} IepHashCode={1}  Conv = {2}  RemoteIp ={3} RemotePort = {4}", serverId, session.GetIepHashCode(),session.GetConv(), session.GetRemoteIp(), session.GetRemotePort());            
            return session;
        }


        public UdpServerSession FindSession(int iepHashCode)
        {
            UdpServerSession session = null;            
            if (dict.ContainsKey(iepHashCode))
            {
                session = dict[iepHashCode];
            }            
            return session;
        }

        public void DelSession(int iepHashCode)
        {            
            if (dict.ContainsKey(iepHashCode))
            {
                UdpServerSession session = dict[iepHashCode];
                usedConvList.Remove(session.GetConv());
                freeConvQueue.Enqueue(session.GetConv());
                Log.InfoAf("[Udp] UdpServer Del UdpSession ServerId={0} IepHashCode={1}  Conv = {2}  RemoteIp ={3} RemotePort = {4}", serverId, session.GetIepHashCode(), session.GetConv(), session.GetRemoteIp(), session.GetRemotePort());                
                dict.Remove(iepHashCode);
            }            
        }

        public bool Update(int loopCount)
        {
            bool busy = false;
            List<int> delList = null;            
            foreach (KeyValuePair<int, UdpServerSession> kv in dict)
            {
                if (kv.Value.KcpUpdate(loopCount))
                {
                    busy = true;
                }

                if (!kv.Value.IsNormalHeartBeatFlag())
                {
                    if(delList == null)
                    {
                        delList = new List<int>();
                    }

                    Log.ErrorAf("[Udp] Conv = {0} HeartBeat Error", kv.Value.GetConv());
                    delList.Add(kv.Key);
                }            
            }            

            if (delList != null)
            {
                for(int i = 0; i < delList.Count; ++i)
                {
                    DelSession(delList[i]);
                }
            }

            return busy; 
        }       
    }
}
