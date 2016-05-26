////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System.ComponentModel;

namespace MarqueControl.EventArguments
{
    /// <summary>
    /// Generic Event Handler.
    /// </summary>
    /// <typeparam name="T">Generic value type.</typeparam>
    public class GenericChangeEventArgs<T> : CancelEventArgs
    {
        private readonly T oldValue;
        private T newValue;

        /// <summary>
        /// Craetes new instance of the <see cref="GenericChangeEventArgs{T}"/>
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        public GenericChangeEventArgs(T oldValue, T newValue)
            : base(false)
        {
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        /// <summary>
        /// Craetes new instance of the <see cref="GenericChangeEventArgs{T}"/>
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <param name="cancel">Perform cancel operation or not.</param>
        public GenericChangeEventArgs(T oldValue, T newValue, bool cancel)
            : base(cancel)
        {
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        /// <summary>
        /// Gets old value.
        /// </summary>
        public T OldValue
        {
            get { return oldValue; }
        }
        /// <summary>
        /// Gets or sets New value
        /// </summary>
        public T NewValue
        {
            get { return newValue; }
            set { newValue = value; }
        }
    }
}
