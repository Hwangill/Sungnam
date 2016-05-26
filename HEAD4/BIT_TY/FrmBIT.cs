using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using Common;
using MarqueControl.Controls;
using MarqueControl.Entity;
using AlexPilotti.FTPS.Client;
using AlexPilotti.FTPS.Common;
using Protocol.GG_SN;
using System.IO.Compression;
using System.Threading;
using Protocol;
using WMPLib;
using AppRtu2;
using DllRtu;
using DllRtu.Interface;

namespace BIT_TY
{
    /// <summary>
    /// 화면 처리부
    /// </summary>
    public partial class FrmBIT : Form
    {
        private Brush br = new SolidBrush(Color.Blue);
        private Image bg = null;
        private MarqueControl.Controls.SuperMarquee[] nMarquees = null;
        private MarqueControl.Controls.SuperMarquee nearBus = new SuperMarquee();
        private Size? _mouseGrabOffset;
        private Image[] pictoImages = new Image[7];
        private InfoCollection ConfigObj = new InfoCollection();
        private BITConfig ConfigBIT = new BITConfig();
        private Label lbTodayW = new Label();
        private Label lbTomorowW = new Label();
        private Label lbTodayTemp = new Label();
        private Label lbTomorowTemp = new Label();
        private Label lbDate= new Label();
        private Label lbTime = new Label();
        private Label lbBITName = new Label();
        private Label lbBITId = new Label();
        private string bitName = "";
        private string bitID = "";
        private List<MediaFileInfo> mediaList = new List<MediaFileInfo>();
        private int dropCnt = 0;
        private string curMedia = "";
        private ConcurrentQueue<PredictInfo> infos = new ConcurrentQueue<PredictInfo>();

        private Form1 frmConsole = new Form1();
        private Protocol.Protocol_GGSN _client;
        private System.Threading.Timer _removeTimer = null;
        private int pageGap = 5000;
        private int nPageNum = 0;


        public FrmBIT()
        {
            
            //!< 오늘, 내일날씨
            //!< 현재 정류장명칭 214, 14  552, 62
            //!< 정류장 번호 214, 66  552, 96
            //!< 날짜 632, 20  758, 44
            //!< 시간 632, 50  758, 78
            //!< 진입중 180, 100  768, 186
            //!< 정보 시작 0, 252 각줄의 높이는 실제 84와 gap 2
            //!< 셀정보 X => 0-31 31-177, 179-261 261-296, 298-691 691-768

            //!< 광고 0, 767  768, 1289
            //!< 뉴스 0, 1290  768, 1366
            //!< 버스속성이미지 31 37 배경으로 사용, 84짜리 이미지 만들고, 위에서 23 떨어진 곳에 복사
            //!< 노선명 글자로 사용
            //!< 남은시간, 분이미지 35, 51. Marquee에서는 배경으로 사용, 84짜리 이미지 만들고, 위에서 16 떨어진 곳에 복사
            //!< 픽토그램 73 84 하단에 7654321 위치를 모르겠음, 이미지를 각각에 대해서 메모리로 만들고 Marquee에서는 배경으로 사용. 글자일 경우 스크롤
            //!< 픽토그램 다음의 경우 18(상단 각진 모서리 위치 55) 만큼 밀려서 출력한다. ==> n개에 대해 n(73 - 18) + 18
            //!< 7개인 경우 
            //!< 첫차, 막차, 현위치 77(71만씀), 84 - 없을 경우 그냥 투명 이미지를 발라야 함. Marquee에서는 배경으로 사용


            XmlSerializer SerializerObj = new XmlSerializer(typeof(InfoCollection));
            FileStream configStream = new FileStream(@"viewconfig.xml", FileMode.Open, FileAccess.Read, FileShare.Read);//InfoConfig.xml
            ConfigObj = (InfoCollection)SerializerObj.Deserialize(configStream);
            configStream.Close();


            SerializerObj = new XmlSerializer(typeof(BITConfig));
            configStream = new FileStream(@"bitconfig.xml", FileMode.Open, FileAccess.Read, FileShare.Read);//InfoConfig.xml
            ConfigBIT = (BITConfig)SerializerObj.Deserialize(configStream);
            configStream.Close();

            Image colNum = Common.Utility.ToolSnippet.CreateTextImage("7             " +
                "6            5             4            3             2            1",
                new System.Drawing.Font("나눔고딕", 13.0F,
                    System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte) (0))),
                Color.Black, Color.Transparent);
            Image subPicto = Image.FromFile(ConfigBIT.BasePath + @"\Image\Vertical\버스방향.png");
            for (int i = 0; i < pictoImages.Length; i++)
            {
                pictoImages[i] = new Bitmap(400, 84, PixelFormat.Format32bppArgb);
                using (Graphics grD = Graphics.FromImage(pictoImages[i]))
                {
                    grD.DrawImage(subPicto, new Rectangle(new Point(i * (73 - 18), 0), new Size(subPicto.Width, subPicto.Height)), 
                        new Rectangle(new Point(0, 0), new Size(subPicto.Width, subPicto.Height)), GraphicsUnit.Pixel);
                    grD.DrawImage(colNum, new Rectangle(new Point(24, 66), new Size(colNum.Width, colNum.Height)),
                        new Rectangle(new Point(0, 0), new Size(colNum.Width, colNum.Height)), GraphicsUnit.Pixel);

                }
            }
            
            InitializeComponent();

            InitLabel();
            InitInfoRow();

            _removeTimer = new System.Threading.Timer(new TimerCallback(nextPage), null, 0, pageGap);
            //_removeTimer.AutoReset = start;
            //_removeTimer.Elapsed += new System.Timers.ElapsedEventHandler(delegate { nextPage(); });  

            tickTimer.Enabled = true;
            label1.Location = new Point(1000,1000);
            label1.ForeColor = Color.OrangeRed;
            this.FormBorderStyle = FormBorderStyle.None;
            Refresh();

            int multiOffset = 0;

            if (Screen.AllScreens.Length > 1)
            {
                multiOffset = Screen.PrimaryScreen.Bounds.Width;
            }
            int wgap = this.Width - this.ClientRectangle.Width;//= bg.Width;
            int hgap = this.Height - this.ClientRectangle.Height;//= bg.Width;
            this.Width = bg.Width + wgap;
            this.Height = bg.Height + hgap;
            this.Location = new Point(multiOffset, 0);
            this.Size = new Size(bg.Width + wgap, bg.Height + hgap);
            
            string zipPath = @"D:\Workz\BitBucket\Purgatory.NET\BIT_TY\BIT_TY\DB\Master\20150830.zip";
            string extractPath = @"D:\Workz\BitBucket\Purgatory.NET\BIT_TY\BIT_TY\DB\Master";
            //Common.Utility.ToolSnippet.ExtractZipArchive(zipPath, extractPath);

            frmConsole.Show();

            _client = new Protocol_GGSN();
            _client.SocketActionHandler += new EventHandler<string>(frmConsole.SocketReceived);
            _client.InfoArrived += delegate(List<PredictInfo> infos)
            {
                UpdateInfo(infos);
            };
            /*
            _client.SocketActionHandler += delegate(object sender, string s)
            {
                if (textBox1 == null)
                    return;
                if (InvokeRequired)
                {
                    textBox1.Invoke((MethodInvoker)delegate
                    {
                        textBox1.AppendText(s);
                        textBox1.AppendText(Environment.NewLine);
                    });
                    return;
                }
                if (textBox1.IsAccessible && textBox1 != null)
                {
                    textBox1.AppendText(s);
                    textBox1.AppendText(Environment.NewLine);
                }

            };
            */
            _client.Connect();

            

            Refresh();
            //SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            //this.BackColor = Color.Transparent;
        }

        private void nextPage(object stateInfo)
        {
            //lock (infos)
            {
                _removeTimer.Change(Timeout.Infinite, Timeout.Infinite);

                DateTime dt = DateTime.Now;
                PredictInfo[] aInfo = infos.ToArray();
                PredictInfo item;
                lock (infos)
                {
                    while (infos.TryDequeue(out item))
                    {
                    }

                    foreach (PredictInfo i in aInfo)
                    {
                        if ((dt - i.recvTime).TotalMinutes < 5)
                        {
                            infos.Enqueue(i);
                        }
                    }

                }
                
                List<PredictInfo> arrival = infos.Where(x => x.bApproach == true).ToList();
                string strApproach = String.Empty;
                foreach (var arrItem in arrival)
                {
                    if (strApproach.Length < 1)
                    {
                        strApproach += arrItem.routeName;
                    }
                    else
                    {
                        strApproach += @",";
                        strApproach += arrItem.routeName;
                    }

                }
                nearBus.Invoke((MethodInvoker)delegate
                {
                    nearBus.Elements[0].Text = strApproach;
                    nearBus.Refresh();
                });


                int rowGap = 6;
                if (nPageNum > infos.Count)
                    nPageNum = 0;

                List<PredictInfo> inforows = infos.Where(x => x.bApproach == false).ToList();//.
                if (nPageNum*ConfigObj.Rows.Count < inforows.Count)
                {
                    inforows = inforows.GetRange(nPageNum * ConfigObj.Rows.Count, inforows.Count - nPageNum * ConfigObj.Rows.Count);
                }
                else
                {
                    nPageNum = 0;
                }
                //inforows.Sort((x, y) => x.nremTime.CompareTo(y.nremTime));
                for (int rowid = 0; rowid < ConfigObj.Rows.Count; rowid++)
                {
                    if (rowid < inforows.Count)
                    {
                        nMarquees[rowid * 6].Invoke((MethodInvoker)delegate
                        {
                            if (nMarquees[rowid * 6].Elements.Count > 1)
                                nMarquees[rowid * 6].Elements.RemoveAt(1);
                            nMarquees[rowid * 6].BackgroundImage = new Bitmap(ConfigBIT.BasePath + @"\Image\Vertical\" + inforows[rowid].busType + @"버스.png");
                        });

                        nMarquees[rowid * 6 + 1].Invoke((MethodInvoker)delegate
                        {
                            if (nMarquees[rowid * 6 + 1].Elements.Count > 1)
                                nMarquees[rowid * 6 + 1].Elements.RemoveAt(1);
                            nMarquees[rowid * 6 + 1].Elements[0].Text = inforows[rowid].routeName;
                        });

                        nMarquees[rowid * 6 + 2].Invoke((MethodInvoker)delegate
                        {
                            if (nMarquees[rowid * 6 + 2].Elements.Count > 1)
                                nMarquees[rowid * 6 + 2].Elements.RemoveAt(1);
                            nMarquees[rowid * 6 + 2].Elements[0].Text = inforows[rowid].remainTime;
                        });

                        nMarquees[rowid * 6 + 3].Invoke((MethodInvoker)delegate
                        {
                            if (nMarquees[rowid * 6 + 3].Elements.Count > 1)
                                nMarquees[rowid * 6 + 3].Elements.RemoveAt(1);
                            nMarquees[rowid * 6 + 3].BackgroundImage = new Bitmap(ConfigBIT.BasePath + @"\Image\Vertical\ArriveTimeBack.png");
                        });


                        nMarquees[rowid * 6 + 4].Invoke((MethodInvoker)delegate
                        {
                            if (nMarquees[rowid * 6 + 4].Elements.Count > 1)
                                nMarquees[rowid * 6 + 4].Elements.RemoveAt(1);

                            if (inforows[rowid].nremStation <= 7)
                            {
                                nMarquees[rowid * 6 + 4].Elements[0].Text = string.Empty;
                                nMarquees[rowid * 6 + 4].BackgroundImage = pictoImages[7 - inforows[rowid].nremStation];
                            }
                            else
                            {
                                nMarquees[rowid * 6 + 4].BackgroundImage = null;
                                nMarquees[rowid * 6 + 4].Elements[0].Text = inforows[rowid].stationName;
                            }
                        });

                        nMarquees[rowid * 6 + 5].Invoke((MethodInvoker)delegate
                        {
                            if (nMarquees[rowid * 6 + 5].Elements.Count > 1)
                                nMarquees[rowid * 6 + 5].Elements.RemoveAt(1);

                            if (inforows[rowid].nremStation <= 7)
                            {
                                if (inforows[rowid].runType == 0)
                                {
                                    nMarquees[rowid * 6 + 5].BackgroundImage =
    new Bitmap(ConfigBIT.BasePath + @"\Image\Vertical\첫차.png");
                                }
                                else if (inforows[rowid].runType == 1)
                                {
                                    nMarquees[rowid * 6 + 5].BackgroundImage =
    new Bitmap(ConfigBIT.BasePath + @"\Image\Vertical\막차.png");
                                }
                                else
                                {
                                    nMarquees[rowid * 6 + 5].BackgroundImage =
    new Bitmap(ConfigBIT.BasePath + @"\Image\Vertical\현위치.png");
                                }

                            }
                            else
                            {
                                nMarquees[rowid * 6 + 5].BackgroundImage = null;
                            }
                        });
                    }
                    else
                    {
                        //!< clear row data
                        for (int icell = 0; icell < ConfigObj.Rows[0].Cells.Count; icell++)
                        {
                            nMarquees[rowid * 6].Invoke((MethodInvoker)delegate
                            {
                                if (nMarquees[rowid * 6 + icell].Elements.Count > 1)
                                    nMarquees[rowid * 6 + icell].Elements.RemoveAt(1);
                                nMarquees[rowid * 6 + icell].Elements[0].Text = string.Empty;
                                nMarquees[rowid * 6 + icell].BackgroundImage = null;
                            });
                        }

                    }
                }
                nPageNum++;
                _removeTimer.Change(pageGap, pageGap);
            }
        }

        private void UpdateInfo(List<PredictInfo> _infos)
        {
            PredictInfo item;
            while (infos.TryDequeue(out item))
            {
            }
            lock (_infos)
            {
                foreach (PredictInfo i in _infos)
                {
                    infos.Enqueue(i);
                }
            }

            return;
        }
        public void LoadStation(string path)
        {
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
                    if (fields[0].Equals(ConfigBIT.ID))
                    {
                        bitName = fields[1];
                        bitID = fields[2];
                        break;
                    }
                }
            }
        }

        private void InitLabel()
        {
            
            LoadStation(ConfigBIT.BasePath + @"/DB/Master/station.csv");
            DateTime dt = DateTime.Now;
            //!< 날씨
            lbTodayW.Location = new Point(ConfigObj.TodayWeather.XOffset, ConfigObj.TodayWeather.YOffset);
            lbTodayW.Size = new Size(ConfigObj.TodayWeather.Width, ConfigObj.TodayWeather.Height);
            lbTodayW.BackColor = ConfigObj.TodayWeather.BGColor;
            lbTodayW.BackgroundImage = Image.FromFile(ConfigBIT.BasePath + @"/Image/Vertical/Weather/맑음.png");
            lbTodayW.BackgroundImageLayout = ImageLayout.Zoom;

            this.Controls.Add(lbTodayW);


            lbTomorowW.Location = new Point(ConfigObj.TomorowWeather.XOffset, ConfigObj.TomorowWeather.YOffset);
            lbTomorowW.Size = new Size(ConfigObj.TomorowWeather.Width, ConfigObj.TomorowWeather.Height);
            lbTomorowW.BackColor = ConfigObj.TodayWeather.BGColor;
            lbTomorowW.BackgroundImage = Image.FromFile(ConfigBIT.BasePath + @"/Image/Vertical/Weather/비.png");
            lbTomorowW.BackgroundImageLayout = ImageLayout.Zoom;

            this.Controls.Add(lbTomorowW);

            lbBITName.Location = new Point(ConfigObj.StationName.XOffset, ConfigObj.StationName.YOffset);
            lbBITName.Size = new Size(ConfigObj.StationName.Width, ConfigObj.StationName.Height);
            lbBITName.BackColor = ConfigObj.StationName.BGColor;
            lbBITName.ForeColor = ConfigObj.StationName.FGColor;
            lbBITName.Font = new System.Drawing.Font(ConfigObj.StationName.FontName/*"나눔고딕 ExtraBold"*/, ConfigObj.StationName.FontSize/*40F*/, 
                System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            //lbBITName.BackgroundImage = Image.FromFile(@"../../Image/Vertical/Weather/비.png");
            //lbBITName.BackgroundImageLayout = ImageLayout.Zoom;
            lbBITName.TextAlign = ContentAlignment.MiddleCenter;
            lbBITName.Text = bitName;
            this.Controls.Add(lbBITName);

            lbBITId.Location = new Point(ConfigObj.StationId.XOffset, ConfigObj.StationId.YOffset);
            lbBITId.Size = new Size(ConfigObj.StationId.Width, ConfigObj.StationId.Height);
            lbBITId.BackColor = ConfigObj.StationId.BGColor;
            lbBITId.ForeColor = ConfigObj.StationId.FGColor;
            lbBITId.Font = new System.Drawing.Font(ConfigObj.StationId.FontName/*"나눔고딕 ExtraBold"*/, ConfigObj.StationId.FontSize/*40F*/,
                System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            lbBITId.TextAlign = ContentAlignment.MiddleCenter;
            lbBITId.Text = bitID;

            this.Controls.Add(lbBITId);

            lbDate.Location = new Point(ConfigObj.TodayDate.XOffset, ConfigObj.TodayDate.YOffset);
            lbDate.Size = new Size(ConfigObj.TodayDate.Width, ConfigObj.TodayDate.Height);
            lbDate.BackColor = ConfigObj.TodayDate.BGColor;
            lbDate.ForeColor = ConfigObj.TodayDate.FGColor;
            lbDate.Font = new System.Drawing.Font(ConfigObj.TodayDate.FontName/*"나눔고딕 ExtraBold"*/, ConfigObj.TodayDate.FontSize/*40F*/,
                System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            lbDate.Text = dt.ToString("MM", CultureInfo.CurrentCulture) + "월 " + dt.ToString("dd", CultureInfo.CurrentCulture) + "일"; //String.Format("{MM}", dt) + "월 " + String.Format("{dd}", dt) + "일";
            lbDate.TextAlign = ContentAlignment.MiddleRight;
            this.Controls.Add(lbDate);

            lbTime.Location = new Point(ConfigObj.TimeNow.XOffset, ConfigObj.TimeNow.YOffset);
            lbTime.Size = new Size(ConfigObj.TimeNow.Width, ConfigObj.TimeNow.Height);
            lbTime.BackColor = ConfigObj.TimeNow.BGColor;
            lbTime.ForeColor = ConfigObj.TimeNow.FGColor;
            lbTime.Font = new System.Drawing.Font(ConfigObj.TimeNow.FontName/*"나눔고딕 ExtraBold"*/, ConfigObj.TimeNow.FontSize/*40F*/,
                System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            lbTime.TextAlign = ContentAlignment.MiddleRight;
            lbTime.Text = dt.ToString("tt", CultureInfo.CurrentCulture) + " " + dt.ToString("HH:mm", CultureInfo.CurrentCulture);

            this.Controls.Add(lbTime);


            lbTodayTemp.Location = new Point(ConfigObj.TodayTemperature.XOffset, ConfigObj.TodayTemperature.YOffset);
            lbTodayTemp.Size = new Size(ConfigObj.TodayTemperature.Width, ConfigObj.TodayTemperature.Height);
            lbTodayTemp.BackColor = ConfigObj.TodayTemperature.BGColor;
            lbTodayTemp.ForeColor = ConfigObj.TodayTemperature.FGColor;
            lbTodayTemp.Font = new System.Drawing.Font(ConfigObj.TodayTemperature.FontName/*"나눔고딕 ExtraBold"*/, ConfigObj.TodayTemperature.FontSize/*40F*/,
                System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            lbTodayTemp.TextAlign = ContentAlignment.MiddleCenter;

            this.Controls.Add(lbTodayTemp);


            lbTomorowTemp.Location = new Point(ConfigObj.TomorowTemperature.XOffset, ConfigObj.TomorowTemperature.YOffset);
            lbTomorowTemp.Size = new Size(ConfigObj.TomorowTemperature.Width, ConfigObj.TomorowTemperature.Height);
            lbTomorowTemp.BackColor = ConfigObj.TomorowTemperature.BGColor;
            lbTomorowTemp.ForeColor = ConfigObj.TomorowTemperature.FGColor;
            lbTomorowTemp.Font = new System.Drawing.Font(ConfigObj.TomorowTemperature.FontName/*"나눔고딕 ExtraBold"*/, ConfigObj.TomorowTemperature.FontSize/*40F*/,
                System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            lbTomorowTemp.TextAlign = ContentAlignment.MiddleCenter;

            this.Controls.Add(lbTomorowTemp);

            UpdateWeather();
        }

        internal string GetWImage(int code)
        {
            switch (code)
            {
                case 1:
                    return "맑음.png";
                    break;
                case 2:
                    return "구름조금.png";
                    break;
                case 3:
                    return "구름많음.png";
                    break;
                case 4:
                    return "흐림.png";
                    break;
                case 5:
                    return "비.png";
                    break;
                case 6:
                    return "비눈.png";
                    break;
                case 7:
                    return "눈.png";
                    break;
            }
            return "맑음.png";
        }

        private void UpdateWeather()
        {
            Encoding ec = System.Text.Encoding.GetEncoding(51949);
            string dbpath = ConfigBIT.BasePath + @"/DB/DB/Weather.wdb";
            string path = ConfigBIT.BasePath + @"/Image/Vertical/Weather/";
            System.IO.StreamReader file = new System.IO.StreamReader(dbpath, ec);
            string wdata = file.ReadLine();

            string[] days = wdata.Split('^');
            string[] todays = days[0].Split('|');

            string wfile = ConfigBIT.BasePath + @"/Image/Vertical/Weather/" + GetWImage(Int32.Parse(todays[1]));

            //1[]
            lbTodayW.BackgroundImage = Image.FromFile(wfile);
            lbTodayTemp.Text = todays[2] + "℃";
            //2[]온도

            string[] tomorows = days[1].Split('|');
            wfile = ConfigBIT.BasePath + @"/Image/Vertical/Weather/" + GetWImage(Int32.Parse(tomorows[1]));
            lbTomorowW.BackgroundImage = Image.FromFile(wfile);
            lbTomorowTemp.Text = tomorows[2] + "℃";
            lbTodayW.Refresh();
            lbTomorowW.Refresh();
        }

        private void InitInfoRow()
        {
            bg = new Bitmap(ConfigObj.BackgroundImagePath);// Image.FromFile(ConfigObj.BackgroundImagePath);
            nearBus.Location = new Point(ConfigObj.NearBus.XOffset, ConfigObj.NearBus.YOffset);
            nearBus.Size = new Size(ConfigObj.NearBus.Width, ConfigObj.NearBus.Height);

            nearBus.MarqueeSpeed = 999;
            nearBus.BackColor = ConfigObj.NearBus.BGColor;
            nearBus.BackgroundImageLayout = ImageLayout.None;
            {
                TextElement element = new TextElement("Element text");
                element.IsLink = false;
                element.ForeColor = ConfigObj.NearBus.FGColor;
                element.Text = " ";
                element.Tag = "Tag for element";
                element.Font = new System.Drawing.Font(ConfigObj.NearBus.FontName/*"나눔고딕 ExtraBold"*/, ConfigObj.NearBus.FontSize/*40F*/, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));

                nearBus.Elements.Add(element);
                nearBus.Visible = true;
            }
            this.Controls.Add(nearBus);


            if (nMarquees == null)
            {
                int rowCount = ConfigObj.Rows.Count;
                int cellCount = 0;
                if (rowCount > 0)
                    cellCount = ConfigObj.Rows[0].Cells.Count;
                nMarquees = new SuperMarquee[rowCount * cellCount];

                int cellNum = 0;
                foreach (InfoRow row in ConfigObj.Rows)
                {
                    foreach (InfoCell cell in row.Cells)
                    {
                        nMarquees[cellNum] = new SuperMarquee();
                        nMarquees[cellNum].Location = new Point(cell.XOffset + row.XOffset, cell.YOffset + row.YOffset);
                        nMarquees[cellNum].Size = new Size(cell.Width, cell.Height);

                        nMarquees[cellNum].MarqueeSpeed = 999;
                        nMarquees[cellNum].BackColor = cell.BGColor;
                        nMarquees[cellNum].BackgroundImageLayout = ImageLayout.None;
                        if (cellNum % 6 == 0)
                        {
                            if ((cellNum / 6) % 2 == 0)
                                nMarquees[cellNum].BackgroundImage = new Bitmap(ConfigBIT.BasePath + @"\Image\Vertical\저상버스.png");
                            else
                            {
                                nMarquees[cellNum].BackgroundImage = new Bitmap(ConfigBIT.BasePath + @"\Image\Vertical\좌석버스.png");
                            }
                        }
                        else if (cellNum % 6 == 3)
                        {
                            nMarquees[cellNum].BackgroundImage = new Bitmap(ConfigBIT.BasePath + @"\Image\Vertical\ArriveTimeBack.png");
                        }
                        else if (cellNum % 6 == 4)
                        {
                            nMarquees[cellNum].BackgroundImage = pictoImages[(cellNum / 6)];//new Bitmap(@"..\..\Image\Vertical\버스방향.png");

                            nMarquees[cellNum].BackgroundImageLayout = ImageLayout.Tile;
                        }
                        else if (cellNum % 6 == 5)
                        {
                            if ((cellNum / 6) % 2 == 0)
                                nMarquees[cellNum].BackgroundImage = new Bitmap(ConfigBIT.BasePath + @"\Image\Vertical\첫차.png");
                            else
                            {
                                nMarquees[cellNum].BackgroundImage = new Bitmap(ConfigBIT.BasePath + @"\Image\Vertical\막차.png");
                            }
                        }

                        nMarquees[cellNum].Elements.Clear();

                        if (nMarquees[cellNum].BackgroundImage == null)
                        {
                            TextElement element = new TextElement("Element text");
                            element.IsLink = false;
                            element.ForeColor = cell.FGColor;
                            element.Text = "2412";
                            element.Tag = "Tag for element";
                            element.Font = new System.Drawing.Font(cell.FontName/*"나눔고딕 ExtraBold"*/, cell.FontSize/*40F*/, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
                            element.ToolTipText = "Tool tip for element";

                            if (cellNum % 6 == 2)
                                element.StringFormat.Alignment = StringAlignment.Far;
                            
                            nMarquees[cellNum].Elements.Add(element);
                            nMarquees[cellNum].Elements.Add(element);
                            nMarquees[cellNum].Visible = true;
                            nMarquees[cellNum].Show();
                        }
                        else
                        {
                            TextElement element = new TextElement("Element text");
                            element.IsLink = false;
                            element.ForeColor = cell.FGColor;
                            element.Text = " ";
                            element.Tag = "Tag for element";
                            element.Font = new System.Drawing.Font(cell.FontName/*"나눔고딕 ExtraBold"*/, cell.FontSize/*40F*/, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
                            element.ToolTipText = "Tool tip for element";

                            if (cellNum % 6 == 2)
                                element.StringFormat.Alignment = StringAlignment.Far;

                            nMarquees[cellNum].Elements.Add(element);
                            nMarquees[cellNum].Visible = true;
                            nMarquees[cellNum].Show();
                        }

                        cellNum++;
                    }
                }
                this.Controls.AddRange(nMarquees);

                for (int rowid = 0; rowid < ConfigObj.Rows.Count; rowid++)
                {
                    for (int icell = 0; icell < ConfigObj.Rows[0].Cells.Count; icell++)
                    {
                        nMarquees[rowid * 6].Invoke((MethodInvoker)delegate
                        {
                            if (nMarquees[rowid * 6 + icell].Elements.Count > 1)
                                nMarquees[rowid * 6 + icell].Elements.RemoveAt(1);
                            nMarquees[rowid * 6 + icell].Elements[0].Text = string.Empty;
                            nMarquees[rowid * 6 + icell].BackgroundImage = null;
                        });
                    }   
                }




                axWindowsMediaPlayer.Location = new Point(0, 767);
                axWindowsMediaPlayer.Size = new Size(768, 1289 - 767);
                // TODO 파일 명 바꾸기
                string scfile = ConfigBIT.BasePath + @"/DB/SNRDB/20151016";
                using (BinaryReader brBinaryReader = new BinaryReader(File.Open(scfile, FileMode.Open)))
                {
                    FileInfo f = new FileInfo(scfile);
                    long s1 = f.Length;
                    if ((s1 - 5) % 35 == 0)
                    {
                        Scenario_DataPlain fileData = new Scenario_DataPlain();
                        fileData.SetDataFrom(brBinaryReader);

                        foreach (var item in fileData.formData)
                        {
                            MediaFileInfo mInfo = new MediaFileInfo();
                            mInfo.fileName = ConfigBIT.BasePath + @"/DB/SNR/" + item.fileName;
                            mInfo.durSeconds = item.durationSecond;
                            mInfo.fileType = item.dataType;
                            mediaList.Add(mInfo);
                        }
                        if (mediaList.Count > 0)
                        {
                            WMPLib.IWMPPlaylist playlist = axWindowsMediaPlayer.playlistCollection.newPlaylist("myplaylist");
                            WMPLib.IWMPMedia media;
                            foreach (var file in mediaList)
                            {
                                media = axWindowsMediaPlayer.newMedia(file.fileName);
                                playlist.appendItem(media);
                            }
                            axWindowsMediaPlayer.settings.setMode("loop", true);
                            axWindowsMediaPlayer.currentPlaylist = playlist;
                            axWindowsMediaPlayer.Ctlcontrols.play();
                            axWindowsMediaPlayer.settings.mute = true;
                            axWindowsMediaPlayer.stretchToFit = true;
                            /*
                            dropCnt = mediaList[0].durSeconds;
                            curMedia = mediaList[0].fileName;
                            axWindowsMediaPlayer.Visible = true;
                            axWindowsMediaPlayer.URL = curMedia;
                            
                            axWindowsMediaPlayer.Ctlcontrols.play();
                             */
                        }
                            
                    }
                    else
                    {
                        Scenario_DataSchedule fileData = new Scenario_DataSchedule();
                        fileData.SetDataFrom(brBinaryReader);
                    }

                }

            }
        }

        private void FrmBIT_Paint(object sender, PaintEventArgs e)
        {
            //label1.Text = DateTime.Now.ToLongTimeString();
            //e.Graphics.Mo
            e.Graphics.DrawImage(bg, 0,0, bg.Width, bg.Height);
            Single a = e.Graphics.DpiX;
            float ww = bg.HorizontalResolution;
            //e.Graphics.PageUnit = GraphicsUnit.Display;
            //e.Graphics.DrawImageUnscaled(bg, e.ClipRectangle);
            //e.Graphics.FillRectangle(br,  e.ClipRectangle);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                _mouseGrabOffset = new Size(e.Location);

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _mouseGrabOffset = null;

            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_mouseGrabOffset.HasValue)
            {
                this.Location = Cursor.Position - _mouseGrabOffset.Value;
                label1.Text = this.Location.ToString();
            }

            base.OnMouseMove(e);
        }

        private void timer_Tick(object sender, EventArgs e)
        {

            switch (DateTime.Now.Second%5)
            {
                case 0:
                    br = new SolidBrush(Color.YellowGreen);
                    break;
                case 1:
                    br = new SolidBrush(Color.OrangeRed);
                    break;
                case 2:
                    br = new SolidBrush(Color.Blue);
                    break;
                case 3:
                    br = new SolidBrush(Color.DarkGreen);
                    break;
                case 4:
                    br = new SolidBrush(Color.Gray);
                    break;
            }
            Invoke((MethodInvoker) delegate
            {
                DateTime dt = DateTime.Now;
                /*
                lock (infos)
                {

                    var target = infos.Where(item => (dt - item.recvTime).TotalMinutes > 5);
                    int res = 0;

                    foreach (var item in target)
                        res++;

                    if (res > 0)
                        infos.RemoveAll(item => (dt - item.recvTime).TotalMinutes > 5);
                }
                */
                lbTime.Text = dt.ToString("tt", CultureInfo.CurrentCulture) + " " + dt.ToString("HH:mm", CultureInfo.CurrentCulture);
                UpdateWeather();
                if (dropCnt > 0)
                {
                    dropCnt--;
                    if (dropCnt == 0)
                    {
                        /*
                        int curridx = mediaList.FindIndex(i => i.fileName == curMedia);
                        if (curridx >= mediaList.Count - 1)
                        {
                            curridx = 0;
                            dropCnt = mediaList[0].durSeconds;
                            curMedia = mediaList[0].fileName;
                            //axWindowsMediaPlayer.URL = curMedia;
                            axWindowsMediaPlayer.stretchToFit = true;
                            axWindowsMediaPlayer.Ctlcontrols.a
                            //axWindowsMediaPlayer.Ctlcontrols.play();
                        }
                        else */
                        {
                            tickTimer.Enabled = false;
                            
                            axWindowsMediaPlayer.Ctlcontrols.next();
                            
                            tickTimer.Enabled = true;
                            /*
                            axWindowsMediaPlayer.Ctlcontrols.stop();
                            dropCnt = mediaList[curridx+1].durSeconds;
                            curMedia = mediaList[curridx + 1].fileName;
                            axWindowsMediaPlayer.URL = curMedia;
                            axWindowsMediaPlayer.stretchToFit = true;
                            axWindowsMediaPlayer.Ctlcontrols.play();
                            */
                        }

                    }
                }

                //this.Width += 1;
                //this.Refresh();
                Invalidate();
            });
        }

        private void OnReconnect(object sender, EventArgs e)
        {
            _client.ForceReConnect();
        }

        private void OnTerminate(object sender, EventArgs e)
        {
            _removeTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _client.Close();
            this.Close();
        }

        private void RTU_Control_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // AppRtu2.AppRtu.aaa();
            

            Thread rtuThread = null;
            RtuInterface rtu = null;

            rtu = new Rtu();

            rtuThread = new Thread(new ThreadStart(rtu.threadRun));

            AppRtu2.AppRtu dlg = new AppRtu2.AppRtu(rtu, rtuThread);
            dlg.ShowDialog();
            
        }
    }


}
