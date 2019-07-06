using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Inputs.Experimental
{
    public class GamepadDevice : InputDevice
    {
        protected override float UpdateAxisState(Axis axis)
        {
            var state = GamePad.GetState(PlayerIndex.One);

            if (axis == Axis.Horizontal)
                return state.ThumbSticks.Left.X;
            else if (axis == Axis.Vertical)
                return state.ThumbSticks.Left.Y;
            else if (axis == Axis.MouseX)
                return state.ThumbSticks.Right.X;
            else if (axis == Axis.MouseY)
                return state.ThumbSticks.Right.Y;

            return 0.0f;
        }

        protected override bool UpdateButtonState(Buttons button)
        {
            return GamePad.GetState(PlayerIndex.One).IsButtonDown(button);
        }
    }
}
