using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Framework.ELog;

namespace Framework.ETcp
{
    public class ListenParas
    {
        public ListenParas(Socket _socket, ISessionfactory _factory, string _host, UInt32 _port)
        {
            ServerSocket = _socket;
            Factory = _factory;
            Host = _host;
            Port = _port;
        }

        public Socket ServerSocket;
        public ISessionfactory Factory;
        public string Host;
        public UInt32 Port;
    }

    public class ConnectParas
    {
        public ConnectParas(Socket _socket, string _host, UInt32 _port, IConnection _conn)
        {
            ClientSocket = _socket;
            Host = _host;
            Port = _port;
            Conn = _conn;
        }

        public Socket ClientSocket;
        public string Host;
        public UInt32 Port;
        public IConnection Conn;
    }

    public class Net : INet
    {
        public bool Init()
        {
            event_queue = new Queue<IEvent>();
            conn_mgr = new Connectionmgr();
            return true;
        }

        public void PushEvent(IEvent evt)
        {
            event_queue.Enqueue(evt);
        }

        public bool Run(int count)
        {
            bool busy = false;

            if (event_queue.Count == 0)
            {
                return busy; 
            }

            if(event_queue.Count < count)
            {
                count = event_queue.Count; 
            }

            for (int i = 0; i < count; ++i)
            {                
                IEvent evt = event_queue.Dequeue();
                if (evt == null)
                {
                    continue;
                }

                IConnection conn = evt.GetConn();
                ISession session = conn.GetSession();

                if (evt.GetEventType() == EventType.ConnEstablishType)
                {
                    session.SetConnection(conn);
                    session.OnEstablish();                    
                }
                else if (evt.GetEventType() == EventType.ConnRecvMsgType)
                {
                    session.ProcessMsg((byte[])evt.GetDatas());
                }
                else if (evt.GetEventType() == EventType.ConnCloseType)
                {
                    session.OnTerminate();
                    session.SetConnection(null);
                }
            }

            return busy;
        }

        public bool Connect(string host, UInt32 port, ISession session)
        {            
            if (session == null)
            {
                Log.ErrorAf("[Net] Host={0},Port={1} Connect Session Error", host, port);
                return false;
            }         

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if(socket == null)
            {
                Log.ErrorAf("[Net] Host={0},Port={1} Connect Socket Error", host, port);
                return false; 
            }

            IConnection conn = conn_mgr.Create(this, socket, session);
            if (conn == null)
            {
                Log.ErrorAf("[Net] Host={0},Port={1} Connect Connection Error", host, port);
                return false;
            }

            IPEndPoint connect_endpoint = new IPEndPoint(IPAddress.Parse(host), (int)port);
            if(connect_endpoint == null)
            {
                Log.ErrorAf("[Net] Host={0},Port={1} Connect IPEndPoint Error", host, port);
                return false; 
            }

            ConnectParas conn_paras = new ConnectParas(socket, host, port, conn);
            if(conn_paras == null)
            {
                Log.ErrorAf("[Net] Host={0},Port={1} Connect ConnectParas Error", host, port);
                return false; 
            }
                        
            try
            {
                socket.BeginConnect(connect_endpoint, on_begin_connect, conn_paras);
            }
            catch (System.Exception ex)
            {
                Log.ErrorAf("[Net] Host={0},Port={1} BeginConnect Error={2}", host, port, ex.ToString());
                return false;
            }            
            return true;
        }

        private void on_begin_connect(IAsyncResult result)
        {            
            ConnectParas conn_paras = (ConnectParas)result.AsyncState;
            if (conn_paras == null)
            {
                Log.ErrorA("[Net] on_begin_connect ConnectParas = null");
                return;
            }

            if (conn_paras.ClientSocket == null)
            {
                Log.ErrorA("[Net] on_begin_connect ConnectParas.ClientSocket = null");
                return;
            }
            
            try
            {
                conn_paras.ClientSocket.EndConnect(result);
            }
            catch (System.Exception ex)
            {
                Log.ErrorAf("[Net] Host={0},Port={1} EndConnect Error={2}", conn_paras.Host, conn_paras.Port, ex.ToString());
                return;
            }            

            conn_paras.Conn.DoAsyncReceive();
        }

        public bool Listen(string host, UInt32 port, ISessionfactory factory, int listen_max_count)
        {
            if (factory == null)
            {
                return false;
            }

            if (listen_max_count == 0)
            {
                listen_max_count = NetDef.LISTEN_MAX_COUNT;
            }

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (socket == null)
            {
                Log.ErrorAf("[Net] Host={0} Port={1} Listen Socket Error", host, port);
                return false;
            }

            IPEndPoint listen_endpoint = new IPEndPoint(IPAddress.Parse(host), (int)port);
            try
            {
                socket.Bind(listen_endpoint);
                socket.Listen(listen_max_count);
                ListenParas paras = new ListenParas(socket, factory, host, port);
                socket.BeginAccept(on_accept, paras);
            }
            catch (System.Exception ex)
            {
                Log.ErrorAf("[Net] Host={0} Port={1} BeginAccept Error={0}", ex.ToString());
                return false;
            }

            return true;
        }

        private void on_accept(IAsyncResult result)
        {
            ListenParas paras = (ListenParas)result.AsyncState;
            if (paras.ServerSocket == null)
            {
                return;
            }

            Socket client_socket = null;

            try
            {
                client_socket = paras.ServerSocket.EndAccept(result);
            }
            catch (System.Exception ex)
            {
                Log.ErrorAf("[Net] Host={0} Port={1} EndAccept Error {2}", paras.Host, paras.Port, ex.ToString());
                return;
            }

            process_accept(client_socket, paras.Factory);

            Log.InfoAf("[Net] Host={0} Port={1} Accept Connection Success", paras.Host, paras.Port);
            try
            {
                paras.ServerSocket.BeginAccept(on_accept, paras);
            }
            catch (System.Exception ex)
            {
                Log.ErrorAf("[Net] Host={0} Port={1} BeginAccept Next Error {2}", paras.Host, paras.Port, ex.ToString());
                return;
            }
        }

        private void process_accept(Socket client_socket, ISessionfactory factory)
        {
            if (client_socket == null || factory == null)
            {
                return;
            }

            ISession session = factory.CreateSession();
            if (session == null)
            {
                return;
            }

            IConnection conn = conn_mgr.Create(this, client_socket, session);
            if (conn == null)
            {
                return;
            }
            conn.DoAsyncReceive();
        }

        private IConnectionMgr conn_mgr = null;
        private Queue<IEvent> event_queue = null;

        public static Net Instance = new Net();
    }
}
