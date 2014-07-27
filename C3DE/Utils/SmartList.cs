using System.Collections.Generic;

namespace C3DE.Utils
{
    /// <summary>
    /// A list to manage component or scene object during the life cycle of the application.
    /// Objects must be insered before the update process to not modify the structure of the main collection.
    /// When the initialization process is done, set CheckRequired to true and call Check method before any updates.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SmartList<T>
    {
        private List<T> _items;
        private List<T> _addList;
        private List<int> _rmList;
        private int _size;
        private int _addSize;
        private int _rmSize;
        private bool _checkRequired;

        public T this[int index]
        {
            get { return _items[index]; }
        }

        public int Size
        {
            get { return _size; }
        }

        public bool CheckRequired
        {
            get { return _checkRequired; }
            set { _checkRequired = value; }
        }

        public SmartList()
        {
            _items = new List<T>();
            _addList = new List<T>();
            _rmList = new List<int>();
            _size = 0;
            _addSize = 0;
            _rmSize = 0;
            _checkRequired = false;
        }

        public void Add(T item)
        {
            if (_items.IndexOf(item) == -1)
            {
                if (_checkRequired)
                {
                    _addList.Add(item);
                    _addSize++;
                }
                else
                {
                    _items.Add(item);
                    _size++;
                }
            }
        }

        public void Remove(T item)
        {
            var index = _items.IndexOf(item);

            if (index > -1)
            {
                if (_checkRequired)
                {
                    _rmList.Add(index);
                    _rmSize++;
                }
                else
                {
                    _items.RemoveAt(index);
                    _size--;
                }
            }
        }

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public void Sort()
        {
            _items.Sort();
        }

        public void Check()
        {
            int i = 0;

            if (_rmSize > 0)
            {
                for (i = 0; i < _rmSize; i++)
                    _items.RemoveAt(_rmList[i]);

                _size -= _rmSize;
                _rmList.Clear();
                _rmSize = 0;
            }

            if (_addSize > 0)
            {
                for (i = 0; i < _addSize; i++)
                    _items.Add(_addList[i]);

                _size += _addSize;
                _addList.Clear();
                _addSize = 0;
            }
        }
    }
}
