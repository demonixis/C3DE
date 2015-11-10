using C3DE.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Editor.Core.Components
{
    using WpfApplication = System.Windows.Application;
    using WpfMouse = System.Windows.Input.Mouse;
    using WpfMouseButtonState = System.Windows.Input.MouseButtonState;
    using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
    using WpfMouseWheelEventArgs = System.Windows.Input.MouseWheelEventArgs;
    using System.Windows;
    using System.Timers;
    using System.Runtime.InteropServices;

    public class EDMouseComponent : MouseComponent
    {
        private UIElement _uiElement;
        private float _wheel = 0;
        private Vector2 _sensibility;
        private Timer _timer;

        #region Fields

        public int LastX { get; set; }
        public int LastY { get; set; }
        public new int X { get; set; }
        public new int Y { get; set; }

        public new float Wheel
        {
            get { return _wheel; }
        }

        public bool[] MouseButtons { get; set; }
        public bool[] LastMouseButtons { get; set; }

        public new bool Moving
        {
            get { return (X != LastX) || (Y != LastY); }
        }

        public new Vector2 Position
        {
            get { return new Vector2(X, Y); }
        }

        public new Vector2 PreviousPosition
        {
            get { return new Vector2(LastX, LastY); }
        }

        #endregion

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        public EDMouseComponent(Game game, UIElement uiElement)
            : base(game)
        {
            MouseButtons = new bool[3];
            LastMouseButtons = new bool[3];
            _sensibility = new Vector2(0.05f);
            _uiElement = uiElement;
            _uiElement.MouseDown += CheckMouseState;
            _uiElement.MouseUp += CheckMouseState;
            _uiElement.MouseMove += CheckMouseState;
            _uiElement.MouseWheel += OnMouseWheel;
            _uiElement.MouseLeave += CheckMouseState;
            _timer = new Timer(50);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            LastX = X;
            LastY = Y;
            LastMouseButtons[0] = MouseButtons[0];
            LastMouseButtons[1] = MouseButtons[1];
            LastMouseButtons[2] = MouseButtons[2];
            _timer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _uiElement.MouseDown -= CheckMouseState;
                _uiElement.MouseUp -= CheckMouseState;
                _uiElement.MouseMove -= CheckMouseState;
                _uiElement.MouseWheel -= OnMouseWheel;
                _uiElement.MouseLeave -= CheckMouseState;
            }
        }

        public override void Update(GameTime gameTime)
        {
            // Delta
            _delta.X = (X - LastX) * _sensibility.X;
            _delta.Y = (Y - LastY) * _sensibility.Y;
            _wheel *= 0.8f;
        }

        public override void SetPosition(int x, int y)
        {
            var xL = (int)WpfApplication.Current.MainWindow.Left;
            var yT = (int)WpfApplication.Current.MainWindow.Top;
            _delta = Vector2.Zero;
            SetCursorPos(x + xL, y + yT);
        }

        #region Mouse click

        public override bool JustClicked(MouseButton button = MouseButton.Left)
        {
            if (button == MouseButton.Any)
                return JustClicked(MouseButton.Left) || JustClicked(MouseButton.Middle) || JustClicked(MouseButton.Right);

            return !MouseButtons[(int)button] && LastMouseButtons[(int)button];
        }

        protected override bool MouseButtonState(MouseButton button, ButtonState state)
        {
            var value = state == ButtonState.Pressed ? true : false;
            return MouseButtons[(int)button] == value;
        }

        #endregion

        #region Event handlers

        private void OnMouseWheel(object sender, WpfMouseWheelEventArgs e)
        {
            _wheel += e.Delta * 0.01f;
        }

        private void CheckMouseState(object sender, WpfMouseEventArgs e)
        {
            MouseButtons[0] = e.LeftButton == WpfMouseButtonState.Pressed;
            MouseButtons[1] = e.MiddleButton == WpfMouseButtonState.Pressed;
            MouseButtons[2] = e.RightButton == WpfMouseButtonState.Pressed;
            
            var position = WpfMouse.GetPosition(_uiElement);

            LastX = X;
            LastY = Y;

            X = (int)position.X;
            Y = (int)position.Y;
        }

        #endregion
    }
}