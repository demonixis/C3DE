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

    public class EDMouseComponent : MouseComponent
    {
        private UIElement _uiElement;
        private float _wheel = 0;
        private bool _needsUpdate;
        private Timer _clickTimer;
        private Vector2 _sensibility;

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

        public EDMouseComponent(Game game, UIElement uiElement)
            : base(game)
        {
            MouseButtons = new bool[3];
            LastMouseButtons = new bool[3];
            _sensibility = new Vector2(0.05f);
            _needsUpdate = false;
            _clickTimer = new Timer(0.2);
            _clickTimer.Elapsed += OnClickTimerDone;
            _uiElement = uiElement;
            _uiElement.MouseDown += OnMouseDown;
            _uiElement.MouseUp += OnMouseDown;
            _uiElement.MouseMove += OnMouseMove;
            _uiElement.MouseWheel += OnMouseWheel;
            _uiElement.MouseLeave += OnMouseLeave;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _uiElement.MouseDown -= OnMouseDown;
                _uiElement.MouseUp -= OnMouseDown;
                _uiElement.MouseMove -= OnMouseMove;
                _uiElement.MouseWheel -= OnMouseWheel;
                _uiElement.MouseLeave -= OnMouseLeave;
            }
        }

        public override void Update(GameTime gameTime)
        {
            // Delta
            _delta.X = (X - LastX) * _sensibility.X;
            _delta.Y = (Y - LastY) * _sensibility.Y;
            _wheel *= 0.8f;

            if (_needsUpdate)
            {
                LastX = X;
                LastY = Y;
                _needsUpdate = false;
            }
        }

        #region Mouse click

        public override bool Clicked(MouseButton button = MouseButton.Left)
        {
            bool clicked = false;

            if (button == MouseButton.Left)
                clicked = !MouseButtons[0] && LastMouseButtons[0];
            else if (button == MouseButton.Middle)
                clicked = !MouseButtons[1] && LastMouseButtons[1];
            else if (button == MouseButton.Right)
                clicked = !MouseButtons[2] && LastMouseButtons[2];

            return clicked;
        }

        protected override bool MouseButtonState(MouseButton button, ButtonState state)
        {
            bool value = state == ButtonState.Pressed ? true : false;
            bool result = false;

            switch (button)
            {
                case MouseButton.Left: result = MouseButtons[0] == value; break;
                case MouseButton.Middle: result = MouseButtons[1] == value; break;
                case MouseButton.Right: result = MouseButtons[2] == value; break;
            }

            return result;
        }

        #endregion

        #region Event handlers

        private void OnClickTimerDone(object sender, ElapsedEventArgs e)
        {
            LastMouseButtons[0] = MouseButtons[0];
            LastMouseButtons[1] = MouseButtons[1];
            LastMouseButtons[2] = MouseButtons[2];
        }

        private void OnMouseDown(object sender, WpfMouseButtonEventArgs e)
        {
            OnMouseMove(sender, e);

            MouseButtons[0] = e.LeftButton == WpfMouseButtonState.Pressed;
            MouseButtons[1] = e.MiddleButton == WpfMouseButtonState.Pressed;
            MouseButtons[2] = e.RightButton == WpfMouseButtonState.Pressed;
            _clickTimer.Start();
        }

        private void OnMouseLeave(object sender, WpfMouseEventArgs e)
        {
            MouseButtons[0] = false;
            MouseButtons[1] = false;
            MouseButtons[2] = false;
        }

        private void OnMouseMove(object sender, WpfMouseEventArgs e)
        {
            var position = WpfMouse.GetPosition(WpfApplication.Current.MainWindow);

            LastX = X;
            LastY = Y;
            
            X = (int)position.X;
            Y = (int)position.Y - 20;

            _needsUpdate = true;
        }

        private void OnMouseWheel(object sender, WpfMouseWheelEventArgs e)
        {
            _wheel += (float)e.Delta * 0.01f;
        }

        #endregion
    }
}