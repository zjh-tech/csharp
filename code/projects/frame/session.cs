using System;
using Framework.ETcp;
using Google.Protobuf;
using System.IO;

public abstract class Session :ISession
{
    public void SetConnection(IConnection _conn)
    {
        conn = _conn; 
    }

    public UInt64 GetSessID()
    {
        return session_id;
    }

    public void SetSessID(UInt64 _session_id)
    {
        session_id = _session_id;
    }

    public ICoder GetCoder()
    {
        return coder; 
    }

    public void SetCoder(ICoder _coder)
    {
        coder = _coder; 
    }

    public bool IsListenType()
    {
        return (session_type == SessionType.LISTEN) ? true : false;  
    }

    public bool IsConnectType()
    {
        return (session_type == SessionType.CONNECT) ? true : false; 
    }

    public void SetConnectType()
    {
        session_type = SessionType.CONNECT; 
    }

    public void SetListenType()
    {
        session_type = SessionType.LISTEN; 
    }

    public void SetSessionFactory(ISessionfactory factory)
    {
        session_factory = factory;
    }

    public ISessionfactory GetSessionFactory()
    {
        return session_factory; 
    }

    public bool AsyncSendMsg(UInt32 msg_id, byte[] datas, IAttachParas attach)
    {
        MemoryStream memstream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(memstream);

        byte[] msg_id_bytes = System.BitConverter.GetBytes(msg_id);   
        Array.Reverse(msg_id_bytes);                             
        writer.Write(msg_id_bytes);

        byte[] attach_datas = null;
        if (attach != null)
        {
            attach_datas = attach.FillNetStream();
        }

        if(attach_datas != null)
        {
            byte[] attach_len_bytes = System.BitConverter.GetBytes((UInt16)attach_datas.Length);
            Array.Reverse(attach_len_bytes);
            writer.Write(attach_len_bytes);
            writer.Write(attach_datas); 
        }
        else
        {
            byte[] attach_len_bytes = System.BitConverter.GetBytes((UInt16)0);
            Array.Reverse(attach_len_bytes);
            writer.Write(attach_len_bytes);
        }

        if(datas != null && datas.Length != 0)
        {
            writer.Write(datas);
        }

        byte[] out_content;
        coder.FillNetStream(memstream.ToArray(),out out_content);
        conn.AsyncSend(out_content);
            
        return true; 
    }

    public bool AsyncSendProtoMsg(UInt32 msg_id, IMessage message, IAttachParas attach)
    {      
        if(message != null)
        {
            //需优化ToByteArray性能   
            //https://www.yht7.com/news/36972
            byte[] datas = message.ToByteArray();            
            AsyncSendMsg(msg_id, datas, attach);
        }
        else
        {
            AsyncSendMsg(msg_id, null,attach);
        }       
        return true; 
    }

    public void ProcessMsg(byte[] datas)
    {
        MemoryStream memstream = new MemoryStream(datas);
        BinaryReader reader = new BinaryReader(memstream);

        byte[] msg_id_bytes = reader.ReadBytes(4);
        Array.Reverse(msg_id_bytes);
        UInt32 msg_id = BitConverter.ToUInt32(msg_id_bytes);
        
        byte[] attach_len_bytes = reader.ReadBytes(2);
        Array.Reverse(attach_len_bytes);
        UInt16 attach_len = BitConverter.ToUInt16(attach_len_bytes);
        
        if(attach_len != 0)
        {
            byte[] attach_datas = reader.ReadBytes(attach_len);
            OnHandlerMsg(msg_id, attach_datas,reader.ReadBytes(datas.Length - (sizeof(UInt32) + attach_len)));
        }
        else
        {
            OnHandlerMsg(msg_id, null, reader.ReadBytes(datas.Length - sizeof(UInt32)));
        }
    }

    public abstract void OnHandlerMsg(UInt32 msg_id,byte[] attach_datas,byte[] datas);

    public void Terminate()
    {
        conn.Terminate(); 
    }

    public abstract void OnEstablish();


    public abstract void OnTerminate();
    

    private UInt64 session_id = 0;
    private IConnection conn = null;    
    private ICoder coder = null;
    private SessionType session_type = SessionType.LISTEN;
    private ISessionfactory session_factory = null; 
}