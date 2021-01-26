using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Engine
{
    public class UdpLoadBalance
    {
        public UInt64 ServerId;
        public uint Conv;
    }

    public class UdpServerMgr : IUdpServerMgr
    {
        public static UdpServerMgr Instance = new UdpServerMgr();

        private Dictionary<UInt64, UdpServer> dict = new Dictionary<ulong, UdpServer>();
        private UInt64 startServerId = 0;
        
        public UdpServer CreateUdpServer()
        {
            ++startServerId;
            UdpServer server = new UdpServer(startServerId);
            dict[server.GetServerId()] = server;
            Log.Infof("[Udp] Add UdpServer ServerId = {0}", server.GetServerId());            
            return server;
        }

        public void DestoryUdpServer(UInt64 serverId)
        {
            if (dict.ContainsKey(serverId))
            {
                dict[serverId].Stop();
                dict.Remove(serverId);
            }
        }      
        
        public UdpServer FindUdpServer(UInt64 serverId)
        {
            if (dict.ContainsKey(serverId))
            {
                return dict[serverId];
            }

            return null;
        }

        public  void SendKcpMessage(UInt64 serverId,int iepHashCode, byte[] datas)
        {
            if (dict.ContainsKey(serverId))
            {
                dict[serverId].SendKcpMessage(iepHashCode,datas);
            }
        }

        public bool Update(int loopCount)
        {
            bool busy = false;
            foreach(KeyValuePair<UInt64, UdpServer> kv in dict)
            {
                if(kv.Value.Update(loopCount))
                {
                    busy = true; 
                }
            }
            return busy;
        }

        //public bool CheckAllBusyState(int Count)
        //{
        //    int freeCount = 0;
        //    foreach(KeyValuePair<UInt64, UdpServer> kv in dict)
        //    {
        //        freeCount += kv.Value.GetUdpSessionMgr().GetFreeConvCount();
        //        if(freeCount > Count)
        //        {
        //            return false; 
        //        }
        //    }

        //    return false; 
        //}

        //public List<UdpLoadBalance> PopFreeConvs(int Count)
        //{
        //    List<UdpLoadBalance> retList = new List<UdpLoadBalance>();
        //    for(int i = 0; i < Count; ++i)
        //    {                
        //        foreach (KeyValuePair<UInt64, UdpServer> kv in dict)
        //        {
        //            if (!kv.Value.GetUdpSessionMgr().IsBusyState())
        //            {
        //                UdpLoadBalance info = new UdpLoadBalance();
        //                info.ServerId = kv.Key;
        //                info.Conv = kv.Value.GetUdpSessionMgr().PopFreeConv();
        //                retList.Add(info);
        //            }
        //        }
        //    }

        //    if(retList.Count != Count)
        //    {
        //        Log.ErrorA("[Udp] PopFreeConvs Error");
        //    }

        //    return retList; 
        //}              
    }
}
