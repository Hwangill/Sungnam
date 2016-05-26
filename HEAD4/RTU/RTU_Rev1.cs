using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Common;

namespace RTU
{
    /// <summary>
    /// CMM1 클래스
    /// </summary>
    public class RTU_TY_Rev1 : RTU_TY
    {
        private readonly byte[] _frameSTX = new byte[] {0x02, 0xFD};
        private readonly byte _frameETX = 0x03;
        private static byte _bitDevId = 0x11;
        private static byte _rtuDevId = 0x21;

        /// <summary>
        /// 보낸 것에 대한 응답대기
        /// </summary>
        private Dictionary<DateTime, _DevId> _waitCmd = new Dictionary<DateTime, _DevId>();

        /// <summary>
        /// 응답대기 Dictionary를 처리하기 위한 Sync Obejct
        /// </summary>
        protected Mutex waitCmdBusy = new Mutex();

        /// <summary>
        /// 송수신 장치 구분
        /// </summary>
        public enum _DevId : byte
        {
            [Description("알수없음")]           UNKNOWN = 0,
            [Description("도어")]               DOOR = 0x41,
            [Description("온도/습도")]          TH = 0x42,
            [Description("조도")]               CDS,
            [Description("마이크")]              MIC,
            [Description("스피커")]              SPK,
            [Description("키버튼")]              KEYBTN,
            //!< Reserved 0x47~0x49
            [Description("팬")]                 FAN = 0x51,
            [Description("히터")]               HEATER,
            [Description("LCD")]                LCD,
            [Description("LED")]                LED,
            //!< Reserved 0x55
            [Description("성남향 모뎀")]         MODEM = 0x56,
            [Description("성남향 충격센서")]       SHK,
            [Description("성남향 BIT 상태요청")]   BITSTAT,
            //!< Reserved 0x59
            [Description("성남향 PC 전원 리셋")]   PCRESET = 0x61,
            [Description("성남향 AC 전원 예약")]   ACRESR,
            [Description("성남향 전체장치 전원")]   MAINPOWER
            //Reserved 0x64~0x69
        };

        /// <summary>
        /// 명령코드
        /// </summary>
        public enum _opCode : byte
        {
            [DescriptionAttribute("알수없음")]      UNKNOWN = 0,

            [DescriptionAttribute("???")]           NACK = 0x05,
            [DescriptionAttribute("???")]           ACK = 0x06,

            [DescriptionAttribute("???")]           LCDPWRSTATUSREQ = 0x70,
            [DescriptionAttribute("???")]           LCDPWRSETREQ,
            [DescriptionAttribute("???")]           LCDPWRSTATUSACK = 0x7A,

            [DescriptionAttribute("???")]           SPKVOLREQ = 0x80,//!< 충격센서
            [DescriptionAttribute("???")]           SPKVOLSETREQ = 0x81,//!< 충격센서
            [DescriptionAttribute("???")]           SPKMUTEREQ = 0x82,
            [DescriptionAttribute("???")]           SPKMUTESET = 0x83,
            [DescriptionAttribute("???")]           SPKVOLACK = 0x8A,//!< 충격센서
            [DescriptionAttribute("???")]           SPKMUTEACK = 0x8C,

            [DescriptionAttribute("???")]           REQ_STATUS = 0xA0,
            [DescriptionAttribute("???")]           RESP_STATUS = 0xAA,

            [DescriptionAttribute("???")]           REQ_COUNT = 0xB0,
            [DescriptionAttribute("???")]           RESP_COUNT = 0xBA,

            [DescriptionAttribute("???")]           REQ_CTRLSET = 0xC0,
            [DescriptionAttribute("???")]           RESP_CTRLSET = 0xCA,

            [DescriptionAttribute("???")]           RTU_EVENT = 0xEA
        };

        /* frame 
         STX 2 Bytes 0x02FD
         Receiver 1, rtu 0x21 used when op code 0xC0
         Sender 1(bit aka 11), rtu when periodic status data
         op code
         body len
         ** body **
         check sum (stx~body xor)
         etx 0x03
         */
		

		protected RTU_TY_Rev1 () : base()
		{

		}

		public RTU_TY_Rev1(PortConfig cfg)
			:this()
		{
			_port.PortName = cfg.Name;
			_port.BaudRate = cfg.bRate;//9600
			_port.DataBits = cfg.dBits;
			//RTU_HWR();
		}

        public RTU_TY_Rev1(string portName)
			: this()
		{
			_port.PortName = portName;
			_port.BaudRate = 9600;
			_port.DataBits = 8;
            RTUReceived += Rev1_RTUReceived;
		}

        /// <summary>
        /// 열고 기본적으로 보내줘야 하는 경우 override
        /// </summary>
        /// <returns>성공여부</returns>
		public override bool Initiate()
		{
			if (Connect ()) {
				//SendCommand (cmdByte [0]);//0 1 2 6 7 8
			} else {
				Console.WriteLine ("RTU connect Fail");	
			}
			return true;
		}

        
        /// <summary>
        /// 버퍼클리어, 재접속 override
        /// </summary>
		public override void Reset()
		{
			try
			{
                byte remByte;
				rxBufferBusy.WaitOne();

				if (_port.IsOpen)
				{
					_port.DiscardInBuffer();
					_port.DiscardOutBuffer();
					_port.Close();
				}
			    
                while (!rxFIFO.IsEmpty)
                {
                    rxFIFO.TryDequeue(out remByte);
                }
				//rxFIFO.Clear();
				_port.Open();
			}
			catch (Exception ex)
			{
			    throw;
			}
			finally
			{
				rxBufferBusy.ReleaseMutex();
			}
		}

        /// <summary>
        /// 체크섬 계산, override
        /// </summary>
        /// <param name="_buf"></param>
        /// <returns>XOR byte</returns>
		protected override byte GetChecksum(ref byte[] _buf)
		{
            byte xorTotalByte = 0;
            for (int i = 0; i < _buf.Length; i++)
                xorTotalByte ^= _buf[i];
            return xorTotalByte;
		}

        /// <summary>
        /// 포트 연결 및 이벤트 핸들러 설정
        /// </summary>
        /// <returns></returns>
		public override bool Connect()
		{
		    base.Connect();
            if (_port.IsOpen)
                RTUReceived += Rev1_RTUReceived;
			return _port.IsOpen;
		}

        /// <summary>
        /// 실제 바이트들에 대한 기본처리 및 파싱 콜
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rev1_RTUReceived(object sender, EventArgs e)
        {
            //byte[] test = new byte[8];
            byte[] test = Enumerable.Repeat<byte>(0x41, 8).ToArray();
            ParseMessage(ref test);
            byte _trash;
            if (_port.IsOpen)
            {
                try
                {
                    waitCmdBusy.WaitOne();
                    byte[] header = new byte[6];
                    if (rxFIFO.Count > 8)
                    {
                        //!< check header
                        rxFIFO.CopyTo(header, 0);

                        if (header[0] == 0x02 && header[1] == 0xFD)
                        {
                            //!< get needed body
                            byte bLen = header[5];
                            int dLen = 8 + (int)bLen;

                            if (header[2] == _bitDevId)
                            {
                                //!< Make Copyed Message
                                if (rxFIFO.Count >= 8 + (int) bLen)
                                {
                                    byte[] msg = new byte[dLen];
                                    rxFIFO.CopyTo(msg, 0);

                                    //!< check tail
                                    if (ValidateChecksum(ref msg))
                                    {
                                        //!< waiter clear
                                        _DevId sendDev = (_DevId)Enum.Parse(typeof(_DevId), msg[3].ToString());
                                        if (sendDev != _DevId.UNKNOWN)
                                        {
                                            var matched = _waitCmd.Where(z => z.Value == sendDev).ToList();
                                            foreach (var item in matched)
                                            {
                                                _waitCmd.Remove(item.Key);
                                            }
                                        }
                                        ParseMessage(ref msg);
                                    }
                                }
                            }
                            else
                            {
                                //!< can not be processed for bit
                            }
                            //!< Dequeue to etx
                            while (dLen > 0)
                            {
                                rxFIFO.TryDequeue(out _trash);
                                dLen--;
                            }
                        }
                        else
                        {
                            //!< Dequeue to stx
                            byte[] _stx = new byte[2];
                            while (rxFIFO.Count >= 2)
                            {
                                rxFIFO.CopyTo(_stx, 0);
                                if (_stx[0] == 0x02 && _stx[1] == 0xFD)
                                {
                                    break;
                                }
                                rxFIFO.TryDequeue(out _trash);
                                rxFIFO.TryDequeue(out _trash);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    //MessageBox.Show(ex.StackTrace);
                }
                finally
                {
                    waitCmdBusy.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// 특정 배열을 포트에 쓴다. 인자내의 바이트 배열은 별도의 과정을 거침없이 그대로 쓴다.
        /// </summary>
        /// <param name="_bytes"></param>
		public override void SendCommand(byte[] _bytes)
		{
			if (_port.IsOpen) {
				_port.Write (_bytes, 0, _bytes.Length);
			} else {
				Reset ();
			}
		}

        /// <summary>
        /// ETX 점검 및 체크섬 확인
        /// </summary>
        /// <param name="bufBytes"></param>
        /// <returns></returns>
        private bool ValidateChecksum(ref byte[] bufBytes)
        {
            if (bufBytes[bufBytes.Length - 1] == _frameETX)
            {
                int checkLen = bufBytes.Length - 2;
                try
                {
                    byte[] checkBytes = new byte[checkLen];
                    Array.Copy(bufBytes, 0, checkBytes, 0, checkLen);

                    if (bufBytes[bufBytes.Length - 2] == GetChecksum(ref checkBytes))
                        return true;
                }
                catch (Exception)
                {
                    //Log exception
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        private void ParseMessage(ref byte[] msg)
        {
            _DevId dev = (_DevId) Enum.Parse(typeof (_DevId), msg[3].ToString());
            string senderdesc = Common.Utility.EnumTypeConverter.GetDescriptionFromEnumValue(dev);

            //!< 망함 다시...
        }

        /// <summary>
        /// 요청 패킷을 생성하여 보냄
        /// </summary>
        /// <param name="targetDevId"></param>
        /// <param name="cmd"></param>
        public void RequestRTUByDevCmd(_DevId targetDevId, _opCode cmd)
        {
            //!< 도어 상태정보 31
            //!< 온습도 32, 온도 31
            //!< 조도 31
            //!< 마이크 31
            //!< 스피커 상태정보 31, 볼륨 설정 30/해제 31
            //!< 뮤트 상태정보 31, 설정 30/해제 31
            //!< 팬 수량 31, 상태
            //!< 팬 제어 동작 제어, 자동모드 설정 30/해제 31, 개별 끔 30, 켬 31 수량만큼
            //!< 히터 전원상태, 전원동작 설정 및 제어 자동/수동 ?? 헷갈림
            //!< LCD 전원상태, 전원제어
            //!< LED 전원상태, 전원제어
            //!< AC 전원 리셋, 예약제어(4byte 시간값이 이해가 안됨)
            //!< 전체장치 전원제어 수량정보, 전체장치 전원상태 정보, 전체장치 전원제어
            //!< 초기값 파라미터 설정 32byte
            //!< 모뎀 리셋
            //!< 충격센서 설정값 읽기, 설정 요청(3Byte)
            //!< 상태요청(단순 응답 확인)
        }

        /// <summary>
        /// RTU로부터의 요청에 대한 응답
        /// </summary>
        /// <param name="reqDevId"></param>
        /// <param name="cmd"></param>
        private void ReplyRTURequest(_DevId reqDevId, _opCode cmd)
        {
            //!< 시간요청 값이 이해가 안됨
        }

        /// <summary>
        /// 단순한 ACK 처리
        /// </summary>
        /// <param name="code"></param>
        /// <param name="dev"></param>
        private void ReplyEventACK(_opCode code, _DevId dev, bool bACK)
        {
            try
            {
                byte[] sendBytes = new byte[8];
                byte[] headBytes = new byte[] { 0x02, 0xFD, (byte)dev, 0x11, 0xAA, 0x00 };
                byte[] tailBytes = new byte[] { 0x00, 0x03 };

                byte chk = GetChecksum(ref headBytes);
                tailBytes[0] = chk;

                headBytes.CopyTo(sendBytes, 0);
                tailBytes.CopyTo(sendBytes, 6);

                SendCommand(sendBytes);
            }
            catch (Exception)
            {
                
                //throw;
            }

        }
    }
}
