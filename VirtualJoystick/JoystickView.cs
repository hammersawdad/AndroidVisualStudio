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

namespace VirtualJoystick
{
    public class JoystickView : View
    {
        // Constants
        private double RAD = 57.2957795;
        public static long DEFAULT_LOOP_INTERVAL = 100; // 100 ms
        public static int FRONT = 3;
        public static int FRONT_RIGHT = 4;
        public static int RIGHT = 5;
        public static int RIGHT_BOTTOM = 6;
        public static int BOTTOM = 7;
        public static int BOTTOM_LEFT = 8;
        public static int LEFT = 1;
        public static int LEFT_FRONT = 2;
        // Variables
        //private OnJoystickMoveListener onJoystickMoveListener; // Listener
        private Thread thread = new Thread();
        private long loopInterval = DEFAULT_LOOP_INTERVAL;
        private int xPosition = 0; // Touch x position
        private int yPosition = 0; // Touch y position
        private double centerX = 0; // Center view x position
        private double centerY = 0; // Center view y position
        private Paint mainCircle;
        private Paint secondaryCircle;
        private Paint button;
        private Paint horizontalLine;
        private Paint verticalLine;
        private int joystickRadius;
        private int buttonRadius;
        private int lastAngle = 0;
        private int lastPower = 0;

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
            verticalLine.Color = Color.Red;

            horizontalLine = new Paint();
            horizontalLine.StrokeWidth = 2;
            horizontalLine.Color = Color.Black;

            button = new Paint(PaintFlags.AntiAlias);
            button.Color = Color.Red;
            button.SetStyle(Paint.Style.Fill);
        }


        protected override void OnDraw(Canvas canvas)
        {
            // super.onDraw(canvas);

            centerX = GetWidth() / 2;
            centerY = GetHeight() / 2;

            // painting the main circle
            canvas.DrawCircle((int)centerX, (int)centerY, joystickRadius,
                    mainCircle);
            // painting the secondary circle
            canvas.DrawCircle((int)centerX, (int)centerY, joystickRadius / 2,
                    secondaryCircle);
            // paint lines
            canvas.DrawLine((float)centerX, (float)centerY, (float)centerX,
                    (float)(centerY - joystickRadius), verticalLine);
            canvas.DrawLine((float)(centerX - joystickRadius), (float)centerY,
                    (float)(centerX + joystickRadius), (float)centerY,
                    horizontalLine);
            canvas.DrawLine((float)centerX, (float)(centerY + joystickRadius),
                    (float)centerX, (float)centerY, horizontalLine);

            // painting the move button
            canvas.DrawCircle(xPosition, yPosition, buttonRadius, button);
        }

        private int GetWidth()
        {
            var metrics = Resources.DisplayMetrics;
            var widthInDp = ConvertPixelsToDp(metrics.WidthPixels);
            return widthInDp;
        }

        private int GetHeight()
        {
            var metrics = Resources.DisplayMetrics;
            var heightInDp = ConvertPixelsToDp(metrics.HeightPixels);
            return heightInDp;
        }

        private int ConvertPixelsToDp(float pixelValue)
        {
            var dp = (int)((pixelValue) / Resources.DisplayMetrics.Density);
            return dp;
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            // before measure, get the center of view
            xPosition = GetWidth() / 2;
            yPosition = GetWidth() / 2;
            int d = System.Math.Min(w, h);
            buttonRadius = (int)(d / 2 * 0.25);
            joystickRadius = (int)(d / 2 * 0.75);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            //base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            int d = System.Math.Min(measure(widthMeasureSpec), measure(heightMeasureSpec));
            SetMeasuredDimension(d, d);

        }

        private int measure(int measureSpec)
        {
            int result = 0;

            // Decode the measurement specifications.
            var specMode = MeasureSpec.GetMode(measureSpec);
            int specSize = MeasureSpec.GetSize(measureSpec);

            if (specMode == MeasureSpecMode.Unspecified)
            {
                // Return a default size of 200 if no bounds are specified.
                result = 200;
            }
            else
            {
                // As you want to fill the available space
                // always return the full available bounds.
                result = specSize;
            }
            return result;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            //return base.OnTouchEvent(e);

            xPosition = (int)e.GetX();
            yPosition = (int)e.GetY();

            double abs = System.Math.Sqrt((xPosition - centerX) * (xPosition - centerX)
                    + (yPosition - centerY) * (yPosition - centerY));
            if (abs > joystickRadius)
            {
                xPosition = (int)((xPosition - centerX) * joystickRadius / abs + centerX);
                yPosition = (int)((yPosition - centerY) * joystickRadius / abs + centerY);
            }

            Invalidate();
            if (e.Action == MotionEventActions.Up)
            {
                xPosition = (int)centerX;
                yPosition = (int)centerY;
                thread.Interrupt();

            }

            return true;
        }

        private int getAngle()
        {
            if (xPosition > centerX)
            {
                if (yPosition < centerY)
                {
                    return lastAngle = (int)(System.Math.Atan((yPosition - centerY)
                            / (xPosition - centerX))
                            * RAD + 90);
                }
                else if (yPosition > centerY)
                {
                    return lastAngle = (int)(System.Math.Atan((yPosition - centerY)
                            / (xPosition - centerX)) * RAD) + 90;
                }
                else
                {
                    return lastAngle = 90;
                }
            }
            else if (xPosition < centerX)
            {
                if (yPosition < centerY)
                {
                    return lastAngle = (int)(System.Math.Atan((yPosition - centerY)
                            / (xPosition - centerX))
                            * RAD - 90);
                }
                else if (yPosition > centerY)
                {
                    return lastAngle = (int)(System.Math.Atan((yPosition - centerY)
                            / (xPosition - centerX)) * RAD) - 90;
                }
                else
                {
                    return lastAngle = -90;
                }
            }
            else
            {
                if (yPosition <= centerY)
                {
                    return lastAngle = 0;
                }
                else
                {
                    if (lastAngle < 0)
                    {
                        return lastAngle = -180;
                    }
                    else
                    {
                        return lastAngle = 180;
                    }
                }
            }
        }

        private int getPower()
        {
            return (int)(100 * System.Math.Sqrt((xPosition - centerX)
                    * (xPosition - centerX) + (yPosition - centerY)
                    * (yPosition - centerY)) / joystickRadius);
        }

        private int getDirection()
        {
            if (lastPower == 0 && lastAngle == 0)
            {
                return 0;
            }

            int a = 0;
            if (lastAngle <= 0)
            {
                a = (lastAngle * -1) + 90;
            }
            else if (lastAngle > 0)
            {
                if (lastAngle <= 90)
                {
                    a = 90 - lastAngle;
                }
                else
                {
                    a = 360 - (lastAngle - 90);
                }
            }

            int direction = (int)(((a + 22) / 45) + 1);
            if (direction > 8)
            {
                direction = 1;
            }
            return direction;
        }
    }
}