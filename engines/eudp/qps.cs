using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public class QpsTool
    {
        public UInt64 Qps = 0;
        public UInt64 Flow = 0;
        public object Locker = new object();

        public void GetAndReset(out UInt64 qps,out UInt64 flow)
        {            
            lock (Locker)
            {
                qps = Qps;
                flow = Flow;

                Qps = 0;
                Flow = 0;
            }         
        }        

        public void AddCount(UInt64 flow)
        {
            lock (Locker)
            {
                ++Qps;
                Flow += flow; 
            }
        }
    };
}