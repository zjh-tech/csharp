
using Engine;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Log.Init(0, "./log");
  
        while (true)
        {
            Thread.Sleep(10);
        }
    }
}
