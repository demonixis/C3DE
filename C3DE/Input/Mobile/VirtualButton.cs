using System;
using Microsoft.Xna.Framework;
using C3DE.UI;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Utils;

namespace C3DE.Input.Mobile
{
	public class VirtualButton : Behaviour
	{
		private Texture2D _buttonTexture;
		private Rectangle _buttonRect;
		private bool _enabled;
		private int _i;
		private bool _exitLoop;
		private bool _lastState;

		public Texture2D ButtonTexture
		{
			get { return _buttonTexture; }
			set 
			{
				_buttonTexture = value;
				_enabled = _buttonTexture != null;
			}
		}

		public Rectangle ButtonRect 
		{
			get { return _buttonRect; }
			set { _buttonRect = value; }
		}

		public bool Pressed { get; protected set; }

		public bool JustPressed { get; protected set; }

		public VirtualButton ()
		{
			_buttonRect = Rectangle.Empty;
			_enabled = false;
			_i = 0;
			_exitLoop = false;
		}

		public override void Update()
		{
			_lastState = Pressed;

			Pressed = false;

			if (_enabled) 
			{
				_i = 0;
				_exitLoop = false;

				while (_i < Input.Touch.MaxFingerPoints && !_exitLoop) 
				{
					if (_buttonRect.Contains (Input.Touch.GetPosition (_i))) 
					{
						Pressed = true;
						_exitLoop = true;
					}
					_i++;
				}
			}

			JustPressed = !_lastState && Pressed;
		}

		public override void OnGUI(GUI ui)
		{
			if (_enabled)
				ui.DrawTexture (ref _buttonRect, _buttonTexture);
		}
	}
}

