using System;
using Framework.ELog; 

public class ServerMgr : ILogicServerFactory
{
    public override void SetLogicServer(SSServerSession sess)
    {
        UInt32 server_type =  sess.GetRemoteServerType(); 
        if(server_type == (UInt32)eServerType.MATCH_SERVER)
        {
            sess.SetLogicServer(new MatchServer());
        } 
        else
        {
            Log.ErrorAf("[ServerMgr] SetLogicServer Not Find ServerType = {0} SSServerSession", GlobalDef.GetServerName(server_type));
        }
    }
}