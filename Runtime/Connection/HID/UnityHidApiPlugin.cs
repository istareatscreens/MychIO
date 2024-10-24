using System;
using System.Runtime.InteropServices;

namespace MychIO.Connection.HidDevice
{
    public static class UnityHidApiPlugin
    {
        // Define the callback delegates
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DataCallbackDelegate(IntPtr data);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void EventCallbackDelegate(string message);


        // Import the functions from the C++ plugin DLL
        [DllImport("UnityHidApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Initialize(
            int vendorId,
            int productId,
            int bufferSize,
            int leftBytesToTruncate,
            int bytesToRead
        );

        [DllImport("UnityHidApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Connect(
            IntPtr plugin,
            EventCallbackDelegate eventCallback
        );

        [DllImport("UnityHidApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Read(
            IntPtr plugin,
             DataCallbackDelegate dataRecievedCallback,
             EventCallbackDelegate eventCallback
        );

        [DllImport("UnityHidApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsConnected(IntPtr plugin);

        [DllImport("UnityHidApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsReading(IntPtr plugin);

        [DllImport("UnityHidApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Dispose(IntPtr plugin);

        [DllImport("UnityHidApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Disconnect(IntPtr plugin);

        [DllImport("UnityHidApiPlugin", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PluginLoaded();

    }
}