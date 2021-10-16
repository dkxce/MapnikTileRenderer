using System;
using System.Collections.Generic;
using System.Text;

using System.Windows;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;

namespace SharedMemory
{
    // test
    class Program { static void Main(string[] args) {
            AnotherProcessMemory.Example();
            Console.WriteLine("---------------");
            SharedMemoryFile.Example();
            Console.ReadLine(); 
    }}

    public class MyForm : System.Windows.Forms.Form
    {
        const int WM_NULL = 0x00;
        const int WM_CREATE = 0x01;
        const int WM_DESTROY = 0x02;
        const int WM_MOVE = 0x03;
        const int WM_SIZE = 0x05;
        const int WM_ACTIVATE = 0x06;
        const int WM_SETFOCUS = 0x07;
        const int WM_KILLFOCUS = 0x08;
        const int WM_ENABLE = 0x0A;
        const int WM_SETREDRAW = 0x0B;
        const int WM_SETTEXT = 0x0C;
        const int WM_GETTEXT = 0x0D;
        const int WM_GETTEXTLENGTH = 0x0E;
        const int WM_PAINT = 0x0F;
        const int WM_CLOSE = 0x10;
        const int WM_QUERYENDSESSION = 0x11;
        const int WM_QUIT = 0x12;
        const int WM_QUERYOPEN = 0x13;
        const int WM_ERASEBKGND = 0x14;
        const int WM_SYSCOLORCHANGE = 0x15;
        const int WM_ENDSESSION = 0x16;
        const int WM_SYSTEMERROR = 0x17;
        const int WM_SHOWWINDOW = 0x18;
        const int WM_CTLCOLOR = 0x19;
        const int WM_WININICHANGE = 0x1A;
        const int WM_SETTINGCHANGE = 0x1A;
        const int WM_DEVMODECHANGE = 0x1B;
        const int WM_ACTIVATEAPP = 0x1C;
        const int WM_FONTCHANGE = 0x1D;
        const int WM_TIMECHANGE = 0x1E;
        const int WM_CANCELMODE = 0x1F;
        const int WM_SETCURSOR = 0x20;
        const int WM_MOUSEACTIVATE = 0x21;
        const int WM_CHILDACTIVATE = 0x22;
        const int WM_QUEUESYNC = 0x23;
        const int WM_GETMINMAXINFO = 0x24;
        const int WM_PAINTICON = 0x26;
        const int WM_ICONERASEBKGND = 0x27;
        const int WM_NEXTDLGCTL = 0x28;
        const int WM_SPOOLERSTATUS = 0x2A;
        const int WM_DRAWITEM = 0x2B;
        const int WM_MEASUREITEM = 0x2C;
        const int WM_DELETEITEM = 0x2D;
        const int WM_VKEYTOITEM = 0x2E;
        const int WM_CHARTOITEM = 0x2F;

        const int WM_SETFONT = 0x30;
        const int WM_GETFONT = 0x31;
        const int WM_SETHOTKEY = 0x32;
        const int WM_GETHOTKEY = 0x33;
        const int WM_QUERYDRAGICON = 0x37;
        const int WM_COMPAREITEM = 0x39;
        const int WM_COMPACTING = 0x41;
        const int WM_WINDOWPOSCHANGING = 0x46;
        const int WM_WINDOWPOSCHANGED = 0x47;
        const int WM_POWER = 0x48;
        const int WM_COPYDATA = 0x4A;
        const int WM_CANCELJOURNAL = 0x4B;
        const int WM_NOTIFY = 0x4E;
        const int WM_INPUTLANGCHANGEREQUEST = 0x50;
        const int WM_INPUTLANGCHANGE = 0x51;
        const int WM_TCARD = 0x52;
        const int WM_HELP = 0x53;
        const int WM_USERCHANGED = 0x54;
        const int WM_NOTIFYFORMAT = 0x55;
        const int WM_CONTEXTMENU = 0x7B;
        const int WM_STYLECHANGING = 0x7C;
        const int WM_STYLECHANGED = 0x7D;
        const int WM_DISPLAYCHANGE = 0x7E;
        const int WM_GETICON = 0x7F;
        const int WM_SETICON = 0x80;

        const int WM_NCCREATE = 0x81;
        const int WM_NCDESTROY = 0x82;
        const int WM_NCCALCSIZE = 0x83;
        const int WM_NCHITTEST = 0x84;
        const int WM_NCPAINT = 0x85;
        const int WM_NCACTIVATE = 0x86;
        const int WM_GETDLGCODE = 0x87;
        const int WM_NCMOUSEMOVE = 0xA0;
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int WM_NCLBUTTONUP = 0xA2;
        const int WM_NCLBUTTONDBLCLK = 0xA3;
        const int WM_NCRBUTTONDOWN = 0xA4;
        const int WM_NCRBUTTONUP = 0xA5;
        const int WM_NCRBUTTONDBLCLK = 0xA6;
        const int WM_NCMBUTTONDOWN = 0xA7;
        const int WM_NCMBUTTONUP = 0xA8;
        const int WM_NCMBUTTONDBLCLK = 0xA9;

        const int WM_KEYFIRST = 0x100;
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_CHAR = 0x102;
        const int WM_DEADCHAR = 0x103;
        const int WM_SYSKEYDOWN = 0x104;
        const int WM_SYSKEYUP = 0x105;
        const int WM_SYSCHAR = 0x106;
        const int WM_SYSDEADCHAR = 0x107;
        const int WM_KEYLAST = 0x108;

        const int WM_IME_STARTCOMPOSITION = 0x10D;
        const int WM_IME_ENDCOMPOSITION = 0x10E;
        const int WM_IME_COMPOSITION = 0x10F;
        const int WM_IME_KEYLAST = 0x10F;

        const int WM_INITDIALOG = 0x110;
        const int WM_COMMAND = 0x111;
        const int WM_SYSCOMMAND = 0x112;
        const int WM_TIMER = 0x113;
        const int WM_HSCROLL = 0x114;
        const int WM_VSCROLL = 0x115;
        const int WM_INITMENU = 0x116;
        const int WM_INITMENUPOPUP = 0x117;
        const int WM_MENUSELECT = 0x11F;
        const int WM_MENUCHAR = 0x120;
        const int WM_ENTERIDLE = 0x121;

        const int WM_CTLCOLORMSGBOX = 0x132;
        const int WM_CTLCOLOREDIT = 0x133;
        const int WM_CTLCOLORLISTBOX = 0x134;
        const int WM_CTLCOLORBTN = 0x135;
        const int WM_CTLCOLORDLG = 0x136;
        const int WM_CTLCOLORSCROLLBAR = 0x137;
        const int WM_CTLCOLORSTATIC = 0x138;

        const int WM_MOUSEFIRST = 0x200;
        const int WM_MOUSEMOVE = 0x200;
        const int WM_LBUTTONDOWN = 0x201;
        const int WM_LBUTTONUP = 0x202;
        const int WM_LBUTTONDBLCLK = 0x203;
        const int WM_RBUTTONDOWN = 0x204;
        const int WM_RBUTTONUP = 0x205;
        const int WM_RBUTTONDBLCLK = 0x206;
        const int WM_MBUTTONDOWN = 0x207;
        const int WM_MBUTTONUP = 0x208;
        const int WM_MBUTTONDBLCLK = 0x209;
        const int WM_MOUSEWHEEL = 0x20A;
        const int WM_MOUSEHWHEEL = 0x20E;

        const int WM_PARENTNOTIFY = 0x210;
        const int WM_ENTERMENULOOP = 0x211;
        const int WM_EXITMENULOOP = 0x212;
        const int WM_NEXTMENU = 0x213;
        const int WM_SIZING = 0x214;
        const int WM_CAPTURECHANGED = 0x215;
        const int WM_MOVING = 0x216;
        const int WM_POWERBROADCAST = 0x218;
        const int WM_DEVICECHANGE = 0x219;

        const int WM_MDICREATE = 0x220;
        const int WM_MDIDESTROY = 0x221;
        const int WM_MDIACTIVATE = 0x222;
        const int WM_MDIRESTORE = 0x223;
        const int WM_MDINEXT = 0x224;
        const int WM_MDIMAXIMIZE = 0x225;
        const int WM_MDITILE = 0x226;
        const int WM_MDICASCADE = 0x227;
        const int WM_MDIICONARRANGE = 0x228;
        const int WM_MDIGETACTIVE = 0x229;
        const int WM_MDISETMENU = 0x230;
        const int WM_ENTERSIZEMOVE = 0x231;
        const int WM_EXITSIZEMOVE = 0x232;
        const int WM_DROPFILES = 0x233;
        const int WM_MDIREFRESHMENU = 0x234;

        const int WM_IME_SETCONTEXT = 0x281;
        const int WM_IME_NOTIFY = 0x282;
        const int WM_IME_CONTROL = 0x283;
        const int WM_IME_COMPOSITIONFULL = 0x284;
        const int WM_IME_SELECT = 0x285;
        const int WM_IME_CHAR = 0x286;
        const int WM_IME_KEYDOWN = 0x290;
        const int WM_IME_KEYUP = 0x291;

        const int WM_MOUSEHOVER = 0x2A1;
        const int WM_NCMOUSELEAVE = 0x2A2;
        const int WM_MOUSELEAVE = 0x2A3;

        const int WM_CUT = 0x300;
        const int WM_COPY = 0x301;
        const int WM_PASTE = 0x302;
        const int WM_CLEAR = 0x303;
        const int WM_UNDO = 0x304;

        const int WM_RENDERFORMAT = 0x305;
        const int WM_RENDERALLFORMATS = 0x306;
        const int WM_DESTROYCLIPBOARD = 0x307;
        const int WM_DRAWCLIPBOARD = 0x308;
        const int WM_PAINTCLIPBOARD = 0x309;
        const int WM_VSCROLLCLIPBOARD = 0x30A;
        const int WM_SIZECLIPBOARD = 0x30B;
        const int WM_ASKCBFORMATNAME = 0x30C;
        const int WM_CHANGECBCHAIN = 0x30D;
        const int WM_HSCROLLCLIPBOARD = 0x30E;
        const int WM_QUERYNEWPALETTE = 0x30F;
        const int WM_PALETTEISCHANGING = 0x310;
        const int WM_PALETTECHANGED = 0x311;

        const int WM_HOTKEY = 0x312;
        const int WM_PRINT = 0x317;
        const int WM_PRINTCLIENT = 0x318;

        const int WM_HANDHELDFIRST = 0x358;
        const int WM_HANDHELDLAST = 0x35F;
        const int WM_PENWINFIRST = 0x380;
        const int WM_PENWINLAST = 0x38F;
        const int WM_COALESCE_FIRST = 0x390;
        const int WM_COALESCE_LAST = 0x39F;
        const int WM_DDE_FIRST = 0x3E0;
        const int WM_DDE_INITIATE = 0x3E0;
        const int WM_DDE_TERMINATE = 0x3E1;
        const int WM_DDE_ADVISE = 0x3E2;
        const int WM_DDE_UNADVISE = 0x3E3;
        const int WM_DDE_ACK = 0x3E4;
        const int WM_DDE_DATA = 0x3E5;
        const int WM_DDE_REQUEST = 0x3E6;
        const int WM_DDE_POKE = 0x3E7;
        const int WM_DDE_EXECUTE = 0x3E8;
        const int WM_DDE_LAST = 0x3E8;

        const int WM_USER = 0x400;
        const int WM_APP = 0x8000;

        const int MyMessage = WM_USER + 0x05; // for example

        [DllImport("user32.dll")]
        private static extern bool SendMessage(
                IntPtr hWnd,      // handle to destination window
                UInt32 Msg,       // message
                Int32 wParam,  // first message parameter
                Int32 lParam   // second message parameter
                ); 

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == MyMessage)
            {
                // do whatever with your message
            }
        }
    }

    // share memory, grab memory, another application memory, another programm memory, another process memory, another executable memory
    // another exe memory, hack memory, open memory

    /// <summary>
    ///     Class to read and write memory from another process
    /// </summary>
    public class AnotherProcessMemory
    {
        private const int PROCESS_WM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_VM_OPERATION = 0x0008;

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, int lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, int lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);


        private int _ProcessID = 0;
        private IntPtr _ProcessHandle = IntPtr.Zero;

        public static int GetCurrentProcessId() { return Process.GetCurrentProcess().Id; }

        public AnotherProcessMemory(int ProcessID)
        {
            this._ProcessID = ProcessID;
        }

        public AnotherProcessMemory(string ProcessName)
        {
            this._ProcessID = Process.GetProcessesByName(ProcessName)[0].Id;
        }

        public void Open(System.IO.FileAccess mode)
        {
            if (mode == System.IO.FileAccess.Read)
                _ProcessHandle = OpenProcess(PROCESS_WM_READ, false, _ProcessID);
            else if (mode == System.IO.FileAccess.Write)
                _ProcessHandle = OpenProcess(PROCESS_VM_WRITE | PROCESS_VM_OPERATION, false, _ProcessID);
            else
                _ProcessHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION, false, _ProcessID);
        }

        public void Close()
        {
            if (IsOpen)
            {
                CloseHandle(_ProcessHandle);
                _ProcessHandle = IntPtr.Zero;
            };
        }

        public bool IsOpen
        {
            get
            {
                return _ProcessHandle != IntPtr.Zero;
            }
        }

        public bool Read(int BaseAddress, byte[] buffer, int bufferSize, ref int noOfBytesRead)
        {
            if (!IsOpen)
                throw new Exception("Process is not opened!");
            return ReadProcessMemory((int)_ProcessHandle, BaseAddress, buffer, bufferSize, ref noOfBytesRead);
        }

        public bool Write(int BaseAddress, byte[] buffer, int bufferSize, ref int noOfBytesWritten)
        {
            if (!IsOpen)
                throw new Exception("Process is not opened!");
            return WriteProcessMemory((int)_ProcessHandle, BaseAddress, buffer, bufferSize, ref noOfBytesWritten);
        }

        public bool Read(int BaseAddress, IntPtr buffer, int bufferSize, ref int noOfBytesRead)
        {
            if (!IsOpen)
                throw new Exception("Process is not opened!");
            return ReadProcessMemory((int)_ProcessHandle, BaseAddress, (int)buffer, bufferSize, ref noOfBytesRead);
        }

        public bool Write(int BaseAddress, IntPtr buffer, int bufferSize, ref int noOfBytesWritten)
        {
            if (!IsOpen)
                throw new Exception("Process is not opened!");
            return WriteProcessMemory((int)_ProcessHandle, BaseAddress, (int)buffer, bufferSize, ref noOfBytesWritten);
        }

        public bool Read(IntPtr BaseAddress, byte[] buffer, int bufferSize, ref int noOfBytesRead)
        {
            if (!IsOpen)
                throw new Exception("Process is not opened!");
            return ReadProcessMemory((int)_ProcessHandle, (int)BaseAddress, buffer, bufferSize, ref noOfBytesRead);
        }

        public bool Write(IntPtr BaseAddress, byte[] buffer, int bufferSize, ref int noOfBytesWritten)
        {
            if (!IsOpen)
                throw new Exception("Process is not opened!");
            return WriteProcessMemory((int)_ProcessHandle, (int)BaseAddress, buffer, bufferSize, ref noOfBytesWritten);
        }

        public bool Read(IntPtr BaseAddress, IntPtr buffer, int bufferSize, ref int noOfBytesRead)
        {
            if (!IsOpen)
                throw new Exception("Process is not opened!");
            return ReadProcessMemory((int)_ProcessHandle, (int)BaseAddress, (int)buffer, bufferSize, ref noOfBytesRead);
        }

        public bool Write(IntPtr BaseAddress, IntPtr buffer, int bufferSize, ref int noOfBytesWritten)
        {
            if (!IsOpen)
                throw new Exception("Process is not opened!");
            return WriteProcessMemory((int)_ProcessHandle, (int)BaseAddress, (int)buffer, bufferSize, ref noOfBytesWritten);
        }

        public static void Example()
        {
            Console.WriteLine("AnotherProcessMemory Example\r\n");
            unsafe
            {
                string filename = "lambda-test-510";
                // TEST
                Console.WriteLine("Current Process Id: {0}", AnotherProcessMemory.GetCurrentProcessId());
                IntPtr MyProcPointer = Marshal.AllocHGlobal(1024);
                Console.WriteLine("Pointer to Memory: {0}", (int)MyProcPointer);
                byte[] mba = new byte[] { 11, 102, 203 };
                Marshal.Copy(mba, 0, MyProcPointer, mba.Length);
                int* MyVar = (int*)((int)MyProcPointer + 3);
                *MyVar = 117042;
                Console.WriteLine("Set FileMapping File & Header: " + filename);
                Console.WriteLine(String.Format("In MyProcPointer: {0},{1},{2}", mba[0], mba[1], mba[2]));
                Console.WriteLine(String.Format("In MyVar: {0}", *MyVar));
                Console.WriteLine();

                FileMapping fm = new FileMapping(filename, System.IO.FileAccess.Write);
                byte[] bMessage = Encoding.ASCII.GetBytes(filename + '\0');
                Marshal.Copy(bMessage, 0, fm.Pointer, bMessage.Length);
                Marshal.Copy(new int[] { AnotherProcessMemory.GetCurrentProcessId(), (int)MyProcPointer }, 0, (IntPtr)((int)fm.Pointer + bMessage.Length), 2);

                //client
                {
                    FileMapping fm2 = new FileMapping(filename, System.IO.FileAccess.ReadWrite);
                    string txtin = Marshal.PtrToStringAnsi(fm2.Pointer);
                    if (txtin == filename)
                    {
                        int[] pa = new int[2];
                        Marshal.Copy((IntPtr)((int)fm2.Pointer + filename.Length + 1), pa, 0, 2);
                        AnotherProcessMemory apm = new AnotherProcessMemory(pa[0]);
                        apm.Open(System.IO.FileAccess.ReadWrite);
                        int bw = 0;
                        byte[] btr = new byte[3];
                        apm.Read(pa[1], btr, btr.Length, ref bw);
                        Console.WriteLine(String.Format("Read from AnotherProcessMemory: {0},{1},{2}", btr[0], btr[1], btr[2]));
                        byte[] btw = new byte[] { 7, 77, 111 };
                        apm.Write(pa[1], btw, btw.Length, ref bw);
                        Console.WriteLine(String.Format("Write to AnotherProcessMemory: {0},{1},{2}", btw[0], btw[1], btw[2]));
                        byte[] myInt = new byte[4];
                        apm.Read(pa[1] + 3, myInt, myInt.Length, ref bw);
                        int rfmi = 0; int* p2rfmi = &rfmi; Marshal.Copy(myInt, 0, (IntPtr)p2rfmi, 4); // or int rfmi = BitConverter.ToInt32(myInt, 0);
                        //apm.Read(pa[1] + 3, (IntPtr)p2rfmi, 4, ref bw);
                        Console.WriteLine(String.Format("Read MyVar from AnotherProcessMemory: {0}", rfmi));
                        rfmi = 987456;
                        Marshal.Copy((IntPtr)p2rfmi, myInt, 0, 4); // or myInt = BitConverter.GetBytes(rfmi);                            
                        apm.Write(pa[1] + 3, myInt, myInt.Length, ref bw);
                        //apm.Write(pa[1] + 3, (IntPtr)p2rfmi, 4, ref bw);
                        Console.WriteLine(String.Format("Write MyVar from AnotherProcessMemory: {0}", rfmi));
                        apm.Close();
                    };
                    byte[] writeback = Encoding.ASCII.GetBytes("SKIP DONE" + '\0');
                    Marshal.Copy(writeback, 0, fm2.Pointer, writeback.Length);
                    fm2.Close();
                    Console.WriteLine("Set FileMapping Header: SKIP DONE");
                    Console.WriteLine();
                }

                string readback = Marshal.PtrToStringAnsi((IntPtr)((int)fm.Pointer + 5));
                Console.WriteLine("FileMapping Header skip 5: " + readback);
                Marshal.Copy(MyProcPointer, mba, 0, mba.Length);
                Console.WriteLine(String.Format("In MyProcPointer: {0},{1},{2}", mba[0], mba[1], mba[2]));
                Console.WriteLine(String.Format("In MyVar: {0}", *MyVar));
                fm.Close();

                Marshal.FreeHGlobal(MyProcPointer);

                Console.WriteLine("\r\nDONE");
            };
        }
    }

    public class SharedMemoryFile : FileMapping
    {
        public SharedMemoryFile(string inMemoryFileName) : base(inMemoryFileName) { }
    }


    /// <summary>
    ///     Class to created shared file in memory
    ///     file can be opened by more then one process
    /// </summary>
    public class FileMapping
    {
        private System.IO.UnmanagedMemoryStream _ums = null;
        private SafeFileMappingHandle _hMapFile = null;
        private IntPtr _filePointer = IntPtr.Zero;
        private string _fileName = "";
        private uint _fileSize = 65536;
        private bool _connected = false;
        private System.IO.FileAccess _access = System.IO.FileAccess.ReadWrite;

        public FileMapping(string inMemoryFileName)
        {
            this._fileName = inMemoryFileName;
            Open(_access);
        }

        public FileMapping(string inMemoryFileName, System.IO.FileAccess access)
        {
            this._fileName = inMemoryFileName;
            Open(this._access = access);
        }

        public FileMapping(string inMemoryFileName, System.IO.FileAccess access, uint fileSize)
        {
            this._fileName = inMemoryFileName;
            this._fileSize = fileSize;
            Open(this._access = access);
        }

        private void Open(System.IO.FileAccess access)
        {
            SECURITY_ATTRIBUTES sa = SECURITY_ATTRIBUTES.Empty;
            if (access == System.IO.FileAccess.Read)
            {
                _hMapFile = NativeMethod.CreateFileMapping(INVALID_HANDLE_VALUE, ref sa, FileProtection.PAGE_READONLY, 0, _fileSize, _fileName);
                _filePointer = NativeMethod.MapViewOfFile(_hMapFile, FileMapAccess.FILE_MAP_READ, 0, 0, _fileSize);
            }
            else if (access == System.IO.FileAccess.Write)
            {
                _hMapFile = NativeMethod.CreateFileMapping(INVALID_HANDLE_VALUE, ref sa, FileProtection.PAGE_READWRITE, 0, _fileSize, _fileName);
                _filePointer = NativeMethod.MapViewOfFile(_hMapFile, FileMapAccess.FILE_MAP_WRITE, 0, 0, _fileSize);
            }
            else
            {
                _hMapFile = NativeMethod.CreateFileMapping(INVALID_HANDLE_VALUE, ref sa, FileProtection.PAGE_READWRITE, 0, _fileSize, _fileName);
                _filePointer = NativeMethod.MapViewOfFile(_hMapFile, FileMapAccess.FILE_MAP_ALL_ACCESS, 0, 0, _fileSize);
            };

            if (_filePointer == IntPtr.Zero) throw new Exception("No Access");
            unsafe
            {
                _ums = new System.IO.UnmanagedMemoryStream((byte*)_filePointer, (long)_fileSize, (long)_fileSize, _access);
            };
            _connected = true;
        }

        public System.IO.FileAccess Access { get { return _access; } }

        public bool IsOpen { get { return _connected; } }

        public uint Length { get { return _fileSize; } }

        public void Close()
        {
            if (_hMapFile != null)
            {
                if (_filePointer != IntPtr.Zero)
                {
                    NativeMethod.UnmapViewOfFile(_filePointer);
                    _filePointer = IntPtr.Zero;
                };
                _hMapFile.Close();
                _hMapFile = null;
            };
            _connected = false;
        }

        public IntPtr Pointer { get { return _filePointer; } }

        public System.IO.Stream GetStream()
        {
            return _ums;
        }

        public void Copy(IntPtr dest, IntPtr src, uint count)
        {
            CopyMemory(dest, src, count);
        }

        public static void CopyMem(IntPtr dest, IntPtr src, uint count)
        {
            CopyMemory(dest, src, count);
        }

        public static void Example()
        {
            Console.WriteLine("FileMapping Example\r\n");

            Console.WriteLine("Creating file `lambda-510` and write `TEST 001` from object A");
            FileMapping fm = new FileMapping("lambda-510", System.IO.FileAccess.Write);
            byte[] bMessage = Encoding.Unicode.GetBytes("TEST 001" + '\0');
            Marshal.Copy(bMessage, 0, fm.Pointer, bMessage.Length);
            byte[] wstxt = Encoding.GetEncoding(1251).GetBytes("TEST WRITE TO FILE BY STREAM\r\n\0\0");
            fm.GetStream().Position = bMessage.Length;
            fm.GetStream().Write(wstxt, 0, wstxt.Length);
            Console.WriteLine("Write to file by Stream from object A - ok");
            Console.WriteLine();

            {
                FileMapping fm2 = new FileMapping("lambda-510", System.IO.FileAccess.ReadWrite);
                string txtB = Marshal.PtrToStringUni(fm2.Pointer);
                Console.WriteLine("Creating file `lambda-510` and read `" + txtB + "` from object B");
                fm2.GetStream().Position = bMessage.Length;
                byte[] rtasstr = new byte[1024];
                fm2.GetStream().Read(rtasstr, 0, rtasstr.Length);
                string ras = System.Text.Encoding.GetEncoding(1251).GetString(rtasstr).Replace("\0", "").Trim();
                Console.WriteLine("Read from file by Stream from object B - " + ras);
                byte[] w2f = Encoding.ASCII.GetBytes(" TEST 002" + '\0');
                Marshal.Copy(w2f, 0, fm2.Pointer, w2f.Length);
                Console.WriteLine("Write `TEST 002` to file from object B");
                fm2.Close();
                Console.WriteLine();
            };

            string txtA = Marshal.PtrToStringAnsi((IntPtr)((int)fm.Pointer + 1));
            Console.WriteLine("Read `" + txtA + "` from object A");
            fm.Close();
            Console.WriteLine("\r\nDone");
        }

        #region Native API Signatures and Types

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern uint SetNamedSecurityInfoW(String pObjectName, SE_OBJECT_TYPE ObjectType, SECURITY_INFORMATION SecurityInfo, IntPtr psidOwner, IntPtr psidGroup, IntPtr pDacl, IntPtr pSacl);

        [DllImport("Advapi32.dll", SetLastError = true)]
        private static extern bool ConvertStringSidToSid(String StringSid, ref IntPtr Sid);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        private enum SE_OBJECT_TYPE
        {
            SE_UNKNOWN_OBJECT_TYPE = 0,
            SE_FILE_OBJECT,
            SE_SERVICE,
            SE_PRINTER,
            SE_REGISTRY_KEY,
            SE_LMSHARE,
            SE_KERNEL_OBJECT,
            SE_WINDOW_OBJECT,
            SE_DS_OBJECT,
            SE_DS_OBJECT_ALL,
            SE_PROVIDER_DEFINED_OBJECT,
            SE_WMIGUID_OBJECT,
            SE_REGISTRY_WOW64_32KEY
        }

        [Flags]
        private enum SECURITY_INFORMATION : uint
        {
            Owner = 0x00000001,
            Group = 0x00000002,
            Dacl = 0x00000004,
            Sacl = 0x00000008,
            ProtectedDacl = 0x80000000,
            ProtectedSacl = 0x40000000,
            UnprotectedDacl = 0x20000000,
            UnprotectedSacl = 0x10000000
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;

            public static SECURITY_ATTRIBUTES Empty
            {
                get
                {
                    SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
                    sa.nLength = sizeof(int) * 2 + IntPtr.Size;
                    sa.lpSecurityDescriptor = IntPtr.Zero;
                    sa.bInheritHandle = 0;
                    return sa;
                }
            }
        }

        /// <summary>
        /// Memory Protection Constants
        /// http://msdn.microsoft.com/en-us/library/aa366786.aspx
        /// </summary>
        [Flags]
        public enum FileProtection : uint
        {
            NONE = 0x00,
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400,
            SEC_FILE = 0x800000,
            SEC_IMAGE = 0x1000000,
            SEC_RESERVE = 0x4000000,
            SEC_COMMIT = 0x8000000,
            SEC_NOCACHE = 0x10000000
        }


        /// <summary>
        /// Access rights for file mapping objects
        /// http://msdn.microsoft.com/en-us/library/aa366559.aspx
        /// </summary>
        [Flags]
        public enum FileMapAccess
        {
            FILE_MAP_COPY = 0x0001,
            FILE_MAP_WRITE = 0x0002,
            FILE_MAP_READ = 0x0004,
            FILE_MAP_ALL_ACCESS = 0x000F001F
        }


        /// <summary>
        /// Represents a wrapper class for a file mapping handle. 
        /// </summary>
        [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
        internal sealed class SafeFileMappingHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            private SafeFileMappingHandle()
                : base(true)
            {
            }

            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            public SafeFileMappingHandle(IntPtr handle, bool ownsHandle)
                : base(ownsHandle)
            {
                base.SetHandle(handle);
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success),
            DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr handle);

            protected override bool ReleaseHandle()
            {
                return CloseHandle(base.handle);
            }
        }


        internal static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);


        /// <summary>
        /// The class exposes Windows APIs used in this code sample.
        /// </summary>
        [SuppressUnmanagedCodeSecurity]
        internal class NativeMethod
        {
            /// <summary>
            /// Creates or opens a named or unnamed file mapping object for a 
            /// specified file.
            /// </summary>
            /// <param name="hFile">
            /// A handle to the file from which to create a file mapping object.
            /// </param>
            /// <param name="lpAttributes">
            /// A pointer to a SECURITY_ATTRIBUTES structure that determines 
            /// whether a returned handle can be inherited by child processes.
            /// </param>
            /// <param name="flProtect">
            /// Specifies the page protection of the file mapping object. All 
            /// mapped views of the object must be compatible with this 
            /// protection.
            /// </param>
            /// <param name="dwMaximumSizeHigh">
            /// The high-order DWORD of the maximum size of the file mapping 
            /// object.
            /// </param>
            /// <param name="dwMaximumSizeLow">
            /// The low-order DWORD of the maximum size of the file mapping 
            /// object.
            /// </param>
            /// <param name="lpName">
            /// The name of the file mapping object.
            /// </param>
            /// <returns>
            /// If the function succeeds, the return value is a handle to the 
            /// newly created file mapping object.
            /// </returns>
            [DllImport("Kernel32.dll", SetLastError = true)]
            public static extern SafeFileMappingHandle CreateFileMapping(
                IntPtr hFile,
                ref SECURITY_ATTRIBUTES lpAttributes,
                FileProtection flProtect,
                uint dwMaximumSizeHigh,
                uint dwMaximumSizeLow,
                string lpName);


            /// <summary>
            /// Maps a view of a file mapping into the address space of a calling
            /// process.
            /// </summary>
            /// <param name="hFileMappingObject">
            /// A handle to a file mapping object. The CreateFileMapping and 
            /// OpenFileMapping functions return this handle.
            /// </param>
            /// <param name="dwDesiredAccess">
            /// The type of access to a file mapping object, which determines the 
            /// protection of the pages.
            /// </param>
            /// <param name="dwFileOffsetHigh">
            /// A high-order DWORD of the file offset where the view begins.
            /// </param>
            /// <param name="dwFileOffsetLow">
            /// A low-order DWORD of the file offset where the view is to begin.
            /// </param>
            /// <param name="dwNumberOfBytesToMap">
            /// The number of bytes of a file mapping to map to the view. All bytes 
            /// must be within the maximum size specified by CreateFileMapping.
            /// </param>
            /// <returns>
            /// If the function succeeds, the return value is the starting address 
            /// of the mapped view.
            /// </returns>
            [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr MapViewOfFile(
                SafeFileMappingHandle hFileMappingObject,
                FileMapAccess dwDesiredAccess,
                uint dwFileOffsetHigh,
                uint dwFileOffsetLow,
                uint dwNumberOfBytesToMap);


            /// <summary>
            /// Unmaps a mapped view of a file from the calling process's address 
            /// space.
            /// </summary>
            /// <param name="lpBaseAddress">
            /// A pointer to the base address of the mapped view of a file that 
            /// is to be unmapped.
            /// </param>
            /// <returns></returns>
            [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);
        }

        #endregion
    }     
}
