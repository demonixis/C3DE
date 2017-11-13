using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.VR
{
	public struct OpenHMDDesc
	{
		public int HorizontalResolution;
		public int VerticalResolution;
		public float HorizontalSize;
		public float VerticalSize;
		public float LensSeparation;
		public float LensVCenter;
		public float LeftEyeFov;
		public float RightEyeFov;
		public float LeftEyeAspect;
		public float RightEyeAspect;
		public Vector3[] DistortionK;
	}

	public class OpenHMDService : VRService
	{
		private IntPtr _context;
		private IntPtr _hmd;
		private Vector2 _distortionCenter;
		private OpenHMDDesc _hmdDescription;
		private Matrix[] _projectionMatrix;

		public OpenHMDService(Game game)
			: base(game)
		{
			_distortionCenter = new Vector2(0.5f, 0.5f);
		}

		public override int TryInitialize()
		{
            _context = OpenHMD.ohmd_ctx_create();
            if (_context == IntPtr.Zero)
                return -1;

            var deviceCount = OpenHMD.ohmd_ctx_probe(_context);
			if (deviceCount < 0)
				return -2;

			_hmd = OpenHMD.ohmd_list_open_device(_context, 0);
			if (_hmd == IntPtr.Zero)
				return -3;

			_hmdDescription = new OpenHMDDesc();
			_hmdDescription.HorizontalResolution = GetIntParameter(OpenHMD.ohmd_int_value.OHMD_SCREEN_HORIZONTAL_RESOLUTION);
			_hmdDescription.VerticalResolution = GetIntParameter(OpenHMD.ohmd_int_value.OHMD_SCREEN_VERTICAL_RESOLUTION);
			_hmdDescription.HorizontalSize = GetFloatParameters(OpenHMD.ohmd_float_value.OHMD_SCREEN_HORIZONTAL_SIZE, 1)[0];
			_hmdDescription.VerticalSize = GetFloatParameters(OpenHMD.ohmd_float_value.OHMD_SCREEN_VERTICAL_SIZE, 1)[0];
			_hmdDescription.LensSeparation = GetFloatParameters(OpenHMD.ohmd_float_value.OHMD_LENS_HORIZONTAL_SEPARATION, 1)[0];
			_hmdDescription.LensVCenter = GetFloatParameters(OpenHMD.ohmd_float_value.OHMD_LENS_VERTICAL_POSITION, 1)[0];
			_hmdDescription.LeftEyeFov = GetFloatParameters(OpenHMD.ohmd_float_value.OHMD_LEFT_EYE_FOV, 1)[0];
			_hmdDescription.RightEyeFov = GetFloatParameters(OpenHMD.ohmd_float_value.OHMD_RIGHT_EYE_FOV, 1)[0];
			_hmdDescription.LeftEyeAspect = GetFloatParameters(OpenHMD.ohmd_float_value.OHMD_LEFT_EYE_ASPECT_RATIO, 1)[0];
			_hmdDescription.RightEyeAspect = GetFloatParameters(OpenHMD.ohmd_float_value.OHMD_RIGHT_EYE_ASPECT_RATIO, 1)[0];

			var distortion = GetFloatParameters(OpenHMD.ohmd_float_value.OHMD_DISTORTION_K, 6);
			_hmdDescription.DistortionK = new Vector3[2];
			_hmdDescription.DistortionK[0] = new Vector3(distortion[0], distortion[1], distortion[2]);
			_hmdDescription.DistortionK[1] = new Vector3(distortion[3], distortion[4], distortion[5]);

			_projectionMatrix = new Matrix[2];
			_projectionMatrix[0] = Matrix.CreatePerspectiveFieldOfView(_hmdDescription.LeftEyeFov, _hmdDescription.LeftEyeAspect, 0.1f, 1000.0f);
			_projectionMatrix[1] = Matrix.CreatePerspectiveFieldOfView(_hmdDescription.RightEyeFov, _hmdDescription.RightEyeAspect, 0.1f, 1000.0f);

            DistortionEffect = Application.Content.Load<Effect>("Shaders/PostProcessing/OsvrDistortion");
            DistortionCorrectionRequired = true;

			return 0;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (_context != IntPtr.Zero)
				OpenHMD.ohmd_ctx_destroy(_context);
		}

		public override void ApplyDistortion(RenderTarget2D renderTarget, int eye)
		{
			DistortionEffect.Parameters["TargetTexture"].SetValue(renderTarget);
			DistortionEffect.Parameters["K1_Red"].SetValue(_hmdDescription.DistortionK[eye].X);
			DistortionEffect.Parameters["K1_Green"].SetValue(_hmdDescription.DistortionK[eye].Y);
			DistortionEffect.Parameters["K1_Blue"].SetValue(_hmdDescription.DistortionK[eye].Z);
			DistortionEffect.Parameters["Center"].SetValue(_distortionCenter);
			DistortionEffect.Techniques[0].Passes[0].Apply();
		}

		public override RenderTarget2D CreateRenderTargetForEye(int eye)
		{
			return new RenderTarget2D(Game.GraphicsDevice, _hmdDescription.VerticalResolution, _hmdDescription.VerticalResolution, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
		}

		public override Matrix GetProjectionMatrix(int eye)
		{
			return _projectionMatrix[eye];
		}

		public override Matrix GetViewMatrix(int eye, Matrix playerPose)
		{
			// TODO Not sure about that, extra testing required.
			var p = GetFloatParameters(OpenHMD.ohmd_float_value.OHMD_POSITION_VECTOR, 3);
			var r = GetFloatParameters(OpenHMD.ohmd_float_value.OHMD_ROTATION_QUAT, 4);

			var translation = Matrix.CreateTranslation(Vector3.Negate(new Vector3(p[0], p[1], p[2])));
			var rotation = Matrix.CreateFromQuaternion(Quaternion.Inverse(new Quaternion(r[0], r[1], r[2], r[3])));
			var viewMatrix = rotation * translation;

			// eye device rotation
			var pitch = 0.0f;
			var roll = 0.0f;
			var yaw = 0.0f;
			var eyeRotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll);

			return eyeRotation * viewMatrix;
		}

		public override float GetRenderTargetAspectRatio(int eye)
		{
			return eye == 0 ? _hmdDescription.LeftEyeAspect : _hmdDescription.RightEyeAspect;
		}

		public override int SubmitRenderTargets(RenderTarget2D renderTargetLeft, RenderTarget2D renderTargetRight)
		{
			return 0;
		}

		public int GetIntParameter(OpenHMD.ohmd_int_value value)
		{
			int result;
			OpenHMD.ohmd_device_geti(_context, value, out result);
			return result;
		}

        public float[] GetFloatParameters(OpenHMD.ohmd_float_value value, int arrayLength)
		{
			var result = new float[arrayLength];

			//Allocate the result memory, otherwise memory gets corrupted on read / write.
			var gch = GCHandle.Alloc(result, GCHandleType.Pinned);
			var f = gch.AddrOfPinnedObject();

			//Get the actual value.
			OpenHMD.ohmd_device_getf(_hmd, value, f);

			//Copy the returned IntPtr to the result array.
			Marshal.Copy(f, result, 0, arrayLength);

			//Free the memory.
			gch.Free();

			return result;
		}
	}
}
