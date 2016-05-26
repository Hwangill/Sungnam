////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;

namespace MarqueControl.Designer
{
    internal class SuperMarqueDesigner : ControlDesigner
    {
        #region Constructor

        public SuperMarqueDesigner()
        {
            AutoResizeHandles = false;
        }

        #endregion

        #region Overrides

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
                SuperMarqueDesignerActionList designerActionList = new SuperMarqueDesignerActionList(this);
                actionListCollection.Add(designerActionList);
                return actionListCollection;
            }
        }

        ///<summary>
        ///Initializes the designer with the specified component.
        ///</summary>
        ///
        ///<param name="component">The <see cref="T:System.ComponentModel.IComponent"></see> to associate with the designer. </param>
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            AutoResizeHandles = true;
        }

        #endregion

        #region Properties

        internal IComponentChangeService ComponentChangeService
        {
            get { return (GetService(typeof (IComponentChangeService)) as IComponentChangeService); }
        }
        internal IDesignerHost DesignerHost
        {
            get { return (GetService(typeof (IDesignerHost)) as IDesignerHost); }
        }
        internal IMenuCommandService MenuCommandService
        {
            get { return (GetService(typeof (IMenuCommandService)) as IMenuCommandService); }
        }
        internal ISelectionService SelectionService
        {
            get { return (GetService(typeof (ISelectionService)) as ISelectionService); }
        }

        #endregion
    }
}