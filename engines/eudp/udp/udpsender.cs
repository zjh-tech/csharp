using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Engine
{   
    public class UdpPackage
    {        
        public IPEndPoint RemoteIEP = null;
        public byte[] Datas = null;
        public int SendSize = 0;

        public UdpPackage(IPEndPoint remoteIEP, byte[] datas)
        {
            RemoteIEP = remoteIEP;
            Datas = datas;
            SendSize = 0;
        }
    }

    public class UdpSender
    {
        private Socket sendSocket;
        private Queue<UdpPackage> sendQueue = new Queue<UdpPackage>();

        public UdpSender(Socket _sendSocket)
        {
            sendSocket = _sendSocket;            
        }
        
        public void SendUdpMsg(Socket sendSocket,IPEndPoint remoteIEP,byte[] datas)
        {            
            UdpPackage pkg = new UdpPackage(remoteIEP,datas);
            bool freeflag = false;
            lock (sendQueue)
            {
                freeflag = (sendQueue.Count == 0) ? true : false;
                sendQueue.Enqueue(pkg);
            }
            
            if (freeflag)
            {                
                try
                {                    
                    sendSocket.BeginSendTo(datas, 0, datas.Length, SocketFlags.None, remoteIEP, OnBeginSendTo,null);
                }
                catch (Exception ex)
                {
                    Log.ErrorAf("[Udp] UdpSender BeginSendTo  Error {0}", ex.ToString());
                    ProcessSendException();
                }
            }                      
        }        

       
        private void OnBeginSendTo(IAsyncResult result)
        {            
            int bytes = 0;

            try
            {
                bytes = sendSocket.EndSendTo(result);
            }
            catch (Exception ex)
            {
                Log.ErrorAf("[Udp] UdpSender  EndSendTo {0}", ex.ToString());
                ProcessSendException();
            }

            UdpPackage curPkg = null;
            UdpPackage nextPkg = null;
            lock (sendQueue)
            {
                UdpPackage pkg = sendQueue.Peek();
                pkg.SendSize += bytes;
                if (pkg.SendSize < pkg.Datas.Length)
                {
                    curPkg = sendQueue.Peek();
                }
                else if (pkg.SendSize == pkg.Datas.Length)
                {                    
                    sendQueue.Dequeue();                    
                    if (sendQueue.Count != 0)
                    {
                        nextPkg = sendQueue.Peek();
                    }
                }                
            }

            if(curPkg != null)
            {
                try
                {
                    sendSocket.BeginSendTo(curPkg.Datas, curPkg.SendSize, curPkg.Datas.Length, SocketFlags.None, curPkg.RemoteIEP, OnBeginSendTo, null);
                }
                catch (Exception ex)
                {
                    Log.ErrorAf("[Udp] UdpSender CurPackage Continue BeginSendTo  Error {0}", ex.ToString());
                    ProcessSendException();
                }
            }

            if(nextPkg != null)
            {
                try
                {
                    sendSocket.BeginSendTo(nextPkg.Datas, 0, nextPkg.Datas.Length, SocketFlags.None, nextPkg.RemoteIEP, OnBeginSendTo, null);
                }
                catch (Exception ex)
                {
                    Log.ErrorAf("[Udp] UdpSender NextPackage BeginSendTo  Error {0}", ex.ToString());
                    ProcessSendException();
                }
            }            
        }

        private void ProcessSendException()
        {
            UdpPackage pkg = null;
            lock (sendQueue)
            {
                pkg = sendQueue.Dequeue();
            }

            Log.ErrorAf("[Udp] UdpSender SendException RemoteIp ={0},RemotePort = {1}", pkg.RemoteIEP.Address.ToString(),pkg.RemoteIEP.Port);
        }
    }
}
