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

namespace RobotController2.Model
{
    public class RobotMessage
    {
        private static string ROBOT_MESSAGE_START = "{";
        private static string ROBOT_MESSAGE_END = "}";
        private static string ROBOT_MESSAGE_DELIMITER = ":";

        public static string FormatSteerMessage(Servo servoA, Servo servoB)
        {

            // TODO: Move this to its own Activity
            servoA.Offset = 4;
            servoB.Offset = 2;


            StringBuilder text = new StringBuilder();
            text.Append(servoA.CurrentRotationPosition + servoA.Offset);
            text.Append(" ");
            text.Append(servoB.CurrentRotationPosition + servoB.Offset);
            return text.ToString();
        }

        public static string FormatRobotMessage(int action, string message)
        {
            StringBuilder text = new StringBuilder();
            text.Append(ROBOT_MESSAGE_START);
            text.Append(action);
            text.Append(ROBOT_MESSAGE_DELIMITER);
            text.Append(message);
            text.Append(ROBOT_MESSAGE_END);
            return text.ToString();
        }
    }
}