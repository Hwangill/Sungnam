////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System.ComponentModel;

namespace MarqueControl.EventArguments
{
    /// <summary>
    /// Generic cancel event handler.
    /// </summary>
    /// <typeparam name="T">Generic value type.</typeparam>
    /// <param name="sender">Source of the event.</param>
    /// <param name="tArgs">Event data associated with the event.</param>
    public delegate void GenericCancelEventHandler<T>(object sender, GenericCancelEventArgs<T> tArgs);

    /// <summary>
    /// Cancel event argument.
    /// </summary>
    /// <typeparam name="T">Generic value type.</typeparam>
    public class GenericCancelEventArgs<T> : CancelEventArgs
    {
        private T value;

        /// <summary>
        /// Create instance for <see cref="GenericCancelEventArgs{T}"/>
        /// </summary>
        /// <param name="value">Event data associated with the event.</param>
        public GenericCancelEventArgs(T value)
            : base(false)
        {
            this.value = value;
        }

        /// <summary>
        /// Create instance for <see cref="GenericCancelEventArgs{T}"/>
        /// </summary>
        /// <param name="value">Event data associated with the event.</param>
        /// <param name="cancel">Perform cancel operation.</param>
        public GenericCancelEventArgs(T value, bool cancel)
            : base(cancel)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets or sets value.
        /// </summary>
        public T Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }
}
