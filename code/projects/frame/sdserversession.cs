using System;
using Framework.ELog;
using Pb;

public delegate bool SDServerFunc(SSClientSession sess, byte[] datas);

public class SDServerSession  : ISSClientSessionHandler
{
    public bool Init()
    {
        iddealer.Register((UInt32)S2SBaseMsgId.ServiceDiscoveryAckId, OnHandlerServiceDiscoveryAck);
        return true;
    }

    public void OnEstablish(SSClientSession sess)
    {
        GlobalDef.GServiceDiscoveryClient.SetSSClientSession(sess);
    }

    public void OnTerminate(SSClientSession sess)
    {
        GlobalDef.GServiceDiscoveryClient.SetSSClientSession(null);
    }

    public void OnHandlerMsg(SSClientSession sess, UInt32 msg_id, byte[] attach_datas, byte[] datas)
    {
        var dealer = iddealer.Find(msg_id);
        if(dealer == null)
        {
            Log.WarnAf("SDServerSession OnHandlerMsg Can Not Find MsgID = {0}", msg_id);
            return; 
        }

        dealer(sess,datas);
    }

    public void OnBeatHeartError(SSClientSession sess)
    {

    }
    public static bool OnHandlerServiceDiscoveryAck(SSClientSession sess,byte[] datas)
    {
        service_discovery_ack ack = service_discovery_ack.Parser.ParseFrom(datas);
        
        if(ack.RebuildFlag == true)
        {
            Log.InfoA("[ServiceDiscovery] Service List Rebuilding");
            return true; 
        }

        if(ack.VerifyFlag == false)
        {
            Log.InfoA("[ServiceDiscovery] Token Verify Error");
            return false; 
        }

        if(GlobalDef.GServiceDiscoveryClient.GetInitFlag() == false)
        {
            GlobalDef.GServiceDiscoveryClient.SetInitFlag();
            //S2S Listen
            if(ack.SdInfo.S2SInterListen != "" && ack.SdInfo.S2SOuterListen != "")
            {
                 string[] listen_array = ack.SdInfo.S2SOuterListen.Split(":");
                 if(listen_array.Length != 2)
                 {
                    GlobalDef.GServer.Quit();
                    return true; 
                 }

                 GlobalDef.GSSServerSessionMgr.Listen(listen_array[0], UInt32.Parse(listen_array[1]),int.MaxValue);
            }

            GlobalDef.GServerCfg.C2SInterListen = ack.SdInfo.C2SInterListen;
            GlobalDef.GServerCfg.C2SOuterListen = ack.SdInfo.C2SOuterListen;
            GlobalDef.GServerCfg.C2SListenMaxCount = ack.SdInfo.C2SMaxCount;

            var cb_func = GlobalDef.GServiceDiscoveryClient.GetCbFunc();
            if (cb_func != null)
            {
                cb_func(); 
            }
        }

        //Add Connect
        int conn_size = ack.SdInfo.ConnList.Count; 
        for(int i = 0; i < conn_size; ++i)
        {
            var conn_attr = ack.SdInfo.ConnList[i];
            if (conn_attr.ServerId == 0)
            {
                continue; 
            }

            bool exist_flag = false; 
            if(GlobalDef.GSSServerSessionMgr.IsInConnectCache(conn_attr.ServerId) || GlobalDef.GSSServerSessionMgr.IsExistSessionOfSrvID(conn_attr.ServerId))
            {
                exist_flag = true; 
            }

            if(exist_flag == false)
            {
                string[] listen_array = conn_attr.Outer.Split(":");
                if (listen_array.Length != 2)
                {
                    Log.InfoAf("[ServiceDiscovery] S2S Outer={0} Error",conn_attr.Outer);                    
                    return true;
                }

                GlobalDef.GSSServerSessionMgr.Connect(conn_attr.ServerId, conn_attr.ServerType, conn_attr.ServerTypeStr, listen_array[0], UInt32.Parse(listen_array[1]), conn_attr.Token);
            }
        }

        return true;
    }

    private IDDealer<SDServerFunc> iddealer = new IDDealer<SDServerFunc>((UInt32)MsgIDRange.SD_MIN_ID,(UInt32)MsgIDRange.SD_MAX_ID);    
}

