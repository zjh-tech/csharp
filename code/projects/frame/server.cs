using System;
using Framework.ELog; 

public interface IServerFacade
{
    bool Init();
    void UnInit();
    void Run();
    void Quit();    
}

public abstract class Server : IServerFacade
{
    public bool Init()
    {
        if (FrameCfgMgr.LoadServerCfg("./server_cfg.xml") == false)
        {
            Console.WriteLine("[Server] LoadServerCfg Error");
            return false; 
        }

        server_id = GlobalDef.GServerCfg.ServerId;
        server_type = GlobalDef.GServerCfg.ServerType;
        token = GlobalDef.GServerCfg.Token;
        server_name = GlobalDef.GServerCfg.ServerName;

        string log_path = GlobalDef.GServerCfg.LogDir + "/" + server_name; 

        if (Log.Init(GlobalDef.GServerCfg.LogLevel, log_path, true) == false)
        {
            Console.WriteLine("[Server] Log Init Error");
            return false; 
        }

#if (DEBUG)
        Log.InfoA("Run Debug");
#else
        Log.InfoA("Run Release");       
#endif

        GlobalDef.GServer = this; 
        return true; 
    }
    public abstract void UnInit();    

    public abstract void Run();
    
    public void Quit()
    {
        terminate = true;
        Log.Info("Server Quit");
    }

    public bool IsQuit()
    {
        return terminate; 
    }

    public UInt64 GetServerID()
    {
        return server_id; 
    }

    public UInt32 GetServerType()
    {
        return server_type; 
    }

    public string GetServerName()
    {
        return server_name; 
    }

    public string GetToken()
    {
        return token; 
    }

    private bool terminate = false;
    private UInt64 server_id = 0;
    private UInt32 server_type = 0;
    private string server_name = "";
    private string token = ""; 
}

