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

        public bool Pressed(Keys key)
        {
            return _kbState.IsKeyDown(key);
        }

        public bool Released(Keys key)
        {
            return _kbState.IsKeyUp(key);
        }

        public bool JustPressed(Keys key)
        {
            return _kbState.IsKeyUp(key) && _lastKbState.IsKeyDown(key);
        }

        public bool Up
        {
            get { return this.Pressed(Keys.Up); }
        }

        public bool Down
        {
            get { return this.Pressed(Keys.Down); }
        }

        public bool Left
        {
            get { return this.Pressed(Keys.Left); }
        }

        public bool Right
        {
            get { return this.Pressed(Keys.Right); }
        }

        public bool Enter
        {
            get { return this.Pressed(Keys.Enter); }
        }

        public bool Space
        {
            get { return this.Pressed(Keys.Space); }
        }

        public bool Escape
        {
            get { return this.Pressed(Keys.Escape); }
        }
    }
}