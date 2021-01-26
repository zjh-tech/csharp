using Framework.ELog;
using Framework.ETcp;
using Framework.ETimer;
using System.Threading;


public class BattleServer : Server
{
    public new bool Init()
    {
        if (base.Init() == false)
        {
            Log.Error("[BattleServer] Server Init Error"); 
            return false; 
        }

        if(Net.Instance.Init() == false)
        {
            Log.Error("[BattleServer] Net Init Error");
            return false; 
        }

        ErrorString err = StaticDataMgr.Instance.LoadAllCfg("../../config/binary");
        if(err != null)
        {
            Log.Error("[BattleServer] StaticDataMgr LoadAllCfg Error");
            return false; 
        }

        GlobalDef.GSSServerSessionMgr.Init(new ServerMgr());

        GlobalDef.GServiceDiscoveryClient.Init(GlobalDef.GServerCfg.SDConnectIp, GlobalDef.GServerCfg.SDConnectPort, GlobalDef.GServer.GetServerID(), GlobalDef.GServer.GetToken());

        Log.Info("[BattleServer] Init Ok");
        return true; 
    }


    public override void Run()
    {
        bool busy = false;
        var net_module = Net.Instance;
        var timer_module = TimeMgr.Instance; 
        
        while (!IsQuit())
        {
            busy = false;

            if (net_module.Run(GlobalDef.NET_LOOP_COUNT))
            {
                busy = true; 
            }

            if (timer_module.Update(GlobalDef.TIMER_LOOP_COUNT))
            {
                busy = true;
            }

            if (!busy)
            {
                Thread.Sleep(1);
            }
        }
    }

    public override void UnInit()
    {

    }
}
