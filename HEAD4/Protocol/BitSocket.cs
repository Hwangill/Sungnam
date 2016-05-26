using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Protocol
{
    public class BitSocketClient : IDisposable
    {
        #region Consts/Default values
        const int SOCKETACTIONTIMEOUTE = 45000;
        const int DEFAULTTIMEOUT = 5000; //Default to 5 seconds on all timeouts
        const int RECONNECTINTERVAL = 10000; //Default to 2 seconds reconnect attempt rate
        #endregion
        #region Components, Events, Delegates, and CTOR
        //Timer used to detect receive timeouts
        private System.Timers.Timer tmrReceiveTimeout = new System.Timers.Timer();
        private System.Timers.Timer tmrSendTimeout = new System.Timers.Timer();
        private System.Timers.Timer tmrConnectTimeout = new System.Timers.Timer();
        public delegate void delDataReceived(BitSocketClient sender, byte[] data);
        public event delDataReceived DataReceived;
        public delegate void delDataSended(BitSocketClient sender, byte[] data);
        public event delDataReceived DataSended;
        public delegate void delConnectionStatusChanged(BitSocketClient sender, ConnectionStatus status);
        public event delConnectionStatusChanged ConnectionStatusChanged;
        public enum ConnectionStatus
        {
            NeverConnected,
            Connecting,
            Connected,
            AutoReconnecting,
            DisconnectedByUser,
            DisconnectedByHost,
            ConnectFail_Timeout,
            ReceiveFail_Timeout,
            SendFail_Timeout,
            SendFail_NotConnected,
            Error
        }
        public List<IPEndPoint> multiServer = new List<IPEndPoint>();
        private int serverRotate = 0;
        public BitSocketClient(IPAddress ip, int port, bool autoreconnect = true)
        {
            IPEndPoint baseServer = new IPEndPoint(ip, port);
            multiServer.Add(baseServer);

            this._IP = baseServer.Address;
            this._Port = baseServer.Port;
            this._AutoReconnect = autoreconnect;
            this._client = new TcpClient(AddressFamily.InterNetwork);
            this._client.NoDelay = true; //Disable the nagel algorithm for simplicity
            ReceiveTimeout = SOCKETACTIONTIMEOUTE;
            SendTimeout = DEFAULTTIMEOUT;
            ConnectTimeout = DEFAULTTIMEOUT;
            ReconnectInterval = RECONNECTINTERVAL;
            tmrReceiveTimeout.AutoReset = false;
            tmrReceiveTimeout.Elapsed += new System.Timers.ElapsedEventHandler(tmrReceiveTimeout_Elapsed);
            tmrConnectTimeout.AutoReset = false;
            tmrConnectTimeout.Elapsed += new System.Timers.ElapsedEventHandler(tmrConnectTimeout_Elapsed);
            tmrSendTimeout.AutoReset = false;
            tmrSendTimeout.Elapsed += new System.Timers.ElapsedEventHandler(tmrSendTimeout_Elapsed);
            //tmrReceiveTimeout.Enabled = false;
            //tmrSendTimeout.Enabled = false;
            ConnectionState = ConnectionStatus.NeverConnected;

        }
        #endregion
        #region Private methods/Event Handlers
        void tmrSendTimeout_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Send Timeout Raised");
            this.ConnectionState = ConnectionStatus.SendFail_Timeout;
            DisconnectByHost();
        }
        void tmrReceiveTimeout_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Recv Timeout Raised");
            this.ConnectionState = ConnectionStatus.ReceiveFail_Timeout;
            DisconnectByHost();
        }
        void tmrConnectTimeout_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Connect Timeout Raised");
            ConnectionState = ConnectionStatus.ConnectFail_Timeout;
            DisconnectByHost();
        }
        private void DisconnectByHost()
        {
            this.ConnectionState = ConnectionStatus.DisconnectedByHost;
            tmrReceiveTimeout.Stop();
            tmrConnectTimeout.Stop();
            if (AutoReconnect)
                Reconnect();
        }
        private void Reconnect()
        {
            if (this.ConnectionState == ConnectionStatus.Connected)
                return;
            
            try
            {
                if (this.ConnectionState == ConnectionStatus.DisconnectedByHost
                    || this.ConnectionState == ConnectionStatus.DisconnectedByUser)
                {
                    Connect();
                }
                else
                {
                    this.ConnectionState = ConnectionStatus.AutoReconnecting;
                    this._client.Client.BeginDisconnect(true, new AsyncCallback(cbDisconnectByHostComplete), this._client.Client);
                }
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch { }
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// Try connecting to the remote host
        /// </summary>
        public void Connect()
        {
            if (this.ConnectionState == ConnectionStatus.Connected)
                return;
            try
            {
                Console.WriteLine("Try Connect....");
                this.ConnectionState = ConnectionStatus.Connecting;
                tmrConnectTimeout.Start();
                this._client.BeginConnect(multiServer[serverRotate].Address, multiServer[serverRotate].Port, new AsyncCallback(cbConnect), this._client.Client);
                serverRotate++;
                if (serverRotate >= multiServer.Count)
                    serverRotate = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Try disconnecting from the remote host
        /// </summary>
        public void Disconnect()
        {
            try
            {
                this.ConnectionState = ConnectionStatus.DisconnectedByUser;
                tmrConnectTimeout.Stop();
                tmrReceiveTimeout.Stop();
                this._client.Client.BeginDisconnect(true, new AsyncCallback(cbDisconnectComplete), this._client.Client);
            }
            catch (Exception exe)
            {
            }
        }
        /// <summary>
        /// Try sending a string to the remote host
        /// </summary>
        /// <param name="data">The data to send</param>
        public void Send(string data)
        {
            if (this.ConnectionState != ConnectionStatus.Connected)
            {
                this.ConnectionState = ConnectionStatus.SendFail_NotConnected;
                return;
            }
            var bytes = _encode.GetBytes(data);
            SocketError err = new SocketError();
            tmrSendTimeout.Stop();
            tmrSendTimeout.Start();
            this._client.Client.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, out err, new AsyncCallback(cbSendComplete), this._client.Client);
            if (err != SocketError.Success)
            {
                Action doDCHost = new Action(DisconnectByHost);
                doDCHost.Invoke();
            }
        }
        /// <summary>
        /// Try sending byte data to the remote host
        /// </summary>
        /// <param name="data">The data to send</param>
        public void Send(byte[] data)
        {
            if (this.ConnectionState != ConnectionStatus.Connected)
                Connect();
            //throw new InvalidOperationException("Cannot send data, socket is not connected");
            SocketError err = new SocketError();
            this._client.Client.BeginSend(data, 0, data.Length, SocketFlags.None, out err, new AsyncCallback(cbSendComplete), this._client.Client);
            if (err != SocketError.Success)
            {
                Console.WriteLine("Send Error" + err.ToString());
                Action doDCHost = new Action(DisconnectByHost);
                doDCHost.Invoke();
            }
            else
            {
                if (DataSended != null && data != null)
                {
                    DataSended.BeginInvoke(this, data, null, this);
                }
            }
            
        }

        public void Dispose()
        {
            try
            {
                tmrConnectTimeout.Stop();
                tmrReceiveTimeout.Stop();
                this._client.GetStream().Close();
                this._client.Close();
                this._client.Client.Dispose();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region --- Callbacks ---
        private void cbConnectComplete()
        {
            if (_client.Connected == true)
            {
                tmrConnectTimeout.Stop();
                tmrReceiveTimeout.Stop();
                ConnectionState = ConnectionStatus.Connected;
                this._client.Client.BeginReceive(this.dataBuffer, 0, this.dataBuffer.Length, SocketFlags.None, new AsyncCallback(cbDataReceived), this._client.Client);
            }
            else
            {
                ConnectionState = ConnectionStatus.Error;
            }
        }
        private void cbDisconnectByHostComplete(IAsyncResult result)
        {
            var r = result.AsyncState as Socket;
            if (r == null)
                throw new InvalidOperationException("Invalid IAsyncResult - Could not interpret as a socket object");
            r.EndDisconnect(result);
            if (this.AutoReconnect)
            {
                Action doConnect = new Action(Connect);
                doConnect.Invoke();
                return;
            }
        }
        private void cbDisconnectComplete(IAsyncResult result)
        {
            var r = result.AsyncState as Socket;
            if (r == null)
            {
                throw new InvalidOperationException("Invalid IAsyncResult - Could not interpret as a socket object");
            }
            r.EndDisconnect(result);
            this.ConnectionState = ConnectionStatus.DisconnectedByUser;
            try
            {
                if (this.AutoReconnect)
                {
                    Action doConnect = new Action(Connect);
                    doConnect.Invoke();
                    return;
                }
                else
                {
                    this._client.Client.Dispose();

                    this._client = new TcpClient(AddressFamily.InterNetwork);
                    this._client.NoDelay = true; //Disable the nagel algorithm for simplicity
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void cbConnect(IAsyncResult result)
        {
            var sock = result.AsyncState as Socket;
            if (result == null)
            {
                Console.WriteLine("Invalid IAsyncResult - Could not interpret as a socket object");
                //throw new InvalidOperationException("Invalid IAsyncResult - Could not interpret as a socket object");
                if (AutoReconnect)
                {
                    System.Threading.Thread.Sleep(ReconnectInterval);
                    Action reconnect = new Action(Connect);
                    reconnect.Invoke();
                    return;
                }
            }
            else
            {
                if (!sock.Connected)
                {
                    if (AutoReconnect)
                    {
                        System.Threading.Thread.Sleep(ReconnectInterval);
                        Action reconnect = new Action(Connect);
                        reconnect.Invoke();
                        return;
                    }
                    else
                        return;
                }
                sock.EndConnect(result);
                var callBack = new Action(cbConnectComplete);
                callBack.Invoke();
            }
        }
        private void cbSendComplete(IAsyncResult result)
        {
            var r = result.AsyncState as Socket;
            if (r == null)
                throw new InvalidOperationException("Invalid IAsyncResult - Could not interpret as a socket object");
            SocketError err = new SocketError();
            r.EndSend(result, out err);
            if (err != SocketError.Success)
            {
                Console.WriteLine("Send Error" + err.ToString());
                Action doDCHost = new Action(DisconnectByHost);
                doDCHost.Invoke();
            }
            else
            {
                lock (SyncLock)
                {
                    tmrSendTimeout.Stop();
                }

                
            }
        }

        private void cbChangeConnectionStateComplete(IAsyncResult result)
        {
            var r = result.AsyncState as BitSocketClient;
            if (r == null)
                throw new InvalidOperationException("Invalid IAsyncResult - Could not interpret as a EDTC object");
            r.ConnectionStatusChanged.EndInvoke(result);
        }

        private void cbDataReceived(IAsyncResult result)
        {
            var sock = result.AsyncState as Socket;
            if (sock == null)
                throw new InvalidOperationException("Invalid IASyncResult - Could not interpret as a socket");
            try
            {
                SocketError err = new SocketError();
                int bytes = sock.EndReceive(result, out err);
                if (err != SocketError.Success)//bytes가 0인 경우가 있다.
                {
                    lock (SyncLock)
                    {
                        Console.WriteLine("Recv Error : " + err.ToString());
                        tmrReceiveTimeout.Stop();
                        tmrReceiveTimeout.Start();
                        return;
                    }
                }
                else
                {
                    lock (SyncLock)
                    {
                        tmrReceiveTimeout.Stop();
                    }
                }
                if (DataReceived != null && bytes != 0)
                {
                    //DataReceived.BeginInvoke (this, _encode.GetString (dataBuffer, 0, bytes), new AsyncCallback (cbDataRecievedCallbackComplete), this);
                    byte[] recv = new byte[bytes];
                    Array.Copy(dataBuffer, 0, recv, 0, bytes);
                    DataReceived.BeginInvoke(this, recv, new AsyncCallback(cbDataRecievedCallbackComplete), this);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static string GetString(byte[] bytes, int size)
        {
            //char[] chars = new char[size / sizeof(char)];
            byte[] chars = new byte[size];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, size);
            Encoding ec = System.Text.Encoding.GetEncoding(51949);
            return ec.GetString(chars);

            //return new string(chars);
        }

        private void cbDataRecievedCallbackComplete(IAsyncResult result)
        {
            var r = result.AsyncState as BitSocketClient;
            if (r == null)
                throw new InvalidOperationException("Invalid IAsyncResult - Could not interpret as EDTC object");
            r.DataReceived.EndInvoke(result);
            SocketError err = new SocketError();
            this._client.Client.BeginReceive(this.dataBuffer, 0, this.dataBuffer.Length, SocketFlags.None, out err, new AsyncCallback(cbDataReceived), this._client.Client);
            if (err != SocketError.Success)
            {
                Action doDCHost = new Action(DisconnectByHost);
                doDCHost.Invoke();
            }
        }
        #endregion
        #region Properties and members
        private IPAddress _IP = IPAddress.None;
        private ConnectionStatus _ConStat;
        private TcpClient _client;
        private byte[] dataBuffer = new byte[5000];
        private bool _AutoReconnect = false;
        private int _Port = 0;
        private Encoding _encode = Encoding.ASCII;
        object _SyncLock = new object();
        /// <summary>
        /// Syncronizing object for asyncronous operations
        /// </summary>
        public object SyncLock
        {
            get
            {
                return _SyncLock;
            }
        }
        /// <summary>
        /// Encoding to use for sending and receiving
        /// </summary>
        public Encoding DataEncoding
        {
            get
            {
                return _encode;
            }
            set
            {
                _encode = value;
            }
        }
        /// <summary>
        /// Current state that the connection is in
        /// </summary>
        public ConnectionStatus ConnectionState
        {
            get
            {
                return _ConStat;
            }
            private set
            {
                bool raiseEvent = value != _ConStat;
                _ConStat = value;
                if (ConnectionStatusChanged != null && raiseEvent)
                    ConnectionStatusChanged.BeginInvoke(this, _ConStat, new AsyncCallback(cbChangeConnectionStateComplete), this);
            }
        }
        /// <summary>
        /// True to autoreconnect at the given reconnection interval after a remote host closes the connection
        /// </summary>
        public bool AutoReconnect
        {
            get
            {
                return _AutoReconnect;
            }
            set
            {
                _AutoReconnect = value;
            }
        }
        public int ReconnectInterval { get; set; }
        /// <summary>
        /// IP of the remote host
        /// </summary>
        public IPAddress IP
        {
            get
            {
                return _IP;
            }
        }
        /// <summary>
        /// Port to connect to on the remote host
        /// </summary>
        public int Port
        {
            get
            {
                return _Port;
            }
        }
        /// <summary>
        /// Time to wait after a receive operation is attempted before a timeout event occurs
        /// </summary>
        public int ReceiveTimeout
        {
            get
            {
                return (int)tmrReceiveTimeout.Interval;
            }
            set
            {
                tmrReceiveTimeout.Interval = (double)value;
            }
        }
        /// <summary>
        /// Time to wait after a send operation is attempted before a timeout event occurs
        /// </summary>
        public int SendTimeout
        {
            get
            {
                return (int)tmrSendTimeout.Interval;
            }
            set
            {
                tmrSendTimeout.Interval = (double)value;
            }
        }
        /// <summary>
        /// Time to wait after a connection is attempted before a timeout event occurs
        /// </summary>
        public int ConnectTimeout
        {
            get
            {
                return (int)tmrConnectTimeout.Interval;
            }
            set
            {
                tmrConnectTimeout.Interval = (double)value;
            }
        }
        #endregion
    }
}