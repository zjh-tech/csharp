using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.IO;
class Program
{
    static void Main(string[] args)
    {
        BattleServer server = new BattleServer();
        if (server.Init())
        {
            server.Run();
        }

        server.UnInit();
    }
}

