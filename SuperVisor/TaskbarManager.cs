using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


namespace SuperVisor
{
    class TaskbarManager
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowA(string lpClassName, string lpWindowName);

        [DllImport("shell32.dll")]
        public static extern UInt32 SHAppBarMessage(
        UInt32 dwMessage,

        ref APPBARDATA pData);


        public enum AppBarMessages
        {
            New =
            0x00000000,
            Remove =
            0x00000001,
            QueryPos =
            0x00000002,
            SetPos =
            0x00000003,
            GetState =
            0x00000004,
            GetTaskBarPos =
            0x00000005,
            Activate =
            0x00000006,
            GetAutoHideBar =
            0x00000007,
            SetAutoHideBar =
            0x00000008,
            WindowPosChanged =
            0x00000009,
            SetState =
            0x0000000a
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public UInt32 cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public Int32 lParam;
        }


        public enum AppBarStates
        {
            AutoHide =
            0x00000001,
            AlwaysOnTop =
            0x00000002
        }


        public void SetTaskbar_AutoHide()
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
            msgData.hWnd = FindWindowA("Shell_TrayWnd", "");
            msgData.lParam = (Int32)(AppBarStates.AutoHide);

            SHAppBarMessage((UInt32)AppBarMessages.SetState, ref msgData);
        }

        public void ResetTaskbar_AutoHide()
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
            msgData.hWnd = FindWindowA("Shell_TrayWnd", "");
            msgData.lParam = (Int32)(AppBarStates.AlwaysOnTop);

            SHAppBarMessage((UInt32)AppBarMessages.SetState, ref msgData);
        }
    }
}
