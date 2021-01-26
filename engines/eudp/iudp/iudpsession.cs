using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public delegate void UdpDisConnDele();
    public interface IUdpSession
    {
        uint GetConv();

        void KcpSend(byte[] datas);
    }

    public interface IUdpServerSession : IUdpSession
    {
        void SetUdpDisConnDele(UdpDisConnDele dele);
    }    
}
