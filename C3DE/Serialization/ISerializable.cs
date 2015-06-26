using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace C3DE.Serialization
{
    public interface ISerializable
    {
        SerializedCollection Serialize();
        void Deserialize(SerializedCollection data);
    }

    public class SerializationItem
    {
        public string Id { get; set; }
        public string Value { get; set; }

        public SerializationItem(string id, string value)
        {
            Id = id;
            Value = value;
        }
    }

    public sealed class SerializedCollection
    {
        private SerializationItem[] _items;
        private int _cursor;

        public SerializationItem[] Items
        {
            get { return _items; }
            set { _items = value; }
        }

        public SerializationItem this[int index]
        {
            get { return _items[index]; }
            set
            {
                if (index >= _items.Length)
                    Array.Resize<SerializationItem>(ref _items, _items.Length + 1);

                _items[index] = value;
            }
        }

        public string this[string id]
        {
            get
            {
                var current = Array.Find(_items, item => item.Id == id);

                if (current != null)
                    return current.Value;

                return string.Empty;
            }
            set
            {
                var current = Array.Find(_items, item => item.Id == id);

                if (current == null)
                    Add(id, value);
                else
                    current.Value = value;
            }
        }

        public int Count
        {
            get { return _items.Length; }
        }

        public SerializedCollection(int capacity)
        {
            _items = new SerializationItem[capacity];
            _cursor = 0;
        }

        public void IncreaseCapacity(int capacity)
        {
            Array.Resize<SerializationItem>(ref _items, _items.Length + capacity);
        }

        public void Add(string id, string value)
        {
            this[_cursor++] = new SerializationItem(id, value);
        }

        public void Add(string id, object value)
        {
            Add(id, value.ToString());
        }
    }
}
