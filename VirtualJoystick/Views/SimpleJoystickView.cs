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

namespace VirtualJoystick
{
    public class SimpleJoystickView : View
    {
        const int NUM_BUBBLES = 5;
        int radius = 60;
        int radius_big = 180;

        public SimpleJoystickView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
        }

        public SimpleJoystickView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
        }

        private void drawBigCircle(Canvas canvas)
        {
            var paintCircle = new Paint() { Color = Color.White };
            canvas.DrawCircle((float)(Width / 2.0), (float)(Height / 2.0), radius_big, paintCircle);
        }

        protected override void OnDraw(Canvas canvas)
        {
            drawBigCircle(canvas);
        }
    }
}