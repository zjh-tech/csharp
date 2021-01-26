using System;
using System.Collections.Generic;
using Framework.ETcp;
using Framework.ETimer;
using Framework.ELog;
using Google.Protobuf;
using Pb;

public interface ISSClientSessionHandler
{
    bool Init();

    void OnEstablish(SSClientSession sess);

    void OnTerminate(SSClientSession sess);

    void OnHandlerMsg(SSClientSession sess, uint msg_id, byte[] attach_datas, byte[] datas);

    void OnBeatHeartError(SSClientSession sess);
}

public class SSClientSession : Session
{
    public override void OnEstablish()
    {
        last_beat_heart_time = Util.GetMillSecond();
        var factory = GetSessionFactory();
        factory.AddSession(this);
        last_beat_heart_time = Util.GetMillSecond();
        handler.OnEstablish(this); 

        timer_register.AddRepeatTimer((UInt32)TimerID.CHECK_BEAT_HEART_TIMER_ID, (long)TimerDelay.CHECK_BEAT_HEART_TIMER_DELAY, "SSClientSession-CheckBeatHeart", check_beat_heart);

        if (IsConnectType())
        {
            timer_register.AddRepeatTimer((UInt32)TimerID.SEND_BEAT_HEART_TIMER_ID, (long)TimerDelay.SEND_BEAT_HEART_TIMER_DELAY, "SSClientSession-SendBeatHeart", send_beat_heart);
        }
    }

    public override void OnHandlerMsg(uint msg_id, byte[] attach_datas, byte[] datas)
    {
        if (msg_id == (UInt32)S2SBaseMsgId.S2SClientSessionPingId)
        {
            last_beat_heart_time = Util.GetMillSecond();
            Log.DebugAf("[SSClientSession] SessionID={0} Recv Ping Send Pong", GetSessID());
            s2s_client_session_pong ack = new s2s_client_session_pong();
            SendProtoMsg((UInt32)S2SBaseMsgId.S2SClientSessionPongId, ack);
            return;
        }
        else if (msg_id == (UInt32)S2SBaseMsgId.S2SClientSessionPongId)
        {
            last_beat_heart_time = Util.GetMillSecond();
            Log.DebugAf("[SSClientSession] SessionID={0} Recv Pong", GetSessID());
            return;
        }

        last_beat_heart_time = Util.GetMillSecond();
        handler.OnHandlerMsg(this, msg_id, attach_datas, datas);
    }

    public override void OnTerminate()
    {
        var factory = GetSessionFactory();
        factory.RemoveSession(GetSessID());
        timer_register.KillAllTimer();
        Log.InfoAf("[SSClientSession] OnTerminate SessionID={0}", GetSessID());
        handler.OnTerminate(this);
    }

    private void check_beat_heart(object[] paras)
    {
        long now = Util.GetMillSecond();
        if ((last_beat_heart_time + beat_heart_max_time) < now)
        {
            Log.ErrorAf("[SSClientSession] SessionID={0}  BeatHeart Exception", GetSessID());
            handler.OnBeatHeartError(this);
            Terminate();
        }
    }

    private void send_beat_heart(object[] paras)
    {
        s2s_client_session_ping req = new s2s_client_session_ping();
        SendProtoMsg((UInt32)S2SBaseMsgId.S2SClientSessionPingId, req);
        Log.DebugAf("[SSClientSession] SessionID={0} Send Ping", GetSessID());
    }


    public void SetHandler(ISSClientSessionHandler _handler)
    {
        handler = _handler;
    }

    public void SendBytes(UInt32 msgID, byte[] datas, IAttachParas attach = null)
    {
        AsyncSendMsg(msgID, datas, attach);
    }

    public void SendProtoMsg(UInt32 msgID, IMessage message, IAttachParas attach = null)
    {
        AsyncSendProtoMsg(msgID, message, attach);
    }

    private ISSClientSessionHandler handler = null;
    private ITimerRegister timer_register = new TimerRegister();
    private long last_beat_heart_time = 0;
    private long beat_heart_max_time = 1000 * 60 * 3;    
    private enum TimerID : UInt32
    {
        CHECK_BEAT_HEART_TIMER_ID = 1,
        SEND_BEAT_HEART_TIMER_ID = 2,
    }

    private enum TimerDelay : long
    {
        CHECK_BEAT_HEART_TIMER_DELAY = 1000 * 60,
        SEND_BEAT_HEART_TIMER_DELAY = 1000 * 15,
    }
}


public class SSClientSessionMgr : ISessionfactory
{
    public void Init()
    {        
        timer_register.AddRepeatTimer((UInt32)TimerID.MGR_CONNECT_TIMEOUT_TIMER_ID, (long)TimerDelay.MGR_CONNECT_TIMEOUT_TIMER_DELAY, "SSClientSessionMgr-TimeOut", connect_time_out);
    }

    public bool IsInConnectCache(UInt64 session_id)
    {
        return connect_cache_dict.ContainsKey(session_id);
    }

    public bool IsExistSessionOfSrvID(UInt64 server_id)
    {
        return session_dict.ContainsKey(session_id);       
    }

    public UInt64 Connect(string _host, UInt32 _port,ISSClientSessionHandler _handler, ICoder _coder)
    {
        _handler.Init();

        SSClientSession session = (SSClientSession)CreateSession();
        session.SetHandler(_handler);
        session.SetCoder(_coder);
        session.SetConnectType(); 
        
        ConnectCache cache = new ConnectCache(session.GetSessID(),_host,_port, Util.GetMillSecond() + mgr_beat_heart_max_time);
        connect_cache_dict[session.GetSessID()] = cache;
        Log.InfoAf("[SSClientSessionMgr] ConnectCache Add SessionID={0},Host={1} Port={2}", session.GetSessID(),_host,_port);

        Net.Instance.Connect(_host, _port, session);
        return session.GetSessID(); 
    }

    void Listen(string ip, UInt32 port, ISSClientSessionHandler _handler, ICoder _coder, int listen_max_count)
    {
        _handler.Init();
        handler = _handler;
        coder = _coder; 
        Net.Instance.Listen(ip, port, this, listen_max_count);
    }    

    public void AddSession(ISession sess)
    {
        UInt64 session_id = sess.GetSessID();
        session_dict[session_id] = (SSClientSession)sess;

        Log.InfoAf("[SSClientSessionMgr] AddSession SessionID={0}", session_id);

        if (connect_cache_dict.ContainsKey(session_id))
        {
            Log.InfoAf("[SSClientSessionMgr] AddSession Triggle ConnectCache Del SessionID={0}", session_id);
            connect_cache_dict.Remove(session_id);
        }
    }

    public ISession CreateSession()
    {
        ++session_id;
        SSClientSession session = new SSClientSession();
        session.SetSessID(session_id);
        session.SetCoder(coder);
        session.SetHandler(handler); 
        session.SetSessionFactory(this);
        Log.InfoAf("[SSClientSessionMgr] CreateSession={0}", session.GetSessID());
        return session;
    }

    public void RemoveSession(UInt64 session_id)
    {
        if (session_dict.ContainsKey(session_id))
        {
            Log.InfoAf("[SSClientSessionMgr] RemoveSession SessionID={0} ", session_id);
            session_dict.Remove(session_id);
        }
    }
            
    private void connect_time_out(object[] state)
    {        
        long now = Util.GetMillSecond();
        List<UInt64> remove_list = new List<UInt64>();
        foreach (KeyValuePair<UInt64, ConnectCache> kv in connect_cache_dict)
        {
            if (kv.Value.ConnectTick < now)
            {
                remove_list.Add(kv.Key);
            }
        }

        if (remove_list.Count != 0)
        {
            int remove_count = remove_list.Count;
            for (int i = 0; i < remove_count; ++i)
            {
                Log.InfoAf("[SSClientSessionMgr] Timeout Triggle ConnectCache Del SessionID={0}", remove_list[i]);
                connect_cache_dict.Remove(remove_list[i]);
            }
        }
    }

    private UInt64 session_id = 0;
    private Dictionary<UInt64, SSClientSession> session_dict = new Dictionary<UInt64, SSClientSession>();
    private ISSClientSessionHandler handler = null;
    private ICoder coder = null; 
    private ITimerRegister timer_register = new TimerRegister();
    private Dictionary<UInt64, ConnectCache> connect_cache_dict = new Dictionary<UInt64, ConnectCache>();
    private long mgr_beat_heart_max_time = 1000 * 10;

    private class ConnectCache
    {
        public ConnectCache(UInt64 session_id,string host,UInt32 port, Int64 connect_tick)
        {
            SessionID = session_id;
            Host = host;
            Port = port;            
            ConnectTick = connect_tick;
        }

        public UInt64 SessionID;
        public string Host;
        public UInt32 Port;        
        public Int64 ConnectTick;
    }

    private enum TimerID : UInt32
    {
        MGR_CONNECT_TIMEOUT_TIMER_ID = 1,
    }

    private enum TimerDelay : long
    {
        MGR_CONNECT_TIMEOUT_TIMER_DELAY = 1000 * 45,
    }
}