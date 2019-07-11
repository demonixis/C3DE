using Microsoft.Xna.Framework;

namespace C3DE.Components.Controllers
{
    /// <summary>
    /// An abstract class to create a camera controller.
    /// </summary>
    public abstract class Controller : Behaviour
    {
        #region Fields

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
        public Vector2 GamepadSensibility { get; set; }

        /// <summary>
        /// Gets or sets the touch sensibility. Default is [1.0f, 1.0f].
        /// </summary>
        public float TouchSensibility { get; set; }

        public bool KeyboardEnabled { get; set; } = true;

        public bool MouseEnabled { get; set; } = true;

        public bool TouchEnabled { get; set; } = true;

        public bool GamepadEnabled { get; set; } = true;

        #endregion

        #region Input

        /// <summary>
        /// Method which call UpdateKeyboard/Mouse/GamepadTouch
        /// </summary>
        protected abstract void UpdateInputs();

        /// <summary>
        /// Update the keyboard input.
        /// </summary>
        protected abstract void UpdateKeyboardInput();

        /// <summary>
        /// Update the mouse input.
        /// </summary>
        protected abstract void UpdateMouseInput();

        /// <summary>
        /// Update the gamepad input.
        /// </summary>
        protected abstract void UpdateGamepadInput();

        /// <summary>
        /// Update the touch input.
        /// </summary>
        protected abstract void UpdateTouchInput();

        #endregion
    }
}
