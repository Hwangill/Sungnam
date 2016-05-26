using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using DllRtu;
using DllRtu.Interface;
using DllRtu.Cmm1RtuInterface;
using Common.Utility;

namespace BIT_TY
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //FrmBIT bit = new FrmBIT();
            //bit.Show();
            Thread rtuThread = null;
            ConstructFactory factory = null;

            Application.Run(new FrmBIT());

            try
            {
                factory = new ConstructFactory();
                rtuThread = new Thread(new ThreadStart(factory.startRtu));
                rtuThread.Start();


                try
                {
                    rtuThread.Abort();
                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogLevel.FATAL, "", ex.ToString());
                }

            }
            catch (System.Exception ex)
            {
                Log.WriteLog(LogLevel.FATAL, "MainFatal:", ex.ToString());
            }
            finally
            {
                try
                {
                    rtuThread.Abort();
                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogLevel.FATAL, "", ex.ToString());
                }
            }
            //log.Fatal("BIT_TY Program END");
        }
    }
}
