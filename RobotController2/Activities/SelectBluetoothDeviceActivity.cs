using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using RobotController2.Model;
using Java.IO;

namespace RobotController2.Activities
{
    [Activity(Label = "SelectBluetoothDeviceActivity")]
    public class SelectBluetoothDeviceActivity : ListActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            //SetContentView(Resource.Layout.SelectBluetoothDevice);

            // Display the list of available devices
            DisplayExistingBoundDevices();
        }

        protected override void OnListItemClick(ListView l, View v, int position, long id)
        {
            ArrayAdapter<string> adapter = (ArrayAdapter<string>)l.Adapter;
            var device = adapter.GetItem(position);

            SelectBluetoothDevice(device);
        }

        private void DisplayExistingBoundDevices()
        {

            // Find the existing Bound devices
            BluetoothAdapter bluetoohthAdapter = BluetoothConnection.Adapter;
            ICollection<BluetoothDevice> existingPairedDevices = bluetoohthAdapter.BondedDevices;

            string[] deviceDescriptions = existingPairedDevices.Select(item => $"{item.Name}|{item.Address}").ToArray();

            ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, deviceDescriptions);
        }

        public void SelectBluetoothDevice(string selectedItem)
        {
            // Parse the string.  It is: [Bluetooth Name]\n[Bluetooth ID]
            string[] stringParts = selectedItem.Split('|');

            if (stringParts.Length == 2)
            {
                // Activate the Bluetooth Input/Output steams
                // (the Bluetooth ID is:  stringParts[1])
                if (ActivateBluetoothStreams(stringParts[1]))
                {
                    // The device was selected successfully.  End the Activity
                    Finish();
                }
                else
                {
                    Toast.MakeText(this, "Selected Bluetooth device was not enabled.", ToastLength.Long).Show();
                }
            }
            else
            {
                Toast.MakeText(this, "There was a problem selecting a Device.", ToastLength.Long).Show();
            }
        }

        private bool ActivateBluetoothStreams(string selectedBluetoothID)
        {
            BluetoothAdapter blueAdapter = BluetoothAdapter.DefaultAdapter;

            BluetoothDevice device = FindSelectedDevice(blueAdapter, selectedBluetoothID);

            // Verify that a device was found
            if (device == null)
            {
                Toast.MakeText(this, "Selected Bluetooth device was not found.", ToastLength.Long).Show();
                return false;
            }

            // Activate the Input and Output streams
            bool success = false;
            try
            {
                ParcelUuid[] uuids = device.GetUuids();
                BluetoothSocket socket = device.CreateRfcommSocketToServiceRecord(uuids[0].Uuid);
                socket.Connect();

                // Store the output stream in a static variable
                BluetoothConnection.OutputStream = socket.OutputStream;

                //inStream = socket.getInputStream();

                success = true;
            }
            catch (IOException e)
            {
                // TODO: Log this exception somewhere

                Toast.MakeText(this, "Failure initializing steams.", ToastLength.Long).Show();
            }

            return success;
        }

        private BluetoothDevice FindSelectedDevice(BluetoothAdapter blueAdapter, String selectedBluetoothID)
        {

            if (blueAdapter == null) return null;
            if (!blueAdapter.IsEnabled) return null;

            BluetoothDevice deviceFound = null;
            ICollection<BluetoothDevice> bondedDevices = blueAdapter.BondedDevices;
            if (bondedDevices.Count() > 0)
            {
                try
                {
                    BluetoothDevice[] devices = bondedDevices.ToArray();

                    foreach (BluetoothDevice device in devices)
                    {
                        if (device.Address == selectedBluetoothID)
                        {
                            deviceFound = device;
                        }
                    }
                }
                catch (Exception e)
                {
                    // TODO: Log this error
                    throw;
                }
            }

            // If the device wasn't found
            return deviceFound;
        }
    }
}
