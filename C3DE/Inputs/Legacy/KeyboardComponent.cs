using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Inputs
{
    public sealed class KeyboardComponent : GameComponent
    {
        private KeyboardState _kbState;
        private KeyboardState _lastKbState;

        public KeyboardComponent(Game game)
            : base(game)
        {
            _kbState = Keyboard.GetState();
            _lastKbState = _kbState;
        }

        public override void Update(GameTime gameTime)
        {
            _lastKbState = _kbState;
            _kbState = Keyboard.GetState();
            base.Update(gameTime);
        }

        public bool JustPressed(Keys key)
        {
            return _kbState.IsKeyUp(key) && _lastKbState.IsKeyDown(key);
        }

        public bool Pressed(Keys key) => _kbState.IsKeyDown(key);
        public bool Released(Keys key) => _kbState.IsKeyUp(key);
        public bool Up => Pressed(Keys.Up);
        public bool Down => Pressed(Keys.Down);
        public bool Left => Pressed(Keys.Left);
        public bool Right => Pressed(Keys.Right);
        public bool Enter => Pressed(Keys.Enter);
        public bool Space => Pressed(Keys.Space);
        public bool Escape => Pressed(Keys.Escape);
    }
}