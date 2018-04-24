using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Text;
using System;

namespace Time_Vote_Bot
{
    public delegate bool Win32Callback(voidptr hwnd, voidptr lParam);

    public unsafe readonly struct voidptr
    {
        private readonly void* _ptr;

        public static int Size => sizeof(voidptr);

        public static voidptr nullptr => default;


        public voidptr(void* p) => _ptr = p;

        public override bool Equals(object obj)
        {
            if (obj is voidptr other)
                return other == this;

            return false;
        }

        public override int GetHashCode() => ((ulong)_ptr).GetHashCode();

        public override string ToString() => Size == sizeof(uint) ? $"0x{(uint)_ptr:x8}" : $"0x{(ulong)_ptr:x16}";

        public static bool operator true(voidptr p) => p != nullptr;
        public static bool operator false(voidptr p) => p == nullptr;
        public static bool operator ==(voidptr p1, voidptr p2) => p1._ptr == p2._ptr;
        public static bool operator !=(voidptr p1, voidptr p2) => !(p1 == p2);
        public static bool operator <(voidptr p1, voidptr p2) => (ulong)p1._ptr < (ulong)p2._ptr;
        public static bool operator <=(voidptr p1, voidptr p2) => (ulong)p1._ptr <= (ulong)p2._ptr;
        public static bool operator >(voidptr p1, voidptr p2) => (ulong)p1._ptr > (ulong)p2._ptr;
        public static bool operator >=(voidptr p1, voidptr p2) => (ulong)p1._ptr >= (ulong)p2._ptr;
        public static voidptr operator ^(voidptr p1, voidptr p2) => (ulong)p1._ptr ^ (ulong)p2._ptr;
        public static voidptr operator &(voidptr p1, voidptr p2) => (ulong)p1._ptr & (ulong)p2._ptr;
        public static voidptr operator |(voidptr p1, voidptr p2) => (ulong)p1._ptr | (ulong)p2._ptr;
        public static voidptr operator ~(voidptr p) => ~(ulong)p._ptr;
        public static voidptr operator -(voidptr p) => -(long)p._ptr;
        public static voidptr operator +(voidptr p) => p;
        public static voidptr operator +(voidptr p1, voidptr p2) => (ulong)p1._ptr + (ulong)p2._ptr;
        public static voidptr operator -(voidptr p1, voidptr p2) => (ulong)p1._ptr - (ulong)p2._ptr;
        public static voidptr operator *(voidptr p1, voidptr p2) => (ulong)p1._ptr * (ulong)p2._ptr;
        public static voidptr operator /(voidptr p1, voidptr p2) => (ulong)p1._ptr / (ulong)p2._ptr;
        public static voidptr operator %(voidptr p1, voidptr p2) => (ulong)p1._ptr % (ulong)p2._ptr;
        public static implicit operator voidptr(void* p) => new voidptr(p);
        public static implicit operator voidptr(GCHandle p) => GCHandle.ToIntPtr(p);
        public static implicit operator voidptr(IntPtr p) => (void*)p;
        public static implicit operator voidptr(ulong p) => (void*)p;
        public static implicit operator voidptr(long p) => (void*)p;
        public static implicit operator void* (voidptr p) => p._ptr;
        public static implicit operator GCHandle(voidptr p) => GCHandle.FromIntPtr(p);
        public static implicit operator IntPtr(voidptr p) => (IntPtr)p._ptr;
        public static implicit operator ulong(voidptr p) => (ulong)p._ptr;
        public static implicit operator long(voidptr p) => (long)p._ptr;
        public static implicit operator uint(voidptr p) => (uint)p._ptr;
        public static implicit operator int(voidptr p) => (int)p._ptr;
    }

    public static class Native
    {
        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(voidptr hWnd, out int lpdwProcessId);

        [DllImport("user32.Dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(voidptr parentHandle, Win32Callback callback, voidptr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(voidptr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowText(voidptr hWnd, StringBuilder sb, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int CloseWindow(voidptr hWnd);

        [DllImport("user32.dll")]
        public static extern int DestroyWindow(voidptr hWnd);

        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool Focus(this voidptr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern voidptr SendMessage(voidptr hWnd, uint Msg, voidptr wParam, voidptr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(voidptr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(voidptr hWnd, voidptr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);


        public static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(0x0002, xpos, ypos, 0, 0);
            Thread.Sleep(20);
            mouse_event(0x0004, xpos, ypos, 0, 0);
        }

        public static (voidptr hwnd, string title)[] GetAllWindows()
        {
            List<(voidptr, string)> hwnds = new List<(voidptr, string)>();

            foreach (Process p in Process.GetProcesses())
                try
                {
                    hwnds.AddRange(GetRootWindowsOfProcess(p.Id).Select(x =>
                    {
                        int len = GetWindowTextLength(x);
                        StringBuilder sb = new StringBuilder(len + 1);

                        GetWindowText(x, sb, len + 1);

                        return (x, sb.ToString());
                    }));
                }
                catch
                {
                }

            return hwnds.ToArray();
        }

        private static IEnumerable<voidptr> GetRootWindowsOfProcess(int pid)
        {
            foreach (voidptr hwnd in GetChildWindows(null))
            {
                GetWindowThreadProcessId(hwnd, out int lpdwProcessId);

                if (lpdwProcessId == pid)
                    yield return hwnd;
            }
        }

        private static List<voidptr> GetChildWindows(voidptr parent)
        {
            bool EnumWindow(voidptr handle, voidptr pointer)
            {
                GCHandle gch = pointer;
                List<voidptr> list = gch.Target as List<voidptr>;

                if (list == null)
                    throw new InvalidCastException("GCHandle Target could not be cast as List<voidptr>");

                list.Add(handle);

                return true;
            }


            List<voidptr> result = new List<voidptr>();
            GCHandle listHandle = GCHandle.Alloc(result);

            try
            {
                EnumChildWindows(parent, EnumWindow, listHandle);
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }

            return result;
        }

        public static void SendKeysTo(voidptr hwnd, string keys, bool wait = true)
        {
            hwnd.Focus();

            if (wait)
                Thread.Sleep(1);

            SendKeys.SendWait(keys);

            if (wait)
                SendKeys.Flush();
        }

        public static void SendTextTo(voidptr hwnd, string text)
        {
            void send(string s) => SendKeys.SendWait(s);

            Thread.Sleep(200);

            foreach (char c in text)
            {
                hwnd.Focus();

                Thread.Sleep(10);

                if (char.IsLetter(c))
                {
                    char lw = char.ToLower(c);

                    if (char.IsUpper(c))
                        send($"+{lw}");
                    else
                        send(lw.ToString());
                }
                else if (char.IsDigit(c))
                    send(c.ToString());
                else
                    switch (c)
                    {
                        case '!':
                        case '"':
                        case '$':
                        case '&':
                        case '=':
                        case '?':
                        case '´':
                        case '#':
                        case '\'':
                        case '\\':
                        case '<':
                        case '>':
                        case '|':
                        case '_':
                        case '.':
                        case ':':
                        case ';':
                        case ',':
                        case '°':
                        case ' ':
                            send(c.ToString());

                            break;
                        case '{':
                        case '}':
                        case '[':
                        case ']':
                        case '(':
                        case ')':
                        case '%':
                        case '+':
                        case '~':
                        case '^':
                            send($"{{{c}}}");

                            break;
                        case '/':
                            send("{DIVIDE}");

                            break;
                        case '\r':
                        case '\n':
                            send("{ENTER}");

                            break;
                        case '\t':
                            send("{TAB}");

                            break;
                        case '-':
                            send("{SUBTRACT}");

                            break;
                        case '*':
                            send("{MULTIPLY}");

                            break;
                    }

                Thread.Sleep(10);
                SendKeys.Flush();
            }
        }
    }
}
