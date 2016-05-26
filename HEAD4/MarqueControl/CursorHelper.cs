////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System.Windows.Forms;

namespace MarqueControl
{
    internal static class CursorHelper
    {
        static CursorHelper()
        {
            GetCursor();
        }

        public static Cursor PressedCursor
        {
            get { return Cursors.Hand; }
        }
        public static Cursor NormalCursor
        {
            get { return Cursors.Hand; }
        }
        private static void GetCursor()
        {
        }
    }
}
