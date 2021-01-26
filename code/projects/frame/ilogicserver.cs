using System;

public abstract class ILogicServer
{
    public abstract void Init();

    public abstract void OnEstablish(SSServerSession sess);

    public abstract void OnTerminate(SSServerSession sess);

    public abstract void OnHandlerMsg(SSServerSession sess, UInt32 msgID, byte[] attach_datas, byte[] datas); 

    public void SetSession(SSServerSession _sess)
    {
        session = _sess; 
    }

    public SSServerSession GetSession()
    {
        return session; 
    }

    private SSServerSession session = null; 
}


public abstract class ILogicServerFactory
{
    public abstract void SetLogicServer(SSServerSession sess); 
}