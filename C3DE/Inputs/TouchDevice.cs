using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace C3DE.Inputs.Experimental
{
    public class TouchDevice : InputDevice
    {
        private TouchCollection _touchState;
        private TouchCollection _lastTouchState;
        private Vector2 _delta;
        private int _maxFingerPoints;

        public TouchDevice()
        {
            _touchState = TouchPanel.GetState();
            _lastTouchState = _touchState;

            if (TouchPanel.GetCapabilities().IsConnected)
                _maxFingerPoints = TouchPanel.GetCapabilities().MaximumTouchCount;
            else
                _maxFingerPoints = 0;
        }

        public override void Update()
        {
            _lastTouchState = _touchState;
            _touchState = TouchPanel.GetState();

            if (_touchState.Count > 0 && _lastTouchState.Count > 0)
            {
                _delta.X = (_touchState[0].Position.X - _lastTouchState[0].Position.X);
                _delta.Y = (_touchState[0].Position.Y - _lastTouchState[0].Position.Y);
            }
            else
            {
                _delta.X = 0;
                _delta.Y = 0;
            }

            base.Update();
        }

        protected override float UpdateAxisState(Axis axis)
        {
            if (axis == Axis.Horizontal)
                return _delta.X;
            else if (axis == Axis.Vertical)
                return _delta.Y;

            return 0.0f;
        }

        protected override bool UpdateButtonState(Buttons button)
        {
            return false;
        }
    }
}
