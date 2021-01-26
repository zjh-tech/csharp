using System; 

public enum MsgCode : UInt32
{
    SUCCESS = 0,
    FAIL = 1,
}

public enum eServerType : UInt32
{
    UNKNOW_SERVER = 0,
    REGISTER_SERVER = 1,
    LOGIN_SERVER = 2,
    GATEWAY_SERVER = 3,
    CENTER_SERVER = 4,
    MATCH_SERVER = 5,
    HALL_SERVER = 6,
    BATTLE_SERVER = 7,
    DB_SERVER = 8,
}
public sealed class GlobalDef
{
    public static Server GServer = null;

    public static ServerCfg GServerCfg = new ServerCfg();

    public static ServiceDiscoveryClient GServiceDiscoveryClient = new ServiceDiscoveryClient();

    public static SSServerSessionMgr GSSServerSessionMgr = new SSServerSessionMgr();

    public static int NET_LOOP_COUNT = 100;

    public static int TIMER_LOOP_COUNT = 100;

    public static string GetServerName(UInt32 server_type)
    {
        if(server_type > ServerNameArray.Length)
        {
            return ""; 
        }
        return ServerNameArray[server_type]; 
    }

    private static string[] ServerNameArray = new string[]
    {
        "Unknow",
        "RegisterServer",
        "LoginServer",
        "GatewayServer",
        "CenterServer",
        "MatchServer",
        "HallServer",
        "BattleServer",
        "DbServer",
    };
}

