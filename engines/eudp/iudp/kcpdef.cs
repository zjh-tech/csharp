using System;

namespace Engine
{
    public class KcpDef
    {
        public const string KcpVerifyReq = "KcpVerifyReq";
        public const int KcpVerifyTokenLen = 32;            //string to byte[] length

        public const string KcpVerifyRes = "KcpVerifyRes";
        public const int KcpConvLen = sizeof(uint);

        public const int KcpHeartBeatCount = 3;
        public const int KcpMaxVerifyDiffTime = 1000 * 30;

        public const UInt32 KcpHeartBeatReqId = 1;
        public const UInt32 KcpHeartBeatResId = 2;
        public const UInt32 KcpPressureId = 3;
    }
}
