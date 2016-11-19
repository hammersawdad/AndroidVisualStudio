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
using Android.Content.Res;

namespace VirtualJoystick.Models
{
    public class Measurements
    {
        private const double RAD = 57.2957795;


        public static int GetPower(int xPosition, int yPosition, double centerX, double centerY, int joystickRadius)
        {
            return (int)(100 * System.Math.Sqrt((xPosition - centerX)
                    * (xPosition - centerX) + (yPosition - centerY)
                    * (yPosition - centerY)) / joystickRadius);
        }

        public static int GetDirection(int lastPower, int lastAngle)
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

        public static int GetAngle(int xPosition, int yPosition, double centerX, double centerY, int lastAngle)
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

        public static int Measure(int measureSpec)
        {
            int result = 0;

            // Decode the measurement specifications.
            var specMode = View.MeasureSpec.GetMode(measureSpec);
            int specSize = View.MeasureSpec.GetSize(measureSpec);

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

        public static int GetWidth(View view)
        {
            var metrics = view.Resources.DisplayMetrics;
            var widthInDp = ConvertPixelsToDp(view, metrics.WidthPixels);
            return widthInDp;
        }

        public static int GetHeight(View view)
        {
            var metrics = view.Resources.DisplayMetrics;
            var heightInDp = ConvertPixelsToDp(view, metrics.HeightPixels);
            return heightInDp;
        }

        private static int ConvertPixelsToDp(View view, float pixelValue)
        {
            var dp = (int)((pixelValue) / view.Resources.DisplayMetrics.Density);
            return dp;
        }

    }
}