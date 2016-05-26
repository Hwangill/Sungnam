////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System.Drawing;

namespace MarqueControl.Entity
{
    partial class TextElement
    {
        #region Should serialize implementation

        private bool ShouldSerializeForeColor()
        {
            return foreColor != SystemColors.ControlText;
        }

        private bool ShouldSerializeIsLink()
        {
            return !isLink;
        }

        private bool ShouldSerializeFont()
        {
            return !font.Equals(new Font("Microsoft Sans Serif", 8.25F));
        }

        private bool ShouldSerializeTag()
        {
            return tag != null;
        }

        private bool ShouldSerializeLeftImageIndex()
        {
            return leftImageIndex != -1;
        }

        private bool ShouldSerializeRightImageIndex()
        {
            return rightImageIndex != -1;
        }
        private bool ShouldSerializeToolTipText()
        {
            return ToolTipText != string.Empty;
        }
        #endregion

        #region Reset implementation

        /// <summary>
        /// Reset the <see cref="TextElement"/>
        /// </summary>
        public void Reset()
        {
            ResetForeColor();
            ResetIsLink();
            ResetFont();
            ResetTag();
            ResetLeftImageIndex();
            ResetRightImageIndex();
            ResetToolTipText();
        }

        private void ResetForeColor()
        {
            foreColor = SystemColors.ControlText;
        }

        private void ResetIsLink()
        {
            isLink = true;
        }

        private void ResetFont()
        {
            font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point); ;
        }

        private void ResetTag()
        {
            tag = null;
        }

        private void ResetLeftImageIndex()
        {
            leftImageIndex = -1;
        }

        private void ResetRightImageIndex()
        {
            rightImageIndex = -1;
        }

        private void ResetToolTipText()
        {
            ToolTipText = string.Empty;
        }
        #endregion
    }
}
