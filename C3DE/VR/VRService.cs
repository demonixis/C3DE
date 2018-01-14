using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.VR
{
    public enum XRButton
    {
        Trigger, Grip, Menu
    }

    public enum XRAxis
    {
        TouchpadX, TouchpadY
    }

    public enum XRAxis2D
    {
        Touchpad
    }

    /// <summary>
    /// Defines a VR Service used by the VRRenderer.
    /// </summary>
    public abstract class VRService : GameComponent
    {
        /// <summary>
        /// Gets or sets the preview render effect.
        /// </summary>
        /// <value>The preview render effect.</value>
        public virtual SpriteEffects PreviewRenderEffect { get; protected set; } = SpriteEffects.None;

        /// <summary>
        /// Gets or sets the distortion correction effect.
        /// </summary>
        public Effect DistortionEffect { get; protected set; }

        /// <summary>
        /// Enable or disable the distortion correction.
        /// </summary>
        public bool DistortionCorrectionRequired { get; protected set; }

        /// <summary>
        /// Creates a new VR Service.
        /// </summary>
        /// <param name="game">Game.</param>
        public VRService(Game game) : base(game) { }

        /// <summary>
        /// Tries to initialize the VR Device.
        /// </summary>
        /// <returns>Returns 0 if the VR device is ready, otherwise it return an error code.</returns>
        public abstract int TryInitialize();

        /// <summary>
        /// Creates the render target for the desired eye.
        /// </summary>
        /// <param name="eye">Left or right eye.</param>
        /// <returns>The render target.</returns>
        public abstract RenderTarget2D CreateRenderTargetForEye(int eye);

        /// <summary>
        /// Gets the projection matrix for the desired eye.
        /// </summary>
        /// <param name="eye">Left or right eye.</param>
        /// <returns>The projection matrix.</returns>
        public abstract Matrix GetProjectionMatrix(int eye);

        /// <summary>
        /// Gets the view matrix.
        /// </summary>
        /// <returns>The view matrix.</returns>
        /// <param name="eye">Left or right eye.</param>
        /// <param name="parent">Player scale.</param>
        public abstract Matrix GetViewMatrix(int eye, Matrix parent);

        /// <summary>
        /// Gets the render target aspect ratio.
        /// </summary>
        /// <returns>The render target aspect ratio.</returns>
        /// <param name="eye">Left or right eye.</param>
        public abstract float GetRenderTargetAspectRatio(int eye);

        /// <summary>
        /// Submits the render targets to the VR SDKs (Compositor).
        /// </summary>
        /// <returns>Returns 0 if the operation is a success, otherwise it returns an error code.</returns>
        /// <param name="renderTargetLeft">The renderTarget used for the left eye.</param>
        /// <param name="renderTargetRight">The renderTarget used for the right eye.</param>
        public abstract int SubmitRenderTargets(RenderTarget2D renderTargetLeft, RenderTarget2D renderTargetRight);

        /// <summary>
        /// Applies the distortion correction effect.
        /// </summary>
        /// <param name="renderTarget">Render target.</param>
        /// <param name="eye">Left or right eye.</param>
        public virtual void ApplyDistortion(RenderTarget2D renderTarget, int eye)
        {
        }

        #region Controllers

        /// <summary>
        /// Gets the local position of a given controller.
        /// </summary>
        /// <param name="hand">Which controller, 0 for the left hand and 1 for the right hand.</param>
        /// <param name="position">The position to update.</param>
        public virtual void GetLocalPosition(int hand, ref Vector3 position) { }

        /// <summary>
        /// Gets the local rotation of a given controller.
        /// </summary>
        /// <param name="hand">Which controller, 0 for the left hand and 1 for the right hand.</param>
        /// <param name="quaternion"></param>
        public virtual void GetLocalRotation(int hand, ref Quaternion quaternion) { }

        /// <summary>
        /// Indicates if the button is pressed.
        /// </summary>
        /// <param name="hand">Which controller, 0 for the left hand and 1 for the right hand.</param>
        /// <param name="button">The button.</param>
        /// <returns></returns>
        public virtual bool GetButton(int hand, XRButton button) => false;

        /// <summary>
        /// Indicates if the button was just pressed.
        /// </summary>
        /// <param name="hand">Which controller, 0 for the left hand and 1 for the right hand.</param>
        /// <param name="button">The button.</param>
        /// <returns></returns>
        public virtual bool GetButtonDown(int hand, XRButton button) => false;

        /// <summary>
        /// Indicates if the button was just released.
        /// </summary>
        /// <param name="hand">Which controller, 0 for the left hand and 1 for the right hand.</param>
        /// <param name="button">The button.</param>
        /// <returns></returns>
        public virtual bool GetButtonUp(int hand, XRButton button) => false;

        /// <summary>
        /// Gets the value of an axis.
        /// </summary>
        /// <param name="hand">Which controller, 0 for the left hand and 1 for the right hand.</param>
        /// <param name="axis">The desired axis.</param>
        /// <returns></returns>
        public virtual float GetAxis(int hand, XRAxis axis) => 0;

        /// <summary>
        /// Gets the value of a 2D axis.
        /// </summary>
        /// <param name="hand">Which controller, 0 for the left hand and 1 for the right hand.</param>
        /// <param name="axis">The desired axis.</param>
        /// <returns></returns>
        public virtual Vector2 GetAxis2D(int hand, XRAxis2D axis) => Vector2.Zero;

        #endregion
    }
}
