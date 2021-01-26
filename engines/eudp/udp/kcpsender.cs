using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using BufferOwner = System.Buffers.IMemoryOwner<byte>;

namespace Engine
{
    public class KcpSender : IKcpCallback
    {
        private Socket sendSocket;
        private IPEndPoint remoteIEP;
        private Kcp kcp;
        private int sendSize = 0;

        public static QpsTool KcpSend = new QpsTool();
        public static QpsTool UdpSend = new QpsTool();

        public void Init(Socket _sendSocket, Kcp _kcp, IPEndPoint _remoteIEP)
        {
            sendSocket = _sendSocket;
            kcp = _kcp;
            remoteIEP = _remoteIEP;
        }        

        public void Send(byte[] datas)
        {
            KcpSend.AddCount((UInt64)datas.Length);
            kcp.Send(datas);
        }

        public void Output(BufferOwner buffer, int avalidLength)
        {
            UdpSend.AddCount((UInt64)avalidLength);

            sendSize = 0;
            //Kcp ==> Udp合包(调用Kcp.Send和Output次数不一致)
            byte[] datas = buffer.Memory.ToArray();
            byte[] real_datas = null;
            if (datas.Length != avalidLength)
            {
                real_datas = new byte[avalidLength];
                Array.Copy(datas, 0, real_datas, 0, avalidLength);
            }

            try
            {
                if (real_datas == null)
                {
                    sendSocket.BeginSendTo(datas, 0, avalidLength, SocketFlags.None, remoteIEP, OnBeginSendTo, datas);
                }
                else
                {
                    sendSocket.BeginSendTo(real_datas, 0, avalidLength, SocketFlags.None, remoteIEP, OnBeginSendTo, real_datas);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorAf("[Udp] Output Error {0}", ex.ToString());
            }
        }

        private void OnBeginSendTo(IAsyncResult result)
        {
            byte[] datas = result.AsyncState as byte[];
            int bytes = 0;

            try
            {
                bytes = sendSocket.EndSendTo(result);
            }
            catch (Exception ex)
            {
                Log.ErrorAf("[Udp] OnBeginSendTo  EndSendTo {0}", ex.ToString());
            }

            sendSize += bytes;
            if (sendSize < datas.Length)
            {
                try
                {
                    sendSocket.BeginSendTo(datas, sendSize, datas.Length, SocketFlags.None, remoteIEP, OnBeginSendTo, datas);
                }
                catch (Exception ex)
                {
                    Log.ErrorAf("[Udp] OnBeginSendTo BeginSendTo  Error {0}", ex.ToString());
                }
            }
        }
    }
}
