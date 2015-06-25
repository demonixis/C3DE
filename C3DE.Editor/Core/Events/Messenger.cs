using System;
using System.Collections.Generic;

namespace C3DE.Editor.Events
{
    public sealed class Messenger
    {
        public delegate void BaseMessage(BasicMessage e);

        private static Dictionary<ushort, List<BaseMessage>> _handlers = new Dictionary<ushort, List<BaseMessage>>();

        public static void Register(ushort name, BaseMessage action)
        {
            if (!_handlers.ContainsKey(name))
            {
                List<BaseMessage> actions = new List<BaseMessage>(1);
                actions.Add(action);

                _handlers.Add(name, actions);
            }
            else
                _handlers[name].Add(action);
        }

        public static void Unregister(ushort name, BaseMessage action)
        {
            if (_handlers.ContainsKey(name))
            {
                if (_handlers[name].Contains(action))
                    _handlers[name].Remove(action);
            }
        }

        public static void Clear()
        {
            foreach (var keyValue in _handlers)
                keyValue.Value.Clear();

            _handlers.Clear();
        }

        public static void Notify(ushort name)
        {
            Notify(name, BasicMessage.Empty);
        }

        public static void Notify(ushort name, string message)
        {
            Notify(name, new BasicMessage(message));
        }

        public static void Notify(ushort name, BasicMessage e)
        {
            if (_handlers.ContainsKey(name))
            {
                for (int i = 0, l = _handlers[name].Count; i < l; i++)
                    _handlers[name][i](e);
            }
        }
    }
}