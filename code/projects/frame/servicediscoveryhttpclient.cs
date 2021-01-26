using System;
using Framework.ETimer;
using Framework.ELog;
using Pb;

public class ServiceDiscoveryHttpClient
{
    public bool Init(string _url, UInt64 _server_id, string _token, SDClientCbFunc _cb_func = null)
    {
        url = "http://" + _url;
        server_id = _server_id;
        token = _token;
        cb_func = _cb_func;
        Log.Info("[SDClient] Http Client Init Ok"); 

        timer_register.AddRepeatTimer((UInt32)TimerID.SERVICEDISCOVERY_UPD_TIMER_ID, (long)TimerDelay.SERVICEDISCOVERY_UPD_TIMER_DELAY, "ServiceDiscoveryClient-SendHttpReq", send_service_discovery_req);
        return true;
    }

    private void send_service_discovery_req(object[] paras)
    {
        service_discovery_req req = new service_discovery_req();
        req.ServerId = server_id;
        req.Token = token;                
        Log.DebugAf("[ServiceDiscoveryHttpClient] ServiceDiscoveryReq ServerID={0}", server_id);
    }    

    private bool init_flag = false;
    private UInt64 server_id = 0;
    private string token = "";
    private ITimerRegister timer_register = new TimerRegister();
    private SDClientCbFunc cb_func = null;
    private string url = ""; 

    private enum TimerID : UInt32
    {
        SERVICEDISCOVERY_UPD_TIMER_ID  = 1,        
    }

    private enum TimerDelay : long
    {
        SERVICEDISCOVERY_UPD_TIMER_DELAY  = 1000 * 3,        
    }
}