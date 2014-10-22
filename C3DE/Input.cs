using C3DE.Inputs;

namespace C3DE
{
    /// <summary>
    /// A static class to use inputs.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// Gets or sets keyboard inputs
        /// </summary>
        public static KeyboardComponent Keys { get; set; }

        /// <summary>
        /// Gets or sets mouse inputs.
        /// </summary>
        public static MouseComponent Mouse { get; set; }

        /// <summary>
        /// Gets or sets gamepad inputs.
        /// </summary>
        public static GamepadComponent Gamepad { get; set; }

        /// <summary>
        /// Gets or sets touch inputs.
        /// </summary>
        public static TouchComponent Touch { get; set; }
    }
}
