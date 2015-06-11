using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace C3DE.Inputs
{
    public enum InputProfileAxis
    {
         None = 0, X, Y
    }

    /// <summary>
    /// Define a standard input profile. It can be use one or more input types.
    /// </summary>
    public struct InputProfile
    {
        public bool HasKeyboard { get; set; }
        public bool HasGamepad { get; set; }
        public bool HasMouse { get; set; }
        public bool HasTouch { get; set; }
        public string Name { get; set; }
        public Keys PositiveKey { get; set; }
        public Keys NegativeKey { get; set; }
        public Buttons PositiveButton { get; set; }
        public Buttons NegativeButton { get; set; }
        public MouseButton MouseButton { get; set; }
        public InputProfileAxis Axis { get; set; }
        public Rectangle TouchZone { get; set; }
    
        internal static InputProfile[] CreateDefaultAxis()
        {
            var profiles = new InputProfile[4];

            profiles[0] = new InputProfile()
            {
                HasKeyboard = true,
                HasGamepad = true,
                HasMouse = false,
                HasTouch = true,
                Name = "Horizontal",
                PositiveKey = Keys.Right,
                NegativeKey = Keys.Left,
                PositiveButton = Buttons.LeftStick,
                NegativeButton = Buttons.LeftStick,
                Axis = InputProfileAxis.X,
                TouchZone = new Rectangle(Screen.VirtualWidthPerTwo, 0, Screen.VirtualWidthPerTwo, Screen.VirtualHeight)
            };

            profiles[1] = new InputProfile()
            {
                HasKeyboard = true,
                HasGamepad = true,
                HasMouse = false,
                HasTouch = true,
                Name = "Vertical",
                PositiveKey = Keys.Up,
                NegativeKey = Keys.Down,
                PositiveButton = Buttons.LeftStick,
                NegativeButton = Buttons.LeftStick,
                Axis = InputProfileAxis.Y,
                TouchZone = new Rectangle(0, 0, Screen.VirtualWidthPerTwo, Screen.VirtualHeight)
            };

            profiles[2] = new InputProfile()
            {
                HasKeyboard = false,
                HasGamepad = true,
                HasMouse = true,
                HasTouch = false,
                Name = "Mouse X",
                PositiveButton = Buttons.RightStick,
                NegativeButton = Buttons.RightStick,
                Axis = InputProfileAxis.X
            };

            profiles[3] = new InputProfile()
            {
                HasKeyboard = false,
                HasGamepad = true,
                HasMouse = true,
                HasTouch = false,
                Name = "Mouse Y",
                PositiveButton = Buttons.RightStick,
                NegativeButton = Buttons.RightStick,
                Axis = InputProfileAxis.Y
            };

            return profiles;
        }

        internal static InputProfile[] CreateDefaultButtons()
        {
            var profiles = new InputProfile[3];

            profiles[0] = new InputProfile()
            {
                HasKeyboard = true,
                HasGamepad = true,
                HasMouse = false,
                HasTouch = false,
                Name = "Exit",
                PositiveKey = Keys.Escape,
                PositiveButton = Buttons.Back,
                Axis = InputProfileAxis.None
            };

            profiles[1] = new InputProfile()
            {
                HasKeyboard = true,
                HasGamepad = true,
                HasMouse = false,
                HasTouch = false,
                Name = "Jump",
                PositiveKey = Keys.Space,
                PositiveButton = Buttons.B
            };

            profiles[2] = new InputProfile()
            {
                HasKeyboard = true,
                HasGamepad = true,
                HasMouse = false,
                HasTouch = false,
                Name = "Fire",
                PositiveKey = Keys.LeftControl,
                PositiveButton = Buttons.A
            };

            return profiles;
        }
    }
}
