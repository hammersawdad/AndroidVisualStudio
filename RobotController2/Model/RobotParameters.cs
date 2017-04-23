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
using Android.Preferences;

namespace RobotController2.Model
{
    class RobotParameters
    {
        public static int ClockwiseMaxSpeed = 180;
        public static int ClockwiseSlowSpeed = 110;
        public static int CounterMaxSpeed = 0;
        public static int CounterSlowSpeed = 85;
        public static int StopSpeed = 90;
        public static int SteeringSensitivityOffset = 40;
        public static int SteeringCenterZoneOffset = 5;

        public static Servo ServoA = new Servo();
        public static Servo ServoB = new Servo();

        //private static string SHARED_PREFERENCES_IDENTIFIER = "robots.robotcontroller2.RobotParameters";

        public static void WriteParametersToDisk(Context context)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            ISharedPreferencesEditor edit = prefs.Edit();
            edit.Clear();

            edit.PutInt("ClockwiseMaxSpeed", (ClockwiseMaxSpeed));
            edit.PutInt("CounterMaxSpeed", (CounterMaxSpeed));
            edit.PutInt("SteeringSensitivityOffset", (SteeringSensitivityOffset));
            edit.PutInt("SteeringCenterZoneOffset", (SteeringCenterZoneOffset));
            edit.PutInt("ServoAOffset", (ServoA.Offset));
            edit.PutInt("ServoBOffset", (ServoB.Offset));

            edit.Apply();
        }

        public static void ReadParametersFromDisk(Context context)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);

            ClockwiseMaxSpeed = prefs.GetInt("ClockwiseMaxSpeed", 180);
            CounterMaxSpeed = prefs.GetInt("CounterMaxSpeed", 0);
            SteeringSensitivityOffset = prefs.GetInt("SteeringSensitivityOffset", 40);
            SteeringCenterZoneOffset = prefs.GetInt("SteeringCenterZoneOffset", 5);
            ServoA.Offset = prefs.GetInt("ServoAOffset", 0);
            ServoB.Offset = prefs.GetInt("ServoBOffset", 0);
        }
    }
}