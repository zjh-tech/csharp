using System;
using System.Collections.Generic;
using Framework.ETimer;
using Framework.ETcp;
using Framework.ELog;
using Google.Protobuf;
using Pb;

public class SSServerSession : Session
{
    public override void OnEstablish()
    {
        session_state = SessionState.VERIFY;
        var factory = GetSessionFactory();
        factory.AddSession(this);
        last_beat_heart_time = Util.GetMillSecond();

        if (IsConnectType())
        {
            Log.InfoAf("[SSServerSession] SessionID={0},ServerID={1},ServerType={2} Send Verify Req", GetSessID(), remote_server_id, remote_server_type_str);
            s2s_server_session_veriry_req req = new s2s_server_session_veriry_req();
            req.ServerId = GlobalDef.GServer.GetServerID();
            req.ServerType = GlobalDef.GServer.GetServerType();
            req.ServerTypeStr = GlobalDef.GServer.GetServerName();
            req.Token = remote_token;
            SendProtoMsg((UInt32)S2SBaseMsgId.S2SServerSessionVeriryReqId, req);
            return;
        }
    }

    public override void OnTerminate()
    {
        if (remote_server_id == 0)
        {
            Log.InfoAf("[SSServerSession] OnTerminate SessionID={0}", GetSessID());
        }
        else
        {
            Log.InfoAf("[SSServerSession] OnTerminate SessionID={0},ServerID={1},ServerType={2}", GetSessID(), remote_server_id, remote_server_type_str);
        }

        timer_register.KillAllTimer();
        var factory = GetSessionFactory();
        factory.RemoveSession(GetSessID());
        if (session_state == SessionState.ESTABLISH)
        {
            logic_server.SetSession(null);
            logic_server.OnTerminate(this);
        }
        session_state = SessionState.CLOSE;
    }

    public override void OnHandlerMsg(UInt32 msg_id, byte[] attach_datas, byte[] datas)
    {
        if (msg_id == (UInt32)S2SBaseMsgId.S2SServerSessionVeriryReqId && IsListenType())
        {
            s2s_server_session_veriry_req req = s2s_server_session_veriry_req.Parser.ParseFrom(datas);

            Action<MsgCode> ack_func = (MsgCode error_code) =>
            {
                if (error_code == MsgCode.FAIL)
                {
                    Terminate();
                    return;
                }

                s2s_server_session_veriry_ack ack = new s2s_server_session_veriry_ack();
                SendProtoMsg((UInt32)S2SBaseMsgId.S2SServerSessionVeriryAckId, ack);
            };

            var factory = (SSServerSessionMgr)GetSessionFactory();
            if (factory.IsExistSessionOfSrvID(req.ServerId))
            {
                Log.InfoAf("[SSServerSession] SessionID={0},ServerID={1} Already Exist", GetSessID(), req.ServerId);
                ack_func(MsgCode.FAIL);
                return;
            }

            SetRemoteServerID(req.ServerId);
            SetRemoteServerType(req.ServerType);
            SetRemoteServerTypeStr(req.ServerTypeStr);
            SetRemoteToken(req.Token);

            if (req.Token != GlobalDef.GServer.GetToken())
            {
                Log.ErrorAf("[SSServerSession] SessionID={0},ServerID={1},ServerType={2} Token Error Verify Fail", GetSessID(), remote_server_id, remote_server_type_str);
                ack_func(MsgCode.FAIL);
                return;
            }

            Log.InfoAf("[SSServerSession] SessionID={0},ServerID={1},ServerType={2} Token Error Verify Success", GetSessID(), remote_server_id, remote_server_type_str);
            on_verify();
            ack_func(MsgCode.SUCCESS);

            return;
        }

        if (msg_id == (UInt32)S2SBaseMsgId.S2SServerSessionVeriryAckId && IsConnectType())
        {
            Log.InfoAf("[SSServerSession] SessionID={0},ServerID={1},ServerType={2} Verify Ok", GetSessID(), remote_server_id, remote_server_type_str);
            on_verify();
            return;
        }

        if (msg_id == (UInt32)S2SBaseMsgId.S2SServerSessionPingId && IsListenType())
        {
            last_beat_heart_time = Util.GetMillSecond();
            s2s_server_session_pong ack = new s2s_server_session_pong();
            SendProtoMsg((UInt32)S2SBaseMsgId.S2SServerSessionPongId, ack);
            Log.DebugAf("[SSServerSession] SessionID={0},ServerID={1},ServerType={2} Recv Ping Send Pong", GetSessID(), remote_server_id, remote_server_type_str);
            return;
        }

        if (msg_id == (UInt32)S2SBaseMsgId.S2SServerSessionPongId && IsConnectType())
        {
            last_beat_heart_time = Util.GetMillSecond();
            Log.DebugAf("[SSServerSession] SessionID={0},ServerID={1},ServerType={2} Recv Pong", GetSessID(), remote_server_id, remote_server_type_str);
            return;
        }

        last_beat_heart_time = Util.GetMillSecond();
        logic_server.OnHandlerMsg(this, msg_id, attach_datas, datas);
    }

    private void on_verify()
    {
        session_state = SessionState.ESTABLISH;
        Log.InfoAf("[SSServerSession] OnVerify SessionID={0},ServerID={1},ServerType={2}", GetSessID(), remote_server_id, remote_server_type_str);
        var factory = (SSServerSessionMgr)GetSessionFactory();
        var logicserverfactory = factory.GetLogicServerFactory();
        logicserverfactory.SetLogicServer(this);
        logic_server.SetSession(this);
        logic_server.OnEstablish(this);

        timer_register.AddRepeatTimer((UInt32)TimerID.CHECK_BEAT_HEART_TIMER_ID, (long)TimerDelay.CHECK_BEAT_HEART_TIMER_DELAY, "SSServerSession-CheckBeatHeart", check_beat_heart);

        if (IsConnectType())
        {
            timer_register.AddRepeatTimer((UInt32)TimerID.SEND_BEAT_HEART_TIMER_ID, (long)TimerDelay.SEND_BEAT_HEART_TIMER_DELAY, "SSServerSession-SendBeatHeart", send_beat_heart);
        }
    }

    private void check_beat_heart(object[] paras)
    {
        long now = Util.GetMillSecond();
        if ((last_beat_heart_time + beat_heart_max_time) < now)
        {
            Log.InfoAf("[SSServerSession] SessionID={0} ServerID={1},ServerType={2}  BeatHeart Exception", GetSessID(), remote_server_id, remote_server_type_str);
            Terminate();
        }
    }

    private void send_beat_heart(object[] paras)
    {
        s2s_server_session_ping req = new s2s_server_session_ping();
        SendProtoMsg((UInt32)S2SBaseMsgId.S2SServerSessionPingId, req);
        Log.DebugAf("[SSServerSession] SessionID={0} ServerID={1},ServerType={2} Send Ping", GetSessID(), remote_server_id, remote_server_type_str);
    }

    public UInt64 GetRemoteServerID()
    {
        return remote_server_id;
    }

    public void SetRemoteServerID(UInt64 _remote_server_id)
    {
        remote_server_id = _remote_server_id;
    }

    public UInt32 GetRemoteServerType()
    {
        return remote_server_type;
    }

    public void SetRemoteServerType(UInt32 _remote_server_type)
    {
        remote_server_type = _remote_server_type;
    }

    public string GetRemoteServerTypeStr()
    {
        return remote_server_type_str;
    }

    public void SetRemoteServerTypeStr(string _remote_server_type_str)
    {
        remote_server_type_str = _remote_server_type_str;
    }

    public ILogicServer GetLogicServer()
    {
        return logic_server;
    }

    public void SetLogicServer(ILogicServer _logic_server)
    {
        logic_server = _logic_server;
        logic_server.Init(); 
    }

    public void SetRemoteToken(string _remote_token)
    {
        remote_token = _remote_token;
    }

    void SendBytes(UInt32 msgID, byte[] datas, IAttachParas attach = null)
    {
        AsyncSendMsg(msgID, datas, attach);
    }

    void SendProtoMsg(UInt32 msgID, IMessage message, IAttachParas attach = null)
    {
        AsyncSendProtoMsg(msgID, message, attach);
    }

    private ITimerRegister timer_register = new TimerRegister();
    private UInt64 remote_server_id = 0;
    private UInt32 remote_server_type = 0;
    private string remote_server_type_str = "";
    private ILogicServer logic_server = null;
    private string remote_token = "";
    private SessionState session_state = SessionState.VERIFY;
    private long last_beat_heart_time = 0;
    private long beat_heart_max_time = 1000 * 60 * 3;

    private enum SessionState
    {
        VERIFY = 1,
        ESTABLISH = 2,
        CLOSE = 3,
    }
    private enum TimerID : UInt32
    {
        CHECK_BEAT_HEART_TIMER_ID = 1,
        SEND_BEAT_HEART_TIMER_ID = 2,
    }

    private enum TimerDelay : long
    {
        CHECK_BEAT_HEART_TIMER_DELAY = 1000 * 10,
        SEND_BEAT_HEART_TIMER_DELAY = 1000 * 30,
    }
}

public class SSServerSessionMgr : ISessionfactory
{
    public void Init(ILogicServerFactory factory)
    {
        logic_server_factory = factory;
        timer_register.AddRepeatTimer((UInt32)TimerID.MGR_OUTPUT_TIMER_ID, (long)TimerDelay.MGR_OUTPUT_TIMER_DELAY, "SSServerSessionMgr-OutPut", out_put);
    }

    public bool IsInConnectCache(UInt64 server_id)
    {
        return connect_cache_dict.ContainsKey(server_id);
    }

    public bool IsExistSessionOfSrvID(UInt64 server_id)
    {
        foreach (KeyValuePair<UInt64, SSServerSession> kv in session_dict)
        {
            if (kv.Value.GetRemoteServerID() == server_id)
            {
                return true;
            }
        }

        return false;
    }

    public void Connect(UInt64 remote_server_id, UInt32 remote_server_type, string remote_server_type_str, string remote_ip, UInt32 remote_port, string remote_token)
    {
        SSServerSession session = (SSServerSession)CreateSession();
        session.SetRemoteServerID(remote_server_id);
        session.SetRemoteServerType(remote_server_type);
        session.SetRemoteServerTypeStr(remote_server_type_str);
        session.SetRemoteToken(remote_token);
        session.SetConnectType();

        ConnectCache cache = new ConnectCache(session.GetSessID(), remote_server_id, remote_server_type, remote_server_type_str, Util.GetMillSecond() + mgr_beat_heart_max_time);
        connect_cache_dict[remote_server_id] = cache;
        Log.InfoAf("[SSServerSessionMgr] ConnectCache Add SessionID={0},ServerID={1}", session.GetSessID(), remote_server_id);

        Net.Instance.Connect(remote_ip, remote_port, session);
    }

    public void Listen(string ip, UInt32 port, int listen_max_count)
    {
        Net.Instance.Listen(ip, port, this, listen_max_count);
    }

    public List<ILogicServer> GetLogicSrvVecBySrvType(UInt32 server_type)
    {
        List<ILogicServer> logic_server_list = null;
        foreach (KeyValuePair<UInt64, SSServerSession> kv in session_dict)
        {
            if (kv.Value.GetRemoteServerType() == server_type)
            {
                logic_server_list.Add(kv.Value.GetLogicServer());
            }
        }
        return logic_server_list;
    }

    public UInt64 GetSessIDBySrvTypeAndHashId(UInt32 server_type, UInt64 hash_id)
    {
        List<ILogicServer> logic_server_list = GetLogicSrvVecBySrvType(server_type);
        if (logic_server_list.Count == 0)
        {
            return 0;
        }

        UInt64 index = hash_id % (UInt64)logic_server_list.Count;
        return logic_server_list[(int)index].GetSession().GetSessID();
    }

    public bool SendProtoMsgBySrvId(UInt64 server_id, UInt32 msg_id, IMessage msg, IAttachParas attach = null)
    {
        var session = FindSessionByServerID(server_id);
        if (session == null)
        {
            return false;
        }

        return session.AsyncSendProtoMsg(msg_id, msg, attach);
    }

    public bool SendProtoMsgBySessId(UInt64 session_id, UInt32 msg_id, IMessage msg, IAttachParas attach = null)
    {
        var session = FindSessionBySessionID(session_id);
        if (session == null)
        {
            return false;
        }

        return session.AsyncSendProtoMsg(msg_id, msg, attach);
    }

    public bool SendBytesMsgBySrvId(UInt64 server_id, UInt32 msg_id, byte[] datas, IAttachParas attach = null)
    {
        var session = FindSessionByServerID(server_id);
        if (session == null)
        {
            return false;
        }

        return session.AsyncSendMsg(msg_id, datas, attach);
    }

    public bool SendBytesMsgBySessId(UInt64 session_id, UInt32 msg_id, byte[] datas, IAttachParas attach = null)
    {
        var session = FindSessionBySessionID(session_id);
        if (session == null)
        {
            return false;
        }

        return session.AsyncSendMsg(msg_id, datas, attach);
    }

    public void BroadCastBytesMsgBySrvType(UInt32 server_type, UInt32 msg_id, byte[] datas, IAttachParas attach = null)
    {
        foreach (KeyValuePair<UInt64, SSServerSession> kv in session_dict)
        {
            if (kv.Value.GetRemoteServerType() == server_type)
            {
                kv.Value.AsyncSendMsg(msg_id, datas, attach);
            }
        }
    }

    public void BroadCastProtoMsgBySrvType(UInt32 server_type, UInt32 msg_id, IMessage msg, IAttachParas attach = null)
    {
        foreach (KeyValuePair<UInt64, SSServerSession> kv in session_dict)
        {
            if (kv.Value.GetRemoteServerType() == server_type)
            {
                kv.Value.AsyncSendProtoMsg(msg_id, msg, attach);
            }
        }
    }

    public bool SendProtoMsgByHashIdAndSrvType(UInt32 server_type, UInt64 hash_id, UInt32 msg_id, IMessage msg, IAttachParas attach = null)
    {
        var logic_server_list = GetLogicSrvVecBySrvType(server_type);
        if (logic_server_list.Count == 0)
        {
            return false;
        }

        UInt64 index = hash_id % (UInt64)logic_server_list.Count;
        return logic_server_list[(int)index].GetSession().AsyncSendProtoMsg(msg_id, msg, attach);
    }

    public void AddSession(ISession sess)
    {
        UInt64 session_id = sess.GetSessID();
        session_dict[session_id] = (SSServerSession)sess;

        Log.InfoAf("[SSServerSessionMgr] AddSession SessionID={0}", session_id);

        if (connect_cache_dict.ContainsKey(session_id))
        {
            Log.InfoAf("[SSServerSessionMgr] AddSession Triggle ConnectCache Del SessionID={0},ServerID={1}", session_id, connect_cache_dict[session_id].ServerID);
            connect_cache_dict.Remove(session_id);
        }
    }

    public ISession CreateSession()
    {
        ++session_id;
        Coder coder = new Coder();

        SSServerSession session = new SSServerSession();
        session.SetSessID(session_id);
        session.SetCoder(coder);
        session.SetSessionFactory(this);
        Log.InfoAf("[SSServerSessionMgr] CreateSession={0}", session.GetSessID());
        return session;
    }

    public void RemoveSession(UInt64 session_id)
    {
        if (session_dict.ContainsKey(session_id))
        {
            Log.InfoAf("[SSServerSessionMgr] RemoveSession SessionID={0} ServerID={1}", session_id, session_dict[session_id].GetRemoteServerID());
            session_dict.Remove(session_id);
        }
    }

    private SSServerSession FindSessionBySessionID(UInt64 session_id)
    {
        if (session_dict.ContainsKey(session_id))
        {
            return session_dict[session_id];
        }

        return null;
    }

    public ILogicServerFactory GetLogicServerFactory()
    {
        return logic_server_factory;
    }

    private SSServerSession FindSessionByServerID(UInt64 server_id)
    {
        foreach (KeyValuePair<UInt64, SSServerSession> kv in session_dict)
        {
            if (kv.Value.GetRemoteServerID() == server_id)
            {
                return kv.Value;
            }
        }

        return null;
    }

    private void out_put(object[] state)
    {
        foreach (KeyValuePair<UInt64, SSServerSession> kv in session_dict)
        {
            Log.InfoAf("[SSServerSessionMgr] OutPut ServerID={0},ServerType={1}", kv.Value.GetRemoteServerID(), kv.Value.GetRemoteServerType());
        }

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
                Log.InfoAf("[SSServerSessionMgr] Timeout Triggle ConnectCache Del SessionID={0},ServerID={1}", remove_list[i], connect_cache_dict[remove_list[i]].ServerID);
                connect_cache_dict.Remove(remove_list[i]);
            }
        }
    }

    private UInt64 session_id = 0;
    private Dictionary<UInt64, SSServerSession> session_dict = new Dictionary<UInt64, SSServerSession>();
    private ILogicServerFactory logic_server_factory = null;
    private ITimerRegister timer_register = new TimerRegister();
    private Dictionary<UInt64, ConnectCache> connect_cache_dict = new Dictionary<UInt64, ConnectCache>();
    private long mgr_beat_heart_max_time = 1000 * 10;

    private class ConnectCache
    {
        public ConnectCache(UInt64 session_id, UInt64 server_id, UInt32 server_type, string server_type_str, Int64 connect_tick)
        {
            SessionID = session_id;
            ServerID = server_id;
            ServerType = server_type;
            ServerTypeStr = server_type_str;
            ConnectTick = connect_tick;
        }

        public UInt64 SessionID;
        public UInt64 ServerID;
        public UInt32 ServerType;
        public string ServerTypeStr;
        public Int64 ConnectTick;
    }

    private enum TimerID : UInt32
    {
        MGR_OUTPUT_TIMER_ID = 1,
    }

    private enum TimerDelay : long
    {
        MGR_OUTPUT_TIMER_DELAY = 1000 * 60,
    }
}