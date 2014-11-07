using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Hardware;
using Android.Content;
using Microsoft.Xna.Framework;

namespace C3DE.Demo
{
    [Activity(Label = "C3DE Demos"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@android:style/Theme.NoTitleBar.Fullscreen"
        , AlwaysRetainTaskState=true
        , LaunchMode=Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.Landscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
    public class MainActivity : AndroidGameActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            RequestWindowFeature (WindowFeatures.NoTitle);
            Window.SetFlags (WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            var g = new AndroidGame();
            var view = (View)g.Services.GetService(typeof(View));
            SetContentView(view);
            g.Run();
        }  
    }
}

