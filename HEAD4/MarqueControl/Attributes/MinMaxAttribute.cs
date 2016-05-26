////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System;

namespace MarqueControl.Attributes
{
    /// <summary>
    /// Minmimum maximum value attribute.
    /// </summary>
    internal sealed class MinMaxAttribute : Attribute
    {
        public static readonly MinMaxAttribute Default = new MinMaxAttribute(0, 999);
        private readonly int minValue;
        private readonly int maxValue;

        /// <summary>
        /// Create instance of <see cref="MinMaxAttribute"/>.
        /// </summary>
        /// <param name="minValue">Minimum value</param>
        /// <param name="maxValue">Maximum value</param>
        public MinMaxAttribute(int minValue, int maxValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
        /// <summary>
        /// Gets minimum value.
        /// </summary>
        public int MinValue
        {
            get { return minValue; }
        }
        /// <summary>
        /// Gets maximum value.
        /// </summary>
        public int MaxValue
        {
            get { return maxValue; }
        }

        ///<summary>
        ///Returns a value that indicates whether this instance is equal to a specified object.
        ///</summary>
        ///
        ///<returns>
        ///true if obj equals the type and value of this instance; otherwise, false.
        ///</returns>
        ///
        ///<param name="obj">An <see cref="T:System.Object"></see> to compare with this instance or null. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            MinMaxAttribute attribute = obj as MinMaxAttribute;
            if (attribute != null)
            {
                return attribute.minValue.Equals(minValue) && attribute.maxValue.Equals(maxValue);
            }
            return false;
        }

        ///<summary>
        ///Returns the hash code for this instance.
        ///</summary>
        ///
        ///<returns>
        ///A 32-bit signed integer hash code.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        ///<summary>
        ///When overridden in a derived class, indicates whether the value of this instance is the default value for the derived class.
        ///</summary>
        ///
        ///<returns>
        ///true if this instance is the default attribute for the class; otherwise, false.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override bool IsDefaultAttribute()
        {
            return Default.Equals(this);
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
            return "Minimum allowed value : " + minValue + " , Maximum allowed value : " + maxValue;
        }
    }
}
