using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace HotKeyNew.Services
{
    class HotKeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_CONTROL = 0x0002; //사용X
        private const uint KEY_I = 0x49;

        private const int HOTKEY_ID = 9000; // 임의의 ID
        private const int WM_HOTKEY = 0x0312;

        private IntPtr _windowHandle;
        private HwndSource _source;

        public event EventHandler HotKeyPressed;
        public bool Register(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            bool success = RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_ALT | MOD_SHIFT, KEY_I);
            if (success)
            {
                _source = HwndSource.FromHwnd(_windowHandle);
                _source.AddHook(WndProc);
            }
            return success;
        }

        public void Unregister()
        {
            if (_source != null)
            {
                _source.RemoveHook(WndProc);
            }
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
        }

        //콜백함수로 생각하면 편함
        //메시징 대기<->처리 반복 같은 느낌으로 보임
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int pressedId = wParam.ToInt32();
                if (pressedId == HOTKEY_ID)
                {
                    HotKeyPressed?.Invoke(this, EventArgs.Empty);
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }
    }
}
