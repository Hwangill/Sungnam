using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DllRtu
{
    public enum EnumControlName
    {
        NA,
        AC,
        DC
    }
    public enum EnumDeviceName
    {
        Fan1,
        Fan2,
        Fan3,
        Fan4,
        Heater,
        NA,
        Lcd,
        Led,
        Light,
        VPN,
        GateWay,
        Modem,
        HeaterFan,
        Dmb
    }

    public enum EnumKeyPress
    {
        NO = 0,
        MENU = 1,
        LEFT = 2,
        UP = 3,
        DOWN = 4,
        RIGHT = 5,
        CONFIRM = 6,
        SOUND = 7,
    }


    public class Cmm1_RtuMath
    {
        EnumDeviceName deviceName_;
        EnumControlName controlName_;
        int controlNo_;

        public Cmm1_RtuMath()
        {
            deviceName_ = EnumDeviceName.NA;
            controlName_ = EnumControlName.NA;
            controlNo_ = 0;
        }

        public Cmm1_RtuMath(string deviceName, EnumControlName controlName, int controlNo)
        {
            this.deviceName_ = getEnumDeviceName(deviceName);
            this.controlName_ = controlName;
            this.controlNo_ = controlNo;
        }

        public EnumDeviceName getEnumDeviceName(string deviceName)
        {
            for (int i = 0; i < 12; i++)
            {
                if (((EnumDeviceName)i).ToString().ToUpper().Equals(deviceName.ToUpper()))
                {
                    return (EnumDeviceName)i;
                }
            }

            return (EnumDeviceName)0;
        }

        public Cmm1_RtuMath(EnumDeviceName deviceName, EnumControlName controlName, int controlNo)
        {
            this.deviceName_ = deviceName;
            this.controlName_ = controlName;
            this.controlNo_ = controlNo;
        }

        public EnumDeviceName DeviceName
        {
            get
            {
                return deviceName_;
            }
            set
            {
                deviceName_ = value;
            }
        }

        public EnumControlName ControlName
        {
            get
            {
                return controlName_;
            }
            set
            {
                controlName_ = value;
            }
        }

        public int ControlNo
        {
            get
            {
                return controlNo_;
            }
            set
            {
                controlNo_ = value;
            }
        }

    }
    public class RtuParamater
    {
        private int HeaterActTemperature_;
        private int FanAct1Temperature_;
        private int FanAct2Temperature_;
        private bool manualFanControl_;
        private bool manualHeaterControl_;


        public RtuParamater()
        {
            HeaterActTemperature_ = 5;
            FanAct1Temperature_ = 35;
            FanAct2Temperature_ = 45;
            manualFanControl_ = false;
            manualHeaterControl_ = false;


        }
        public int HeaterActTemperature
        {
            get
            {
                return HeaterActTemperature_;
            }
            set
            {
                HeaterActTemperature_ = value;
            }
        }
        public int FanAct1Temperature
        {
            get
            {
                return FanAct1Temperature_;
            }
            set
            {
                FanAct1Temperature_ = value;
            }
        }
        public int FanAct2Temperature
        {
            get
            {
                return FanAct2Temperature_;
            }
            set
            {
                FanAct2Temperature_ = value;
            }
        }
        public bool manualFanControl
        {
            get
            {
                return manualFanControl_;
            }
            set
            {
                manualFanControl_ = value;
            }
        }
        public bool manualHeaterControl
        {
            get
            {
                return manualHeaterControl_;
            }
            set
            {
                manualHeaterControl_ = value;
            }
        }

    }
    public class RtuDevice
    {

        public bool DeviceMode;
        public bool DeviceStatus;
        public int DeviceOn;
        public int DeviceOff;
        public RtuDevice()
        {
            DeviceMode = false;
            DeviceStatus = false;
            DeviceOn = 0;
            DeviceOff = 0;
        }

    }


    public class RtuStatus
    {

        //Cmm1 변수
        public const int AC_Count_ = 6;
        public const int DC_Count_ = 4;
        public const int Door_Count_ = 2;

        public bool[] AC_ = new bool[AC_Count_];
        public bool[] DC_ = new bool[DC_Count_];
        private bool[] Door_ = new bool[Door_Count_];
        private bool SoundMute_;
        private byte SoundAmp_;


        //만든다 
        public string STemperature;
        public string SHumidity;

        private const int Device_Count_ = 3;

        //cmm3
        private bool[] DoorStatus_ = new bool[Door_Count_];
        private RtuDevice[] Device_ = new RtuDevice[Device_Count_];

        private float Temperature_;
        private float Humidity_;
      //  private byte SoundVolume_;
        public byte Cdsvalue;


        private const int AC_STAT_Count = 24;
        private const int DC_STAT_Count = 36;
        public const int AC_Count = 6;
        public const int DC_Count = 9;
        public const int FAN_Count = 4;
        public const int TEMP_Count = 3;

        public bool[] AC = new bool[AC_Count];
        public bool[] DC = new bool[DC_Count];
        public int[] AC_STAT_Value = new int[AC_STAT_Count];
        public int[] DC_STAT_Value = new int[DC_STAT_Count];
        public bool[] DOOR = new bool[Door_Count_];
        public byte[] FAN_STAT = new byte[3];
        public byte[] HEATER_STAT = new byte[4];
        public int FAN_MODE;
        public bool[] FAN = new bool[FAN_Count];
        public byte[] TEMP = new byte[TEMP_Count];
        public int HUMI;
        public byte Sound_VOL;
        public int Sound_MUTE;
        public int HEATER_MODE;

        public byte LCD_RGB_Status;
        public byte LCD_DVI_Status;
        public byte LCD_Double;

        public bool[] lcd_status = new bool[2];


        public string DataView()
        {

            return "\n AC_Count[" + AC_Count + "]" +
       "\n DC_Count[" + DC_Count + "]" +
       "\n Door_Count[" + Door_Count_ + "]" +
       "\n AC_(0)[" + AC_[0].ToString() + "]" +
       "\n AC_(1)[" + AC_[1].ToString() + "]" +
       "\n AC_(2)[" + AC_[2].ToString() + "]" +
       "\n AC_(3)[" + AC_[3].ToString() + "]" +
       "\n AC_(4)[" + AC_[4].ToString() + "]" +
       "\n AC_(5)[" + AC_[5].ToString() + "]" +
       "\n DC_(0)[" + DC_[0].ToString() + "]" +
       "\n DC_(1)[" + DC_[1].ToString() + "]" +
       "\n DC_(2)[" + DC_[2].ToString() + "]" +
       "\n DC_(3)[" + DC_[3].ToString() + "]" +
       "\n Door_(0)[" + Door_[0].ToString() + "]" +
       "\n Door_(1)[" + Door_[1].ToString() + "]" +
       "\n Temperature_[" + Temperature_.ToString() + "]" +
       "\n Humidity_[" + Humidity_.ToString() + "]" +
       "\n SoundMute_[" + SoundMute_.ToString() + "]" +
       "\n SoundAmp_[" + SoundAmp_.ToString() + "]";
        }

        public RtuStatus()
        {
            STemperature = "+30.30";
            SHumidity = "+00.00";

            SoundMute_ = false;
            SoundAmp_ = 0x15;

            //SoundVolume_ = 0x09;
            DoorStatus_[0] = false;
            DoorStatus_[1] = false;
            Device_[0] = new RtuDevice();
            Device_[1] = new RtuDevice();
            Device_[2] = new RtuDevice();

            FAN_STAT[0] = 0x00;
            FAN_STAT[1] = 0x23;
            FAN_STAT[2] = 0x01;

            HEATER_STAT[0] = 0x00;
            HEATER_STAT[1] = 0x0A;
            HEATER_STAT[2] = 0x50;
            HEATER_STAT[3] = 0x01;

            AC[0] = true;
            AC[1] = true;
            AC[2] = false;
            AC[3] = false;
            AC[4] = false;
            AC[5] = false;
            DC[0] = true; //5v_1
            DC[1] = true; //5v_2
            DC[2] = true; //12v_1
            DC[3] = true; //12v_2
            DC[4] = true; //12v_3
            DC[5] = true; //12v_4
            DC[6] = true; //24v_1
            DC[7] = true; //24v_2
            DC[8] = true; //24v_3
            DOOR[0] = false;
            DOOR[1] = false;
            FAN[0] = false;
            FAN[1] = false;
            FAN[2] = false;
            FAN[3] = false;
            FAN_MODE = 0x00;
            Sound_VOL = 0x06;
            Sound_MUTE = 0x00;
            HEATER_MODE = 0x00;

            HUMI = 50;

            Cdsvalue = 0x00;

            TEMP[0] = 0x00;
            TEMP[1] = 0x14;
            TEMP[2] = 0x00;

            LCD_RGB_Status = 0x31;
            LCD_DVI_Status = 0x31;
            LCD_Double = 0x33;

            for(int i=0; i<AC_STAT_Count; i++)
            {
                AC_STAT_Value[i] = 0;
            }
            for (int i = 0; i < DC_STAT_Count; i++)
            {
                DC_STAT_Value[i] = 0;
            }

            lcd_status[0] = false;
            lcd_status[1] = false;

        }

        //구조체 라인


        public bool setACDC(Cmm1_RtuMath rtuMath, int status)
        {
            if (status == 1)
            {
                return setACDC(rtuMath, true);
            }
            else
            {
                return setACDC(rtuMath, false);
            }
        }

        public bool setACDC(EnumControlName controlName, int deviceNum, int status)
        {
            if (status == 1)
            {
                return setACDC(controlName, deviceNum, true);
            }
            else
            {
                return setACDC(controlName, deviceNum, false);
            }

        }

        public bool setACDC(Cmm1_RtuMath rtuMath, bool status)
        {
            return setACDC(rtuMath.ControlName, rtuMath.ControlNo, status);
        }

        public bool setACDC(EnumControlName controlName, int deviceNum, bool status)
        {
            if (EnumControlName.AC == controlName)
            {
                return setAC(deviceNum, status);
            }
            else if (EnumControlName.DC == controlName)
            {
                return setDC(deviceNum, status);
            }
            else
            {
                return false;
            }

        }

        public bool setAC(int deviceNum, int status)
        {
            if (status == 1)
            {
                return setAC(deviceNum, true);
            }
            else
            {
                return setAC(deviceNum, false);
            }

        }

        public bool setAC(int deviceNum, bool status)
        {
            if (AC_.Length > deviceNum)
            {
                AC_[deviceNum] = status;
            }
            else
            {
                return false;
            }
            return true;
        }

        public bool setDC(int deviceNum, int status)
        {
            if (status == 1)
            {
                return setDC(deviceNum, true);
            }
            else
            {
                return setDC(deviceNum, false);
            }
        }

        public bool setDC(int deviceNum, bool status)
        {
            if (DC_.Length > deviceNum)
            {
                DC_[deviceNum] = status;
            }
            else
            {
                return false;
            }
            return true;
        }

        public int getACDC(Cmm1_RtuMath rtuMath)
        {
            if (EnumControlName.AC == rtuMath.ControlName)
            {
                return getAC(rtuMath.ControlNo);
            }
            else if (EnumControlName.DC == rtuMath.ControlName)
            {
                return getDC(rtuMath.ControlNo);
            }
            else
            {
                return -1;
            }

        }

        public int getACDC(EnumControlName controlName, int deviceNum)
        {
            if (EnumControlName.AC == controlName)
            {
                return getAC(deviceNum);
            }
            else if (EnumControlName.DC == controlName)
            {
                return getDC(deviceNum);
            }
            else
            {
                return -1;
            }

        }

        public int getAC(int deviceNum)
        {
            if (AC_.Length > deviceNum)
            {
                if (AC_[deviceNum])
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return -1;
            }
        }

        public int getDC(int deviceNum)
        {
            if (DC_.Length > deviceNum)
            {
                if (DC_[deviceNum])
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return -1;
            }
        }


        public bool setDoor(int deviceNum, int status)
        {
            if (status == 1)
            {
                return setDoor(deviceNum, true);
            }
            else
            {
                return setDoor(deviceNum, false);
            }
        }

        public bool setDoor(int deviceNum, bool status)
        {
            if (Door_.Length > deviceNum)
            {
                Door_[deviceNum] = status;
            }
            else
            {
                return false;
            }
            return true;
        }

        public int getDoor(int deviceNum)
        {
            if (Door_.Length > deviceNum)
            {
                if (Door_[deviceNum])
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return -1;
            }
        }


        public float Temperature
        {
            get
            {
                return Temperature_;
            }
            set
            {
                Temperature_ = value;
            }
        }

        public float Humidity
        {
            get
            {
                return Humidity_;
            }
            set
            {
                Humidity_ = value;
            }
        }
        public byte SoundAmp
        {
            get
            {
                return SoundAmp_;
            }
            set
            {
                SoundAmp_ = value;
            }
        }
        public bool SoundMute
        {
            get
            {
                return SoundMute_;
            }
            set
            {
                SoundMute_ = value;
            }
        }
        public int Door_Count
        {
            get
            {
                return Door_Count_;
            }
        }
       


    }

    public class RtuSendData
    {
        private byte[] sendData_;
        private int sendDataLen_;

        public RtuSendData()
        {
            sendData_ = null;
            sendDataLen_ = 0;
        }

        public RtuSendData(byte[] sendData, int sendDataLen)
        {
            this.sendDataLen_ = sendDataLen;
            this.sendData_ = sendData;
        }

        public byte[] sendData
        {
            get
            {
                return sendData_;
            }
            set
            {
                sendData_ = null;
                sendData_ = new byte[sendDataLen_];
                for (int loopCount = 0; loopCount < sendDataLen_; loopCount++)
                {
                    sendData_[loopCount] = value[loopCount];
                }

            }
        }

        public int sendDataLen
        {
            get
            {
                return sendDataLen_;
            }
            set
            {
                sendDataLen_ = value;
            }
        }
    }





}
