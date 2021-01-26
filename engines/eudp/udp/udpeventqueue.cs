using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public class UdpEventQueue : IUdpEventQueue
    {
        private Queue<IUdpEvent> eventQueue = new Queue<IUdpEvent>();

        public void PushEvent(IUdpEvent evt)
        {
            lock (eventQueue)
            {
                eventQueue.Enqueue(evt);
            }
        }
        public IUdpEvent PopEvent()
        {
            lock (eventQueue)
            {
                return (eventQueue.Count != 0) ? eventQueue.Dequeue() : null;
            }
        }
    }
}
