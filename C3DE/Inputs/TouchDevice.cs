using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace C3DE.Inputs.Experimental
{
    public class TouchDevice : InputDevice
    {
        private TouchCollection _touchState;
        private TouchCollection _lastTouchState;
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

        protected override float UpdateAxisState(Axis axis)
        {
            if (_maxFingerPoints > 0)
            {

            }

            return 0.0f;
        }

        protected override bool UpdateButtonState(Buttons button)
        {
            if (_maxFingerPoints > 0)
            {

            }

            return false;
        }
    }
}
