using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Hardware;
using Android.Content;
using Microsoft.Xna.Framework;

namespace C3DE.Demo
{
    [Activity(Label = "C3DE.Demo"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState=true
        , LaunchMode=Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.Landscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
	public class MainActivity : AndroidGameActivity, ISensorEventListener
    {
		private SensorManager _sensorManager;
		private static readonly object _syncLock = new object();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            RequestWindowFeature (WindowFeatures.NoTitle);
            Window.SetFlags (WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

			_sensorManager = (SensorManager)GetSystemService (Context.SensorService);

            var g = new AndroidGame();
            SetContentView((View)g.Services.GetService(typeof(View)));
            g.Run();
        }

		protected override void OnResume()
		{
			base.OnResume ();
			_sensorManager.RegisterListener (this, _sensorManager.GetDefaultSensor (SensorType.Accelerometer), SensorDelay.Ui);
		}

		protected override void OnPause()
		{
			base.OnPause ();
			_sensorManager.UnregisterListener (this);
		}

		public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
		{

		}

		public void OnSensorChanged(SensorEvent e)
		{
			if (e.Sensor.Type == SensorType.Accelerometer) 
			{
				lock (_syncLock)
				{
					C3DE.Inputs.Accelerometer.X = MathHelper.ToRadians (e.Values [0] * 90.0f / 10.0f);
					C3DE.Inputs.Accelerometer.Y = MathHelper.ToRadians (e.Values [1] * 90.0f / 10.0f);
					C3DE.Inputs.Accelerometer.Z = MathHelper.ToRadians (e.Values [2] * 90.0f / 10.0f);
				}
			}
		}
    }
}

