using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Inputs.Experimental
{
    public class MouseDevice : InputDevice
    {
        private MouseState _mouseState;
        private MouseState _prevState;
        protected Vector2 _delta;
        private float _deltaWheel;

        public MouseDevice()
        {
            _mouseState = Mouse.GetState();
            _prevState = _mouseState;
        }

        public override void Update()
        {
            // Update states
            _prevState = _mouseState;
            _mouseState = Mouse.GetState();

            // Delta
            if (Screen.LockCursor)
            {
                _delta.X = (_mouseState.X - Screen.WidthPerTwo);
                _delta.Y = (_mouseState.Y - Screen.HeightPerTwo);
            }
            else
            {
                _delta.X = (_mouseState.X - _prevState.X);
                _delta.Y = (_mouseState.Y - _prevState.Y);
            }

            _deltaWheel = _mouseState.ScrollWheelValue - _prevState.ScrollWheelValue;

            base.Update();
        }

        protected override float UpdateAxisState(Axis axis)
        {
            if (axis == Axis.MouseX)
                return _delta.X;
            else if (axis == Axis.MouseY)
                return _delta.Y;

            return 0.0f;
        }

        protected override bool UpdateButtonState(Buttons button)
        {
            if (button == Buttons.LeftTrigger)
                return _mouseState.RightButton == ButtonState.Pressed;
            else if (button == Buttons.RightTrigger)
                return _mouseState.LeftButton == ButtonState.Pressed;
            else if (button == Buttons.LeftShoulder)
                return _deltaWheel < -0.25f;
            else if (button == Buttons.RightShoulder)
                return _deltaWheel > 0.25f;

            return false;
        }
    }
}
