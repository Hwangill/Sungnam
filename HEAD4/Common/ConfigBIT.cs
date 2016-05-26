using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Drawing;
namespace Common
{
    /// <summary>
    /// BIT 설정 xml 정의
    /// </summary>
    [Serializable()]
    public class BITConfig
    {
        [XmlElement("ID")]
        public string ID { get; set; }
        [XmlElement("Server")]
        public List<string> Server { get; set; }
        [XmlElement("UART")]
        public string UARTPath { get; set; }
        [XmlElement("Camera")]
        public string CamPath { get; set; }
        [XmlElement("Color")]
        public string Color { get; set; }
        [XmlElement("Volume")]
        public string SoundVolume { get; set; }
        [XmlElement("RTU")]
        public string RTUPath { get; set; }
        [XmlElement("BasePath")]
        public string BasePath { get; set; }
    }

    [Serializable()]
    public class InfoCell
    {
        [XmlAttribute("IsLabel")]
        public bool IsLabel { get; set; }
        [XmlAttribute("Width")]
        public int Width { get; set; }
        [XmlAttribute("Height")]
        public int Height { get; set; }
        [XmlAttribute("XOffset")]
        public int XOffset { get; set; }
        [XmlAttribute("YOffset")]
        public int YOffset { get; set; }
        [XmlIgnore]
        public Color FGColor { get; set; }
        [XmlAttribute("TextColor")]
        public string TextColor
        {
            get { return ColorTranslator.ToHtml(FGColor); }
            set { FGColor = ColorTranslator.FromHtml(value); }
        }


        [XmlIgnore]
        public Color BGColor { get; set; }
        [XmlAttribute("BackColor")]
        public string BackColor
        {
            get { return ColorTranslator.ToHtml(BGColor); }
            set { BGColor = ColorTranslator.FromHtml(value); }
        }
        [XmlAttribute("FontName")]
        public string FontName { get; set; }

        [XmlAttribute("FontSize")]
        public float FontSize { get; set; }

        [XmlText]
        public string FieldName { get; set; }
    }

    [Serializable()]
    public class InfoRow
    {
        [XmlAttribute("Width")]
        public int Width { get; set; }
        [XmlAttribute("Height")]
        public int Height { get; set; }

        [XmlAttribute("XOffset")]
        public int XOffset { get; set; }
        [XmlAttribute("YOffset")]
        public int YOffset { get; set; }

        [XmlElement("Cells")]
        public List<InfoCell> Cells { get; set; }
    }

    [Serializable()]
    public class InfoCollection
    {
        [XmlElement("BackgroundImagePath")]
        public string BackgroundImagePath { get; set; }

        [XmlElement("TodayWeather")]
        public InfoCell TodayWeather { get; set; }
        [XmlElement("TomorowWeather")]
        public InfoCell TomorowWeather { get; set; }
        [XmlElement("TodayTemperature")]
        public InfoCell TodayTemperature { get; set; }
        [XmlElement("TomorowTemperature")]
        public InfoCell TomorowTemperature { get; set; }

        [XmlElement("StationName")]
        public InfoCell StationName { get; set; }
        [XmlElement("StationId")]
        public InfoCell StationId { get; set; }

        [XmlElement("TodayDate")]
        public InfoCell TodayDate { get; set; }
        [XmlElement("TimeNow")]
        public InfoCell TimeNow { get; set; }

        [XmlElement("NetworkStatus")]
        public InfoCell NetworkStatus { get; set; }

        [XmlElement("RTUStatus")]
        public InfoCell RTUStatus { get; set; }

        [XmlElement("CameraStatus")]
        public InfoCell CameraStatus { get; set; }

        [XmlElement("NearBus")]
        public InfoCell NearBus { get; set; }

        [XmlElement("Rows")]
        public List<InfoRow> Rows { get; set; }
    }
}