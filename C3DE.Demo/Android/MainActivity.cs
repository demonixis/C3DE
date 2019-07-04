using Android.App;
using Android.Views;
using Android.OS;
using Android.Content.PM;
using static C3DE.Demo.DemoGame;

namespace C3DE.Demo
{
    [Activity(Label = "MonoGame"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , AlwaysRetainTaskState = true
        , LaunchMode = LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.Landscape
        , ConfigurationChanges = ConfigChanges.Orientation |
        ConfigChanges.Keyboard |
        ConfigChanges.KeyboardHidden)]
    public class MainActivity : Microsoft.Xna.Framework.AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var g = new Engine();
            SetContentView((View)g.Services.GetService(typeof(View)));
            InitializeGame();
            g.Run();
        }
    }
}


