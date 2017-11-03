using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Components;
using Microsoft.Xna.Framework.Input;

namespace C3DE.VR
{
	/// <summary>
	/// A service used to simulate the VR view.
	/// </summary>
    public class NullVRService : VRService
    {
		private Vector3 _headRotation;

        public NullVRService(Game game) : base(game) { }

		public override int TryInitialize()
		{
			DistortionEffect = Game.Content.Load<Effect>("Shaders/PostProcessing/OsvrDistortion");
			DistortionCorrectionRequired = true;
			Game.Components.Add(this);
			return 0;
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			// Rotation.
			_headRotation.X -= Input.Mouse.Delta.Y * Time.DeltaTime * 0.25f;
			_headRotation.Y -= Input.Mouse.Delta.X * Time.DeltaTime * 0.25f;
		}

        public override RenderTarget2D CreateRenderTargetForEye(int eye)
        {
            return new RenderTarget2D(Game.GraphicsDevice, Screen.Width / 2, Screen.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
        }

        public override Matrix GetProjectionMatrix(int eye) => Camera.Main.projection;

		public override Matrix GetViewMatrix(int eye, Matrix playerPose)
        {
			var rotationMatrix = Matrix.CreateFromYawPitchRoll(_headRotation.Y, _headRotation.X, 0.0f);
			var target = Vector3.Transform(Vector3.Forward, rotationMatrix);
			return Matrix.CreateLookAt(Vector3.Zero, target, Vector3.Up);
		}

		public override float GetRenderTargetAspectRatio(int eye) => 1.0f;

		public override int SubmitRenderTargets(RenderTarget2D renderTargetLeft, RenderTarget2D renderTargetRight) => 0;

        public override void ApplyDistortion(RenderTarget2D renderTarget, int eye)
        {
            DistortionEffect.Parameters["TargetTexture"].SetValue(renderTarget);
            DistortionEffect.Parameters["K1_Red"].SetValue(1f);
            DistortionEffect.Parameters["K1_Green"].SetValue(1f);
            DistortionEffect.Parameters["K1_Blue"].SetValue(1f);
            DistortionEffect.Parameters["Center"].SetValue(new Vector2(0.5f, 0.5f));
            DistortionEffect.Techniques[0].Passes[0].Apply();
        }
	}
}
