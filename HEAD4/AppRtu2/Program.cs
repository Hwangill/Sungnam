using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Data;
using System.Xml;
using System.IO;
using DllRtu;
using DllRtu.Interface;
using System.Runtime.InteropServices;

using log4net;
using log4net.Config;
using Common.Utility;

namespace AppRtu2
{
    static class Program
    {
        [DllImport("user32.dll")]

        public static extern IntPtr FindWindow(

            string lpClassName, // class name

            string lpWindowName // window name

        );
        #region log4net
        static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion //log4net
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>

        [STAThread]
        static void Main()
        {

            #region log4net
            XmlConfigurator.Configure(new System.IO.FileInfo(".\\config/rtuLog.xml"));
            #endregion //log4net

            log.Fatal("AppRtu Program Start");

            IntPtr hWnd = FindWindow(null, "AppRtu");

            if (hWnd.ToInt32() > 0)
            {
                //존재 할 경구 중복 일 경우
                log.Fatal("AppRtu Program 이미 실행 중입니다.");
                Application.Exit();
                return;
            }

            Thread rtuThread = null;
            RtuInterface rtu = null;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {

                rtu = new Rtu();

                rtuThread = new Thread(new ThreadStart(rtu.threadRun));
                //rtuThread.Start();
                Application.Run(new AppRtu(rtu, rtuThread));
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex.ToString());
            }

        }
    }
}
