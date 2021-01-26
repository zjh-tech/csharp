using System;
using Framework.ETimer;
using Framework.ELog;
using Pb;


public delegate void SDClientCbFunc();

public class ServiceDiscoveryClient
{
    public bool Init(string _host, UInt32 _port, UInt64 _server_id, string _token, SDClientCbFunc _cb_func = null)
    {
        ssclient_session_mgr.Init();

        host = _host;
        port = _port;
        server_id = _server_id;
        token = _token;
        cb_func = _cb_func;        
        session_id = ssclient_session_mgr.Connect(host, port, new SDServerSession(), new Coder());
        
        timer_register.AddRepeatTimer((UInt32)TimerID.SD_CLIENT_SEND_REQ_TIMER_ID, (long)TimerDelay.SD_CLIENT_SEND_REQ_TIMER_DELAY, "ServiceDiscoveryClient-SendReq", send_service_discovery_req);
        timer_register.AddRepeatTimer((UInt32)TimerID.SD_CLIENT_RECONNECT_TIMER_ID, (long)TimerDelay.SD_CLIENT_RECONNECT_TIMER_DELAY, "ServiceDiscoveryClient-Reconnect", check_reconnect);

        return true; 
    }

    public void SetSSClientSession(SSClientSession sess)
    {
        ssclient_session = sess; 
    }

    public bool GetInitFlag()
    {
        return init_flag; 
    }

    public void SetInitFlag()
    {
        init_flag = true; 
    }

    public SDClientCbFunc GetCbFunc()
    {
        return cb_func; 
    }

    private void send_service_discovery_req(object[] paras)
    {
        if(ssclient_session == null)
        {
            return; 
        }

        service_discovery_req req = new service_discovery_req();
        req.ServerId = server_id;
        req.Token = token;
        ssclient_session.SendProtoMsg((UInt32)S2SBaseMsgId.ServiceDiscoveryReqId, req);
        Log.DebugAf("[ServiceDiscoveryClient] ServiceDiscoveryReq ServerID={0}", server_id); 
    }

    private void check_reconnect(object[] paras)
    {
        if(ssclient_session != null)
        {
            return; 
        }

        if((ssclient_session_mgr.IsInConnectCache(session_id) == false) && ssclient_session_mgr.IsExistSessionOfSrvID(session_id) == false)
        {
            session_id = ssclient_session_mgr.Connect(host, port, new SDServerSession(), new Coder());
            Log.InfoAf("[ServiceDiscoveryClient] Reconnect Session={0},Host={1} Port={2}", session_id, host, port);
        }
    }

    private string host = "";
    private UInt32 port = 0; 
    private UInt64 server_id = 0; 
    private string token = ""; 
    private SDClientCbFunc cb_func = null; 
    private bool init_flag = false; 
    private ITimerRegister timer_register = new TimerRegister();
    private SSClientSessionMgr ssclient_session_mgr = new SSClientSessionMgr();
    private SSClientSession ssclient_session = null; 
    private UInt64 session_id = 0;

    private enum TimerID : UInt32
    {
        SD_CLIENT_SEND_REQ_TIMER_ID = 1,
        SD_CLIENT_RECONNECT_TIMER_ID = 2,
    }

    private enum TimerDelay : long
    {
        SD_CLIENT_SEND_REQ_TIMER_DELAY = 1000 * 3,
        SD_CLIENT_RECONNECT_TIMER_DELAY = 1000 * 10,
    }
}

