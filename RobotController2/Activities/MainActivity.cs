using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Bluetooth;
using RobotController2.Model;
using Android.Content;

namespace RobotController2.Activities
{
    [Activity(
        Label = "RobotController", 
        MainLauncher = true, 
        Icon = "@drawable/icon", 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Landscape //This is what controls orientation
    )] 
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the layout resource
            SetContentView (Resource.Layout.Main);

            // Initialize the buttons
            Button bluetoothbutton = FindViewById<Button>(Resource.Id.BluetoothButton);
            bluetoothbutton.Click += BluetoothButton_Click;

            Button controllerButton = FindViewById<Button>(Resource.Id.ControllerButton);
            controllerButton.Click += ControllerButton_Click;
        }

        private void ControllerButton_Click(object sender, System.EventArgs e)
        {
            StartRobotControllerActivity();
        }

        private void BluetoothButton_Click(object sender, System.EventArgs e)
        {
            StartBlueToothController();
        }


        public void StartBlueToothController()
        {

            // Initialize Bluetooth on this device
            BluetoothAdapter adapter = InitializeDeviceBlueTooth();
            if (validateBoothtoothAdapter(adapter))
            {
                // set the adapter to use by the rest of the application
                BluetoothConnection.Adapter = adapter;

                // Bluetooth is enabled, so start the activity that selects the Device Connection
                StartSelectDeviceActivity();
            }
        }

        public BluetoothAdapter InitializeDeviceBlueTooth()
        {
            // take an instance of BluetoothAdapter - Bluetooth radio
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;

            if (adapter != null)
            {
                // If the BlueTooth Adapter is not enabled, then enable it
                if (!adapter.IsEnabled)
                {
                    Toast.MakeText(this, "Please Enable Bluetooth.", ToastLength.Short).Show();
                }
            }

            // Return the BluetoothAdapter
            return adapter;
        }
        private bool validateBoothtoothAdapter(BluetoothAdapter adapter)
        {
            if (adapter == null)
            {
                Toast.MakeText(this, "No Bluetooth Support found.", ToastLength.Short).Show();
                return false;
            }

            if (!adapter.IsEnabled)
            {
                Toast.MakeText(this, "Bluetooth was NOT enabled.", ToastLength.Short).Show();
                return false;
            }

            // Otherwise
            return true;
        }

        private void StartSelectDeviceActivity()
        {
            // Create an instance of the BlueToothController activity
            var selectBluetoothDeviceActivity = new Intent(this, typeof(SelectBluetoothDeviceActivity));
            StartActivity(selectBluetoothDeviceActivity);
        }
        private void StartRobotControllerActivity()
        {
            // Create an instance of the BlueToothController activity
            var robotMainControllerActivity = new Intent(this, typeof(RobotMainControllerActivity));
            StartActivity(robotMainControllerActivity);
        }
    }
}

