using C3DE.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Editor.Components
{
    using WpfApplication = System.Windows.Application;
    using WpfMouse = System.Windows.Input.Mouse;
    using WpfMouseButton = System.Windows.Input.MouseButton;
    using WpfMouseButtonState = System.Windows.Input.MouseButtonState;
    using WpfMouseButtonEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
    using WpfMouseWheelEventArgs = System.Windows.Input.MouseWheelEventArgs;
    using System.Windows;

    public class EditorMouseComponent : MouseComponent
    {
        private UIElement _uiElement;
        private int _mouseWheel;
        private int _lastMouseWheel;
        private bool _needsUpdate;
        
        #region Fields

        public int LastX { get; set; }
        public int LastY { get; set; }
        public new int X { get; set; }
        public new int Y { get; set; }

        public new int Wheel
        {
            get { return _mouseWheel - _lastMouseWheel; }
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

        public EditorMouseComponent(Game game, UIElement uiElement)
            : base(game)
        {
            MouseButtons = new bool[3];
            LastMouseButtons = new bool[3];
            _sensibility = new Vector2(0.05f);
            _uiElement = uiElement;
            _needsUpdate = false;
        }

        public override void Initialize()
        {
            base.Initialize();

            _uiElement.MouseDown += OnMouseDown;
            _uiElement.MouseUp += OnMouseUp;
            _uiElement.MouseMove += OnMouseMove;
            _uiElement.MouseWheel += OnMouseWheel;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _uiElement.MouseDown -= OnMouseDown;
                _uiElement.MouseUp -= OnMouseUp;
                _uiElement.MouseMove -= OnMouseMove;
                _uiElement.MouseWheel -= OnMouseWheel;
            }
        }

        public override void Update(GameTime gameTime)
        {
            // Delta
            _delta.X = (X - LastX) * _sensibility.X;
            _delta.Y = (Y - LastY) * _sensibility.Y;

            if (_needsUpdate)
            {
                X = LastX;
                Y = LastY;
                _needsUpdate = false;
            }
        }

        #region Mouse click

        public new bool Clicked(MouseButton button = MouseButton.Left)
        {
            bool clicked = false;

            if (button == MouseButton.Left)
                clicked = MouseButtons[0] == false && LastMouseButtons[0] == true;
            else if (button == MouseButton.Middle)
                clicked = MouseButtons[1] == false && LastMouseButtons[1] == true;
            else if (button == MouseButton.Right)
                clicked = MouseButtons[1] == false && LastMouseButtons[2] == true;

            return clicked;
        }

        public new bool Down(MouseButton button)
        {
            return MouseButtonState(button, ButtonState.Pressed);
        }

        public new bool Up(MouseButton button = MouseButton.Left)
        {
            return MouseButtonState(button, ButtonState.Released);
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

        private void OnMouseDown(object sender, WpfMouseButtonEventArgs e)
        {
            LastMouseButtons[0] = MouseButtons[0];
            LastMouseButtons[1] = MouseButtons[1];
            LastMouseButtons[2] = MouseButtons[2];

            MouseButtons[0] = e.LeftButton == WpfMouseButtonState.Pressed;
            MouseButtons[1] = e.MiddleButton == WpfMouseButtonState.Pressed;
            MouseButtons[2] = e.RightButton == WpfMouseButtonState.Pressed;
        }

        private void OnMouseUp(object sender, WpfMouseButtonEventArgs e)
        {

            LastMouseButtons[0] = MouseButtons[0];
            LastMouseButtons[1] = MouseButtons[1];
            LastMouseButtons[2] = MouseButtons[2];

            MouseButtons[0] = e.LeftButton == WpfMouseButtonState.Pressed;
            MouseButtons[1] = e.MiddleButton == WpfMouseButtonState.Pressed;
            MouseButtons[2] = e.RightButton == WpfMouseButtonState.Pressed;
        }

        private void OnMouseMove(object sender, WpfMouseEventArgs e)
        {
            var position = WpfMouse.GetPosition(WpfApplication.Current.MainWindow);

            LastX = X;
            LastY = Y;

            X = (int)position.X;
            Y = (int)position.Y;

            _needsUpdate = true;
        }

        private void OnMouseWheel(object sender, WpfMouseWheelEventArgs e)
        {
            _lastMouseWheel = _mouseWheel;
            _mouseWheel = e.Delta;
            _needsUpdate = true;
        }
    }
}