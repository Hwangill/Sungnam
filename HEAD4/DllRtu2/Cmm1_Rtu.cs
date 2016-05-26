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
using DllRtu.Cmm1RtuInterface;
using DllRtu.Config;

namespace DllRtu
{

    public class Cmm1_Rtu : Cmm1_RtuInterface
    {
        private string programName = "Cmm1_Rtu";

        private const int sleepTime = 100;
        private int renewCycleTime = 60;

        private bool rtuThreadStatus = false;
        private bool sendComplete = true;

        private SerialPort serialPort = null;
        private bool serialError;

        private string configFile = "./config/rtuPropertys.xml";
        #region 각데이터 정의

        //ACK/NAK
        private const byte ACK = 0x06;
        private const byte NAK = 0x05;
        private byte[] ACKB = { 0x06 };
        private byte[] NAKB = { 0x05 };

        private const byte dataSTX0 = 0x02;
        private const byte dataSTX1 = 0xFD;
        private const byte dataETX = 0x03;
        private const byte dataCommonReq = 0x31;

        private const byte ETHOST = 0x11;// Mean BIT main board; aka BIT Application
        private const byte ETSUB = 0x21;// Mean Env Control board	

        private const byte ETDoor = 0x41;
        private const byte ETTempHumi = 0x42;
        private const byte ETCDS = 0x43;
        private const byte ETMIC = 0x44;
        private const byte ETSPK = 0x45;
        private const byte ETKEY = 0x46;

        private const byte ETReserved0 = 0x47;
        private const byte ETReserved1 = 0x48;
        private const byte ETReserved2 = 0x49;

        private const byte ETFAN = 0x51;
        private const byte ETHEATER = 0x52;
        private const byte ETLCD = 0x53;
        private const byte ETLED = 0x54;
        private const byte ETDMB = 0x55;
        private const byte ETROUTE = 0x56;//모뎀

        private const byte ETReserved3 = 0x57;
        private const byte ETReserved4 = 0x58;
        private const byte ETReserved5 = 0x59;

        private const byte ETBITPWR = 0x61;//BIT 리셋
        private const byte ETMAINRST = 0x62;//전원 예약관련
        private const byte ETRESV_ACPWR = 0x63;//AC 상태

        private const byte ETReserved6 = 0x64;
        private const byte ETReserved7 = 0x65;
        private const byte ETReserved8 = 0x66;
        private const byte ETReserved9 = 0x67;
        private const byte ETReserved10 = 0x68;
        private const byte ETReserved11 = 0x69;


        private const byte ECLCD0 = 0x70;
        private const byte ECLCD1 = 0x71;//LCD 밝기제어
        private const byte ECLCD2 = 0x72;//LCD. 상태
        private const byte ECLCD3 = 0x73;//LCD 전원제어

        private const byte ECSPK0 = 0x80;//스피커상태
        private const byte ECSPK1 = 0x81;//스피커제어
        private const byte ECSPK2 = 0x82;//뮤트상태
        private const byte ECSPK3 = 0x83;//뮤트제어

        private const byte ECDMB0 = 0x90;//외장 DMB 전원상태
        private const byte ECDMB1 = 0x91;//POWER
        private const byte ECDMB2 = 0x92;//SCAN
        private const byte ECDMB3 = 0x93;
        private const byte ECDMB4 = 0x94;//CHANNEL
        private const byte ECDMB5 = 0x95;//무신호
        private const byte ECDMB6 = 0x96;

        private const byte ECReqStatus = 0xA0;
        private const byte ECReqCount = 0xB0;
        private const byte ECReqControl = 0xC0;

        private const byte ECACK = 0x06;
        private const byte ECNAK = 0x05;

        private const byte ECDataLCD0 = 0x7A;
        private const byte ECDataLCD1 = 0x7B;
        private const byte ECDataLCD2 = 0x7C;

        private const byte ECDataSPK0 = 0x8A;//스피커 카운트
        private const byte ECDataSPK1 = 0X8B;
        private const byte ECDataSPK2 = 0x8C;

        private const byte ECDataDMB0 = 0x9A;
        private const byte ECDataDMB1 = 0X9B;
        private const byte ECDataDMB2 = 0X9C;
        private const byte ECDataDMB3 = 0x9D;

        private const byte ECDataStatus = 0xAA;
        private const byte ECDataCount = 0xBA;
        private const byte ECDAtaControl = 0xCA;
        private const byte ECDataEvent = 0xEA;


        #endregion

        private byte[] sendMSG = new byte[256];


        private List<Cmm1_RtuMath> listRtuMath = new List<Cmm1_RtuMath>();
        private static RtuStatus rtuStatus = new RtuStatus();
        private static RtuStatus rtuControl = new RtuStatus();
        private static RtuParamater rtuParamater = new RtuParamater();

        private RtuPropertys rtuPropertys = null;
        public event Cmm1_RtuEventHandler rtuEventHandler;

        public bool detailMode = true;


        public Cmm1_Rtu()
        {
            initRtu();
        }

        public Cmm1_Rtu(string configFile)
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

                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.AC0, EnumControlName.AC, 0));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.AC1, EnumControlName.AC, 1));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.AC2, EnumControlName.AC, 2));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.AC3, EnumControlName.AC, 3));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.AC4, EnumControlName.AC, 4));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.AC5, EnumControlName.AC, 5));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.DC0, EnumControlName.DC, 0));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.DC1, EnumControlName.DC, 1));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.DC2, EnumControlName.DC, 2));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.DC3, EnumControlName.DC, 3));
                rtuStatus.SoundAmp = (byte)int.Parse(rtuPropertys.SoundAmp);
                this.renewCycleTime = int.Parse(rtuPropertys.RenewCycleTime);

                rtuParamater.HeaterActTemperature = int.Parse(rtuPropertys.HeaterActionTempature);
                rtuParamater.FanAct1Temperature = int.Parse(rtuPropertys.Fan1ActionTempature);
                rtuParamater.FanAct2Temperature = int.Parse(rtuPropertys.Fan2ActionTempature);

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
                listRtuMath.Clear();
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.AC0, EnumControlName.AC, 0));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.AC1, EnumControlName.AC, 1));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.AC2, EnumControlName.AC, 2));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.AC3, EnumControlName.AC, 3));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.AC4, EnumControlName.AC, 4));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.AC5, EnumControlName.AC, 5));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.DC0, EnumControlName.DC, 0));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.DC1, EnumControlName.DC, 1));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.DC2, EnumControlName.DC, 2));
                listRtuMath.Add(new Cmm1_RtuMath(rtuPropertys.DC3, EnumControlName.DC, 3));
                rtuStatus.SoundAmp = (byte)int.Parse(rtuPropertys.SoundAmp);
                this.renewCycleTime = int.Parse(rtuPropertys.RenewCycleTime);

                rtuParamater.HeaterActTemperature = int.Parse(rtuPropertys.HeaterActionTempature);
                rtuParamater.FanAct1Temperature = int.Parse(rtuPropertys.Fan1ActionTempature);
                rtuParamater.FanAct2Temperature = int.Parse(rtuPropertys.Fan2ActionTempature);

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
                return;
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

        public bool sendToRtu(RtuSendData rtuSendData)
        {
            return sendToRtu(rtuSendData.sendData, rtuSendData.sendDataLen);
        }

        private bool sendToRtu(byte[] bytesData, int len)
        {
            string functionName = "sendToRtu";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                sendComplete = false;
                for (int loopCount = 0; loopCount < 3; loopCount++)
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

        public void parser(byte[] recvData)
        {
            string functionName = "parser";
            byte recvID;
            byte sendID;
            byte cmd;
            byte[] data;
            int dataLen;
            byte checkSum;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                if (recvData.Length == 0)
                {
                    return;
                }
                if (recvData[0] == ACK || recvData[0] == NAK)
                {
                    sendComplete = true;
                    return;
                }
                else if (recvData[0] == 0x21)
                {
                    parser(ConvertUtil.subBytes(recvData, 1, recvData.Length - 1));
                    return;
                }
                else if (recvData[0] == dataSTX0 && recvData[1] == dataSTX1)
                {
                    recvID = recvData[2];
                    sendID = recvData[3];
                    cmd = recvData[4];
                    dataLen = (int)recvData[5];

                    if (recvData.Length < dataLen + 6)
                    {
                        return;
                    }


                    data = new byte[dataLen];


                    for (int loopCount = 0; loopCount < dataLen; loopCount++)
                    {
                        data[loopCount] = recvData[6 + loopCount];
                    }

                    checkSum = recvData[6 + dataLen];

                    if (checkSum == makeCheckSum(recvData, 6 + dataLen))
                    {
                        if (cmd == ECDataEvent)
                        {
                            this.serialPort.Write(ACKB, 0, ACKB.Length);
                        }
                    }
                    else
                    {
                        if (cmd == ECDataEvent)
                        {
                            this.serialPort.Write(NAKB, 0, NAKB.Length);
                        }
                        Log.WriteLog(LogLevel.ERROR, programName, functionName, "CheckSum", "[" + ConvertUtil.byteToHexString(checkSum) + "][" + ConvertUtil.byteToHexString(makeCheckSum(recvData, 7 + dataLen)) + "]");
                        return;
                    }

                    sendComplete = true;

                    RtuSendData rtuSendData = new RtuSendData(recvData, recvData.Length);
                    if (detailMode)
                    {
                        rtuEventHandler(this, new RtuEventArgs("RecvData", (object)rtuSendData));
                    }

                    switch (sendID)
                    {
                        case ETSUB:
                            break;
                        case ETDoor:
                            for (int loopCount = 0; loopCount < rtuStatus.Door_Count; loopCount++)
                            {
                                if (data[loopCount] == 0x30)
                                {
                                    rtuStatus.setDoor(loopCount, false);
                                }
                                else
                                {
                                    rtuStatus.setDoor(loopCount, true);
                                }
                            }

                            rtuEventHandler(this, new RtuEventArgs("Door", (object)rtuStatus));

                            break;
                        case ETTempHumi:
                            char[] charArray = new char[dataLen];

                            for (int loopCount = 0; loopCount < dataLen; loopCount++)
                            {
                                charArray[loopCount] = (char)data[loopCount];
                            }



                            rtuStatus.Temperature = float.Parse(new string(charArray, 0, 6));
                            rtuStatus.STemperature = (new string(charArray, 0, 6));
                            rtuStatus.Humidity = float.Parse(new string(charArray, 6, 6));
                            rtuStatus.SHumidity = (new string(charArray, 6, 6));

                            rtuEventHandler(this, new RtuEventArgs("Temperature", (object)rtuStatus));
                            break;
                        case ETKEY:

                            EnumKeyPress keyNum;
                            if (data[0] != 0x31)//음성
                            {
                                keyNum = EnumKeyPress.SOUND;


                                rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                            }
                            else if (data[1] != 0x31)//확인
                            {
                                keyNum = EnumKeyPress.CONFIRM;


                                rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                            }
                            else if (data[2] != 0x31)//우
                            {
                                keyNum = EnumKeyPress.RIGHT;


                                rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                            }
                            else if (data[3] != 0x31)//하
                            {
                                keyNum = EnumKeyPress.DOWN;


                                rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                            }
                            else if (data[4] != 0x31)//상
                            {
                                keyNum = EnumKeyPress.UP;


                                rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                            }
                            else if (data[5] != 0x31)//좌
                            {
                                keyNum = EnumKeyPress.LEFT;

                                rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                            }
                            else if (data[6] != 0x31)//메뉴
                            {
                                keyNum = EnumKeyPress.MENU;

                                rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));
                            }

                            else
                            {
                                keyNum = EnumKeyPress.NO;
                            }
                            //rtuEventHandler(this, new RtuEventArgs("KeyPress", (object)keyNum));

                            break;
                        case ETFAN:
                            for (int loopCount = 0; loopCount < rtuStatus.DC_.Length; loopCount++)
                            {
                                if (data[loopCount] == 0x30)
                                {
                                    rtuStatus.setDC(loopCount, false);
                                }
                                else
                                {
                                    rtuStatus.setDC(loopCount, true);
                                }
                            }
                            rtuEventHandler(this, new RtuEventArgs("DC", (object)rtuStatus));
                            break;
                        case ETRESV_ACPWR:
                            for (int loopCount = 0; loopCount < rtuStatus.AC_.Length; loopCount++)
                            {
                                if (data[loopCount] == 0x30)
                                {
                                    rtuStatus.setAC(loopCount, false);
                                }
                                else
                                {
                                    rtuStatus.setAC(loopCount, true);
                                }
                            }
                            rtuEventHandler(this, new RtuEventArgs("AC", (object)rtuStatus));
                            break;
                        case ETSPK: //스피커,뮤트

                            if (cmd == ECDataSPK0)
                            {
                                rtuStatus.SoundAmp = data[0];
                                rtuEventHandler(this, new RtuEventArgs("SoundAmp", (object)rtuStatus));
                            }
                            else if (cmd == ECDataSPK2)
                            {
                                if (data[0] == 0x30)
                                {
                                    rtuStatus.SoundMute = false;
                                }
                                else
                                {
                                    rtuStatus.SoundMute = true;
                                }
                                rtuEventHandler(this, new RtuEventArgs("SoundMute", (object)rtuStatus));
                            }
                            break;
                        case ETMIC: // 충격감지 
                            int impactLevel = data[0] - 0x30;
                            for (int i = 9; i < recvData.Length; i = i + 9)
                            {
                                if (impactLevel < (recvData[i - 3] - 0x30))
                                {
                                    impactLevel = recvData[i - 3] - 0x30;
                                }
                            }

                            rtuEventHandler(this, new RtuEventArgs("Impact", (object)impactLevel.ToString()));
                            break;

                        default:
                            break;
                    }

                    /*
                    if (recvData.Length - dataLen - 8 > 0)
                    {
                        parser(ConvertUtil.subBytes(recvData, 8 + dataLen, recvData.Length - dataLen - 8));
                    }
                    */

                }
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
            }
        }

        private byte[] makeSendData(byte recvID, byte sendID, byte cmd, byte[] data, int dataLen)
        {
            string functionName = "makeSendData";
            byte[] sendData = new byte[dataLen + 8];

            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                sendData[0] = dataSTX0;
                sendData[1] = dataSTX1;
                sendData[2] = recvID;
                sendData[3] = sendID;
                sendData[4] = cmd;
                sendData[5] = (byte)dataLen;
                for (int loopCount = 0; loopCount < dataLen; loopCount++)
                {
                    sendData[6 + loopCount] = data[loopCount];
                }

                sendData[6 + dataLen] = makeCheckSum(sendData, 7 + dataLen);
                sendData[7 + dataLen] = dataETX;
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
            }

            return sendData;
        }

        private byte makeCheckSum(byte[] sendData, int len)
        {
            string functionName = "makeCheckSum";
            byte CheckSum = 0x00;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                for (int loopCount = 0; loopCount < len; loopCount++)
                {
                    CheckSum = (byte)((int)CheckSum ^ (int)sendData[loopCount]);
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

        #region Rtu 조회 MakeData
        public RtuSendData makeInquiryDoor()
        {
            string functionName = "makeDoor";
            RtuSendData returnValue = null;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                int dataLen = 1;
                byte[] data = new byte[dataLen];
                data[0] = dataCommonReq;
                returnValue = new RtuSendData(makeSendData(ETDoor, ETHOST, ECReqStatus, data, dataLen), dataLen + 8);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = null;
            }

            return returnValue;
        }
        public RtuSendData makeInquiryTemperature()
        {
            string functionName = "makeInquiryTemperature";
            RtuSendData returnValue = null;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                int dataLen = 1;
                byte[] data = new byte[dataLen];
                data[0] = dataCommonReq;
                returnValue = new RtuSendData(makeSendData(ETTempHumi, ETHOST, ECReqStatus, data, dataLen), dataLen + 8);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = null;
            }

            return returnValue;
        }
        public RtuSendData makeInquirySoundAmp()
        {
            string functionName = "makeInquirySoundAmp";
            RtuSendData returnValue = null;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                int dataLen = 1;
                byte[] data = new byte[dataLen];
                data[0] = dataCommonReq;
                returnValue = new RtuSendData(makeSendData(ETSPK, ETHOST, ECSPK0, data, dataLen), dataLen + 8);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = null;
            }

            return returnValue;
        }
        public RtuSendData makeInquirySoundMute()
        {
            string functionName = "makeInquirySoundMute";
            RtuSendData returnValue = null;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                int dataLen = 1;
                byte[] data = new byte[dataLen];
                data[0] = dataCommonReq;
                returnValue = new RtuSendData(makeSendData(ETSPK, ETHOST, ECSPK2, data, dataLen), dataLen + 8);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = null;
            }

            return returnValue;
        }
        public RtuSendData makeInquiryAC()
        {
            string functionName = "makeInquiryAC";
            RtuSendData returnValue = null;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                int dataLen = 1;
                byte[] data = new byte[dataLen];
                data[0] = dataCommonReq;
                returnValue = new RtuSendData(makeSendData(ETRESV_ACPWR, ETHOST, ECReqStatus, data, dataLen), dataLen + 8);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = null;
            }

            return returnValue;
        }
        public RtuSendData makeInquiryDC()
        {
            string functionName = "makeInquiryDC";
            RtuSendData returnValue = null;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                int dataLen = 1;
                byte[] data = new byte[dataLen];
                data[0] = dataCommonReq;
                returnValue = new RtuSendData(makeSendData(ETFAN, ETHOST, ECReqStatus, data, dataLen), dataLen + 8);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = null;
            }

            return returnValue;
        }
        #endregion

        #region Rtu 조회
        private bool inquiryDoor()
        {
            string functionName = "inquiryDoor";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = sendToRtu(makeInquiryDoor());
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool inquiryTemperature()
        {
            string functionName = "inquiryTemperature";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = sendToRtu(makeInquiryTemperature());
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool inquirySoundAmp()
        {
            string functionName = "inquirySoundAmp";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = sendToRtu(makeInquirySoundAmp());
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool inquirySoundMute()
        {
            string functionName = "inquirySoundMute";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = sendToRtu(makeInquirySoundMute());
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool inquiryAC()
        {
            string functionName = "inquiryAC";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = sendToRtu(makeInquiryAC());
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool inquiryDC()
        {
            string functionName = "inquiryDC";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = sendToRtu(makeInquiryDC());
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool inquiryACDC()
        {
            string functionName = "inquiryACDC";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = sendToRtu(makeInquiryAC());
                Thread.Sleep(sleepTime);
                returnValue = returnValue && sendToRtu(makeInquiryDC());
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

        #region Rtu 제어 MakeData
        public RtuSendData makeControlSoundAmp(byte soundAmp)
        {
            string functionName = "makeControlSoundAmp";
            RtuSendData returnValue = null;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                int dataLen = 2;
                byte[] data = new byte[dataLen];
                data[0] = 0x31;
                data[1] = soundAmp;
                returnValue = new RtuSendData(makeSendData(ETSPK, ETHOST, ECSPK1, data, dataLen), dataLen + 8);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = null;
            }

            return returnValue;
        }
        public RtuSendData makeControlSoundMute(bool soundMute)
        {
            string functionName = "makeControlSoundMute";
            RtuSendData returnValue = null;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                int dataLen = 1;
                byte[] data = new byte[dataLen];
                if (soundMute)
                {
                    data[0] = 0x31;
                }
                else
                {
                    data[0] = 0x30;
                }
                returnValue = new RtuSendData(makeSendData(ETSPK, ETHOST, ECSPK3, data, dataLen), dataLen + 8);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = null;
            }

            return returnValue;
        }
        public RtuSendData makeControlAC()
        {
            string functionName = "makeControlAC";
            RtuSendData returnValue = null;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                int dataLen = 6;
                byte[] data = new byte[dataLen];
                data[0] = convertStatus2Byte(rtuControl.getAC(0));
                data[1] = convertStatus2Byte(rtuControl.getAC(1));
                data[2] = convertStatus2Byte(rtuControl.getAC(2));
                data[3] = convertStatus2Byte(rtuControl.getAC(3));
                data[4] = convertStatus2Byte(rtuControl.getAC(4));
                data[5] = convertStatus2Byte(rtuControl.getAC(5));
                returnValue = new RtuSendData(makeSendData(ETRESV_ACPWR, ETHOST, ECReqControl, data, dataLen), dataLen + 8);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = null;
            }

            return returnValue;
        }
        public RtuSendData makeControlDC()
        {
            string functionName = "makeControlDC";
            RtuSendData returnValue = null;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                int dataLen = 5;
                byte[] data = new byte[dataLen];
                data[0] = 0x30;
                data[1] = convertStatus2Byte(rtuControl.getDC(0));
                data[2] = convertStatus2Byte(rtuControl.getDC(1));
                data[3] = convertStatus2Byte(rtuControl.getDC(2));
                data[4] = convertStatus2Byte(rtuControl.getDC(3));
                returnValue = new RtuSendData(makeSendData(ETFAN, ETHOST, ECReqControl, data, dataLen), dataLen + 8);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = null;
            }

            return returnValue;
        }
        public RtuSendData makeControlRtuReset()
        {
            string functionName = "makeControlDC";
            RtuSendData returnValue = null;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                int dataLen = 5;
                byte[] data = new byte[dataLen];
                data[0] = 0x30;
                data[1] = convertStatus2Byte(rtuControl.getDC(0));
                data[2] = convertStatus2Byte(rtuControl.getDC(1));
                data[3] = convertStatus2Byte(rtuControl.getDC(2));
                data[4] = convertStatus2Byte(rtuControl.getDC(3));
                returnValue = new RtuSendData(makeSendData(ETFAN, ETHOST, ECReqControl, data, dataLen), dataLen + 8);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = null;
            }

            return returnValue;
        }
        public RtuSendData makeControlPcReset()
        {
            string functionName = "makeControlDC";
            RtuSendData returnValue = null;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                int dataLen = 5;
                byte[] data = new byte[dataLen];
                data[0] = 0x30;
                data[1] = convertStatus2Byte(rtuControl.getDC(0));
                data[2] = convertStatus2Byte(rtuControl.getDC(1));
                data[3] = convertStatus2Byte(rtuControl.getDC(2));
                data[4] = convertStatus2Byte(rtuControl.getDC(3));
                returnValue = new RtuSendData(makeSendData(ETFAN, ETHOST, ECReqControl, data, dataLen), dataLen + 8);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = null;
            }

            return returnValue;
        }
        public RtuSendData makeControlPcPower()
        {
            string functionName = "makeControlDC";
            RtuSendData returnValue = null;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                int dataLen = 5;
                byte[] data = new byte[dataLen];
                data[0] = 0x30;
                data[1] = convertStatus2Byte(rtuControl.getDC(0));
                data[2] = convertStatus2Byte(rtuControl.getDC(1));
                data[3] = convertStatus2Byte(rtuControl.getDC(2));
                data[4] = convertStatus2Byte(rtuControl.getDC(3));
                returnValue = new RtuSendData(makeSendData(ETFAN, ETHOST, ECReqControl, data, dataLen), dataLen + 8);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = null;
            }

            return returnValue;
        }

        #endregion

        #region Rtu 제어
        private bool controlSoundAmp(byte soundAmp)
        {
            string functionName = "controlSoundAmp";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = sendToRtu(makeControlSoundAmp(soundAmp));
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool controlSoundMute(bool soundMute)
        {
            string functionName = "controlSoundMute";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = sendToRtu(makeControlSoundMute(soundMute));
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool controlAC()
        {
            string functionName = "controlAC";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = sendToRtu(makeControlAC());
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool controlDC()
        {
            string functionName = "controlDC";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = sendToRtu(makeControlDC());
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool controlRtuReset()
        {
            string functionName = "controlRtuReset";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = sendToRtu(makeControlRtuReset());
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool controlPcReset()
        {
            string functionName = "controlPcReset";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = sendToRtu(makeControlPcReset());
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool controlPcPower()
        {
            string functionName = "controlPcPower";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = sendToRtu(makeControlPcPower());
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool controlACDC()
        {
            string functionName = "controlACDC";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = controlAC();
                Thread.Sleep(sleepTime);
                returnValue = returnValue && controlDC();
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

        #region Rtu 제어후 조회
        private bool controlAfterInquiryAC()
        {
            string functionName = "controlAfterInquiryAC";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = controlAC();
                Thread.Sleep(sleepTime);
                returnValue = returnValue && inquiryAC();
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool controlAfterInquiryDC()
        {
            string functionName = "controlAfterInquiryDC";
            bool returnValue = false;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                Thread.Sleep(sleepTime);
                returnValue = controlDC();
                Thread.Sleep(sleepTime);
                returnValue = returnValue && inquiryDC();
                Thread.Sleep(sleepTime);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private bool controlAfterInquiryACDC()
        {
            return controlAfterInquiryACDC(false);
        }
        private bool controlAfterInquiryACDC(bool execFlag)
        {
            string functionName = "controlAfterInquiryACDC";
            bool returnValue = false;
            int controlCode;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {

                if (execFlag)
                {
                    controlCode = 3;
                }
                else
                {
                    controlCode = getControlCode(true);
                }

                if (controlCode == 3)
                {// AC &DC
                    Thread.Sleep(sleepTime);
                    returnValue = controlAfterInquiryAC();
                    Thread.Sleep(sleepTime);
                    returnValue = returnValue && controlAfterInquiryDC();
                    Thread.Sleep(sleepTime);
                }
                else if (controlCode == 2)
                {//AC
                    Thread.Sleep(sleepTime);
                    returnValue = controlAfterInquiryAC();
                    Thread.Sleep(sleepTime);
                }
                else if (controlCode == 1)
                {//DC
                    Thread.Sleep(sleepTime);
                    returnValue = controlAfterInquiryDC();
                    Thread.Sleep(sleepTime);
                }
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }
        private int controlAfterInquiryDevice(EnumDeviceName enumDeviceName, bool status)
        {
            bool returnValue = false;
            try
            {
                returnValue = setDeviceControl(enumDeviceName, status);
                Thread.Sleep(sleepTime);
                returnValue = returnValue && controlACDC();
                Thread.Sleep(sleepTime);
                returnValue = returnValue && inquiryACDC();
                Thread.Sleep(sleepTime);
                if (returnValue)
                {
                    return getDeviceStatus(enumDeviceName);
                }

                return -1;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.TRACE, programName, "Exception", ex.ToString());
                return -1;
            }
        }
        #endregion

        #region SubFunction
        private int getControlCode(bool flag)
        {

            bool ac, dc;
            ac = false;
            dc = false;

            int loopCnt;
            if (flag)
            {
                for (loopCnt = 0; loopCnt < rtuStatus.AC_.Length; loopCnt++)
                {
                    if (rtuStatus.getAC(loopCnt) != rtuControl.getAC(loopCnt))
                    {
                        ac = true;
                    }
                }
                for (loopCnt = 0; loopCnt < rtuStatus.DC_.Length; loopCnt++)
                {
                    if (rtuStatus.getDC(loopCnt) != rtuControl.getDC(loopCnt))
                    {
                        dc = true;
                    }
                }
            }
            else
            {
                rtuControl.Humidity = rtuStatus.Humidity;
                rtuControl.SoundAmp = rtuStatus.SoundAmp;
                rtuControl.SoundMute = rtuStatus.SoundMute;
                rtuControl.Temperature = rtuStatus.Temperature;
                rtuControl.setAC(0, rtuStatus.getAC(0));
                rtuControl.setAC(1, rtuStatus.getAC(1));
                rtuControl.setAC(2, rtuStatus.getAC(2));
                rtuControl.setAC(3, rtuStatus.getAC(3));
                rtuControl.setAC(4, rtuStatus.getAC(4));
                rtuControl.setAC(5, rtuStatus.getAC(5));
                rtuControl.setDC(0, rtuStatus.getDC(0));
                rtuControl.setDC(1, rtuStatus.getDC(1));
                rtuControl.setDC(2, rtuStatus.getDC(2));
                rtuControl.setDC(3, rtuStatus.getDC(3));
                rtuControl.setDoor(0, rtuStatus.getDoor(0));
                rtuControl.setDoor(1, rtuStatus.getDoor(1));

            }

            if (ac && dc)
            {
                return 3;
            }
            else if (ac)
            {
                return 2;
            }
            else if (dc)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        private byte convertStatus2Byte(int status)
        {
            if (status == 1)
            {
                return 0x31;
            }
            else if (status == 0)
            {
                return 0x30;
            }
            else
            {
                return 0x30;
            }
        }
        public bool setSoundAmp(int soundAmp)
        {
            bool returnValue = false;
            try
            {
                returnValue = controlSoundAmp((byte)soundAmp);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.TRACE, programName, "Exception", ex.ToString());
                returnValue = false;
            }
            return returnValue;
        }
        public bool setSoundMute(bool soundMute)
        {
            bool returnValue = false;
            try
            {
                returnValue = controlSoundMute(soundMute);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.TRACE, programName, "Exception", ex.ToString());
                returnValue = false;
            }
            return returnValue;
        }

        private bool setDeviceControl(EnumDeviceName DeviceName, bool status)
        {
            int loopCnt;
            for (loopCnt = 0; loopCnt < listRtuMath.Count; loopCnt++)
            {
                if (listRtuMath[loopCnt].DeviceName == DeviceName)
                {
                    return rtuControl.setACDC(listRtuMath[loopCnt], status);
                }
            }
            return false;

        }
        private bool setDeviceStatus(EnumDeviceName DeviceName, bool status)
        {
            int loopCnt;
            for (loopCnt = 0; loopCnt < listRtuMath.Count; loopCnt++)
            {
                if (listRtuMath[loopCnt].DeviceName == DeviceName)
                {
                    return rtuStatus.setACDC(listRtuMath[loopCnt], status);
                }
            }
            return false;

        }

        private int getDeviceControl(EnumDeviceName DeviceName)
        {
            int loopCnt;

            for (loopCnt = 0; loopCnt < listRtuMath.Count; loopCnt++)
            {
                if (listRtuMath[loopCnt].DeviceName == DeviceName)
                {
                    return rtuControl.getACDC(listRtuMath[loopCnt]);
                }
            }

            return -1;
        }
        private int getDeviceStatus(EnumDeviceName DeviceName)
        {
            int loopCnt;

            for (loopCnt = 0; loopCnt < listRtuMath.Count; loopCnt++)
            {
                if (listRtuMath[loopCnt].DeviceName == DeviceName)
                {
                    return rtuStatus.getACDC(listRtuMath[loopCnt]);
                }
            }

            return -1;
        }

        public RtuStatus getRtuStatus()
        {
            return rtuStatus;
        }
        public RtuStatus getRtuControl()
        {
            return rtuControl;
        }
        public RtuParamater getRtuParamater()
        {
            return rtuParamater;
        }

        public void setRtuStatus(RtuStatus inStatus)
        {
            rtuStatus = inStatus;
        }
        public void setRtuControl(RtuStatus inStatus)
        {
            rtuControl = inStatus;
        }
        public void setRtuParamater(RtuParamater inParamater)
        {
            rtuParamater.FanAct1Temperature = inParamater.FanAct1Temperature;
            rtuParamater.FanAct2Temperature = inParamater.FanAct2Temperature;
            rtuParamater.HeaterActTemperature = inParamater.HeaterActTemperature;
            rtuParamater.manualFanControl = inParamater.manualFanControl;
            rtuParamater.manualHeaterControl = inParamater.manualHeaterControl;
        }

        public bool setRtuControl(string DeviceName, bool status)
        {
            return setDeviceControl(listRtuMath[0].getEnumDeviceName(DeviceName), status);
        }
        public bool setRtuStatus(string DeviceName, bool status)
        {
            return setDeviceStatus(listRtuMath[0].getEnumDeviceName(DeviceName), status);
        }

        public int getRtuControl(string DeviceName)
        {
            return getDeviceControl(listRtuMath[0].getEnumDeviceName(DeviceName));
        }
        public int getRtuStatus(string DeviceName)
        {
            return getDeviceStatus(listRtuMath[0].getEnumDeviceName(DeviceName));
        }
        #endregion



        private bool autoControlTemperature()
        {
            string functionName = "autoControlTemperature";
            float floatTemperature;
            bool returnValue = false;
            getControlCode(false);
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            try
            {
                floatTemperature = rtuStatus.Temperature;

                if (rtuParamater.FanAct2Temperature < floatTemperature)
                {
                    if (!rtuParamater.manualFanControl)
                    {
                        setDeviceControl(EnumDeviceName.Fan1, true);
                        setDeviceControl(EnumDeviceName.Fan2, true);
                    }
                    if (!rtuParamater.manualHeaterControl)
                    {
                        setDeviceControl(EnumDeviceName.Heater, false);
                        setDeviceControl(EnumDeviceName.HeaterFan, false);
                    }
                }
                else if (rtuParamater.FanAct1Temperature < floatTemperature)
                {
                    if (!rtuParamater.manualFanControl)
                    {
                        setDeviceControl(EnumDeviceName.Fan1, true);
                        setDeviceControl(EnumDeviceName.Fan2, false);
                    }
                    if (!rtuParamater.manualHeaterControl)
                    {
                        setDeviceControl(EnumDeviceName.Heater, false);
                        setDeviceControl(EnumDeviceName.HeaterFan, false);
                    }
                }
                else if (rtuParamater.HeaterActTemperature > floatTemperature)
                {
                    if (!rtuParamater.manualFanControl)
                    {
                        setDeviceControl(EnumDeviceName.Fan1, false);
                        setDeviceControl(EnumDeviceName.Fan2, false);
                    }
                    if (!rtuParamater.manualHeaterControl)
                    {
                        setDeviceControl(EnumDeviceName.Heater, true);
                        setDeviceControl(EnumDeviceName.HeaterFan, true);
                    }
                }
                else
                {
                    if (!rtuParamater.manualFanControl)
                    {
                        setDeviceControl(EnumDeviceName.Fan1, false);
                        setDeviceControl(EnumDeviceName.Fan2, false);
                    }
                    if (!rtuParamater.manualHeaterControl)
                    {
                        setDeviceControl(EnumDeviceName.Heater, true);
                        setDeviceControl(EnumDeviceName.HeaterFan, true);
                    }
                }
                returnValue = controlAfterInquiryACDC();
            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
                returnValue = false;
            }

            return returnValue;
        }


        public int runDeviceControl(EnumDeviceName device, bool status)
        {
            bool returnValue;

            returnValue = setDeviceControl(device, status);

            returnValue = controlAfterInquiryACDC();

            return getDeviceStatus(device);
        }
        public void runRtuControl()
        {
            controlAfterInquiryACDC();
        }
        #region 외부 호출 함수

        #endregion

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
            string functionName = "threadRun";
            bool returnValue;
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");

            rtuThreadStatus = true;

            if (startSerial())
            {
                returnValue = setSoundAmp(rtuStatus.SoundAmp);
                Thread.Sleep(sleepTime);
                returnValue = returnValue && inquiryACDC();
                Thread.Sleep(sleepTime);
            }

            while (rtuThreadStatus)
            {
                try
                {
                        if (startSerial())
                        {
                            returnValue = inquiryTemperature();
                            Thread.Sleep(sleepTime);
                            returnValue = returnValue && autoControlTemperature();
                            Thread.Sleep(sleepTime);
                            returnValue = setSoundAmp(rtuStatus.SoundAmp);
                            Thread.Sleep(sleepTime);
                            returnValue = returnValue && inquiryACDC();
                            Thread.Sleep(sleepTime);
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
