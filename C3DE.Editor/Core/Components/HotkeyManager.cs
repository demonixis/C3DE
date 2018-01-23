using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace C3DE.Editor
{
    public class HotkeyManager : GameComponent
    {
        public event Action<string> ActionKeyJustPressed = null;

        public HotkeyManager(Game game) : base(game)
        {
        }

        public override void Update(GameTime gameTime)
        {
            var keys = Input.Keys;

            if (keys.JustPressed(Keys.Delete))
                ActionKeyJustPressed?.Invoke("Delete");

            if (keys.JustPressed(Keys.S))
                ActionKeyJustPressed?.Invoke("Save");

            if (keys.JustPressed(Keys.O))
                ActionKeyJustPressed?.Invoke("Load");

            if (keys.Pressed(Keys.LeftControl) && keys.JustPressed(Keys.A))
                ActionKeyJustPressed?.Invoke("Select All");

            if (keys.Pressed(Keys.LeftControl) && keys.JustPressed(Keys.C))
                ActionKeyJustPressed?.Invoke("Copy");

            if (keys.Pressed(Keys.LeftControl) && keys.JustPressed(Keys.X))
                ActionKeyJustPressed?.Invoke("Cut");

            if (keys.Pressed(Keys.LeftControl) && keys.JustPressed(Keys.V))
                ActionKeyJustPressed?.Invoke("Past");
        }
    }
}
