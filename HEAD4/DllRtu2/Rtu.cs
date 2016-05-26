using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Runtime.InteropServices;
using Common.Utility;
using DllRtu.Event;
using DllRtu.Interface;
using DllRtu.Config;

namespace DllRtu
{
    //클래스 라이브버비
    public class Rtu : RtuInterface
    {
        private int lcdcheck = 6;
        private DateTime impacttime;
        private int sleepImpactTime = 10 * 1000;

        private string programName = "Rtu";

        private const int sleepTime = 100;
        private int renewCycleTime = 60;

        private bool rtuThreadStatus = false;
        private bool sendComplete = true;

        private SerialPort serialPort = null;
        private bool serialError;

        private string configFile = "./config/rtuPropertys.xml";

        #region RTU 값 정의
        // STX
        private const byte STX = 0x02;
        private const byte NSTX = 0xFD;

        // ETX
        private const byte ETX = 0x3F;

        // ID
        private const int bit_id = 0;
        private const int rtu_id = 1;
        private const byte BIT_ID = 0x11;
        private const byte RTU_ID = 0x21;


        // OPCODE
        private const int opcode = 2;
        private const byte OPCODE_GET_REQUEST_01 = 0x01; // 상태요청
        private const byte OPCODE_RESPONSE_02 = 0x02; // 상태요청응답
        private const byte OPCODE_SET_REQUEST_03 = 0x03; // 상태설정요청
        private const byte OPCODE_ACK_SET_04 = 0x04; // 상태설정요청응답
        private const byte OPCODE_EVENT_05 = 0x05; //이벤트
        private const byte OPCODE_ACK_EVENT_06 = 0x06; //이벤트응답
        private const byte OPCODE_GET_SETVAL_07 = 0x07; //설정갑상태
        private const byte OPCODE_RESPONSE_SETVAL_08 = 0x08; //설정값상태응답
        private const byte OPCODE_SET_SETVAL_09 = 0x09;//설정값설정요청
        private const byte OPCODE_ACK_SETVAL_0A = 0x0A;//설정값설정요청응답
        private const byte OPCODE_ALARM_0B = 0x0B;//알람



        //CMD
        private const int cmd = 3;
        private const byte CMD_DOOR = 0x01;
        private const byte CMD_TEMP = 0x02;
        private const byte CMD_HUMI = 0x03;
        private const byte CMD_CDS = 0x04;
        private const byte CMD_KEY = 0x05;
        private const byte CMD_NOISE = 0x06;
        private const byte CMD_IMPACT = 0x07;
        private const byte CMD_SPK_AMP = 0x08;
        private const byte CMD_CAMERA_FLASH = 0x09;
        private const byte CMD_PC_RST = 0x0A;
        private const byte CMD_PC_PWR = 0x0B;
        private const byte CMD_FAN = 0x0C;
        private const byte CMD_HEATER = 0x0D;
        private const byte CMD_DC_PWR = 0x0E;
        private const byte CMD_AC_PWR = 0x0F;
        private const byte CMD_RTC = 0x10;
        private const byte CMD_SYS_INFO = 0x11;
        private const byte CMD_MON_PWR = 0x12;
        private const byte CMD_ENV_RST = 0x13;
        private const byte CMD_FANERR_UPDATE = 0x14;
        private const byte CMD_SYSPWR_STAT = 0x15;
        private const byte CMD_FACTORY = 0x16;
        private const byte CMD_PC_PWRBTN = 0x17;
        private const byte CMD_SYS_STAT = 0x18;
        private const byte CMD_CDS_PWRCTL = 0x19;
        private const byte CMD_DEV_STAT = 0x1A;
        private const byte CMD_PWRSW_OFFON = 0x1B;
        private const byte CMD_BIT_STAT = 0x1C;
        private const byte CMD_ENV_SETVAL = 0x1D;
        private const byte CMD_BIT_PROC_RESTART = 0x1E;
        private const byte CMD_BIT_DMB = 0x1F;
        private const byte CMD_BIT_CAPTURE = 0x20;
        private const byte CMD_AD_BOARD_STAT = 0x21;
        private const byte CMD_BIT_RELOAD = 0x22;
        private const byte CMD_ICMP_PWRCTL = 0x23;
        private const byte CMD_AD_BOARD_CTL = 0x24;
        private const byte CMD_AC_FAN_PORT = 0x26;
        private const byte CMD_SOUND_UPDOWN = 0x30;
        private const byte CMD_IPADDRESS = 0x32;
        private const byte CMD_CK_STAT = 0x33;
        private const byte CMD_DC_STAT = 0x34;
        private const byte CMD_AC_STAT = 0x35;
        private const byte CMD_ADBOARD_LCD_STAT = 0x36; //

        // CMD_KEY
        private const int CMD_Key_down = 05; // 하 키
        private const int CMD_Key_right = 06; // 우 키
        private const int CMD_Key_enter = 07; // 확인 키
        private const int CMD_Key_sound = 08; // 음성 키
        private const int CMD_Key_menu = 10; // 메뉴 키
        private const int CMD_Key_left = 11; // 좌 키
        private const int CMD_Key_up = 12; // 상 키


        private const byte hexbyte_00 = 0x00;
        private const byte hexbyte_01 = 0x01;
        private const byte hexbyte_02 = 0x02;

        private const int temp = 5;


        #endregion



        private byte[] sendMSG = new byte[256];

        private Rtu_Parsing rtu_Parsing = new Rtu_Parsing();

        private static RtuStatus rtuStatus = new RtuStatus();

        private RtuPropertys rtuPropertys = null;
        public event RtuEventHandler rtuEventHandler;

        public bool detailMode = true;


        public Rtu()
        {
            initRtu();
        }
        public Rtu(string configFile)
        {
            this.configFile = configFile;
            initRtu();
        }
        public void Dispose()
        {
        }
        private bool initRtu()
        {

            try
            {
                rtuPropertys = new RtuPropertys(configFile);

                this.renewCycleTime = int.Parse(rtuPropertys.RenewCycleTime);

                // Auto Mode => false 처리?

            }
            catch (System.Exception ex)
            {
                throw ex;
            }

            return true;

        }
        public bool propertysLoad()
        {
            string functionName = "propertysLoad";
            bool returnValue = false;
            try
            {
                rtuPropertys.propertysLoad();
                this.renewCycleTime = int.Parse(rtuPropertys.RenewCycleTime);

                returnValue = startSerial();
            }
            catch (Exception ex)
            {
                returnValue = false;
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
            }

            return returnValue;
        }
        private bool startSerial()
        {
            string functionName = "startSerial";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                returnValue = InitSerial();
                returnValue = returnValue && PortOpen();
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private Parity getParity(string parity)
        {

            if ("0".Equals(parity))
            {
                return Parity.None;
            }
            else if ("1".Equals(parity))
            {
                return Parity.Odd;
            }
            else if ("2".Equals(parity))
            {
                return Parity.Even;
            }
            else if ("3".Equals(parity))
            {
                return Parity.Mark;
            }
            else if ("4".Equals(parity))
            {
                return Parity.Space;
            }
            else
            {
                return Parity.None;
            }

        }
        private StopBits getStopBits(string stopBits)
        {

            if ("0".Equals(stopBits))
            {
                return StopBits.None;
            }
            else if ("1".Equals(stopBits))
            {
                return StopBits.One;
            }
            else if ("2".Equals(stopBits))
            {
                return StopBits.Two;
            }
            else if ("3".Equals(stopBits))
            {
                return StopBits.OnePointFive;
            }
            else
            {
                return StopBits.One;
            }

        }
        public bool InitSerial()
        {
            string functionName = "initSerial";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                if (null != this.serialPort)
                {
                    PortClose();
                    serialPort.Dispose();
                }
                this.serialPort = new SerialPort();
                this.serialPort.PortName = rtuPropertys.Port;
                this.serialPort.BaudRate = int.Parse(rtuPropertys.BaudRate);
                this.serialPort.Parity = getParity(rtuPropertys.Parity);
                this.serialPort.DataBits = int.Parse(rtuPropertys.DataBits);
                this.serialPort.StopBits = getStopBits(rtuPropertys.StopBits);


                // Set the read/write timeouts
                this.serialPort.ReadTimeout = -1;
                this.serialPort.WriteTimeout = -1;
                this.serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);
                this.serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(serialPort_errorReceived);

                returnValue = true;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        public bool PortOpen()
        {
            string functionName = "PortOpen";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                serialError = false;
                if (!serialPort.IsOpen)
                {
                    this.serialPort.Open();
                }
                returnValue = true;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        public bool PortClose()
        {
            string functionName = "PortClose";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                if (serialPort.IsOpen)
                {
                    this.serialPort.Close();
                }
                returnValue = true;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string functionName = "serialPort_DataReceived";
            Thread.Sleep(30);

            try
            {
                int recvDataSize = serialPort.BytesToRead;
                byte[] recvData = new byte[recvDataSize];
                serialPort.Read(recvData, 0, recvDataSize);
                Log.WriteLog(LogLevel.INFO, programName, functionName, "recvData", (string)ConvertUtil.convert("B2HS", recvData, recvDataSize));
                if (recvDataSize > 4)
                {
                    RtuSendData rtuSendData = new RtuSendData(recvData, recvData.Length);
                    if (detailMode)
                    {
                        rtuEventHandler(this, new RtuEventArgs("RecvData", (object)rtuSendData));
                    }

                    string[] stringarry = rtu_Parsing.byte_parsing(recvData);
                    for (int i = 1; i < stringarry.Length; i++)
                    {
                        if (stringarry[i].StartsWith("1121"))
                        {

                            parser((byte[])ConvertUtil.convert("HS2B", stringarry[i], stringarry[i].Length));

                        }
                        else
                        {
                            Log.WriteLog(LogLevel.ERROR, programName, functionName, "잘못된 시리얼 recive");
                        }

                    }
                }
                else
                {
                    Log.WriteLog(LogLevel.ERROR, programName, functionName, "잘못된 시리얼 길이 recive");
                }
                //  }
                return;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
                return;
            }
        }
        private void serialPort_errorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            string functionName = "serialPort_errorReceived";
            serialError = true;
            string strErr = "";
            switch (e.EventType)
            {
                case SerialError.Frame:
                    strErr = "HardWare Framing Error";
                    break;
                case SerialError.Overrun:
                    strErr = "Charaters Buffer Over Run";
                    break;
                case SerialError.RXOver:
                    strErr = "Input Buffer OverFlow";
                    break;
                case SerialError.RXParity:
                    strErr = "Founded Parity Error";
                    break;
                case SerialError.TXFull:
                    strErr = "Write Buffer was Fulled";
                    break;
                default:
                    break;
            }

            Log.WriteLog(LogLevel.ERROR, programName, functionName, serialError.ToString(), strErr);
            return;
        }
        public bool sendToRtu(byte[] bytesData, int len)
        {
            string functionName = "sendToRtu";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                sendComplete = false;
                for (int loopCount = 0; loopCount < 1; loopCount++)
                {
                    Log.WriteLog(LogLevel.INFO, programName, functionName, "bytesData", (string)ConvertUtil.convert("B2HS", bytesData, len));
                    this.serialPort.Write(bytesData, 0, len);
                    Thread.Sleep(sleepTime);
                    if (sendComplete)
                    {
                        break;
                    }
                }
                returnValue = true;
            }
            catch (Exception e)
            {
                serialError = true;
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }
            return returnValue;
        }
        //시리얼 프로토컬 날라오면 여기서 정리 
        public void parser(byte[] recvData)
        {
            //02 FD 11 21 05 05 18 02 02 02 02 02 01 01 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 D4 3F
            //02 FD 11 21 05 05 18 02 02 02 02 02 02 02 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 D7 3F 
            string functionName = "parser";
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                byte[] op_cmd = new byte[2];
                if (recvData[bit_id] == BIT_ID && recvData[rtu_id] == RTU_ID)
                {

                    #region 파싱 시작

                    switch (recvData[opcode])
                    {
                        #region// 상태요청 라인 01
                        case OPCODE_GET_REQUEST_01:
                            switch (recvData[cmd])
                            {
                                case CMD_CK_STAT:
                                    op_cmd = new byte[2] { OPCODE_RESPONSE_02,CMD_CK_STAT };
                                    sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))),
                                        makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                                    rtuEventHandler(this, new RtuEventArgs("CMD_CK_STAT", ""));
                                    break;

                            }

                            break;
                        #endregion

                        #region// 상태요청 응답 처리 02
                        case OPCODE_RESPONSE_02: //상태요청 응답라인
                            switch (recvData[cmd]) //CMD 코드 처리
                            {
                                #region Door
                                case CMD_DOOR:
                                    if (recvData[5] == hexbyte_01)
                                    {
                                        rtuStatus.DOOR[0] = true;
                                    }
                                    else
                                    {
                                        rtuStatus.DOOR[0] = false;
                                    }

                                    //DOOR2
                                    if (recvData[6] == hexbyte_01)
                                    {
                                        //OPEN
                                        rtuStatus.DOOR[1] = true;
                                    }
                                    else
                                    {
                                        rtuStatus.DOOR[1] = false;
                                    }
                                    rtuEventHandler(this, new RtuEventArgs("Door", (object)recvData));
                                    break;
                                #endregion

                                #region DC POWER
                                case CMD_DC_PWR:
                                    for (int i = 0; i < rtuStatus.DC.Length; i++)
                                    {
                                        if (recvData[i + 5] == hexbyte_01)
                                        {
                                            //ON
                                            rtuStatus.DC[i] = true;
                                        }
                                        else
                                        {
                                            rtuStatus.DC[i] = false;
                                        }
                                    }
                                    //DC 11 21 02 0E 09 01 01 01 01 01 01   01   01 01 CB3F

                                    rtuEventHandler(this, new RtuEventArgs("DC_POWER", (RtuStatus)rtuStatus));
                                    break;
                                #endregion

                                #region 조도 센스
                                case CMD_CDS:

                                    rtuStatus.Cdsvalue = recvData[5];
                                    rtuEventHandler(this, new RtuEventArgs("testc", (RtuStatus)rtuStatus));

                                    break;

                                #endregion

                                #region 히터 모드
                                case CMD_HEATER:
                                    if (recvData[5] == 0x02) // 히터 자동모드 
                                    {
                                        //히터모드이면  대전은 1
                                        rtuStatus.HEATER_MODE = 0x01;
                                    }
                                    else
                                    {
                                        rtuStatus.HEATER_MODE = 0x00;
                                    }
                                    rtuEventHandler(this, new RtuEventArgs("HeaterMode", (RtuStatus)rtuStatus));
                                    break;
                                #endregion

                                #region AC POWER
                                case CMD_AC_PWR:
                                    for (int i = 0; i < rtuStatus.AC.Length; i++)
                                    {
                                        if (recvData[i + 5] == hexbyte_01)
                                        {
                                            //ON
                                            rtuStatus.AC[i] = true;
                                        }
                                        else
                                        {
                                            rtuStatus.AC[i] = false;
                                        }
                                    }
                                    rtuEventHandler(this, new RtuEventArgs("AC_POWER", (RtuStatus)rtuStatus));
                                    break;
                                #endregion

                                #region 습도
                                case CMD_HUMI:
                                    rtuStatus.HUMI = (int)recvData[5];

                                    rtuEventHandler(this, new RtuEventArgs("Hyumdity", (RtuStatus)rtuStatus));
                                    break;
                                #endregion

                                #region 온도
                                case CMD_TEMP:
                                    rtuStatus.TEMP[0] = recvData[5]; //0이면 영상온도
                                    rtuStatus.TEMP[1] = recvData[6];
                                    rtuStatus.TEMP[2] = recvData[7];

                                    rtuEventHandler(this, new RtuEventArgs("Temperature", (RtuStatus)rtuStatus));
                                    break;
                                #endregion

                                #region IP주소
                                case CMD_IPADDRESS:
                                    rtuEventHandler(this, new RtuEventArgs("Ipaddress", (object)recvData));
                                    break;
                                #endregion

                                #region DC상태
                                case CMD_DC_STAT:
                                    for (int i = 0; i < rtuStatus.DC_STAT_Value.Length; i++)
                                    {
                                        rtuStatus.DC_STAT_Value[i] = recvData[temp + i];
                                    }
                                    rtuEventHandler(this, new RtuEventArgs("dcstate", (object)rtuStatus.DC_STAT_Value));
                                    break;
                                #endregion

                                #region AC상태
                                case CMD_AC_STAT:
                                    for (int i = 0; i < rtuStatus.AC_STAT_Value.Length; i++)
                                    {
                                        rtuStatus.AC_STAT_Value[i] = recvData[temp + i];
                                    }
                                    rtuEventHandler(this, new RtuEventArgs("acstate", (object)rtuStatus.AC_STAT_Value));
                                    break;
                                #endregion

                                #region CMD_ADBOARD_LCD_STAT
                                case CMD_ADBOARD_LCD_STAT:
                                        if (recvData[temp] == 0x01)
                                        {
                                            rtuStatus.lcd_status[0] = true;
                                        }
                                        else
                                        {
                                            rtuStatus.lcd_status[0] = false;
                                        }
                                        if (recvData[temp + 1] == 0x01)
                                        {
                                            rtuStatus.lcd_status[1] = true;
                                        }
                                        else
                                        {
                                            rtuStatus.lcd_status[1] = false;
                                        }
                                       // rtuEventHandler(this, new RtuEventArgs("acstate", (object)rtuStatus.lcd_status));
                                    break;
                                #endregion
                            }
                            break;
                        #endregion
                            
                        #region// 상태설정요청 처리 03
                        case OPCODE_SET_REQUEST_03:
                            break;
                        #endregion

                        #region// 상태설정요청 응답 처리 04
                        case OPCODE_ACK_SET_04:
                            switch(recvData[cmd])
                            {

                                #region AC POWER
                                case CMD_AC_PWR:
                                    for (int i = 0; i < rtuStatus.AC.Length; i++)
                                    {
                                        if (recvData[i + 5] == hexbyte_01)
                                        {
                                            //ON
                                            rtuStatus.AC[i] = true;
                                        }
                                        else if(recvData[i +5] == hexbyte_02)
                                        {
                                            rtuStatus.AC[i] = false;
                                        }
                                    }
                                   // rtuEventHandler(this, new RtuEventArgs("AC_POWER", (RtuStatus)rtuStatus));
                                    break;

                                #endregion

                                #region DC POWER
                                case CMD_DC_PWR:


                                    for (int i = 0; i < rtuStatus.DC.Length; i++)
                                    {
                                        if (recvData[i + 5] == hexbyte_01)
                                        {
                                            //ON
                                            rtuStatus.DC[i] = true;
                                        }
                                        else if(recvData[i+5] == hexbyte_02)
                                        {
                                            rtuStatus.DC[i] = false;
                                        }
                                    }

                                    //DC 11 21 02 0E 09 01 01 01 01 01 01   01   01 01 CB3F

                                    //rtuEventHandler(this, new RtuEventArgs("DC_POWER", (RtuStatus)rtuStatus));
                                    break;
                                #endregion
                            }
                            break;
                        #endregion

                        #region// 이벤트 처리 05
                        case OPCODE_EVENT_05:
                            switch (recvData[cmd])
                            {
                                #region IMPACT
                                case CMD_IMPACT:
                                    if ((DateTime.Now - impacttime).TotalMilliseconds > sleepImpactTime)
                                    {
                                        impacttime = DateTime.Now;

                                        op_cmd = new byte[2] { OPCODE_ACK_EVENT_06, CMD_IMPACT };
                                        sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                                        rtuEventHandler(this, new RtuEventArgs("IMPACT", null));
                                    }
                                    
                                    break;
                                #endregion

                                #region KEY
                                case CMD_KEY:
                                    EnumKeyPress keyNum;
                                    if (recvData[CMD_Key_sound] == hexbyte_01)//음성
                                    {
                                        //02 11 21 05 05 18 01 02 02 02 02 02 02 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 29 3F 
                                        keyNum = EnumKeyPress.SOUND;
                                        rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                                    }
                                    else if (recvData[CMD_Key_enter] == hexbyte_01)//확인
                                    {
                                        //11 21 05 05 18 02 02 01 02 02 02 02 02 00 00 00000000000000000000
                                        //02 11 21 05 05 18 02 01 02 02 02 02 02 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 29 3F 
                                        keyNum = EnumKeyPress.CONFIRM;
                                        rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                                    }
                                    else if (recvData[CMD_Key_right] == hexbyte_01)//우
                                    {
                                        //02 11 21 05 05 18 02 02 01 02 02 02 02 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 29 3F 
                                        keyNum = EnumKeyPress.RIGHT;
                                        rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                                    }
                                    else if (recvData[CMD_Key_down] == hexbyte_01)//하
                                    {
                                        //02 11 21 05 05 18 02 02 02 01 02 02 02 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 29 3F 
                                        keyNum = EnumKeyPress.DOWN;
                                        rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                                    }
                                    else if (recvData[CMD_Key_up] == hexbyte_01)//상
                                    {
                                        //11 21 05 05 18 02 02 02 02 02 02 02 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 D4 3F  
                                        //02 11 21 05 05 18 02 02 02 02 01 02 02 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 29 3F 
                                        keyNum = EnumKeyPress.UP;
                                        rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                                    }
                                    else if (recvData[CMD_Key_left] == hexbyte_01)//좌
                                    {
                                        //02 11 21 05 05 18 02 02 02 02 02 01 02 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 29 3F 
                                        keyNum = EnumKeyPress.LEFT;
                                        rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                                    }
                                    else if (recvData[CMD_Key_menu] == hexbyte_01)//메뉴
                                    {
                                        //11 21 05 05 18 02 02 02 02 02 01 02 02 00 0000000000000000000000
                                        keyNum = EnumKeyPress.MENU;
                                        rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                                    }
                                    else
                                    {
                                        op_cmd = new byte[2] { OPCODE_ACK_EVENT_06, CMD_KEY };
                                        sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                                    }
                                        break;
                                #endregion

                                #region Door
                                case CMD_DOOR:

                                    if (recvData[5] == hexbyte_01)
                                    {
                                        rtuStatus.DOOR[0] = true;
                                    }
                                    else
                                    {
                                        rtuStatus.DOOR[0] = false;
                                    }

                                    //DOOR2
                                    if (recvData[6] == hexbyte_01)
                                    {
                                        //OPEN
                                        rtuStatus.DOOR[1] = true;
                                    }
                                    else
                                    {
                                        rtuStatus.DOOR[1] = false;
                                    }
                                    op_cmd = new byte[2] { OPCODE_ACK_EVENT_06, CMD_DOOR };
                                    sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                                    rtuEventHandler(this, new RtuEventArgs("Door", (object)recvData));
                                    break;
                                #endregion

                                #region CMD_SOUND_UPDOWN:
                                case CMD_SOUND_UPDOWN:

                                    // 11 21 05 30 04 02 01 00 00 FD 3F 
                                    if (recvData[6] == 0x01)
                                    {
                                        rtuEventHandler(this, new RtuEventArgs("SoundUp", null));
                                    }
                                    else if (recvData[5] == 0x01)
                                    {
                                        rtuEventHandler(this, new RtuEventArgs("SoundDown", null));
                                    }
                                    else if (recvData[5] == 0x01 && recvData[6] == 0x01)
                                    {
                                        //rtuEventHandler(this, new RtuEventArgs("SoundIni", null));
                                    }

                                    op_cmd = new byte[2] { OPCODE_ACK_EVENT_06, CMD_SOUND_UPDOWN };
                                    sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                                    break;
                                #endregion

                                #region CMD_SPK_AMP
                                case CMD_SPK_AMP:

                                    // 11 21 05 08 02 02 01 FD 3F 
                                   // if (recvData[5] == 0x00)
                                 //   {
                                       
                                     //   rtuEventHandler(this, new RtuEventArgs("SPKAMP_MUTE", null));
                                 //   }
                                 //   else
                                 //   {
                                        int spk = recvData[6];
                                        rtuEventHandler(this, new RtuEventArgs("SPKAMP", spk));
                               //     }

                                    op_cmd = new byte[2] { OPCODE_ACK_EVENT_06, CMD_SPK_AMP };
                                    sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                                    break;
                                #endregion

                                #region 웹하면 리로리드
                                case CMD_BIT_RELOAD:
                                    rtuEventHandler(this, new RtuEventArgs("Reload", (int)recvData[6]));
                                    break;
                                #endregion

                                #region 웹하면 캡쳐
                                case CMD_BIT_CAPTURE:
                                    rtuEventHandler(this, new RtuEventArgs("CAPTURE", (int)recvData[6]));
                                    break;
                                #endregion

                                #region DMB제어
                                case CMD_BIT_DMB:
                                    int dmbstatus = 0;
                                    if (recvData[6] == 0x01)
                                    {
                                        dmbstatus = 1;
                                    }
                                    else
                                    {
                                        dmbstatus = 0;
                                    }
                                    rtuEventHandler(this, new RtuEventArgs("DMBONOFF", (int)dmbstatus));
                                    break;
                                #endregion

                                #region 프로세스 재시작
                                case CMD_BIT_PROC_RESTART:
                                    int sleep = (int)recvData[6];
                                    Thread.Sleep(sleep * 1000);
                                    rtuEventHandler(this, new RtuEventArgs("reStart", null));
                                    break;
                                #endregion
                            }
                            break;
                        #endregion

                        #region// 이벤트 응답 처리 06
                        case OPCODE_ACK_EVENT_06:
                            break;
                        #endregion

                        #region// 설정값상태요청 처리 07
                        case OPCODE_GET_SETVAL_07:
                            break;
                        #endregion

                        #region// 설정값상태요청 응답 처리 08
                        case OPCODE_RESPONSE_SETVAL_08:
                            switch (recvData[cmd])
                            {
                                #region 팬 상태 설정 값0
                                case CMD_FAN:
                                    for (int i = 0; i < rtuStatus.FAN_STAT.Length; i++)
                                    {
                                        rtuStatus.FAN_STAT[i] = recvData[i + 5];
                                    }
                                    rtuEventHandler(this, new RtuEventArgs("FanSTAT", (RtuStatus)rtuStatus));
                                    break;
                                #endregion

                                #region 히터 상태 설정 값0
                                case CMD_HEATER:
                                    for (int i = 0; i < rtuStatus.HEATER_STAT.Length; i++)
                                    {
                                        rtuStatus.HEATER_STAT[i] = recvData[i + 5];
                                    }
                                    rtuEventHandler(this, new RtuEventArgs("HeaterSTAT", (RtuStatus)rtuStatus));
                                    break;
                                #endregion
                            }
                            break;
                        #endregion

                        #region// 설정값설정요청 처리 09
                        case OPCODE_SET_SETVAL_09:
                            break;
                        #endregion

                        #region// 설정값설정요청 응답 처리 0A
                        case OPCODE_ACK_SETVAL_0A:
                            switch (recvData[cmd])
                            {
                                #region 팬 상태 설정 Reive
                                case CMD_FAN:
                                    for (int i = 0; i < rtuStatus.FAN_STAT.Length; i++)
                                    {
                                      rtuStatus.FAN_STAT[i] = recvData[i + 5];
                                  }
                                    rtuEventHandler(this, new RtuEventArgs("FanSTAT", (RtuStatus)rtuStatus));
                                    break;
                                #endregion

                                #region 히터 상태 설정 Reive
                                case CMD_HEATER:

                                    for (int i = 0; i < rtuStatus.HEATER_STAT.Length; i++)
                                    {
                                        rtuStatus.HEATER_STAT[i] = recvData[i + 5];
                                    }
                                    rtuEventHandler(this, new RtuEventArgs("HeaterSTAT", (RtuStatus)rtuStatus));
                                    break;
                                #endregion

                                #region IP주소 상태 설정 RECIVE
                                case CMD_IPADDRESS:
                                    rtuEventHandler(this, new RtuEventArgs("Ipaddress", (object)recvData));
                                    break;
                                #endregion

                                #region IP주소 상태 설정 RECIVE
                                case CMD_ADBOARD_LCD_STAT:
                                    rtuEventHandler(this, new RtuEventArgs("lcdCheck", (object)recvData));
                                    break;
                                #endregion

                            }
                            break;
                        #endregion

                        #region// 알람 11
                        case OPCODE_ALARM_0B:
                            break;
                        #endregion
                    }
                    #endregion

                }
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
            }
        }
        public byte[] makeSendData(byte[] op_cmd)
        {
            string functionName = "makeSendData";
            byte[] sendData = new byte[7];

            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                sendData[0] = STX;
                sendData[1] = NSTX;
                sendData[2] = RTU_ID;
                sendData[3] = BIT_ID;
                sendData[4] = op_cmd[0];
                sendData[5] = op_cmd[1];
                sendData[6] = 0x00;

            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
            }

            return sendData;
        }
        public byte[] makeCheckSum(byte[] data)
        {
            string functionName = "makeCheckSum";
            byte[] CheckSum = new byte[2] { 0x00, 0x3F };
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                for (int i = 0; i < data.Length; i++)
                {
                    CheckSum[0] = (byte)((int)CheckSum[0] ^ (int)data[i]);
                }
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                CheckSum[0] = 0;
            }

            return CheckSum;
        }
        public byte[] makeByteSum(byte[] serial_Header, byte[] serial_Tail)
        {
            byte[] full_data = new byte[serial_Header.Length + serial_Tail.Length];

            Buffer.BlockCopy(serial_Header, 0, full_data, 0, serial_Header.Length);
            Buffer.BlockCopy(serial_Tail, 0, full_data, serial_Header.Length, serial_Tail.Length);

            return full_data;
        }
        public RtuPropertys getPropertys()
        {
            return this.rtuPropertys;
        }
        #region Rtu 조회
        private void lcd_bright()
        {
            lcdcheck = 17;
            string functionName = "lcd_bright ";
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                byte[] Header_byte = { STX, NSTX, RTU_ID, BIT_ID, 0x03, 0x24, 0x04, 0x00, 0x64, 0x00, 0x00 };
                byte[] bbbb = makeCheckSum(Header_byte);
                byte[] full_data = new byte[Header_byte.Length + bbbb.Length];

                Buffer.BlockCopy(Header_byte, 0, full_data, 0, Header_byte.Length);
                Buffer.BlockCopy(bbbb, 0, full_data, Header_byte.Length, bbbb.Length);


                sendToRtu(full_data, full_data.Length);

                //sendToRtu(ConvertUtil.hexStringToByte("02FD211103240400640000"+88+"3F"), 13);
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
            }
        }
        private void lcd_dark()
        {
            lcdcheck = 6;
            string functionName = "lcd_dark";
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                sendToRtu(ConvertUtil.hexStringToByte("02FD211103240400320000DE3F"), 13);
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
            }
        }
        private void bitstat()
        {
            string functionName = "bitStatus";
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                sendToRtu(ConvertUtil.hexStringToByte("02FD2111021C00D13F"), 9);
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
            }
        }
        private bool getTEMP()
        {
            string functionName = "getStatusTEMPERATURE ";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_TEMP };
                sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                Thread.Sleep(sleepTime);

                returnValue = true;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool getHUMI()
        {
            string functionName = "getStatusHUMI";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_HUMI };
                sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                /*
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("02FD2111010300CD3F"), 9);
                Thread.Sleep(sleepTime);
                */
                returnValue = true;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool getFANMODE()
        {
            string functionName = "getStatusFANMODE";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                byte[] op_cmd = new byte[2] { OPCODE_GET_SETVAL_07, CMD_FAN };
                sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                /*
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("02FD2111 07 0C 00C43F"), 9);
                Thread.Sleep(sleepTime);
                */
                returnValue = true;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool getAC()
        {
            string functionName = "getStatusAC";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_AC_PWR };
                sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                /*
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("02FD2111010F00C13F"), 9);
                Thread.Sleep(sleepTime);
                */
                returnValue = true;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool getDC()
        {
            string functionName = "getStatusDC";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_DC_PWR };
                sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                /*
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("02FD2111010E00C03F"), 9);
                Thread.Sleep(sleepTime);
                */
                returnValue = true;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool getDOOR()
        {
            string functionName = "getStatusDOOR";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_DOOR };
                sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                /*
                Thread.Sleep(sleepTime);f
                sendToRtu(ConvertUtil.hexStringToByte("02FD2111010100CF3F"), 9);
                Thread.Sleep(sleepTime);
                */
                returnValue = true;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool getSOUND()
        {
            string functionName = "getStatusSOUND";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_SPK_AMP };
                sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                /*
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("02FD2111010800C63F"), 9);
                Thread.Sleep(sleepTime);
                */
                returnValue = true;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool getHEATERMODE()
        {
            string functionName = "getStatusHEATERMODE";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_HEATER };
                sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                /*
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("02FD2111070D00C53F"), 9);
                Thread.Sleep(sleepTime);
                 */

                returnValue = true;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool getLCDSTATUS()
        {
            string functionName = "getLCDSTATUS";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {

                byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_ADBOARD_LCD_STAT };
                sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);




               // byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_AD_BOARD_STAT };
               // sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                /*
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("02FD2111012100EF3F"), 9);
                Thread.Sleep(sleepTime);
                */
                returnValue = true;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool getDCSTATUS()
        {
            string functionName = "getDCSTATUS";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_DC_STAT };
                sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                /*
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("02FD2111012100EF3F"), 9);
                Thread.Sleep(sleepTime);
                */
                returnValue = true;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }

        private bool getACSTATUS()
        {
            string functionName = "getACSTATUS";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_AC_STAT };
                sendToRtu(makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))), makeByteSum(makeSendData(op_cmd), makeCheckSum(makeSendData(op_cmd))).Length);
                /*
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("02FD2111012100EF3F"), 9);
                Thread.Sleep(sleepTime);
                */
                returnValue = true;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }

        #endregion
        #region Rtu 제어
        public bool controlPcReset()
        {
            string functionName = "controlPcReset";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                //022111030A01013B3F
                sendToRtu(ConvertUtil.hexStringToByte("02 21 11 03 0A 01 01 3B3F"), 9);
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }

        #endregion
        public RtuStatus getRtuStatus()
        {
            return rtuStatus;
        }
        #region Thread관련
        public bool getRtuThreadStatus()
        {
            return rtuThreadStatus;
        }
        public void setRtuThreadStatus(bool status)
        {
            rtuThreadStatus = status;
        }
        #endregion
        public void threadRun()
        {
            DateTime time = DateTime.Now;
            string functionName = "threadRun";
            bool returnValue;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");

            rtuThreadStatus = true;
            // startSerial();

            
            if (startSerial())
            {
                returnValue = getTEMP();
                Thread.Sleep(sleepTime);
                returnValue = getHUMI();
                Thread.Sleep(sleepTime);
                returnValue = getAC();
                Thread.Sleep(sleepTime);
                returnValue = getDC();
                Thread.Sleep(sleepTime);
                returnValue = getDOOR();
                Thread.Sleep(sleepTime);
                returnValue = getFANMODE();
                Thread.Sleep(sleepTime);
                returnValue = getSOUND();
                Thread.Sleep(sleepTime);
                returnValue = getHEATERMODE();
                Thread.Sleep(sleepTime);
                returnValue = getLCDSTATUS();
            }
            
            while (rtuThreadStatus)
            {
                try
                {
                        if (startSerial())
                        {
                            returnValue = getTEMP();
                            Thread.Sleep(sleepTime);
                            returnValue = getHUMI();
                            Thread.Sleep(sleepTime);
                            returnValue = getAC();
                            Thread.Sleep(sleepTime);
                            returnValue = getDC();
                            Thread.Sleep(sleepTime);
                            returnValue = getDOOR();
                            Thread.Sleep(sleepTime);
                            returnValue = getFANMODE();
                            Thread.Sleep(sleepTime);
                            returnValue = getSOUND();
                            Thread.Sleep(sleepTime);
                            returnValue = getHEATERMODE();
                            Thread.Sleep(sleepTime);
                            returnValue = getLCDSTATUS();
                            Thread.Sleep(sleepTime);
                            returnValue = getACSTATUS();
                            Thread.Sleep(sleepTime);
                            returnValue = getDCSTATUS();
                        }
                }
                catch (Exception e)
                {
                    Log.WriteLog(LogLevel.TRACE, programName, functionName, "Exception", e.ToString());
                }
                Thread.Sleep(renewCycleTime * 1000);
            }
        }

    }
}
