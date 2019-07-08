using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace C3DE.Inputs.Experimental
{
    public sealed class TouchDevice : InputDevice
    {
        private TouchCollection _touchState;
        private TouchCollection _lastTouchState;
        public Vector2[] _delta;

        public int MaxFingerPoints { get; private set; }

        public TouchDevice()
        {
            _touchState = TouchPanel.GetState();
            _lastTouchState = _touchState;

            if (TouchPanel.GetCapabilities().IsConnected)
                MaxFingerPoints = TouchPanel.GetCapabilities().MaximumTouchCount;
            else
                MaxFingerPoints = 0;

            _delta = new Vector2[MaxFingerPoints];
        }

        private void UpdateState(int index)
        {
            if (_touchState.Count > 0 && _lastTouchState.Count > 0)
            {
                _delta[index].X = (_touchState[0].Position.X - _lastTouchState[0].Position.X);
                _delta[index].Y = (_touchState[0].Position.Y - _lastTouchState[0].Position.Y);
            }
            else
            {
                _delta[index].X = 0;
                _delta[index].Y = 0;
            }
        }

        public override void Update()
        {
            _lastTouchState = _touchState;
            _touchState = TouchPanel.GetState();

            for (var i = 0; i < _touchState.Count; i++)
                UpdateState(i);

            base.Update();
        }

        protected override float UpdateAxisState(Axis axis)
        {
            if (axis == Axis.MouseX)
                return _delta[0].X;
            else if (axis == Axis.MouseY)
                return _delta[0].Y;

            return 0.0f;
        }

        protected override bool UpdateButtonState(Buttons button)
        {
            return false;
        }
    }
}
