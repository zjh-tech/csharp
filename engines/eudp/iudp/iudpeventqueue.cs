using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public interface IUdpEventQueue
    {
        void PushEvent(IUdpEvent evt);

        IUdpEvent PopEvent();
    }
}
