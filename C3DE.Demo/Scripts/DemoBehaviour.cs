using C3DE.Components;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo.Scripts
{
    public class DemoBehaviour : Behaviour
    {
        public override void Update()
        {
            if (Input.Keys.JustPressed(Keys.Escape) || Input.Gamepad.JustPressed(Buttons.Back))
                Application.LoadLevel(0);
        }
    }
}
