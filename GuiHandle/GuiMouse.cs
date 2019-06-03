
using System;
using System.Runtime.InteropServices;

namespace GuiHandle
{
    public static class GuiMouse
    {
        #region 指针移动
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public override string ToString()
            {
                return ("X:" + X + ", Y:" + Y);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool GetCursorPos(out POINT pt);



        [DllImport("user32.dll")] //引入dll
        static extern int SetCursorPos(int x, int y);

        public static POINT GetMousePos()
        {
            POINT pt;
            GetCursorPos(out pt);
            return pt;
        }

        public static void SetMousePos(POINT pt)
        {
            SetCursorPos(pt.X, pt.Y);
        }
        #endregion
        [DllImport("user32.dll")]
        static extern void mouse_event(MouseEventFlag flags, int dx, int dy, int buttons, UIntPtr extraInfo);

        static bool hasMouseDown = false;
        [Flags]
        enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }

        public static void MouseLeftDown()
        {
            mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
        }

        public static void MouseLeftUp()
        {
            mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">负数是下滑，正数是上滑 一般-100 +100</param>
        /// <returns></returns>
        public static void MouseScroll(int value)
        {
            mouse_event(MouseEventFlag.Wheel, 0, 0, value, UIntPtr.Zero);
        }
    }

    public static class GuiKeyboard
    {
        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        static extern void Keybd_event(
              int bvk,//虚拟键值 ESC键对应的是27
              int bScan,//0
              int dwFlags,//0为按下，1按住，2释放
              int dwExtraInfo//0
              );
        public static void KeyboardClick(int key)
        {
            Keybd_event(key, 0, 0, 0);
        }
    }
}