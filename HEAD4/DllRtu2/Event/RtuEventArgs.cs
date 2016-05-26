using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DllRtu.Event
{
    public class RtuEventArgs : EventArgs
    {
        private string cmd = "";
        private object paramObject;

        public RtuEventArgs(object paramObject)
        {
            this.cmd = "FromRtu";
            this.paramObject = paramObject;
        }

        public RtuEventArgs(string cmd, object paramObject)
        {
            this.cmd = cmd;
            this.paramObject = paramObject; 
        }

        public string getCmd()
        {
            return this.cmd;
        }
        public object getParamObject()
        {
            return this.paramObject;
        }
    }

}


