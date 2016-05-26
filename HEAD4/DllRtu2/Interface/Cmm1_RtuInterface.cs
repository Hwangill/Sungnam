using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using DllRtu.Event;
using DllRtu.Config;

namespace DllRtu.Cmm1RtuInterface
{
    public delegate void Cmm1_RtuEventHandler(object sender, RtuEventArgs args);
   public  interface Cmm1_RtuInterface
    {
        event Cmm1_RtuEventHandler rtuEventHandler;

        void Dispose();
        RtuPropertys getPropertys();
        RtuStatus getRtuControl();
        int getRtuControl(string DeviceName);
        RtuParamater getRtuParamater();
        RtuStatus getRtuStatus();
        int getRtuStatus(string DeviceName);
        bool getRtuThreadStatus();
        bool InitSerial();
        RtuSendData makeControlAC();
        RtuSendData makeControlDC();
        RtuSendData makeControlPcPower();
        RtuSendData makeControlPcReset();
        RtuSendData makeControlRtuReset();
        RtuSendData makeControlSoundAmp(byte soundAmp);
        RtuSendData makeControlSoundMute(bool soundMute);
        RtuSendData makeInquiryAC();
        RtuSendData makeInquiryDC();
        RtuSendData makeInquiryDoor();
        RtuSendData makeInquirySoundAmp();
        RtuSendData makeInquirySoundMute();
        RtuSendData makeInquiryTemperature();
        bool PortClose();
        bool PortOpen();
        bool propertysLoad();
        int runDeviceControl(EnumDeviceName device, bool status);
        void runRtuControl();
        bool sendToRtu(RtuSendData rtuSendData);
        void setRtuControl(RtuStatus inStatus);
        bool setRtuControl(string DeviceName, bool status);
        void setRtuParamater(RtuParamater inParamater);
        void setRtuStatus(RtuStatus inStatus);
        bool setRtuStatus(string DeviceName, bool status);
        void setRtuThreadStatus(bool status);
        bool setSoundAmp(int soundAmp);
        bool setSoundMute(bool soundMute);
        void threadRun();
        void parser(byte[] recvData);
    }
}
