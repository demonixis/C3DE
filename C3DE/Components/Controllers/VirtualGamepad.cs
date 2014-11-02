using C3DE.Resources;
using C3DE.Inputs;
using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;


namespace C3DE.Components.Controllers
{
    public class VirtualGamepad : Behaviour
    {
        private Vector2 _startPosition;
        private Vector2 _movePosition;
        private Vector2 _position;
        private Texture2D _texture;
        private bool _mustCenter;
        private bool _enableMouse;
        private Vector2 _limits;

        // Cache vars
        private float _scale;
        private float _tdx;
        private float _tdy;
        private bool _tTouched;
        private bool _tReleased;

        public Color GamepadColor { get; set; }

        public float DeadZone { get; set; }

        public int FingerTarget { get; set; }

        public float Scale {
            get { return _scale; }
            set
            {
                _scale = value;
                ShowAt(_startPosition.X, _startPosition.Y);
            }
        }

        public Vector2 Sensibility { get; set; }

        public Texture2D GamepadTexture
        {
            get { return _texture; }
            set
            {
                _texture = value;
                ShowAt(_startPosition.X, _startPosition.Y);
            }
        }

        public Vector2 Position
        {
            get { return _position; }
        }

        public VirtualGamepad()
            : base()
        {
            _enableMouse = false;
            _startPosition = Vector2.Zero;
            _movePosition = Vector2.Zero;
            _limits = Vector2.Zero;
            _position = Vector2.Zero;
            _scale = 1.0f;
            GamepadColor = new Color(1.0f, 1.0f, 1.0f, 0.15f);
            DeadZone = 0.2f;
            FingerTarget = 0;
            Sensibility = Vector2.One;
        }

        public override void Start()
        {
            if (_texture == null)
            {
                var t1 = GraphicsHelper.CreateCircleTexture(Color.Black, 256);
                var t2 = GraphicsHelper.CreateCircleTexture(Color.GhostWhite, 210);
                _texture = GraphicsHelper.Combine(t1, t2);
            }

            var x = (_texture.Width * Scale) / 2;
            var y = Screen.Height - (_texture.Height * Scale + (_texture.Height * Scale) / 2);

            ShowAt(x, y);
        }

        public override void Update()
        {
            _tdx = Input.Touch.Delta(FingerTarget).X;
            _tdy = Input.Touch.Delta(FingerTarget).Y;
            _tTouched = Input.Touch.Pressed(FingerTarget);
            _tReleased = Input.Touch.Released(FingerTarget);

            if (_enableMouse)
            {
                _tdx += Input.Mouse.Delta.X;
                _tdy += Input.Mouse.Delta.Y;
                _tTouched = _tTouched || Input.Mouse.Down(MouseButton.Left);
                _tReleased = _tReleased || Mouse.GetState().LeftButton == ButtonState.Released;
            }

			if (!_tReleased)
            {
                _movePosition.X += _tdx;
                _movePosition.Y += _tdy;

                if (_movePosition.X < _startPosition.X - _limits.X)
                    _movePosition.X = _startPosition.X - _limits.X;

                else if (_movePosition.X > _startPosition.X + _limits.X)
                    _movePosition.X = _startPosition.X + _limits.X;

                if (_movePosition.Y < _startPosition.Y - _limits.Y)
                    _movePosition.Y = _startPosition.Y - _limits.Y;

                else if (_movePosition.Y > _startPosition.Y + _limits.Y)
                    _movePosition.Y = _startPosition.Y + _limits.Y;

                _position.X = Round((_movePosition.X - _startPosition.X) / _limits.X);
                _position.Y = Round((_movePosition.Y - _startPosition.Y) / _limits.Y);

                _position.X = ((Math.Abs(_position.X) < DeadZone) ? 0.0f : _position.X) * Sensibility.X;
                _position.Y = ((Math.Abs(_position.Y) < DeadZone) ? 0.0f : _position.Y) * Sensibility.Y;

                _mustCenter = true;
            }
            else if (_mustCenter)
            {
                _movePosition.X = _startPosition.X;
                _movePosition.Y = _startPosition.Y;
                _position.X = 0;
                _position.Y = 0;
                _mustCenter = false;
            }
        }

        public override void OnGUI(GUI ui)
        {
            ui.DrawTexture(_movePosition, _texture, GamepadColor);
        }

        public void ShowAt(float x, float y)
        {
            _movePosition.X = x;
            _movePosition.Y = y;

            _startPosition.X = x;
            _startPosition.Y = y;

            _limits.X = _texture.Width / 2 * Scale;
            _limits.Y = _texture.Height / 2 * Scale;

            _mustCenter = false;

            _position.X = 0.0f;
            _position.Y = 0.0f;
        }

        private float Round(float value)
        {
            return (float)Math.Round(value * 1000.0f) / 1000.0f;
        }
    }
}
