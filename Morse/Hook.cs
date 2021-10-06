using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Morse
{
    public struct POINT
    {
        public int X;
        public int Y;

        public static implicit operator Point(POINT point)
        {
            return new Point(point.X, point.Y);
        }
    }
    public class Hook
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        public static Point GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint); 
            return lpPoint;
        }
    }
}