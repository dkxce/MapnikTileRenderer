using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Runtime.InteropServices;

namespace share_folder_creator
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                NetShareFolder("c:\\Mapnik", "Mapnik", "Rendering Project");
                Console.WriteLine("OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            };
            Console.ReadLine();
        }

        [DllImport("Netapi32.dll")]
        private static extern uint NetShareAdd(
            [MarshalAs(UnmanagedType.LPWStr)] string strServer,
            Int32 dwLevel,
            ref SHARE_INFO_502 buf,
            out uint parm_err
        );

        private enum NetError : uint
        {
            NERR_Success = 0,
            NERR_BASE = 2100,
            NERR_UnknownDevDir = (NERR_BASE + 16),
            NERR_DuplicateShare = (NERR_BASE + 18),
            NERR_BufTooSmall = (NERR_BASE + 23),
        }

        private enum SHARE_TYPE : uint
        {
            STYPE_DISKTREE = 0,
            STYPE_PRINTQ = 1,
            STYPE_DEVICE = 2,
            STYPE_IPC = 3,
            STYPE_TEMPORARY = 0x40000000,
            STYPE_SPECIAL = 0x80000000,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SHARE_INFO_502
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string shi502_netname;
            public SHARE_TYPE shi502_type;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string shi502_remark;
            public Int32 shi502_permissions;
            public Int32 shi502_max_uses;
            public Int32 shi502_current_uses;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string shi502_path;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string shi502_passwd;
            public Int32 shi502_reserved;
            public IntPtr shi502_security_descriptor;
        }

        private static void NetShareFolder(string path, string shareName, string shareDesc)
        {
            // string path = @"C:\MyShareDirectory"; // do not append comma, it'll fail

            SHARE_INFO_502 info = new SHARE_INFO_502();
            info.shi502_netname = shareName;
            info.shi502_type = SHARE_TYPE.STYPE_DISKTREE;
            info.shi502_remark = shareDesc;
            info.shi502_permissions = 0;    // ignored for user-level security
            info.shi502_max_uses = -1;
            info.shi502_current_uses = 0;    // ignored for set
            info.shi502_path = path;
            info.shi502_passwd = null;        // ignored for user-level security
            info.shi502_reserved = 0;
            info.shi502_security_descriptor = IntPtr.Zero;

            uint error = 0;
            uint result = NetShareAdd(null, 502, ref info, out error);
            Console.WriteLine(result);
            Console.WriteLine(error);
        }


        private static void QshareFolder(string FolderPath, string ShareName, string Description)
        {
            // Create a ManagementClass object
            ManagementClass managementClass = new ManagementClass("Win32_Share");
            // Create ManagementBaseObjects for in and out parameters
            ManagementBaseObject inParams = managementClass.GetMethodParameters("Create");
            ManagementBaseObject outParams;
            // Set the input parameters
            inParams["Description"] = Description;
            inParams["Name"] = ShareName;
            inParams["Path"] = FolderPath;
            inParams["Type"] = 0x0; // Disk Drive
            //Another Type:
            // DISK_DRIVE = 0x0
            // PRINT_QUEUE = 0x1
            // DEVICE = 0x2
            // IPC = 0x3
            // DISK_DRIVE_ADMIN = 0x80000000
            // PRINT_QUEUE_ADMIN = 0x80000001
            // DEVICE_ADMIN = 0x80000002
            // IPC_ADMIN = 0x8000003

            inParams["MaximumAllowed"] = null;
            inParams["Password"] = null;
            inParams["Access"] = null;
            
            // Invoke the method on the ManagementClass object
            outParams = managementClass.InvokeMethod("Create", inParams, null);

            // Check to see if the method invocation was successful
            if ((uint)(outParams.Properties["ReturnValue"].Value) != 0)
            {
                throw new Exception("Unable to share directory.");
            };
        }
    }
}
