using System;
using System.Runtime.InteropServices;

namespace St.Common.Helper
{
    public class HotKeyApi
    {
        //Modifiers
        public const uint ModAlt = 0x0001;

        //Virtual Keys
        public const uint ZKey = 0x5A;

        //HotKey Id
        public const int ActivateHotKeyId = 9000;

        //HotKey Message Id
        public const int HotKeyMsgId = 0x0312;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
