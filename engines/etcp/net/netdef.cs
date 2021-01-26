using System;

namespace Framework.ETcp
{
    public class NetDef
    {
        public const int MSG_BUFF_SIZE = 1024 * 64;

        public const int MSG_SEND_BUFF_SIZE = MSG_BUFF_SIZE * 2;

        public const int MSG_RECV_BUFF_SIZE = MSG_BUFF_SIZE * 2;

        public const int LISTEN_MAX_COUNT = 20000;       
    }


    public enum ConnectionState : long
    {
        CONNECTING = 0,
        ESTABLISHED = 1,
        CLOSING = 2,
        CLOSED = 3,
    }
}
