////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using MarqueControl.Attributes;
using MarqueControl.Collection;
using MarqueControl.Controls;
using MarqueControl.Designer;
using MarqueControl.Entity;

namespace MarqueControl.Designer
{
    internal class SuperMarqueDesignerActionList : DesignerActionList
    {
        #region Constructor

        public SuperMarqueDesignerActionList(IDesigner designer)
            : base(designer.Component)
        {
            SuperMarquee marquee = designer.Component as SuperMarquee;
            if (marquee != null)
            {
                marquee.Elements.Removed += delegate
                                                {
                                                    marquee.Invalidate();
                                                    RefreshDisplay();
                                                };
                marquee.Elements.Inserted += delegate
                                                 {
                                                     marquee.Invalidate();
                                                     RefreshDisplay();
                                                 };
                marquee.Elements.Cleared += delegate
                                                {
                                                    marquee.Invalidate();
                                                    RefreshDisplay();
                                                };
                marquee.Elements.Changed += delegate
                                                {
                                                    marquee.Invalidate();
                                                    RefreshDisplay();
                                                };
            }
        }

        #endregion

        #region Overrides

        ///<summary>
        ///Gets or sets a value indicating whether the smart tag panel should automatically be displayed when it is created.
        ///</summary>
        ///
        ///<returns>
        ///true if the panel should be shown when the owning component is created; otherwise, false. The default is false.
        ///</returns>
        ///
        public override bool AutoShow
        {
            get { return true; }
            set { base.AutoShow = value; }
        }

        ///<summary>
        ///Returns the collection of <see cref="T:System.ComponentModel.Design.DesignerActionItem"></see> objects contained in the list.
        ///</summary>
        ///
        ///<returns>
        ///A <see cref="T:System.ComponentModel.Design.DesignerActionItem"></see> array that contains the items in this list.
        ///</returns>
        ///
        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionHeaderItem("Appearance", "Appearance"));
            items.Add(new DesignerActionMethodItem(this, "Reset", "Reset", "Appearance", true));
            items.Add(new DesignerActionMethodItem(this, "SetStripColor", "Choose Strip Color", "Appearance", true));
            items.Add(new DesignerActionMethodItem(this, "SetBackColor", "Choose Back Color", "Appearance", true));
            items.Add(new DesignerActionPropertyItem("BackgroundImage", "BackgroundImage", "Appearance"));
            items.Add(new DesignerActionPropertyItem("BackgroundImageLayout", "BackgroundImageLayout", "Appearance"));
            items.Add(new DesignerActionPropertyItem("ImageList", "ImageList", "Appearance"));
            if (SuperMarquee.ImageList == null)
            {
                items.Add(new DesignerActionMethodItem(this, "AddImageList", "Add ImageList", "Appearance", true));
            }
            else
            {
                items.Add(new DesignerActionMethodItem(this, "RemoveImageList", "Remove ImageList", "Appearance", true));
            }

            items.Add(new DesignerActionHeaderItem("Collection", "Collection"));
            items.Add(new DesignerActionPropertyItem("Elements", "Element Editor", "Collection"));
            items.Add(new DesignerActionMethodItem(this, "AddElement", "Add Element", "Collection", true));
            if (SuperMarquee.Elements.Count > 0)
            {
                items.Add(new DesignerActionMethodItem(this, "ClearElement", "Clear Element", "Collection", true));
            }

            items.Add(new DesignerActionHeaderItem("Behavior", "Behavior"));
            items.Add(new DesignerActionMethodItem(this, "OnDock", GetDockText(), "Behavior", true));
            items.Add(new DesignerActionMethodItem(this, "StartStop", SuperMarquee.Running ? "Stop marquee" : "Start marquee", "Behavior", true));
            items.Add(new DesignerActionPropertyItem("HoverStop", "Hover Stop", "Behavior"));
            items.Add(new DesignerActionPropertyItem("AutoRewind", "Auto Rewind", "Behavior"));
            items.Add(new DesignerActionPropertyItem("MarqueeSpeed", "Marquee Speed", "Behavior"));
            items.Add(new DesignerActionMethodItem(this, "ShowStrip", SuperMarquee.ShowStrip ? "Hide Strip" : "Show Strip", "Behavior", true));
            return items;
        }

        #endregion

        #region Property

        private SuperMarquee SuperMarquee
        {
            get { return (SuperMarquee)Component; }
        }

        #endregion

        #region Helper Methods

        private void RefreshDisplay()
        {
            DesignerActionUIService service = (DesignerActionUIService)GetService(typeof(DesignerActionUIService));
            if (service != null)
            {
                service.Refresh(SuperMarquee);
            }
        }

        private static Color GetColor(Color defColor)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = defColor;
            dlg.FullOpen = true;
            dlg.SolidColorOnly = false;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                return dlg.Color;
            }
            return Color.Empty;
        }

        private string GetDockText()
        {
            if (SuperMarquee.Dock == DockStyle.None)
            {
                return "Dock in parent container";
            }
            return "Undock in parent container";
        }

        #endregion

        #region Designer Properties

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public GenericCollection<TextElement> Elements
        {
            get { return SuperMarquee.Elements; }
        }
        public ImageList ImageList
        {
            get { return SuperMarquee.ImageList; }
            set
            {
                if (SuperMarquee.ImageList != value)
                {
                    SuperMarquee.ImageList = value;
                    SuperMarquee.Invalidate();
                }
            }
        }
        public bool HoverStop
        {
            get { return SuperMarquee.HoverStop; }
            set
            {
                if (SuperMarquee.HoverStop != value)
                {
                    SuperMarquee.HoverStop = value;
                    SuperMarquee.Invalidate();
                }
            }
        }

        public bool AutoRewind
        {
            get { return SuperMarquee.AutoRewind; }
            set
            {
                if (SuperMarquee.AutoRewind != value)
                {
                    SuperMarquee.AutoRewind = value;
                    SuperMarquee.Invalidate();
                }
            }
        }

        public ImageLayout BackgroundImageLayout
        {
            get { return SuperMarquee.BackgroundImageLayout; }
            set
            {
                if (SuperMarquee.BackgroundImageLayout != value)
                {
                    SuperMarquee.BackgroundImageLayout = value;
                    SuperMarquee.Invalidate();
                }
            }
        }

        public Image BackgroundImage
        {
            get { return SuperMarquee.BackgroundImage; }
            set
            {
                if (SuperMarquee.BackgroundImage != value)
                {
                    SuperMarquee.BackgroundImage = value;
                    SuperMarquee.Invalidate();
                }
            }
        }

        [Editor(typeof(RangeEditor), typeof(UITypeEditor))]
        [MinMax(1, 999)]
        public int MarqueeSpeed
        {
            get { return SuperMarquee.MarqueeSpeed; }
            set
            {
                if (SuperMarquee.MarqueeSpeed != value)
                {
                    SuperMarquee.MarqueeSpeed = value;
                    SuperMarquee.Invalidate();
                }
            }
        }

        #endregion

        #region Designer Methods

        protected virtual void ClearElement()
        {
            SuperMarquee.Elements.Clear();
            SuperMarquee.Invalidate();
            RefreshDisplay();
        }

        protected virtual void AddElement()
        {
            SuperMarquee.Elements.Add(new TextElement());
            SuperMarquee.Invalidate();
            RefreshDisplay();
        }

        protected virtual void AddImageList()
        {
            ImageList imageList = GetExistingImageList();
            if(imageList == null)
            {
                imageList = CreateNewImagelist();
            }
            SuperMarquee.ImageList = imageList;
            SuperMarquee.Invalidate();
            RefreshDisplay();
        }

        private ImageList CreateNewImagelist()
        {
            IContainer container = null;
            Form form = SuperMarquee.FindForm();
            if (form != null)
            {
                container = form.Container;
            }
            else
            {
                if (SuperMarquee.Parent != null)
                {
                    container = SuperMarquee.Parent.Container;
                }
            }
            if (container == null)
            {
                return null;
            }
            ImageList imageList = new ImageList(container);
            return imageList;
        }

        private ImageList GetExistingImageList()
        {
            IContainer container = null;
            Form form = SuperMarquee.FindForm();
            if (form != null)
            {
                container = form.Container;
            }
            else
            {
                if (SuperMarquee.Parent != null)
                {
                    container = SuperMarquee.Parent.Container;
                }
            }
            if(container == null)
            {
                return null;
            }
            ImageList imageList = null;
            for (int i = 0; i < container.Components.Count; i++)
            {
                if (container.Components[i] is ImageList)
                {
                    imageList = (ImageList)container.Components[i];
                    break;
                }
            }
            return imageList;
        }

        protected virtual void RemoveImageList()
        {
            SuperMarquee.ImageList = null;
            SuperMarquee.Invalidate();
            RefreshDisplay();
        }

        protected virtual void ShowStrip()
        {
            SuperMarquee.ShowStrip = !SuperMarquee.ShowStrip;
            SuperMarquee.Invalidate();
            RefreshDisplay();
        }

        protected virtual void OnDock()
        {
            if (SuperMarquee.Dock == DockStyle.Fill)
            {
                SuperMarquee.Dock = DockStyle.None;
            }
            else
            {
                SuperMarquee.Dock = DockStyle.Fill;
            }
            RefreshDisplay();
        }

        protected virtual void SetStripColor()
        {
            Color c = GetColor(SuperMarquee.StripColor);
            if (!c.IsEmpty)
            {
                SuperMarquee.StripColor = c;
                SuperMarquee.Invalidate();
            }
        }

        protected virtual void SetBackColor()
        {
            Color c = GetColor(SuperMarquee.BackColor);
            if (!c.IsEmpty)
            {
                SuperMarquee.BackColor = c;
                SuperMarquee.Invalidate();
            }
        }

        protected virtual void Reset()
        {
            SuperMarquee.Reset();
            SuperMarquee.Invalidate();
            RefreshDisplay();
        }

        protected virtual void StartStop()
        {
            SuperMarquee.Running = !SuperMarquee.Running;
            RefreshDisplay();
        }

        #endregion
    }
}
