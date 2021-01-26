using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    public interface IUdpNet
    {
        bool Init(string _name);

        bool Update(int loopCount);
    }
}
