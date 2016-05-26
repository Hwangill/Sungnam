////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System;

namespace MarqueControl.EventArguments
{
    /// <summary>
    /// Item click argument.
    /// </summary>
    public class ItemClickEventArgs : EventArgs
    {
        private readonly int index;

        /// <summary>
        /// Create  instance for EventAgrs
        /// </summary>
        /// <param name="index"></param>
        public ItemClickEventArgs(int index)
        {
            this.index = index;
        }

        /// <summary>
        /// Index of the element clicked.
        /// </summary>
        public int Index
        {
            get { return index; }
        }
    }
}
