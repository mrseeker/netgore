using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DemoGame.Client
{
    /// <summary>
    /// A collection for the <see cref="MapDrawExtensionBase"/>.
    /// </summary>
    public class MapDrawExtensionCollection : ICollection<MapDrawExtensionBase>
    {
        readonly List<MapDrawExtensionBase> _extensions = new List<MapDrawExtensionBase>();

        Map _map;

        /// <summary>
        /// Gets or sets the current <see cref="Map"/>. Can be null.
        /// </summary>
        public Map Map
        {
            get { return _map; }
            set
            {
                if (_map == value)
                    return;

                _map = value;

                foreach (var extension in _extensions)
                {
                    extension.Map = _map;
                }
            }
        }

        /// <summary>
        /// Adds items to this collection.
        /// </summary>
        /// <param name="extensions">The items to add.</param>
        public void Add(IEnumerable<MapDrawExtensionBase> extensions)
        {
            foreach (var item in extensions)
            {
                Add(item);
            }
        }

        #region ICollection<MapDrawExtensionBase> Members

        /// <summary>
        /// Adds an item to this collection.
        /// </summary>
        /// <param name="extension">The item to add.</param>
        public void Add(MapDrawExtensionBase extension)
        {
            if (_extensions.Contains(extension))
                return;

            _extensions.Add(extension);
        }

        /// <summary>
        /// Clears all items in the collection.
        /// </summary>
        public void Clear()
        {
            _extensions.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise,
        /// false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
        public bool Contains(MapDrawExtensionBase item)
        {
            return _extensions.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an
        /// <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the
        /// elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/>
        /// must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/>
        /// at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/>
        /// is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/>
        /// is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is
        /// multidimensional.-or-<paramref name="arrayIndex"/> is equal to or greater than the length of
        /// <paramref name="array"/>.-or-The number of elements in the source
        /// <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from
        /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type
        /// <see cref="MapDrawExtensionBase"/> cannot be cast automatically to the type of the destination
        /// <paramref name="array"/>.</exception>
        void ICollection<MapDrawExtensionBase>.CopyTo(MapDrawExtensionBase[] array, int arrayIndex)
        {
            _extensions.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the specified item from the collection.
        /// </summary>
        /// <param name="extension">The item to remove.</param>
        /// <returns>True if the item was successfully removed; otherwise false.</returns>
        public bool Remove(MapDrawExtensionBase extension)
        {
            return _extensions.Remove(extension);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </returns>
        public int Count
        {
            get { return _extensions.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
        /// </returns>
        bool ICollection<MapDrawExtensionBase>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<MapDrawExtensionBase> GetEnumerator()
        {
            return _extensions.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}