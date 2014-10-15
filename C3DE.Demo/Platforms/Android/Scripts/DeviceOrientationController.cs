using C3DE.Components;
using C3DE.Inputs;
using C3DE.Components.Controllers;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Demo.Scripts
{
	public class DeviceOrientationController : FirstPersonController
	{
		private Camera _camera;
        private Accelerometer _accelSensor;
        private Vector3 _accelReading;
        private bool _accelStarted;

		public override void Start()
		{
			_camera = GetComponent<Camera>();

            _accelStarted = true;

            _accelSensor = new Accelerometer();

            try 
            {
                _accelSensor.Start();
                _accelSensor.CurrentValueChanged += OnAccelChanged;
            }
            catch (Exception e)
            {
                _accelStarted = false;
            }

			if (_camera == null)
				throw new Exception("No camera attached to this scene object.");
		}

        public override void OnDestroy()
        {
            _accelSensor.Stop();
        }

        private void OnAccelChanged (object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            _accelReading = e.SensorReading.Acceleration;
            Debug.Log(_accelReading);
        }

		public override void Update()
		{
            if (_accelStarted)
            {
                transform.Rotation = new Vector3(MathHelper.PiOver2 - _accelReading.X, _accelReading.Y, -_accelReading.Z);
            } 

			_camera.Target = transform.Position + Vector3.Transform(_camera.Reference, Matrix.CreateFromYawPitchRoll(transform.Rotation.Y, transform.Rotation.X, transform.Rotation.Z));
		}
	}
}