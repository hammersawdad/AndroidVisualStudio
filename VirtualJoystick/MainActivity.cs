using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;

namespace VirtualJoystick
{
    [Activity(Label = "VirtualJoystick", MainLauncher = true, Icon = "@drawable/icon",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Landscape //This is what controls orientation
    )]
    public class MainActivity : Activity
    {
        TextView _positionTextView;
        JoystickView _joystick;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            // Initialize controls
            _positionTextView = FindViewById<TextView>(Resource.Id.PositionTextView);
            _joystick = FindViewById<JoystickView>(Resource.Id.Joystick);
            _joystick.PositionChanged += _joystick_PositionChanged;
        }

        private void _joystick_PositionChanged(object sender, JoystickPositionEventArgs e)
        {
            _positionTextView.Text = $"X:{e.PositionX}  Y:{e.PositionY}";
        }
    }
}

