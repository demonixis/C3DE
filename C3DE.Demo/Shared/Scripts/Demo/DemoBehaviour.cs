using C3DE.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Demo.Scripts
{
    public class DemoBehaviour : Behaviour
    {
        private Vector2 _oldResolution;

        public override void Update()
        {
            base.Update();

            if (Input.Keys.Pressed(Keys.LeftAlt) && Input.Keys.JustPressed(Keys.Enter))
            {
                var fullscreen = !Screen.Fullscreen;

                if (fullscreen)
                {
                    _oldResolution.X = Screen.Width;
                    _oldResolution.Y = Screen.Height;
                    Screen.SetBestResolution(true);
                }
                else
                {
                    Screen.SetResolution((int)_oldResolution.X, (int)_oldResolution.Y, false);
                }

                Screen.Fullscreen = fullscreen;
            }

            if (Input.Keys.JustPressed(Keys.Escape) || Input.Gamepad.JustPressed(Buttons.Back))
            {
                Screen.ShowCursor = true;
                Screen.LockCursor = false;
                Application.SceneManager.LoadLevel(0);
            }
        }
    }
}
