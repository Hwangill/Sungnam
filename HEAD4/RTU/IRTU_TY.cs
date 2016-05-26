using System;
using System.IO.Ports;
using System.Xml.Serialization;

namespace RTU
{
    /// <summary>
    /// 시리얼 포트 설정에 대한 것
    /// </summary>
    [Serializable()]
    public class PortConfig
    {
        [XmlElement("PortName")]
        public string Name { get; set; }
        [XmlElement("BaudRate")]
        public int bRate { get; set; }
        [XmlElement("DataBits")]
        public int dBits { get; set; }
        [XmlElement("Parity")]
        public int parity { get; set; }
        [XmlElement("StopBits")]
        public string sBits { get; set; }
        [XmlElement("DTREnable")]
        public string dtrEnable { get; set; }
        [XmlElement("RTSEnable")]
        public string rtsEnable { get; set; }
        [XmlElement("Handshake")]
        public string hs { get; set; }
        [XmlElement("Timeout")]
        public string timeout { get; set; }
        [XmlElement("Encoding")]
        public string Encode { get; set; }
    }
    
    /// <summary>
    /// RTU 기본 인터페이스 정의
    /// </summary>
    public interface IRTU_TY : IDisposable
    {
        /// <summary>
        /// 초기화 및 포트 열기
        /// </summary>
        /// <returns>포트 열림 여부</returns>
        bool Initiate();

        /// <summary>
        /// In/Out 버퍼를 클리어하고, 다시 Open
        /// </summary>
        void Reset();

        /// <summary>
        /// 단순 연결
        /// </summary>
        /// <returns>Open 결과</returns>
        bool Connect();

        /// <summary>
        /// 전송
        /// </summary>
        /// <param name="_bytes"></param>
        void SendCommand(byte[] _bytes);

        /// <summary>
        /// 포트의 연결여부
        /// </summary>
        /// <returns>Open 상태</returns>
        bool IsConnected();
    }
}