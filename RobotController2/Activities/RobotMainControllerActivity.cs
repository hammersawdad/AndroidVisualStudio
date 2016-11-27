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
using RobotController2.Model;
using RobotController2.Views;
using Android.Content.PM;

namespace RobotController2.Activities
{
    [Activity(Label = "RobotMainControllerActivity",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Landscape //This is what controls orientation
    )]
    public class RobotMainControllerActivity : Activity
    {
        //  CONSTANTS
        private static int ROBOT_ACTION_STEER = 1;
        private static int ROBOT_ACTION_LIGHT = 2;
        private static int SEEKBAR_CENTER_VALUE = 50;

        // ENUMERATIONS
        private enum ServoDirection { Forward, Stop, Backward }

        // CONTROLS
        TextView _potisionTextView;

        public object BluetoothConnector { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the layout resource
            SetContentView(Resource.Layout.RobotMainController);

            // Initialize controls
            initializeControls();
        }

        private void initializeControls()
        {
            // Load any saved settings
            RobotParameters.ReadParametersFromDisk(this);

            // Find Controls
            Button initializeButton = (Button)FindViewById(Resource.Id.InitializationButton);
            Button onButton = (Button)FindViewById(Resource.Id.OnButton);
            Button offButton = (Button)FindViewById(Resource.Id.OffButton);
            JoystickView joystick = (JoystickView)FindViewById(Resource.Id.Joystick);
            _potisionTextView = (TextView)FindViewById(Resource.Id.PositionTextView);

            // Wire Events
            initializeButton.Click += InitializeButton_Click;
            onButton.Click += OnButton_Click;
            offButton.Click += OffButton_Click;
            joystick.PositionChanged += Joystick_PositionChanged;
            joystick.PositionStop += Joystick_PositionStop;
        }

        private void InitializeButton_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(RobotParametersActivity));
            StartActivity(intent);
        }

        private void OnButton_Click(object sender, EventArgs e)
        {
            sendRobotMessage(ROBOT_ACTION_LIGHT, "ON");
        }

        private void OffButton_Click(object sender, EventArgs e)
        {
            sendRobotMessage(ROBOT_ACTION_LIGHT, "OFF");
        }

        private void Joystick_PositionChanged(object sender, JoystickPositionEventArgs e)
        {
            _potisionTextView.Text = $"X:{e.PositionX}  Y:{e.PositionY}";

            // Determine the Servo direction (Forward, Backward, Stop)
            // stop = 0, forward > 0, backward < 0
            ServoDirection servoDirection = ServoDirection.Stop;
            if (e.PositionY > 0) servoDirection = ServoDirection.Forward;
            if (e.PositionY < 0) servoDirection = ServoDirection.Backward;

            SteerRobot(servoDirection, e.PositionX);
        }

        private void Joystick_PositionStop(object sender, JoystickPositionEventArgs e)
        {
            // Wait milliseconds so the controller is sure to get the stop message
            System.Threading.Thread.Sleep(100);
            SteerRobot(ServoDirection.Stop, 0);
        }

        private void SteerRobot(ServoDirection servoDirection, int turnValue)
        {
            // If moving FORWARD then ...
            if (servoDirection == ServoDirection.Forward)
            {
                // If turning LEFT
                if (turnValue <= 0)
                {
                    RobotParameters.ServoA.CurrentRotationPosition = RobotParameters.CounterMaxSpeed;
                    RobotParameters.ServoB.CurrentRotationPosition = calculateSlowWheel(RobotParameters.ClockwiseMaxSpeed, turnValue);
                }
                // If turning RIGHT
                if (turnValue > 0)
                {
                    RobotParameters.ServoA.CurrentRotationPosition = calculateSlowWheel(RobotParameters.CounterMaxSpeed, turnValue);
                    RobotParameters.ServoB.CurrentRotationPosition = RobotParameters.ClockwiseMaxSpeed;
                }
            }

            // If moving BACKWARD then ...
            if (servoDirection == ServoDirection.Backward)
            {
                // If turning LEFT
                if (turnValue <= 0)
                {
                    RobotParameters.ServoA.CurrentRotationPosition = calculateSlowWheel(RobotParameters.ClockwiseMaxSpeed, turnValue);
                    RobotParameters.ServoB.CurrentRotationPosition = RobotParameters.CounterMaxSpeed;
                }
                // If turning RIGHT
                if (turnValue > 0)
                {
                    RobotParameters.ServoA.CurrentRotationPosition = RobotParameters.ClockwiseMaxSpeed;
                    RobotParameters.ServoB.CurrentRotationPosition = calculateSlowWheel(RobotParameters.CounterMaxSpeed, turnValue);
                }
            }

            // If STOPPING then ...
            if (servoDirection == ServoDirection.Stop)
            {
                // If turning LEFT
                RobotParameters.ServoA.CurrentRotationPosition = RobotParameters.StopSpeed;
                RobotParameters.ServoB.CurrentRotationPosition = RobotParameters.StopSpeed;
            }

            // Display the final speed
            _potisionTextView.Text += $"\r\nA:{RobotParameters.ServoA.CurrentRotationPosition}  B:{RobotParameters.ServoB.CurrentRotationPosition}";

            // Send the Message to the Robot
            string message = RobotMessage.FormatSteerMessage(RobotParameters.ServoA, RobotParameters.ServoB);
            sendRobotMessage(ROBOT_ACTION_STEER, message);
        }

        private int calculateOffsetFromCenter(int directionValue)
        {
            // Get the offset from center (always a positive number)
            int offsetFromCenter = Math.Abs(SEEKBAR_CENTER_VALUE - directionValue);

            // Determine if the seekbar is very close to the center.
            //  If so, then treat is as the center, to help drive straight
            if (offsetFromCenter <= RobotParameters.SteeringCenterZoneOffset)
            {
                // the seekbar is right near the center, so treat is as being dead center
                offsetFromCenter = 0;
            }
            else
            {
                // the seekbar is outside of the "padded" zone, so ...
                // 1. subtract the Padding Offset
                // 2. add the Steering Sensivity offset
                offsetFromCenter = offsetFromCenter - RobotParameters.SteeringCenterZoneOffset + RobotParameters.SteeringSensitivityOffset;
            }

            return offsetFromCenter;
        }

        private int calculateSlowWheel(int fullSpeed, int turnValue)
        {
            // The "turnValue" will be a range of around:   -200 to +200
            // The TARGET range is:                         0 to 180
            //      ClockwiseMaxSpeed   = 180
            //      StopSpeed           = 90
            //      CounterMaxSpeed     = 0

            // Each wheel turns in a different direction.  So:
            // If the full speed is Clockwise (180),
            //      then we subtract the absolute value of the "turnValue" from 180
            // If the full speed is Counter Clockwise (0), 
            //      then we add the absolute value of the "turnValue" to 0

            // Adjust the "turnValue" to the range of around:  0 to 100
            int adjustedTurnValue = Math.Abs(turnValue) / 2;

            // Set the rotaion speed of the "slow" wheel
            // Don't allow the return value to go past the mid-point (the STOP value)
            int rotationPosition = RobotParameters.StopSpeed; // default
            if (fullSpeed == RobotParameters.ClockwiseMaxSpeed)
            {
                rotationPosition = RobotParameters.ClockwiseMaxSpeed - adjustedTurnValue;
                if (rotationPosition < RobotParameters.StopSpeed)
                {
                    rotationPosition = RobotParameters.StopSpeed;
                }
            }

            if (fullSpeed == RobotParameters.CounterMaxSpeed)
            {
                rotationPosition = RobotParameters.CounterMaxSpeed + adjustedTurnValue;
                if (rotationPosition > RobotParameters.StopSpeed)
                {
                    rotationPosition = RobotParameters.StopSpeed;
                }
            }

            return rotationPosition;
        }

        private void sendRobotMessage(int action, String message)
        {
            // Format the message
            string robotMessage = RobotMessage.FormatRobotMessage(action, message);

            // Send the Bluetooth data
            BluetoothConnection.Write(robotMessage);
        }
    }
}