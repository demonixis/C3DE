using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace C3DE.Inputs.Experimental
{
    public class Input : GameComponent
    {
        private static List<InputDevice> _devices = new List<InputDevice>();

        public Input(Game game) : base(game)
        {
            AddInputDevice<KeyboardDevice>();
            AddInputDevice<MouseDevice>();
            AddInputDevice<GamepadDevice>();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var device in _devices)
                device.Update();

            base.Update(gameTime);
        }

        public static void AddInputDevice<T>() where T : InputDevice
        {
            foreach (var device in _devices)
            {
                if (device is T)
                    return;
            }

            _devices.Add(default(T));
        }

        public static void RemoveInputDevice<T>() where T : InputDevice
        {
            var index = -1;
            for (var i = 0; i < _devices.Count; i++)
            {
                if (_devices[i] is T)
                    index = i;
            }

            if (index > -1)
                _devices.RemoveAt(index);
        }

        public static bool Get(Buttons button)
        {
            foreach (var device in _devices)
            {
                if (device.Get(button))
                    return true;
            }

            return false;
        }

        public static bool GetDown(Buttons button)
        {
            foreach (var device in _devices)
            {
                if (device.GetDown(button))
                    return true;
            }

            return false;
        }

        public static bool GetUp(Buttons button)
        {
            foreach (var device in _devices)
            {
                if (device.GetUp(button))
                    return true;
            }

            return false;
        }

        public static float Get(Axis axis)
        {
            var value = 0.0f;

            foreach (var device in _devices)
            {
                value = device.Get(axis);
                if (Math.Abs(value) > 0)
                    return value;
            }

            return value;
        }

        public static Vector2 Get(Axis2D axis)
        {
            var value = Vector2.Zero;

            foreach (var device in _devices)
            {
                value = device.Get(axis);
                if (Math.Abs(value.X) + Math.Abs(value.Y) > 0)
                    return value;
            }

            return value;
        }
    }
}
