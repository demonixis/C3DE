using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
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
            AddInputDevice<TouchDevice>();
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var device in _devices)
            {
                if (device.Enabled)
                    device.Update();
            }

            base.Update(gameTime);
        }

        #region Device Management

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

        public static void SetDeviceEnabled<T>(bool enabled) where T : InputDevice
        {
            foreach (var device in _devices)
            {
                if (device is T)
                    device.Enabled = enabled;
            }
        }

        public static T GetDevice<T>() where T : InputDevice
        {
            foreach (var device in _devices)
            {
                if (device is T)
                    return (T)device;
            }

            return null;
        }

        #endregion

        #region CrossInput

        public static bool GetButton(Buttons button)
        {
            foreach (var device in _devices)
            {
                if (device.Enabled && device.Get(button))
                    return true;
            }

            return false;
        }

        public static bool GetButtonDown(Buttons button)
        {
            foreach (var device in _devices)
            {
                if (device.Enabled && device.GetDown(button))
                    return true;
            }

            return false;
        }

        public static bool GetButtonUp(Buttons button)
        {
            foreach (var device in _devices)
            {
                if (device.Enabled && device.GetUp(button))
                    return true;
            }

            return false;
        }

        public static float Get(Axis axis)
        {
            var value = 0.0f;

            foreach (var device in _devices)
            {
                if (!device.Enabled)
                    continue;

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
                if (!device.Enabled)
                    continue;

                value = device.Get(axis);
                if (Math.Abs(value.X) + Math.Abs(value.Y) > 0)
                    return value;
            }

            return value;
        }

        #endregion

        #region Keyboard Specific

        public static bool GetKey(Keys key) => Keyboard.GetState().IsKeyDown(key);

        #endregion

        #region Touch Specific

        public static void GetTouchPosition(int index, ref Vector2 position)
        {
            var state = TouchPanel.GetState();
            if (state.Count > index)
            {
                position.X = state[index].Position.X;
                position.Y = state[index].Position.Y;
            }

            position.X = 0;
            position.Y = 0;
        }

        #endregion
    }
}
