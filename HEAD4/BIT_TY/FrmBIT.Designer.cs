namespace BIT_TY
{
    partial class FrmBIT
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region ---- Windows Form Designer generated code ----

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmBIT));
            this.tickTimer = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.axWindowsMediaPlayer = new AxWMPLib.AxWindowsMediaPlayer();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripExit = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripReconnect = new System.Windows.Forms.ToolStripMenuItem();
            this.카메라보기ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.RTU_Control_ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // tickTimer
            // 
            this.tickTimer.Interval = 1000;
            this.tickTimer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(122, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // axWindowsMediaPlayer
            // 
            this.axWindowsMediaPlayer.Enabled = true;
            this.axWindowsMediaPlayer.Location = new System.Drawing.Point(163, 152);
            this.axWindowsMediaPlayer.Name = "axWindowsMediaPlayer";
            this.axWindowsMediaPlayer.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWindowsMediaPlayer.OcxState")));
            this.axWindowsMediaPlayer.Size = new System.Drawing.Size(75, 195);
            this.axWindowsMediaPlayer.TabIndex = 1;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripExit,
            this.ToolStripReconnect,
            this.카메라보기ToolStripMenuItem,
            this.RTU_Control_ToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(153, 114);
            // 
            // ToolStripExit
            // 
            this.ToolStripExit.Name = "ToolStripExit";
            this.ToolStripExit.Size = new System.Drawing.Size(152, 22);
            this.ToolStripExit.Text = "종료";
            this.ToolStripExit.Click += new System.EventHandler(this.OnTerminate);
            // 
            // ToolStripReconnect
            // 
            this.ToolStripReconnect.Name = "ToolStripReconnect";
            this.ToolStripReconnect.Size = new System.Drawing.Size(152, 22);
            this.ToolStripReconnect.Text = "재접속";
            this.ToolStripReconnect.Click += new System.EventHandler(this.OnReconnect);
            // 
            // 카메라보기ToolStripMenuItem
            // 
            this.카메라보기ToolStripMenuItem.Name = "카메라보기ToolStripMenuItem";
            this.카메라보기ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.카메라보기ToolStripMenuItem.Text = "카메라 보기";
            // 
            // RTU_Control_ToolStripMenuItem
            // 
            this.RTU_Control_ToolStripMenuItem.Name = "RTU_Control_ToolStripMenuItem";
            this.RTU_Control_ToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.RTU_Control_ToolStripMenuItem.Text = "RTU_Control";
            this.RTU_Control_ToolStripMenuItem.Click += new System.EventHandler(this.RTU_Control_ToolStripMenuItem_Click);
            // 
            // FrmBIT
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(337, 371);
            this.ContextMenuStrip = this.contextMenuStrip;
            this.Controls.Add(this.axWindowsMediaPlayer);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmBIT";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FrmBIT";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FrmBIT_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Timer tickTimer;
        private System.Windows.Forms.Label label1;
        private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ToolStripExit;
        private System.Windows.Forms.ToolStripMenuItem ToolStripReconnect;
        private System.Windows.Forms.ToolStripMenuItem 카메라보기ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem RTU_Control_ToolStripMenuItem;
    }
}