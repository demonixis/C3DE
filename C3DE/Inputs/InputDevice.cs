using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace C3DE.Inputs.Experimental
{
    public enum Axis
    {
        Horizontal, Vertical,
        MouseX, MouseY,
        LeftTrigger, RightTriger
    }

    public enum Axis2D
    {
        LeftStick, RightStick
    }

    public abstract class InputDevice
    {
        private bool[] _lastButtonStates;
        private bool[] _buttonStates;
        private float[] _axisStates;

        public virtual void Initialize()
        {
            var buttonCount = Enum.GetNames(typeof(Buttons)).Length;
            var axisCount = Enum.GetNames(typeof(Axis)).Length;

            _buttonStates = new bool[buttonCount];
            _lastButtonStates = new bool[buttonCount];
            _axisStates = new float[axisCount];
        }

        public virtual void Update()
        {
            for (var i = 0; i < _buttonStates.Length; i++)
            {
                _lastButtonStates[i] = _buttonStates[i];
                _buttonStates[i] = UpdateButtonState((Buttons)i);
            }

            for (var i = 0; i < _axisStates.Length; i++)
            {
                _axisStates[i] = UpdateAxisState((Axis)i);
            }
        }

        protected abstract bool UpdateButtonState(Buttons button);
        protected abstract float UpdateAxisState(Axis axis);

        public bool Get(Buttons button) => _buttonStates[(int)button];

        public bool GetDown(Buttons button)
        {
            return _buttonStates[(int)button] && !_lastButtonStates[(int)button];
        }

        public bool GetUp(Buttons button)
        {
            return !_buttonStates[(int)button] && _lastButtonStates[(int)button];
        }

        public float Get(Axis axis) => _axisStates[(int)axis];

        public Vector2 Get(Axis2D axis)
        {
            if (axis == Axis2D.LeftStick)
                return new Vector2(Get(Axis.Horizontal), Get(Axis.Vertical));
            else
                return new Vector2(Get(Axis.MouseX), Get(Axis.MouseY));
        }
    }
}
