namespace Engine
{
    public class KcpHeartBeatReqMsg : UdpMsg
    {
        public KcpHeartBeatReqMsg()
        {
            msgid = KcpDef.KcpHeartBeatReqId;
        }
    }

    public class KcpHeartBeatResMsg : UdpMsg
    {
        public KcpHeartBeatResMsg()
        {
            msgid = KcpDef.KcpHeartBeatResId;
        }
    }

    public class KcpPressureMsg : UdpMsg
    {
        public KcpPressureMsg()
        {
            msgid = KcpDef.KcpPressureId;
        }
    }
}
