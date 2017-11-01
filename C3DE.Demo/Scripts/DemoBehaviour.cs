using C3DE.Components;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo.Scripts
{
    public class DemoBehaviour : Behaviour
    {
        public override void Update()
        {
            if (Input.Keys.Pressed(Keys.LeftAlt) && Input.Keys.JustPressed(Keys.Enter))
                Screen.Fullscreen = !Screen.Fullscreen;

            if (Input.Keys.JustPressed(Keys.Escape) || Input.Gamepad.JustPressed(Buttons.Back))
                Application.SceneManager.LoadLevel(0);
        }
    }
}
