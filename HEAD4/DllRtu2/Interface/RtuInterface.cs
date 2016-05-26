using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using DllRtu.Event;
using DllRtu.Config;

namespace DllRtu.Interface
{

    public delegate void RtuEventHandler(object sender, RtuEventArgs args);

    public interface RtuInterface : IDisposable
    {


        bool sendToRtu(byte[] bytesData, int len);
        byte[] makeSendData(byte[] op_cmd);
        byte[] makeCheckSum(byte[] data);
       // byte[] makeByteSum(byte[] serial_Header, byte[] serial_Tail);
        byte[] makeByteSum(byte[] serial_Header, byte[] serial_Tail);

        RtuPropertys getPropertys();
        bool propertysLoad();
        event RtuEventHandler rtuEventHandler;
        void threadRun();
        bool getRtuThreadStatus();
        void setRtuThreadStatus(bool status);

        bool InitSerial();
        bool PortOpen();
        bool PortClose();

        void parser(byte[] recvData);

        RtuStatus getRtuStatus();
       
    }




}
