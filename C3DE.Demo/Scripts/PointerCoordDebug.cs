using C3DE.Components;
using C3DE.UI;
using Microsoft.Xna.Framework;

namespace C3DE.Demo.Scripts
{
    public class PointerCoordDebug : Behaviour
    {
        private Vector2 lineOne = new Vector2(10.0f);
        private Vector2 lineTwo = new Vector2(10.0f, 30.0f);
        private string[] coords = new string[2];

        public override void OnGUI(GUI ui)
        {
            coords[0] = string.Format("Mouse X: {0}", Input.Mouse.X);
            coords[1] = string.Format("Mouse Y: {0}", Input.Mouse.Y);

            ui.Label(lineOne, coords[0]);
            ui.Label(lineTwo, coords[1]);
        }
    }
}
