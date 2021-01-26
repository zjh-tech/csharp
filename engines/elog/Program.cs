using System;
using System.Threading;
using Framework.ELog;

class Program
{
    static void Main(string[] args)
    {
        string fileDir = "./log";
        Log.Init(0, fileDir, true);

        for (int i = 0; i < 500; ++i)
        {
            Log.DebugA("DebugA");
            Log.InfoA("InfoA");
            Log.WarnA("WarnA");
            Log.ErrorA("ErrorA");

            Log.DebugAf("DebugAf{0}", 1);
            Log.InfoAf("InfoAf{0}", 1);
            Log.WarnAf("WarnAf{0}", 1);
            Log.ErrorAf("ErrorAf{0}", 1);
        }

        Log.Debug("Debug");
        Log.Info("Info");
        Log.Warn("Warn");
        Log.Error("Error");

        Log.Debugf("Debugf{0}", 1);
        Log.Infof("Infof{0}", 1);
        Log.Warnf("Warnf{0}", 1);
        Log.Errorf("Errorf{0}", 1);

        while (true)
        {
            Log.Infof("DebugAf{0}", 2);

            Thread.Sleep(10);
        }
    }
}
