namespace Framework.ETcp
{
    public class Event : IEvent
    {
        private EventType evt_type;
        private IConnection conn;
        private object datas;

        public Event(EventType _evt_type, IConnection _conn, object _datas)
        {
            evt_type = _evt_type;
            conn = _conn;
            datas = _datas;
        }

        public EventType GetEventType()
        {
            return evt_type;
        }

        public IConnection GetConn()
        {
            return conn;
        }

        public object GetDatas()
        {
            return datas;
        }
    }
}
