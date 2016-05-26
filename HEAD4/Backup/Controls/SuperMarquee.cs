////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using MarqueControl.Attributes;
using MarqueControl.Collection;
using MarqueControl.Designer;
using MarqueControl.Entity;
using MarqueControl.Enums;
using MarqueControl.EventArguments;

namespace MarqueControl.Controls
{
    /// <summary>
    /// Control featuring marque and has easy to use designer.
    /// </summary>
    [Designer(typeof(SuperMarqueDesigner))]
    [DefaultEvent("ItemClicked")]
    [DefaultProperty("Elements")]
    public partial class SuperMarquee : Control
    {

        #region Events

        /// <summary>
        /// Item is clicked.
        /// </summary>
        [Category("Text element events")]
        [Description("Item is clicked.")]
        public event EventHandler<ItemClickEventArgs> ItemClicked;
        /// <summary>
        /// Item was double clicked.
        /// </summary>
        [Category("Text element events")]
        [Description("Item was double clicked.")]
        public event EventHandler<ItemClickEventArgs> ItemDoubleClicked;
        /// <summary>
        /// Lap was completed.
        /// </summary>
        [Category("Lap events")]
        [Description("Lap was completed.")]
        public event EventHandler LapCompleted;
        /// <summary>
        /// Lap was completed.
        /// </summary>
        [Category("ToolTip events")]
        [Description("Before ToolTip show.")]
        public event GenericCancelEventHandler<ToolTipData> BeforeToolTip;

        #endregion

        #region Private Fields

        private readonly GenericCollection<TextElement> elements = new GenericCollection<TextElement>();
        private bool autoRewind;
        private Rectangle bound;
        private int firstElementWidth;
        private int firstIndex;
        private bool hover;
        private bool hoverStop;
        private ImageList imageList;
        private int maxHeight = 0;
        private int offset = int.MinValue;
        private Timer tmrRefresh;
        private Color stripColor;
        private bool showStrip;
        private int tmpOffset;
        private bool lapComplete;
        private bool showing;
        private bool autoToolTip = true;

        #endregion

        #region Constructor

        /// <summary>
        /// Create instance of the <see cref="SuperMarquee"/>
        /// </summary>
        public SuperMarquee()
        {
            InitializeComponent();
            MarqueeSpeed = 900;
            autoRewind = true;
            hoverStop = true;
            stripColor = Color.Transparent;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            elements.Removed += OnRemoved;
            elements.Inserted += OnInserted;
        }

        #endregion

        #region Overrides

        ///<summary>
        ///Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event.
        ///</summary>
        ///
        ///<param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data. </param>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (firstIndex >= 0 && firstIndex < elements.Count)
            {
                using (Graphics g = e.Graphics)
                {
                    if (showStrip)
                    {
                        g.FillRectangle(new SolidBrush(stripColor), bound);
                    }
                    Point start = new Point(bound.X, Height/2);
                    firstElementWidth = (int) elements[firstIndex].GetSize().Width;
                    int i = firstIndex;
                    if(firstIndex == 0 && autoRewind && lapComplete)
                    {
                        Point buffStart = new Point(bound.X, Height / 2);
                        elements[Elements.Count-1].DrawElement(g, ref buffStart, tmpOffset);
                    }
                    for (; i < elements.Count; i++)
                    {
                        elements[i].DrawElement(g, ref start, offset);
                        if (start.X - offset > Width)
                        {
                            break;
                        }
                    }
                }
            }
        }

        ///<summary>
        ///Raises the <see cref="E:System.Windows.Forms.Control.MouseEnter"></see> event.
        ///</summary>
        ///
        ///<param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data. </param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            hover = true;
        }

        ///<summary>
        ///Raises the <see cref="E:System.Windows.Forms.Control.MouseLeave"></see> event.
        ///</summary>
        ///
        ///<param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data. </param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hover = false;
            if (showing && autoToolTip)
            {
                ttMain.Hide(this);
                showing = false;
            }
        }

        ///<summary>
        ///Raises the <see cref="E:System.Windows.Forms.Control.SizeChanged"></see> event.
        ///</summary>
        ///
        ///<param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data. </param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            bound = new Rectangle(0, (Height/2) - ((maxHeight + 4)/2), Width, maxHeight + 4);
            if (Width != 0 && offset == int.MinValue)
            {
                offset = -Width;
            }
        }

        ///<summary>
        ///Raises the <see cref="E:System.Windows.Forms.Control.MouseClick"></see> event.
        ///</summary>
        ///
        ///<param name="e">An <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data. </param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            HitTestInfo test = HitTest(e.Location);
            if (test != null && test.Area != HitTestArea.None)
            {
                if (test.Area == HitTestArea.Item && this[test.Index].IsLink)
                {
                    OnItemClicked(test.Index);
                }
            }
        }

        ///<summary>
        ///Raises the <see cref="E:System.Windows.Forms.Control.DoubleClick"></see> event.
        ///</summary>
        ///
        ///<param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data. </param>
        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
            HitTestInfo test = HitTest(PointToClient(MousePosition));
            if (test != null && test.Area != HitTestArea.None)
            {
                if (test.Area == HitTestArea.Item && this[test.Index].IsLink)
                {
                    OnItemDoubleClicked(test.Index);
                }
            }
        }

        ///<summary>
        ///Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"></see> event.
        ///</summary>
        ///
        ///<param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data. </param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            HitTestInfo test = HitTest(e.Location);
            if (test != null && test.Area != HitTestArea.None)
            {
                if (test.Area == HitTestArea.Item && !showing && autoToolTip)
                {
                    showing = true;
                    Point loc = new Point(this[test.Index].TextRect.X, this[test.Index].TextRect.Y + this[test.Index].Font.Height);
                    ToolTipData data = new ToolTipData(this[test.Index].ToolTipText, test.Index, loc);
                    GenericCancelEventArgs<ToolTipData> args = new GenericCancelEventArgs<ToolTipData>(data, false);
                    OnBeforeToolTip(args);
                    if(!args.Cancel)
                    {
                        ttMain.Show(args.Value.ToolTipText, this, data.Location);
                    }
                }

                if (test.Area == HitTestArea.Item && this[test.Index].IsLink)
                {
                    Cursor = CursorHelper.NormalCursor;
                }
                else
                {
                    Cursor = Cursors.Default;
                    if (showing && autoToolTip)
                    {
                        ttMain.Hide(this);
                        showing = false;
                    }
                }
            }
        }

        ///<summary>
        ///Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"></see> event.
        ///</summary>
        ///
        ///<param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data. </param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            HitTestInfo test = HitTest(e.Location);
            if (test != null && test.Area != HitTestArea.None)
            {
                if (test.Area == HitTestArea.Item && this[test.Index].IsLink)
                {
                    Cursor = CursorHelper.PressedCursor;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        #endregion

        #region Event Handler

        /// <summary>
        /// Handler for timer Tick.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Agruments attached with the event.</param>
        private void OnTimerTick(object sender, EventArgs e)
        {
            if (hoverStop && hover)
            {
                return;
            }
            if (firstIndex >= elements.Count)
            {
                return;
            }
            int buffer = elements[firstIndex].LeftRect.Width + elements[firstIndex].RightRect.Width + 8;



            if (firstElementWidth >= offset - buffer)
            {
                offset++;
                tmpOffset++;
            }
            else
            {
                if(elements.Count > firstIndex && firstIndex >= 0 && firstIndex < elements.Count - 1)
                {
                    int shift;
                    firstIndex++;
                    if (elements[firstIndex].LeftImageIndex != -1 && elements[firstIndex].LeftImageIndex < imageList.Images.Count && elements[firstIndex - 1].RightImageIndex != -1 && elements[firstIndex - 1].RightImageIndex < imageList.Images.Count)
                    {
                        shift = -elements[firstIndex - 1].LeftRect.Width + elements[firstIndex].RightRect.Width + 2;
                    }
                    else if ((elements[firstIndex - 1].RightImageIndex != -1 && elements[firstIndex - 1].RightImageIndex < imageList.Images.Count) && (elements[firstIndex].LeftImageIndex == -1 || elements[firstIndex].LeftImageIndex > imageList.Images.Count))
                    {
                        shift = 3;
                    }
                    else if ((elements[firstIndex - 1].RightImageIndex != -1 || elements[firstIndex - 1].RightImageIndex > imageList.Images.Count))
                    {
                        shift = 3;
                    }
                    else
                    {
                        shift = 9;
                    }
                    offset = shift;
                }
                if (elements.Count > 0 && firstIndex == 0)
                {
                    if (elements[elements.Count - 1].LeftImageIndex != -1 && elements[elements.Count - 1].LeftImageIndex < imageList.Images.Count)
                    {
                        tmpOffset = 2;
                    }
                    else
                    {
                        tmpOffset = 9;
                    }
                }
            }
            if (autoRewind && firstIndex >= elements.Count - 1)
            {
                firstIndex = 0;
                offset = -Width;
                lapComplete = true;

                if (lapComplete)
                {
                    if (elements[elements.Count - 1].LeftImageIndex != -1 && elements[elements.Count - 1].LeftImageIndex < imageList.Images.Count)
                    {
                        tmpOffset = 2;
                    }
                    else
                    {
                        tmpOffset = 9;
                    }
                }
            }

            if (firstIndex == 0 && autoRewind && lapComplete)
            {
                if (tmpOffset == elements[Elements.Count - 1].TextRect.Width)
                {
                    OnLapCompleted();
                }
            }
            Invalidate();
        }

        /// <summary>
        /// Handler for <see cref="TextElement"/> removed from the collection.
        /// </summary>
        /// <param name="index">Index of the <see cref="TextElement"/></param>
        /// <param name="value">Value of the <see cref="TextElement"/> </param>
        private void OnRemoved(int index, TextElement value)
        {
            maxHeight = GetMaxHeight();
        }

        /// <summary>
        /// Handler for <see cref="TextElement"/> being inserted in the collection.
        /// </summary>
        /// <param name="index">Index of the <see cref="TextElement"/></param>
        /// <param name="value">Value of the <see cref="TextElement"/> </param>
        private void OnInserted(int index, TextElement value)
        {
            if (string.IsNullOrEmpty(value.Text))
            {
                value.Text = GetDefaultText();
            }
            value.SetParent(this);
            if (DesignMode)
            {
                Container.Add(value);
            }
            maxHeight = GetMaxHeight();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the marquee.
        /// </summary>
        public void StartMarquee()
        {
            tmrRefresh.Start();
        }

        /// <summary>
        /// Stop the marquee.
        /// </summary>
        public void StopMarquee()
        {
            tmrRefresh.Stop();
        }

        /// <summary>
        /// Reset the marquee.
        /// </summary>
        public void ResetMorquee()
        {
            offset = -Width;
            firstIndex = 0;
        }

        /// <summary>
        /// Performs the HitTest.
        /// </summary>
        /// <param name="p">Point at which HitTest will be performed.</param>
        /// <returns>returm <see cref="HitTestInfo"/> object containing HitTest information.</returns>
        public HitTestInfo HitTest(Point p)
        {
            HitTestInfo info = new HitTestInfo();
            info.Area = HitTestArea.None;
            info.Point = p;
            info.Index = -1;
            for (int i = firstIndex; i < elements.Count; i++)
            {
                if (elements[i].TextRect.Contains(p))
                {
                    info.Area = HitTestArea.Item;
                    info.Index = i;
                    break;
                }
                else if (elements[i].LeftRect.Contains(p))
                {
                    info.Area = HitTestArea.LeftImage;
                    info.Index = i;
                    break;
                }
                else if (elements[i].RightRect.Contains(p))
                {
                    info.Area = HitTestArea.RightImage;
                    info.Index = i;
                    break;
                }
            }
            if (info.Area == HitTestArea.None)
            {
                if (bound.Contains(p))
                {
                    info.Area = HitTestArea.Strip;
                }
                else if (ClientRectangle.Contains(p))
                {
                    info.Area = HitTestArea.Control;
                }
            }
            return info;
        }

        #endregion

        #region Public Property

        /// <summary>
        /// Gets or sets that marquee will be running or not if mouse hover is there.
        /// </summary>
        [Category("Behavior")]
        public bool HoverStop
        {
            get { return hoverStop; }
            set
            {
                if (hoverStop != value)
                {
                    hoverStop = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets on completion of one round of marquee it will start again or not.
        /// </summary>
        /// <value>SuperMarquee will be auto rewined or not.</value>
        [Category("Behavior")]
        public bool AutoRewind
        {
            get { return autoRewind; }
            set
            {
                if (autoRewind != value)
                {
                    autoRewind = value;
                }
            }
        }

        /// <summary>
        /// Gets list of the <see cref="TextElement"/> associated with the control.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category("Collection")]
        public GenericCollection<TextElement> Elements
        {
            get { return elements; }
        }

        /// <summary>
        /// Gets the <see cref="TextElement"/> at the specified index.
        /// </summary>
        /// <param name="index">Index of the <see cref="TextElement"/> to be obtained.</param>
        /// <returns><see cref="TextElement"/> at the specified index.</returns>
        [Category("Collection")]
        public TextElement this[int index]
        {
            get
            {
                if (index < 0)
                {
                    throw new IndexOutOfRangeException("Index specified should be less than the size of collection.");
                }
                if (index > elements.Count)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                return elements[index];
            }
        }

        /// <summary>
        /// Gets or sets whether marquee is running or not.
        /// </summary>
        /// <value>SuperMarquee is running or not.</value>
        [Category("Behavior")]
        public bool Running
        {
            get { return tmrRefresh.Enabled; }
            set { tmrRefresh.Enabled = value; }
        }

        /// <summary>
        /// Gets or sets whether tool tip will be shown autometically..
        /// </summary>
        /// <value>ToolTip will be shown or not.</value>
        [Category("Behavior")]
        public bool AutoToolTip
        {
            get { return autoToolTip; }
            set { autoToolTip = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Windows.Forms.ImageList"/> associated with the component.
        /// </summary>
        /// <value>ImageList associated with the component.</value>
        [Category("Appearance")]
        public ImageList ImageList
        {
            get { return imageList; }
            set { if(imageList != value){ imageList = value;} }
        }

        /// <summary>
        /// Gets or sets the Color of the stripe. Enable <see cref="ShowStrip"/> for displaying the strip.
        /// </summary>or of the supermarquee strip.
        /// <value>Col.</value>
        [Category("Appearance")]
        public Color StripColor
        {
            get { return stripColor; }
            set { if(stripColor != value){ stripColor = value;} }
        }

        /// <summary>
        /// Gets or sets that strip will be shown or not.
        /// </summary>
        [Category("Appearance")]
        public bool ShowStrip
        {
            get { return showStrip; }
            set
            {
                if (showStrip != value)
                {
                    showStrip = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Speed of the marquee. Maximum speed is 999 and minimum speed is 1.
        /// </summary>
        [Editor(typeof(RangeEditor), typeof(UITypeEditor))]
        [MinMax(1, 999)]
        [Category("Behavior")]
        public int MarqueeSpeed
        {
            get { return 1000 - tmrRefresh.Interval; }
            set
            {
                if ((1000 - tmrRefresh.Interval) != value)
                {
                    if (1000 - value < 1)
                    {
                        value = 999;
                    }
                    if (1000 - value > 999)
                    {
                        value = 1;
                    }
                    tmrRefresh.Interval = 1000 - value;
                    
                    Invalidate();
                }
            }
        }
        /// <summary>
        /// Not relevent to the control
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new bool Enabled
        {
            get { return base.Enabled; }
            set { base.Enabled = value; }
        }

        /// <summary>
        /// Not relevent to the control
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;
            }
        }

        /// <summary>
        /// Not relevent to the control
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public new Color ForeColor
        {
            get
            {
                return base.ForeColor;
            }
            set
            {
                base.ForeColor = value;
            }
        }



        #endregion

        #region Virtual Methods

        /// <summary>
        /// Fires <see cref="ItemDoubleClicked"/> event.
        /// </summary>
        /// <param name="index">Index of the <see cref="TextElement"/></param>
        protected virtual void OnItemDoubleClicked(int index)
        {
            if (ItemDoubleClicked != null)
            {
                ItemDoubleClicked(this, new ItemClickEventArgs(index));
            }
        }

        /// <summary>
        /// Fires <see cref="ItemClicked"/> event.
        /// </summary>
        /// <param name="index">Index of the <see cref="TextElement"/></param>
        protected virtual void OnItemClicked(int index)
        {
            if (ItemClicked != null)
            {
                ItemClicked(this, new ItemClickEventArgs(index));
            }
        }

        /// <summary>
        /// Fires <see cref="LapCompleted"/> event.
        /// </summary>
        protected virtual void OnLapCompleted()
        {
            if (LapCompleted != null)
            {
                LapCompleted(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Fires event BeforeToolTip.
        /// </summary>
        /// <param name="args">Event data of <see cref="BeforeToolTip"/></param>
        protected virtual void OnBeforeToolTip(GenericCancelEventArgs<ToolTipData> args)
        {
            if(BeforeToolTip != null)
            {
                BeforeToolTip(this, args);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the default text for the newly added <see cref="TextElement"/> object
        /// </summary>
        /// <returns>Default text for added <see cref="TextElement"/> object </returns>
        private string GetDefaultText()
        {
            int count = 1;
            string defaultText = "Element" + 1;
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Text.Equals(defaultText, StringComparison.CurrentCultureIgnoreCase))
                {
                    count++;
                    defaultText = "Element" + (count);
                    i = 0;
                }
            }
            return defaultText;
        }

        /// <summary>
        /// Gets the maximum height of the stripe.
        /// </summary>
        /// <returns></returns>
        private int GetMaxHeight()
        {
            int currMax = 0;
            for (int i = 0; i < Elements.Count; i++)
            {
                currMax = Math.Max(currMax, (int) this[i].GetSize().Height);
            }
            return currMax;
        }

        #endregion
    }
}