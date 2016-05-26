using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace RTU
{
    /// <summary>
    /// RTU 기능정의 추상 클래스
    /// </summary>
    public abstract class RTU_TY : IRTU_TY
    {
        #region --- Fields ---
        protected bool disposed = false;
        protected SerialPort _port = null;

        /// <summary>
        /// Thread-Safe Received Queue
        /// </summary>
        protected ConcurrentQueue<byte> rxFIFO = new ConcurrentQueue<byte>();

        /// <summary>
        /// Interlock mutex
        /// </summary>
        protected Mutex rxBufferBusy = new Mutex();

        #endregion

        #region --- Constructors ---
        protected RTU_TY ()
		{
			if (_port == null) 
			{
				_port = new SerialPort ();
				_port.Parity = Parity.None;
				_port.StopBits = StopBits.One;
				_port.DtrEnable = false;
				_port.RtsEnable = false;
				_port.Handshake = Handshake.None;

                _port.ReadTimeout = SerialPort.InfiniteTimeout;
				_port.Encoding = Encoding.UTF8;
			}
		}

		public RTU_TY(PortConfig cfg)
			:this()
		{
			_port.PortName = cfg.Name;
			_port.BaudRate = cfg.bRate;//9600
			_port.DataBits = cfg.dBits;
			//RTU_HWR();
		}

        public RTU_TY(string portName)
			: this()
		{
			_port.PortName = portName;
			_port.BaudRate = 115200;
			_port.DataBits = 8;
            _port.DataReceived += DataReceived;
		}
        #endregion

        #region --- Properties ---

        #endregion

        #region --- Events ---
        //public delegate void DataReceiveHandler(byte[] _msg);
        //public event DataReceiveHandler<byte[]> RTUReceived;

        /// <summary>
        /// 외부로 연결된 이벤트 핸들러
        /// </summary>
        public event EventHandler RTUReceived;
        #endregion

        #region --- Methods ---

        /// <summary>
        /// 인터페이스 구현부
        /// </summary>
        #region --- Interface Implementation ---
        ~RTU_TY()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //!< CA1063
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {

            }
            disposed = true;
        }

        public abstract void SendCommand(byte[] _bytes);

        public virtual bool Connect()
        {
            _port.Open();
            if (_port.IsOpen)
            {
                _port.DiscardInBuffer();
                _port.DiscardOutBuffer();
                
            }
            return _port.IsOpen;
        }

        public bool IsConnected()
		{
			if (_port != null) {
				if (_port.IsOpen)
					return true;
			}
			return false;
		}

        public abstract void Reset();

        public abstract bool Initiate();
        #endregion

        /// <summary>
        /// 메시지의 체크섬 확인
        /// </summary>
        /// <param name="_buf">체크섬 게산 대상</param>
        /// <returns>byte 형의 체크섬 결과</returns>
        protected abstract byte GetChecksum(ref byte[] _buf);

        //!< Byte 배열에 대한 처리만 수행한다.
        /// <summary>
        /// SerialPort의 데이터 도착 이벤트 핸들러
        /// </summary>
        /// <param name="sender">event snder</param>
        /// <param name="e">Event</param>
        protected virtual void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string _completedMsg = "";
            if (_port.IsOpen)
            {
                try
                {
                    rxBufferBusy.WaitOne();
                    while (_port.BytesToRead > 0)
                    {
                        int bytesread = _port.BytesToRead;
                        if (bytesread >= 0)
                        {
                            byte[] buffer = new byte[bytesread];
                            _port.Read(buffer, 0, bytesread);

                            for (int i = 0; i < bytesread; i++)
                                rxFIFO.Enqueue(buffer[i]);
                        }
                    }
                    if (rxFIFO.Count >= 8)//!< if reached minimum required buffer
                        OnRTUReceived(EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    //MessageBox.Show(ex.StackTrace);
                }
                finally
                {
                    rxBufferBusy.ReleaseMutex();
                }
            }

        }

        /// <summary>
        /// DataReceived로부터 호출되는 실체 핸들러
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnRTUReceived(EventArgs e)
        {
            EventHandler handler = RTUReceived;
            if (handler != null)
                handler(this, e);
        }
        #endregion
    }
}