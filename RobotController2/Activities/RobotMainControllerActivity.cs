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

namespace RobotController2.Activities
{
    [Activity(Label = "RobotMainControllerActivity")]
    public class RobotMainControllerActivity : Activity
    {

        //  CONSTANTS
        private static int ROBOT_ACTION_STEER = 1;
        private static int ROBOT_ACTION_LIGHT = 2;
        private static int SEEKBAR_CENTER_VALUE = 50;

        // ENUMERATIONS
        private enum ServoDirection { Forward, Stop, Backward }

        // CLASS SCOPED VARIABLES
        private ServoDirection _servoDirection;
        private SeekBar _seekBar;

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

            // Set the default direction to STOP
            _servoDirection = ServoDirection.Stop;

            // Setup methods for the Seekbar
            _seekBar = (SeekBar)FindViewById(Resource.Id.SeekBar);
            _seekBar.ProgressChanged += SeekBar_ProgressChanged;

            // Initialize Buttons
            Button initializeButton = (Button)FindViewById(Resource.Id.InitializationButton);
            Button onButton = (Button)FindViewById(Resource.Id.OnButton);
            Button offButton = (Button)FindViewById(Resource.Id.OffButton);
            Button forwardButton = (Button)FindViewById(Resource.Id.ForwardButton);
            Button backwardButton = (Button)FindViewById(Resource.Id.BackwardButton);
            Button stopButton = (Button)FindViewById(Resource.Id.StopButton);

            initializeButton.Click += InitializeButton_Click;
            onButton.Click += OnButton_Click;
            offButton.Click += OffButton_Click;
            forwardButton.Click += ForwardButton_Click;
            backwardButton.Click += BackwardButton_Click;
            stopButton.Click += StopButton_Click;


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
        private void ForwardButton_Click(object sender, EventArgs e)
        {
            // Recenter the steering bar
            recenterSteeringControl();

            // Move Forward
            _servoDirection = ServoDirection.Forward;
            SteerRobot(SEEKBAR_CENTER_VALUE);
        }
        private void BackwardButton_Click(object sender, EventArgs e)
        {
            // Recenter the steering bar
            recenterSteeringControl();

            // Move Forward
            _servoDirection = ServoDirection.Backward;
            SteerRobot(SEEKBAR_CENTER_VALUE);
        }
        private void StopButton_Click(object sender, EventArgs e)
        {
            // Recenter the steering bar
            recenterSteeringControl();

            // Stop
            _servoDirection = ServoDirection.Stop;
            SteerRobot(SEEKBAR_CENTER_VALUE);
        }

        private void SeekBar_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            // If the SeekBar changed due to touching it, then steer the robot
            if (e.FromUser)
            {
                SteerRobot(e.Progress);
            }
        }



        private void SteerRobot(int directionValue)
        {
            // The directionValue should be between 0 and 100
            //   a value of 50 is straight

            // Get the offset from center (always a positive number)
            int offsetFromCenter = calculateOffsetFromCenter(directionValue);

            // If moving FORWARD then ...
            if (_servoDirection == ServoDirection.Forward)
            {
                // If turning LEFT
                if (directionValue <= SEEKBAR_CENTER_VALUE)
                {
                    RobotParameters.ServoA.CurrentRotationPosition = RobotParameters.CounterMaxSpeed;
                    RobotParameters.ServoB.CurrentRotationPosition = calculateSlowWheel(RobotParameters.ClockwiseMaxSpeed, offsetFromCenter);
                }
                // If turning RIGHT
                if (directionValue > SEEKBAR_CENTER_VALUE)
                {
                    RobotParameters.ServoA.CurrentRotationPosition = calculateSlowWheel(RobotParameters.CounterMaxSpeed, offsetFromCenter);
                    RobotParameters.ServoB.CurrentRotationPosition = RobotParameters.ClockwiseMaxSpeed;
                }
            }


            // If moving BACKWARD then ...
            if (_servoDirection == ServoDirection.Backward)
            {
                // If turning LEFT
                if (directionValue <= SEEKBAR_CENTER_VALUE)
                {
                    RobotParameters.ServoA.CurrentRotationPosition = calculateSlowWheel(RobotParameters.ClockwiseMaxSpeed, offsetFromCenter);
                    RobotParameters.ServoB.CurrentRotationPosition = RobotParameters.CounterMaxSpeed;
                }
                // If turning RIGHT
                if (directionValue > SEEKBAR_CENTER_VALUE)
                {
                    RobotParameters.ServoA.CurrentRotationPosition = RobotParameters.ClockwiseMaxSpeed;
                    RobotParameters.ServoB.CurrentRotationPosition = calculateSlowWheel(RobotParameters.CounterMaxSpeed, offsetFromCenter);
                }
            }


            // If STOPPING then ...
            if (_servoDirection == ServoDirection.Stop)
            {
                // If turning LEFT
                RobotParameters.ServoA.CurrentRotationPosition = RobotParameters.StopSpeed;
                RobotParameters.ServoB.CurrentRotationPosition = RobotParameters.StopSpeed;
            }

            // Send the Message to the Robot
            string message = RobotMessage.FormatSteerMessage(RobotParameters.ServoA, RobotParameters.ServoB);
            updateText(message);
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

        private int calculateSlowWheel(int fullSpeed, int offsetFromCenter)
        {
            // Default to Full Speed
            int rotationPosition = fullSpeed;

            // Don't allow the return value to go past the mid-point (the STOP value)
            if (fullSpeed == RobotParameters.ClockwiseMaxSpeed)
            {
                rotationPosition = fullSpeed - offsetFromCenter;
                if (rotationPosition < RobotParameters.StopSpeed)
                {
                    rotationPosition = RobotParameters.StopSpeed;
                }
            }

            if (fullSpeed == RobotParameters.CounterMaxSpeed)
            {
                rotationPosition = fullSpeed + offsetFromCenter;
                if (rotationPosition > RobotParameters.StopSpeed)
                {
                    rotationPosition = RobotParameters.StopSpeed;
                }
            }

            return rotationPosition;
        }

        private void updateText(string text)
        {
            TextView textView = (TextView)FindViewById(Resource.Id.Text);
            textView.Text = text;
        }

        private void recenterSteeringControl()
        {
            // Recenter the steering bar
            SeekBar seekBar = (SeekBar)FindViewById(Resource.Id.SeekBar);
            seekBar.Progress = SEEKBAR_CENTER_VALUE;
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