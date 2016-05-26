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
using DllRtu.Config;
using DllRtu.IdonRtuInterface;

namespace DllRtu
{
    //클래스 라이브버비
    public class Idon_Rtu : Idon_RtuInterface
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
        #region 각데이터 정의

        //ACK/NAK
        private const byte STX = 0x02;
        private const byte ETX = 0x03;

        private const byte OpCodeACK = 0x06;
        private const byte OpCodeNAK = 0x15;

        private const byte ID = 0x67;

        //이벤트
        private const byte OpCode_E_Key = 0x2E;//키입력
        private const byte OpCode_E_Door = 0x2C;//도어
        //요청
        private const byte OpCode_R_Temperature = 0x28;//온도
        private const byte OpCode_R_Humidity = 0x2A;//습도
        private const byte OpCode_R_Door = 0x2D;//도어
        private const byte OpCode_R_Volume = 0x3D;//볼륨
        private const byte OpCode_R_Fan1 = 0x43;//팬 환풍
        private const byte OpCode_R_Fan2 = 0x47;//팬 LCD
        private const byte OpCode_R_Heater = 0x4B;//히터
        //제어
        private const byte OpCode_C_Volume = 0x3C;//볼륨
        private const byte OpCode_C_Mode_Fan1 = 0x40;//팬 환풍
        private const byte OpCode_C_Status_Fan1 = 0x41;//팬 환풍
        private const byte OpCode_C_AutoSet_Fan1 = 0x42;//팬 환풍
        private const byte OpCode_C_Mode_Fan2 = 0x44;//팬 LCD
        private const byte OpCode_C_Status_Fan2 = 0x45;//팬 LCD
        private const byte OpCode_C_AutoSet_Fan2 = 0x46;//팬 LCD
        private const byte OpCode_C_Mode_Heater = 0x48;//히터
        private const byte OpCode_C_Status_Heater = 0x49;//히터
        private const byte OpCode_C_AutoSet_Heater = 0x4A;//히터
        private const byte OpCode_C_PcReset = 0x60;//PC Reset





        #endregion

        private byte[] sendMSG = new byte[256];


        private static RtuStatus rtuStatus = new RtuStatus();

        private RtuPropertys rtuPropertys = null;
        public event Idon_RtuEventHandler idon_rtuEventHandler;

        public bool detailMode = true;

        public Idon_Rtu()
        {
            initRtu();
        }

        public Idon_Rtu(string configFile)
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
                if (recvDataSize > 0)
                {

                    byte[] recvData = new byte[recvDataSize];
                    serialPort.Read(recvData, 0, recvDataSize);
                    Log.WriteLog(LogLevel.INFO, programName, functionName, "recvData", (string)ConvertUtil.convert("B2HS", recvData, recvDataSize));
                    parser(recvData);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
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

        private bool sendToRtu(byte[] bytesData, int len)
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
            string functionName = "parser";
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {

                if (recvData[0] == 0x02 && recvData[1] == 0x11 && recvData[2] == 0x21)
                {
                    if(recvData[3]==0x01 && recvData[4] == 0x1C)
                    {
                        //BIT 상태 묻기CMD_CK_STAT
                        bitstat();
                        idon_rtuEventHandler(this, new RtuEventArgs("CMD_CK_STAT", null));
                    }
                    else if (recvData[3] == 0x05 && recvData[4] == 0x07)
                    {
                        //IMPACT 충격 감지기
                        //충격감지 02 11 21 05 07 03 19 0A 02 22 3F 
                        //충격감지 이벤트 발생
                        //processEventHandler(this, new ProcessEventArgs("IMPACT", null));
                        if ((DateTime.Now - impacttime).TotalMilliseconds > sleepImpactTime)
                        {

                            idon_rtuEventHandler(this, new RtuEventArgs("IMPACT", null));
                            impacttime = DateTime.Now;

                        }
                       
                    }

                    else if (recvData[3] == 0x02 && recvData[4] == 0x0E)
                    {
                        //DC /
                        if (recvData[21] == 0x01)
                        {
                            // ON
                            rtuStatus.DC[4] = true;
                        }
                        else
                        {
                            //false == OFF
                            rtuStatus.DC[4] = false;
                        }
                    }

                    else if (recvData[3] == 0x05 && recvData[4] == 0x05)
                    {
                        //KEY 이벤트
                        EnumKeyPress keyNum;
                        if (recvData[6] == 0x01)//음성
                        {
                            //02 11 21 05 05 18 01 02 02 02 02 02 02 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 29 3F 
                            keyNum = EnumKeyPress.SOUND;
                        }
                        else if (recvData[7] == 0x01)//확인
                        {
                            //02 11 21 05 05 18 02 01 02 02 02 02 02 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 29 3F 
                            keyNum = EnumKeyPress.CONFIRM;
                        }
                        else if (recvData[8] == 0x01)//우
                        {
                            //02 11 21 05 05 18 02 02 01 02 02 02 02 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 29 3F 
                            keyNum = EnumKeyPress.RIGHT;
                        }
                        else if (recvData[9] == 0x01)//하
                        {
                            //02 11 21 05 05 18 02 02 02 01 02 02 02 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 29 3F 
                            keyNum = EnumKeyPress.DOWN;
                        }
                        else if (recvData[10] == 0x01)//상
                        {
                            //02 11 21 05 05 18 02 02 02 02 01 02 02 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 29 3F 
                            keyNum = EnumKeyPress.UP;
                        }
                        else if (recvData[11] == 0x01)//좌
                        {
                            //02 11 21 05 05 18 02 02 02 02 02 01 02 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 29 3F 
                            keyNum = EnumKeyPress.LEFT;
                        }
                        else if (recvData[12] == 0x01)//메뉴
                        {
                            //02 11 21 05 05 18 02 02 02 02 02 02 01 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 29 3F 
                            keyNum = EnumKeyPress.MENU;
                        }
                        else
                        {
                            keyNum = EnumKeyPress.NO;
                        }

                        idon_rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                    }

                    else if (recvData[3] == 0x02 && recvData[4] == 0x01)
                    {
                        //도어 02 11 21 02 01 04 02 02 00 00 35 3F 
                        //도어 02 11 21 05 01 04 02 01 00 00 31 3F 
                        //도어 02 11 21 05 01 04 02 02 00 00 32 3F 
                        // 02 11 21 05 01 04 01 02 00 00 31 3F

                        //DOOR1
                        if (recvData[6] == 0x01)
                        {

                            rtuStatus.DOOR[0] = true;


                        }
                        else
                        {

                            rtuStatus.DOOR[0] = false;
                        }

                        //DOOR2
                        if (recvData[7] == 0x01)
                        {
                            //OPEN
                            rtuStatus.DOOR[1] = true;
                        }
                        else
                        {
                            rtuStatus.DOOR[1] = false;
                        }
                    }
                    else if (recvData[3] == 0x02 && recvData[4] == 0x02)
                    {
                        //온도 02 11 21 02 02 09 00 1D 00 00 00 00 00 00 00 26 3F 
                        //온도 02 11 21 02 02 09 00 1B 4B 00 00 00 00 00 00 6B 3F 

                        //TEMP
                        rtuStatus.TEMP[0] = recvData[6]; //0이면 영상온도
                        rtuStatus.TEMP[1] = recvData[7];
                        rtuStatus.TEMP[2] = recvData[8];

                    }
                    else if (recvData[3] == 0x02 && recvData[4] == 0x03)
                    {
                        //습도 02 11 21 02 03 03 20 00 00 10 3F 

                        //HUMI
                        rtuStatus.HUMI = (int)recvData[6];
                    }

                    else if (recvData[3] == 0x02 && recvData[4] == 0x0F)
                    {
                        //AC 02 11 21 02 0F 10 01 01 01 01 02 02 00 00 00 00 00 00 00 00 00 00 2F 3F 

                        //AC
                        if (recvData[6] == 0x01)
                        {
                            //ON
                            rtuStatus.AC[0] = true;
                        }
                        else
                        {
                            rtuStatus.AC[0] = false;
                        }

                        if (recvData[7] == 0x01)
                        {
                            //ON
                            rtuStatus.AC[1] = true;
                        }
                        else
                        {
                            rtuStatus.AC[1] = false;
                        }

                        if (recvData[8] == 0x01)
                        {
                            //ON
                            rtuStatus.AC[2] = true;
                        }
                        else
                        {
                            rtuStatus.AC[2] = false;
                        }

                        if (recvData[9] == 0x01)
                        {
                            //ON
                            rtuStatus.AC[3] = true;
                        }
                        else
                        {
                            rtuStatus.AC[3] = false;
                        }
                        if (recvData[10] == 0x01)
                        {
                            //ON
                            rtuStatus.AC[4] = true;
                        }
                        else
                        {
                            rtuStatus.AC[4] = false;
                        }

                        if (recvData[11] == 0x01)
                        {
                            //ON
                            rtuStatus.AC[5] = true;
                        }
                        else
                        {
                            rtuStatus.AC[5] = false;
                        }
                    }


                    else if (recvData[3] == 0x02 && recvData[4] == 0x0C)
                    {
                        //FAN 02 11 21 02 0C 14 02 00 02 00 02 00 02 00 02 00 00 00 00 00 00 00 00 00 00 00 2A 3F 

                        //FAN
                        if (recvData[6] == 0x01)
                        {
                            //ON
                            rtuStatus.FAN[0] = true;
                        }
                        else
                        {
                            rtuStatus.FAN[0] = false;
                        }

                        if (recvData[7] == 0x01)
                        {
                            //ON
                            rtuStatus.FAN[1] = true;
                        }
                        else
                        {
                            rtuStatus.FAN[1] = false;
                        }

                        if (recvData[8] == 0x01)
                        {
                            //ON
                            rtuStatus.FAN[2] = true;
                        }
                        else
                        {
                            rtuStatus.FAN[2] = false;
                        }

                        if (recvData[9] == 0x01)
                        {
                            //ON
                            rtuStatus.FAN[3] = true;
                        }
                        else
                        {
                            rtuStatus.FAN[3] = false;
                        }

                    }
                    else if (recvData[3] == 0x07 && recvData[4] == 0x0C)
                    {
                        //02-21-11-07-0C-00-39-3F
                        //팬모드 02 11 21 08 0C 0D 00 1E 01 00 00 00 00 00 00 00 00 00 00 24 3F 
                        if (recvData[8] == 0x01)
                        {
                            //자동
                            rtuStatus.FAN_MODE = 0x00;
                        }
                        else
                            //수동
                            rtuStatus.FAN_MODE = 0x01;
                    }
                    else if (recvData[3] == 0x02 && recvData[4] == 0x08)
                    {
                        //사운드
                        //02-21-11-01-08-00-3B-3F
                        //02 11 21 02 08 03 01 1D 00 27 3F 
                        if (recvData[6] == 0x02)
                        {
                            rtuStatus.Sound_MUTE = 0x01;
                        }
                        else
                        {
                            rtuStatus.Sound_MUTE = 0x00;
                        }
                        int sound = (int)recvData[7] / 3;
                        rtuStatus.Sound_VOL = (byte)sound;


                    }
                    else if (recvData[3] == 0x01 && recvData[4] == 0x08)
                    {
                        //히터 모드
                        //02-21-11-07-0D-00-38-3F
                        //02 11 21 08 0D 0E 00 05 44 01 00 00 00 00 00 00 00 00 00 00 79 3F 
                        if (recvData[9] == 0x02)
                        {
                            //rtu수동모드 2 , 대전은 1
                            rtuStatus.HEATER_MODE = 0x01;
                        }
                        else
                        {
                            rtuStatus.HEATER_MODE = 0x00;
                        }
                    }
                    else if (recvData[3] == 0x05 && recvData[4] == 0x1E)
                    {
                        //프로세스재시작
                        int sleep = (int)recvData[6];
                        Thread.Sleep(sleep * 1000);
                        idon_rtuEventHandler(this, new RtuEventArgs("reStart", null));

                    }
                    else if (recvData[3] == 0x05 && recvData[4] == 0x1F)
                    {
                        //DMB 제어
                        int dmbstatus = 0;
                        if (recvData[6] == 0x01)
                        {
                            dmbstatus = 1;
                        }
                        else
                        {
                            dmbstatus = 0;
                        }
                        idon_rtuEventHandler(this, new RtuEventArgs("DMBONOFF", (int)dmbstatus));

                    }
                    else if (recvData[3] == 0x05 && recvData[4] == 0x20)
                    {
                        //Capture 제어
                        idon_rtuEventHandler(this, new RtuEventArgs("CAPTURE", (int)recvData[6]));
                    }
                    else if (recvData[3] == 0x02 && recvData[4] == 0x21)
                    {
                        
                        //LCD
                        if (recvData[6] == 0x05 || recvData[6] == 0x0B)
                        {
                            //ON
                            DateTime time = DateTime.Now;
                            if (time.Hour >= 17 || time.Hour < 06)
                            {
                                rtuStatus.LCD_RGB_Status = 0x33; 
                            }
                            else
                                rtuStatus.LCD_RGB_Status = 0x31;
                            //OFF
                        }
                        else
                            rtuStatus.LCD_RGB_Status = 0x30; //무조건 까만 화면

                    }
                    else if (recvData[3] == 0x05 && recvData[4] == 0x22)
                    {
                        //웹하면 Reload
                        idon_rtuEventHandler(this, new RtuEventArgs("Reload", (int)recvData[6]));
                    }
                    else if (recvData[3] == 0x01 && recvData[4] == 0x10)
                    {
                        //시간 요청 동기화 BIT , RTU간의
                        RTC();
                        idon_rtuEventHandler(this, new RtuEventArgs("RTC", null));
                    }
                }
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
            }
        }

        private byte[] makeSendData(byte OpCode, byte CMD,byte[] data)
        {
            string functionName = "makeSendData";
            byte[] sendData = new byte[9];

            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                sendData[0] = STX;
                sendData[1] =0x21;
                sendData[2] = 0x11;
                sendData[3] = OpCode;
                sendData[4] = CMD;
                for (int i = 5; i < 5 + data.Length; i++)
                {
                    
                }
              //  sendData[7] = makeCheckSum(ID, OpCode, data);
                sendData[8] = ETX;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
            }

            return sendData;
        }

        private byte makeCheckSum(byte[] data)
        {
            string functionName = "makeCheckSum";
            byte CheckSum = 0x00;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                for (int i = 0; i < data.Length; i++)
                {
                    CheckSum = (byte)((int)CheckSum ^ (int)data[i]);
                }
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                CheckSum = 0;
            }

            return CheckSum;
        }

        public RtuPropertys getPropertys()
        {
            return this.rtuPropertys;
        }

        private void lcd_bright()
        {
            lcdcheck = 17;
            string functionName = "lcd_bright";
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                sendToRtu(ConvertUtil.hexStringToByte("02211103240400640000753F"), 12);
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
                sendToRtu(ConvertUtil.hexStringToByte("02211103240400320000233F"), 12);
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
                sendToRtu(ConvertUtil.hexStringToByte("022111021C000C3F"), 8);
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
            }
        }

        private void RTC()
        {
            string functionName = "RTC";
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                byte dateTimeHH = (byte)DateTime.Now.Hour;
                byte dateTimemm = (byte)DateTime.Now.Minute;
                byte dateTimess = (byte)DateTime.Now.Second;

                byte[] datetime = { (byte)DateTime.Now.Hour, (byte)DateTime.Now.Minute, (byte)DateTime.Now.Second };
                string date = BitConverter.ToString(datetime);
                date = date.Replace("-", "");
                string data1 = "022111021003" + date;
                string cksum = makeCheckSum(ConvertUtil.hexStringToByte(data1)).ToString("X");
                if (cksum.Length  == 1)
                {
                    cksum="0"+cksum;
                }

                sendToRtu(ConvertUtil.hexStringToByte(data1 + cksum+"3F"), 11);
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
            }
        }

        #region Rtu 조회
        private bool getTEMP()
        {
            string functionName = "getStatusTEMPERATURE";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("022111010200313F"), 8);
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
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("022111010300303F"), 8);
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

        private bool getFANMODE()
        {
            string functionName = "getStatusFANMODE";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("022111070C00393F"), 8);
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
        private bool getAC()
        {
            string functionName = "getStatusAC";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("022111010F003C3F"), 8);
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
        private bool getDC()
        {
            string functionName = "getStatusDC";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("022111010E003D3F"), 8);
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
        private bool getDOOR()
        {
            string functionName = "getStatusDOOR";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("022111010100323F"), 8);
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
        private bool getSOUND()
        {
            string functionName = "getStatusSOUND";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("0221110108003B3F"), 8);
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
        private bool getHEATERMODE()
        {
            string functionName = "getStatusHEATERMODE";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("022111070D00383F"), 8);
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
        private bool getLCDSTATUS()
        {
            string functionName = "getLCDSTATUS";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                sendToRtu(ConvertUtil.hexStringToByte("022111012100123F"), 8);
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
                sendToRtu(ConvertUtil.hexStringToByte("022111030A01013B3F"), 9);
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


        #region SubFunction

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
            if (time.Hour >= 17 || time.Hour < 6)
            {
                lcd_dark();
                //lcdcheck = 6;
            }
            else
            {
                lcd_bright();
               // lcdcheck = 17;
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

                        }
                }
                catch (Exception e)
                {
                    Log.WriteLog(LogLevel.TRACE, programName, functionName, "Exception", e.ToString());
                }
                try
                {
                    time = DateTime.Now;
                    //아침 LCD체크
                    if (time.Hour == lcdcheck)
                    {
                        if (lcdcheck == 17)
                        {
                            lcd_dark();
                        }
                        else if (lcdcheck == 6)
                        {
                            lcd_bright();
                        }

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
