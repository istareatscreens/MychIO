using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MychIO.Helper
{
    public static class WindowHandleHelper
    {
        // Import the GetActiveWindow function from user32.dll (Windows API)
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        // Import the GetForegroundWindow function from user32.dll (Windows API)
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // Import Unity's internal API to get the window handle (Windows only)
#if UNITY_EDITOR_WIN
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hwnd, uint wCmd);
        private const uint GW_OWNER = 4; // Used to get the window that owns the game window
#endif

        // Method to retrieve the HWND for the application window (not Unity Editor window)
        public static IntPtr GetHWND(Action<string> eventCallback)
        {
            IntPtr hwnd = IntPtr.Zero;

#if UNITY_EDITOR
            // Check if we are running inside the Unity Editor (play mode)
            hwnd = GetUnityGameWindowHWND();
            if (hwnd == IntPtr.Zero)
            {
                eventCallback("Failed to get application window handle in Unity Editor.");
            }
#else
            // In a release build (not in Unity Editor), GetForegroundWindow() works correctly
            hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
            {
                eventCallback("Failed to get application window handle in release build.");
            }
#endif

            return hwnd;
        }

        // In Unity Editor, get the game window HWND (even if not in foreground)
        private static IntPtr GetUnityGameWindowHWND()
        {
            IntPtr hwnd = IntPtr.Zero;

#if UNITY_EDITOR_WIN
            // Get the main Unity window handle (Editor window)
            hwnd = GetActiveWindow();

            // Check if the game window is owned by the Unity editor window
            if (hwnd != IntPtr.Zero)
            {
                hwnd = GetWindow(hwnd, GW_OWNER);
            }
#endif

            return hwnd;
        }

    }
}