using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Engine
{
    public class UdpNet : IUdpNet
    {
        private string name;
        private UdpEventQueue eventQueue;

        public static UdpNet Instance = new UdpNet();

        public bool Init(string _name)
        {
            name = _name;
            eventQueue = new UdpEventQueue();
            return true;
        }

        public void PushEvent(IUdpEvent evt)
        {
            eventQueue.PushEvent(evt);
        }

        public bool Update(int loopCount)
        {
            bool busy = false;
            for (int i = 0; i < loopCount; ++i)
            {
                IUdpEvent evt = eventQueue.PopEvent();
                if (evt == null)
                {
                    continue;
                }

                if (evt.IsServerFlag() && evt.GetEvtType() == UdpEventType.VerifyReq)
                {
                    ProcessVerifyReqMsg(evt);
                }
                else if(!evt.IsServerFlag() && evt.GetEvtType() == UdpEventType.VerifyRes)
                {
                    ProcessVerifyResMsg(evt);
                }
                else if(evt.GetEvtType() == UdpEventType.RecvMsg)
                {
                    if(evt.IsServerFlag())
                    {
                        ProcessServerKcpMsg(evt);
                    }
                    else
                    {
                        ProcessClientKcpMsg(evt);
                    }                                        
                }
            }

            UdpServerMgr.Instance.Update(loopCount);

            return busy;
        }

        private void ProcessVerifyReqMsg(IUdpEvent evt)
        {
            UdpServerReceiver serverReceiver = evt.GetUdpReceiver() as UdpServerReceiver;
            if(serverReceiver == null)
            {
                return; 
            }

            int iepHashCode = evt.GetIPEndPoint().GetHashCode();
            UdpServerSessionMgr mgr = serverReceiver.GetUdpSessionMgr();

            UdpServerSession session = mgr.FindSession(iepHashCode);
            if (session == null)
            {
                session = mgr.CreateSession(serverReceiver.GetSocket(), evt.GetIPEndPoint());
            }
            else
            {
                if (!session.IsRepeatedVerify(iepHashCode))
                {
                    mgr.DelSession(iepHashCode);
                    session = mgr.CreateSession(serverReceiver.GetSocket(), evt.GetIPEndPoint());
                }
            }

            if (session == null)
            {
                Log.ErrorAf("[Udp] Verify Ok And Create Session Error");
                return;
            }

            byte[] resKey = System.Text.Encoding.Default.GetBytes(KcpDef.KcpVerifyRes);
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memoryStream);
            writer.Write(resKey);
            writer.Write(session.GetConv());
            serverReceiver.GetUdpSender().SendUdpMsg(serverReceiver.GetSocket(), evt.GetIPEndPoint(),memoryStream.ToArray());
        }

        private void ProcessVerifyResMsg(IUdpEvent evt)
        {
            UdpClientReceiver receiver = evt.GetUdpReceiver() as UdpClientReceiver;
            if(receiver == null)
            {
                return; 
            }

            UdpClientSessionMgr mgr = receiver.GetUdpClientSessionMgr();
            UdpClientSession session = mgr.FindSession(evt.GetConv());
            if (session != null)
            {
                Log.WarnAf("[Udp] UdpClientReceiver session exist conv = {0}",evt.GetConv());
                return;
            }            
            receiver.GetUdpVerfiyResDele()(evt.GetConv(), receiver.GetSocket(), evt.GetIPEndPoint(), mgr);
        }

        private void ProcessServerKcpMsg(IUdpEvent evt)
        {
            int iepHashCode = evt.GetIPEndPoint().GetHashCode();
            UdpServerReceiver receiver = evt.GetUdpReceiver() as UdpServerReceiver;
            if(receiver == null)
            {
                return; 
            }

            UdpServerSession session = receiver.GetUdpSessionMgr().FindSession(iepHashCode);
            if (session == null)
            {
                Log.WarnAf("[Udp] UdpServerReceiver ServerId = {0} Conv = {1}", receiver.GetServerId(),evt.GetConv());
            }
            else if (evt.GetConv() == session.GetConv())
            {
                session.KcpInput(evt.GetMessage());
            }
        }
        private void ProcessClientKcpMsg(IUdpEvent evt)
        {
            UdpClientReceiver receiver = evt.GetUdpReceiver() as UdpClientReceiver;
            if(receiver == null)
            {
                return; 
            }

            UdpClientSession session = receiver.GetUdpClientSessionMgr().FindSession(evt.GetConv());
            if (session == null)
            {
                Log.WarnAf("[Udp] UdpClientReceiver Conv = {0} Not Exist", evt.GetConv());
            }
            else if (evt.GetConv() == session.GetConv())
            {
                session.KcpInput(evt.GetMessage());
            }
        }
    }
}
