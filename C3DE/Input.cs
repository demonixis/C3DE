using C3DE.Inputs;

namespace C3DE
{
    public class Input
    {
        public static KeyboardComponent Keys { get; set; }
        public static MouseComponent Mouse { get; set; }
        public static GamepadComponent Gamepad { get; set; }
        public static TouchComponent Touch { get; set; }
    }
}
