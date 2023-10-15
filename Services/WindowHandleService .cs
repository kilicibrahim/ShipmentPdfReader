using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Maui.Controls;
using ShipmentPdfReader.Services.Interfaces;

namespace ShipmentPdfReader.Services
{
    public class WindowHandleService : IWindowHandleService
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public IntPtr GetMainWindowHandle()
        {
            // Get the process ID
            int processId = Process.GetCurrentProcess().Id;

            // Find the main window handle
            // Note: Adjust the window class name and window title as per your application
            IntPtr hwnd = FindWindow("YourWindowClassName", "YourWindowTitle");

            // Validate the handle
            if (hwnd == IntPtr.Zero)
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }

            return hwnd;
        }
    }
}
