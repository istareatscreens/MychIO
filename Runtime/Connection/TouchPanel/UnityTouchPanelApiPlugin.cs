using System;
using System.Runtime.InteropServices;
using MychIO.Helper;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MychIO.Connection.TouchPanelDevice
{

#if UNITY_EDITOR
    // This is to force cleanup of the plugin to prevent crashes in unity development environment
    [InitializeOnLoad]
#endif
    public static class UnityTouchPanelApiPlugin
    {

        static UnityTouchPanelApiPlugin()
        {
            ReloadPlugin();
        }


        // Define the callback delegates
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DataCallbackDelegate(IntPtr data);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void EventCallbackDelegate(string message);


        // Import the functions from the C++ plugin DLL
        [DllImport("UnityTouchPanelApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Initialize(
            int deviceClassification,
            IntPtr window_handle,
            int pollingRateMs
        );

        [DllImport("UnityTouchPanelApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Connect(
            IntPtr plugin,
            EventCallbackDelegate eventCallback
        );

        [DllImport("UnityTouchPanelApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Read(
            IntPtr plugin,
             DataCallbackDelegate dataRecievedCallback,
             EventCallbackDelegate eventCallback
        );

        [DllImport("UnityTouchPanelApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsConnected(IntPtr plugin);

        [DllImport("UnityTouchPanelApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsReading(IntPtr plugin);

        [DllImport("UnityTouchPanelApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Dispose(IntPtr plugin);

        [DllImport("UnityTouchPanelApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Disconnect(IntPtr plugin);

        [DllImport("UnityTouchPanelApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool StopReading(IntPtr plugin);

        // Plugin control

        [DllImport("UnityTouchPanelApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PluginLoaded();

        // This method should be called on program startup (To ensure all past pointers are released)
        [DllImport("UnityTouchPanelApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReloadPlugin();

        public static IntPtr Initialize(int deviceClassification, int pollingRateMs, Action<string> eventCallback)
        {
            IntPtr windowHandle = WindowHandleHelper.GetHWND(eventCallback);
            if (windowHandle == IntPtr.Zero)
            {
                eventCallback?.Invoke("Failed to get window handle.");
                return IntPtr.Zero;
            }

            return Initialize(deviceClassification, windowHandle, pollingRateMs);
        }


    }
}