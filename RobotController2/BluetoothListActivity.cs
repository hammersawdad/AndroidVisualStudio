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

namespace RobotController2
{
    [Activity(Label = "BluetoothListActivity")]
    public class BluetoothListActivity : ListActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            string[] devices = { "one", "two", "three" };

            ListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, devices);
        }
    }
}