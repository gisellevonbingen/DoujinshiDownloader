using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DoujinshiDownloader
{
    internal class NativeMethods
    {
        public static int HWND_BROADCAST { get; } = 0xffff;
        public static int WM_ShowSingleInstance { get; } = RegisterWindowMessage("WM_Giselle.DoujinshiDownloader.ShowSingleInstance");

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();
    }

}
