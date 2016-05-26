////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using MarqueControl.Attributes;
using MarqueControl.Controls;
using MarqueControl.Designer;

namespace MarqueControl.Entity
{
    /// <summary>
    /// Component to be used as display in marquee control.
    /// </summary>
    [Designer(typeof(TextElementDesigner))]
    [ToolboxItem(false)]
    public partial class TextElement : Component
    {
        #region Private Fields

        private Color foreColor;
        private Font font;
        private bool isLink;
        private int leftImageIndex;
        private Rectangle leftRect = new Rectangle(0, 0, 0, 0);
        private SuperMarquee parent;
        private int rightImageIndex;
        private Rectangle rightRect = new Rectangle(0, 0, 0, 0);
        private object tag;
        private string text;
        private Rectangle textRect = new Rectangle(0, 0, 0, 0);
        private string toolTipText;
        private StringFormat elf;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public TextElement()
        {
            text = string.Empty;
            foreColor = SystemColors.ControlText;
            isLink = true;
            font = new Font("Microsoft Sans Serif", 8.25F);
            leftImageIndex = -1;
            rightImageIndex = -1;
            toolTipText = string.Empty;
            elf = new StringFormat();
            elf.Alignment = StringAlignment.Center;
            elf.LineAlignment = StringAlignment.Center;
            elf.Trimming = StringTrimming.None;
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="text">Text of the element.</param>
        public TextElement(string text)
        {
            this.text = text;
            foreColor = SystemColors.ControlText;
            isLink = true;
            font = new Font("Microsoft Sans Serif", 8.25F);
            leftImageIndex = -1;
            rightImageIndex = -1;
            toolTipText = string.Empty;
            elf = new StringFormat();
            elf.Alignment = StringAlignment.Center;
            elf.LineAlignment = StringAlignment.Center;
            elf.Trimming = StringTrimming.None;
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="text">Text of the element.</param>
        /// <param name="isLink">Indicates behaves like a link or not.</param>
        public TextElement(string text, bool isLink)
        {
            this.text = text;
            foreColor = SystemColors.ControlText;
            this.isLink = isLink;
            font = new Font("Microsoft Sans Serif", 8.25F);
            leftImageIndex = -1;
            rightImageIndex = -1;
            toolTipText = string.Empty;
            elf = new StringFormat();
            elf.Alignment = StringAlignment.Center;
            elf.LineAlignment = StringAlignment.Center;
            elf.Trimming = StringTrimming.None;
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="text">Text of the new element.</param>
        /// <param name="foreColor">Text color of the element</param>
        public TextElement(string text, Color foreColor)
        {
            this.text = text;
            this.foreColor = foreColor;
            font = new Font("Microsoft Sans Serif", 8.25F);
            isLink = true;
            leftImageIndex = -1;
            rightImageIndex = -1;
            toolTipText = string.Empty;
            elf = new StringFormat();
            elf.Alignment = StringAlignment.Center;
            elf.LineAlignment = StringAlignment.Center;
            elf.Trimming = StringTrimming.None;
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="text">Text of the new element.</param>
        /// <param name="foreColor">Text color of the element</param>
        /// <param name="isLink">Indicates behaves like a link or not.</param>
        public TextElement(string text, Color foreColor, bool isLink)
        {
            this.text = text;
            this.foreColor = foreColor;
            font = new Font("Microsoft Sans Serif", 8.25F);
            this.isLink = isLink;
            leftImageIndex = -1;
            rightImageIndex = -1;
            toolTipText = string.Empty;
            elf = new StringFormat();
            elf.Alignment = StringAlignment.Center;
            elf.LineAlignment = StringAlignment.Center;
            elf.Trimming = StringTrimming.None;
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="text">Text of the new element.</param>
        /// <param name="foreColor">Text color of the element</param>
        /// <param name="font">F</param>ont of the element.
        public TextElement(string text, Color foreColor, Font font)
        {
            this.text = text;
            this.foreColor = foreColor;
            this.font = font;
            isLink = true;
            leftImageIndex = -1;
            rightImageIndex = -1;
            toolTipText = string.Empty;
            elf = new StringFormat();
            elf.Alignment = StringAlignment.Center;
            elf.LineAlignment = StringAlignment.Center;
            elf.Trimming = StringTrimming.None;
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="text">Text of the new element.</param>
        /// <param name="foreColor">Text color of the element</param>
        /// <param name="font">F</param>ont of the element.
        /// <param name="isLink">Indicates behaves like a link or not.</param>
        public TextElement(string text, Color foreColor, Font font, bool isLink)
        {
            this.text = text;
            this.foreColor = foreColor;
            this.font = font;
            this.isLink = isLink;
            leftImageIndex = -1;
            rightImageIndex = -1;
            toolTipText = string.Empty;
            elf = new StringFormat();
            elf.Alignment = StringAlignment.Center;
            elf.LineAlignment = StringAlignment.Center;
            elf.Trimming = StringTrimming.None;
        }

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="text">Text of the new element.</param>
        /// <param name="leftImageIndex">Left image index</param>
        /// <param name="rightImageIndex">Right image index</param>
        /// <param name="isLink">Indicates behaves like a link or not.</param>
        public TextElement(string text, int leftImageIndex, int rightImageIndex, bool isLink)
        {
            this.text = text;
            foreColor = SystemColors.ControlText;
            font = new Font("Microsoft Sans Serif", 8.25F);
            this.leftImageIndex = leftImageIndex;
            this.rightImageIndex = rightImageIndex;
            this.isLink = isLink;
            toolTipText = string.Empty;
            elf = new StringFormat();
            elf.Alignment = StringAlignment.Center;
            elf.LineAlignment = StringAlignment.Center;
            elf.Trimming = StringTrimming.None;
        }

        #endregion

        #region internal Methods

        internal SizeF GetSize()
        {
            using (Graphics ge = Graphics.FromImage(new Bitmap(10, 10)))
            {
                SizeF sz = ge.MeasureString(text, font);
                int imageFactor = 0;
                int imageHeight = 0;
                if (leftImageIndex >= 0 && parent.ImageList != null && leftImageIndex < parent.ImageList.Images.Count)
                {
                    imageFactor += parent.ImageList.ImageSize.Width;
                    imageHeight = parent.ImageList.ImageSize.Height;
                }
                if (rightImageIndex >= 0 && parent.ImageList != null && rightImageIndex < parent.ImageList.Images.Count)
                {
                    imageFactor += parent.ImageList.ImageSize.Width;
                    imageHeight = parent.ImageList.ImageSize.Height;
                }
                return new SizeF(sz.Width + imageFactor + 8, Math.Max(sz.Height, imageHeight));
            }
        }

        internal void DrawElement(Graphics g, ref Point startPoint, int offset)
        {
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            SizeF size = GetSize();
            if (leftImageIndex >= 0 && parent.ImageList != null && leftImageIndex < parent.ImageList.Images.Count)
            {
                Image leftImage = parent.ImageList.Images[leftImageIndex];
                leftRect = new Rectangle(startPoint.X - offset + 4, (startPoint.Y - (leftImage.Height/2)), leftImage.Width, leftImage.Height);
                g.DrawImage(leftImage, leftRect);
                startPoint.X = startPoint.X + leftImage.Width + 4;
            }
            SizeF textSize = g.MeasureString(text, font);
            textRect = new Rectangle(startPoint.X - offset, (startPoint.Y - ((int) size.Height/2)), (int) size.Width, (int) size.Height);

            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            if (elf.Alignment == StringAlignment.Far)
            {
                textRect.Offset((int)(size.Width - textSize.Width) * 2, 0);
                g.DrawString(text, font, new SolidBrush(foreColor), textRect, elf);
            }
            else
                g.DrawString(text, font, new SolidBrush(foreColor), textRect, elf);
            startPoint.X = (int) (startPoint.X + size.Width);

            if (rightImageIndex >= 0 && parent.ImageList != null && rightImageIndex < parent.ImageList.Images.Count)
            {
                Image rightImage = parent.ImageList.Images[rightImageIndex];
                rightRect = new Rectangle(startPoint.X - offset, (startPoint.Y - (rightImage.Height/2)), rightImage.Width, rightImage.Height);
                g.DrawImage(rightImage, rightRect);
                startPoint.X = startPoint.X + rightImage.Width + 4;
            }
        }

        /// <summary>
        /// Sets the parent of the element.
        /// </summary>
        /// <param name="marquee">Parent control</param>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        internal void SetParent(SuperMarquee marquee)
        {
            parent = marquee;
        }

        #endregion

        #region Public Property

        /// <summary>
        /// Text displayed in this element.
        /// </summary>
        [Description("Text displayed in this element.")]
        [Category("Behavior")]
        [Editor(typeof (MultilineStringEditor), typeof (UITypeEditor))]
        [Localizable(true)]
        public string Text
        {
            get { return text; }
            set { text = value; }
        }
        /// <summary>
        /// Text color of the element.
        /// </summary>
        [Description("Text color of the element.")]
        [Category("Appearance")]
        public Color ForeColor
        {
            get { return foreColor; }
            set { foreColor = value; }
        }
        /// <summary>
        /// Indicates whether the element will be displayed as a link or not.
        /// </summary>
        [Description("Indicates whether the element will be displayed as a link or not.")]
        [Category("Behavior")]
        public bool IsLink
        {
            get { return isLink; }
            set { isLink = value; }
        }
        /// <summary>
        /// Font of this element.
        /// </summary>
        [Description("Font of this element.")]
        [Category("Appearance")]
        public Font Font
        {
            get { return font; }
            set { font = value; }
        }

        public StringFormat StringFormat
        {
            get { return elf; }
            set { elf = value; }
        }
        /// <summary>
        /// User data associated with the element.
        /// </summary>
        [Description("Text displayed in this element.")]
        [Category("Behavior")]
        [Editor(typeof (MultilineStringEditor), typeof (UITypeEditor))]
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }
        /// <summary>
        /// Left image index of the image to be displayed.
        /// </summary>
        [Description("Left image index of the image to be displayed.")]
        [Category("Appearance")]
        [Editor(typeof (ImageListIndexEditor), typeof (UITypeEditor)), ImageProperty("Parent.ImageList"), TypeConverter(typeof (ImageIndexConverter))]
        public int LeftImageIndex
        {
            get { return leftImageIndex; }
            set { leftImageIndex = value; }
        }
        /// <summary>
        /// Right image index of the image to be displayed.
        /// </summary>
        [Description("Right image index of the image to be displayed.")]
        [Category("Appearance")]
        [Editor(typeof (ImageListIndexEditor), typeof (UITypeEditor)), ImageProperty("Parent.ImageList"), TypeConverter(typeof (ImageIndexConverter))]
        public int RightImageIndex
        {
            get { return rightImageIndex; }
            set { rightImageIndex = value; }
        }
        /// <summary>
        /// Parent where this element is added.
        /// </summary>
        [Description("Parent where this element is added.")]
        [Category("Parent")]
        public SuperMarquee Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }
        /// <summary>
        /// Text displayed in this element.
        /// </summary>
        [Description("Text displayed in this element.")]
        [Category("Behavior")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        [Localizable(true)]
        public string ToolTipText
        {
            get { return toolTipText; }
            set { toolTipText = value; }
        }

        #endregion

        #region Internal Property

        internal Rectangle TextRect
        {
            get { return textRect; }
        }
        internal Rectangle LeftRect
        {
            get { return leftRect; }
        }
        internal Rectangle RightRect
        {
            get { return rightRect; }
        }

        #endregion

        #region Overrides

        ///<summary>
        ///Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override string ToString()
        {
            return text;
        }

        ///<summary>
        ///Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        ///</summary>
        ///
        ///<returns>
        ///A hash code for the current <see cref="T:System.Object"></see>.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        ///<summary>
        ///Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Component"></see> and optionally releases the managed resources.
        ///</summary>
        ///
        ///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
        protected override void Dispose(bool disposing)
        {
            text = string.Empty;
            foreColor = Color.Empty;
            isLink = true;
            font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            leftImageIndex = -1;
            rightImageIndex = -1;
            base.Dispose(disposing);
        }

        #endregion
    }
}