using Microsoft.Xna.Framework.Input;

namespace C3DE.Inputs.Experimental
{
    public class KeyboardDevice : InputDevice
    {
        protected override float UpdateAxisState(Axis axis)
        {
            var state = Keyboard.GetState();

            if (axis == Axis.Horizontal)
            {
                if (state.IsKeyDown(Keys.Q))
                    return -1.0f;
                else if (state.IsKeyDown(Keys.D))
                    return 1.0f;
            }
            else if (axis == Axis.Vertical)
            {
                if (state.IsKeyDown(Keys.Z))
                    return -1.0f;
                else if (state.IsKeyDown(Keys.S))
                    return 1.0f;
            }

            return 0.0f;
        }

        protected override bool UpdateButtonState(Buttons button)
        {
            var state = Keyboard.GetState();

            // Special Buttons
            if (button == Buttons.Start)
                return state.IsKeyDown(Keys.Escape);
            else if (button == Buttons.Back)
                return state.IsKeyDown(Keys.I);

            // DPad
            else if (button == Buttons.DPadDown)
                return state.IsKeyDown(Keys.Down);
            else if (button == Buttons.DPadLeft)
                return state.IsKeyDown(Keys.LeftShift);
            else if (button == Buttons.DPadRight)
                return state.IsKeyDown(Keys.Right);
            else if (button == Buttons.DPadUp)
                return state.IsKeyDown(Keys.Up);

            // Buttons
            else if (button == Buttons.A)
                return state.IsKeyDown(Keys.E);
            else if (button == Buttons.B)
                return state.IsKeyDown(Keys.C);
            else if (button == Buttons.X)
                return state.IsKeyDown(Keys.R);
            else if (button == Buttons.Y)
                return state.IsKeyDown(Keys.Space);

            // Bumpers
            else if (button == Buttons.LeftShoulder)
                return state.IsKeyDown(Keys.A);
            else if (button == Buttons.RightShoulder)
                return state.IsKeyDown(Keys.E);

            // Thumbsticks
            else if (button == Buttons.LeftStick)
                return state.IsKeyDown(Keys.LeftShift);
            else if (button == Buttons.RightStick)
                return state.IsKeyDown(Keys.RightShift);

            return false;
        }
    }
}
