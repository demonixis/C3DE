using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Components.Controllers.Mobile
{
    public class VirtualGamepad : Behaviour
    {
        private Vector2 _startPosition;
        private Vector2 _movePosition;
        private Vector2 _position;
        private Texture2D _texture;
        private bool _mustCenter;

        private Vector2 _limits;
		private Rectangle _borderLimits;

        // Cache vars
        private float _scale;
        private float _tdx;
        private float _tdy;
        private bool _tTouched;
        private bool _tReleased;
		private int _i;
		private bool _exitLoop;

        public Color GamepadColor { get; set; }

        public float DeadZone { get; set; }

        public float Scale 
		{
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
            _startPosition = Vector2.Zero;
            _movePosition = Vector2.Zero;
            _limits = Vector2.Zero;
            _position = Vector2.Zero;
			_borderLimits = Rectangle.Empty;
            _scale = 1.0f;
            GamepadColor = new Color(1.0f, 1.0f, 1.0f, 0.15f);
            DeadZone = 0.2f;
            Sensibility = Vector2.One;
        }

        public override void Start()
        {
            if (_texture == null)
            {
                var t1 = GraphicsHelper.CreateCircleTexture(Color.TransparentBlack, 150);
                var t2 = GraphicsHelper.CreateCircleTexture(Color.GhostWhite, 140);
                _texture = GraphicsHelper.Combine(t1, t2);
            }

            var x = (_texture.Width * Scale) / 2;
            var y = Screen.VirtualHeight - (_texture.Height * Scale + (_texture.Height * Scale) / 2);

			_borderLimits = new Rectangle (0, Screen.VirtualHeightPerTwo, Screen.VirtualWidthPerTwo, Screen.Height);

            ShowAt(x, y);
        }

        public override void Update()
        {
			_i = 0;
			_exitLoop = false;

			while (_i < Input.Touch.MaxFingerPoints && !_exitLoop) 
			{
				if (_borderLimits.Contains (Input.Touch.GetPosition (_i))) 
				{
					_tdx = Input.Touch.Delta(_i).X;
					_tdy = Input.Touch.Delta(_i).Y;
					_tTouched = Input.Touch.Pressed(_i);
					_tReleased = Input.Touch.Released(_i);
					_exitLoop = true; 
				}

				_i++;
			}
				
			if (!_tReleased)
				UpdateStickPosition ();
			else if (_mustCenter)
				Reset ();
        }

        public override void OnGUI(GUI ui)
        {
            ui.DrawTexture(_movePosition, _texture, GamepadColor);
        }

		private void Reset()
		{
			_movePosition.X = _startPosition.X;
			_movePosition.Y = _startPosition.Y;
			_position.X = 0;
			_position.Y = 0;
			_mustCenter = false;
		}

		private void UpdateStickPosition()
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
