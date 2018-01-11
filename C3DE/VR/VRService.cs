using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.VR
{
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
        /// <param name="playerPose">Player scale.</param>
        public abstract Matrix GetViewMatrix(int eye, Matrix playerPose);

        /// <summary>
        /// Gets the position and the rotation of the given controller.
        /// </summary>
        /// <param name="hand">Which hand: 0 for left, 1 for right.</param>
        /// <param name="position">The position vector</param>
        /// <param name="rotation">The rotation vector</param>
        public virtual void GetHandTransform(int hand, ref Vector3 position, ref Quaternion rotation) { }

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

        public virtual int SubmitRenderTarget(int eye, RenderTarget2D renderTarget) => 0;

        /// <summary>
        /// Applies the distortion correction effect.
        /// </summary>
        /// <param name="renderTarget">Render target.</param>
        /// <param name="eye">Left or right eye.</param>
        public virtual void ApplyDistortion(RenderTarget2D renderTarget, int eye)
        {
        }
    }
}
