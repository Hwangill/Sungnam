////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace MarqueControl.Controls
{
    partial class SuperMarquee
    {
        #region Should serialize implementation

        private bool ShouldSerializeHoverStop()
        {
            return !HoverStop;
        }

        private bool ShouldSerializeAutoRewind()
        {
            return !AutoRewind;
        }

        private bool ShouldSerializeRunning()
        {
            return !Running; ;
        }

        private bool ShouldSerializeImageList()
        {
            return ImageList != null;
        }

        private bool ShouldSerializeStripColor()
        {
            return StripColor != Color.Transparent;
        }

        private bool ShouldSerializeShowStrip()
        {
            return ShowStrip;
        }

        private bool ShouldSerializeMarqueeSpeed()
        {
            return MarqueeSpeed != 900;
        }
        private bool ShouldSerializeAutoToolTip()
        {
            return !AutoToolTip;
        }
        #endregion

        #region Reset implementation

        /// <summary>
        /// Reset the <see cref="SuperMarquee"/>
        /// </summary>
        public void Reset()
        {
            ResetHoverStop();
            ResetAutoRewind();
            ResetRunning();
            ResetImageList();
            ResetStripColor();
            ResetShowStrip();
            ResetMarqueeSpeed();
        }

        private void ResetHoverStop()
        {
            HoverStop = true;
        }

        private void ResetAutoRewind()
        {
            AutoRewind = true;
        }

        private void ResetRunning()
        {
            Running = true; ;
        }

        private void ResetImageList()
        {
            ImageList = null;
        }

        private void ResetStripColor()
        {
            StripColor = Color.Transparent;
        }

        private void ResetShowStrip()
        {
            ShowStrip = false;
        }

        private void ResetMarqueeSpeed()
        {
            MarqueeSpeed = 900;
        }
        private void ResetAutoToolTip()
        {
            AutoToolTip = true;
        }
        #endregion
    }
}
