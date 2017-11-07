using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.Inputs
{
    public enum Axis
    {
        Horizontal, Vertical, LookUp, Turn, Trigger
    }

    public enum Button
    {
        A, B, X, Y, LeftStick, RightStick,
        LeftBumper, RightBumper, LeftTrigger, RightTrigger,
        Back, Start
    }

    public abstract class InputDevice
    {
        private bool[] m_LastButtonStates;
        private bool[] m_ButtonStates;
        private float[] m_AxisStates;

        public InputDevice()
        {

        }

        public abstract void Update();

        public void UpdateState(Button button, bool value, float deltaTime)
        {
            var index = (int)button;
            m_ButtonStates[index] = value;
        }

        public void UpdateAxis(Axis axis, float deltaTime)
        {

        }
    }
}
