using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using DllRtu.Event;
using DllRtu.Config;

namespace DllRtu.IdonRtuInterface
{
    public delegate void Idon_RtuEventHandler(object sender, RtuEventArgs args);

    public interface Idon_RtuInterface : IDisposable
    {

        

        RtuPropertys getPropertys();
        bool propertysLoad();
        event Idon_RtuEventHandler idon_rtuEventHandler;
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
