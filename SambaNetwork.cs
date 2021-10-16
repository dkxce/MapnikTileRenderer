using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

using BOOL = System.Boolean;
using DWORD = System.UInt32;
using LPWSTR = System.String;
using NET_API_STATUS = System.UInt32;

namespace SambaNetwork
{
    public class RemoteConnection : IDisposable
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct USE_INFO_2
        {
            internal LPWSTR ui2_local;
            internal LPWSTR ui2_remote;
            internal LPWSTR ui2_password;
            internal DWORD ui2_status;
            internal DWORD ui2_asg_type;
            internal DWORD ui2_refcount;
            internal DWORD ui2_usecount;
            internal LPWSTR ui2_username;
            internal LPWSTR ui2_domainname;
        }

        [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern NET_API_STATUS NetUseAdd(LPWSTR UncServerName, DWORD Level, ref USE_INFO_2 Buf, out DWORD ParmError);

        [DllImport("NetApi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern NET_API_STATUS NetUseDel(LPWSTR UncServerName, LPWSTR UseName, DWORD ForceCond);

        private enum ResourceScope
        {
            RESOURCE_CONNECTED = 1,
            RESOURCE_GLOBALNET,
            RESOURCE_REMEMBERED,
            RESOURCE_RECENT,
            RESOURCE_CONTEXT
        }
        private enum ResourceType
        {
            RESOURCETYPE_ANY,
            RESOURCETYPE_DISK,
            RESOURCETYPE_PRINT,
            RESOURCETYPE_RESERVED
        }
        private enum ResourceUsage
        {
            RESOURCEUSAGE_CONNECTABLE = 0x00000001,
            RESOURCEUSAGE_CONTAINER = 0x00000002,
            RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
            RESOURCEUSAGE_SIBLING = 0x00000008,
            RESOURCEUSAGE_ATTACHED = 0x00000010
        }
        private enum ResourceDisplayType
        {
            RESOURCEDISPLAYTYPE_GENERIC,
            RESOURCEDISPLAYTYPE_DOMAIN,
            RESOURCEDISPLAYTYPE_SERVER,
            RESOURCEDISPLAYTYPE_SHARE,
            RESOURCEDISPLAYTYPE_FILE,
            RESOURCEDISPLAYTYPE_GROUP,
            RESOURCEDISPLAYTYPE_NETWORK,
            RESOURCEDISPLAYTYPE_ROOT,
            RESOURCEDISPLAYTYPE_SHAREADMIN,
            RESOURCEDISPLAYTYPE_DIRECTORY,
            RESOURCEDISPLAYTYPE_TREE,
            RESOURCEDISPLAYTYPE_NDSCONTAINER
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NETRESOURCE
        {
            public ResourceScope oResourceScope;
            public ResourceType oResourceType;
            public ResourceDisplayType oDisplayType;
            public ResourceUsage oResourceUsage;
            public string sLocalName;
            public string sRemoteName;
            public string sComments;
            public string sProvider;
        }

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(ref NETRESOURCE oNetworkResource, string sPassword, string sUserName, int iFlags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2(string sLocalName, uint iFlags, int iForce);

        private bool disposed = false;

        private string sUNCPath;
        private string sUser;
        private string sPassword;
        private string sDomain;
        private int iLastError;

        public RemoteConnection(){}

        /// <summary>
        /// The last system error code returned from NetUseAdd or NetUseDel.  Success = 0
        /// </summary>
        public int LastError
        {
            get { return iLastError; }
        }

        public void Dispose()
        {
            if (!this.disposed) { Disconnect(); }
            disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Connects to a UNC path using the credentials supplied.
        /// </summary>
        /// <param name="UNCPath">Fully qualified domain name UNC path</param>
        /// <param name="User">A user with sufficient rights to access the path.</param>
        /// <param name="Domain">Domain of User.</param>
        /// <param name="Password">Password of User</param>
        /// <returns>True if mapping succeeds.  Use LastError to get the system error code.</returns>
        public bool Connect(string UNCPath, string User, string Domain, string Password)
        {
            //Checks if the last character is \ as this causes error on mapping a drive.
            if (UNCPath.Substring(UNCPath.Length - 1, 1) == @"\")
                 UNCPath = UNCPath.Substring(0, UNCPath.Length - 1);
            sUNCPath = UNCPath;
            sUser = User;
            sPassword = Password;
            sDomain = Domain;
            return NetUseWithCredentials();
        }

        private bool NetUseWithCredentials()
        {
            uint returncode;
            try
            {
                USE_INFO_2 useinfo = new USE_INFO_2();

                useinfo.ui2_remote = sUNCPath;
                useinfo.ui2_username = sUser;
                useinfo.ui2_domainname = sDomain;
                useinfo.ui2_password = sPassword;
                useinfo.ui2_asg_type = 0;
                useinfo.ui2_usecount = 1;
                uint paramErrorIndex;
                returncode = NetUseAdd(null, 2, ref useinfo, out paramErrorIndex);
                iLastError = (int)returncode;
                return returncode == 0;
            }
            catch
            {
                iLastError = Marshal.GetLastWin32Error();
                return false;
            }
        }

        /// <summary>
        /// Ends the connection to the remote resource 
        /// </summary>
        /// <returns>True if it succeeds.  Use LastError to get the system error code</returns>
        public bool Disconnect()
        {
            uint returncode;
            try
            {
                returncode = NetUseDel(null, sUNCPath, 2);
                iLastError = (int)returncode;
                return (returncode == 0);
            }
            catch
            {
                iLastError = Marshal.GetLastWin32Error();
                return false;
            }
        }

        ~RemoteConnection()
        {
            Dispose();
        }

        public static int MapDrive(string sDriveLetter, string sNetworkPath, string user, string password)
        {
            //Checks if the last character is \ as this causes error on mapping a drive.
            if (sNetworkPath.Substring(sNetworkPath.Length - 1, 1) == @"\")
                sNetworkPath = sNetworkPath.Substring(0, sNetworkPath.Length - 1);

            NETRESOURCE oNetworkResource = new NETRESOURCE();
            oNetworkResource.oResourceType = ResourceType.RESOURCETYPE_DISK;
            oNetworkResource.sLocalName = sDriveLetter + ":";
            oNetworkResource.sRemoteName = sNetworkPath;

            if (IsDriveMapped(sDriveLetter))
                UnmapDrive(sDriveLetter, true);

            int result = WNetAddConnection2(ref oNetworkResource, user, password, 0);
            return result;
        }

        public static int UnmapDrive(string sDriveLetter, bool bForceDisconnect)
        {
            if (bForceDisconnect)
                return WNetCancelConnection2(sDriveLetter + ":", 0, 1);
            else
                return WNetCancelConnection2(sDriveLetter + ":", 0, 0);
        }

        public static bool IsDriveMapped(string sDriveLetter)
        {
            string[] DriveList = Environment.GetLogicalDrives();
            for (int i = 0; i < DriveList.Length; i++)
                if (sDriveLetter + ":\\" == DriveList[i].ToString())
                    return true;
            return false;
        }
    }

    /* EXAMPLE
            RemoteConnection unc = new RemoteConnection();
            RemoteConnection.MapDrive("Q", @"\\192.168.0.1\TEST\", @"domain\user","pass");

            if (unc.Connect(@"\\192.168.0.1\TEST", @"user", "domain", "pass"))
            {
                string[] dirs = Directory.GetDirectories(@"\\192.168.0.1\TEST");
                foreach (string d in dirs)
                {
                    MessageBox.Show(d);
                };
            }
            else MessageBox.Show("Failed to connect");
            SambaNetwork.RemoteConnection.UnmapDrive  ("Q", true);
    
     */
 
     public enum LogonType
     {
      LOGON32_LOGON_INTERACTIVE = 2,
      LOGON32_LOGON_NETWORK = 3,
      LOGON32_LOGON_BATCH = 4,
      LOGON32_LOGON_SERVICE = 5,
      LOGON32_LOGON_UNLOCK = 7,
      LOGON32_LOGON_NETWORK_CLEARTEXT = 8, // Win2K or higher
      LOGON32_LOGON_NEW_CREDENTIALS = 9 // Win2K or higher
     };
     
     public enum LogonProvider
     {
      LOGON32_PROVIDER_DEFAULT = 0,
      LOGON32_PROVIDER_WINNT35 = 1,
      LOGON32_PROVIDER_WINNT40 = 2,
      LOGON32_PROVIDER_WINNT50 = 3
     };
     
     public enum ImpersonationLevel
     {
      SecurityAnonymous = 0,
      SecurityIdentification = 1,
      SecurityImpersonation = 2,
      SecurityDelegation = 3
     }
     
     class Win32NativeMethods
     {
      [DllImport("advapi32.dll", SetLastError = true)]
      public static extern int LogonUser( string lpszUserName,
           string lpszDomain,
           string lpszPassword,
           int dwLogonType,
           int dwLogonProvider,
           ref IntPtr phToken);
     
      [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern int DuplicateToken( IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);
     
      [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern bool RevertToSelf();
     
      [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
      public static extern bool CloseHandle(IntPtr handle);
     }
     
     /// <summary>
     ///    Allows code to be executed under the security context of a specified user account.
     /// </summary>
     /// <remarks> 
     ///
     /// Implements IDispose, so can be used via a using-directive or method calls;
     ///  ...
     ///
     ///  var imp = new Impersonator( "myUsername", "myDomainname", "myPassword" );
     ///  imp.UndoImpersonation();
     ///
     ///  ...
     ///
     ///  var imp = new Impersonator();
     ///  imp.Impersonate("myUsername", "myDomainname", "myPassword");
     ///  imp.UndoImpersonation();
     ///
     ///  ...
     ///
     ///  using ( new Impersonator( "myUsername", "myDomainname", "myPassword" ) )
     ///  {
     ///   ...
     ///   1
     ///   ...
     ///  }
     ///
     ///  ...
     /// </remarks>
     public class Impersonator : IDisposable
     {
          private WindowsImpersonationContext _wic;
         
          /// <summary>
          /// Begins impersonation with the given credentials, Logon type and Logon provider.
          /// </summary>
          /// <param name="userName">Name of the user.</param>
          /// <param name="domainName">Name of the domain.</param>
          /// <param name="password">The password. <see cref="System.String"/></param>
          /// <param name="logonType">Type of the logon.</param>
          /// <param name="logonProvider">The logon provider. <see cref="Mit.Sharepoint.WebParts.EventLogQuery.Network.LogonProvider"/></param>
          public Impersonator(string userName, string domainName, string password, LogonType logonType, LogonProvider logonProvider)
          {
            Impersonate(userName, domainName, password, logonType, logonProvider);
          }
         
          /// <summary>
          /// Begins impersonation with the given credentials.
          /// </summary>
          /// <param name="userName">Name of the user.</param>
          /// <param name="domainName">Name of the domain.</param>
          /// <param name="password">The password. <see cref="System.String"/></param>
          public Impersonator(string userName, string domainName, string password)
          {
            Impersonate(userName, domainName, password, LogonType.LOGON32_LOGON_INTERACTIVE, LogonProvider.LOGON32_PROVIDER_DEFAULT);
          }
         
          /// <summary>
          /// Initializes a new instance of the <see cref="Impersonator"/> class.
          /// </summary>
          public Impersonator() {}
         
          /// <summary>
          /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
          /// </summary>
          public void Dispose() { UndoImpersonation(); }
         
          /// <summary>
          /// Impersonates the specified user account.
          /// </summary>
          /// <param name="userName">Name of the user.</param>
          /// <param name="domainName">Name of the domain.</param>
          /// <param name="password">The password. <see cref="System.String"/></param>
          public void Impersonate(string userName, string domainName, string password)
          {
            Impersonate(userName, domainName, password, LogonType.LOGON32_LOGON_INTERACTIVE, LogonProvider.LOGON32_PROVIDER_DEFAULT);
          }
         
          /// <summary>
          /// Impersonates the specified user account.
          /// </summary>
          /// <param name="userName">Name of the user.</param>
          /// <param name="domainName">Name of the domain.</param>
          /// <param name="password">The password. <see cref="System.String"/></param>
          /// <param name="logonType">Type of the logon.</param>
          /// <param name="logonProvider">The logon provider. <see cref="Mit.Sharepoint.WebParts.EventLogQuery.Network.LogonProvider"/></param>
          public void Impersonate(string userName, string domainName, string password, LogonType logonType, LogonProvider logonProvider)
          {
               UndoImpersonation();
             
               IntPtr logonToken = IntPtr.Zero;
               IntPtr logonTokenDuplicate = IntPtr.Zero;
               try
               {
                // revert to the application pool identity, saving the identity of the current requestor
                _wic = WindowsIdentity.Impersonate(IntPtr.Zero);
             
                // do logon & impersonate
                if (Win32NativeMethods.LogonUser(userName,
                    domainName,
                    password,
                    (int)logonType,
                    (int)logonProvider,
                    ref logonToken) != 0)
                {
                 if (Win32NativeMethods.DuplicateToken(logonToken, (int)ImpersonationLevel.SecurityImpersonation, ref logonTokenDuplicate) != 0)
                 {
                     WindowsIdentity wi = new WindowsIdentity(logonTokenDuplicate);
                     wi.Impersonate(); // discard the returned identity context (which is the context of the application pool)
                 }
                 else
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }
                else
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
               }
               finally
               {
                   if (logonToken != IntPtr.Zero)
                       Win32NativeMethods.CloseHandle(logonToken);
             
                   if (logonTokenDuplicate != IntPtr.Zero)
                       Win32NativeMethods.CloseHandle(logonTokenDuplicate);
               }
          }
         
          /// <summary>
          /// Stops impersonation.
          /// </summary>
          public void UndoImpersonation()
          {
           // restore saved requestor identity
           if (_wic != null)
            _wic.Undo();
           _wic = null;
          }
     }
}