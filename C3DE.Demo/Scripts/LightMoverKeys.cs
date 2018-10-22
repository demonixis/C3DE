using C3DE.Components;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo.Scripts
{
    public class LightMoverKeys : Behaviour
    {
        public override void Update()
        {
            if (Input.Keys.Pressed(Keys.NumPad8) || Input.Gamepad.Pressed(Buttons.DPadUp))
                m_Transform.Translate(0, 0, 0.1f);

            else if (Input.Keys.Pressed(Keys.NumPad5) || Input.Gamepad.Pressed(Buttons.DPadDown))
                m_Transform.Translate(0, 0, -0.1f);

            if (Input.Keys.Pressed(Keys.NumPad4) || Input.Gamepad.Pressed(Buttons.DPadLeft))
                m_Transform.Translate(0.1f, 0, 0);

            else if (Input.Keys.Pressed(Keys.NumPad6) || Input.Gamepad.Pressed(Buttons.DPadRight))
                m_Transform.Translate(-0.1f, 0, 0);
        }
    }
}
