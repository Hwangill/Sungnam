using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Protocol;
using Protocol.GG_SN;
using RTU;

namespace BIT_TY
{
    public partial class Form1 : Form
    {
        private Protocol.Protocol_GGSN _client;
        public Form1()
        {
            InitializeComponent();
            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width, 0);
            Refresh();
            //System.Windows.SystemParameters.PrimaryScreenWidth
            DataFrame ddx = new DataFrame();
            Byte[] qq = DataDef.SerializeToBytes(ddx);
            DataFrame dfx = (DataFrame)DataDef.DeserializeFromBytes(qq);
            Byte[] _qqbu = new Byte[5];
            _qqbu[4] = 99;
            using (BinaryReader br = new BinaryReader(new MemoryStream()))
            {
                br.BaseStream.Write(_qqbu, 0, 5);
                br.BaseStream.Seek(0, SeekOrigin.Begin);
                ddx.SetDataFrom(br);
            }


            
            //_client.GetDeliveryACK((ushort) 1);
            //_client.ResponseAuthenticaction((ushort)1);


            //RTU_TY _rtu = new RTU_TY_Rev1("COM1");
            //_rtu.RTUReceived += _rtu_RTUReceived;
            //_rtu.OnRTUReceived(EventArgs.Empty);
        }

        public void SocketReceived(object sender, string bytestring)
        {
            if (textBox1 == null)
                return;
            if (InvokeRequired)
            {
                textBox1.Invoke((MethodInvoker)delegate
                {
                    textBox1.AppendText(bytestring);
                    textBox1.AppendText(Environment.NewLine);
                });
                return;
            }
            if (textBox1.IsAccessible && textBox1 != null)
            {
                textBox1.AppendText(bytestring);
                textBox1.AppendText(Environment.NewLine);
            }
        }
        
        void _rtu_RTUReceived(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                //Invoke(new MethodInvoker(...)
                //Invoke((MethodInvoker)delegate { OnExtentChange(this, MapView.GetExtents()); });
                return;
            }
            //!< 컨트롤에 그리기
            throw new NotImplementedException();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _client.Connect();
        }

        private void OnClosed(object sender, FormClosedEventArgs e)
        {
            _client.Close();
        }
    }
}
