using Microsoft.Xna.Framework;

namespace C3DE.Components.Controllers
{
    public class Controller : Behaviour
    {
        /// <summary>
        /// Gets or sets the velocity factor. Default is 0.95f.
        /// </summary>
        public float Velocity { get; set; }

        /// <summary>
        /// Gets or sets the angular velocity factor. Default is 0.95f.
        /// </summary>
        public float AngularVelocity { get; set; }

        /// <summary>
        /// Gets or sets the move speed. Default is 1.0f.
        /// </summary>
        public float MoveSpeed { get; set; }

        /// <summary>
        /// Gets or sets the rotation speed. Default is 0.1f.
        /// </summary>
        public float RotationSpeed { get; set; }

        /// <summary>
        /// Gets or sets the look speed. Default is 0.15f.
        /// </summary>
        public float LookSpeed { get; set; }

        /// <summary>
        /// Gets or sets the strafe speed. Default is 0.75f.
        /// </summary>
        public float StrafeSpeed { get; set; }

        /// <summary>
        /// Gets or sets the mouse sensibility. Default is [0.15f, 0.15f].
        /// </summary>
        public Vector2 MouseSensibility { get; set; }

        /// <summary>
        /// Gets or sets the gamepad sensibility. Default is [0.15f, 0.15f].
        /// </summary>
        public float GamepadSensibility { get; set; }
    }
}
