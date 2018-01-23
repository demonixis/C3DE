using C3DE.Components;
using Microsoft.Xna.Framework.Input;
using System;

namespace C3DE.Demo.Scripts.Editor.Components
{
    public class HotkeyManager : Behaviour
    {
        public enum ActionKeys
        {
            All = 0, Copy, Cut, Past, Shift, Ctrl, Suppr,
            Save
        }

        public event Action<ActionKeys> ActionKeyJustPressed = null;

        public override void Update()
        {
            var keys = Input.Keys;

            if (keys.JustPressed(Keys.Delete))
                ActionKeyJustPressed?.Invoke(ActionKeys.Suppr);

            if (keys.JustPressed(Keys.S))
                ActionKeyJustPressed?.Invoke(ActionKeys.Save);

            if (keys.Pressed(Keys.LeftControl) && keys.JustPressed(Keys.A))
                ActionKeyJustPressed?.Invoke(ActionKeys.All);

            if (keys.Pressed(Keys.LeftControl) && keys.JustPressed(Keys.C))
                ActionKeyJustPressed?.Invoke(ActionKeys.Copy);

            if (keys.Pressed(Keys.LeftControl) && keys.JustPressed(Keys.X))
                ActionKeyJustPressed?.Invoke(ActionKeys.Cut);

            if (keys.Pressed(Keys.LeftControl) && keys.JustPressed(Keys.V))
                ActionKeyJustPressed?.Invoke(ActionKeys.Past);
        }
    }
}
