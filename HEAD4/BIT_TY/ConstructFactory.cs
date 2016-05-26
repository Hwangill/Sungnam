using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Data;
using System.Net;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Data.Common;

using System.Reflection;

using Common.Utility;
using DllRtu.IdonRtuInterface;
using DllRtu.Cmm1RtuInterface;
using DllRtu;
using DllRtu.Interface;

namespace BIT_TY
{
    public class ConstructFactory
    {
        public RtuInterface rtu = null;
        public Idon_RtuInterface idonrtu = null;
        public Cmm1_RtuInterface cmm1rtu = null;

        public string rtutype;
        public string window;

        public ConstructFactory()
        {
            try
            {
                Assembly assemObj = Assembly.GetExecutingAssembly();
                Version v = assemObj.GetName().Version; // 현재 실행되는 어셈블리..dll의 버전 가져오기

                int majorV = v.Major; // 주버전
                int minorV = v.Minor; // 부버전
                int buildV = v.Build; // 빌드번호
                int revisionV = v.Revision; // 수정번호

                string FullV = majorV.ToString() + ".";
                FullV = FullV + minorV.ToString() + ".";
                FullV = FullV + buildV.ToString() + ".";
                FullV = FullV + revisionV.ToString();

                if (rtutype == "idon")
                {
                    idonrtu = new Idon_Rtu("./config/rtuPropertys.xml");

                }
                else if (rtutype == "cmm3")
                {
                    rtu = new Rtu("./config/rtuPropertys.xml");
                }
                else if (rtutype == "cmm1")
                {
                    cmm1rtu = new Cmm1_Rtu("./config/rtuPropertys.xml");
                }
                else
                {
                    idonrtu = new Idon_Rtu("./config/rtuPropertys.xml");
                }

            }
            catch (Exception e)
            {
                Log.WriteLog(LogLevel.FATAL, "", e.ToString());    
            }
            finally
            {

            }
        }
        public void startRtu()
        {
            try
            {

                if (rtutype == "idon")
                {
                    idonrtu.threadRun();
                }
                else if (rtutype == "cmm3")
                {
                    rtu.threadRun();
                }
                else if (rtutype == "cmm1")
                {
                    cmm1rtu.threadRun();
                }
                else
                {
                    rtu.threadRun();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, ex.ToString());
            }
        }
    }
     
}
