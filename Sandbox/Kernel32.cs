using System;
using System.Runtime.InteropServices;

namespace Sandbox
{
    public class Kernel32
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentThread();
        [DllImport("kernel32.dll")]
        public static extern IntPtr SetThreadAffinityMask(IntPtr th, IntPtr mask);
    }
}
