using System;
using C3DE.Components;
using C3DE.Inputs;
using Microsoft.Xna.Framework;
using C3DE.Components.Controllers;

namespace C3DE.Demo.Scripts
{
	public class DeviceOrientationController : FirstPersonController
	{
		private Camera _camera;

		public override void Start()
		{
			_camera = GetComponent<Camera>();

			if (_camera == null)
				throw new Exception("No camera attached to this scene object.");
		}

		public override void Update()
		{
			//transform.Rotation = new Vector3 (Accelerometer.X, Accelerometer.Y, Accelerometer.Z);

			_camera.Target = transform.Position + Vector3.Transform(_camera.Reference, Matrix.CreateFromYawPitchRoll(transform.Rotation.Y, transform.Rotation.X, transform.Rotation.Z));
		}
	}
}