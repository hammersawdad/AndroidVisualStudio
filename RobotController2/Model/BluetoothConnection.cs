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
using System.IO;

namespace RobotController2.Model
{
    public class BluetoothConnection
    {
        private static int BLUETOOTH_THROTTLE_MILLISECONDS = 50;

        private static DateTime lastBlueToothTransmit = DateTime.Now;

        public static string BLUETOOTH_ID = "com.gtillett.robots.robotcontroller2.BLUETOOTH_ID";

        public static BluetoothAdapter Adapter;
        public static Stream OutputStream;


        public static void Write(string message)
        {
            // Ensure that we don't send messages too quickly
            DateTime currentTime = DateTime.Now;
            if (lastBlueToothTransmit.AddMilliseconds(BLUETOOTH_THROTTLE_MILLISECONDS) > currentTime)
            {
                return;
            }

            lastBlueToothTransmit = currentTime;

            try
            {
                if (OutputStream == null)
                {
                    Console.Write(message);
                }
                else
                {
                    OutputStream.Write(Encoding.ASCII.GetBytes(message), 0, message.Length);
                }
            }
            catch (IOException e)
            {
                // TODO: Log this somewhere
                throw;
            }
        }
    }
}