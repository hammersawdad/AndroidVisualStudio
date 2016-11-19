using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Java.Lang;
using RobotController2.Model;
using static Android.Views.ViewGroup;

namespace RobotController2.Views
{
    public class JoystickView : View
    {
        // Constants
        public const long DEFAULT_LOOP_INTERVAL = 100; // 100 ms
        public const int FRONT = 3;
        public const int FRONT_RIGHT = 4;
        public const int RIGHT = 5;
        public const int RIGHT_BOTTOM = 6;
        public const int BOTTOM = 7;
        public const int BOTTOM_LEFT = 8;
        public const int LEFT = 1;
        public const int LEFT_FRONT = 2;

        // Variables
        //private Thread thread = new Thread();
        private int _positionX = 0; // Touch x position
        private int _positionY = 0; // Touch y position
        private double _centerX = 0; // Center view x position
        private double _centerY = 0; // Center view y position
        private Paint mainCircle;
        private Paint secondaryCircle;
        private Paint button;
        private Paint horizontalLine;
        private Paint verticalLine;
        private int _joystickRadius;
        private int _buttonRadius;
        private bool _isInitialDraw = true;

        public JoystickView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public JoystickView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        public event EventHandler<JoystickPositionEventArgs> PositionChanged;
        public event EventHandler<JoystickPositionEventArgs> PositionStop;

        private void Initialize()
        {
            mainCircle = new Paint(PaintFlags.AntiAlias);
            mainCircle.Color = Color.White;
            mainCircle.SetStyle(Paint.Style.FillAndStroke);

            secondaryCircle = new Paint();
            secondaryCircle.Color = Color.Green;
            secondaryCircle.SetStyle(Paint.Style.Stroke);

            verticalLine = new Paint();
            verticalLine.StrokeWidth = 5;
            verticalLine.Color = Color.Black;

            horizontalLine = new Paint();
            horizontalLine.StrokeWidth = 5;
            horizontalLine.Color = Color.Black;

            button = new Paint(PaintFlags.AntiAlias);
            button.Color = Color.Red;
            button.SetStyle(Paint.Style.Fill);
        }

        protected override void OnDraw(Canvas canvas)
        {
            // Set the center values
            _centerX = Measurements.GetWidth(this) / 2;
            _centerY = Measurements.GetHeight(this) / 2;

            // Unexplainable Offsets
            _centerX = _centerX * 1.5;
            _centerY = _centerY * 2;

            // painting the main circle
            canvas.DrawCircle((int)_centerX, (int)_centerY, _joystickRadius, mainCircle);
            // painting the secondary circle
            canvas.DrawCircle((int)_centerX, (int)_centerY, _joystickRadius / 2, secondaryCircle);

            // paint lines
            canvas.DrawLine((float)_centerX, (float)_centerY, (float)_centerX,
                    (float)(_centerY - _joystickRadius), verticalLine);
            canvas.DrawLine((float)(_centerX - _joystickRadius), (float)_centerY,
                    (float)(_centerX + _joystickRadius), (float)_centerY,
                    horizontalLine);
            canvas.DrawLine((float)_centerX, (float)(_centerY + _joystickRadius),
                    (float)_centerX, (float)_centerY, horizontalLine);

            // If this is the Initial drawing of the joystick,
            //  then center the control button
            if (_isInitialDraw)
            {
                _positionX = (int)_centerX;
                _positionY = (int)_centerY;
                _isInitialDraw = false;
            }

            // painting the move button
            canvas.DrawCircle(_positionX, _positionY, _buttonRadius, button);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            // before measure, get the center of view
            _positionX = Measurements.GetWidth(this) / 2;
            _positionY = Measurements.GetHeight(this) / 2;
            int d = System.Math.Min(w, h);
            _buttonRadius = (int)(d / 2 * 0.25);
            _joystickRadius = (int)(d / 2 * 0.75);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            int d = System.Math.Min(Measurements.Measure(widthMeasureSpec), Measurements.Measure(heightMeasureSpec));
            SetMeasuredDimension(d, d);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            _positionX = (int)e.GetX();
            _positionY = (int)e.GetY();

            double abs = System.Math.Sqrt((_positionX - _centerX) * (_positionX - _centerX)
                    + (_positionY - _centerY) * (_positionY - _centerY));
            if (abs > _joystickRadius)
            {
                _positionX = (int)((_positionX - _centerX) * _joystickRadius / abs + _centerX);
                _positionY = (int)((_positionY - _centerY) * _joystickRadius / abs + _centerY);
            }

            Invalidate();
            if (e.Action == MotionEventActions.Up)
            {
                _positionX = (int)_centerX;
                _positionY = (int)_centerY;

                // Raise the STOP event
                OnPositionStop();
            }
            else
            {
                // Raise the event
                OnPositionChanged();
            }

            return true;
        }

        protected virtual void OnPositionChanged()
        {
            // Report the X,Y coordinates in relationship to the center fo the joystick
            JoystickPositionEventArgs args = new JoystickPositionEventArgs();
            args.PositionX = ((int)_centerX - _positionX) * -1;
            args.PositionY = (int)_centerY - _positionY;

            PositionChanged?.Invoke(this, args);
        }

        protected virtual void OnPositionStop()
        {
            // Report the X,Y coordinates in relationship to the center fo the joystick
            JoystickPositionEventArgs args = new JoystickPositionEventArgs();
            args.PositionX = 0;
            args.PositionY = 0;

            PositionStop?.Invoke(this, args);
        }
    }

    public class JoystickPositionEventArgs : EventArgs
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
    }
}