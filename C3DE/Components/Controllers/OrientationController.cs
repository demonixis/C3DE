using C3DE.Components;
using C3DE.Inputs;
using C3DE.Components.Controllers;
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;
using System;

namespace C3DE.Components.Controllers
{
    public class OrientationController : Controller
	{
		private Camera _camera;
        private Accelerometer _accelSensor;
        private Vector3 _accelReading;
        private Vector3 _lastAccelReading;
        private Vector3 _delta;
        private bool _accelStarted;

		public override void Start()
		{
            _accelStarted = true;
            _accelSensor = new Accelerometer();
            _camera = GetComponent<Camera>();

            try 
            {
                _accelSensor.Start();
                _accelSensor.CurrentValueChanged += OnAccelChanged;
            }
            catch (Exception ex)
            {
                _accelStarted = false;
                Debug.Log(ex.Message);
            }

			if (_camera == null)
				throw new Exception("No camera attached to this scene object.");
		}

        public override void OnDestroy()
        {
            if (_accelStarted)
                _accelSensor.Stop();
        }

        private void OnAccelChanged (object sender, SensorReadingEventArgs<AccelerometerReading> e)
        {
            _lastAccelReading.X = _accelReading.X;
            _lastAccelReading.Y = _accelReading.Y;
            _lastAccelReading.Z = _accelReading.Z;

            _accelReading.X = Round(e.SensorReading.Acceleration.X);
            _accelReading.Y = Round(e.SensorReading.Acceleration.Y);
            _accelReading.Z = Round(e.SensorReading.Acceleration.Z);

            _delta.X = _accelReading.X - _lastAccelReading.X;
            _delta.Y = _accelReading.Y - _lastAccelReading.Y;
            _delta.Z = _accelReading.Z - _lastAccelReading.Z;
        }

		public override void Update()
		{
            if (_accelStarted)
            {
                if (_accelReading.Z < 0.0f)
                    _accelReading.X = 1 + (1 - _accelReading.X);

                _accelReading.Z = 0.0f;
              
                transform.Rotation = Vector3.Lerp(transform.Rotation, new Vector3(_accelReading.X * MathHelper.PiOver2 + MathHelper.ToRadians(90), MathHelper.TwoPi * _accelReading.Y, MathHelper.PiOver2 * _accelReading.Z), Time.DeltaTime * 16);
            } 

			_camera.Target = transform.Position + Vector3.Transform(_camera.Reference, Matrix.CreateFromYawPitchRoll(transform.Rotation.Y, transform.Rotation.X, transform.Rotation.Z));
        }

        private float Round(float value, float precision = 100)
        {
            return (float)(Math.Round(value * precision) / precision);
        }

        protected override void UpdateGamepadInput() { }
            
        protected override void UpdateInputs() { }
       
        protected override void UpdateKeyboardInput() { }

        protected override void UpdateMouseInput() { }

        protected override void UpdateTouchInput() { }
	}
}