////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using MarqueControl.EventArguments;

namespace MarqueControl.Collection
{
    /// <summary>
    /// Collection which supports events.
    /// </summary>
    /// <typeparam name="T">Collection element type.</typeparam>
    public class GenericCollection<T> : CollectionBase, IDeserializationCallback, IDisposable, ISerializable
    {
        #region Delegates

        /// <summary>
        /// Delegate Collection changed.
        /// </summary>
        /// <param name="index">Index of collecten being modified.</param>
        /// <param name="value">Modified value.</param>
        public delegate void CollectionChangedHandler(int index, T value);

        /// <summary>
        /// Delegate Collection changing.
        /// </summary>
        /// <param name="index">Index of collecten being modified.</param>
        /// <param name="value">Modified value.</param>
        public delegate void CollectionChangingHandler(int index, GenericCancelEventArgs<T> value);

        /// <summary>
        /// Delegate for collection cleared.
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        public delegate void CollectionClearHandler();

        /// <summary>
        /// Delegate for collection clearing.
        /// </summary>
        /// <param name="value">Cancel event args which contains collection data.</param>
        public delegate void CollectionClearingHandler(GenericCancelEventArgs<GenericCollection<T>> value);

        /// <summary>
        /// Delegate for index being changed.
        /// </summary>
        /// <param name="index">Index being changed.</param>
        /// <param name="oldValue">Old value.</param>
        /// <param name="newValue">New value.</param>
        public delegate void ItemChangeHandler(int index, T oldValue, T newValue);

        /// <summary>
        /// Delegate for Item changing.
        /// </summary>
        /// <param name="index">Index of item which is changing.</param>
        /// <param name="e">Change event argument.</param>
        public delegate void ItemChangingHandler(int index, GenericChangeEventArgs<T> e);

        /// <summary>
        /// Delegate for Item being validated.
        /// </summary>
        /// <param name="value">Item being validated.</param>
        public delegate void ValidateHandler(T value);

        #endregion

        #region Constructor

        /// <summary>
        /// Create new instance of the collection.
        /// </summary>
        public GenericCollection()
        {
        }

        /// <summary>
        /// Create new instance of the collection.
        /// </summary>
        /// <param name="owner">Owner of the collection.</param>
        public GenericCollection(object owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Create new instance of the collection.
        /// </summary>
        /// <param name="info">Serialization Info</param>
        /// <param name="context">Streaming Context</param>
        protected GenericCollection(SerializationInfo info, StreamingContext context)
        {
            siInfo = info;
        }

        /// <summary>
        /// Create new instance of the collection.
        /// </summary>
        /// <param name="items">Items to be inserted by default.</param>
        public GenericCollection(IEnumerable<T> items)
            : this()
        {
            foreach (T barItem in items)
            {
#pragma warning disable DoNotCallOverridableMethodsInConstructor
                OnInsert(InnerList.Count, barItem);
#pragma warning restore DoNotCallOverridableMethodsInConstructor
                InnerList.Add(barItem);
#pragma warning disable DoNotCallOverridableMethodsInConstructor
                OnInsertComplete(InnerList.Count - 1, barItem);
#pragma warning restore DoNotCallOverridableMethodsInConstructor
            }
        }

        /// <summary>
        /// Create new instance of the collection.
        /// </summary>
        /// <param name="items">Items to be inserted by default.</param>
        public GenericCollection(GenericCollection<T> items)
            : this()
        {
            foreach (T item in items)
            {
                T newItem = (T)(item is ICloneable ? (item as ICloneable).Clone() : item);
#pragma warning disable DoNotCallOverridableMethodsInConstructor
                OnInsert(InnerList.Count, newItem);
#pragma warning restore DoNotCallOverridableMethodsInConstructor
                InnerList.Add(newItem);
#pragma warning disable DoNotCallOverridableMethodsInConstructor
                OnInsertComplete(InnerList.Count - 1, newItem);
#pragma warning restore DoNotCallOverridableMethodsInConstructor
            }
        }

        #endregion

        #region Property

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">Index of item to be retrieved.</param>
        /// <returns></returns>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public T this[int index]
        {
            get { return (T)InnerList[index]; }
            set { InnerList[index] = value; }
        }

        /// <summary>
        /// Gets or sets the Owner of the collection.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Collection is cleared.
        /// </summary>
        [Category("Collection")]
        [Description("Collection is cleared.")]
        public event CollectionClearHandler Cleared;

        /// <summary>
        /// Collection is clearing.
        /// </summary>
        [Category("Collection")]
        [Description("Collection is clearing.")]
        public event CollectionClearingHandler Clearing;

        /// <summary>
        /// Item is inserted in the collection.
        /// </summary>
        [Category("Collection")]
        [Description("Item is inserted in the collection.")]
        public event CollectionChangedHandler Inserted;

        /// <summary>
        /// Item is inserting in the collection.
        /// </summary>
        [Category("Collection")]
        [Description("Item is inserting in the collection.")]
        public event CollectionChangingHandler Inserting;

        /// <summary>
        /// Item is removed from the collection.
        /// </summary>
        [Category("Collection")]
        [Description("Item is removed from the collection.")]
        public event CollectionChangedHandler Removed;

        /// <summary>
        /// Item is removing from the collection.
        /// </summary>
        [Category("Collection")]
        [Description("Item is removing from the collection.")]
        public event CollectionChangingHandler Removing;

        /// <summary>
        /// Item value is changing in the collection.
        /// </summary>
        [Category("Collection")]
        [Description("Item value is changing in the collection.")]
        public event ItemChangingHandler Changing;

        /// <summary>
        /// Item value is changed in the collection.
        /// </summary>
        [Category("Collection")]
        [Description("Item value is changed in the collection.")]
        public event ItemChangeHandler Changed;

        /// <summary>
        /// Item is being validated.
        /// </summary>
        [Category("Collection")]
        [Description("Item is being validated.")]
        public event ValidateHandler Validating;

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds an item to the end of the collection.
        /// </summary>
        /// <param name="item">The item to be added to the end of the Collection. The value can be null.</param>
        /// <returns>The index at which the value has been added.</returns>
        public int Add(T item)
        {
            OnInsert(InnerList.Count, item);
            int index = InnerList.Add(item);
            OnInsertComplete(InnerList.Count, item);
            return index;
        }

        /// <summary>
        /// Adds the array of elements.
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(T[] items)
        {
            foreach (T item in items)
            {
                OnInsert(InnerList.Count, item);
                InnerList.Add(item);
                OnInsertComplete(InnerList.Count, item);
            }
        }

        /// <summary>
        /// Adds an item(s) to the end of the collection.
        /// </summary>
        /// <param name="items">The item to be added to the end of the Collection. The value can be null. </param>
        public void Add(T[] items)
        {
            foreach (T item in items)
            {
                OnInsert(InnerList.Count, item);
                InnerList.Add(item);
                OnInsertComplete(InnerList.Count, item);
            }
        }

        /// <summary>
        /// Inserts an element into the Collection at the specified index.
        /// </summary>
        /// <param name="index">Index at which item has to be inserted.</param>
        /// <param name="item">Item to be inserted</param>
        public void Insert(int index, T item)
        {
            OnInsert(index, item);
            InnerList.Insert(index, item);
            OnInsertComplete(index, item);
        }

        /// <summary>
        /// Removes item from the collection.
        /// </summary>
        /// <param name="item">Item to be removed.</param>
        public void Remove(T item)
        {
            int index = IndexOf(item);
            OnRemove(index, item);
            InnerList.Remove(item);
            OnRemoveComplete(index, item);
        }

        /// <summary>
        /// Gets the last index of item.
        /// </summary>
        /// <param name="item">Item to be searched.</param>
        /// <returns>Gets the last index of the element.</returns>
        public int LastIndexOf(T item)
        {
            return InnerList.LastIndexOf(item);
        }

        /// <summary>
        /// Gets the last index of the supplied item from the starting index.
        /// </summary>
        /// <param name="item">Item to be searched.</param>
        /// <param name="startIndex">Start index from which searching will be done.</param>
        /// <returns>Gets the last index of the element.</returns>
        public int LastIndexOf(T item, int startIndex)
        {
            return InnerList.LastIndexOf(item, startIndex);
        }

        /// <summary>
        /// Gets the last index of the supplied item from the starting index. 
        /// </summary>
        /// <param name="item">The System.Object to locate in the System.Collections.ArrayList. The value can be null.</param>
        /// <param name="startIndex">The zero-based starting index of the backward search.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <returns>The zero-based index of the last occurrence of value within the range of elements in the System.Collections.ArrayList that contains count number of elements and ends at startIndex, if found; otherwise, -1.</returns>
        public int LastIndexOf(T item, int startIndex, int count)
        {
            return InnerList.LastIndexOf(item, startIndex, count);
        }

        /// <summary>
        /// Inserts the elements to the specified index.
        /// </summary>
        /// <param name="index">Index at which insertion will be take palce.</param>
        /// <param name="items">Items to be inserted.</param>
        public void InsertRange(int index, GenericCollection<T> items)
        {
            InnerList.InsertRange(index, items);
        }

        /// <summary>
        /// Finds collection contains the supplied element.
        /// </summary>
        /// <param name="item">Item to be searched.</param>
        /// <returns>return true if found, else false.</returns>
        public bool Contains(T item)
        {
            return InnerList.Contains(item);
        }

        /// <summary>
        /// Gets the index of the supplied item.
        /// </summary>
        /// <param name="value">Item to be searched.</param>
        /// <returns>returns index of the iotem. Returns -1 if not found in the collection.</returns>
        public int IndexOf(T value)
        {
            return InnerList.IndexOf(value);
        }

        /// <summary>
        /// Gets the index of the supplied item.
        /// </summary>
        /// <param name="value">Item to be searched.</param>
        /// <param name="startIndex">Index from which searching will start.</param>
        /// <returns>returns index of the iotem. Returns -1 if not found in the collection.</returns>
        public int IndexOf(T value, int startIndex)
        {
            return InnerList.IndexOf(value, startIndex);
        }

        /// <summary>
        /// Gets the index of the supplied item.
        /// </summary>
        /// <param name="value">Item to be searched.</param>
        /// <param name="startIndex">Index from which searching will start.</param>
        /// <param name="count">The number of elements in the section to search.</param>
        /// <returns>returns index of the iotem. Returns -1 if not found in the collection.</returns>
        public int IndexOf(T value, int startIndex, int count)
        {
            return InnerList.IndexOf(value, startIndex, count);
        }

        #endregion

        #region Overrides

        ///<summary>
        ///Performs additional custom processes when clearing the contents of the <see cref="T:System.Collections.CollectionBase"></see> instance.
        ///</summary>
        ///
        protected override void OnClear()
        {
            GenericCancelEventArgs<GenericCollection<T>> e = new GenericCancelEventArgs<GenericCollection<T>>(this);
            if (Clearing != null)
            {
                Clearing(e);
                if (e.Cancel)
                {
                    return;
                }
            }
            base.OnClear();
        }

        ///<summary>
        ///Performs additional custom processes after clearing the contents of the <see cref="T:System.Collections.CollectionBase"></see> instance.
        ///</summary>
        ///
        protected override void OnClearComplete()
        {
            base.OnClearComplete();
            if (Cleared != null)
            {
                Cleared();
            }
        }

        ///<summary>
        ///Performs additional custom processes before inserting a new element into the <see cref="T:System.Collections.CollectionBase"></see> instance.
        ///</summary>
        ///
        ///<param name="value">The new value of the element at index.</param>
        ///<param name="index">The zero-based index at which to insert value.</param>
        protected override void OnInsert(int index, object value)
        {
            GenericCancelEventArgs<T> e = new GenericCancelEventArgs<T>((T)value);
            if (Inserting != null)
            {
                Inserting(index, e);
                if (e.Cancel)
                {
                    return;
                }
            }
            base.OnInsert(index, value);
        }

        ///<summary>
        ///Performs additional custom processes after inserting a new element into the <see cref="T:System.Collections.CollectionBase"></see> instance.
        ///</summary>
        ///
        ///<param name="value">The new value of the element at index.</param>
        ///<param name="index">The zero-based index at which to insert value.</param>
        protected override void OnInsertComplete(int index, object value)
        {
            base.OnInsertComplete(index, value);
            if (Inserted != null)
            {
                Inserted(index, (T)value);
            }
        }

        ///<summary>
        ///Performs additional custom processes when removing an element from the <see cref="T:System.Collections.CollectionBase"></see> instance.
        ///</summary>
        ///
        ///<param name="value">The value of the element to remove from index.</param>
        ///<param name="index">The zero-based index at which value can be found.</param>
        protected override void OnRemove(int index, object value)
        {
            GenericCancelEventArgs<T> e = new GenericCancelEventArgs<T>((T)value);
            if (Removing != null)
            {
                Removing(index, e);
                if (e.Cancel)
                {
                    return;
                }
            }
            base.OnRemove(index, value);
        }

        ///<summary>
        ///Performs additional custom processes after removing an element from the <see cref="T:System.Collections.CollectionBase"></see> instance.
        ///</summary>
        ///
        ///<param name="value">The value of the element to remove from index.</param>
        ///<param name="index">The zero-based index at which value can be found.</param>
        protected override void OnRemoveComplete(int index, object value)
        {
            base.OnRemoveComplete(index, value);
            if (Removed != null)
            {
                Removed(index, (T)value);
            }
        }

        ///<summary>
        ///Performs additional custom processes when validating a value.
        ///</summary>
        ///
        ///<param name="value">The object to validate.</param>
        protected override void OnValidate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
            if (Validating != null)
            {
                Validating((T)value);
            }
            base.OnValidate(value);
        }

        ///<summary>
        ///Performs additional custom processes before setting a value in the <see cref="T:System.Collections.CollectionBase"></see> instance.
        ///</summary>
        ///
        ///<param name="oldValue">The value to replace with newValue.</param>
        ///<param name="newValue">The new value of the element at index.</param>
        ///<param name="index">The zero-based index at which oldValue can be found.</param>
        protected override void OnSet(int index, object oldValue, object newValue)
        {
            GenericChangeEventArgs<T> e = new GenericChangeEventArgs<T>((T)oldValue, (T)newValue);
            if (Changing != null)
            {
                Changing(index, e);
                if (e.Cancel)
                {
                    return;
                }
            }
            base.OnSet(index, oldValue, newValue);
        }

        ///<summary>
        ///Performs additional custom processes after setting a value in the <see cref="T:System.Collections.CollectionBase"></see> instance.
        ///</summary>
        ///
        ///<param name="oldValue">The value to replace with newValue.</param>
        ///<param name="newValue">The new value of the element at index.</param>
        ///<param name="index">The zero-based index at which oldValue can be found.</param>
        protected override void OnSetComplete(int index, object oldValue, object newValue)
        {
            base.OnSetComplete(index, oldValue, newValue);
            if (Changed != null)
            {
                Changed(index, (T)oldValue, (T)newValue);
            }
        }

        #endregion

        private object owner;

        private SerializationInfo siInfo;

        #region IDeserializationCallback Members

        ///<summary>
        ///Runs when the entire object graph has been deserialized.
        ///</summary>
        ///
        ///<param name="sender">The object that initiated the callback. The functionality for this parameter is not currently implemented. </param>
        public void OnDeserialization(object sender)
        {
            if (siInfo != null)
            {
                Clear();
                if (siInfo.GetInt32("Count") != 0)
                {
                    Clear();
                    int num = siInfo.GetInt32("Count");
                    for (int i = 0; i < num; i++)
                    {
                        Add((T)siInfo.GetValue("Items" + i, typeof(T)));
                    }
                }
                siInfo = null;
            }
        }

        #endregion

        #region IDisposable Members

        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        public void Dispose()
        {
            owner = null;
            List.Clear();
            InnerList.Clear();
            siInfo = null;
        }

        #endregion

        #region ISerializable Members

        ///<summary>
        ///Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> with the data needed to serialize the target object.
        ///</summary>
        ///
        ///<param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"></see>) for this serialization. </param>
        ///<param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> to populate with data. </param>
        ///<exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            info.AddValue("Count", Count);
            if (Count != 0)
            {
                for (int i = 0; i < Count; i++)
                {
                    info.AddValue("Items" + i, this[i]);
                }
            }
        }

        #endregion

        /// <summary>
        /// Sets the index of the child to the specified index.
        /// </summary>
        /// <param name="item">Child of which index is to be set.</param>
        /// <param name="index">New index of the child.</param>
        public void SetChildIndex(T item, int index)
        {
            if (List.Count > 0)
            {
                int num = IndexOf(item);
                if (index < 0)
                {
                    index = 0;
                }
                if (index >= List.Count)
                {
                    index = List.Count - 1;
                }
                if ((index >= 0) && (index < List.Count))
                {
                    if (num < index)
                    {
                        for (int i = num; i < index; i++)
                        {
                            List[i] = List[i + 1];
                        }
                        List[index] = item;
                    }
                    else if (num > index)
                    {
                        for (int j = num; j > index; j--)
                        {
                            List[j] = List[j - 1];
                        }
                        List[index] = item;
                    }
                }
            }
        }

        /// <summary>
        /// Sortes the collection with specified <see cref="IComparer"/>
        /// </summary>
        /// <param name="comparer"><see cref="IComparer"/> for sorting.</param>
        public void Sort(IComparer comparer)
        {
            if ((List.Count > 0) && (comparer != null))
            {
                object[] array = new object[List.Count];
                for (int i = 0; i < List.Count; i++)
                {
                    array[i] = List[i];
                }
                Array.Sort(array, comparer);
                List.Clear();
                for (int j = 0; j < array.Length; j++)
                {
                    List.Add(array[j]);
                }
            }
        }
    }
}
