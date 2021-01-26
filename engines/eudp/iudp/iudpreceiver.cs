using System.Net;
using System.Net.Sockets;

namespace Engine
{
    public delegate bool UdpVerfiyReqDele(byte[] datas, int offset);

    public delegate bool UdpVerfiyResDele(uint conv, Socket _sendSocket,IPEndPoint remoteIEP,UdpClientSessionMgr _mgr);

    public interface IUdpReceiver
    {
    }
}
