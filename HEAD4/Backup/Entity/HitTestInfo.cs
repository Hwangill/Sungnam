////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System.Drawing;
using MarqueControl.Controls;
using MarqueControl.Enums;

namespace MarqueControl.Entity
{
    /// <summary>
    /// HitTest information class  for the SuperMarquee control.
    /// </summary>
    public class HitTestInfo
    {
        private HitTestArea area;
        private int index;
        private Point point;

        /// <summary>
        /// Default constructor
        /// </summary>
        internal HitTestInfo()
        {
        }

        /// <summary>
        /// Create instance of <see cref="HitTestInfo"/>. Use this to get the HitTest information.
        /// </summary>
        /// <param name="control"><see cref="SuperMarquee"/> for which HitTest is to be performed.</param>
        /// <param name="testPoint">HitTest Point</param>
        public HitTestInfo(SuperMarquee control, Point testPoint)
        {
            if (control != null)
            {
                HitTestInfo test = control.HitTest(testPoint);
                index = test.index;
                point = test.point;
                area = test.area;
            }
            else
            {
                index = -1;
                point = testPoint;
                area = HitTestArea.None;
            }
        }

        /// <summary>
        /// Index of the <see cref="TextElement"/> if point is above element otherwise -1.
        /// </summary>
        public int Index
        {
            get { return index; }
            internal set { index = value; }
        }

        /// <summary>
        /// HitTest point.
        /// </summary>
        public Point Point
        {
            get { return point; }
            internal set { point = value; }
        }

        /// <summary>
        /// <see cref="HitTestArea"/> at which point is located.
        /// </summary>
        public HitTestArea Area
        {
            get { return area; }
            internal set { area = value; }
        }
    }
}
