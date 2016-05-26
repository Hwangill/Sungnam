using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;
using Common.Utility;
using DllRtu;
using DllRtu.Event;
using DllRtu.Interface;
using DllRtu.Config;
using System.Reflection;

namespace AppRtu2
{
    public partial class AppRtu : Form
    {

        private Version version = new Version();
        private string programName = "Rtu2Form";

        private DateTime impacttime;
        private int sleepImpactTime = 10 * 1000;

        private RtuInterface rtu = null;
        private RtuPropertys rtuPropertys = null;
        private RtuStatus rtuStatus = null;
        private RtuStatus rtuStatusUpdate = null;
        private bool keyPress = false;
        private Thread rtuThread = null;

        private delegate void eventDelegateRtuStatus(RtuStatus eventStatus);
        private delegate void eventDelegateRtuSendData(RtuSendData eventSendData);
        private delegate void eventDelegateEnumKeyPress(EnumKeyPress keyNum);
        private delegate void eventDelegateString(string impactLevel);


        private const int ac_count = 6;
        private const int dc_count = 9;
        private const int serial_DataFild = 4;

        private byte fan_mode = 0x00;
        private byte heater_mode = 0x00;

        private byte[] serial_header = new byte[] { 0x02, 0xFD, 0x21, 0x11 };

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
        
        private const byte CMD_DC_STAT = 0x34;
        private const byte CMD_AC_STAT = 0x35;


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



        #endregion


        public AppRtu(RtuInterface rtu, Thread rtuThread)
        {
            CheckForIllegalCrossThreadCalls = false;
            this.rtu = rtu;
            if (rtu != null)
            {
                rtu.rtuEventHandler += new RtuEventHandler(rtuEvent);
            }
            this.rtuPropertys = rtu.getPropertys();
            this.rtuThread = rtuThread;
            InitializeComponent();
            rtuStatus = new RtuStatus();
            timerUpdate.Start();

            Assembly asm = Assembly.LoadFrom(@"AppRtu.exe");
            AssemblyName name = asm.GetName();
           // MessageBox.Show(name.Version.ToString());
            this.labelappVersion.Text = "Version : "+name.Version.ToString();

            timerTextCheck.Interval = 1*60*1000;
            timerTextCheck.Start();
            timerImpact.Interval = 500;
            timerImpact.Start();

        }

        private void rtuInit()
        {
          
            byte[] op_cmd = new byte[2] { 0x01, 0x01 };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
            op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_TEMP };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
            op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_HUMI };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
            op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_CDS };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
            op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_AC_PWR };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
            op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_DC_PWR };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
            op_cmd = new byte[2] { OPCODE_GET_SETVAL_07, CMD_FAN };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
            op_cmd = new byte[2] { OPCODE_GET_SETVAL_07, CMD_HEATER };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
            op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_DC_STAT };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
            op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_AC_STAT };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
            
   

        }

        public void rtuEvent(object sender, RtuEventArgs args)
        {
            string functionName = "rtuEvent";

            Log.WriteLog(LogLevel.TRACE, programName, functionName, "Start");
            Log.WriteLog(LogLevel.DEBUG, programName, functionName, "cmd", args.getCmd());
            try
            {
                functionName = functionName + ":" + sender.GetType().ToString() + ":" + args.getCmd();
                string cmd = args.getCmd();

                // Rtu 에서 보낸값
                #region RecvData
                if ("RecvData".Equals(cmd))
                {

                    recvDataEvent((RtuSendData)args.getParamObject());

                }
                #endregion
                #region Door
                else if ("Door".Equals(cmd))
                {
                    doorEvent((byte[])args.getParamObject());
                }
                #endregion
                #region Ipaddress
                else if ("Ipaddress".Equals(cmd))
                {
                    IPEvent((byte[])args.getParamObject());
                }
                #endregion

                #region AC
                else if ("AC_POWER".Equals(cmd))
                {
                    acEvent((RtuStatus)args.getParamObject());
                }
                #endregion
                #region AC_STAT
                else if ("acstate".Equals(cmd))
                {
                    ac_StatEvent((int[])args.getParamObject());
                   // acEvent((RtuStatus)args.getParamObject());
                }
                #endregion

                #region DC
                else if ("DC_POWER".Equals(cmd))
                {
                    dcEvent((RtuStatus)args.getParamObject());
                }
                #endregion
                #region DC_STAT
                else if ("dcstate".Equals(cmd))
                {
                    dc_StatEvent((int[])args.getParamObject());
                }
                #endregion

                #region Temperature
                else if ("Temperature".Equals(cmd))
                {
                    temperatureEvent((RtuStatus)args.getParamObject());
                }
                #endregion
                #region humi
                else if ("Hyumdity".Equals(cmd))
                {
                    humidityEvent((RtuStatus)args.getParamObject());
                }
                #endregion
                #region CDS
                else if ("testc".Equals(cmd))
                {
                    cdsEvent((RtuStatus)args.getParamObject());


                    // button3.Text = args.getParamObject().ToString();
                }
                #endregion
                #region KeyPress
                else if ("KeyPress".Equals(cmd))
                {
                    EnumKeyPress keyNum = (EnumKeyPress)args.getParamObject();
                    keyPressEvent(keyNum);
                }
                #endregion

                #region soundVolume
                else if ("soundVolume".Equals(cmd))
                {
                    soundVolume((RtuStatus)args.getParamObject());
                }
                #endregion

                #region Heater
                else if ("Heater".Equals(cmd))
                {
                    //   heaterEvent((RtuStatus)args.getParamObject());
                }
                #endregion
                #region Heater_STAT
                else if ("HeaterSTAT".Equals(cmd))
                {
                    heaterSTATEvent((RtuStatus)args.getParamObject());
                }
                #endregion


                #region FAN
                else if ("Fan".Equals(cmd))
                {
                    //fanEvent((RtuStatus)args.getParamObject());
                }
                #endregion
                #region FAN_STAT
                else if ("FanSTAT".Equals(cmd))
                {
                    fanSTATEvent((RtuStatus)args.getParamObject());
                }
                #endregion

                #region Impact
                else if ("IMPACT".Equals(cmd))
                {
                    if ((DateTime.Now - impacttime).TotalMilliseconds > sleepImpactTime)
                    {
                        impacttime = DateTime.Now;
                        impactEvent();
                    }
                }
                #endregion
                #region Impact
                else if ("lcdCheck".Equals(cmd))
                {
                    LcdChekc((byte[])args.getParamObject());
                }
                #endregion


            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
            }

            Log.WriteLog(LogLevel.TRACE, programName, functionName, "End");
        }

        private void LcdChekc(byte[] revData)
        {
            if (revData[5] == 0x01)
            {
                lcdCheckStatLable.Text = "단면형";

            }
            else if (revData[5] == 0x02)
            {
                lcdCheckStatLable.Text = "단면형";
            }
            else
            {
                lcdCheckStatLable.Text = "Error"+revData[5].ToString();
            }
        }



        private void sendDataToRtu(RtuSendData rtuSendData)
        {
            this.richTextBoxSendRecvView.AppendText("\r\n Send : " + (string)ConvertUtil.convert("B2HS", rtuSendData.sendData, rtuSendData.sendDataLen));
            this.richTextBoxSendRecvView.ScrollToCaret();
            // this.rtu.sendToRtu(rtuSendData);
        }

        private void sendDataToRtu(byte[] Byte)
        {
            this.richTextBoxSendRecvView.AppendText("\r\n Send : " + (string)ConvertUtil.convert("B2HS", Byte, Byte.Length));
            this.richTextBoxSendRecvView.ScrollToCaret();
            // this.rtu.sendToRtu(rtuSendData);
        }

        private void trackBarCtrlSoundVlume_ValueChanged(object sender, EventArgs e)
        {
            this.textBoxCtrlSoundVlume.Text = this.trackBarCtrlSoundVlume.Value.ToString();
        }


        #region Rtu 이벤트 처리

        private void fanSTATEvent(RtuStatus eventRtustatus)
        {
            StringBuilder sb = new StringBuilder();
            if (eventRtustatus.FAN_STAT[0] == 0x00)
            {
                sb.Append(" + ");
                textBoxFan1Mark.Text = "+";
            }
            else
            {
                sb.Append(" - ");
                textBoxFan1Mark.Text = "-";
            }
            sb.Append(eventRtustatus.FAN_STAT[1].ToString());
            textBoxFan1Temp.Text = eventRtustatus.FAN_STAT[1].ToString();
            fanvaluetextBox.Text = sb.ToString();


            if (eventRtustatus.FAN_STAT[2] == 0x01)
            {
                fan_mode = eventRtustatus.FAN_STAT[2];
                this.fanmodelabel.FlatStyle = FlatStyle.Popup;
                this.fanmodelabel.BackColor = Color.Pink;
                this.buttonCtrlModeFan1.FlatStyle = FlatStyle.Popup;
                this.buttonCtrlModeFan1.BackColor = Color.Pink;
                this.button_ac5.Enabled = false;
            }
            else
            {
                fan_mode = eventRtustatus.FAN_STAT[2];
                this.fanmodelabel.FlatStyle = FlatStyle.Flat;
                // this.heatermodelabel.Text = "수동";
                this.fanmodelabel.BackColor = Color.Yellow;
                this.buttonCtrlModeFan1.FlatStyle = FlatStyle.Flat;
                // this.heatermodelabel.Text = "수동";
                this.buttonCtrlModeFan1.BackColor = Color.Yellow;
                this.button_ac5.Enabled = true;
            }

        }
        private void heaterSTATEvent(RtuStatus eventRtustatus)
        {
            StringBuilder sb = new StringBuilder();
            if (eventRtustatus.HEATER_STAT[0] == 0x00)
            {
                sb.Append("온도 : + ");
                textBoxCtrlHeaterMark.Text = " + ";
            }
            else
            {
                sb.Append("온도 :  - ");
                textBoxCtrlHeaterMark.Text = " - ";
            }
            sb.Append(eventRtustatus.HEATER_STAT[1].ToString() + " , 습도 : ");
            textBoxCtrlHeaterTemp.Text = eventRtustatus.HEATER_STAT[1].ToString();
            sb.Append(eventRtustatus.HEATER_STAT[2].ToString() + " %");
            textBoxCtrlHeaterHumi.Text = eventRtustatus.HEATER_STAT[2].ToString();
            heatervaluetextBox.Text = sb.ToString();



            if (eventRtustatus.HEATER_STAT[3] == 0x01)
            {
                heater_mode = eventRtustatus.HEATER_STAT[3];
                this.heatermodelabel.FlatStyle = FlatStyle.Popup;
                this.heatermodelabel.BackColor = Color.Pink;
                this.buttonCtrlModeHeater.FlatStyle = FlatStyle.Popup;
                this.buttonCtrlModeHeater.BackColor = Color.Pink;
                this.button_ac4.Enabled = false;
            }
            else
            {
                heater_mode = eventRtustatus.HEATER_STAT[3];
                this.heatermodelabel.FlatStyle = FlatStyle.Flat;
                this.heatermodelabel.BackColor = Color.Yellow;
                this.buttonCtrlModeHeater.FlatStyle = FlatStyle.Flat;
                this.buttonCtrlModeHeater.BackColor = Color.Yellow;
                this.button_ac4.Enabled = true;
            }
        }
        private void recvDataEvent(RtuSendData eventSendData)
        {
            string functionName = "recvDataEvent";
            try
            {
                if (this.InvokeRequired == false)
                {

                    this.richTextBoxSendRecvView.AppendText("\r\n Recv : " + (string)ConvertUtil.convert("B2HS", eventSendData.sendData, eventSendData.sendDataLen));
                    this.richTextBoxSendRecvView.ScrollToCaret();
                    //02FD1121050703000000CE3F
                }
                else
                {
                    eventDelegateRtuSendData dd = new eventDelegateRtuSendData(recvDataEvent);
                    object[] t = new object[] { eventSendData };
                    this.BeginInvoke(dd, t);
                }

            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
            }
        }
        private void doorEvent(byte[] data_byte)
        {
            string functionName = "doorEvent";
            try
            {
                //둘다 오픈
                if (data_byte[5] == 0x01 && data_byte[6] == 0x01)
                {
                    door_label.ImageIndex = 3;
                }
                //윗만오픈
                else if (data_byte[5] == 0x01 && data_byte[6] == 0x02)
                {
                    door_label.ImageIndex = 1;
                }
                //아랫만오픈
                else if (data_byte[5] == 0x02 && data_byte[6] == 0x01)
                {
                    door_label.ImageIndex = 2;
                }
                //둘다 닫힘
                else if (data_byte[5] == 0x02 && data_byte[6] == 0x02)
                {
                    door_label.ImageIndex = 0;
                }


                /*
                if (this.InvokeRequired == false)
                {
                    #region Delegate
                    for (int loopCount = 0; loopCount < eventStatus.Door_Count; loopCount++)
                    {
                        if (eventStatus.getDoor(loopCount) == 1)
                        {

                            this.groupBoxCtrlStatus.Controls["labelStatusDoor_" + loopCount.ToString()].BackColor = Color.Red;
                            this.groupBoxCtrlStatus.Controls["labelStatusDoor_" + loopCount.ToString()].Text = "Open";
                        }
                        else
                        {
                            this.groupBoxCtrlStatus.Controls["labelStatusDoor_" + loopCount.ToString()].BackColor = Color.Green;
                            this.groupBoxCtrlStatus.Controls["labelStatusDoor_" + loopCount.ToString()].Text = "Close";
                        }
                    }
                    #endregion
                }
                else
                {
                    eventDelegateRtuStatus dd = new eventDelegateRtuStatus(doorEvent);
                    object[] t = new object[] { eventStatus };
                    this.BeginInvoke(dd, t);
                }

                */
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
            }

        }
        private void IPEvent(byte[] data_byte)
        {
            string functionName = "IPEvent";
            try
            {
                for (int loopCount = 0; loopCount < 4; loopCount++)
                {
                    this.groupBoxIPADDRESS.Controls["tb_IPADDRESS" + loopCount.ToString()].Text = data_byte[6 + loopCount].ToString();
                }
                for (int loopCount = 0; loopCount < 4; loopCount++)
                {
                    this.groupBoxIPADDRESS.Controls["tb_GATEWAY" + loopCount.ToString()].Text = data_byte[11 + loopCount].ToString();
                }
                for (int loopCount = 0; loopCount < 4; loopCount++)
                {
                    this.groupBoxIPADDRESS.Controls["tb_SERVER_IP" + loopCount.ToString()].Text = data_byte[16 + loopCount].ToString();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
            }

        }
        private void acEvent(RtuStatus eventRtustatus)
        {
            for (int i = 0; i < eventRtustatus.AC.Length; i++)
            {
                if (eventRtustatus.AC[i])
                {
                    this.acPower_groupBox.Controls["button_ac" + i.ToString()].BackColor = Color.Green;
                }
                else
                    this.acPower_groupBox.Controls["button_ac" + i.ToString()].BackColor = Color.Red;
            }

        }
        private void ac_StatEvent(int[] acSateValue)
        {
            string temp = "";
            for (int i = 1; i <= acSateValue.Length; i++)
            {
                if (i % 2 == 0)
                {
                    this.acPower_groupBox.Controls["textBoxAc" + (i / 2 - 1).ToString()].Text = temp + "." + acSateValue[i - 1].ToString();
                }
                else
                {
                    temp = acSateValue[i - 1].ToString();
                }
            }
        }
        private void dcEvent(RtuStatus eventRtustatus)
        {
            for (int i = 0; i < eventRtustatus.DC.Length; i++)
            {
                if (eventRtustatus.DC[i])
                {
                    //this.dcpower_groupBox.Controls["button_dc" + i.ToString()].Text = "ON";
                    this.dcpower_groupBox.Controls["button_dc" + i.ToString()].BackColor = Color.Green;

                }
                else
                {
                    this.dcpower_groupBox.Controls["button_dc" + i.ToString()].BackColor = Color.Red;
                  //  this.dcpower_groupBox.Controls["button_dc" + i.ToString()].Text = "OFF";
                }
            }

        }
        private void dc_StatEvent(int[] dcSateValue) //textBoxDc0
        {
            string temp = "";
            for (int i = 1; i <= dcSateValue.Length; i++)
            {
                if (i % 2 == 0)
                {
                    this.dcpower_groupBox.Controls["textBoxDc" + (i/2-1).ToString()].Text = temp + "." + dcSateValue[i - 1].ToString();
                }
                else
                {
                    temp = dcSateValue[i-1].ToString();
                }
            }
        }
        private void temperatureEvent(RtuStatus eventStatus)
        {
            string functionName = "temperatureEvent";
            StringBuilder sb = new StringBuilder();
            try
            {
                if (this.InvokeRequired == false)
                {
                    #region Delegate

                    if (eventStatus.TEMP[0] == 0x00)
                    {
                        sb.Append("+");
                    }
                    else
                    {
                        sb.Append("-");
                    }
                    sb.Append(eventStatus.TEMP[1].ToString() + " . ");
                    sb.Append(eventStatus.TEMP[2].ToString());
                    textBoxTemperature.Text = sb.ToString() + " ℃";

                    #endregion
                }
                else
                {
                    eventDelegateRtuStatus dd = new eventDelegateRtuStatus(temperatureEvent);
                    object[] t = new object[] { eventStatus };
                    this.BeginInvoke(dd, t);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
            }
        }
        private void humidityEvent(RtuStatus eventStatus)
        {
            string functionName = "humidityEvent";
            try
            {
                if (this.InvokeRequired == false)
                {
                    #region Delegate
                    this.textBoxHumi.Text = eventStatus.HUMI.ToString() + " %";
                    #endregion
                }
                else
                {
                    eventDelegateRtuStatus dd = new eventDelegateRtuStatus(humidityEvent);
                    object[] t = new object[] { eventStatus };
                    this.BeginInvoke(dd, t);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
            }
        }
        private void cdsEvent(RtuStatus eventStatus)
        {
            string functionName = "CDS";
            try
            {
                this.textBoxCds.Text = eventStatus.Cdsvalue.ToString();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
            }
        }
        private void keyPressEvent(EnumKeyPress keyNum)
        {
            string functionName = "keyPressEvent";
            try
            {
                if (this.InvokeRequired == false)
                {
                    #region Delegate
                    switch (keyNum)
                    {
                        case EnumKeyPress.MENU:
                            this.labelStatusKeyPressMenu.BackColor = Color.Green;
                            break;
                        case EnumKeyPress.LEFT:
                            this.labelStatusKeyPressLeft.BackColor = Color.Green;
                            break;
                        case EnumKeyPress.UP:
                            this.labelStatusKeyPressUp.BackColor = Color.Green;
                            break;
                        case EnumKeyPress.DOWN:
                            this.labelStatusKeyPressDown.BackColor = Color.Green;
                            break;
                        case EnumKeyPress.RIGHT:
                            this.labelStatusKeyPressRight.BackColor = Color.Green;
                            break;
                        case EnumKeyPress.CONFIRM:
                            this.labelStatusKeyPressConfirm.BackColor = Color.Green;
                            break;
                        case EnumKeyPress.SOUND:
                            this.labelStatusKeyPressSound.BackColor = Color.Green;
                            break;
                    }

                    if (!timerKeyPress.Enabled)
                    {
                        timerKeyPress.Start();
                    }
                    #endregion
                }
                else
                {
                    eventDelegateEnumKeyPress dd = new eventDelegateEnumKeyPress(keyPressEvent);
                    object[] t = new object[] { keyNum };
                    this.BeginInvoke(dd, t);
                }


            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
            }


        }
        private void fan1Event(RtuStatus eventStatus)
        {
            string functionName = "modeFan1Event";
            try
            {
                if (this.InvokeRequired == false)
                {
                    #region Delegate

                    #endregion
                }
                else
                {
                    eventDelegateRtuStatus dd = new eventDelegateRtuStatus(fan1Event);
                    object[] t = new object[] { eventStatus };
                    this.BeginInvoke(dd, t);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
            }
        }
        private void fan2Event(RtuStatus eventStatus)
        {
            string functionName = "modeFan1Event";
            try
            {
                if (this.InvokeRequired == false)
                {
                    #region Delegate

                    #endregion
                }
                else
                {
                    eventDelegateRtuStatus dd = new eventDelegateRtuStatus(fan2Event);
                    object[] t = new object[] { eventStatus };
                    this.BeginInvoke(dd, t);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
            }
        }
        private void soundVolume(RtuStatus eventStatus)
        {
            string functionName = "soundVolume";
            try
            {
                if (this.InvokeRequired == false)
                {
                    #region Delegate
                    // this.textBoxSoundVolume.Text = ((int)eventStatus.SoundVolume).ToString();
                    #endregion
                }
                else
                {
                    eventDelegateRtuStatus dd = new eventDelegateRtuStatus(soundVolume);
                    object[] t = new object[] { eventStatus };
                    this.BeginInvoke(dd, t);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
            }
        }
        private void impactEvent()
        {
            string functionName = "impactEvent";
            try
            {

                this.impact_label.ImageIndex = 1;


            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
            }
        }
        #endregion

        #region 상태 조회

        private void buttonCtrlConnect_Click(object sender, EventArgs e)
        {
            bool rtu_init = rtu.InitSerial();
            bool rtu_open = rtu.PortOpen();
       //     if (rtu_init && rtu_open)
        //    {
       //         buttonCtrlConnect.Enabled = false;
       //         buttonCtrlDisConnect.Enabled = true;
       //         rtuInit();
       //     }
       //     else
       //     {
//
       //         buttonCtrlConnect.Enabled = true;
       //         buttonCtrlDisConnect.Enabled = false;
       //     }
        }
        private void buttonCtrlDisConnect_Click(object sender, EventArgs e)
        {
            bool rtu_close = rtu.PortClose();
            if (rtu_close)
            {
                buttonCtrlDisConnect.Enabled = false;
                buttonCtrlConnect.Enabled = true;
            }
            else
            {
                buttonCtrlDisConnect.Enabled = true;
                buttonCtrlConnect.Enabled = false;
            }

        }
        private void buttonInquiryDoor_Click(object sender, EventArgs e)
        {
            byte[] op_cmd = new byte[2] { 0x01, 0x01 };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
        }
        private void ac_button_Click(object sender, EventArgs e)
        {

            byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_AC_PWR };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
        }
        private void dc_button_Click(object sender, EventArgs e)
        {
            byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_DC_PWR };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
        }
        private void buttonInquiryTemperature_Click(object sender, EventArgs e)
        {
            byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_TEMP };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
        }
        private void cds_button_Click(object sender, EventArgs e)
        {
            byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_CDS };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
        }
        private void buttonInquiryHumidity_Click(object sender, EventArgs e)
        {
            byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_HUMI };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);

        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(richTextBoxSendRecvView.MaxLength.ToString() + richTextBoxSendRecvView.Text.Length.ToString());
            //20110902
            int[] itemp = new int[36] { 33, 11, 14, 25, 33, 11, 4, 5, 33, 11, 4, 5, 33, 11, 4, 5, 33, 11, 4, 5, 33, 11, 4, 5, 33, 11, 4, 5, 33, 11, 4, 5, 33, 11, 4, 5 };
            dc_StatEvent(itemp);

        }
        private void timerImpactCheck(object sender, EventArgs e)
        {
            if (this.impact_label.ImageIndex == 1)
            {
                Thread.Sleep(1000);
                this.impact_label.ImageIndex = 0;
            }

        }
        private void timerKeyPress_Tick(object sender, EventArgs e)
        {
            if (keyPress)
            {
                keyPress = false;
                this.labelStatusKeyPressMenu.BackColor = SystemColors.Control;
                this.labelStatusKeyPressLeft.BackColor = SystemColors.Control;
                this.labelStatusKeyPressUp.BackColor = SystemColors.Control;
                this.labelStatusKeyPressDown.BackColor = SystemColors.Control;
                this.labelStatusKeyPressRight.BackColor = SystemColors.Control;
                this.labelStatusKeyPressConfirm.BackColor = SystemColors.Control;
                this.labelStatusKeyPressSound.BackColor = SystemColors.Control;
                timerKeyPress.Stop();
            }
            else
            {
                keyPress = true;
            }
        }

        #region Make 데이터 and Send
        private void setSendDataRtu(byte[] op_cmd, int data_int, int whereValue, byte[] setbyte)
        {
            string functionName = "setSendDataRtu";

            int count = 0;
            int setcount = 0;
            byte[] senddata = new byte[serial_DataFild + op_cmd.Length + data_int + 1]; //4 + 2 + 6 + 1

            int[] temp = new int[setbyte.Length]; //3
            for (int j = 0; j < temp.Length; j++)
            {
                temp[j] = 6 + whereValue++;
                //temp[0] = 6+1
                //temp[1] = 6+2
                //temp[2] = 6+3
            }

            for (int i = 0; i < senddata.Length; i++)
            {


                // 0x02, 0xFD, 0x21, 0x11
                if (i >= 0 && i < 4)
                {
                    senddata[i] = serial_header[i];
                }
                else if (i > 3 && i < 6)
                {
                    //OPCODE , CMD 
                    senddata[i] = op_cmd[count++];
                }
                else if (i == 6)
                {
                    //데이터 길이 
                    if (data_int == 0)
                    {
                        return;
                    }
                    else
                    {
                        senddata[i] = Convert.ToByte(data_int.ToString());
                    }
                }
                else if (i > 6)
                {
                    try
                    {
                        if (setcount < setbyte.Length && temp[setcount] == i)
                        {
                            //temp[0] = 6+1
                            //temp[1] = 6+2
                            //temp[2] = 6+3
                            //   temp++;
                            senddata[i] = setbyte[setcount];
                            setcount++;
                        }
                        else
                        {
                            senddata[i] = 0x00;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.WriteLog(LogLevel.TRACE, programName, functionName, " 배열 초과" + e.ToString());
                        senddata[i] = 0x00;
                    }
                }
            }

            makeCheckSum(senddata);

        }
        public void makeCheckSum(byte[] data)
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

            makeByteSum(data, CheckSum);
        }
        public void makeByteSum(byte[] serial_Header, byte[] serial_Tail)
        {
            byte[] full_data = new byte[serial_Header.Length + serial_Tail.Length];

            Buffer.BlockCopy(serial_Header, 0, full_data, 0, serial_Header.Length);
            Buffer.BlockCopy(serial_Tail, 0, full_data, serial_Header.Length, serial_Tail.Length);

            sendDataToRtu(full_data);

            this.rtu.sendToRtu(full_data, full_data.Length);
        }
        #endregion

        #region AC 전원 제어
        private void button_ac0_Click(object sender, EventArgs e)
        {
            string functionName = "AC0 Set";
            try
            {
                int setdata = 1;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_AC_PWR };
                // byte[] setData = new byte[ac_count]{0x01
                if (this.button_ac0.BackColor == Color.Green)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, ac_count, setdata, setbyte);
                    this.button_ac0.BackColor = Color.Red;
                    //this.button_ac0.Text = "AC1 OFF";

                }
                else if (this.button_ac0.BackColor == Color.Red)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, ac_count, setdata, setbyte);
                    this.button_ac0.BackColor = Color.Green;
                    //this.button_ac0.Text = "AC1 ON";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }

        }
        private void button_ac1_Click(object sender, EventArgs e)
        {
            string functionName = "AC1 Set";
            try
            {
                int setdata = 2;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_AC_PWR };
                // byte[] setData = new byte[ac_count]{0x01
                if (this.button_ac1.BackColor == Color.Green)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, ac_count, setdata, setbyte);
                    button_ac1.BackColor = Color.Red;
                    //button_ac1.Text = "AC2 OFF";

                }
                else if (this.button_ac1.BackColor == Color.Red)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, ac_count, setdata, setbyte);
                    this.button_ac1.BackColor = Color.Green;
                   // this.button_ac1.Text = "AC2 ON";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }
        private void button_ac2_Click(object sender, EventArgs e)
        {
            string functionName = "AC2 Set";
            try
            {
                int setdata = 3;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_AC_PWR };
                // byte[] setData = new byte[ac_count]{0x01
                if (this.button_ac2.BackColor == Color.Green)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, ac_count, setdata, setbyte);
                    this.button_ac2.BackColor = Color.Red;
                    //this.button_ac2.Text = "AC3 OFF";

                }
                else if (this.button_ac2.BackColor == Color.Red)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, ac_count, setdata, setbyte);
                    this.button_ac2.BackColor = Color.Green;
                    //this.button_ac2.Text = "AC3 ON";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }
        private void button_ac3_Click(object sender, EventArgs e)
        {
            string functionName = "AC3 Set";
            try
            {
                int setdata = 4;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_AC_PWR };
                // byte[] setData = new byte[ac_count]{0x01
                if (this.button_ac3.BackColor == Color.Green)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, ac_count, setdata, setbyte);
                    this.button_ac3.BackColor = Color.Red;
                  //  this.button_ac3.Text = "AC4 OFF";

                }
                else if (this.button_ac3.BackColor == Color.Red)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, ac_count, setdata, setbyte);
                    this.button_ac3.BackColor = Color.Green;
                  //  this.button_ac3.Text = "AC4 ON";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }
        private void button_ac4_Click(object sender, EventArgs e)
        {
            string functionName = "AC4 Set";
            try
            {
                int setdata = 5;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_AC_PWR };
                // byte[] setData = new byte[ac_count]{0x01
                if (this.button_ac4.BackColor == Color.Green && heater_mode == 0x02)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, ac_count, setdata, setbyte);
                    this.button_ac4.BackColor = Color.Red;
                  //  this.button_ac4.Text = "AC5 OFF";

                }
                else if (this.button_ac4.BackColor == Color.Red && heater_mode == 0x02)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, ac_count, setdata, setbyte);
                    this.button_ac4.BackColor = Color.Green;
                 //   this.button_ac4.Text = "AC5 ON";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }
        private void button_ac5_Click(object sender, EventArgs e)
        {
            string functionName = "AC5 Set ";
            try
            {
                int setdata = 6;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_AC_PWR };
                // byte[] setData = new byte[ac_count]{0x01
                if (this.button_ac5.BackColor == Color.Green && fan_mode == 0x02)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, ac_count, setdata, setbyte);
                    this.button_ac5.BackColor = Color.Red;
               //     this.button_ac5.Text = "AC6 OFF";

                }
                else if (this.button_ac5.BackColor == Color.Red && fan_mode == 0x02)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, ac_count, setdata, setbyte);
                    this.button_ac5.BackColor = Color.Green;
               //     this.button_ac5.Text = "AC6 ON";
                }

            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }
        #endregion

        #region DC 전원 제어
        private void button_dc0_Click(object sender, EventArgs e)
        {
            string functionName = "DC5v01 Set";
            try
            {
                int setdata = 1;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_DC_PWR };
                // byte[] setData = new byte[ac_count]{0x01
                if (this.button_dc0.BackColor == Color.Green)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc0.BackColor = Color.Red;
                   // this.button_dc0.Text = "OFF";

                }
                else if (this.button_dc0.BackColor == Color.Red)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc0.BackColor = Color.Green;
                  //  this.button_dc0.Text = "ON";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }
        private void button_dc1_Click(object sender, EventArgs e)
        {
            string functionName = "DC5v02 Set";
            try
            {
                int setdata = 2;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_DC_PWR };
                if (this.button_dc1.BackColor == Color.Green)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc1.BackColor = Color.Red;
                 //   this.button_dc1.Text = "OFF";


                }
                else if (this.button_dc1.BackColor == Color.Red)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc1.BackColor = Color.Green;
                //    this.button_dc1.Text = "ON";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }
        private void button_dc2_Click(object sender, EventArgs e)
        {
            string functionName = "DC12v01 Set";
            try
            {
                int setdata = 3;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_DC_PWR };
                // byte[] setData = new byte[ac_count]{0x01
                if (this.button_dc2.BackColor == Color.Green)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc2.BackColor = Color.Red;
               //     this.button_dc2.Text = "OFF";

                }
                else if (this.button_dc2.BackColor == Color.Red)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc2.BackColor = Color.Green;
                //    this.button_dc2.Text = "ON";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }
        private void button_dc3_Click(object sender, EventArgs e)
        {
            string functionName = "DC12v02 Set";
            try
            {
                int setdata = 4;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_DC_PWR };
                // byte[] setData = new byte[ac_count]{0x01
                if (this.button_dc3.BackColor == Color.Green)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc3.BackColor = Color.Red;
                  //  this.button_dc3.Text = "OFF";

                }
                else if (this.button_dc3.BackColor == Color.Red)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc3.BackColor = Color.Green;
                 //   this.button_dc3.Text = "ON";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }
        private void button_dc4_Click(object sender, EventArgs e)
        {
            string functionName = "DC12v03 Set";
            try
            {
                int setdata = 5;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_DC_PWR };
                // byte[] setData = new byte[ac_count]{0x01
                if (this.button_dc4.BackColor == Color.Green)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc4.BackColor = Color.Red;
                 //   this.button_dc4.Text = "OFF";

                }
                else if (this.button_dc4.BackColor == Color.Red)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc4.BackColor = Color.Green;
               //     this.button_dc4.Text = "ON";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }
        private void button_dc5_Click(object sender, EventArgs e)
        {
            string functionName = "DC12v04 Set";
            try
            {
                int setdata = 6;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_DC_PWR };
                // byte[] setData = new byte[ac_count]{0x01
                if (this.button_dc5.BackColor == Color.Green)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc5.BackColor = Color.Red;
                  //  this.button_dc5.Text = "OFF";

                }
                else if (this.button_dc5.BackColor == Color.Red)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc5.BackColor = Color.Green;
                 //   this.button_dc5.Text = "ON";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }
        private void button_dc6_Click(object sender, EventArgs e)
        {
            string functionName = "DC24v01 Set";
            try
            {
                int setdata = 7;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_DC_PWR };
                // byte[] setData = new byte[ac_count]{0x01
                if (this.button_dc6.BackColor == Color.Green)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc6.BackColor = Color.Red;
                   // this.button_dc6.Text = "OFF";

                }
                else if (this.button_dc6.BackColor == Color.Red)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc6.BackColor = Color.Green;
                //    this.button_dc6.Text = "ON";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }
        private void button_dc7_Click(object sender, EventArgs e)
        {
            string functionName = "DC24v02 Set";
            try
            {
                int setdata = 8;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_DC_PWR };
                // byte[] setData = new byte[ac_count]{0x01
                if (this.button_dc7.BackColor == Color.Green)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc7.BackColor = Color.Red;
                //    this.button_dc7.Text = "OFF";

                }
                else if (this.button_dc7.BackColor == Color.Red)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc7.BackColor = Color.Green;
                //    this.button_dc7.Text = "ON";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }
        private void button_dc8_Click(object sender, EventArgs e)
        {
            string functionName = "DC24v03 Set";
            try
            {
                int setdata = 9;
                byte[] data = new byte[] { OPCODE_SET_REQUEST_03, CMD_DC_PWR };
                // byte[] setData = new byte[ac_count]{0x01
                if (this.button_dc8.BackColor == Color.Green)
                {
                    byte[] setbyte = new byte[] { 0x02 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc8.BackColor = Color.Red;
                  //  this.button_dc8.Text = "OFF";

                }
                else if (this.button_dc8.BackColor == Color.Red)
                {
                    byte[] setbyte = new byte[] { 0x01 };
                    setSendDataRtu(data, dc_count, setdata, setbyte);
                    this.button_dc8.BackColor = Color.Green;
                 //   this.button_dc8.Text = "ON";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }

        #endregion



        private void buttonInquiryFan1_Click(object sender, EventArgs e)
        {
            byte[] op_cmd = new byte[2] { OPCODE_GET_SETVAL_07, CMD_FAN };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);

        }
        private void buttonInquiryHeater_Click(object sender, EventArgs e)
        {
            byte[] op_cmd = new byte[2] { OPCODE_GET_SETVAL_07, CMD_HEATER };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);

        }
        private void buttonCtrlModeHeater_Click(object sender, EventArgs e)
        {
            if (this.buttonCtrlModeHeater.BackColor == Color.Yellow)
            {
                this.buttonCtrlModeHeater.FlatStyle = FlatStyle.Popup;
                //this.buttonCtrlModeHeater.Text = "자동";
                this.buttonCtrlModeHeater.BackColor = Color.Pink;
            }
            else if (this.buttonCtrlModeHeater.BackColor == Color.Pink)
            {
                this.buttonCtrlModeHeater.FlatStyle = FlatStyle.Flat;
                //this.buttonCtrlModeHeater.Text = "수동";
                this.buttonCtrlModeHeater.BackColor = Color.Yellow;
            }
        }
        private void buttonCtrlModeFan1_Click(object sender, EventArgs e)
        {
            if (this.buttonCtrlModeFan1.BackColor == Color.Yellow)
            {
                this.buttonCtrlModeFan1.FlatStyle = FlatStyle.Popup;
                // this.buttonCtrlModeFan1.Text = "자동";
                this.buttonCtrlModeFan1.BackColor = Color.Pink;
            }
            else if (this.buttonCtrlModeFan1.BackColor == Color.Pink)
            {
                this.buttonCtrlModeFan1.FlatStyle = FlatStyle.Flat;
                // this.buttonCtrlModeFan1.Text = "수동";
                this.buttonCtrlModeFan1.BackColor = Color.Yellow;
            }
        }
        private void buttonCtrlAutoFan1_Click(object sender, EventArgs e)
        {
            byte[] setTest = new byte[3];
            string functionName = "FAN Set ";
            try
            {
                if (textBoxFan1Mark.Text.Contains("+"))
                {
                    setTest[0] = 0x00;
                }
                else if (textBoxFan1Mark.Text.Contains("-"))
                {
                    setTest[0] = 0x01;
                }
                else
                {
                    setTest[0] = 0x00;
                }

                setTest[1] = (byte)Convert.ToInt32(textBoxFan1Temp.Text);
                if (buttonCtrlModeFan1.BackColor == Color.Yellow)
                {
                    setTest[2] = 0x02;
                }
                else
                {
                    setTest[2] = 0x01;
                }


                int setdata = 1;
                byte[] data = new byte[] { OPCODE_SET_SETVAL_09, CMD_FAN };
                setSendDataRtu(data, setTest.Length, setdata, setTest);
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }

        private void trackBarCtrlSoundVlume_Scroll(object sender, EventArgs e)
        {

        }


        private void button3_Click(object sender, EventArgs e)
        {
            byte[] dataSend = (byte[])ConvertUtil.convert("HS2B", textBox1.Text, textBox1.Text.Length);
            this.rtu.sendToRtu(dataSend, dataSend.Length);
        }

        private void buttonCtrHeaterSet_Click(object sender, EventArgs e)
        {
            byte[] setTest = new byte[4];
            string functionName = "HEATER Set ";
            try
            {
                if (textBoxCtrlHeaterMark.Text.Contains("+"))
                {
                    setTest[0] = 0x00;
                }
                else if (textBoxCtrlHeaterMark.Text.Contains("-"))
                {
                    setTest[0] = 0x01;
                }
                else
                {
                    setTest[0] = 0x00;
                }

                setTest[1] = (byte)Convert.ToInt32(textBoxCtrlHeaterTemp.Text);
                setTest[2] = (byte)Convert.ToInt32(textBoxCtrlHeaterHumi.Text);
                if (this.buttonCtrlModeHeater.BackColor == Color.Yellow)
                {
                    setTest[3] = 0x02;
                }
                else
                {
                    setTest[3] = 0x01;
                }


                int setdata = 1;
                byte[] data = new byte[] { OPCODE_SET_SETVAL_09, CMD_HEATER };
                setSendDataRtu(data, setTest.Length, setdata, setTest);
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {

            if (this.checkBoxUpdate.Checked == true)
            {

                  //Thread.Sleep(200);
                #region 온도 , 습도 , 조도센스 ,팬 , 히터,AC,DC

                //AC
                byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_AC_PWR };
                this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
             
                //온도
                op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_TEMP };
                this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
                //습도
                op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_HUMI };
                this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);

                //조도센스
                op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_CDS };
                this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);

                //팬
                op_cmd = new byte[2] { OPCODE_GET_SETVAL_07, CMD_FAN };
                this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);

                //히터
                op_cmd = new byte[2] { OPCODE_GET_SETVAL_07, CMD_HEATER };
                this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);

                //DC
                op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_DC_PWR };
                this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
                
                op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_AC_STAT };
                this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);

                op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_DC_STAT };
                this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
        
                rtuStatusUpdate = rtu.getRtuStatus();

                if (rtuStatusUpdate.TEMP[1] >= rtuStatusUpdate.FAN_STAT[1] && rtuStatusUpdate.FAN_STAT[2] == 0x01)
                {
                    fan_mode = rtuStatusUpdate.FAN_STAT[2];
                    this.button_ac5.BackColor = Color.Green;
                    this.button_ac5.Enabled = false;
                }
                else if (rtuStatusUpdate.TEMP[1] <= rtuStatusUpdate.FAN_STAT[1] && rtuStatusUpdate.FAN_STAT[2] == 0x01)
                {
                    fan_mode = rtuStatusUpdate.FAN_STAT[2];
                    this.button_ac5.BackColor = Color.Red;
                    this.button_ac5.Enabled = false;
                }


                if (rtuStatusUpdate.TEMP[1] <= rtuStatusUpdate.HEATER_STAT[1] || rtuStatusUpdate.HEATER_STAT[2] <=rtuStatusUpdate.HUMI  && rtuStatusUpdate.HEATER_STAT[3] == 0x01)
                {
                    heater_mode = rtuStatusUpdate.HEATER_STAT[3];
                    this.button_ac4.BackColor = Color.Green;
                    this.button_ac4.Enabled = false;
                }
                else if (rtuStatusUpdate.TEMP[1] >= rtuStatusUpdate.HEATER_STAT[1] || rtuStatusUpdate.HEATER_STAT[2] >= rtuStatusUpdate.HUMI && rtuStatusUpdate.HEATER_STAT[3] == 0x01)
                {
                    heater_mode = rtuStatusUpdate.HEATER_STAT[3];
                    this.button_ac4.BackColor = Color.Red;
                    this.button_ac4.Enabled = false;
                }

                #endregion
            }
        }

        private void timerTextCheck_Tick(object sender, EventArgs e)
        {
            if (richTextBoxSendRecvView.MaxLength-2000 < richTextBoxSendRecvView.Text.Length)
            {
                richTextBoxSendRecvView.Clear();
            }
        }

        private void buttonIpAddress_Click(object sender, EventArgs e)
        {
            byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_IPADDRESS };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
       
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string functionName = "IPSENDDATA";
            try
            {
                byte[] setTest = new byte[15];

                setTest[0] = 0x01;
                //  setTest[1] = (byte)Convert.ToInt32(textBoxCtrlHeaterTemp.Text);
                for (int loopCount = 0; loopCount < 4; loopCount++)
                {
                    setTest[1 + loopCount] = (byte)Convert.ToInt32(groupBoxsetIPADDRESS.Controls["tb_setIPADDRESS" + loopCount.ToString()].Text);
                }
                setTest[5] = 0x02;
                for (int loopCount = 0; loopCount < 4; loopCount++)
                {
                    setTest[6 + loopCount] = (byte)Convert.ToInt32(groupBoxsetIPADDRESS.Controls["tb_setGATEWAY" + loopCount.ToString()].Text);
                }
                setTest[10] = 0x03;
                for (int loopCount = 0; loopCount < 4; loopCount++)
                {
                    setTest[11 + loopCount] = (byte)Convert.ToInt32(groupBoxsetIPADDRESS.Controls["tb_setSERVER_IP" + loopCount.ToString()].Text);
                }
                int setdata = 1;
                byte[] data = new byte[] { OPCODE_SET_SETVAL_09, CMD_IPADDRESS };
                setSendDataRtu(data, setTest.Length, setdata, setTest);
            }
            catch(Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, ex.ToString());
            }
        }
        private void int_KeyPress(object sender, KeyPressEventArgs e)
        {
            //숫자,백스페이스,마이너스,소숫점 만 입력받는다.
            //if (!(Char.IsDigit(e.KeyChar)) && e.KeyChar != 8 && e.KeyChar != 45 && e.KeyChar != 46) //8:백스페이스,45:마이너스,46:소수점
                if (!(Char.IsDigit(e.KeyChar)) && e.KeyChar != 8)
                {
                e.Handled = true;
            }

        }

        private void tb_setIPADDRESS_TextChanged(object sender, EventArgs e)
        {
            tb_setGATEWAY0.Text = tb_setIPADDRESS0.Text;
            tb_setGATEWAY1.Text = tb_setIPADDRESS1.Text;
            tb_setGATEWAY2.Text = tb_setIPADDRESS2.Text;
        }

        private void button5_Click(object sender, EventArgs e)
        {
          byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_AC_STAT };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
              
        }

        private void button6_Click(object sender, EventArgs e)
        {

            byte[] op_cmd = new byte[2] { OPCODE_GET_REQUEST_01, CMD_DC_STAT };
            this.rtu.sendToRtu(this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))), this.rtu.makeByteSum(this.rtu.makeSendData(op_cmd), this.rtu.makeCheckSum(this.rtu.makeSendData(op_cmd))).Length);
       
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0: tb_setSERVER_IP0.Text = "10"; tb_setSERVER_IP1.Text = "2"; tb_setSERVER_IP2.Text = "2"; tb_setSERVER_IP3.Text = "140"; break;
                case 1: tb_setSERVER_IP0.Text = "210"; tb_setSERVER_IP1.Text = "99"; tb_setSERVER_IP2.Text = "67"; tb_setSERVER_IP3.Text = "112"; break;
                case 2: tb_setSERVER_IP0.Text = ""; tb_setSERVER_IP1.Text = ""; tb_setSERVER_IP2.Text = ""; tb_setSERVER_IP3.Text = ""; break;
                default: break;

            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                string bstr = "02FD211109360101F03F";
                byte[] data = (byte[])ConvertUtil.convert("HS2B", bstr, bstr.Length);
                this.rtu.sendToRtu(data, data.Length);
            }
            else if (radioButton2.Checked) 
            {
                string bstr = "02FD211109360102F33F";
                byte[] data = (byte[])ConvertUtil.convert("HS2B", bstr, bstr.Length);
                this.rtu.sendToRtu(data, data.Length);
            }
        }
    }
}