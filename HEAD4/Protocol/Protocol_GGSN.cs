using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AlexPilotti.FTPS.Client;
using AlexPilotti.FTPS.Common;
using Common;
using Common.Utility;
using Protocol.GG_SN;

namespace Protocol
{
    public enum OpCode : byte
    {
        [Description("알수없음")]        UNKNOWN = 0,
        [Description("인증요청")]        REQ_AUTH = 0x01,//!< 02 대기
        [Description("인증 제출")]        RESP_AUTH = 0x02,//!< 83 대기
        [Description("접근허가")]        PERM_AUTH_DA = 0x83,
        [Description("통신환경 정보전달")]        INFO_COMM_ENV_DA = 0x84,
        [Description("동기화 정보")]        INFO_SYNC_DA = 0x85,
        [Description("파일 처리 지시")]        REQ_FILE_OP_DA = 0x86,
        [Description("파일처리 결과")]        REPORT_FILE_OP_DA = 0x87,
        [Description("상태보고 시작 요청")]        REQ_STATUS_START = 0x08,//!< 09 대기
        [Description("상태보고 시작 응답")]        RESP_STATUS_START = 0x09,
        [Description("주기적 상태보고")]        PERIODIC_STATUS_DA = 0x8A,
        [Description("안내 정보 시작 요청")]        REQ_BISINFO_START_DA = 0x8B,
        [Description("안내 정보 시작 응답")]        RESP_BISINFO_START_DA = 0x8C,
        [Description("안내정보")]        BIS_INFO_DA = 0x8D,
        [Description("제어 지시")]        REQ_CONTROL_DA = 0x8E,
        [Description("Reset 지시")]        REQ_RESET_DA = 0x8F,
        [Description("상태 요청")]        REQ_STATUS = 0x10,//!< 11대기
        [Description("상태 응답")]        RESP_STATUS = 0x11,
        [Description("표출 언어 설정")]        SET_LANG_DA = 0x92,
        [Description("파일 서버 지정")]        SET_FILESERV_DA = 0x93,
        [Description("제공 서버 정보 요청")]        REQ_PROVIDER_INFO = 0x14,//!< 15 대기
        [Description("제공 서버 정보 응답")]        RESP_PPOVIDER_INFO = 0x15,
        [Description("제공 서버 정보 지정")]        SET_PROVIDER_INFO_DA = 0x96,
        [Description("동작 환경 온도 요청")]        REQ_TEMPERATURE = 0x17,//!< 18 대기
        [Description("동작 환경 온도 응답")]        RESP_TEMPERATURE = 0x18,
        [Description("동작 환경 온도 지정")]        SET_TEMPERATURE_DA = 0x99,
        [Description("LCD(LED) On/Off 일정 요청")]        REQ_DISP_SCHEDULE = 0x1A,//!< 1B 대기
        [Description("LCD(LED) On/Off 일정 응답")]        RESP_DISP_SCHEDULE = 0x1B,
        [Description("LCD(LED) On/Off 일정 지정")]        SET_DISP_SCHEDULE_DA = 0x9C,
        [Description("차량 위치 요청")]        REQ_VEH_LOC_DA = 0x9D,
        [Description("차량 위치 응답")]        RESP_VEH_LOC_DA = 0x9E,
        [Description("표출화면 Capture 시작 요청")]        REQ_SCRCAP_START_DA = 0x9F,
        [Description("표출화면 Capture 시작 응답")]        RESP_SCRCAP_START_DA = 0xA0,
        [Description("표출화면 Capture 중지 요청")]        REQ_SCRCAP_STOP_DA = 0xA1,
        [Description("표출화면 Capture 보고")]        REPORT_SCRCAP_DA = 0xA2,
        [Description("Sound Mixer 조회 요청")]        REQ_SNDMXR = 0x23,//!< 24 대기
        [Description("Sound Mixer 조회 응답")]        RESP_SNDMXR = 0x24,
        [Description("Sound Mixer 설정")]        SET_SNDMXR_DA = 0xA5,
        [Description("단문 전송")]        SHORT_MSG_DA = 0xC0,
        [Description("카메라 제어 요청")]        REQ_CAM_CONTROL_DA = 0xC1,
        [Description("카메라 제어 응답")]        RESP_CAM_CONTROL_DA = 0xC2,
        [Description("카메라 이미지 보고")]        REPORT_CAM_IMAGE_DA = 0xC3,
        [Description("충격 기록 요청")]        REQ_SHK_RECORD_DA = 0xD0,
        [Description("충격 기록 응답")]        RESP_SHK_RECORD_DA = 0xD1,
        [Description("시정 홍보 일괄 삭제 요청")] REQ_CLEAR_PROMO_DA = 0xE1,
        [Description("공인 IP 요청")]        REQ_PUB_IP_DA = 0xE2,
        [Description("공인 IP 응답")]        RESP_PUB_IP_DA = 0xE3,
        [Description("Delivery ACK")]        DELIVERY_ACK_DA = 0x7F,
    }


    public class Protocol_GGSN : IProtocol
    {
        private BitSocketClient _client;
        private ushort cSeq = 0;
        private DateTime lastRecv;
        private MemoryStream _instrm = new MemoryStream();
        private ConcurrentQueue<byte[]> ccBytesQueue = new ConcurrentQueue<byte[]>();
        private ManualResetEvent sig_;
        private volatile bool _tokill;
        private Thread listenThread = null;
        private Info_CommEnv commEnv = new Info_CommEnv();
        public event EventHandler<string> SocketActionHandler;
        public delegate void InfoHandler(List<PredictInfo> _infos);
        public event InfoHandler InfoArrived;
        private byte[] listFileInfo = null;
        private byte nFileInfo = 0;
        private List<Route> listRoute = new List<Route>();
        private List<Station> listStation = new List<Station>();
        private List<RouteStation> listRouteStation = new List<RouteStation>();
        private List<RouteAllocation> listRouteAlloc = new List<RouteAllocation>(); 
        private List<PredictInfo> listPredictInfo = new List<PredictInfo>();
        private BITConfig ConfigBIT = new BITConfig();
        private ConcurrentQueue<FTPTransfer> ftpQueue = new ConcurrentQueue<FTPTransfer>();
        private IPAddress pubIP = null;



        public void ForceReConnect()
        {
            this._client.AutoReconnect = false;
            this._client.Disconnect();

            while (this._client.ConnectionState == BitSocketClient.ConnectionStatus.Connected
                   || this._client.ConnectionState == BitSocketClient.ConnectionStatus.Connecting)
            {
                Thread.Sleep(100);    
            }
            
            this._client.Connect();
            this._client.AutoReconnect = true;

        }


        public void Connect()
        {
            if (listenThread != null && listenThread.IsAlive)
            {
                _client.Disconnect();
                //_client.Connect();
                return;
            }

            XmlSerializer SerializerObj = new XmlSerializer(typeof(BITConfig));
            FileStream configStream = new FileStream(@"bitconfig.xml", FileMode.Open, FileAccess.Read, FileShare.Read);//InfoConfig.xml
            ConfigBIT = (BITConfig)SerializerObj.Deserialize(configStream);
            configStream.Close();

            LoadRoute(ConfigBIT.BasePath + @"/DB/Master/route.csv");
            LoadStation(ConfigBIT.BasePath + @"/DB/Master/station.csv");
            LoadRouteStation(ConfigBIT.BasePath + @"/DB/Master/route_station.csv");
            LoadRouteAllocation(ConfigBIT.BasePath + @"/DB/Master/route_allocaplan.csv");
            /*
            FTPTransfer toTrans = new FTPTransfer();
            toTrans.remoteFile = @"/BIS/DOWN/SNR/20151016";
            toTrans.localFile = @"../../DB/SNRDB/20151016";
            ftpQueue.Enqueue(toTrans);

            toTrans = new FTPTransfer();
            toTrans.remoteFile = @"/BIS/DOWN/WEATHER/Weather.wdb";
            toTrans.localFile = @"../../DB/DB/Weather.wdb";
            ftpQueue.Enqueue(toTrans);

            toTrans = new FTPTransfer();
            toTrans.remoteFile = @"/BIS/DOWN/NEWS/News.ndb";
            toTrans.localFile = @"../../DB/DB/News.ndb";
            ftpQueue.Enqueue(toTrans);

            StartFTPTask();

            Common.Utility.ToolSnippet.CsvUnescapeSplit(@"../../DB/Master/route.csv");
            */
            _tokill = false;
            lastRecv = DateTime.Now;
            _client = new BitSocketClient(IPAddress.Parse("175.214.78.38"), 6006, true);
            //_client = new BitSocketClient(ExternalIP.Parse("211.236.104.101"), 6100, true);

            _client.DataReceived += new BitSocketClient.delDataReceived(client_DataReceived);
            _client.ConnectionStatusChanged += new BitSocketClient.delConnectionStatusChanged(OnConnectionStatusChanged);
            _client.DataSended += delegate(BitSocketClient sender, byte[] data)
            {
                if (SocketActionHandler != null)
                    SocketActionHandler.BeginInvoke(this, "Send >>> " + BitConverter.ToString(data), null, null);
            };
            _client.Connect();

            listenThread = new Thread(new ThreadStart(Run));
            listenThread.Start();
        }


        public void LoadRoute(string path)
        {
            listRoute.Clear();
            string[] seps = { "\",", ",\"" };
            char[] quotes = { '\"', ' ' };
            foreach (var line in File.ReadAllLines(path, Encoding.UTF8).Skip(1))
            {
                var fields = line
                    .Split(seps, StringSplitOptions.None)
                    .Select(s => s.Trim(quotes).Replace("\\\"", "\""))
                    .ToArray();
                if (fields.Count() == 9)
                {
                    Route item = new Route();
                    item.routeID = fields[0];
                    item.routeName = fields[1];
                    item.routeType = fields[2];
                    item.startStationID = fields[3];
                    item.endStationID = fields[4];
                    item.companyName = fields[5];
                    item.phoneNumber = fields[6];
                    item.firstTime = fields[7];
                    item.lastTime = fields[8];
                    listRoute.Add(item);
                }
                //Console.Write("{0} | ", field);
                //Console.WriteLine();
            }
        }

        public void LoadStation(string path)
        {
            listStation.Clear();
            string[] seps = { "\",", ",\"" };
            char[] quotes = { '\"', ' ' };
            foreach (var line in File.ReadAllLines(path, Encoding.UTF8).Skip(1))
            {
                var fields = line
                    .Split(seps, StringSplitOptions.None)
                    .Select(s => s.Trim(quotes).Replace("\\\"", "\""))
                    .ToArray();
                if (fields.Count() == 6)
                {
                    Station item = new Station();
                    item.stationID = fields[0];
                    item.stationName = fields[1];
                    item.mobileNo = fields[2];
                    item.engName = fields[3];
                    item.chnName = fields[4];
                    item.jpnName = fields[5];
                    listStation.Add(item);
                }
            }
        }

        public void LoadRouteStation(string path)
        {
            listRouteStation.Clear();
            string[] seps = { "\",", ",\"" };
            char[] quotes = { '\"', ' ' };
            foreach (var line in File.ReadAllLines(path, Encoding.UTF8).Skip(1))
            {
                var fields = line
                    .Split(seps, StringSplitOptions.None)
                    .Select(s => s.Trim(quotes).Replace("\\\"", "\""))
                    .ToArray();
                if (fields.Count() == 3)
                {
                    RouteStation item = new RouteStation();
                    item.routeID = fields[0];
                    item.stationOrder = fields[1];
                    item.stationID = fields[2];
                    listRouteStation.Add(item);
                }
            }
        }

        public void LoadRouteAllocation(string path)
        {
            listRouteAlloc.Clear();
            string[] seps = { "\",", ",\"" };
            char[] quotes = { '\"', ' ' };
            foreach (var line in File.ReadAllLines(path, Encoding.UTF8).Skip(1))
            {
                var fields = line
                    .Split(seps, StringSplitOptions.None)
                    .Select(s => s.Trim(quotes).Replace("\\\"", "\""))
                    .ToArray();
                if (fields.Count() == 8)
                {
                    RouteAllocation item = new RouteAllocation();
                    item.period_id = fields[0];
                    item.dow_tp = fields[1];
                    item.st_date = fields[2];
                    item.ed_date = fields[3];
                    item.routeID = fields[4];
                    item.stationID = fields[5];
                    item.dep_time = fields[6];
                    item.dep_bstop_tp = fields[7];
                    listRouteAlloc.Add(item);
                }
            }
        }

        private void RespPublicIP(DataFrame df)
        {
            new Action(async () =>
            {
                await Task.Run(() =>
                {
                    try
                    {
                        byte[] _payload = null;
                        pubIP = Common.Utility.ToolSnippet.GetExternalIpDyndns();
                        Resp_External_IP resp = new Resp_External_IP();
                        resp.ResultCode = 0;
                        resp.IPVersion = 0;
                        resp.IPResult = new UInt32[] { (UInt32)BitConverter.ToInt32(IPAddress.Parse(pubIP.ToString()).GetAddressBytes(), 0) };
                        df.DataLength = (ushort)resp.GetBytes().Length;
                        df.OpCode = (byte)OpCode.RESP_PUB_IP_DA;
                        _payload = new Byte[5 + df.DataLength];
                        Array.Copy(df.GetBytes(), 0, _payload, 0, 5);
                        Array.Copy(resp.GetBytes(), 0, _payload, 5, df.DataLength);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                });
            }).Invoke();
        }

        private void StartFTPTask()
        {
            if (ftpQueue.Count > 0)
            {
                new Action(async () =>
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            using (FTPSClient _ftp = new FTPSClient())
                            {
                                NetworkCredential credential = new NetworkCredential("bit", @"!@#123qwe");
                                X509Certificate x509ClientCert = null;
                                _ftp.Connect(@"175.214.78.38", 2121,
                                    credential,
                                    ESSLSupportMode.DataChannelRequested,
                                    new RemoteCertificateValidationCallback(ValidateTestServerCertificate),
                                    x509ClientCert,
                                    0,
                                    0,
                                    0,
                                    120000,
                                    true,
                                    EDataConnectionMode.Active);
                                _ftp.SetTransferMode(ETransferMode.Binary);
                                FTPTransfer item = new FTPTransfer();
                                if (ftpQueue.TryPeek(out item))
                                {
                                    _ftp.GetFile(item.remoteFile, item.localFile,
                                        new FileTransferCallback(TransferCallback));
                                }

                                _ftp.Close();
                                //Thread.Sleep(5000);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    });
                }).Invoke();
            }
            
        }
        private static bool ValidateTestServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool certOk = false;

            if (sslPolicyErrors == SslPolicyErrors.None)
                certOk = true;
            else
            {
                Console.Error.WriteLine();

                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) > 0)
                    Console.Error.WriteLine("WARNING: SSL/TLS remote certificate chain errors");

                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) > 0)
                    Console.Error.WriteLine("WARNING: SSL/TLS remote certificate name mismatch");

                if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) > 0)
                    Console.Error.WriteLine("WARNING: SSL/TLS remote certificate not available");

                //if (options.sslInvalidServerCertHandling == EInvalidSslCertificateHandling.Accept)
                certOk = true;
            }
            return certOk;
        }

        private void TransferCallback(FTPSClient sender, ETransferActions action, string localObjectName, string remoteObjectName, ulong fileTransmittedBytes, ulong? fileTransferSize, ref bool cancel)
        {
            switch (action)
            {
                case ETransferActions.FileDownloaded:
                case ETransferActions.FileUploaded:
                    //OnFileTransferCompleted(fileTransmittedBytes, fileTransferSize);
                    {
                        try
                        {
                            Debug.WriteLine("File Download Completed - " + remoteObjectName + ": " + DateTime.Now);
                            FTPTransfer item = new FTPTransfer();
                            ftpQueue.TryDequeue(out item);
                            StartFTPTask();
                            FileInfo f = new FileInfo(localObjectName);
                            long s1 = f.Length;
                            /*
                            using (BinaryReader b = new BinaryReader(File.Open(localObjectName, FileMode.Open)))
                            {
                                if ((s1 - 5) % 35 == 0)
                                {
                                    Scenario_DataPlain fileData = new Scenario_DataPlain();
                                    fileData.SetDataFrom(b);
                                }
                                else
                                {
                                    Scenario_DataSchedule fileData = new Scenario_DataSchedule();
                                    fileData.SetDataFrom(b);
                                }

                            }
                            */
                        }
                        catch (Exception ex)
                        {

                            throw;
                        }

                    }
                    break;
                case ETransferActions.FileDownloadingStatus:
                case ETransferActions.FileUploadingStatus:
                    //OnFileTransferStatus(action, localObjectName, remoteObjectName, fileTransmittedBytes, fileTransferSize);
                    break;
                case ETransferActions.RemoteDirectoryCreated:
                    break;
                case ETransferActions.LocalDirectoryCreated:
                    break;
            }
        }

        public void Close()
        {
            _tokill = true;
            listenThread.Join();
        }

        private void ListFileInfo()
        {

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    string[] masterFiles = Directory.GetFiles(ConfigBIT.BasePath + @"/DB/Master");
                    string masterFile = masterFiles.Single(s => s.EndsWith(@".zip"));
                    MasterFileInfo master = new MasterFileInfo();
                    master.fileName = Path.GetFileName(masterFile);
                    master.fileNameLen = (byte)master.fileName.Length;
                    master.fileType = 0;
                    master.idMode = 0;
                    FileInfo masterinfo = new FileInfo(masterFile);
                    master.fileTime = (UInt32)((masterinfo.CreationTimeUtc - new DateTime(1970, 1, 1)).TotalSeconds);//zip파일의 날짜 정보

                    writer.Write(master.GetBytes(), 0, master.GetBytes().Length);
                    nFileInfo++;

                    string[] scFiles = Directory.GetFiles(ConfigBIT.BasePath + @"/DB/SNRDB");
                    string scenarioFile = scFiles[0];
                    SimpleFileInfo scenario = new SimpleFileInfo();
                    scenario.fileName = Path.GetFileName(scenarioFile);
                    scenario.fileNameLen = (byte)Encoding.UTF8.GetBytes(scenario.fileName).Length;
                    scenario.fileType = FileType.AD_SCENARIO;
                    FileInfo scinfo = new FileInfo(scenarioFile);
                    scenario.fileTime = (UInt32)((scinfo.CreationTimeUtc - new DateTime(1970, 1, 1)).TotalSeconds);//zip파일의 날짜 정보
                    writer.Write(scenario.GetBytes(), 0, scenario.GetBytes().Length);
                    nFileInfo++;

                    //!< 시정홍보 Contents, List SNR 파일, exclude SNR.DAT
                    string[] contents = Directory.GetFiles(ConfigBIT.BasePath + @"/DB/SNR");
                    foreach (var file in contents)
                    {
                        //if (!file.Contains(@"SNR.DAT"))
                        {
                            GG_SN.SimpleFileInfo contentFileInfo = new GG_SN.SimpleFileInfo();
                            contentFileInfo.fileName = Path.GetFileName(file);
                            contentFileInfo.fileNameLen = (byte) Encoding.UTF8.GetBytes(contentFileInfo.fileName).Length;
                            contentFileInfo.fileType = FileType.AD_CONTENTS;
                            FileInfo finfo = new FileInfo(file);
                            contentFileInfo.fileTime = (UInt32)((finfo.CreationTimeUtc - new DateTime(1970, 1, 1)).TotalSeconds);//zip파일의 날짜 정보
                            writer.Write(contentFileInfo.GetBytes(), 0, contentFileInfo.GetBytes().Length);
                            nFileInfo++;
                        }
                    }

                    //!< 뉴스등 기가
                    string[] dbFiles = Directory.GetFiles(ConfigBIT.BasePath + @"/DB/DB");
                    foreach (var file in dbFiles)
                    {
                        //if (!file.Contains(@"SNR.DAT"))
                        {
                            GG_SN.SimpleFileInfo contentFileInfo = new GG_SN.SimpleFileInfo();
                            contentFileInfo.fileName = Path.GetFileName(file);
                            contentFileInfo.fileNameLen = (byte)Encoding.UTF8.GetBytes(contentFileInfo.fileName).Length;
                            //!< 이름에 따라 파일 유형 잡아주기
                            if (file.Contains(@"News"))
                                contentFileInfo.fileType = FileType.NEWS;
                            else if (file.Contains(@"Weather"))
                                contentFileInfo.fileType = FileType.WEATHER;
                            FileInfo dbinfo = new FileInfo(file);
                            contentFileInfo.fileTime = (UInt32)((dbinfo.CreationTimeUtc - new DateTime(1970, 1, 1)).TotalSeconds);//zip파일의 날짜 정보
                            writer.Write(contentFileInfo.GetBytes(), 0, contentFileInfo.GetBytes().Length);
                            nFileInfo++;
                        }
                    }
                }
                listFileInfo = stream.ToArray();
            }

        }

        private void Run()
        {
            
            nFileInfo = 0;
            ListFileInfo();
            while (!_tokill)
            {
                HandleProtocol();
                Thread.Sleep(10);
            }
        }

        #region --- Network Packet ---
        //!< Authen Req<, Rep>, Access<, Arg indicate<
        //!< Data Sync>, ACK<, 
        //!< {May Omit} File Indicat<, ACK >, File Resp >, ACK <
        //!< {Periodic, May Omit}Status Report Req <, Status Report Rep >
        //!< Service Invoke >, Ack< Service Init <, Ack>, Service Sub <, Ack > 
        void OnConnectionStatusChanged(BitSocketClient sender, BitSocketClient.ConnectionStatus status)
        {

            //if (status != BitSocketClient.ConnectionStatus.Connected)
            {
                if (SocketActionHandler != null) 
                    SocketActionHandler.BeginInvoke(this, status.ToString(), null, null);
            }
            Console.WriteLine("Connection Status Changed : " + status.ToString());
        }

        private void client_DataReceived(BitSocketClient sender, byte[] data)
        {
            if (SocketActionHandler != null)
                SocketActionHandler.BeginInvoke(this, "Recv <<< " + BitConverter.ToString(data), null, null);
            lastRecv = DateTime.Now;
            ccBytesQueue.Enqueue(data);
        }

        private void FillStream(long pos)
        {
            byte[] dequeued = null;
            _instrm.Position = pos;
            if (ccBytesQueue.TryDequeue(out dequeued))
            {

                MemoryStream tmpStrm = new MemoryStream();
                if (_instrm.Capacity > 0)
                {
                    Int32 oldSize = _instrm.Capacity - (Int32)pos;
                    _instrm.Position = pos;
                    _instrm.CopyTo(tmpStrm);
                    tmpStrm.Write(dequeued, 0, dequeued.Length);
                    tmpStrm.Position = 0;

                    _instrm.SetLength(0);
                    _instrm.Position = 0;
                    _instrm.Write(tmpStrm.ToArray(), 0, (int)tmpStrm.Length);
                    _instrm.Position = 0;
                    _instrm.Flush();

                }
                else
                {
                    _instrm.Capacity = dequeued.Length;
                    _instrm.Write(dequeued, 0, dequeued.Length);
                    _instrm.Seek(0, SeekOrigin.Begin);
                }
            }
            else
            {
                int a = 1;
            }
        }
        #endregion

        public void OnInfoChanged(EventArgs e)
        {
            InfoHandler handler = InfoArrived;
            if (handler != null)
            {
                listPredictInfo.Sort((x, y) => x.nremTime.CompareTo(y.nremTime));
                lock (listPredictInfo)
                {
                    handler(listPredictInfo);
                }
                
            }
            //handler(this, e);
        }
        private void HandleProtocol()
        {
            int nRead = 0;
            byte[] byteRecv = new byte[5];
            bool bReady = false;
            long rewindPos = 0;
            DateTime now = DateTime.Now;
            TimeSpan netelapsed = now.Subtract(lastRecv);
            lock (_instrm)
            {
                if (_instrm.Capacity < 5)
                {
                    FillStream(_instrm.Position);
                    return;
                }
                rewindPos = _instrm.Position;
                nRead = _instrm.Read(byteRecv, 0, 5);

                if (nRead == 5)
                {
                    if (Enum.IsDefined(typeof (OpCode), byteRecv[4]))
                    {
                        DataFrame df = new DataFrame();
                        using (BinaryReader br = new BinaryReader(new MemoryStream()))
                        {
                            br.BaseStream.Write(byteRecv, 0, 5);
                            br.BaseStream.Seek(0, SeekOrigin.Begin);
                            df.SetDataFrom(br);
                        }

                        OpCode opCode = (OpCode) Enum.Parse(typeof (OpCode), byteRecv[4].ToString());
                        ushort senderseq = Convert.ToUInt16((16 * (int)byteRecv[0]) + (int)byteRecv[1]);
                        ushort bodyLen = Convert.ToUInt16((16 * (int)byteRecv[2]) + (int)byteRecv[3]);
                        byte[] byteBody = null;
                        if (bodyLen > 0)//!< 1개라도 있으면은 > 0이다 기존은 > 1
                        {
                            byteBody = new byte[bodyLen];

                            if (_instrm.Read(byteBody, 0, bodyLen) != bodyLen)
                            {
                                FillStream(rewindPos);
                                return;
                            }
                        }
                        else
                        {
                            //FillStream(rewindPos+5);
                        }
                        Debug.Write(BitConverter.ToString(byteRecv));
                        if (byteBody != null)
                        {
                            Debug.Write("-");
                            Debug.WriteLine(BitConverter.ToString(byteBody));
                        }
                        switch (opCode)
                        {
                            case OpCode.INFO_COMM_ENV_DA:
                                {
                                    try
                                    {
                                        using (BinaryReader br = new BinaryReader(new MemoryStream()))
                                        {
                                            br.BaseStream.Write(byteBody, 0, byteBody.Length);
                                            br.BaseStream.Seek(0, SeekOrigin.Begin);
                                            commEnv.SetDataFrom(br);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        if (SocketActionHandler != null)
                                            SocketActionHandler.BeginInvoke(this, "Exception Recv <<< " + ex.StackTrace, null, null);
                                        //throw;
                                    }

                                }
                                _client.Send(GetDeliveryACK(senderseq));
                                _client.Send(MakeSync());
                                _client.Send(Request(OpCode.REQ_BISINFO_START_DA));
                                break;
                            case OpCode.REQ_FILE_OP_DA:
                                {
                                    _client.Send(GetDeliveryACK(senderseq));
                                    try
                                    {
                                        if (byteBody[1] == 0)
                                        {
                                            using (BinaryReader br = new BinaryReader(new MemoryStream()))
                                            {
                                                br.BaseStream.Write(byteBody, 1, byteBody.Length-1);
                                                br.BaseStream.Seek(0, SeekOrigin.Begin);
                                                MasterFileInfo mInfo = new MasterFileInfo();
                                                mInfo.SetDataFrom(br);
                                                _client.Send(MakeFileOpRespons(byteBody[0], mInfo.GetBytes()));
                                            }
                                        }
                                        else
                                        {
                                            using (BinaryReader br = new BinaryReader(new MemoryStream()))
                                            {
                                                br.BaseStream.Write(byteBody, 1, byteBody.Length-1);
                                                br.BaseStream.Seek(0, SeekOrigin.Begin);
                                                SimpleFileInfo sInfo = new SimpleFileInfo();
                                                sInfo.SetDataFrom(br);
                                                _client.Send(MakeFileOpRespons(byteBody[0], sInfo.GetBytes()));
                                            }
                                        }
                                        
                                    }
                                    catch (Exception ex)
                                    {
                                        if (SocketActionHandler != null)
                                            SocketActionHandler.BeginInvoke(this, "Exception Recv <<< " + ex.StackTrace, null, null);
                                        //throw;
                                    }
                                    
                                }
                                break;
                            case OpCode.RESP_BISINFO_START_DA:
                                {
                                    try
                                    {
                                        listPredictInfo.Clear();
                                        using (BinaryReader br = new BinaryReader(new MemoryStream()))
                                        {
                                            br.BaseStream.Write(byteBody, 0, byteBody.Length);
                                            br.BaseStream.Seek(0, SeekOrigin.Begin);
                                            BIS_Info predInfo = new BIS_Info();
                                            predInfo.SetDataFrom(br);

                                            foreach (RouteUnit item in predInfo.rInfo)
                                            {
                                                if (item.routestatus == 2)
                                                {
                                                    foreach (var unit in item.bInfo.bInfo)
                                                    {
                                                        PredictInfo inforow = new PredictInfo();
                                                        inforow.routeName =
                                                            listRoute.Single(s => s.routeID == Encoding.UTF8.GetString(item.route_id)).routeName;
                                                        inforow.startStationName = listStation.Single(s => s.stationID == listRoute.Single(r => r.routeID == Encoding.UTF8.GetString(item.route_id)).startStationID).stationName;
                                                        inforow.stationName = listStation.Single(s => s.stationID == Encoding.UTF8.GetString(unit.locid)).stationName;
                                                        
                                                        inforow.nremTime = unit.remaintime;
                                                        inforow.nremStation = unit.remainstop;
                                                        inforow.remainStation = unit.remainstop.ToString();
                                                        inforow.remainTime = unit.remaintime.ToString();
                                                        inforow.recvTime = DateTime.Now.ToLocalTime();
                                                        inforow.busid = Encoding.UTF8.GetString(unit.id);

                                                        if ((int) (unit.busattrib & 64) >= 64)
                                                        {
                                                            inforow.runType = 1;
                                                        }
                                                        else if ((int) (unit.busattrib & 128) >= 128)
                                                        {
                                                            inforow.runType = 0;
                                                        }
                                                        else
                                                        {
                                                            inforow.runType = 2;
                                                        }
                                                       

                                                        if ((int) (unit.busattrib & 32) >= 32)
                                                            inforow.busType = @"저상";
                                                        else
                                                        {
                                                            Route rType =
                                                                listRoute.Single(
                                                                    s =>
                                                                        s.routeID ==
                                                                        Encoding.UTF8.GetString(item.route_id));
                                                            switch (Int32.Parse(rType.routeType))
                                                            {
                                                                case 11:
                                                                case 12:
                                                                case 21:
                                                                case 22:
                                                                    inforow.busType = @"좌석";
                                                                    break;
                                                                case 13:
                                                                case 23:
                                                                case 43:
                                                                    inforow.busType = @"일반";
                                                                    break;
                                                                case 30:
                                                                    inforow.busType = @"마을";
                                                                    break;
                                                                case 41:
                                                                case 42:
                                                                    inforow.busType = @"광역";
                                                                    break;
                                                                default:
                                                                    inforow.busType = @"일반";
                                                                    break;
                                                            }
                                                        }

                                                        if (inforow.nremTime <= 1 ||
                                                            (unit.remainstop == 1 && unit.remaintime <= 2))
                                                            inforow.bApproach = true;
                                                        else
                                                        {
                                                            inforow.bApproach = false;
                                                        }
                                                        if ((unit.remaintime == 65535 || unit.remainstop == 65535)
                                                            || Encoding.UTF8.GetString(unit.locid) == ConfigBIT.ID)
                                                        {
                                                            listPredictInfo.RemoveAll(t => t.busid == Encoding.UTF8.GetString(unit.id));
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("======={0} {1}번 {2}분 {3}전 {4}", inforow.busType, inforow.routeName,
                                                                inforow.remainTime, inforow.remainStation, inforow.stationName);
                                                            listPredictInfo.RemoveAll(t => t.routeID == inforow.routeID && t.busid == inforow.busid);
                                                            listPredictInfo.Add(inforow);
                                                        }
                                                    }

                                                }
                                                else
                                                {
                                                    if (item.routestatus == 0 || item.routestatus == 1 ||
                                                        item.routestatus == 3)
                                                    {
                                                        listPredictInfo.RemoveAll(t => t.routeID == Encoding.UTF8.GetString(item.route_id));
                                                    }
                                                }
                                            }
                                            OnInfoChanged(EventArgs.Empty);
                                            /*
                                            if (InfoArrived != null)
                                            {
                                                InfoArrived(listPredictInfo);
                                            }
                                            */
                                            //commEnv.SetDataFrom(br);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        if (SocketActionHandler != null)
                                            SocketActionHandler.BeginInvoke(this, "Exception Recv <<< " + ex.StackTrace, null, null);
                                        //throw;
                                    }

                                }
                                _client.Send(GetDeliveryACK(senderseq));
                                break;
                            case OpCode.BIS_INFO_DA:
                                {
                                    try
                                    {
                                        using (BinaryReader br = new BinaryReader(new MemoryStream()))
                                        {
                                            br.BaseStream.Write(byteBody, 0, byteBody.Length);
                                            br.BaseStream.Seek(0, SeekOrigin.Begin);
                                            BIS_Info predInfo = new BIS_Info();
                                            predInfo.SetDataFrom(br);

                                            foreach (RouteUnit item in predInfo.rInfo)
                                            {
                                                if (item.routestatus == 2)
                                                {
                                                    foreach (var unit in item.bInfo.bInfo)
                                                    {
                                                        PredictInfo inforow = new PredictInfo();
                                                        inforow.routeName =
                                                            listRoute.Single(s => s.routeID == Encoding.UTF8.GetString(item.route_id)).routeName;
                                                        inforow.startStationName = listStation.Single(s => s.stationID == listRoute.Single(r => r.routeID == Encoding.UTF8.GetString(item.route_id)).startStationID).stationName;
                                                        inforow.stationName = listStation.Single(s => s.stationID == Encoding.UTF8.GetString(unit.locid)).stationName;

                                                        inforow.nremTime = unit.remaintime;
                                                        inforow.nremStation = unit.remainstop;
                                                        inforow.remainStation = unit.remainstop.ToString();
                                                        inforow.remainTime = unit.remaintime.ToString();
                                                        inforow.recvTime = DateTime.Now.ToLocalTime();
                                                        inforow.busid = Encoding.UTF8.GetString(unit.id);

                                                        if ((int)(unit.busattrib & 64) >= 64)
                                                        {
                                                            inforow.runType = 1;
                                                        }
                                                        else if ((int)(unit.busattrib & 128) >= 128)
                                                        {
                                                            inforow.runType = 0;
                                                        }
                                                        else
                                                        {
                                                            inforow.runType = 2;
                                                        }


                                                        if ((int)(unit.busattrib & 32) >= 32)
                                                            inforow.busType = @"저상";
                                                        else
                                                        {
                                                            Route rType =
                                                                listRoute.Single(
                                                                    s =>
                                                                        s.routeID ==
                                                                        Encoding.UTF8.GetString(item.route_id));
                                                            switch (Int32.Parse(rType.routeType))
                                                            {
                                                                case 11:
                                                                case 12:
                                                                case 21:
                                                                case 22:
                                                                    inforow.busType = @"좌석";
                                                                    break;
                                                                case 13:
                                                                case 23:
                                                                case 43:
                                                                    inforow.busType = @"일반";
                                                                    break;
                                                                case 30:
                                                                    inforow.busType = @"마을";
                                                                    break;
                                                                case 41:
                                                                case 42:
                                                                    inforow.busType = @"광역";
                                                                    break;
                                                                default:
                                                                    inforow.busType = @"일반";
                                                                    break;
                                                            }
                                                        }

                                                        if (inforow.nremTime <= 1 ||
                                                            (unit.remainstop == 1 && unit.remaintime <= 2))
                                                            inforow.bApproach = true;
                                                        else
                                                        {
                                                            inforow.bApproach = false;
                                                        }
                                                        if ((unit.remaintime == 65535 || unit.remainstop == 65535)
                                                            || Encoding.UTF8.GetString(unit.locid) == ConfigBIT.ID)
                                                        {
                                                            listPredictInfo.RemoveAll(t => t.busid == Encoding.UTF8.GetString(unit.id));
                                                        }
                                                        else
                                                        {
                                                            Console.WriteLine("======={0} {1}번 {2}분 {3}전 {4}", inforow.busType, inforow.routeName,
                                                                inforow.remainTime, inforow.remainStation, inforow.stationName);
                                                            listPredictInfo.RemoveAll(t => t.routeID == inforow.routeID && t.busid == inforow.busid);
                                                            listPredictInfo.Add(inforow);
                                                        }
                                                    }

                                                }
                                                else
                                                {
                                                    if (item.routestatus == 0 || item.routestatus == 1 ||
                                                        item.routestatus == 3)
                                                    {
                                                        listPredictInfo.RemoveAll(t => t.routeID == Encoding.UTF8.GetString(item.route_id));
                                                    }
                                                }
                                            }
                                            OnInfoChanged(EventArgs.Empty);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        if (SocketActionHandler != null)
                                            SocketActionHandler.BeginInvoke(this, "Exception Recv <<< " + ex.StackTrace, null, null);
                                        //throw;
                                    }

                                }
                                _client.Send(GetDeliveryACK(senderseq));
                                break;
                            case OpCode.REQ_CONTROL_DA:
                            case OpCode.REQ_RESET_DA:
                            case OpCode.SET_LANG_DA:
                            case OpCode.SET_FILESERV_DA:
                            case OpCode.SET_PROVIDER_INFO_DA:
                            case OpCode.SET_TEMPERATURE_DA:
                            case OpCode.SET_DISP_SCHEDULE_DA:
                            case OpCode.RESP_VEH_LOC_DA:
                            case OpCode.REQ_SCRCAP_START_DA:
                            case OpCode.REQ_SCRCAP_STOP_DA:
                            case OpCode.SET_SNDMXR_DA:
                            case OpCode.SHORT_MSG_DA:
                            case OpCode.REQ_CAM_CONTROL_DA:
                            case OpCode.REQ_SHK_RECORD_DA:
                            case OpCode.REQ_CLEAR_PROMO_DA:
                                _client.Send(GetDeliveryACK(senderseq));
                                break;
                            case OpCode.REQ_PUB_IP_DA:
                                _client.Send(GetDeliveryACK(senderseq));
                            {
                                DataFrame respdf = new DataFrame();
                                respdf.DataLength = 0;
                                respdf.Seq = cSeq++;
                                RespPublicIP(respdf);
                            }
                                //_client.Send(MakeResponse(opCode, senderseq));
                                break;
                            case OpCode.PERM_AUTH_DA:
                                _client.Send(GetDeliveryACK(senderseq));
                                //_client.Send(Request(OpCode.REQ_BISINFO_START_DA));
                                break;

                            //!< Required Paired Action
                            case OpCode.REQ_STATUS_START:
                                //Resp OpCode.RESP_STATUS_START, OpCode.PERIODIC_STATUS_DA
                                //_client.Send(Request(OpCode.REQ_BISINFO_START_DA));
                                _client.Send(MakeResponse(opCode, senderseq));
                                _client.Send(MakePeriodic(OpCode.PERIODIC_STATUS_DA));
                                break;
                            case OpCode.REQ_STATUS:
                            case OpCode.REQ_PROVIDER_INFO:
                            case OpCode.REQ_TEMPERATURE:
                            case OpCode.REQ_DISP_SCHEDULE:
                            case OpCode.REQ_SNDMXR:
                                _client.Send(MakeResponse(opCode, senderseq));
                                break;
                            case OpCode.REQ_AUTH:
                                _client.Send(ResponseAuthenticaction(senderseq));
                                break;
                            //case 
                        }
                    }
                    else
                    {
                        FillStream(rewindPos+1);
                    }
                }
                else
                {
                    FillStream(rewindPos);
                }
            }
        }

        public byte[] ResponseAuthenticaction(ushort seq)
        {
            Byte[] ackBuf = new Byte[16+5];
            DataFrame df = new DataFrame();
            df.OpCode = (byte)OpCode.RESP_AUTH;
            df.DataLength = 16;
            df.Seq = cSeq++;
            Byte[] has = df.GetBytes();//ByteStreamCovert.ToByteArray(df);
            int x = has.Length;
            Array.Copy(has, 0, ackBuf, 0, x);


            Resp_Authentication auth = new Resp_Authentication();
            auth.Seq = seq;
            auth.bitid = Encoding.UTF8.GetBytes(ConfigBIT.ID);//new byte[] { 0x32, 0x33, 0x37, 0x30, 0x30, 0x30, 0x31, 0x30, 0x31 };
            auth.support_statusmask = 0xFFFF;
            auth.netType = 0;
            Byte[] body = auth.WriteToStream();
            Array.Copy(body, 0, ackBuf, x, body.Length);
            return ackBuf;
        }

        public byte[] GetDeliveryACK(ushort seq)
        {
            Byte[] ackBuf = new Byte[7];
            DataFrame df = new DataFrame();
            df.OpCode = (byte)OpCode.DELIVERY_ACK_DA;
            df.DataLength = 2;
            df.Seq = cSeq++;
            Byte[] has = df.GetBytes();//ByteStreamCovert.ToByteArray(df);
            int x = has.Length;
            Array.Copy(has, 0, ackBuf, 0, x);
            Array.Copy(ByteStreamCovert.GetBigEndian(BitConverter.GetBytes(seq)), 0, ackBuf, x, 2);
            return ackBuf;

            return null;
        }

        public byte[] Request(OpCode code)
        {
            DataFrame df = new DataFrame();
            df.OpCode = (byte)code;
            df.DataLength = 0;
            df.Seq = cSeq++;

            Byte[] _payload = null;
            if (code == OpCode.REQ_BISINFO_START_DA)
            {
                _payload = new Byte[5];
                Array.Copy(df.GetBytes(), 0, _payload, 0, 5);
            }
            else
            {
            }

            return _payload;

            return null;
        }


        public byte[] MakeSync()
        {
            DataFrame df = new DataFrame();
            df.OpCode = (byte) OpCode.INFO_SYNC_DA;

            Info_Sync sync = new Info_Sync();
            sync.nEntry = (byte)nFileInfo;
            sync.Entries = listFileInfo;
            df.DataLength = (ushort)sync.GetBytes().Length;
            df.Seq = cSeq++;

            Byte[] syncBuf = new Byte[5 + df.DataLength];
            Byte[] has = df.GetBytes();//ByteStreamCovert.ToByteArray(df);
            Array.Copy(df.GetBytes(), 0, syncBuf, 0, 5);
            Array.Copy(sync.GetBytes(), 0, syncBuf, 5, df.DataLength);
            return syncBuf;
        }

        private byte[] MakePeriodic(OpCode _Code)
        {
            DataFrame df = new DataFrame();
            df.OpCode = (byte)_Code;
            df.DataLength = 0;
            df.Seq = cSeq++;

            Byte[] _payload = null;
            if (_Code == OpCode.PERIODIC_STATUS_DA)
            {
                Periodic_Status _stat = new Periodic_Status();
                _stat.nInfos = 0;
                _stat.statusvalue = null;
                df.DataLength = (ushort)_stat.GetBytes().Length;

                _payload = new Byte[5 + df.DataLength];
                Array.Copy(df.GetBytes(), 0, _payload, 0, 5);
                Array.Copy(_stat.GetBytes(), 0, _payload, 5, df.DataLength);
            }
            return _payload;
        }

        private byte[] MakeFileOpRespons(byte cmd, byte[] fileinfo)
        {
            DataFrame df = new DataFrame();
            df.OpCode = (byte)OpCode.REPORT_FILE_OP_DA;
            df.DataLength = (ushort)(fileinfo.Length + 2);
            df.Seq = cSeq++;

            byte[] opResult = new byte[]{cmd, 0};

            Byte[] _payload = null;
            _payload = new Byte[5 + df.DataLength];
            Array.Copy(df.GetBytes(), 0, _payload, 0, 5);
            Array.Copy(opResult, 0, _payload, 5, 2);
            Array.Copy(fileinfo, 0, _payload, 7, fileinfo.Length);

            return _payload;
        }
        public byte[] MakeResponse(OpCode _Code, ushort _Seq)
        {
            Byte[] _payload = null;
            DataFrame df = new DataFrame();
            df.DataLength = 0;
            df.Seq = cSeq++;

            OpCode _pair = OpCode.UNKNOWN;
            switch (_Code)
            {
                //case OpCode.REQ_AUTH: aka ResponseAuthenticaction()
                //    _pair = OpCode.RESP_AUTH;
                    //df.DataLength = Resp_Authentication.size
                //    break;
                //case OpCode.RESP_AUTH: //Server Reply
                //    _pair = OpCode.PERM_AUTH_DA;
                //    df.DataLength = Permit_Authentication.size
                //    break;
                case OpCode.REQ_STATUS_START:
                    _pair = OpCode.RESP_STATUS_START;
                {
                    Resp_StatusStart resp = new Resp_StatusStart();
                    resp.reqSeq = _Seq;
                    resp.nInfos = 0;
                    resp.statusvalue = null;
                    df.DataLength = (ushort)resp.GetBytes().Length;
                    df.OpCode = (byte)_pair;
                    _payload = new Byte[5 + df.DataLength];
                    Array.Copy(df.GetBytes(), 0, _payload, 0, 5);
                    Array.Copy(resp.GetBytes(), 0, _payload, 5, df.DataLength);
                }
                    break;
                case OpCode.REQ_STATUS:
                    _pair = OpCode.RESP_STATUS;
                {
                    Resp_StatusStart resp = new Resp_StatusStart();
                    resp.reqSeq = _Seq;
                    resp.nInfos = 0;
                    resp.statusvalue = null;
                    df.DataLength = (ushort)resp.GetBytes().Length;
                    df.OpCode = (byte)_pair;
                    _payload = new Byte[5 + df.DataLength];
                    Array.Copy(df.GetBytes(), 0, _payload, 0, 5);
                    Array.Copy(resp.GetBytes(), 0, _payload, 5, df.DataLength);
                }
                    break;
                //case OpCode.REQ_PROVIDER_INFO: //Server Reply
                //    _pair = OpCode.RESP_PPOVIDER_INFO;
                //    break;
                case OpCode.REQ_TEMPERATURE:
                    _pair = OpCode.RESP_TEMPERATURE;
                {
                    Resp_Temperature resp = new Resp_Temperature();
                    resp.Seq = _Seq;
                    df.DataLength = (ushort)resp.GetBytes().Length;
                    df.OpCode = (byte)_pair;
                    _payload = new Byte[5 + df.DataLength];
                    Array.Copy(df.GetBytes(), 0, _payload, 0, 5);
                    Array.Copy(resp.GetBytes(), 0, _payload, 5, df.DataLength);
                }
                    break;
                case OpCode.REQ_DISP_SCHEDULE:
                    _pair = OpCode.RESP_DISP_SCHEDULE;
                    //df.DataLength = Resp_DisplaySchedule.size
                    break;
                case OpCode.REQ_SNDMXR:
                    _pair = OpCode.RESP_SNDMXR;
                    //df.DataLength = Resp_SoundMixer.size
                    break;

                case OpCode.REQ_PUB_IP_DA:

                    _pair = OpCode.RESP_PUB_IP_DA;
                {
                    Resp_External_IP resp = new Resp_External_IP();
                    resp.ResultCode = 0;
                    resp.IPVersion = 0;
                    resp.IPResult = new UInt32[] { (UInt32)BitConverter.ToInt32(IPAddress.Parse(pubIP.ToString()).GetAddressBytes(), 0) };
                    df.DataLength = (ushort)resp.GetBytes().Length;
                    df.OpCode = (byte)_pair;
                    _payload = new Byte[5 + df.DataLength];
                    Array.Copy(df.GetBytes(), 0, _payload, 0, 5);
                    Array.Copy(resp.GetBytes(), 0, _payload, 5, df.DataLength);
                }
                    break;
            }
            
            return _payload;

        }
    }
}