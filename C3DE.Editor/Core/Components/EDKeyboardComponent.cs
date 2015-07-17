using System;
using System.Collections.Generic;

namespace C3DE.Editor.Core.Components
{
    using Microsoft.Xna.Framework;
    using Win = System.Windows;
    using WinInput = System.Windows.Input;

    public class EDKeyboardComponent : GameComponent
    {
        private Dictionary<Win.Input.Key, bool> keys;

        public EDKeyboardComponent(Game game, Win.UIElement uiElement)
            : base(game)
        {
            uiElement.KeyUp += uiElement_KeyUp;
            uiElement.KeyDown += uiElement_KeyDown;

            var size = Enum.GetNames(typeof(WinInput.Key)).Length;
            keys = new Dictionary<WinInput.Key, bool>(size);
        }

        private void uiElement_KeyDown(object sender, WinInput.KeyEventArgs e)
        {
            SetKeyValue(e.Key, true);
        }

        private void uiElement_KeyUp(object sender, WinInput.KeyEventArgs e)
        {
            SetKeyValue(e.Key, false);
        }

        private void SetKeyValue(WinInput.Key key, bool value)
        {
            if (keys.ContainsKey(key))
                keys[key] = value;
            else
                keys.Add(key, value);
        }

        public bool Pressed(WinInput.Key key)
        {
            if (keys.ContainsKey(key))
                return keys[key];

            return false;
        }
    }
}
