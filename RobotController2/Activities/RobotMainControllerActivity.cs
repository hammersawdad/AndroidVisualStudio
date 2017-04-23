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
using System.Timers;
using Android.Graphics;

namespace RobotController2.Activities
{
    /// <summary>
    /// Main Activity
    /// 
    /// WHEEL SPEEDS
    ///     Left wheel = second value in json
    ///         Stop = 90
    ///         Forward Slow = 110
    ///         Forward Fast = 130
    ///         Backward Slow = 85
    ///         Backward Fast = 75
    ///     Right wheel = first value in json
    ///         Stop = 92
    ///         Forward Slow = 85
    ///         Forward Fast = 75
    ///         Backward Slow = 105
    ///         Backward Fast = 130
    /// </summary>
    [Activity(Label = "RobotMainControllerActivity",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.Landscape //This is what controls orientation
    )]
    public class RobotMainControllerActivity : Activity
    {
        // FLAGS
        bool _lightOn = false;

        // ENUMERATIONS
        private enum ServoDirection { Forward, Stop, Backward }
        private enum RobotAction
        {
            Steer = 1,
            Light = 2,
            Shoulder = 3,
            Wrist = 4,
            Gripper = 5,
            Head = 6
        }
        private enum ButtonAction
        {
            HeadLeft,
            HeadRight,
            ShoulderOpen,
            ShoulderClose,
            WristOpen,
            WristClose,
            GripperOpen,
            GripperClose
        }


        // CONTROLS
        TextView _potisionTextView;
        ButtonAction _buttonAction;
        private Timer _timer;
        public object BluetoothConnector { get; private set; }

        private Button _headLeftButton;
        private Button _headRightButton;
        private Button _shoulderOpenButton;
        private Button _shoulderCloseButton;
        private Button _wristOpenButton;
        private Button _wristCloseButton;
        private Button _gripperOpenButton;
        private Button _gripperCloseButton;

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

            // Text
            _potisionTextView = (TextView)FindViewById(Resource.Id.PositionTextView);

            // Button Controls
            Button initializeButton = (Button)FindViewById(Resource.Id.InitializationButton);
            Button lightButton = (Button)FindViewById(Resource.Id.LightButton);
            initializeButton.Click += InitializeButton_Click;
            lightButton.Click += LightButton_Click;

            // Joystick
            JoystickView joystick = (JoystickView)FindViewById(Resource.Id.Joystick);
            joystick.PositionChanged += Joystick_PositionChanged;
            joystick.PositionStop += Joystick_PositionStop;

            // Head and Arm buttons
            _headLeftButton = (Button)FindViewById(Resource.Id.HeadLeftButton);
            _headRightButton = (Button)FindViewById(Resource.Id.HeadRightButton);
            _shoulderOpenButton = (Button)FindViewById(Resource.Id.ShoulderOpenButton);
            _shoulderCloseButton = (Button)FindViewById(Resource.Id.ShoulderCloseButton);
            _wristOpenButton = (Button)FindViewById(Resource.Id.WristOpenButton);
            _wristCloseButton = (Button)FindViewById(Resource.Id.WristCloseButton);
            _gripperOpenButton = (Button)FindViewById(Resource.Id.GripperOpenButton);
            _gripperCloseButton = (Button)FindViewById(Resource.Id.GripperCloseButton);

            _headLeftButton.Touch += Button_Touch;
            _headRightButton.Touch += Button_Touch;
            _shoulderOpenButton.Touch += Button_Touch;
            _shoulderCloseButton.Touch += Button_Touch;
            _wristOpenButton.Touch += Button_Touch;
            _wristCloseButton.Touch += Button_Touch;
            _gripperOpenButton.Touch += Button_Touch;
            _gripperCloseButton.Touch += Button_Touch;

            _timer = new Timer();
            _timer.Interval = 1;  // interval in milliseconds
            _timer.Enabled = false;
            _timer.Elapsed += TimerEvent;
            _timer.AutoReset = true;
        }

        private void Button_Touch(object sender, View.TouchEventArgs e)
        {
            if (sender == _headLeftButton)
            {
                _buttonAction = ButtonAction.HeadLeft;
            }
            if (sender == _headRightButton)
            {
                _buttonAction = ButtonAction.HeadRight;
            }
            if (sender == _shoulderOpenButton)
            {
                _buttonAction = ButtonAction.ShoulderOpen;
            }
            if (sender == _shoulderCloseButton)
            {
                _buttonAction = ButtonAction.ShoulderClose;
            }
            if (sender == _wristOpenButton)
            {
                _buttonAction = ButtonAction.WristOpen;
            }
            if (sender == _wristCloseButton)
            {
                _buttonAction = ButtonAction.WristClose;
            }
            if (sender == _gripperOpenButton)
            {
                _buttonAction = ButtonAction.GripperOpen;
            }
            if (sender == _gripperCloseButton)
            {
                _buttonAction = ButtonAction.GripperClose;
            }

            // Set the Timer
            SetTimer(e);
        }

        private void SetTimer(View.TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Down)
            {
                _timer.Enabled = true;
            }
            if (e.Event.Action == MotionEventActions.Up)
            {
                _timer.Enabled = false;
            }
        }

        private void TimerEvent(Object source, ElapsedEventArgs e)
        {
            switch (_buttonAction)
            {
                case ButtonAction.HeadLeft:
                    sendRobotMessage(RobotAction.Head, "OPEN");
                    break;
                case ButtonAction.HeadRight:
                    sendRobotMessage(RobotAction.Head, "CLOSE");
                    break;
                case ButtonAction.ShoulderOpen:
                    sendRobotMessage(RobotAction.Shoulder, "OPEN");
                    break;
                case ButtonAction.ShoulderClose:
                    sendRobotMessage(RobotAction.Shoulder, "CLOSE");
                    break;
                case ButtonAction.WristOpen:
                    sendRobotMessage(RobotAction.Wrist, "OPEN");
                    break;
                case ButtonAction.WristClose:
                    sendRobotMessage(RobotAction.Wrist, "CLOSE");
                    break;
                case ButtonAction.GripperOpen:
                    sendRobotMessage(RobotAction.Gripper, "OPEN");
                    break;
                case ButtonAction.GripperClose:
                    sendRobotMessage(RobotAction.Gripper, "CLOSE");
                    break;
            }
        }

        private void InitializeButton_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(RobotParametersActivity));
            StartActivity(intent);
        }

        private void LightButton_Click(object sender, EventArgs e)
        {
            if (_lightOn)
            {
                sendRobotMessage(RobotAction.Light, "OFF");
            }
            else
            {
                sendRobotMessage(RobotAction.Light, "ON");
            }
            _lightOn = !_lightOn;
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
            sendRobotMessage(RobotAction.Steer, message);
        }

        private int calculateSlowWheel(int fullSpeed, int turnValue)
        {
            int rotationPosition = fullSpeed; // default

            if (fullSpeed == RobotParameters.ClockwiseMaxSpeed)
            {
                if (turnValue < -80)
                {
                    rotationPosition = RobotParameters.ClockwiseSlowSpeed;
                }
                if (turnValue < -140)
                {
                    rotationPosition = RobotParameters.StopSpeed;
                }
            }

            if (fullSpeed == RobotParameters.CounterMaxSpeed)
            {
                if (turnValue > 80)
                {
                    rotationPosition = RobotParameters.CounterSlowSpeed;
                }
                if (turnValue > 140)
                {
                    rotationPosition = RobotParameters.StopSpeed;
                }
            }

            return rotationPosition;
        }

        private int calculateSlowWheel_SmoothTransition(int fullSpeed, int turnValue)
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

        private void sendRobotMessage(RobotAction action, String message)
        {
            // Format the message
            string robotMessage = RobotMessage.FormatRobotMessage((int)action, message);

            // Send the Bluetooth data
            BluetoothConnection.Write(robotMessage);
        }
    }
}