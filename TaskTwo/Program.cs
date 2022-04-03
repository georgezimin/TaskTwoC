using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;

namespace TaskTwo
{
    class Program
    {
        const int WM_GETTEXT = 0x000D;
        const int WM_GETTEXTLENGTH = 0x000E;
        const int WM_SETTEXT = 0x000C;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wparam, int lparam);

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [STAThread]
        static void Main(string[] args)
        {
            InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(new System.Globalization.CultureInfo("en-US"));
            Process process = Process.Start("notepad.exe");
            process.WaitForInputIdle(900);
            SetForegroundWindow(process.MainWindowHandle);
            IntPtr hEdit = FindWindowEx(process.MainWindowHandle, IntPtr.Zero, "Edit", IntPtr.Zero);
            Int32 size = SendMessage(hEdit, WM_GETTEXTLENGTH, 0, 0).ToInt32();
            StringBuilder text = new StringBuilder(size + 1);
            text.Append("ЭтоTest2;№%?");
            SendMessage(hEdit, (int)WM_SETTEXT, 0, text); // отправка сообщения SendMessage
            Clipboard.SetData(DataFormats.Text, "EtoТестовоеЗначение1№;%:");
            Thread.Sleep(1000);
            SendKeys.SendWait("^v"); // Вставка через ctrl+v
            TypeCustomMessage("EtoEsheTest324435#$%$#%"); // Еще вставка сендами но с учетом спецсимволов
            Thread.Sleep(1000);
            SendKeys.SendWait("^h"); //открываем окно заменить
            Dictionary<IntPtr, string> hwnds = new Dictionary<IntPtr, string>();
            EnumWindows((hWnd, lParam) => {
                if (IsWindowVisible(hWnd) && GetWindowTextLength(hWnd) != 0)
                {
                    hwnds.Add(hWnd, GetWindowText(hWnd));
                }
                return true;
            }, IntPtr.Zero);
            IntPtr editWindow = hwnds.Where(x => x.Value == "Заменить").FirstOrDefault().Key;
            IntPtr editFieldFrom = FindWindowByIndex(editWindow, 1);
            IntPtr editFieldTo = FindWindowByIndex(editWindow, 2);
            text.Clear();
            text.Append("ЭтоTest2;№%?");
            SendMessage(editFieldFrom, (int)WM_SETTEXT, 0, text); // вставляем что заменить
            text.Clear();
            text.Append("Мы все потеряли");
            SendMessage(editFieldTo, (int)WM_SETTEXT, 0, text); // вставляем на что заменить
            IntPtr editButton = FindWindowEx(editWindow, IntPtr.Zero, "Button", "Заменить &все");
            Thread.Sleep(1000);
            SendMessage(editButton, BM_CLICK, (IntPtr)1, (IntPtr)0);
            SendKeys.SendWait("^s"); // вызываем окно сохранения
            Thread.Sleep(1000);
            hwnds.Clear();
            EnumWindows((hWnd, lParam) => {
                if (IsWindowVisible(hWnd) && GetWindowTextLength(hWnd) != 0)
                {
                    hwnds.Add(hWnd, GetWindowText(hWnd));
                }
                return true;
            }, IntPtr.Zero);
            IntPtr saveWindow = hwnds.Where(x => x.Value == "Сохранение").FirstOrDefault().Key; //ищем вновь появившееся окно сохранить
            SetForegroundWindow(saveWindow);
            SendKeys.SendWait($@"C:\Users\{ Environment.UserName}\Desktop\sample.txt");
            IntPtr saveButton = FindWindowEx(saveWindow, IntPtr.Zero, "Button", "Со&хранить");
            SendMessage(saveButton, BM_CLICK, (IntPtr)1, (IntPtr)0); //сохраняем
            Thread.Sleep(1000);
            process.Kill();
        }

        public static void TypeCustomMessage(string message)
        {
            SendKeys.SendWait(Regex.Replace(message, @"(\+|\^|%|~|\(|\)|\[|]|\{|})", "{$1}"));
        }

        static IntPtr FindWindowByIndex(IntPtr hWndParent, int index)
        {
            if (index == 0)
                return hWndParent;
            else
            {
                int ct = 0;
                IntPtr result = IntPtr.Zero;
                do
                {
                    result = FindWindowEx(hWndParent, result, "Edit", IntPtr.Zero);
                    if (result != IntPtr.Zero)
                        ++ct;
                }
                while (ct < index && result != IntPtr.Zero);
                return result;
            }
        }

        static string GetWindowText(IntPtr hWnd)
        {
            int len = GetWindowTextLength(hWnd) + 1;
            StringBuilder sb = new StringBuilder(len);
            len = GetWindowText(hWnd, sb, len);
            return sb.ToString(0, len);
        }
        
        const int
            WM_LBUTTONDOWN = 513,
            WM_LBUTTONUP = 514,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_KEYDOWN = 256,
            WM_CHAR = 258,
            WM_KEYUP = 257,
            WM_SETFOCUS = 7,
            WM_SYSCOMMAND = 274,
            
            WM_CLEAR = 0x303,
            WM_PAINT = 15,
            WM_SETCURSOR = 32,
            WM_KILLFOCUS = 8,
            WM_NCHITTEST = 132,
            WM_USER = 1024,
            WM_MOUSEACTIVATE = 33,
            WM_MOUSEMOVE = 512,
            WM_LBUTTONDBLCLK = 515,
            WM_COMMAND = 273,
            VK_DOWN = 0x28,
            VK_RETURN = 0x0D,
            BM_SETSTATE = 243,
            BM_CLICK = 0x00F5,
            SW_HIDE = 0,
            SW_MAXIMIZE = 3,
            SW_MINIMIZE = 6,
            SW_RESTORE = 9,
            SW_SHOW = 5,
            SW_SHOWDEFAULT = 10,
            SW_SHOWMAXIMIZED = 3,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOWNORMAL = 1,
            SC_MINIMIZE = 32,
            EM_SETSEL = 0x00B1,
            CAPACITY = 256,
            CB_SETCURSEL = 0x014E;
    }
}
