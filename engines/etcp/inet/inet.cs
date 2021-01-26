using System;

namespace Framework.ETcp
{
    public interface INet
    {
        void PushEvent(IEvent evt);

        bool Connect(string host, UInt32 port, ISession session);

        bool Listen(string host, UInt32 port, ISessionfactory factory, int listen_max_count);

        bool Run(int count);
    }
}


