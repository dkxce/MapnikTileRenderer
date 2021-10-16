using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace tilemerger
{
    class Program
    {
        const string version = "TileZipperArchivator v0.2";

        #region FileStructs
        [StructLayout(LayoutKind.Sequential,Size=1024)]
        public struct CustomHeader
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string TilesRelativePath;

            public CustomHeader(string TilesRelativePath)
            {
                this.TilesRelativePath = TilesRelativePath;
            }
        }
        [StructLayout(LayoutKind.Sequential,Size=1292)]
        public struct CustomFile
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
            public string RelativePath;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Name;
            public uint Size;
            public uint File;
            public uint Position;

            public CustomFile(string RelativePath, string Name, uint Size)
            {
                this.RelativePath =  RelativePath;
                this.Name = Name;
                this.Size = Size;
                this.File = 0;
                this.Position = 0;
            }
        }
        [StructLayout(LayoutKind.Sequential,Size = 22)]
        public struct Tile
        {
            public int X;
            public int Y;
            public ushort Z;
            public uint Size;
            public uint File;
            public uint Position;

            public Tile(int X, int Y, ushort Z, uint Size)
            {
                this.X = X;
                this.Y = Y;
                this.Z = Z;
                this.Size = Size;
                this.File = 0;
                this.Position = 0;
            }

            public static Tile ArrayToTile(byte[] bytes)
            {

                Tile res = new Tile();
                res.X = BitConverter.ToInt32(bytes, 0);
                res.Y = BitConverter.ToInt32(bytes, 4);
                res.Z = BitConverter.ToUInt16(bytes, 8);
                res.File = BitConverter.ToUInt32(bytes, 10);
                res.Position = BitConverter.ToUInt32(bytes, 14);
                res.Size = BitConverter.ToUInt32(bytes, 18);
                return res;
            }

            public static byte[] TileToArray(Tile t)
            {
                List<byte> ba = new List<byte>();
                ba.AddRange(BitConverter.GetBytes(t.X));
                ba.AddRange(BitConverter.GetBytes(t.Y));
                ba.AddRange(BitConverter.GetBytes(t.Z));
                ba.AddRange(BitConverter.GetBytes(t.File));
                ba.AddRange(BitConverter.GetBytes(t.Position));
                ba.AddRange(BitConverter.GetBytes(t.Size));
                return ba.ToArray();
            }
        }

        private static T ArrayToStruct<T>(byte[] bytes)
        {
            IntPtr p = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, p, bytes.Length);
            T res = (T)Marshal.PtrToStructure(p, typeof(T));
            Marshal.FreeHGlobal(p);
            return res;
        }

        private static byte[] StructToArray<T>(T str)
        {
            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(str));
            Marshal.StructureToPtr(str, p, true);
            byte[] res = new byte[Marshal.SizeOf(str)];
            Marshal.Copy(p, res, 0, res.Length);
            Marshal.FreeHGlobal(p);
            return res;
        }        
        #endregion

        #region Program vars
        private static ulong totaldirs = 0;
        private static ulong totalfiles = 0;
        private static ulong totalsize = 0;
        private static DateTime started = DateTime.Now;

        private static uint maxfilesize = 512 * 1024 * 1024; // 512MB Default
        private static uint mfFilesNo = 0;
        private static uint mfIndexNo = 0;
        private static FileStream mfMain = null;
        private static FileStream mfFiles = null;
        private static FileStream mfIndex = null;
        
        private static CustomHeader cstHeader = new CustomHeader("");
        private static List<CustomFile> cstFiles = new List<CustomFile>();

        private static byte skipfiles = 0;

        private static byte[] rwBuffer = new byte[32768];
        private static FileStream rwStream = null;
        #endregion

        #region KERNEL32
        public const int MAX_PATH = 260;
        public const int MAX_ALTERNATE = 14;

        [StructLayout(LayoutKind.Sequential)]
        public struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WIN32_FIND_DATA
        {
            public FileAttributes dwFileAttributes;
            public FILETIME ftCreationTime;
            public FILETIME ftLastAccessTime;
            public FILETIME ftLastWriteTime;
            public uint nFileSizeHigh; //changed all to uint, otherwise you run into unexpected overflow
            public uint nFileSizeLow;  //|
            public uint dwReserved0;   //|
            public uint dwReserved1;   //v
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ALTERNATE)]
            public string cAlternate;
        }        

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FindClose(IntPtr hFindFile);
        #endregion

        private static void UpdateMergeStatus(string directory, string relativepath, WIN32_FIND_DATA findData)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("{0}\t\r\n",version);
            Console.WriteLine("Merging tiles...\t");
            Console.WriteLine("Current path:\t");
            Console.WriteLine(" " + directory);
            Console.WriteLine("Current file:\t");
            Console.WriteLine(" " + relativepath + findData.cFileName);
            Console.WriteLine("Merged {0} files in {1} dirs", totalfiles, totaldirs);
            if (totalsize < 1024)
                Console.WriteLine("Size: {0} B\t\t", totalsize);
            else if (totalsize < 1024 * 1024)
                Console.WriteLine("Size: {0:0.00} KB\t\t", (double)totalsize / 1024);
            else
                Console.WriteLine("Size: {0:0.00} MB\t\t", (double)totalsize / 1024 / 1024);
            TimeSpan elapsed = DateTime.Now.Subtract(started);
            Console.WriteLine("Elapsed: " + String.Format("{0:00}:{1:00}:{2:00}:{3:00}", new object[] { elapsed.Days, elapsed.Hours, elapsed.Minutes, elapsed.Seconds }));

            mfFiles.Flush();
            mfIndex.Flush();
        }
        
        private static void RecurseSearch(string directory, string relativepath, int level)
        {
            if (directory.EndsWith(@"\")) directory = directory.Remove(directory.Length - 1);

            IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
            WIN32_FIND_DATA findData;
            
            // LOCAL @"\\?\" + directory + @"\*"  NETWORK @"" + directory + @"\*"
            IntPtr findHandle = FindFirstFile(@"" + directory + @"\*", out findData);
            if (findHandle != INVALID_HANDLE_VALUE)
            {
                do
                {
                    if ((findData.dwFileAttributes & FileAttributes.Directory) == 0) //FILE
                    {                        
                        long size = (long)findData.nFileSizeLow + (long)findData.nFileSizeHigh * 4294967296;
                        
                        ProcessFile(directory + @"\" + findData.cFileName, findData.cFileName, relativepath, size);

                        totalfiles++;
                        totalsize += (ulong)size;
                        
                        skipfiles++;
                        if (skipfiles % 16 == 0)
                            UpdateMergeStatus(directory, relativepath, findData);
                    }
                    else //DIR
                    {                        
                        if (findData.cFileName != "." && findData.cFileName != "..")
                            if (level != 0)  // allows 0 to complete search.
                            {
                                totaldirs++;
                                RecurseSearch(directory + @"\" + findData.cFileName, relativepath + findData.cFileName + @"\", level - 1);
                            };
                    };
                }
                while (FindNextFile(findHandle, out findData));
                FindClose(findHandle);
                UpdateMergeStatus(directory, relativepath, findData);
            };
        }

        private static void ProcessFile(string path, string name, string recursivepath, long size)
        {
            bool customfile = true;
            if ((Path.GetExtension(name).ToLower() == ".png") &&(name.IndexOf("C") == 0))
            {
                try
                {
                    string xx = Path.GetFileNameWithoutExtension(name).Substring(1);
                    int x = int.Parse(xx, System.Globalization.NumberStyles.HexNumber);

                    if (recursivepath.EndsWith(@"\")) recursivepath = recursivepath.Remove(recursivepath.Length - 1);
                    string yy = recursivepath.Substring(recursivepath.LastIndexOf(@"\") + 2);
                    int y = int.Parse(yy, System.Globalization.NumberStyles.HexNumber);

                    recursivepath = recursivepath.Remove(recursivepath.LastIndexOf(@"\"));
                    string zz = recursivepath.Substring(recursivepath.LastIndexOf(@"\") + 2);
                    int z = int.Parse(zz, System.Globalization.NumberStyles.HexNumber);

                    customfile = false;

                    // Process Tile
                    ProcessTile(path, size, x, y, z);
                }
                catch { };
            };

            if (customfile && (size < 10485760)) // 10 MB
            {
                CustomFile cf = new CustomFile(recursivepath, name, (uint)size);
                ProcessCustom(path, cf);
            };
        }

        private static void ProcessTile(string path, long size, int x, int y, int z)
        {
            Tile t = new Tile(x, y, (byte)z, (uint)size);
            CopyFileToStr(path, t.Size, out t.File, out t.Position);

            if (mfIndex.Position + Marshal.SizeOf(t) > maxfilesize)
            {
                mfIndex.Close();
                mfIndex = new FileStream(mfMain.Name + ".index." + String.Format("{0:000}", ++mfIndexNo), FileMode.Create, FileAccess.Write, FileShare.None);
            };
            byte[] ba = Tile.TileToArray(t);
            //Tile check = Tile.ArrayToTile(ba);
            mfIndex.Write(ba, 0, ba.Length);
        }

        private static void ProcessCustom(string path, CustomFile cd)
        {
            CopyFileToStr(path, cd.Size, out cd.File, out cd.Position);
            byte[] ba = StructToArray<CustomFile>(cd);
            mfMain.Write(ba, 0, ba.Length);
        }
        
        private static void CopyFileToStr(string path, uint size, out uint fileNo, out uint Position)
        {
            fileNo = mfFilesNo;
            Position = (uint)mfFiles.Position;
            if (Position > size + maxfilesize)
            {
                mfFiles.Close();
                mfFiles = new FileStream(mfMain.Name + ".files." + String.Format("{0:000}", ++mfFilesNo), FileMode.Create, FileAccess.Write, FileShare.None);
                fileNo = mfFilesNo;
                Position = 0;
            };

            rwStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            int read;
            while ((read = rwStream.Read(rwBuffer, 0, rwBuffer.Length)) > 0)
                mfFiles.Write(rwBuffer, 0, read);
            rwStream.Close();
        }

        private static void MergeTiles(string source, string dest)
        {
            source = Path.GetFullPath(source);

            mfMain = new FileStream(dest, FileMode.Create, FileAccess.Write, FileShare.None);
            mfIndex = new FileStream(mfMain.Name + ".index." + String.Format("{0:000}", ++mfIndexNo), FileMode.Create, FileAccess.Write, FileShare.None);
            mfFiles = new FileStream(mfMain.Name + ".files." + String.Format("{0:000}", ++mfFilesNo), FileMode.Create, FileAccess.Write, FileShare.None);

            CustomHeader ch = new CustomHeader("");
            List<string> dirs = new List<string>(Directory.GetDirectories(source));
            do
            {
                string dir = dirs[0];
                dirs.RemoveAt(0);
                dirs.AddRange(Directory.GetDirectories(dir));
                if (dir.IndexOf("_alllayers") >= 0)
                {
                    dir = dir.Replace(source+@"\", "")+@"\";
                    ch.TilesRelativePath = dir;
                    dirs.Clear();
                };                
            }
            while (dirs.Count > 0);            

            byte[] ba = StructToArray<CustomHeader>(ch);
            mfMain.Write(ba, 0, ba.Length);
            mfMain.Flush();

            // DoWork
            RecurseSearch(source, "", 10);

            mfIndex.Close();
            mfMain.Close();
            mfFiles.Close();


            mfMain = new FileStream(mfMain.Name + ".txt", FileMode.Create, FileAccess.Write, FileShare.None);
            ba = System.Text.Encoding.GetEncoding(1251).GetBytes(String.Format("{0} bytes in {1} files in {2} dirs",totalsize,totalfiles,totaldirs));
            mfMain.Write(ba, 0, ba.Length);
            mfMain.Close();
        }

        #region GetTilePath
        public static string GetTileSubPath(int x, int y, int z)
        {
            return String.Format(@"\L{0}\R{1}\C{2}.png", TwoZ(z), ToHex8(y), ToHex8(x));
        }

        private static string TwoZ(int val)
        {
            return val.ToString().Length > 1 ? val.ToString() : "0" + val.ToString();
        }

        private static string ToHex8(int val)
        {
            string res = val.ToString("X");
            while (res.Length < 8) res = "0" + res;
            return res;
        }
        #endregion

        private static void SaveFile(string name, uint File, uint Pos, uint Size)
        {
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(name));
            if (mfFilesNo != File)
            {
                mfFiles.Close();
                mfFiles = new FileStream(mfMain.Name + ".files." + String.Format("{0:000}", mfFilesNo = File), FileMode.Open, FileAccess.Read);
            };
            mfFiles.Position = (long)Pos;

            rwStream = new FileStream(name, FileMode.Create, FileAccess.Write);
            int totalRead = 0;
            int read;
            while ((read = mfFiles.Read(rwBuffer, 0, (int)((Size - totalRead) > rwBuffer.Length ? rwBuffer.Length : Size - totalRead))) > 0)
            {
                totalRead += read;
                rwStream.Write(rwBuffer, 0, read);
            };
            rwStream.Close();
        }

        private static void UpdateExtractStatus(string path, string name)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("{0}\t\r\n", version);
            Console.WriteLine("Extracting tiles...\t");
            Console.WriteLine("Current path:\t");
            Console.WriteLine(" " + path);
            Console.WriteLine("Current file:\t");
            Console.WriteLine(" " + name);
            Console.WriteLine("Extracted {0} files in {1} dirs", totalfiles, totaldirs);
            if (totalsize < 1024)
                Console.WriteLine("Size: {0} B\t\t", totalsize);
            else if (totalsize < 1024 * 1024)
                Console.WriteLine("Size: {0:0.00} KB\t\t", (double)totalsize / 1024);
            else
                Console.WriteLine("Size: {0:0.00} MB\t\t", (double)totalsize / 1024 / 1024);
            TimeSpan elapsed = DateTime.Now.Subtract(started);
            Console.WriteLine("Elapsed: " + String.Format("{0:00}:{1:00}:{2:00}:{3:00}", new object[] { elapsed.Days, elapsed.Hours, elapsed.Minutes, elapsed.Seconds }));
        }

        private static void ExtractTiles(string source, string dest)
        {
            mfMain = new FileStream(source, FileMode.Open, FileAccess.Read);
            mfFiles = new FileStream(mfMain.Name + ".files." + String.Format("{0:000}", ++mfFilesNo), FileMode.Open, FileAccess.Read);

            byte[] ba = new byte[Marshal.SizeOf(new CustomHeader())];
            mfMain.Read(ba, 0, ba.Length);
            CustomHeader ch = ArrayToStruct<CustomHeader>(ba);
            string tilesDir = dest + @"\" + ch.TilesRelativePath;
            Directory.CreateDirectory(tilesDir);

            string CD = "";

            ba = new byte[Marshal.SizeOf(new CustomFile())];
            while (mfMain.Position < mfMain.Length)
            {
                mfMain.Read(ba, 0, ba.Length);
                CustomFile cf = ArrayToStruct<CustomFile>(ba);
                if (CD != cf.RelativePath) { CD = cf.RelativePath; totaldirs++; }
                Directory.CreateDirectory(dest + @"\" + cf.RelativePath + @"\");
                SaveFile(dest + @"\" + cf.RelativePath + @"\" + cf.Name, cf.File, cf.Position, cf.Size);

                totalfiles++;
                totalsize += (ulong)cf.Size;

                UpdateExtractStatus(dest + @"\" + cf.RelativePath + @"\", cf.Name);                
            };
            mfMain.Close();

            {
                string trp = ch.TilesRelativePath.Replace(@"\\", @"\");
                totaldirs += (ulong)(trp.Length - trp.Replace(@"\", "").Length);
            };
            int CY = -1;
            int CZ = -1;

            ba = new byte[22];
            string indexFileName = "";
            while (File.Exists(indexFileName = mfMain.Name + ".index." + String.Format("{0:000}", ++mfIndexNo)))
            {
                mfIndex = new FileStream(indexFileName, FileMode.Open, FileAccess.Read);
                while (mfIndex.Position < mfIndex.Length)
                {
                    mfIndex.Read(ba, 0, ba.Length);
                    Tile t = Tile.ArrayToTile(ba);
                    if (CZ != t.Z) { CZ = t.Z; CY = -1; totaldirs++; }
                    if (CY != t.Y) { CY = t.Y; totaldirs++; }
                    string fileName = GetTileSubPath(t.X, t.Y, t.Z);
                    SaveFile(dest + @"\" + ch.TilesRelativePath + fileName, t.File, t.Position, t.Size);

                    totalfiles++;
                    totalsize += (ulong)t.Size;
                    skipfiles++;

                    if (skipfiles % 16 == 0)
                        UpdateExtractStatus(dest + @"\" + ch.TilesRelativePath + @"\", fileName);
                };
            };

            UpdateExtractStatus("", "");

            mfIndex.Close();
            mfFiles.Close();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("{0}\r\n",version);

            // -merge \\192.168.0.18\__mapnik\TILES_MSK E:\tiles.bin 1M /wait
            // -extract E:\tiles.bin E:\_TILES\ /wait

            //RecurseSearch(@"\\192.168.33.9\__mapnik\TILES_MSK\", "", 10);

            if ((args == null) || (args.Length < 3))
            {
                Console.WriteLine("syntax:\r\n  tza.exe -merge \"source_dir\"  \"destination_file\" [fileSizeM] [/wait]");
                Console.WriteLine("  tza.exe -extract \"source_file\" \"destination_dir\" [/wait]");
                Console.WriteLine("\r\ndefault fileSize is 512M");
                Console.WriteLine("\r\nexamples:");
                Console.WriteLine("  tza.exe -merge \"C:\\Tiles\"  \"D:\\tiles.tza\"");
                Console.WriteLine("  tza.exe -merge \"C:\\Tiles\"  \"D:\\tiles.tza\" 10M");
                Console.WriteLine("  tza.exe -merge \"C:\\Tiles\"  \"D:\\tiles.tza\" 10M /wait");
                Console.WriteLine("  tza.exe -extract \"D:\\tiles.tza\" \"C:\\Tiles\"");
                Console.WriteLine("  tza.exe -extract \"D:\\tiles.tza\" \"C:\\Tiles\" /wait");
                Console.WriteLine("\r\nЛучше всего применять к папке, в который лежит conf.xml");
                Console.WriteLine("  в случае примера путь к файлу: C:\\Tiles\\conf.xml");
                System.Threading.Thread.Sleep(5000);
                return;
            };

            Console.Clear();

            string method = args[0].ToLower();
            string source = args[1].ToLower();
            string dest = args[2].ToLower();
            bool wait = false;
            for (int i = 2; i < args.Length; i++)
            {
                if (args[i].ToUpper().EndsWith("M"))
                {
                    string sz = args[i];
                    sz = sz.Remove(sz.Length - 1);
                    maxfilesize = uint.Parse(sz) * 1024 * 1024;
                };
                if (args[i].ToLower() == "/wait") wait = true;
            };

            if (method == "-merge") MergeTiles(source, dest);
            if (method == "-extract") ExtractTiles(source, dest);

            Console.WriteLine("DONE");
            if (wait)
            {
                Console.WriteLine("Press Enter to Continue");
                Console.ReadLine();
            };
        }
    }
}
