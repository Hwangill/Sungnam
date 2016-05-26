////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
namespace MarqueControl.Controls
{
    partial class SuperMarquee
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
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].Dispose();
            }
            tmrRefresh.Dispose();
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tmrRefresh = new System.Windows.Forms.Timer(this.components);
            this.ttMain = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // tmrRefresh
            // 
            this.tmrRefresh.Enabled = true;
            this.tmrRefresh.Interval = 20;
            this.tmrRefresh.Tick += new System.EventHandler(this.OnTimerTick);
            // 
            // ttMain
            // 
            this.ttMain.AutomaticDelay = 100;
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.ToolTip ttMain;
    }
}
