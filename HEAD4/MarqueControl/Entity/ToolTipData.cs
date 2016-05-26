////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System.Drawing;

namespace MarqueControl.Entity
{
    /// <summary>
    /// Stores data of ToolTip.
    /// </summary>
    public class ToolTipData
    {
        private string toolTipText;
        private readonly int itemIndex;
        private Point location;

        /// <summary>
        /// Create instance of the class.
        /// </summary>
        /// <param name="toolTipText">text associated with ToolTip</param>
        /// <param name="itemIndex">index of the Item over which ToolTip will be displayed</param>
        /// <param name="location">location of ToolTip relative to Control. Do not forget to call Control.PointToClient method.</param>
        public ToolTipData(string toolTipText, int itemIndex, Point location)
        {
            this.toolTipText = toolTipText;
            this.itemIndex = itemIndex;
            this.location = location;
        }
        /// <summary>
        /// Gets or sets text associated with ToolTip
        /// </summary>
        public string ToolTipText
        {
            get { return toolTipText; }
            set { toolTipText = value; }
        }
        /// <summary>
        /// Gets index of the Item over which ToolTip will be displayed.
        /// </summary>
        public int ItemIndex
        {
            get { return itemIndex; }
        }
        /// <summary>
        /// Gets or sets location of ToolTip relative to Control. Do not forget to call Control.PointToClient method.
        /// </summary>
        public Point Location
        {
            get { return location; }
            set { location = value; }
        }

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
            return toolTipText;
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
    }
}
