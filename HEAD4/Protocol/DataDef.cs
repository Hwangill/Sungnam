using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Common.Utility;

namespace Protocol
{
    public class DataDef
    {
        public static byte[] SerializeToBytes<T>(T item)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, item);
                stream.Seek(0, SeekOrigin.Begin);
                return stream.ToArray();
            }
        }

        public static object DeserializeFromBytes(byte[] bytes)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream(bytes))
            {
                //stream.Flush();
                try
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    return formatter.Deserialize(stream);
                }
                catch (SerializationException ex)
                {
                    throw;
                }
                catch (Exception)
                {
                    
                    throw;
                }

            }
        }
    }

    namespace GG_SN
    {
        public enum FileType
        {
            [Description("Master Data")]        MASTER_DATA = 0,
            [Description("시정홍보 시나리오")]        AD_SCENARIO = 1,
            [Description("시정홍보 컨텐츠")]        AD_CONTENTS = 2,
            [Description("뉴스")]        NEWS = 3,
            [Description("시정공지")]        ANNOUNCEMENT = 4,
            [Description("날씨")]        WEATHER = 5,
            [Description("ISM 스케쥴")]        ISM_SCHEDULE = 6,
            [Description("일정 적용 형 시정홍보")]        DATE_APPLY_AD = 7,
            [Description("전등 켜고 끔 스케쥴")]        LAMP_SCHEDULE = 8,
            [Description("정류소 안내기 프로그램")]        BIT_PRG = 100,
            [Description("제어보드 펌웨어")]        RTU_FIRMWARE = 101
        }

        [Flags]
        public enum BusRouteMask
        {
            
        }

        [Flags]
        public enum BusRunMask
        {
            
        }

        [Flags]
        public enum StatusMask : int
        {

        }

        [Flags]
        public enum FileOperation : byte
        {
            //0 성공
            //인증실패, 파일없음, 잘못된 명령, 전송 실패, Fileserver 응답없음, 알수없는 오류
        }

        public class FTPTransfer
        {
            public string remoteFile;
            public string localFile;
        }

        public class MediaFileInfo
        {
            public string fileName;
            public byte fileType;
            public int durSeconds;
        }

        public class PredictInfo
        {
            public DateTime recvTime;
            public string routeName;
            public string stationName;
            public string remainTime;
            public UInt16 nremTime;
            public UInt16 nremStation;
            public bool bApproach;
            public string remainStation;


            public string routeID;
            public string busid;
            public string busName;
            public string busType;
            public int runType;//!< 0첫차, 1막차, .... 현위치
            public string startStationName;
            public string endStationName;
        }

        public class BISInfoEventArgs : EventArgs
        {
            public List<PredictInfo> Info { get; private set; }

            public BISInfoEventArgs(List<PredictInfo> info)
            {
                this.Info = info.ToList();//new List<PredictInfo>(info);
            }
        }

        public class Route
        {
            public string routeID;
            public string routeName;
            public string routeType;
            public string startStationID;
            public string endStationID;
            public string companyName;
            public string phoneNumber;
            public string firstTime;
            public string lastTime;
        }

        public class Station
        {
            public string stationID;
            public string stationName;
            public string mobileNo;
            public string engName;
            public string chnName;
            public string jpnName;
        }

        public class RouteAllocation
        {
            public string period_id;
            public string dow_tp;
            public string st_date;
            public string ed_date;
            public string routeID;
            public string stationID;
            public string dep_time;
            public string dep_bstop_tp;
        }

        public class RouteStation
        {
            public string routeID;
            public string stationOrder;
            public string stationID;
        }


        public interface IBinarySerializable
        {
            byte[] GetBytes();
            void WriteDataTo(BinaryWriter _Writer);
            void SetDataFrom(BinaryReader _Reader);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class DataFrame : IBinarySerializable
        {
            public ushort Seq;
            public ushort DataLength;
            public byte OpCode;

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(ByteStreamCovert.ReverseBytes(Seq));
                        writer.Write(ByteStreamCovert.ReverseBytes(DataLength));
                        writer.Write(OpCode);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write((ushort)Seq);
                _Writer.Write((ushort)DataLength);
                _Writer.Write((byte)OpCode);
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                Seq = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt16());
                DataLength = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt16());
                OpCode = _Reader.ReadByte();
            }
            
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class MasterFileInfo : IBinarySerializable
        {
            public FileType fileType { get; set; }

            public UInt32 fileTime { get; set; }

            public byte idMode { get; set; }

            public DateTime makeTime { get; set; }
            public byte fileNameLen { get; set; }
            public string fileName { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write((byte)fileType);
                _Writer.Write(ByteStreamCovert.ReverseBytes(fileTime));
                _Writer.Write(idMode);
                _Writer.Write(fileNameLen);
                _Writer.Write(Encoding.UTF8.GetBytes(fileName));
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                fileType = (FileType)Enum.Parse(typeof(FileType), _Reader.ReadByte().ToString());
                fileTime = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                idMode = _Reader.ReadByte();
                makeTime = new DateTime(1970, 1, 1).AddSeconds(fileTime).ToLocalTime();
                fileNameLen = _Reader.ReadByte();
                fileName = Encoding.UTF8.GetString(_Reader.ReadBytes((int)fileNameLen));
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class SimpleFileInfo : IBinarySerializable
        {
            public FileType fileType { get; set; }

            public UInt32 fileTime { get; set; }

            public DateTime makeTime { get; set; }
            public byte fileNameLen { get; set; }
            public string fileName { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write((byte)fileType);
                _Writer.Write(ByteStreamCovert.ReverseBytes(fileTime));
                _Writer.Write(fileNameLen);
                _Writer.Write(Encoding.UTF8.GetBytes(fileName));
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                fileType = (FileType)Enum.Parse(typeof(FileType), _Reader.ReadByte().ToString());
                fileTime = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                makeTime = new DateTime(1970, 1, 1).AddSeconds(fileTime).ToLocalTime();
                fileNameLen = _Reader.ReadByte();
                fileName = Encoding.UTF8.GetString(_Reader.ReadBytes((int)fileNameLen));
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class FileServer : IBinarySerializable
        {
            public uint ExternalIP { get; set; }
            public IPAddress ExtIpAddress { get; set; }
            public ushort Port { get; set; }
            public byte ServerType { get; set; }
            public FTP_Authentication AuthInfo { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(ByteStreamCovert.ReverseBytes(ExternalIP));
                _Writer.Write(ByteStreamCovert.ReverseBytes(Port));
                _Writer.Write(ServerType);
                _Writer.Write(AuthInfo.GetBytes());
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                ExternalIP = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                ExtIpAddress = new IPAddress(new byte[] { 
                (byte)((ExternalIP>>24) & 0xFF) ,
                (byte)((ExternalIP>>16) & 0xFF) , 
                (byte)((ExternalIP>>8)  & 0xFF) , 
                (byte)( ExternalIP & 0xFF)});
                Port = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt16());
                ServerType = _Reader.ReadByte();
                AuthInfo = new FTP_Authentication();
                AuthInfo.SetDataFrom(_Reader);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class FTP_Authentication : IBinarySerializable
        {
            public byte NameLen { get; set; }
            public string Name { get; set; }
            public byte PasswdLen { get; set; }
            public string Password { get; set; }
            public byte DataMode { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(NameLen);
                _Writer.Write(Encoding.UTF8.GetBytes(Name));
                _Writer.Write(PasswdLen);
                _Writer.Write(Encoding.UTF8.GetBytes(Password));
                _Writer.Write(DataMode);
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                NameLen = _Reader.ReadByte();
                Name = Encoding.UTF8.GetString(_Reader.ReadBytes(NameLen));
                PasswdLen = _Reader.ReadByte();
                Password = Encoding.UTF8.GetString(_Reader.ReadBytes(PasswdLen));
                DataMode = _Reader.ReadByte();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class RF_Authentication
        {

        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Resp_Authentication
        {
            public ushort Seq { get; set; }
            public byte[] bitid { get; set; }//9 byte
            public UInt32 support_statusmask { get; set; }
            public byte netType { get; set; }

            public byte[] WriteToStream()
            {
                //BinaryFormatter formatter = new BinaryFormatter();
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(ByteStreamCovert.ReverseBytes(Seq));
                        writer.Write(bitid);
                        writer.Write(ByteStreamCovert.ReverseBytes(support_statusmask));
                        writer.Write(netType);
                    }
                    return stream.ToArray();
                }
                return null;
                //writer.Write(toWrite.info);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Permit_Authentication : IBinarySerializable
        {
            public ushort Seq;
            public byte Permit;
            public byte NetType;

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(ByteStreamCovert.ReverseBytes(Seq));
                        writer.Write(Permit);
                        writer.Write(NetType);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(ByteStreamCovert.ReverseBytes(Seq));
                _Writer.Write(Permit);
                _Writer.Write(NetType);
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                Seq = _Reader.ReadUInt16();
                Permit = _Reader.ReadByte();
                NetType = _Reader.ReadByte();
                //Filename = _Reader.ReadString();//Encoding.Default.GetString();
            }
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Scenario_WithoutSchedule : IBinarySerializable
        {
            public byte orderNum { get; set; }
            public byte dataType { get; set; }
            public byte[] fileBytes { get; set; }

            public string fileName { get; set; }
            public byte durationSecond { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(orderNum);
                _Writer.Write(dataType);
                _Writer.Write(fileBytes);
                _Writer.Write(durationSecond);
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                Encoding ec = System.Text.Encoding.GetEncoding(51949);
                orderNum = _Reader.ReadByte();
                dataType = _Reader.ReadByte();
                fileBytes = _Reader.ReadBytes(32);
                fileName = ec.GetString(fileBytes);
                durationSecond = _Reader.ReadByte();


            }
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Scenario_WithSchedule : IBinarySerializable
        {
            public byte orderNum { get; set; }
            public int utctimeStart { get; set; }
            public int utctimeEnd { get; set; }
            public byte dataType { get; set; }
            public byte[] fileName { get; set; }
            public byte durationSecond { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(orderNum);
                _Writer.Write(ByteStreamCovert.ReverseBytes((UInt32)utctimeStart));
                _Writer.Write(ByteStreamCovert.ReverseBytes((UInt32)utctimeEnd));
                _Writer.Write(dataType);
                _Writer.Write(fileName);
                _Writer.Write(durationSecond);
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                orderNum = _Reader.ReadByte();
                utctimeStart = (int)ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                utctimeEnd = (int)ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                dataType = _Reader.ReadByte();
                fileName = _Reader.ReadBytes(32);
                durationSecond = _Reader.ReadByte();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Scenario_DataPlain : IBinarySerializable
        {
            public int utctime { get; set; }
            public byte frmCount { get; set; }
            public Scenario_WithoutSchedule[] formData { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(ByteStreamCovert.ReverseBytes((UInt32)utctime));
                _Writer.Write(frmCount);
                if (frmCount > 0)
                {
                    foreach (var item in formData)
                    {
                        _Writer.Write(item.GetBytes());
                    }
                }
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                utctime = (int)ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                DateTime kk = new DateTime(1970, 1, 1).AddSeconds(utctime);
                frmCount = _Reader.ReadByte();
                if (frmCount > 0)
                {
                    formData = new Scenario_WithoutSchedule[frmCount];

                    for (int idx = 0; idx < frmCount; idx++)
                    {
                        formData[idx] = new Scenario_WithoutSchedule();
                        formData[idx].SetDataFrom(_Reader);
                    }
                }
            }
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Scenario_DataSchedule : IBinarySerializable
        {
            public int utctime { get; set; }
            public byte frmCount { get; set; }

            public Scenario_WithSchedule[] formData { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(ByteStreamCovert.ReverseBytes((UInt32)utctime));
                _Writer.Write(frmCount);
                if (frmCount > 0)
                {
                    foreach (var item in formData)
                    {
                        _Writer.Write(item.GetBytes());
                    }
                }
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                utctime = (int)ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                frmCount = _Reader.ReadByte();
                if (frmCount > 0)
                {
                    formData = new Scenario_WithSchedule[frmCount];

                    for (int idx = 0; idx < frmCount; idx++)
                    {
                        formData[idx].SetDataFrom(_Reader);
                    }
                }
            }
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Info_CommEnv : IBinarySerializable
        {
            public int servertime { get; set; }
            public DateTime serverDTTM { get; set; }
            public byte langcode { get; set; }
            //public byte idmode { get; set; }
            public byte waittime { get; set; }
            public byte retrycnt { get; set; }
            public FileServer fserver { get; set; }
            public byte nstation { get; set; }
            private string[] stationid { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(ByteStreamCovert.ReverseBytes((UInt32)servertime));
                _Writer.Write(langcode);
                //_Writer.Write(idmode);
                _Writer.Write(waittime);
                _Writer.Write(retrycnt);
                //_Writer.Write(fserver.GetB);
                _Writer.Write(nstation);
                if (nstation > 0)
                {

                    
                }
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                servertime = (int) ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());

                serverDTTM = new DateTime(1970, 1, 1).AddSeconds(servertime).ToLocalTime();

                langcode = _Reader.ReadByte();
                //idmode = _Reader.ReadByte();
                waittime = _Reader.ReadByte();
                retrycnt = _Reader.ReadByte();
                fserver = new FileServer();
                fserver.SetDataFrom(_Reader);
                nstation = _Reader.ReadByte();
                if (nstation > 0)
                {
                    stationid = new string[nstation];
                    for (int idx = 0; idx < nstation ; idx++)
                    {
                        stationid[idx] = System.Text.Encoding.UTF8.GetString(_Reader.ReadBytes(9));
                    }
                }

            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Info_Sync : IBinarySerializable 
        {
            public byte nEntry { get; set; }
            public byte[] Entries { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(nEntry);
                _Writer.Write(Entries);
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                nEntry = _Reader.ReadByte();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Req_FileOperation
        {
            private byte nEntry;//0 - down, 1 - up, 2 - delete
            private SimpleFileInfo ent;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Resp_FileOperation
        {
            private byte nEntry;//0 - down, 1 - up, 2 - delete
            private FileOperation operation;
            private SimpleFileInfo ent;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Req_StatusStart
        {
            private ushort usPeriod;//0 - down, 1 - up, 2 - delete
            private StatusMask status;
        }

        //메모리 2, lcd/led x, y 2byte
        //주변밝기 2
        //충격감지 정보 2(영상개수, 충격양)
        //알람감지여부, lcd 전원 켜짐상태, lcd 출력밝기, led 전원, 제어보드 상태,
        // 팬(동작, 제어), 히터(동작, 제어), 
        //온도, 습도, 음량
        //도어센서
        //캠0, 캠1 상태
        //ism ?? 제어전용
        //ac 전원
        //충격 thr
        //스크린 캡쳐 상태
        //언어상태 0 한국어
        //캠2

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class StatusControlUnit : IBinarySerializable
        {
            public byte Id { get; set; }
            public byte[] statusvalue { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(Id);
                if (statusvalue != null)
                    _Writer.Write(statusvalue);
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                Id = _Reader.ReadByte();
                //FileName = Encoding.UTF8.GetString(_Reader.ReadBytes((int)fileNameLen));
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Resp_StatusStart : IBinarySerializable
        {
            public ushort reqSeq { get; set; }//0 - down, 1 - up, 2 - delete
            public byte nInfos { get; set; }

            public StatusControlUnit[] statusvalue { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(ByteStreamCovert.ReverseBytes(reqSeq));
                _Writer.Write(nInfos);
                if (statusvalue != null)
                {
                    foreach (var stat in statusvalue)
                    {
                        _Writer.Write(stat.GetBytes());
                    }
                }
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                reqSeq = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt16());
                nInfos = _Reader.ReadByte();
                //FileName = Encoding.UTF8.GetString(_Reader.ReadBytes((int)fileNameLen));
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Periodic_Status : IBinarySerializable
        {
            public byte nInfos { get; set; }

            public StatusControlUnit[] statusvalue { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(nInfos);
                if (statusvalue != null)
                {
                    foreach (var stat in statusvalue)
                    {
                        _Writer.Write(stat.GetBytes());
                    }    
                }

            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                nInfos = _Reader.ReadByte();
                //FileName = Encoding.UTF8.GetString(_Reader.ReadBytes((int)fileNameLen));
            }
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class BusUnit : IBinarySerializable
        {
            public byte[]   id = new byte[9];//9
            public UInt16   remaintime;
            public byte[]   locid = new byte[9];//9
            public byte     remainstop;
            public byte     busattrib;
            public byte     busstatus; //!< 1이면 확장데이터 있음
            public byte[]   extender = null;// 현재 항목수 - 1, 정보항목ID - 1, 종착역 ID - 9 만 정의됨

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(id);
                _Writer.Write(ByteStreamCovert.ReverseBytes(remaintime));
                _Writer.Write(locid);
                _Writer.Write(remainstop);
                _Writer.Write(busattrib);
                _Writer.Write(busstatus);//!< 왁장은 일단 패스
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                id = _Reader.ReadBytes(9);
                remaintime = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt16());
                locid = _Reader.ReadBytes(9);
                remainstop = _Reader.ReadByte();
                busattrib = _Reader.ReadByte();
                busstatus = _Reader.ReadByte();
                //!< 왁장은 일단 패스
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class BusUnits : IBinarySerializable
        {
            public byte unitcnt;
            public BusUnit[] bInfo = null;

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(unitcnt);
                foreach (var item in bInfo)
                {
                    _Writer.Write(item.GetBytes());
                }
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                unitcnt = _Reader.ReadByte();
                bInfo = new BusUnit[unitcnt];
                for (int idx = 0; idx < (int) unitcnt; idx++)
                {
                    bInfo[idx] = new BusUnit();
                    bInfo[idx].SetDataFrom(_Reader);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class RouteUnit : IBinarySerializable
        {
            public byte[] dest_station_id = new byte[9];//9
            public byte[] route_id = new byte[9];//9
            public UInt32 servercalctime;//time_t, Convert.ToDateTime
            public byte routestatus;
            public BusUnits bInfo;//if routestatus == 2 only
            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(dest_station_id);
                _Writer.Write(route_id);
                _Writer.Write(ByteStreamCovert.ReverseBytes((UInt32)servercalctime));
                _Writer.Write(routestatus);
                if (routestatus == 2)
                {
                    _Writer.Write(bInfo.GetBytes());
                }
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                dest_station_id = _Reader.ReadBytes(9);
                route_id = _Reader.ReadBytes(9);
                servercalctime = (UInt32)ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                routestatus = _Reader.ReadByte();

                if (routestatus == 2)
                {
                    bInfo = new BusUnits();
                    bInfo.SetDataFrom(_Reader);
                }

            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class BIS_Info : IBinarySerializable//시작, 안내정보 모두 동일
        {
            public byte routcnt;
            public RouteUnit[] rInfo;//routecnt

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write((byte)routcnt);
                foreach (var item in rInfo)
                {
                    _Writer.Write(item.GetBytes());
                }
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                routcnt = _Reader.ReadByte();
                rInfo = new RouteUnit[routcnt];
                for (int idx = 0; idx < (int) routcnt; idx++)
                {
                    rInfo[idx] = new RouteUnit();
                    rInfo[idx].SetDataFrom(_Reader);
                }
            }
        }

        //ISM을 제외하고 모두 1Byte
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Req_Control
        {
            private byte id;
            private byte[] vv;//routecnt
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Resp_Status
        {
            public ushort Seq;
            public byte Count;

            public StatusControlUnit[] statusvalue = null;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class ProviderUnit
        {
            private uint ip;
            private ushort port;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Resp_Provider
        {
            private ushort seq;
            ProviderUnit[] primary = new ProviderUnit[2];
            ProviderUnit[] secondary = new ProviderUnit[2];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Set_Provider
        {
            ProviderUnit[] primary = new ProviderUnit[2];
            ProviderUnit[] secondary = new ProviderUnit[2];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Temperature : IBinarySerializable
        {
            public byte Max { get; set; }
            public byte Min { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(Max);
                _Writer.Write(Min);
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                Max = _Reader.ReadByte();
                Min = _Reader.ReadByte();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Resp_Temperature : IBinarySerializable
        {
            public ushort Seq { get; set; }
            public Temperature Value { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(Seq);
                _Writer.Write(Value.GetBytes());
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                Seq = _Reader.ReadByte();
                //Value = _Reader.ReadByte();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class DisplaySchedule
        {
            private byte onhour;
            private byte onminute;
            private byte offhour;
            private byte offminute;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Resp_DisplaySchedule
        {
            private ushort seq;
            private DisplaySchedule schedule;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Resp_VehicleLoc
        {
            private byte[] routeid;
            private byte cnt;
            private BusUnit[] locationinfo;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class FileUnit
        {
            private byte pathlen;
            private byte[] pathname;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Req_ScreenCapture_Start
        {
            private byte interval;
            private byte cnt;
            private FileServer server;
            private FileUnit file;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Report_ScreenCapture
        {
            private byte cnt;
            private FileUnit[] file;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class SoundMixerUnit
        {
            private byte mixerid;
            private ushort vol;
            private byte muteflag;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Resp_SoundMixer
        {
            private ushort seq;
            private byte cnt;
            private SoundMixerUnit[] mixer = null;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class Set_SoundMixer
        {
            private byte cnt;
            private SoundMixerUnit[] mixer = null;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable()]
        public class ShortMessage : IBinarySerializable
        {
            private byte info;
            private uint startTime;
            private uint endTime;
            private ushort nLen;
            private byte[] msg = null;
            private byte ttslen;
            private byte[] ttsfileloc = null;

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(info);
                _Writer.Write(ByteStreamCovert.ReverseBytes((UInt32)startTime));
                _Writer.Write(ByteStreamCovert.ReverseBytes((UInt32)endTime));
                _Writer.Write(ByteStreamCovert.ReverseBytes((ushort)nLen));
                _Writer.Write(msg);
                _Writer.Write(ttslen);
                _Writer.Write(ttsfileloc);
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                info = _Reader.ReadByte();
                startTime = (uint)ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                endTime = (uint)ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                endTime = (ushort)ByteStreamCovert.ReverseBytes(_Reader.ReadUInt16());
                msg = _Reader.ReadBytes(nLen);
                //idmode = _Reader.ReadByte();
                ttslen = _Reader.ReadByte();
                ttsfileloc = _Reader.ReadBytes(ttslen);
            }
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public class StillImage_Control
        {
            public byte GrabInterval { get; set; }
            public byte BatchCount { get; set; }
            public FileServer ServerInfo { get; set; }
            public byte PathLen { get; set; }
            public string UploadPath { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public class VideoTransmit_Control
        {
            public uint ServerAddress { get; set; }
            public ushort ServerPort { get; set; }
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public class VideoBroadcast_Control
        {
            public ushort Port { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public class Req_CAM_Control
        {
            public byte channel { get; set; }
            public byte command { get; set; }
            //!< 
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public class Resp_CAM_Control
        {
            public byte ResultCode { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public class CAM_Image : SimpleFileInfo
        {
            
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public class Report_CAM_Image
        {
            public byte FileCount { get; set; }
            public CAM_Image[] FileSets { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public class Req_Shock_Record : CAM_Image
        {
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public class Shock_Record : IBinarySerializable
        {
            public UInt32 RaiseTime { get; set; }
            public UInt16 TimeFriction { get; set; }
            public byte Level { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(ByteStreamCovert.ReverseBytes(RaiseTime));
                _Writer.Write(ByteStreamCovert.ReverseBytes(TimeFriction));
                _Writer.Write(ByteStreamCovert.ReverseBytes(Level));
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                RaiseTime = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                TimeFriction = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt16());
                Level = _Reader.ReadByte();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public class Resp_Shock_Record : IBinarySerializable
        {
            public Shock_Record Record { get; set; }

            public byte videoNameLen { get; set; }

            public byte[] videoName { get; set; }
            public byte BitField { get; set; }
            public Shock_Record[] Repeats { get; set; }

            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(Record.GetBytes());
                _Writer.Write(videoNameLen);
                _Writer.Write(videoName);
                _Writer.Write(BitField);
                //_Writer.Write(ByteStreamCovert.ReverseBytes(Level));
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                Record.SetDataFrom(_Reader);
                //TimeFriction = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt16());
                videoNameLen = _Reader.ReadByte();
                videoName = _Reader.ReadBytes(videoNameLen);
                BitField = _Reader.ReadByte();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public class Req_External_IP
        {
            public byte Method { get; set; }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [Serializable]
        public class Resp_External_IP : IBinarySerializable
        {
            public byte ResultCode { get; set; }
            public byte IPVersion { get; set; }

            public UInt32[] IPResult { get; set; }

            public IPAddress[] ExtIpAddress { get; set; }



            public byte[] GetBytes()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        WriteDataTo(writer);
                    }
                    return stream.ToArray();
                }
            }

            public void WriteDataTo(BinaryWriter _Writer)
            {
                _Writer.Write(ResultCode);
                _Writer.Write(IPVersion);
                if (IPVersion == 0)
                    _Writer.Write(ByteStreamCovert.ReverseBytes(IPResult[0]));
                else
                {
                    _Writer.Write(ByteStreamCovert.ReverseBytes(IPResult[0]));
                    _Writer.Write(ByteStreamCovert.ReverseBytes(IPResult[1]));
                    _Writer.Write(ByteStreamCovert.ReverseBytes(IPResult[2]));
                    _Writer.Write(ByteStreamCovert.ReverseBytes(IPResult[3]));
                }
            }

            public void SetDataFrom(BinaryReader _Reader)
            {
                ResultCode = _Reader.ReadByte();
                IPVersion = _Reader.ReadByte();
                if (IPVersion == 0)
                {
                    IPResult = new UInt32[] { 0 };
                    IPResult[0] = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                    ExtIpAddress = new IPAddress[]{IPAddress.None};
                    ExtIpAddress[0] = new IPAddress(new byte[] { 
                                (byte)((IPResult[0]>>24) & 0xFF) ,
                                (byte)((IPResult[0]>>16) & 0xFF) , 
                                (byte)((IPResult[0]>>8)  & 0xFF) , 
                                (byte)( IPResult[0] & 0xFF)});
                }
                else
                {
                    IPResult = new UInt32[] { 0,0,0,0 };
                    ExtIpAddress = new IPAddress[] { IPAddress.None, IPAddress.None, IPAddress.None, IPAddress.None };
                    IPResult[0] = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                    IPResult[1] = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                    IPResult[2] = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                    IPResult[3] = ByteStreamCovert.ReverseBytes(_Reader.ReadUInt32());
                }
                    
            }
        }
    }
}