using System;

namespace C3DE.Editor.Events
{
    public class BasicMessage
    {
        public static BasicMessage Empty
        {
            get { return new BasicMessage(); }
        }

        public string Message { get; set; }

        public BasicMessage()
        {
            Message = string.Empty;
        }

        public BasicMessage(string message)
        {
            Message = message;
        }
    }

    public class GenericMessage<T> : BasicMessage
    {
        public T Value { get; set; }

        public GenericMessage(T value = default(T), string message = "")
            : base(message)
        {
            Value = value;
        }
    }
}