using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.Security.Cryptography;

namespace Engine
{
    public class UdpReceiver : IUdpReceiver
    {
        protected static int MSG_BUFF_SIZE = 64 * 1000;
        protected Socket socket;
        protected IPEndPoint localIEP;
        protected byte[] recvBuff = new byte[MSG_BUFF_SIZE];
        protected bool terminate = false;
        protected EndPoint remoteIEP;
        protected UdpSender udpSender;

        public static QpsTool UdpRecv = new QpsTool();

        public Socket GetSocket()
        {
            return socket;
        }

        public UdpSender GetUdpSender()
        {
            return udpSender;
        }

        public void Terminate()
        {
            terminate = true;
            Log.Infof("[Udp] UdpReceiver Terminate localIp = {0},localPort = {1}", localIEP.Address.ToString(), localIEP.Port);
        }

        public bool StartReceive(IPEndPoint _localIEP)
        {
            localIEP = _localIEP;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

#if !__linux__
            //udp api cause exception
            socket.IOControl(-1744830452, new byte[] { 0, 0, 0, 0 }, null);
#endif

            socket.Bind(localIEP);

            udpSender = new UdpSender(socket);
            remoteIEP = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                socket.BeginReceiveFrom(recvBuff, 0, MSG_BUFF_SIZE, SocketFlags.None, ref remoteIEP, OnBeginReceiveFrom, null);
            }
            catch (System.Exception ex)
            {
                Log.ErrorAf("[Udp] UdpReceiver BeginReceiveFrom First Error {0}", ex.ToString());
            }

            Log.Infof("[Udp] UdpReceiver BeginReceiveFrom localIp = {0},localPort = {1}", localIEP.Address.ToString(), localIEP.Port);
            return true;
        }

        private void OnBeginReceiveFrom(IAsyncResult result)
        {
            int bytes = 0;
            remoteIEP = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                bytes = socket.EndReceiveFrom(result, ref remoteIEP);
            }
            catch (System.Exception ex)
            {
                Log.ErrorAf("[Udp] UdpReceiver EndReceiveFrom Error {0} Remote = {1}", ex.ToString(), remoteIEP.ToString());
            }

            if (bytes != 0)
            {
                UdpRecv.AddCount((UInt64)bytes);

                byte[] datas = new byte[bytes];
                Array.Copy(recvBuff, 0, datas, 0, bytes);
                //if (BitConverter.IsLittleEndian)
                //{
                //    Array.Reverse(datas);
                //}
                ProcessUdpMsg(datas);
            }

            if (terminate)
            {
                Log.Info("[Udp] UdpReceiver Terminate");
                return;
            }

            remoteIEP = new IPEndPoint(IPAddress.Any, 0);

            try
            {
                socket.BeginReceiveFrom(recvBuff, 0, MSG_BUFF_SIZE, SocketFlags.None, ref remoteIEP, OnBeginReceiveFrom, null);
            }
            catch (System.Exception ex)
            {
                Log.ErrorAf("[Udp] UdpReceiver BeginReceiveFrom Next Error {0}", ex.ToString());
            }
        }
        protected virtual void ProcessUdpMsg(byte[] datas)
        {

        }
    }
}
