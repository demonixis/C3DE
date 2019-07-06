using System;
using System.Collections.Generic;
using Gwen.Control;
using Gwen.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Gwen.Renderer.MonoGame.Input
{
	public class MonoGameInput
	{
		private Canvas m_Canvas = null;

		private int m_MouseX = 0;
		private int m_MouseY = 0;

		private KeyboardState m_OldKeyboardState;
		private MouseState m_OldMouseState;

		private Dictionary<Keys, Key> m_SpecialKeyMap = new Dictionary<Keys, Key>
		{
			[Keys.Back] = Key.Backspace,
			[Keys.Enter] = Key.Return,
			[Keys.Escape] = Key.Escape,
			[Keys.Tab] = Key.Tab,
			[Keys.Space] = Key.Space,
			[Keys.Up] = Key.Up,
			[Keys.Down] = Key.Down,
			[Keys.Left] = Key.Left,
			[Keys.Right] = Key.Right,
			[Keys.Home] = Key.Home,
			[Keys.End] = Key.End,
			[Keys.Delete] = Key.Delete,
			[Keys.LeftControl] = Key.Control,
			[Keys.RightControl] = Key.Control,
			[Keys.LeftAlt] = Key.Alt,
			[Keys.LeftShift] = Key.Shift,
			[Keys.RightShift] = Key.Shift,
		};

		public MonoGameInput(Game game)
		{
#if WINDOWS || DESKTOPGL || WINDOWS_UWP
			game.Window.TextInput += OnTextInput;
#endif
		}

		public void Initialize(Canvas c)
		{
			m_Canvas = c;
		}

		public bool ProcessMouseState()
		{
			MouseState state = Mouse.GetState();
			bool result = false;

			if (state.LeftButton == ButtonState.Pressed && m_OldMouseState.LeftButton == ButtonState.Released)
				result = ProcessMouseButtons(true, true);
			else if (state.LeftButton == ButtonState.Released && m_OldMouseState.LeftButton == ButtonState.Pressed)
				result = ProcessMouseButtons(true, false);

			if (state.RightButton == ButtonState.Pressed && m_OldMouseState.RightButton == ButtonState.Released)
				result = ProcessMouseButtons(false, true);
			else if (state.RightButton == ButtonState.Released && m_OldMouseState.RightButton == ButtonState.Pressed)
				result = ProcessMouseButtons(false, false);

			if (state.ScrollWheelValue != m_OldMouseState.ScrollWheelValue)
				result = ProcessMouseWheel(m_OldMouseState.ScrollWheelValue - state.ScrollWheelValue);

			if (state.Position != m_OldMouseState.Position)
				result = ProcessMouseMove(state.X, state.Y);

			m_OldMouseState = state;

			return result;
		}

		public bool ProcessMouseButtons(bool left, bool pressed)
		{
			return m_Canvas.Input_MouseButton(left ? 0 : 1, pressed);
		}

		public bool ProcessMouseMove(int x, int y)
		{
			int dx = x - m_MouseX;
			int dy = y - m_MouseY;

			m_MouseX = x;
			m_MouseY = y;

			return m_Canvas.Input_MouseMoved(m_MouseX, m_MouseY, dx, dy);
		}

		public bool ProcessMouseWheel(int delta)
		{
			return m_Canvas.Input_MouseWheel(-delta);
		}

		public bool ProcessKeyboardState()
		{
			KeyboardState state = Keyboard.GetState();
			bool result = false;

			foreach (Keys key in m_SpecialKeyMap.Keys)
			{
				if (state.IsKeyDown(key) && !m_OldKeyboardState.IsKeyDown(key))
					result = ProcessKeyDown(key);
				else if (!state.IsKeyDown(key) && m_OldKeyboardState.IsKeyDown(key))
					result = ProcessKeyUp(key);
			}

			m_OldKeyboardState = state;

			return result;
		}

		public bool ProcessKeyDown(Keys keyCode)
		{
			char ch = TranslateChar(keyCode);

			if (InputHandler.DoSpecialKeys(m_Canvas, ch))
				return false;

			Key key = TranslateKeyCode(keyCode);

			return m_Canvas.Input_Key(key, true);
		}

		public bool ProcessKeyUp(Keys keyCode)
		{
			Key key = TranslateKeyCode(keyCode);

			return m_Canvas.Input_Key(key, false);
		}

#if WINDOWS || DESKTOPGL || WINDOWS_UWP
		private void OnTextInput(object sender, TextInputEventArgs e)
		{
			m_Canvas.Input_Character(e.Character);
		}
#endif

		private Key TranslateKeyCode(Keys keyCode)
		{
			Key key;
			if (m_SpecialKeyMap.TryGetValue(keyCode, out key))
				return key;
			else
				return Key.Invalid;
		}

		private char TranslateChar(Keys keyCode)
		{
			if (keyCode >= Keys.A && keyCode <= Keys.Z)
				return (char)('a' + ((int)keyCode - (int)Keys.A));
			return ' ';
		}

		public bool ProcessTouchState()
		{
			bool result = false;

			var touchCol = TouchPanel.GetState();

			foreach (var touch in touchCol)
			{
				int x = (int)touch.Position.X;
				int y = (int)touch.Position.Y;

				int dx = x - m_MouseX;
				int dy = y - m_MouseY;

				m_MouseX = x;
				m_MouseY = y;

				switch (touch.State)
				{
					case TouchLocationState.Pressed:
						// Cause mouse hover event
						m_Canvas.Input_MouseMoved(x, y, dx, dy);

						result = m_Canvas.Input_MouseButton(0, true);
						break;
					case TouchLocationState.Released:
						result = m_Canvas.Input_MouseButton(0, false);
						
						// Cause mouse leave event
						m_Canvas.Input_MouseMoved(int.MaxValue, int.MaxValue, dx, dy);
						break;
					case TouchLocationState.Moved:
						result = m_Canvas.Input_MouseMoved(x, y, dx, dy);
						break;
				}
			}

			return result;
		}
	}
}
