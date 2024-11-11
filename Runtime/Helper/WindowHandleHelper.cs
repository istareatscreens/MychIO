using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MychIO.Helper
{
    public static class WindowHandleHelper
    {
        // (Windows API)
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        // (Windows API)
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // Method to retrieve the HWND for the application window (Windows only)
        public static IntPtr GetHWND(Action<string> eventCallback)
        {
            IntPtr hwnd = IntPtr.Zero;

            hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
            {
                eventCallback("Failed to get application window handle.");
            }

            return hwnd;
        }

    }
}