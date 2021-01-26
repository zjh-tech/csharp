using System;

namespace Engine
{
    public interface IUdpMsgHandler
    {
         void OnHandle(UInt32 msgid, byte[] datas, IUdpSession udpsession);
    }
}