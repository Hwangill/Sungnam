using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Common.Utility;

namespace DllRtu
{
   public class Rtu_Parsing
    {
       private string programName = "Rtu_Parsing";
       private string[] paserdatabyte;
       private string[] hexstringarry;

       public string[] byte_parsing(byte[] rtuProtocal)
       {
           string functionName = "byte_parsing";
           Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
           byte[] databyte = rtuProtocal;

             string hexstring = BitConverter.ToString(databyte).Replace("-", string.Empty);

             try
             {
                 if (databyte.Length > 4)
                 {
                     if (hexstring.StartsWith("02FD"))
                     {
                         paserdatabyte = hexbytegarry_function(hexstring);
                     }
                     else
                     {
                         int postion = hexstring.IndexOf("02FD");
                         if (postion > 0)
                         {
                             hexstring = hexstring.Substring(postion);
                         }

                         paserdatabyte = hexbytegarry_function(hexstring);

                     }

                 }

             }
             catch (Exception e)
             {
                 Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
             }
             return paserdatabyte;
       }

       private string[] hexbytegarry_function(string hexstring)
       {
           string functionName = "hexbytegarry_function";

           try
           {
               hexstringarry = Regex.Split(hexstring, "02FD");

           }
           catch (Exception e)
           {
               Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", e.ToString());
           }
           return hexstringarry;
       }
    }
}
