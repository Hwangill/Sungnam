using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Data;
using System.Xml;
using System.IO;
using Common.Utility;

namespace DllRtu.Config
{
    public class RtuPropertys
    {
        private string programName = "RtuPropertys";
        private string configFile = "./config/RtuPropertys.xml";
        private XmlDocument xmlDocument = null;
        private DataSet dataSet = null;

        #region RtuPropertys 변수 선언
        private string DllRtuVersion_;
        #region Serial
        private string Port_;
        private string BaudRate_;
        private string DataBits_;
        private string StopBits_;
        private string Parity_;
        private string RenewCycleTime_;
        #endregion        
        #region Mapping
        private string AC0_;
        private string AC1_;
        private string AC2_;
        private string AC3_;
        private string AC4_;
        private string AC5_;
        private string DC0_;
        private string DC1_;
        private string DC2_;
        private string DC3_;
        private string DC4_;
        private string DC5_;
        private string DC6_;
        private string DC7_;
        private string DC8_;
        #endregion        
        #region Tempature
        private string HeaterActionTempature_;
        private string Fan1ActionTempature_;
        private string Fan2ActionTempature_;
        #endregion        
        #region Sound
        private string SoundAmp_;
        private string SoundMute_;
        #endregion
        #endregion

        public RtuPropertys()
        {
            initConfig();
        }

        public RtuPropertys(string configfile)
        {
            this.configFile = configfile;
            initConfig();
        }

        public bool initConfig()
        {
            string functionName = "initConfig";
            
            bool returnValue = false;
            dataSet = new DataSet();
            xmlDocument = new XmlDocument();
                        
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            
            try
            {
                if (!File.Exists(configFile))
                {
                    Log.WriteLog(LogLevel.INFO, programName, functionName, "configFile Not Exists");
                    makeXML();
                }

                dataSet.ReadXml(configFile);
                xmlDocument.LoadXml(dataSet.GetXml());
                Log.WriteLog(LogLevel.DEBUG, programName, functionName, "XML Data", xmlDocument.ToString());
                returnValue = propertysLoad();
            }
            catch (System.Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }

        private void makeXML()
        {
            string functionName = "makeXML";
           
            XmlTextWriter xmlTextWriter = null;
                      
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");

            try
            {

                xmlTextWriter = new XmlTextWriter(configFile, System.Text.Encoding.Default);
                xmlTextWriter.Formatting = Formatting.Indented;

                #region RtuPropertys
                //RtuPropertys 

                xmlTextWriter.WriteStartElement("RtuPropertys");

                //DllRtuVersion
                if (true)
                {
                    xmlTextWriter.WriteStartElement("DllRtuVersion");
                    xmlTextWriter.WriteValue("1.0.0.0");
                    xmlTextWriter.WriteEndElement();
                }

                #region Serial
                //Serial
                if(true)
                {
                    xmlTextWriter.WriteStartElement("Serial");
                    
                    if(true)
                    {//Port
                        xmlTextWriter.WriteStartElement("Port");
                        xmlTextWriter.WriteValue("COM4");
                        xmlTextWriter.WriteEndElement();
                    }
                    if(true)
                    {//BaudRate
                        xmlTextWriter.WriteStartElement("BaudRate");
                        xmlTextWriter.WriteValue("9600");
                        xmlTextWriter.WriteEndElement();
                    }
                    if(true)
                    {//StopBits
                        xmlTextWriter.WriteStartElement("StopBits");
                        xmlTextWriter.WriteValue("1");
                        xmlTextWriter.WriteEndElement();
                    }
                    if(true)
                    {//Parity
                        xmlTextWriter.WriteStartElement("Parity");
                        xmlTextWriter.WriteValue("0");
                        xmlTextWriter.WriteEndElement();
                    }
                    if(true)
                    {//DataBits
                        xmlTextWriter.WriteStartElement("DataBits");
                        xmlTextWriter.WriteValue("8");
                        xmlTextWriter.WriteEndElement();
                    }
                     
                    if(true)
                    {//RenewCycleTime
                        xmlTextWriter.WriteStartElement("RenewCycleTime");
                        xmlTextWriter.WriteValue("60");
                        xmlTextWriter.WriteEndElement();
                    }
                        xmlTextWriter.WriteEndElement();
                }
                     
                #endregion
                                 
                #region Mapping
                //Mapping
                if(true)
                {
                    xmlTextWriter.WriteStartElement("Mapping");
                    
                    // AC
                    if(true)
                    {//AC1
                        xmlTextWriter.WriteStartElement("AC0");
                        xmlTextWriter.WriteValue("NA");
                        xmlTextWriter.WriteEndElement();
                    }                   
                    if(true)
                    {//AC1
                        xmlTextWriter.WriteStartElement("AC1");
                        xmlTextWriter.WriteValue("NA");
                        xmlTextWriter.WriteEndElement();
                    }
                    if(true)
                    {//AC2
                        xmlTextWriter.WriteStartElement("AC2");
                        xmlTextWriter.WriteValue("NA");
                        xmlTextWriter.WriteEndElement();
                    }
                    if(true)
                    {//AC3
                        xmlTextWriter.WriteStartElement("AC3");
                        xmlTextWriter.WriteValue("NA");
                        xmlTextWriter.WriteEndElement();
                    }
                    if(true)
                    {//AC4
                        xmlTextWriter.WriteStartElement("AC4");
                        xmlTextWriter.WriteValue("NA");
                        xmlTextWriter.WriteEndElement();
                    }
                    if(true)
                    {//AC5
                        xmlTextWriter.WriteStartElement("AC5");
                        xmlTextWriter.WriteValue("NA");
                        xmlTextWriter.WriteEndElement();
                    }
                    
                    // DC
                    if(true)
                    {//DC0
                        xmlTextWriter.WriteStartElement("DC0");
                        xmlTextWriter.WriteValue("NA");
                        xmlTextWriter.WriteEndElement();
                    }
                    if(true)
                    {//DC1
                        xmlTextWriter.WriteStartElement("DC1");
                        xmlTextWriter.WriteValue("NA");
                        xmlTextWriter.WriteEndElement();
                    }
                    if(true)
                    {//DC2
                        xmlTextWriter.WriteStartElement("DC2");
                        xmlTextWriter.WriteValue("NA");
                        xmlTextWriter.WriteEndElement();
                    }
                    if(true)
                    {//DC3
                        xmlTextWriter.WriteStartElement("DC3");
                        xmlTextWriter.WriteValue("NA");
                        xmlTextWriter.WriteEndElement();
                    }
                    if (true)
                    {//DC4
                        xmlTextWriter.WriteStartElement("DC4");
                        xmlTextWriter.WriteValue("NA");
                        xmlTextWriter.WriteEndElement();
                    }
                    if (true)
                    {//DC5
                        xmlTextWriter.WriteStartElement("DC5");
                        xmlTextWriter.WriteValue("NA");
                        xmlTextWriter.WriteEndElement();
                    }
                    if (true)
                    {//DC6
                        xmlTextWriter.WriteStartElement("DC6");
                        xmlTextWriter.WriteValue("NA");
                        xmlTextWriter.WriteEndElement();
                        if (true)
                        {//DC7
                            xmlTextWriter.WriteStartElement("DC7");
                            xmlTextWriter.WriteValue("NA");
                            xmlTextWriter.WriteEndElement();
                        }
                        if (true)
                        {//DC8
                            xmlTextWriter.WriteStartElement("DC8");
                            xmlTextWriter.WriteValue("NA");
                            xmlTextWriter.WriteEndElement();
                        }
                    }
                        xmlTextWriter.WriteEndElement();
                }
                      
                #endregion

                #region Tempature
                //Tempature
                if(true)
                {
                    xmlTextWriter.WriteStartElement("Tempature");
                    
                    if(true)
                    {//HeaterActionTempature
                        xmlTextWriter.WriteStartElement("HeaterActionTempature");
                        xmlTextWriter.WriteValue("10");
                        xmlTextWriter.WriteEndElement();
                    }                   
                    if(true)
                    {//Fan1ActionTempature
                        xmlTextWriter.WriteStartElement("Fan1ActionTempature");
                        xmlTextWriter.WriteValue("30");
                        xmlTextWriter.WriteEndElement();
                    }
                    if(true)
                    {//Fan2ActionTempature
                        xmlTextWriter.WriteStartElement("Fan2ActionTempature");
                        xmlTextWriter.WriteValue("40");
                        xmlTextWriter.WriteEndElement();
                    }
                    xmlTextWriter.WriteEndElement();
                }
                      
                #endregion
                               
                #region Sound
                //Sound
                if(true)
                {
                    xmlTextWriter.WriteStartElement("Sound");
                    if(true)
                    {//SoundAmp
                        xmlTextWriter.WriteStartElement("SoundAmp");
                        xmlTextWriter.WriteValue("15");
                        xmlTextWriter.WriteEndElement();
                    }     
                    if(true)
                    {//SoundMute
                        xmlTextWriter.WriteStartElement("SoundMute");
                        xmlTextWriter.WriteValue("0");
                        xmlTextWriter.WriteEndElement();
                    }                
                    xmlTextWriter.WriteEndElement();
                }
                #endregion

                xmlTextWriter.WriteEndElement();
                #endregion

                xmlTextWriter.Flush();
                xmlTextWriter.Close();

            }
            catch (System.Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                try
                {
                    if (xmlTextWriter != null)
                    {
                        xmlTextWriter.Flush();
                        xmlTextWriter.Close();
                    }
                }
                catch (System.Exception ex2)
                {
                    Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex2.ToString());
                }

            }
        }

        public bool propertysLoad()
        {
            string functionName = "propertysLoad";
            
            bool returnValue = false;
                       
            Log.WriteLog(LogLevel.TRACE, programName, functionName, "process");
            
            try
            {

                #region RtuPropertys propertysLoad                                                                                                                        #region RtuPropertys Load
                DllRtuVersion_ = xmlDocument.SelectNodes("//DllRtuVersion").Item(0).InnerText;
                #region Serial
                Port_= xmlDocument.SelectNodes("//Serial/Port").Item(0).InnerText;
                BaudRate_= xmlDocument.SelectNodes("//Serial/BaudRate").Item(0).InnerText;
                DataBits_= xmlDocument.SelectNodes("//Serial/DataBits").Item(0).InnerText;
                StopBits_= xmlDocument.SelectNodes("//Serial/StopBits").Item(0).InnerText;
                Parity_= xmlDocument.SelectNodes("//Serial/Parity").Item(0).InnerText;
                RenewCycleTime_= xmlDocument.SelectNodes("//Serial/RenewCycleTime").Item(0).InnerText;
                #endregion        
                #region Mapping
                AC0_= xmlDocument.SelectNodes("//Mapping/AC0").Item(0).InnerText;
                AC1_= xmlDocument.SelectNodes("//Mapping/AC1").Item(0).InnerText;
                AC2_= xmlDocument.SelectNodes("//Mapping/AC2").Item(0).InnerText;
                AC3_= xmlDocument.SelectNodes("//Mapping/AC3").Item(0).InnerText;
                AC4_= xmlDocument.SelectNodes("//Mapping/AC4").Item(0).InnerText;
                AC5_= xmlDocument.SelectNodes("//Mapping/AC5").Item(0).InnerText;
                DC0_= xmlDocument.SelectNodes("//Mapping/DC0").Item(0).InnerText;
                DC1_= xmlDocument.SelectNodes("//Mapping/DC1").Item(0).InnerText;
                DC2_= xmlDocument.SelectNodes("//Mapping/DC2").Item(0).InnerText;
                DC3_= xmlDocument.SelectNodes("//Mapping/DC3").Item(0).InnerText;
                DC4_ = xmlDocument.SelectNodes("//Mapping/DC4").Item(0).InnerText;
                DC5_ = xmlDocument.SelectNodes("//Mapping/DC5").Item(0).InnerText;
                DC6_ = xmlDocument.SelectNodes("//Mapping/DC6").Item(0).InnerText;
                DC7_ = xmlDocument.SelectNodes("//Mapping/DC7").Item(0).InnerText;
                DC8_ = xmlDocument.SelectNodes("//Mapping/DC8").Item(0).InnerText;
                #endregion       
                #region Tempature
                HeaterActionTempature_= xmlDocument.SelectNodes("//Tempature/HeaterActionTempature").Item(0).InnerText;
                Fan1ActionTempature_= xmlDocument.SelectNodes("//Tempature/Fan1ActionTempature").Item(0).InnerText;
                Fan2ActionTempature_= xmlDocument.SelectNodes("//Tempature/Fan2ActionTempature").Item(0).InnerText;
                #endregion        
                #region Sound
                SoundAmp_= xmlDocument.SelectNodes("//Sound/SoundAmp").Item(0).InnerText;
                SoundMute_= xmlDocument.SelectNodes("//Sound/SoundMute").Item(0).InnerText;
                #endregion
                #endregion

                Log.WriteLog(LogLevel.DEBUG, programName, functionName, "XML Data", xmlDocument.ToString());
                
            }
            catch (System.Exception ex)
            {
                Log.WriteLog(LogLevel.ERROR, programName, functionName, "Exception", ex.ToString());
                returnValue = false;
            }

            return returnValue;
        }

        #region RtuPropertys get/set
        public string DllRtuVersion
        {
            get
            {
                return DllRtuVersion_;
            }
            set
            {
                xmlDocument.SelectNodes("//DllRtuVersion").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                DllRtuVersion_ = value;
            }
        }

        #region Serial
        public string Port
        {
            get
            {
                return Port_;
            }
            set
            {
                xmlDocument.SelectNodes("//Serial/Port").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                Port_ = value;
            }
        }
        public string BaudRate
        {
            get
            {
                return BaudRate_;
            }
            set
            {
                xmlDocument.SelectNodes("//Serial/BaudRate").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                BaudRate_ = value;
            }
        }
        public string StopBits
        {
            get
            {
                return StopBits_;
            }
            set
            {
                xmlDocument.SelectNodes("//Serial/StopBits").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                StopBits_ = value;
            }
        }
        public string Parity
        {
            get
            {
                return Parity_;
            }
            set
            {
                xmlDocument.SelectNodes("//Serial/Parity").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                Parity_ = value;
            }
        }
        public string DataBits
        {
            get
            {
                return DataBits_;
            }
            set
            {
                xmlDocument.SelectNodes("//Serial/DataBits").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                DataBits_ = value;
            }
        }
        public string RenewCycleTime
        {
            get
            {
                return RenewCycleTime_;
            }
            set
            {
                xmlDocument.SelectNodes("//Serial/RenewCycleTime").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                RenewCycleTime_ = value;
            }
        }
        #endregion        
        #region Mapping
        public string AC0
        {
            get
            {
                return AC0_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/AC0").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                AC0_ = value;
            }
        }
        public string AC1
        {
            get
            {
                return AC1_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/AC1").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                AC1_ = value;
            }
        }
        public string AC2
        {
            get
            {
                return AC2_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/AC2").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                AC2_ = value;
            }
        }
        public string AC3
        {
            get
            {
                return AC3_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/AC3").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                AC3_ = value;
            }
        }
        public string AC4
        {
            get
            {
                return AC4_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/AC4").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                AC4_ = value;
            }
        }
        public string AC5
        {
            get
            {
                return AC5_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/AC5").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                AC5_ = value;
            }
        }
        public string DC0
        {
            get
            {
                return DC0_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/DC0").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                DC0_ = value;
            }
        }
        public string DC1
        {
            get
            {
                return DC1_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/DC1").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                DC1_ = value;
            }
        }
        public string DC2
        {
            get
            {
                return DC2_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/DC2").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                DC2_ = value;
            }
        }
        public string DC3
        {
            get
            {
                return DC3_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/DC3").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                DC3_ = value;
            }
        }
        public string DC4
        {
            get
            {
                return DC4_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/DC4").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                DC4_ = value;
            }
        }
        public string DC5
        {
            get
            {
                return DC5_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/DC5").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                DC5_ = value;
            }
        }
        public string DC6
        {
            get
            {
                return DC6_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/DC6").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                DC6_ = value;
            }
        }
        public string DC7
        {
            get
            {
                return DC7_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/DC7").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                DC5_ = value;
            }
        }
        public string DC8
        {
            get
            {
                return DC8_;
            }
            set
            {
                xmlDocument.SelectNodes("//Mapping/DC85").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                DC8_ = value;
            }
        }
        #endregion        
        #region Tempature
        public string HeaterActionTempature
        {
            get
            {
                return HeaterActionTempature_;
            }
            set
            {
                xmlDocument.SelectNodes("//Tempature/HeaterActionTempature").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                HeaterActionTempature_ = value;
            }
        }
        public string Fan1ActionTempature
        {
            get
            {
                return Fan1ActionTempature_;
            }
            set
            {
                xmlDocument.SelectNodes("//Tempature/Fan1ActionTempature").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                Fan1ActionTempature_ = value;
            }
        }
        public string Fan2ActionTempature
        {
            get
            {
                return Fan2ActionTempature_;
            }
            set
            {
                xmlDocument.SelectNodes("//Tempature/Fan2ActionTempature").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                Fan2ActionTempature_ = value;
            }
        }
        #endregion        
        #region Sound
        public string SoundAmp
        {
            get
            {
                return SoundAmp_;
            }
            set
            {
                xmlDocument.SelectNodes("//Sound/SoundAmp").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                SoundAmp_ = value;
            }
        }
        public string SoundMute
        {
            get
            {
                return SoundMute_;
            }
            set
            {
                xmlDocument.SelectNodes("//Sound/SoundMute").Item(0).InnerText = value;
                xmlDocument.Save(configFile);
                SoundMute_ = value;
            }
        }
        #endregion        
        #endregion        
        
        public string DataView()
        {
            string returnValue = "";

            returnValue += "\n ";
            returnValue += "\n DllRtuVersion         [" + DllRtuVersion + "]";
            returnValue += "\n ";
            returnValue += "\n Port                  [" + Port                  + "]";
            returnValue += "\n BaudRate              [" + BaudRate              + "]";
            returnValue += "\n DataBits              [" + DataBits              + "]";
            returnValue += "\n StopBits              [" + StopBits              + "]";
            returnValue += "\n Parity                [" + Parity                + "]";
            returnValue += "\n RenewCycleTime        [" + RenewCycleTime        + "]";
            returnValue += "\n ";
            returnValue += "\n AC0                   [" + AC0                   + "]";
            returnValue += "\n AC1                   [" + AC1                   + "]";
            returnValue += "\n AC2                   [" + AC2                   + "]";
            returnValue += "\n AC3                   [" + AC3                   + "]";
            returnValue += "\n AC4                   [" + AC4                   + "]";
            returnValue += "\n AC5                   [" + AC5                   + "]";
            returnValue += "\n DC0                   [" + DC0                   + "]";
            returnValue += "\n DC1                   [" + DC1                   + "]";
            returnValue += "\n DC2                   [" + DC2                   + "]";
            returnValue += "\n DC3                   [" + DC3                   + "]";
            returnValue += "\n DC4                   [" + DC4 + "]";
            returnValue += "\n DC5                   [" + DC5 + "]";
            returnValue += "\n DC6                   [" + DC6 + "]";
            returnValue += "\n DC7                   [" + DC7 + "]";
            returnValue += "\n DC8                   [" + DC8 + "]";
            returnValue += "\n ";
            returnValue += "\n HeaterActionTempature [" + HeaterActionTempature + "]";
            returnValue += "\n Fan1ActionTempature   [" + Fan1ActionTempature   + "]";
            returnValue += "\n Fan2ActionTempature   [" + Fan2ActionTempature   + "]";
            returnValue += "\n ";
            returnValue += "\n SoundAmp              [" + SoundAmp              + "]";
            returnValue += "\n SoundMute             [" + SoundMute             + "]";
            returnValue += "\n ";

            Log.WriteLog(LogLevel.DEBUG, "DataView", returnValue);
            return returnValue;
        }




    }
}
