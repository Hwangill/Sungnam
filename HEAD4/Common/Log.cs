using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;
using log4net.Config;

namespace Common.Utility
{



    /// <summary>
    /// 요약
    /// </summary>
    /// <example>
    /// 예제 설명
    /// <code>
    /// 코드 표현
    /// </code>
    /// </example>
    /// <param name="param1">파라미터</param>
    /// <return>리턴값</return>

    #region LogLevel 정의

    /// <summary>
    /// LogLevel 정의
    /// </summary>
    public enum LogLevel
    {
        TRACE = 0,
        DEBUG = 2,
        INFO = 4,
        WARN = 6,
        ERROR = 8,
        FATAL = 10,
    }
    #endregion

    public class Log
    {

        #region log4net
        static readonly ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion //log4net

        /// <summary>
        /// BitLog
        /// </summary>
        public Log()
        {


        }



        #region Write Log 매서드


        /// <summary>
        /// 모든 로그 쓰기
        /// </summary>
        /// <example>
        /// 로그의 레벨 및 타이틀 메세지 입력
        /// <code>
        /// Write(logLevel,title, message ) 
        /// </code>
        /// </example>
        /// <param name="logLevel">logLevel</param>
        /// <param name="title">title</param>
        /// <param name="message">message</param>
        static public void WriteLog(LogLevel logLevel, string message)
        {

            switch (logLevel)
            {
                case LogLevel.TRACE: Console.WriteLine(message); break;
                case LogLevel.DEBUG: log.Debug(message); break;
                case LogLevel.INFO: log.Info(message); break;
                case LogLevel.WARN: log.Warn(message); break;
                case LogLevel.ERROR: log.Error(message); break;
                case LogLevel.FATAL: log.Fatal(message); break;

            }

        }


        /// <summary>
        /// 모든 로그 쓰기
        /// </summary>
        /// <example>
        /// 로그의 레벨 및 타이틀 메세지 입력
        /// <code>
        /// Write(logLevel,title, message ) 
        /// </code>
        /// </example>
        /// <param name="logLevel">logLevel</param>
        /// <param name="title">title</param>
        /// <param name="message">message</param>
        static public void WriteLog(LogLevel logLevel, string title, string message)
        {
            WriteLog(logLevel, title + message);
        }

        /// <summary>
        /// 모든 로그 쓰기
        /// </summary>
        /// <example>
        /// 로그의 레벨 및 타이틀 메세지 입력
        /// <code>
        /// Write(logLevel,title, message ) 
        /// </code>
        /// </example>
        /// <param name="logLevel">logLevel</param>
        /// <param name="title1">title1</param>
        /// <param name="title2">title2</param>
        /// <param name="message">message</param>
        static public void WriteLog(LogLevel logLevel, string title1, string title2, string message)
        {
            string title = title1 + ":" + title2;
            WriteLog(logLevel, title + message);
        }

        /// <summary>
        /// 모든 로그 쓰기
        /// </summary>
        /// <example>
        /// 로그의 레벨 및 타이틀 메세지 입력
        /// <code>
        /// Write(logLevel,title, message ) 
        /// </code>
        /// </example>
        /// <param name="logLevel">logLevel</param>
        /// <param name="title1">title1</param>
        /// <param name="title2">title2</param>
        /// <param name="title3">title3</param>
        /// <param name="message">message</param>
        static public void WriteLog(LogLevel logLevel, string title1, string title2, string title3, string message)
        {
            string title = title1 + ":" + title2 + ":" + title3;
            WriteLog(logLevel, title + message);
        }

        #endregion




    }
}