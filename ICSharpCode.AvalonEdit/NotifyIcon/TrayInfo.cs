using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Microsoft.Windows.Controls
{
    internal class AppBarInfo
    {
        private APPBARDATA m_data;

        public ScreenEdge Edge
        {
            get { return (ScreenEdge)m_data.uEdge; }
        }

        private unsafe Rectangle GetWorkArea()
        {
            var size = Marshal.SizeOf(typeof(RECT));
            fixed (void* pointer = new byte[size])
            {
                var result = WinApi.SystemParametersInfo(WinApi.SPI_GETWORKAREA, 0, pointer, 0);
                if (result == 1)
                {
                    var rectangle = *(RECT*)pointer;
                    return new Rectangle(rectangle.left, rectangle.top,
                        rectangle.right - rectangle.left,
                        rectangle.bottom - rectangle.top);
                }
                else
                    return default(Rectangle);
            }
        }

        public void GetPosition(string strClassName, string strWindowName)
        {
            m_data = new APPBARDATA();
            m_data.cbSize = Marshal.SizeOf(typeof(APPBARDATA));
            var hWnd = WinApi.FindWindow(strClassName, strWindowName);

            if (hWnd != IntPtr.Zero && WinApi.SHAppBarMessage(WinApi.ABM_GETTASKBARPOS, ref m_data) != 1)
                throw new Exception("Failed to communicate with the given AppBar");
        }

        /// <summary>
        /// Gets the position of the system tray.
        /// </summary>
        /// <returns>Tray coordinates.</returns>
        public Point GetTrayLocation()
        {
            this.GetPosition("Shell_TrayWnd", null);

            var rcWorkArea = this.GetWorkArea();
            int x = 0, y = 0;

            if (this.Edge == ScreenEdge.Left)
            {
                x = rcWorkArea.Left + 2;
                y = rcWorkArea.Bottom;
            }
            else if (this.Edge == ScreenEdge.Bottom)
            {
                x = rcWorkArea.Right;
                y = rcWorkArea.Bottom;
            }
            else if (this.Edge == ScreenEdge.Top)
            {
                x = rcWorkArea.Right;
                y = rcWorkArea.Top;
            }
            else if (this.Edge == ScreenEdge.Right)
            {
                x = rcWorkArea.Right;
                y = rcWorkArea.Bottom;
            }

            return new Point { X = x, Y = y };
        }
    }
}