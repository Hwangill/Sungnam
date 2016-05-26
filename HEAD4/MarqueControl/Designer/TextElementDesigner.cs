////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design.Behavior;
using MarqueControl.Entity;

namespace MarqueControl.Designer
{
    internal class TextElementDesigner : ComponentDesigner
    {
        private TextElement TextElement
        {
            get { return (TextElement)Component; }
        }

        ///<summary>
        ///Gets the design-time action lists supported by the component associated with the designer.
        ///</summary>
        ///
        ///<returns>
        ///The design-time action lists supported by the component associated with the designer.
        ///</returns>
        ///
        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection actionListCollection = new DesignerActionListCollection();
                TextElementDesignerActionList designerActionList = new TextElementDesignerActionList(this);
                actionListCollection.Add(designerActionList);
                return actionListCollection;
            }
        }

        protected override IComponent ParentComponent
        {
            get
            {
                return TextElement.Parent;
            }
        }

        internal BehaviorService BehaviorService
        {
            get
            {
                BehaviorService service = GetService(typeof(BehaviorService)) as BehaviorService;
                if (service != null)
                {
                    service.AdornerWindowGraphics.Clip = new Region(new Rectangle(0,0 , 10, 10));
                }
                return service;
            }
        }
    }

    internal class TextElementDesignerActionList : DesignerActionList
    {
        private TextElement TextElement
        {
            get
            {
                return (TextElement) Component;
            }
        }

        #region Constructor

        public TextElementDesignerActionList(IDesigner designer)
            : base(designer.Component)
        {
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
            items.Add(new DesignerActionMethodItem(this, "ForeColor", "ForeColor", "Appearance", true));
            items.Add(new DesignerActionPropertyItem("Font", "Font", "Appearance"));

            items.Add(new DesignerActionHeaderItem("Behavior", "Behavior"));
            items.Add(new DesignerActionPropertyItem("Text", "Text", "Behavior"));
            items.Add(new DesignerActionPropertyItem("IsLink", "Is Link", "Behavior"));
            return items;
        }

        #endregion

        protected void Reset()
        {
            TextElement.Reset();
            TextElement.Parent.Invalidate();
        }

        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Text
        {
            get { return TextElement.Text; }
            set
            {
                if (!TextElement.Text.Equals(value))
                {
                    TextElement.Text = value;
                    TextElement.Parent.Invalidate();
                }
            }
        }

        public void ForeColor()
        {
            Color c = GetColor(TextElement.ForeColor);
            if (!c.IsEmpty)
            {
                TextElement.ForeColor = c;
                TextElement.Parent.Invalidate();
            }
        }
        public bool IsLink
        {
            get { return TextElement.IsLink; }
            set
            {
                if (!TextElement.IsLink.Equals(value))
                {
                    TextElement.IsLink = value;
                    TextElement.Parent.Invalidate();
                }
            }
        }
        public Font Font
        {
            get { return TextElement.Font; }
            set
            {
                if (!TextElement.Font.Equals(value))
                {
                    TextElement.Font = value;
                    TextElement.Parent.Invalidate();
                }
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
    }
}
