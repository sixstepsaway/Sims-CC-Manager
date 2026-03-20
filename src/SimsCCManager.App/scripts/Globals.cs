using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Godot;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using MoreLinq.Extensions;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.OptionLists;
using SimsCCManager.PackageReaders;
using SimsCCManager.SettingsSystem;

namespace SimsCCManager.Globals
{
    public class GlobalVariables
    {
        public static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        public static string State = "Alpha";
        public static string CurrentVersion = "0.1";
        public static string AppName = "Sims CC Manager";
        public static string MyDocuments = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        public static string AppFolder {
            get { 
                if (PortableMode) { 
                    return InstallDirectory; 
                } else { 
                    if (!Directory.Exists(AppFolderNormal))
                    {
                        Directory.CreateDirectory(AppFolderNormal);
                    }
                    return AppFolderNormal; 
                }
            }
        }
        public static string AppFolderNormal = Path.Combine(MyDocuments, AppName);
        public static string AppFolderDebug = Path.Combine(MyDocuments, string.Format("{0}_Debug", AppName));
        public static string InstallDirectory = System.Environment.CurrentDirectory;

        public static bool DebugMode { get { if (LoadedSettings != null) return LoadedSettings.DebugMode; else return false; }}
        public static bool PortableMode { get { if (LoadedSettings != null) return LoadedSettings.PortableMode; else return false; }}
        public static bool RestrictCPU { get { if (LoadedSettings != null) return LoadedSettings.CPURestrict; else return false; }}
        public static bool CensorSkins { get { if (LoadedSettings != null) return LoadedSettings.CensorSkins; else return false; }}
        public static bool DebugToConsole = false;
        public static bool LoggedIn = false;
        public static bool GameRunning = false;
        public static string ffmpeg = Path.Combine(InstallDirectory, "tools\\ffmpeg\\bin\\ffmpeg.exe");
        public static string imagemagick = Path.Combine(InstallDirectory, "tools\\imagemagick\\magick.exe");
        public static string SettingsFile = Path.Combine(AppFolder, "Settings.xml");
        public static string TempFolder = Path.Combine(AppFolder, "Temp");
        public static string MovedItemsFile = Path.Combine(TempFolder, "moveditems.xml");
        public static string LogFolder = Path.Combine(AppFolder, "Logs");
        public static string DataFolder = Path.Combine(InstallDirectory, "Data");
        public static string OverridesFolder = Path.Combine(DataFolder, "Overrides");
        public static string ThemesFolder = Path.Combine(AppFolder, "Themes");


        public static ParallelOptions ParallelSettings
        {            
            get { 
                
                if (RestrictCPU)
                {
                    if (System.Environment.ProcessorCount > 1)
                    {
                        return new() { 
                            MaxDegreeOfParallelism = System.Environment.ProcessorCount - ((System.Environment.ProcessorCount / 4)*3)
                        };
                    } else
                    {
                        return new() { 
                            MaxDegreeOfParallelism = 1
                        };
                    }
                } else
                {
                    if (System.Environment.ProcessorCount > 1)
                    {
                        return new() { 
                            MaxDegreeOfParallelism = System.Environment.ProcessorCount - (System.Environment.ProcessorCount / 8)
                        };
                    } else
                    {
                        return new() { 
                            MaxDegreeOfParallelism = 1
                        };
                    }
                }
            }
        }






        public static List<string> Sims2DataFolders = new() { "Collections","LockedBins","Logs","PetBreeds","PetCoats","SC4Terrains","Teleport","LotCatalog" };
        public static List<string> Sims2DataFiles = new() { "Accessory.cache", "Groups.cache"};
        public static List<string> Sims2SavesFolders = new() { "Neighborhoods", "PackagedLots" };
        public static List<string> Sims2SettingsFolders = new() { "Cameras", "Config" };
        public static List<string> Sims2MediaFolders = new() { "Movies", "Music", "Paintings", "Screenshots", "Storytelling", "Thumbnails" };

        public static List<string> Sims3DataFolders = new() { "Collections", "ContentPatch", "DCBackup", "DCCache", "Downloads", "FeaturedItems", "IGACache", "SigsCache", "Thumbnails"  };
        public static List<string> Sims3DataFiles = new() { "CASPartCache.package", "compositorCache.package", "DeviceConfig.log", "scriptCache.package", "simCompositorCache.package", "Sims3LauncherLogFile.log", "Sims3Logs.xml", "Version.tag" };
        public static List<string> Sims3SavesFolders = new() { "CurrentGame.sims3", "Saves", "Exports", "SavedOutfits" };
        public static List<string> Sims3SettingsFiles = new() { "options.ini" };
        public static List<string> Sims3MediaFolders = new() { "Custom Music", "Recorded Videos", "Screenshots" };

        public static List<string> Sims4DataFolders = new() { "cachestr",  "Content", "onlinethumbnailcache", "ReticulatedSplinesView"};
        public static List<string> Sims4DataFiles = new() { "accountDataDB.package", "avatarcache.package", "clientDB.package", "ConnectionStatus.txt","GameVersion.txt", "houseDescription-client.package", "localsimtexturecache.package", "localthumbcache.package", "notify.glob", "UserData.lock"};
        public static List<string> Sims4SavesFolders = new() { "Saves", "Tray" };
        public static List<string> Sims4SettingsFolders = new() { "ConfigOverride"};
        public static List<string> Sims4SettingsFiles = new() { "UserSetting.ini", "Options.ini", "Events.ini", "GameConfig.ini", "Config.log"};
        public static List<string> Sims4MediaFolders = new() { "Recorded Videos", "Screenshots", "Custom Music" };

        public static List<string> SimsMedievalSavesFolders = new()
        {
            "Saves", "SavedSims", "CurrentGame.tsm"
        };

        public static List<string> SimsMedievalSavesFiles = new()
        {
            "userPresets.package"
        };

        public static List<string> SimsMedievalSettingsFiles = new()
        {
            "Options.ini"
        };

        public static List<string> SimsMedievalMediaFolders = new() { "Screenshots" };

        public static List<List<SimsOverrides>> Sims2Overrides = new();

        public static List<SpecificOverrides> Sims2SpecificOverrides = new();

        public static List<string> Sims2OverrideImages = new();



        public static MainWindow mainWindow;

        public static SCCMSettings LoadedSettings;
        public static SCCMTheme LoadedTheme = Themes.DefaultThemes[0];


        public static List<string> SimsFileExtensions = new(){
            ".package",
            ".sims3pack",
            ".sims2pack",
            ".ts4script"
        };

        public static List<string> Sims2Exes = new(){
            "Sims2EP9",
            "Sims2EP9RPC"
        };
        public static List<string> Sims3Exes = new(){
            "TS3W",
            "TS3"
        };
        public static List<string> Sims4Exes = new(){
            "TS4_DX9_x64",
            "TS4_x64"
        };
        public static List<string> SimsMedievalExes = new(){
            "TSM"
        };

        public static void RemoveTempFiles()
        {
            if (Directory.Exists(TempFolder))
            {
                Directory.Delete(TempFolder, true);
            }
        }

    }

    public class Utilities
    {
        public const string SignatureGzip = "1F-8B-08";
        public const string SignatureZip = "50-4B-03-04";

        public static bool CheckSignature(string filepath, int signatureSize, string expectedSignature)
        {
            if (String.IsNullOrEmpty(filepath)) throw new ArgumentException("Must specify a filepath");
            if (String.IsNullOrEmpty(expectedSignature)) throw new ArgumentException("Must specify a value for the expected file signature");
            using (FileStream fs = new FileStream(filepath, FileMode.Open, System.IO.FileAccess.ReadWrite))
            {
                if (fs.Length < signatureSize)
                    return false;
                byte[] signature = new byte[signatureSize];
                int bytesRequired = signatureSize;
                int index = 0;
                while (bytesRequired > 0)
                {
                    int bytesRead = fs.Read(signature, index, bytesRequired);
                    bytesRequired -= bytesRead;
                    index += bytesRead;
                }
                string actualSignature = BitConverter.ToString(signature);
                if (actualSignature == expectedSignature) return true;
                return false;
            }
        }

        


        public static void MoveExisting(string destination, bool folder = false)
        {
            if (folder)
            {
                if (Directory.Exists(destination))
                {
                    string movedto = Utilities.IncrementName(destination, true);
                    Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destination, movedto);
                }
            } else
            {
                if (File.Exists(destination))
                {
                    string movedto = Utilities.IncrementName(destination, true);
                    File.Move(destination, movedto);
                }
            }            
        }



        public static string IncrementName(string inputLocation, bool directory = false, int increment = 0)
        {
            
                
            
            if (directory)
            {
                if (Directory.Exists(inputLocation))
                {
                    string testLocation = string.Format("{0}_({1})", inputLocation, increment);
                    if (Directory.Exists(testLocation))
                    {
                        inputLocation = IncrementName(inputLocation, directory, increment);
                    } else
                    {
                        inputLocation = testLocation;
                    }  
                }                
            } else
            {
                if (File.Exists(inputLocation))
                {
                    FileInfo fileInfo = new(inputLocation);
                    string noextension = fileInfo.FullName.Replace(fileInfo.Extension, "");
                    string testLocation = string.Format("{0}_({1}){2}", noextension, increment, fileInfo.Extension);
                    if (File.Exists(testLocation))
                    {
                        inputLocation = IncrementName(inputLocation, directory, increment);
                    } else
                    {
                        inputLocation = testLocation;
                    }
                }
            }
            increment++;
            return inputLocation;
        }
        public static bool IsEven(int val){
            if ((val & 0x1) == 0){
                return true;
            } else {
                return false;
            }
        }

        public static long DirSize(DirectoryInfo d) 
        {    
            long size = 0;    
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis) 
            {      
                size += fi.Length;    
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis) 
            {
                size += DirSize(di);   
            }
            return size;  
        }

        static readonly string[] SizeSuffixes = 
                   { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            //From https://stackoverflow.com/a/14488941 
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); } 
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", 
                adjustedSize, 
                SizeSuffixes[mag]);
        }

        public static string GetPathForExe(string registryKey)
        {
            string InstallPath = "";
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(registryKey);

            if (regKey != null)
            {
                InstallPath = regKey.GetValue("Install Dir").ToString();
            }
            return InstallPath;
        }        

        private static void StartProcess(string processname){
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog("Process started!");
            Process[] runninggame = Process.GetProcessesByName(processname);
            if (runninggame.Length == 0){
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog("No game... Waiting!");
                StartProcess(processname);
            } else {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog("Found the game!");
                return;
            }
        }

        private static string WaitProcess(string processname, string result){
            Process[] runninggame = Process.GetProcessesByName(processname);
            if (runninggame.Length != 0){
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog("Game is running!");
                return WaitProcess(processname, result);
            } else {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog("Looks like the game closed!");
                return result;
            }
        }

        public static string SafeFileName(string input)
        {
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            return String.Join("_", input.Split(invalids, StringSplitOptions.RemoveEmptyEntries) ).TrimEnd('.');
        }

        public static void MoveToRecycleBin(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
                //credit: https://www.meziantou.net/moving-files-and-folders-to-recycle-bin-in-dotnet.htm
                var shellType = Type.GetTypeFromProgID("Shell.Application", throwOnError: true)!;
                dynamic shellApp = Activator.CreateInstance(shellType)!;

                // https://learn.microsoft.com/en-us/windows/win32/api/shldisp/ne-shldisp-shellspecialfolderconstants?WT.mc_id=DT-MVP-5003978
                var recycleBin = shellApp.Namespace(0xa);

                // https://learn.microsoft.com/en-us/windows/win32/shell/folder-movehere?WT.mc_id=DT-MVP-5003978
                recycleBin.MoveHere(path);
            }
        }

        public static string RemoveInvalidXmlChars(string content)
        {
            return  new string(content.Where(ch =>System.Xml.XmlConvert.IsXmlChar(ch)).ToArray());
        }
    }

    public class InstanceControllers
    {
        public static void ClearInstance(GameInstance instance = null, bool onlyclear = true)
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Removing residual files!"));

            VFSFiles filelist = new();
            XmlSerializer simsPackageSerializer = new XmlSerializer(typeof(VFSFiles));
            using (FileStream fileStream = new(GlobalVariables.MovedItemsFile, FileMode.Open, System.IO.FileAccess.Read)){
                using (StreamReader streamReader = new(fileStream)){
                    filelist = (VFSFiles)simsPackageSerializer.Deserialize(streamReader);
                    streamReader.Close();
                }
                fileStream.Close();
            }

            foreach (LinkedItems item in filelist.ItemsLinked)
            {
                if (item.IsFolder)
                {
                    if (Directory.Exists(item.LinkLocation))
                    {                    
                        DirectoryInfo fileInfo = new(item.LinkLocation);
                        if (fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                        {
                            Directory.Delete(item.LinkLocation);
                        }
                    }
                } else
                {
                   if (File.Exists(item.LinkLocation))
                    {                    
                        FileInfo fileInfo = new(item.LinkLocation);
                        if (fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint))
                        {
                            File.Delete(item.LinkLocation);
                        }
                    } 
                }                
            }

            foreach (MovedItems item in filelist.ItemsMoved)
            {
                if (item.IsFolder)
                {
                    if (Directory.Exists(item.MovedTo))
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(item.MovedTo, item.OriginalLocation);
                    }
                } else
                {
                    if (File.Exists(item.MovedTo))
                    {
                        File.Move(item.MovedTo, item.OriginalLocation);
                    }
                }
            }

            foreach (string createdFolder in filelist.FoldersCreated)
            {
                DirectoryInfo directory = new(createdFolder);                
                if (Directory.Exists(createdFolder))
                {
                    Directory.Delete(createdFolder, true);
                }
            }
            if (File.Exists(GlobalVariables.MovedItemsFile)) File.Delete(GlobalVariables.MovedItemsFile);

            if (!onlyclear)
            {
                switch (instance.GameChoice)
                {
                    case SimsGames.Sims2:
                        GetSims2LocalFiles(instance);
                    break;
                    case SimsGames.Sims3:
                        GetSims3LocalFiles(instance);
                    break;
                    case SimsGames.Sims4:
                        GetSims4LocalFiles(instance);
                    break;
                }
            }
        }

        public static void GetSimsMedievalLocalFiles(GameInstance instance)
        {
            string location = instance.GameDocumentsFolder;
            List<string> directories = Directory.GetDirectories(location).ToList();
            if (instance.LoadedProfile.LocalSaves)
            {
                foreach (string folder in GlobalVariables.SimsMedievalSavesFolders)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalSaveFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
                foreach (string file in GlobalVariables.SimsMedievalSavesFiles)
                {
                    string filePath = Path.Combine(location, file);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalSaveFolder, file);
                    if (File.Exists(filePath))
                    {
                        if (File.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            File.Move(destinationPath, moveloc);
                            
                        }
                        File.Move(filePath, destinationPath);
                    }
                }
            }
            if (instance.LoadedProfile.LocalMedia)
            {
                foreach (string folder in GlobalVariables.SimsMedievalMediaFolders)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalMediaFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
            }
            if (instance.LoadedProfile.LocalSettings)
            {
                foreach (string file in GlobalVariables.SimsMedievalSettingsFiles)
                {
                    string filePath = Path.Combine(location, file);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalSettingsFolder, file);
                    if (File.Exists(filePath))
                    {
                        if (File.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            File.Move(destinationPath, moveloc);
                            
                        }
                        File.Move(filePath, destinationPath);
                    }
                }
            }

        }

        public static void GetSims4LocalFiles(GameInstance instance)
        {
            string location = instance.GameDocumentsFolder;
            List<string> directories = Directory.GetDirectories(location).ToList();
            if (instance.LoadedProfile.LocalData)
            {
                foreach (string folder in GlobalVariables.Sims4DataFolders)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalDataFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
                foreach (string file in GlobalVariables.Sims4DataFiles)
                {
                    string filePath = Path.Combine(location, file);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalDataFolder, file);
                    if (File.Exists(filePath))
                    {
                        if (File.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, false);
                            File.Move(destinationPath, moveloc);
                            
                        }
                        File.Move(filePath, destinationPath);
                    }
                }
            }
            if (instance.LoadedProfile.LocalSaves)
            {
                foreach (string folder in GlobalVariables.Sims4SavesFolders)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalSaveFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
            }
            if (instance.LoadedProfile.LocalMedia)
            {
                foreach (string folder in GlobalVariables.Sims4MediaFolders)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalMediaFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
            }
            if (instance.LoadedProfile.LocalSettings)
            {
                foreach (string folder in GlobalVariables.Sims4SettingsFiles)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalSettingsFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
                foreach (string folder in GlobalVariables.Sims4SettingsFolders)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalSettingsFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
            }
        }

        public static void GetSims3LocalFiles(GameInstance instance)
        {
            string location = instance.GameDocumentsFolder;
            List<string> directories = Directory.GetDirectories(location).ToList();
            if (instance.LoadedProfile.LocalData)
            {
                foreach (string folder in GlobalVariables.Sims3DataFolders)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalDataFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
                foreach (string file in GlobalVariables.Sims3DataFiles)
                {
                    string filePath = Path.Combine(location, file);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalDataFolder, file);
                    if (File.Exists(filePath))
                    {
                        if (File.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, false);
                            File.Move(destinationPath, moveloc);
                            
                        }
                        File.Move(filePath, destinationPath);
                    }
                }
            }
            if (instance.LoadedProfile.LocalSaves)
            {
                foreach (string folder in GlobalVariables.Sims3SavesFolders)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalSaveFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
            }
            if (instance.LoadedProfile.LocalMedia)
            {
                foreach (string folder in GlobalVariables.Sims3MediaFolders)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalMediaFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
            }
            if (instance.LoadedProfile.LocalSettings)
            {
                foreach (string folder in GlobalVariables.Sims3SettingsFiles)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalSettingsFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
            }
        }

        public static void GetSims2LocalFiles(GameInstance instance)
        {
            string location = instance.GameDocumentsFolder;
            List<string> directories = Directory.GetDirectories(location).ToList();
            if (instance.LoadedProfile.LocalData)
            {
                foreach (string folder in GlobalVariables.Sims2DataFolders)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalDataFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
                foreach (string file in GlobalVariables.Sims2DataFiles)
                {
                    string filePath = Path.Combine(location, file);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalDataFolder, file);
                    if (File.Exists(filePath))
                    {
                        if (File.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, false);
                            File.Move(destinationPath, moveloc);
                            
                        }
                        File.Move(filePath, destinationPath);
                    }
                }
            }
            if (instance.LoadedProfile.LocalSaves)
            {
                foreach (string folder in GlobalVariables.Sims2SavesFolders)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalSaveFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
            }
            if (instance.LoadedProfile.LocalMedia)
            {
                foreach (string folder in GlobalVariables.Sims2MediaFolders)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalMediaFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
            }
            if (instance.LoadedProfile.LocalSettings)
            {
                foreach (string folder in GlobalVariables.Sims2SettingsFolders)
                {
                    string folderPath = Path.Combine(location, folder);
                    string destinationPath = Path.Combine(instance.LoadedProfile.LocalSettingsFolder, folder);
                    if (Directory.Exists(folderPath))
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                            string moveloc = Utilities.IncrementName(destinationPath, true);
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                            
                        }
                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(folderPath, destinationPath);
                    }
                }
            }
        }

        public static ConcurrentBag<Task> runningTasks = new();

        public static GameInstance InitialLoad(GameInstance gameInstance)
        {
            XmlSerializer simsPackageSerializer = new XmlSerializer(typeof(SimsPackage));
            List<string> files = Directory.GetFiles(gameInstance.InstanceFolders.InstancePackagesFolder).ToList();
            List<string> folders = Directory.GetDirectories(gameInstance.InstanceFolders.InstancePackagesFolder).ToList();
            
            List<string> catFolders = folders.Where(x => x.Contains("__CATEGORY_")).ToList();
            foreach (string dir in catFolders)
            {
                files.AddRange(Directory.GetFiles(dir));
                folders.AddRange(Directory.GetDirectories(dir));
            }

            foreach (string file in files)
            {
                FileInfo fi = new(file);
                if (GlobalVariables.SimsFileExtensions.Contains(fi.Extension))
                {
                    SimsPackage simsPackage = new();
                    simsPackage.Location = file; 
                    if (File.Exists(simsPackage.InfoFile))
                    {
                        if (GlobalVariables.DebugMode)
                        {                    
                            using (FileStream fileStream = new(simsPackage.InfoFile, FileMode.Open, System.IO.FileAccess.Read)){
                                using (StreamReader streamReader = new(fileStream)){
                                    simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                                    streamReader.Close();
                                }
                                fileStream.Close();
                            }
                        } else
                        {
                            using (FileStream fs = File.OpenRead(simsPackage.InfoFile))
                            using (ZipFile zipFile = new ZipFile(fs))
                            {
                                if (!Utilities.CheckSignature(simsPackage.InfoFile, 4, Utilities.SignatureZip)){
                                    using (FileStream fileStream = new(simsPackage.InfoFile, FileMode.Open, System.IO.FileAccess.Read)){
                                        using (StreamReader streamReader = new(fileStream)){
                                            simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                                            streamReader.Close();
                                        }
                                        fileStream.Close();
                                    }
                                } else
                                {
                                    foreach (ZipEntry entry in zipFile)
                                    {
                                        if (!entry.IsFile) continue; // Skip directories
                                        
                                        using (Stream zipStream = zipFile.GetInputStream(entry))
                                        using (MemoryStream outputStream = new())
                                        {
                                            zipStream.CopyTo(outputStream);
                                            simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(outputStream);
                                        }
                                    }
                                }                        
                            }
                        }
                        simsPackage.HasBeenRead = true;
                    } else
                    {
                        simsPackage.FileName = fi.Name;
                        simsPackage.StandAlone = true;
                        simsPackage.Game = gameInstance.GameChoice;
                        simsPackage.PackageCategory = gameInstance.Categories.First(x => x.Name == "Default"); 
                        //simsPackage.WriteXML();                        
                    }
                    gameInstance._packages.Add(simsPackage);
                    int incBy = 1;
                    GlobalVariables.mainWindow.IncrementLoadingScreen(incBy, fi.Name.Replace(".info", ""), "Globals: ReadPackages 1");
                } else
                {
                    GlobalVariables.mainWindow.IncrementLoadingScreen(1, fi.Name.Replace(".info", ""), "Globals: ReadPackages 2");
                }
            }

            foreach (string folder in folders)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", folder));
                DirectoryInfo fi = new(folder);
                if (!fi.Name.StartsWith("__CATEGORY_"))
                {
                    SimsPackage simsPackage = new() { Location = folder, IsDirectory = true };
                    if (File.Exists(simsPackage.InfoFile))
                    {
                        if (GlobalVariables.DebugMode)
                        {                    
                            using (FileStream fileStream = new(simsPackage.InfoFile, FileMode.Open, System.IO.FileAccess.Read)){
                                using (StreamReader streamReader = new(fileStream)){
                                    simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                                    streamReader.Close();
                                }
                                fileStream.Close();
                            }
                        } else
                        {
                            using (FileStream fs = File.OpenRead(simsPackage.InfoFile))
                            using (ZipFile zipFile = new ZipFile(fs))
                            {
                                if (!Utilities.CheckSignature(simsPackage.InfoFile, 4, Utilities.SignatureZip)){
                                    using (FileStream fileStream = new(simsPackage.InfoFile, FileMode.Open, System.IO.FileAccess.Read)){
                                        using (StreamReader streamReader = new(fileStream)){
                                            simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                                            streamReader.Close();
                                        }
                                        fileStream.Close();
                                    }
                                } else
                                {
                                    foreach (ZipEntry entry in zipFile)
                                    {
                                        if (!entry.IsFile) continue; // Skip directories
                                        
                                        using (Stream zipStream = zipFile.GetInputStream(entry))
                                        using (MemoryStream outputStream = new())
                                        {
                                            zipStream.CopyTo(outputStream);
                                            simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(outputStream);
                                        }
                                    }
                                }                        
                            }
                        } 
                        simsPackage.HasBeenRead = true;
                    } else
                    {
                        simsPackage.IsDirectory = true;
                        simsPackage.FileName = fi.Name;
                        simsPackage.StandAlone = true;
                        simsPackage.PackageCategory = gameInstance.Categories.First(x => x.Name == "Default");
                        simsPackage.Game = gameInstance.GameChoice;   
                        simsPackage.LinkedFiles.AddRange(Directory.EnumerateFiles(simsPackage.Location, "*.*", SearchOption.AllDirectories));
                        simsPackage.LinkedFolders.AddRange(Directory.EnumerateDirectories(simsPackage.Location, "*.*", SearchOption.AllDirectories));
                        //simsPackage.WriteXML();                     
                    }
                    gameInstance._packages.Add(simsPackage);
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", folder));
                    
                    int incBy = 1;
                    if (simsPackage.IsDirectory)
                    {
                        if (simsPackage.LinkedFiles.Count > 0) incBy += simsPackage.LinkedFiles.Count;
                        if (simsPackage.LinkedFolders.Count > 0) incBy += simsPackage.LinkedFolders.Count;
                    }
                    GlobalVariables.mainWindow.IncrementLoadingScreen(incBy, fi.Name.Replace(".info", ""), "Globals: ReadPackages 3");
                }
            }

            


            foreach (SimsPackage package in gameInstance._packages.OrderBy(x=>x.FileName))
            {
                gameInstance.Files.Add(package);
            }
            foreach (SimsDownload dl in gameInstance._downloads.OrderBy(x=>x.FileName))
            {
                gameInstance.Files.Add(dl);
            }
            return gameInstance;
        }

        /*public static GameInstance LoadInstanceFiles(GameInstance gameInstance)
        {
            runningTasks = new();
            List<string> files = Directory.GetFiles(gameInstance.InstanceFolders.InstancePackagesFolder).ToList();
            List<string> folders = Directory.GetDirectories(gameInstance.InstanceFolders.InstancePackagesFolder).ToList();
            
            List<string> catFolders = folders.Where(x => x.Contains("__CATEGORY_")).ToList();
            foreach (string dir in catFolders)
            {
                files.AddRange(Directory.GetFiles(dir));
                folders.AddRange(Directory.GetDirectories(dir));
            }


            foreach (string file in files)
            {
                Task t = new Task(() => {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                    FileInfo fi = new(file);
                    if (GlobalVariables.SimsFileExtensions.Contains(fi.Extension))
                    {
                        SimsPackage simsPackage = ReadPackage(file, gameInstance, fi);                    
                        gameInstance._packages.Add(simsPackage);                        
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));
                        int incBy = 1;
                        if (simsPackage.IsDirectory)
                        {
                            if (simsPackage.LinkedFiles.Count > 0) incBy += simsPackage.LinkedFiles.Count;
                            if (simsPackage.LinkedFolders.Count > 0) incBy += simsPackage.LinkedFolders.Count;
                        }
                        GlobalVariables.mainWindow.IncrementLoadingScreen(incBy, fi.Name.Replace(".info", ""), "Globals: ReadPackages 1");
                    } else
                    {
                        GlobalVariables.mainWindow.IncrementLoadingScreen(1, fi.Name.Replace(".info", ""), "Globals: ReadPackages 2");
                    }                    
                });
                runningTasks.Add(t);
            }
            foreach (string file in folders)
            {
                
                Task t = new Task(() => {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                    DirectoryInfo fi = new(file);
                    if (!fi.Name.StartsWith("__CATEGORY_"))
                    {
                        SimsPackage simsPackage = ReadPackage(file, gameInstance, fi);                
                        gameInstance._packages.Add(simsPackage);
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));
                        
                        if (!simsPackage.RootMod) gameInstance = GetSubDirectories(gameInstance, file, simsPackage);
                        
                        int incBy = 1;
                        if (simsPackage.IsDirectory)
                        {
                            if (simsPackage.LinkedFiles.Count > 0) incBy += simsPackage.LinkedFiles.Count;
                            if (simsPackage.LinkedFolders.Count > 0) incBy += simsPackage.LinkedFolders.Count;
                        }
                        GlobalVariables.mainWindow.IncrementLoadingScreen(incBy, fi.Name.Replace(".info", ""), "Globals: ReadPackages 3");
                    }                    
                });
                runningTasks.Add(t);
            }
            foreach (string file in Directory.GetFiles(gameInstance.InstanceFolders.InstanceDownloadsFolder))
            {
                Task t = new Task(() => {
                    FileInfo f = new(file);
                    SimsDownload simsDownload = ReadDownload(file, f);
                    gameInstance._downloads.Add(simsDownload);
                    GlobalVariables.mainWindow.IncrementLoadingScreen(1, f.Name.Replace(".info", ""), "Globals: ReadPackages 4");
                });
                runningTasks.Add(t);
            }   

            Parallel.ForEach(runningTasks, GlobalVariables.ParallelSettings, t =>
            {
                t.Start();
            });

            while (runningTasks.Any(x => !x.IsCompleted))
            {
                
            }

            runningTasks.Clear();
            Task r = new Task(() => {
               gameInstance = FindOrphans(gameInstance);
            });
            runningTasks.Add(r);
            /*t = new Task(() => {
               gameInstance = FindDupes(gameInstance);
            });
            runningTasks.Add(t);*/
            /*Parallel.ForEach(runningTasks, GlobalVariables.ParallelSettings, t =>
            {
                t.Start();
            });
            
            while (runningTasks.Any(x => !x.IsCompleted))
            {
                
            }
            
            
            //gameInstance = FindOverrides(gameInstance);


            foreach (SimsPackage package in gameInstance._packages.OrderBy(x=>x.FileName))
            {
                gameInstance.Files.Add(package);
            }
            foreach (SimsDownload dl in gameInstance._downloads.OrderBy(x=>x.FileName))
            {
                gameInstance.Files.Add(dl);
            }
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Packages: {0}, Downloads: {1}, Files: {2}", gameInstance._packages.Count, gameInstance._downloads.Count, gameInstance.Files.Count));
            return gameInstance;      
        }*/

        public static GameInstance FindDupes(GameInstance gameInstance)
        {
            if (gameInstance._packages.Any(x => x.Game == SimsGames.Sims2))
            {
                gameInstance = FindS2Dupes(gameInstance);
            }

            return gameInstance;
        }
        public static GameInstance FindOverrides(GameInstance gameInstance)
        {
            
            if (gameInstance._packages.Any(x => x.Game == SimsGames.Sims2))
            {
                gameInstance = FindS2Overrides(gameInstance);
            }

            return gameInstance;
        }

        public static GameInstance FindS2Dupes(GameInstance gameInstance)
        {
            /*List<SimsPackage> dupes = gameInstance._packages.Where(x => gameInstance._packages.Any(p => x.ObjectGUID == p.ObjectGUID && x.Identifier != p.Identifier && !string.IsNullOrEmpty(x.ObjectGUID) && !string.IsNullOrEmpty(p.ObjectGUID))).ToList();
                     
            StringBuilder sb = new();
            foreach (SimsPackage package in dupes)
            {
                
                sb.AppendLine(package.FileName);
            }
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("DUPLICATES/CONFLICTS FOUND: {0}", sb.ToString()));*/

            

            return gameInstance;
        }

        public static GameInstance FindS2Overrides(GameInstance gameInstance)
        {
            List<List<SimsOverrides>> overrides = new();   

            XmlSerializer serializer = new XmlSerializer(typeof(List<SimsOverrides>));

            string[] xmls = [..Directory.EnumerateFiles(@"O:\Godot Projects\SimsCCManager\src\SimsCCManager.App\Data\Overrides\Sims 2", "*.xml")];

            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found {0} xml files for overrides.", xmls.Count()));

            foreach (string xml in xmls)
            {
                using (FileStream fileStream = new(xml, FileMode.Open, System.IO.FileAccess.Read)){
                    using (StreamReader streamReader = new(fileStream)){
                        List<SimsOverrides> ovrd = (List<SimsOverrides>)serializer.Deserialize(streamReader);         
                        overrides.Add(ovrd);               
                        streamReader.Close();
                    }
                    fileStream.Close();
                }
            }

            int ovc = 0;
            foreach (List<SimsOverrides> list in overrides)
            {
                ovc += list.Count;
            }

            GlobalVariables.mainWindow.ChangeStage(ovc, "Finding defaults and overrides", 4);

            ConcurrentBag<OverrideMatch> Matches = new();

            Parallel.ForEach(overrides, GlobalVariables.ParallelSettings, or =>
            {
                Parallel.ForEach(or, GlobalVariables.ParallelSettings, o =>
                {   
                    GlobalVariables.mainWindow.IncrementLoadingScreen(1, "", "Globals: Overrides: entries");
                    foreach (SimsID id in o.Entries)
                    {                            
                        List<SimsPackage> packages = [..gameInstance._packages.Where(x => x.Sims2Data.IndexEntries.Any(i => i.CompleteID == id.FullKey && !string.IsNullOrEmpty(i.CompleteID) && !string.IsNullOrEmpty(id.FullKey)))];
                        foreach (SimsPackage p in packages)
                        {
                            OverrideMatch match = new() {
                                Package = p, OverrideReference = o
                            };
                            
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}", match.ToString()));
                            Matches.Add(match);
                        }
                    }                    
                });
                Parallel.ForEach(or, GlobalVariables.ParallelSettings, o =>
                {        
                    GlobalVariables.mainWindow.IncrementLoadingScreen(1, "", "Globals: Overrides: guids");            
                    foreach (ItemOverride io in o.GuidOverrides)
                    {                            
                        List<SimsPackage> packages = [..gameInstance._packages.Where(x => x.ObjectGUID == io.Overridden && !string.IsNullOrEmpty(x.ObjectGUID) && !string.IsNullOrEmpty(io.Overridden))];
                        foreach (SimsPackage p in packages)
                        {
                            OverrideMatch match = new() {
                                Package = p, OverrideReference = o
                            };                                
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}", match.ToString()));
                            Matches.Add(match);
                        }
                    }                    
                });
                Parallel.ForEach(or, GlobalVariables.ParallelSettings, o =>
                {
                    GlobalVariables.mainWindow.IncrementLoadingScreen(1, "", "Globals: Overrides: texture name overrides");
                    foreach (ItemOverride io in o.TextureNameOverrides)
                    {                            
                        List<SimsPackage> packages = [..gameInstance._packages.Where(p => p.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName == io.Overridden && !string.IsNullOrEmpty(t.FullTXTRName) && !string.IsNullOrEmpty(io.Overridden)))];

                        packages.AddRange(gameInstance._packages.Where(p => p.Sims2Data.TXMTDataBlock.Any(t => t.stdMatBaseTextureName == io.Overridden && !string.IsNullOrEmpty(t.stdMatBaseTextureName) && !string.IsNullOrEmpty(io.Overridden))));

                        packages = [..packages.Distinct()];

                        foreach (SimsPackage p in packages)
                        {
                            OverrideMatch match = new() {
                                Package = p, OverrideReference = o
                            };                                
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}", match.ToString()));
                            Matches.Add(match);
                        }
                    }
                });
                Parallel.ForEach(or, GlobalVariables.ParallelSettings, o =>
                {
                    GlobalVariables.mainWindow.IncrementLoadingScreen(1, "", "Globals: Overrides: filenameoverrides");
                    foreach (ItemOverride io in o.FileNameOverrides)
                    {                            
                        List<SimsPackage> packages = [..gameInstance._packages.Where(x => x.Sims2Data.SHPEDataBlock.Any(s => s.FileName == io.Overridden && !string.IsNullOrEmpty(s.FileName) && !string.IsNullOrEmpty(io.Overridden)))];
                        foreach (SimsPackage p in packages)
                        {
                            OverrideMatch match = new() {
                                Package = p, OverrideReference = o
                            };                                
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}", match.ToString()));
                            Matches.Add(match);
                        }
                    }
                });
                Parallel.ForEach(or, GlobalVariables.ParallelSettings, o =>
                {
                    GlobalVariables.mainWindow.IncrementLoadingScreen(1, "", "Globals: Overrides: refs");
                    foreach (ItemOverride io in o.References)
                    {                            
                        List<SimsPackage> packages = [..gameInstance._packages.Where(x => x.Sims2Data.TXMTDataBlock.Any(t => t.MaterialDescription == io.Overridden && !string.IsNullOrEmpty(t.MaterialDescription) && !string.IsNullOrEmpty(io.Overridden)))];
                        foreach (SimsPackage p in packages)
                        {
                            OverrideMatch match = new() {
                                Package = p, OverrideReference = o
                            };                                
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}", match.ToString()));
                            Matches.Add(match);
                        }
                    }
                });

            });

            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found {0} matches", Matches.Count));

            Matches = [..Matches.GroupBy(x => x.Package).SelectMany(group => group)];
            
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found {0} package matches", Matches.Count));

            foreach (OverrideMatch match in Matches)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Updating {0} with overrides.", match.Package.FileName));
                //gameInstance._packages.First(x => x.Identifier == match.Package.Identifier).Override = true;
                gameInstance._packages.First(x => x.Identifier == match.Package.Identifier).OverrideReference.Add(match.OverrideReference);
                gameInstance._packages.First(x => x.Identifier == match.Package.Identifier).WriteXML();
            }

            /*if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found {0} overrides.", overrides.Count));

            List<SimsOverrides> overwrittenguids = [..og];

            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Overwritten guids: {0}", overwrittenguids.Count));

            List<SimsOverrides> overwrittentextures = [..ot];
            
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Overwritten textures: {0}", overwrittentextures.Count));

            List<SimsOverrides> overwrittenfilenames = [..of];
            
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Overwritten filenames: {0}", overwrittenfilenames.Count));

            List<SimsOverrides> overwrittenreferences = [..oref];
            
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Overwritten refs: {0}", overwrittenreferences.Count));

            List<SimsOverrides> overwrittenentries = [..oe];
            
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Overwritten entries: {0}", overwrittenentries.Count));
            
            StringBuilder sb = new();
            sb.Append("List:");
            int i = 0;
            foreach (SimsOverrides package in overwrittenguids)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Getting guid override #{0}.", i));
                List<SimsPackage> p = new();
                if (gameInstance._packages.Any(x => package.Entries.Any(g => x.Sims2Data.IndexEntries.Any(p => p.CompleteID == g.FullKey && !string.IsNullOrEmpty(p.CompleteID) && !string.IsNullOrEmpty(g.FullKey))))) {
                    p = [..gameInstance._packages.Where(x => package.Entries.Any(g => x.Sims2Data.IndexEntries.Any(p => p.CompleteID == g.FullKey && !string.IsNullOrEmpty(p.CompleteID) && !string.IsNullOrEmpty(g.FullKey))))];
                    
                    foreach (SimsPackage pack in p)
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Package {0} is a GUID override.", pack.FileName));
                        sb.AppendLine(string.Format("Package {0} is a GUID override.", pack.FileName));
                        gameInstance._packages.First(x => x.Identifier == pack.Identifier).Override = true;
                        gameInstance._packages.First(x => x.Identifier == pack.Identifier).OverrideReference.Add(package);
                    }
                } else
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Couldn't find a package to match guid override #{0}.", overwrittenguids.IndexOf(package)));
                }
                i++;
            }
            foreach (SimsOverrides package in overwrittentextures)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Getting texture overrides #{0}.", i));
                List<SimsPackage> p = new();
                if (gameInstance._packages.Any(x => package.TextureNameOverrides.Any(g => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName == g.Overridden && !string.IsNullOrEmpty(t.FullTXTRName) && !string.IsNullOrEmpty(g.Overridden)))))
                {
                    p = [..gameInstance._packages.Where(x => package.TextureNameOverrides.Any(g => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName == g.Overridden && !string.IsNullOrEmpty(t.FullTXTRName) && !string.IsNullOrEmpty(g.Overridden))))];
                    foreach (SimsPackage pack in p)
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Package {0} is a texture override.", pack.FileName));
                        sb.AppendLine(string.Format("Package {0} is a texture override.", pack.FileName));
                        gameInstance._packages.First(x => x.Identifier == pack.Identifier).Override = true;
                        gameInstance._packages.First(x => x.Identifier == pack.Identifier).OverrideReference.Add(package);
                    }                
                } else if (gameInstance._packages.Any(x => package.TextureNameOverrides.Any(g => x.Sims2Data.TXMTDataBlock.Any(t => t.MaterialDescription == g.Overridden && !string.IsNullOrEmpty(g.Overridden) && !string.IsNullOrEmpty(t.MaterialDescription)))))
                {
                    p = [..gameInstance._packages.Where(x => package.TextureNameOverrides.Any(g => x.Sims2Data.TXMTDataBlock.Any(t => t.MaterialDescription == g.Overridden && !string.IsNullOrEmpty(g.Overridden) && !string.IsNullOrEmpty(t.MaterialDescription))))];
                    foreach (SimsPackage pack in p)
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Package {0} is a texture override.", pack.FileName));
                        sb.AppendLine(string.Format("Package {0} is a texture override.", pack.FileName));
                        gameInstance._packages.First(x => x.Identifier == pack.Identifier).Override = true;
                        gameInstance._packages.First(x => x.Identifier == pack.Identifier).OverrideReference.Add(package);
                    }
                } else if (gameInstance._packages.Any(x => package.TextureNameOverrides.Any(g => x.Sims2Data.TXMTDataBlock.Any(t => t.stdMatBaseTextureName == g.Overridden && !string.IsNullOrEmpty(t.stdMatBaseTextureName) && !string.IsNullOrEmpty(g.Overridden)))))
                {
                    p = [..gameInstance._packages.Where(x => package.TextureNameOverrides.Any(g => x.Sims2Data.TXMTDataBlock.Any(t => t.stdMatBaseTextureName == g.Overridden && !string.IsNullOrEmpty(t.stdMatBaseTextureName) && !string.IsNullOrEmpty(g.Overridden))))];
                    foreach (SimsPackage pack in p)
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Package {0} is a texture override.", pack.FileName));
                        sb.AppendLine(string.Format("Package {0} is a texture override.", pack.FileName));
                        gameInstance._packages.First(x => x.Identifier == pack.Identifier).Override = true;
                        gameInstance._packages.First(x => x.Identifier == pack.Identifier).OverrideReference.Add(package);
                    } 
                }
                i++;              
            }
            i=0;
            foreach (SimsOverrides package in overwrittenreferences)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Getting reference overrides #{0}.", i));
                List<SimsPackage> p = new();
                if (gameInstance._packages.Any(x => package.TextureNameOverrides.Any(g => x.Sims2Data.TXMTDataBlock.Any(t => t.MaterialDescription == g.Overridden && !string.IsNullOrEmpty(g.Overridden) && !string.IsNullOrEmpty(t.MaterialDescription)))))
                {
                    p = [..gameInstance._packages.Where(x => package.TextureNameOverrides.Any(g => x.Sims2Data.TXMTDataBlock.Any(t => t.MaterialDescription == g.Overridden && !string.IsNullOrEmpty(g.Overridden) && !string.IsNullOrEmpty(t.MaterialDescription))))];
                    foreach (SimsPackage pack in p)
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Package {0} is a reference override.", pack.FileName));
                        sb.AppendLine(string.Format("Package {0} is a reference override.", pack.FileName));
                        gameInstance._packages.First(x => x.Identifier == pack.Identifier).Override = true;
                        gameInstance._packages.First(x => x.Identifier == pack.Identifier).OverrideReference.Add(package);
                    }  
                }
                i++;        
            }    
            i = 0;       
            foreach (SimsOverrides package in overwrittenentries)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Getting entry ovrrides #{0}.", i));
                List<SimsPackage> p = new();
                if (gameInstance._packages.Any(x => package.Entries.Any(g => x.Sims2Data.IndexEntries.Any(i => i.CompleteID == g.FullKey && !string.IsNullOrEmpty(i.CompleteID) && !string.IsNullOrEmpty(g.FullKey)))))
                {
                    p = [..gameInstance._packages.Where(x => package.Entries.Any(g => x.Sims2Data.IndexEntries.Any(i => i.CompleteID == g.FullKey && !string.IsNullOrEmpty(i.CompleteID) && !string.IsNullOrEmpty(g.FullKey))))];
                    foreach (SimsPackage pack in p)
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Package {0} is an entry override.", pack.FileName));
                        sb.AppendLine(string.Format("Package {0} is an entry GUID override.", pack.FileName));
                        gameInstance._packages.First(x => x.Identifier == pack.Identifier).Override = true;
                        gameInstance._packages.First(x => x.Identifier == pack.Identifier).OverrideReference.Add(package);
                    }  
                }
                i++;     
            }
            
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("OVERRIDES FOUND: {0}", sb.ToString()));
                        

            /*List<SimsPackage> clothingdefaults = [..gameInstance._packages.Where(x => x.Sims2Data.TXTRDataBlock.Any(t => GlobalVariables.Sims2DefaultOverrideTextureNames_Clothing.Any(c => t.TextureName.Contains(c))))];
            List<SimsPackage> hairdefaults = [..gameInstance._packages.Where(x => x.Sims2Data.TXTRDataBlock.Any(t => GlobalVariables.Sims2DefaultOverrideTextureNames_Hair.Any(c => t.TextureName.Contains(c))))];

            foreach (SimsPackage p in clothingdefaults)
            {
                gameInstance._packages.First(x => x.Identifier == p.Identifier).Override = true;                
                string reff = GlobalVariables.Sims2DefaultOverrideTextureNames_Clothing.First(x => p.Sims2Data.TXTRDataBlock.Any(t => t.TextureName.Contains(x)));
                FunctionSortList functionSort = new();
                if (reff.Contains("body"))
                {
                    functionSort = new() { Category = "Clothing", Subcategory = "Full Body"};
                } else if (reff.Contains("bottom"))
                {
                    functionSort = new() { Category = "Clothing", Subcategory = "Bottom"};
                } else if (reff.Contains("top"))
                {
                    functionSort = new() { Category = "Clothing", Subcategory = "Top"};
                } else
                {
                    functionSort = new() { Category = "Clothing"};
                }
                gameInstance._packages.First(x => x.Identifier == p.Identifier).Sims2Data.FunctionSort.Add(functionSort);
                gameInstance._packages.First(x => x.Identifier == p.Identifier).OverrideReference = reff;
                gameInstance._packages.First(x => x.Identifier == p.Identifier).WriteXML();
            }
            foreach (SimsPackage p in hairdefaults)
            {
                gameInstance._packages.First(x => x.Identifier == p.Identifier).Override = true;
                gameInstance._packages.First(x => x.Identifier == p.Identifier).Sims2Data.AltType = "Hair";
                string reff = GlobalVariables.Sims2DefaultOverrideTextureNames_Clothing.First(x => p.Sims2Data.TXTRDataBlock.Any(t => t.TextureName.Contains(x)));
                gameInstance._packages.First(x => x.Identifier == p.Identifier).OverrideReference = reff;
                gameInstance._packages.First(x => x.Identifier == p.Identifier).WriteXML();
            }*/

            return gameInstance;
        }

        

        public static GameInstance FindOrphans(GameInstance gameInstance)
        {
            if (gameInstance._packages.Any(x => x.Game == SimsGames.Sims2))
            {
                gameInstance = FindS2Orphans(gameInstance);
            }
            
            return gameInstance;
        }

        public static GameInstance FindS2Orphans(GameInstance gameInstance)
        {            
            List<SimsPackage> meshes = [..gameInstance._packages.Where(x => x.Mesh && !x.Recolor && x.Game == SimsGames.Sims2 && !x.GameMod && !x.Type.Contains("Collection") && !x.Type.Contains("Slider") && !x.Type.Contains("Face Template") && x.Type != "Skin" && !x.Type.Contains("Eyecolor"))];
            List<SimsPackage> recolors = [..gameInstance._packages.Where(x => x.Recolor && !x.Mesh && x.Game == SimsGames.Sims2 && !x.GameMod && !x.Type.Contains("Collection") && !x.Type.Contains("Slider") && !x.Type.Contains("Face Template") && x.Type != "Skin" && !x.Type.Contains("Eyecolor"))];
            List<SimsPackage> both = [..gameInstance._packages.Where(x => x.Recolor && x.Mesh && x.Game == SimsGames.Sims2 && !x.GameMod && !x.Type.Contains("Collection") && !x.Type.Contains("Slider") && !x.Type.Contains("Face Template") && x.Type != "Skin" && !x.Type.Contains("Eyecolor"))];
            meshes.AddRange([..gameInstance._packages.Where(x => (x.Sims2Data.SHPEDataBlock.Count > 0 || x.Sims2Data.GMDCDataBlock.Count > 0) && !x.Recolor && x.Game == SimsGames.Sims2 && !x.GameMod && !x.Type.Contains("Collection") && !x.Type.Contains("Slider") && !x.Type.Contains("Face Template") && x.Type != "Skin" && !x.Type.Contains("Eyecolor"))]);
            meshes = meshes.Distinct().ToList();
            recolors.AddRange([..gameInstance._packages.Where(x => (x.Sims2Data.TXTRDataBlock.Count > 0 || x.Sims2Data.MMATDataBlock.Count > 0 || x.Sims2Data.XHTNDataBlock.Count > 0) && !x.Mesh && x.Game == SimsGames.Sims2 && !x.GameMod && !x.Type.Contains("Collection") && !x.Type.Contains("Slider") && !x.Type.Contains("Face Template") && x.Type != "Skin" && !x.Type.Contains("Eyecolor"))]);
            recolors = recolors.Distinct().ToList();

            both.AddRange([..gameInstance._packages.Where(x => (x.Sims2Data.TXTRDataBlock.Count > 0 || x.Sims2Data.MMATDataBlock.Count > 0 || x.Sims2Data.XHTNDataBlock.Count > 0) && x.Mesh && x.Game == SimsGames.Sims2)]);
            both.AddRange([..gameInstance._packages.Where(x => (x.Sims2Data.SHPEDataBlock.Count > 0 || x.Sims2Data.GMDCDataBlock.Count > 0) && x.Recolor && x.Game == SimsGames.Sims2)]);
            both = both.Distinct().ToList();

            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Finding orphans for {0} meshes, {1} recolors and {2} who have both", meshes.Count, recolors.Count, both.Count));

            List<SimsPackage> orphans = [..meshes.Where(x => !recolors.Any(r => r.ObjectGUID == x.ObjectGUID) && !both.Any(b => b.ObjectGUID == x.ObjectGUID && !string.IsNullOrEmpty(b.ObjectGUID) && !string.IsNullOrEmpty(x.ObjectGUID)))];
            orphans.AddRange([..recolors.Where(x => !meshes.Any(r => r.ObjectGUID == x.ObjectGUID) && !both.Any(b => b.ObjectGUID == x.ObjectGUID && !string.IsNullOrEmpty(b.ObjectGUID) && !string.IsNullOrEmpty(x.ObjectGUID)))]);
            

            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found {0} orphans", orphans.Count));

            int msv = orphans.Count + recolors.Count + meshes.Count;

            GlobalVariables.mainWindow.ChangeStage(gameInstance._packages.Count, "Finding orphans", 2);

            foreach (SimsPackage orphan in orphans)
            {   
                GlobalVariables.mainWindow.IncrementLoadingScreen(1, orphan.FileName.Replace(".package", ""), "Globals: Orphans: orphan");       
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking orphan file {0}", orphan.FileName));
                if (orphan.Sims2Data != null)
                {
                    orphan.Orphan = true;
                    if (orphan.Type != "Slider" || orphan.Type != "Face Template" || !orphan.GameMod)
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0} is Slider, FaceTemplate or Mod, and is not orphan", orphan.FileName));
                        gameInstance._packages.First(x => x.Identifier == orphan.Identifier).Orphan = true;
                    }

                    if (orphan.Type.Contains("Slider") || orphan.Type.Contains("Face Template") || orphan.Type.Contains("Collection") || orphan.Type.Contains("Hider") || orphan.GameMod )
                    {
                        gameInstance._packages.First(x => x.Identifier == orphan.Identifier).Orphan = false;
                        break;
                    }

                    if (!string.IsNullOrEmpty(orphan.ObjectGUID) && gameInstance._packages.Any(x => x.ObjectGUID == orphan.ObjectGUID))
                    {                        
                        if (orphan.Recolor)
                        {
                            if (gameInstance._packages.Any(x => x.ObjectGUID == orphan.ObjectGUID && x.Mesh))
                            {
                                SimsPackage mesh = gameInstance._packages.First(x => x.ObjectGUID == orphan.ObjectGUID && x.Mesh);
                                List<SimsPackage> rec = gameInstance._packages.Where(x => x.ObjectGUID == orphan.ObjectGUID && !x.Mesh).ToList();
                                foreach (SimsPackage package in rec)
                                {
                                    if (gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Type == "Unknown")
                                    {
                                        gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Sims2Data.AltType = package.Sims2Data.AltType;
                                        gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Sims2Data.FunctionSort = package.Sims2Data.FunctionSort;
                                    } else if (gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Type != "Unknown")
                                    {
                                        gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.AltType = orphan.Sims2Data.AltType;
                                        gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.FunctionSort = orphan.Sims2Data.FunctionSort;
                                    }
                                    gameInstance._packages.First(x => x.Identifier == package.Identifier).MatchingMesh = mesh.FileName;
                                    gameInstance._packages.First(x => x.Identifier == package.Identifier).Orphan = false;
                                    gameInstance._packages.First(x => x.Identifier == mesh.Identifier).MatchingRecolors.Add(package.FileName);
                                }
                            }                            
                        } else if (orphan.Mesh)
                        {
                            List<SimsPackage> rec = gameInstance._packages.Where(x => x.ObjectGUID == orphan.ObjectGUID && !x.Mesh).ToList();
                            foreach (SimsPackage package in rec)
                            {
                                if (gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Type == "Unknown")
                                {
                                    gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Sims2Data.AltType = package.Sims2Data.AltType;
                                    gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Sims2Data.FunctionSort = package.Sims2Data.FunctionSort;
                                } else if (gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Type != "Unknown")
                                {
                                    gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.AltType = orphan.Sims2Data.AltType;
                                    gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.FunctionSort = orphan.Sims2Data.FunctionSort;
                                }
                                gameInstance._packages.First(x => x.Identifier == package.Identifier).MatchingMesh = orphan.FileName;
                                gameInstance._packages.First(x => x.Identifier == package.Identifier).Orphan = false;
                                gameInstance._packages.First(x => x.Identifier == orphan.Identifier).MatchingRecolors.Add(package.FileName);
                            }                           
                        }
                            
                        
            
                    }
                    
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: Checking for MMAT", orphan.FileName));
                    if ((orphan.PackageData as Sims2Data).MMATDataBlock.Count != 0 && orphan.Orphan)
                    {                        
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: has {1} MMAT!", orphan.FileName, orphan.Sims2Data.MMATDataBlock.Count));
                        string TextureName = orphan.Sims2Data.MMATDataBlock[0].Name;
                        if (TextureName.Contains('!')) TextureName = TextureName.Split('!')[^1];
                        if (TextureName.Contains('_')) TextureName = TextureName.Split('_')[0];
                        if (TextureName.Contains('-')) TextureName = TextureName.Split('-')[0];
                        if (TextureName != "")
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: Searching for texture: {1}", orphan.FileName, TextureName));
                        
                            if(gameInstance._packages.Any(x => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName.Contains(TextureName, StringComparison.CurrentCultureIgnoreCase)))){
                                gameInstance._packages.First(x => x.Identifier == orphan.Identifier).Orphan = false;
                                gameInstance._packages.First(x => x.Identifier == orphan.Identifier).MatchingRecolors.Add(gameInstance._packages.First(x => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName.Contains(TextureName, StringComparison.CurrentCultureIgnoreCase))).FileName);
                                
                                
                                List<SimsPackage> matchingrecolors = gameInstance._packages.Where(x => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName.Contains(TextureName, StringComparison.CurrentCultureIgnoreCase))).ToList();
                                foreach (SimsPackage package in matchingrecolors)
                                {
                                    if (gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Type == "Unknown")
                                    {
                                        gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Sims2Data.AltType = package.Sims2Data.AltType;
                                        gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Sims2Data.FunctionSort = package.Sims2Data.FunctionSort;
                                    } else if (gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Type != "Unknown")
                                    {
                                        gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.AltType = orphan.Sims2Data.AltType;
                                        gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.FunctionSort = orphan.Sims2Data.FunctionSort;
                                    }
                                    gameInstance._packages.First(p => p.Identifier == package.Identifier).MatchingMesh = orphan.FileName;
                                    gameInstance._packages.First(p => p.Identifier == package.Identifier).Orphan = false;
                                }
                            }
                        } else
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: Texture name was blank: {1}", orphan.FileName, orphan.Sims2Data.MMATDataBlock[0].Name));
                        }
                                                
                    } 
                    
                    if ((orphan.PackageData as Sims2Data).XNGBDataBlock != null && orphan.Orphan)
                    {                        
                        if (orphan.Sims2Data.XNGBDataBlock.Count > 0){
                            string TextureName = orphan.Sims2Data.XNGBDataBlock[0].ModelName;
                            if (TextureName.Contains('!')) TextureName = TextureName.Split('!')[^1];
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh package has no textures. Searching for {0}", TextureName));

                            if(gameInstance._packages.Any(x => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName.Contains(TextureName, StringComparison.CurrentCultureIgnoreCase)))){
                                gameInstance._packages.First(x => x.Identifier == orphan.Identifier).Orphan = false;
                                gameInstance._packages.First(x => x.Identifier == orphan.Identifier).MatchingRecolors.Add(gameInstance._packages.First(x => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName.Contains(TextureName, StringComparison.CurrentCultureIgnoreCase))).FileName);
                                gameInstance._packages.First(x => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName.Contains(TextureName, StringComparison.CurrentCultureIgnoreCase))).MatchingMesh = orphan.FileName;
                            }
                        }                        
                    } 
                    
                    if (orphan.Sims2Data.EntryCount("txtr") > 0 && (orphan.Sims2Data.EntryCount("gmdc") > 0 || orphan.Sims2Data.EntryCount("shpe") > 0 ))
                    {
                        gameInstance._packages.First(x => x.Identifier == orphan.Identifier).Orphan = false;
                    } 
                    
                    if (orphan.Sims2Data.SHPEDataBlock.Count != 0 && orphan.Orphan)
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("File {0} has a SHPE", orphan.FileName));
                        
                        if (gameInstance._packages.Any(x => x.Sims2Data.EIDRDataBlock.Any(e => e.ResourceKeys.Any(r => orphan.Sims2Data.SHPEDataBlock.Any(s => s.FullKey == r.FullKey)))))
                        {
                            List<SimsPackage> texturePackages = [..gameInstance._packages.Where(x => x.Sims2Data.EIDRDataBlock.Any(e => e.ResourceKeys.Any(r => orphan.Sims2Data.SHPEDataBlock.Any(s => s.FullKey == r.FullKey))))];
                            foreach (SimsPackage package in texturePackages)
                            {
                                if (gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Type == "Unknown")
                                {
                                    gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Sims2Data.AltType = package.Sims2Data.AltType;
                                    gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Sims2Data.FunctionSort = package.Sims2Data.FunctionSort;
                                } else if (gameInstance._packages.First(p => p.Identifier == orphan.Identifier).Type != "Unknown")
                                {
                                    gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.AltType = orphan.Sims2Data.AltType;
                                    gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.FunctionSort = orphan.Sims2Data.FunctionSort;
                                }
                                gameInstance._packages.First(p => p.Identifier == package.Identifier).MatchingMesh = orphan.FileName;
                                gameInstance._packages.First(p => p.Identifier == package.Identifier).Orphan = false;
                            }
                            gameInstance._packages.First(x => x.Identifier == orphan.Identifier).Orphan = false;
                            gameInstance._packages.First(x => x.Identifier == orphan.Identifier).MatchingRecolors.AddRange(texturePackages.Select(x => x.FileName));
                        }
                    }
                }                                
            }

            foreach (SimsPackage recolor in recolors)
            {      
                GlobalVariables.mainWindow.IncrementLoadingScreen(1, recolor.FileName.Replace(".package", ""), "Globals: Orphans: recolor");       
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking recolor file {0}", recolor.FileName));
                if (recolor.Sims2Data != null)
                {
                    // find non-guid recolors - hairs for example
                
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking recolor GUID {0} for a match.", recolor.ObjectGUID));
                    if (!string.IsNullOrEmpty(recolor.ObjectGUID) && meshes.Any(x => x.ObjectGUID == recolor.ObjectGUID))
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found a matching guid for {0} in meshes!", recolor.ObjectGUID));
                        
                        if(gameInstance._packages.Any(x => x.ObjectGUID == recolor.ObjectGUID && x.Mesh)){
                            SimsPackage mesh = gameInstance._packages.First(x => x.ObjectGUID == recolor.ObjectGUID && x.Mesh);
                            List<SimsPackage> rec = gameInstance._packages.Where(x => x.ObjectGUID == recolor.ObjectGUID && !x.Mesh).ToList();
                            foreach (SimsPackage package in rec)
                            {
                                if (gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Type == "Unknown")
                                {
                                    gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Sims2Data.AltType = package.Sims2Data.AltType;
                                    gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Sims2Data.FunctionSort = package.Sims2Data.FunctionSort;
                                } else if (gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Type != "Unknown")
                                {
                                    gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.AltType = mesh.Sims2Data.AltType;
                                    gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.FunctionSort = mesh.Sims2Data.FunctionSort;
                                }
                                gameInstance._packages.First(x => x.Identifier == package.Identifier).MatchingMesh = mesh.FileName;
                                gameInstance._packages.First(x => x.Identifier == package.Identifier).Orphan = false;
                                gameInstance._packages.First(x => x.Identifier == mesh.Identifier).MatchingRecolors.Add(package.FileName);
                            }
                        }
                            
                            
                            
                                
                    } else if (!string.IsNullOrEmpty(recolor.ObjectGUID) && both.Any(x => x.ObjectGUID == recolor.ObjectGUID))
                    {
                        if (gameInstance._packages.Any(x => x.ObjectGUID == recolor.ObjectGUID && x.Mesh)){ 
                            SimsPackage mesh = gameInstance._packages.First(x => x.ObjectGUID == recolor.ObjectGUID && x.Mesh);
                            List<SimsPackage> rec = gameInstance._packages.Where(x => x.ObjectGUID == recolor.ObjectGUID && !x.Mesh).ToList();
                            foreach (SimsPackage package in rec)
                            {
                                if (gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Type == "Unknown")
                                {
                                    gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Sims2Data.AltType = package.Sims2Data.AltType;
                                    gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Sims2Data.FunctionSort = package.Sims2Data.FunctionSort;
                                } else if (gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Type != "Unknown")
                                {
                                    gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.AltType = mesh.Sims2Data.AltType;
                                    gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.FunctionSort = mesh.Sims2Data.FunctionSort;
                                }
                                gameInstance._packages.First(x => x.Identifier == package.Identifier).MatchingMesh = mesh.FileName;
                                gameInstance._packages.First(x => x.Identifier == package.Identifier).Orphan = false;
                                gameInstance._packages.First(x => x.Identifier == mesh.Identifier).MatchingRecolors.Add(package.FileName);
                            }
                        }   
                    } else if (recolor.Sims2Data.EIDRDataBlock.Count > 0)
                    {
                        if (gameInstance._packages.Any(x => x.Sims2Data.IndexEntries.Any(i => recolor.Sims2Data.EIDRDataBlock.Any(e => e.ResourceKeys.Any(r => r.FullKey == i.CompleteID && x.Mesh)))))
                        {
                            if (gameInstance._packages.Any(x => x.Sims2Data.IndexEntries.Any(i => recolor.Sims2Data.EIDRDataBlock.Any(e => e.ResourceKeys.Any(r => r.FullKey == i.CompleteID && x.Mesh)))))
                            {
                                SimsPackage mesh = gameInstance._packages.First(x => x.Sims2Data.IndexEntries.Any(i => recolor.Sims2Data.EIDRDataBlock.Any(e => e.ResourceKeys.Any(r => r.FullKey == i.CompleteID && x.Mesh))));
                                if (mesh.Sims2Data.SHPEDataBlock.Count > 0)
                                {
                                    List<SimsPackage> matches = gameInstance._packages.Where(x => x.Sims2Data.EIDRDataBlock.Any(e => e.ResourceKeys.Any(r => mesh.Sims2Data.IndexEntries.Any(i => i.CompleteID == r.FullKey)))).ToList();
                                    foreach (SimsPackage package in matches)
                                    {
                                        if (gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Type == "Unknown")
                                        {
                                            gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Sims2Data.AltType = package.Sims2Data.AltType;
                                            gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Sims2Data.FunctionSort = package.Sims2Data.FunctionSort;
                                        } else if (gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Type != "Unknown")
                                        {
                                            gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.AltType = mesh.Sims2Data.AltType;
                                            gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.FunctionSort = mesh.Sims2Data.FunctionSort;
                                        }
                                        gameInstance._packages.First(p => p.Identifier == package.Identifier).MatchingMesh = mesh.FileName;
                                        gameInstance._packages.First(p => p.Identifier == package.Identifier).Orphan = false;
                                    }
                                    gameInstance._packages.First(x => x.Identifier == mesh.Identifier).Orphan = false;
                                    gameInstance._packages.First(x => x.Identifier == mesh.Identifier).MatchingRecolors.AddRange(matches.Select(x => x.FileName));
                                }
                            }
                            
                        }
                    }
                }                
            }

            foreach (SimsPackage mesh in meshes)
            {  
                GlobalVariables.mainWindow.IncrementLoadingScreen(1, mesh.FileName.Replace(".package", ""), "Globals: Orphans: meshes");       
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking mesh file {0}", mesh.FileName));
                if (mesh.Sims2Data != null)
                {
                    if (mesh.Sims2Data.SHPEDataBlock.Count > 0)
                    {
                        if (gameInstance._packages.Any(x => x.Sims2Data.EIDRDataBlock.Any(e => e.ResourceKeys.Any(r => mesh.Sims2Data.IndexEntries.Any(i => i.CompleteID == r.FullKey))))){
                            List<SimsPackage> matches = gameInstance._packages.Where(x => x.Sims2Data.EIDRDataBlock.Any(e => e.ResourceKeys.Any(r => mesh.Sims2Data.IndexEntries.Any(i => i.CompleteID == r.FullKey)))).ToList();
                            foreach (SimsPackage package in matches)
                            {
                                if (gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Type == "Unknown")
                                {
                                    gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Sims2Data.AltType = package.Sims2Data.AltType;
                                    gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Sims2Data.FunctionSort = package.Sims2Data.FunctionSort;
                                } else if (gameInstance._packages.First(p => p.Identifier == mesh.Identifier).Type != "Unknown")
                                {
                                    gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.AltType = mesh.Sims2Data.AltType;
                                    gameInstance._packages.First(p => p.Identifier == package.Identifier).Sims2Data.FunctionSort = mesh.Sims2Data.FunctionSort;
                                }
                                gameInstance._packages.First(p => p.Identifier == package.Identifier).MatchingMesh = mesh.FileName;
                                gameInstance._packages.First(p => p.Identifier == package.Identifier).Orphan = false; 
                            }
                            
                            gameInstance._packages.First(x => x.Identifier == mesh.Identifier).Orphan = false;
                            gameInstance._packages.First(x => x.Identifier == mesh.Identifier).MatchingRecolors.AddRange(matches.Select(x => x.FileName));
                        }
                    }
                }                            
            }

            foreach (SimsPackage package in gameInstance._packages){
                gameInstance._packages.First(x=>x.Identifier == package.Identifier).MatchingRecolors = gameInstance._packages.First(x=>x.Identifier == package.Identifier).MatchingRecolors.Distinct().ToList();
                gameInstance._packages.First(x=>x.Identifier == package.Identifier).WriteXML();
            }
            return gameInstance;
        }






        private static SimsPackage GetLinked(GameInstance gameInstance, SimsPackage package)
        {
            if (package.LinkedFiles.Count != 0)
            {
                foreach (string file in package.LinkedFiles)
                {
                    if (!gameInstance.Files.OfType<SimsPackage>().Any(x => x.Location == file))
                    {
                        FileInfo fi = new(file);
                        SimsPackage subpackage = new();
                        string infoFile = string.Format("{0}.info", file);
                        if (File.Exists(infoFile))
                        {
                            XmlSerializer simsPackageSerializer = new XmlSerializer(typeof(SimsPackage));
                            using (FileStream fileStream = new(infoFile, FileMode.Open, System.IO.FileAccess.Read)){
                                using (StreamReader streamReader = new(fileStream)){
                                    subpackage = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                                    streamReader.Close();
                                }
                                fileStream.Close();
                            }
                        } else
                        {
                            subpackage.IsDirectory = false;
                            subpackage.StandAlone = false; 
                            subpackage.FileName = fi.Name;
                            subpackage.Game = gameInstance.GameChoice;
                            switch (subpackage.Game)
                            {
                                case SimsGames.Sims2:
                                subpackage.Sims2Data = new();
                                break;
                                case SimsGames.Sims3:
                                subpackage.Sims3Data = new();
                                break;
                                case SimsGames.Sims4:
                                subpackage.Sims4Data = new();
                                break;
                            }
                            subpackage.Location = file;
                            if (File.Exists(subpackage.Location))
                            {
                                SimsPackageReader simsPackageReader = new();
                                try { simsPackageReader.ReadPackage(subpackage.Location); } catch (Exception e)
                                {
                                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Couldn't read package {0}: {1} ({2})", subpackage.FileName, e.Message, e.StackTrace));
                                }
                                subpackage.PackageData = simsPackageReader.SimsData;
                            }
                            subpackage.DateCreated = File.GetCreationTime(file);
                            subpackage.DateUpdated = DateTime.Now;
                            subpackage.PackageCategory = gameInstance.Categories.First(x => x.Name == "Default");
                            subpackage.WriteXML();
                        }
                        if (!package.LinkedPackages.Contains(subpackage) && !subpackage.StandAlone) package.LinkedPackages.Add(subpackage);                        
                    } else
                    {
                        SimsPackage subpackage = gameInstance._packages.First(x => x.Location == file);
                        if (!package.LinkedPackages.Contains(subpackage) && !subpackage.StandAlone) package.LinkedPackages.Add(subpackage);
                    }                   
                }  
            }
            if (package.LinkedFolders.Count != 0)
            {
                foreach (string file in package.LinkedFolders)
                {
                    if (!gameInstance._packages.Any(x => x.Location == file))
                    {
                        DirectoryInfo fi = new(file);
                        SimsPackage subpackage = new();
                        string infoFile = string.Format("{0}.info", file);
                        if (File.Exists(infoFile))
                        {
                            XmlSerializer simsPackageSerializer = new XmlSerializer(typeof(SimsPackage));
                            using (FileStream fileStream = new(infoFile, FileMode.Open, System.IO.FileAccess.Read)){
                                using (StreamReader streamReader = new(fileStream)){
                                    subpackage = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                                    streamReader.Close();
                                }
                                fileStream.Close();
                            }
                        } else
                        {
                            subpackage.IsDirectory = true;
                            subpackage.StandAlone = false; 
                            subpackage.FileName = fi.Name;
                            subpackage.Game = gameInstance.GameChoice;
                            switch (subpackage.Game)
                            {
                                case SimsGames.Sims2:
                                subpackage.Sims2Data = new();
                                break;
                                case SimsGames.Sims3:
                                subpackage.Sims3Data = new();
                                break;
                                case SimsGames.Sims4:
                                subpackage.Sims4Data = new();
                                break;
                            }
                            subpackage.Location = file;
                            subpackage.DateCreated = File.GetCreationTime(file);
                            subpackage.DateUpdated = DateTime.Now;
                            subpackage.PackageCategory = gameInstance.Categories.First(x => x.Name == "Default");
                            subpackage.WriteXML();
                        }
                        if (!package.LinkedPackageFolders.Contains(subpackage) && !subpackage.StandAlone) package.LinkedPackageFolders.Add(subpackage);
                    } else
                    {
                        SimsPackage subpackage = gameInstance._packages.First(x => x.Location == file);
                        if (!package.LinkedPackageFolders.Contains(subpackage) && !subpackage.StandAlone) package.LinkedPackageFolders.Add(subpackage);
                    }                   
                }  
            }

            return package;
        }

        /*private static GameInstance GetLinked(GameInstance gameInstance, SimsPackage package)
        {
            if (package.LinkedFiles.Count != 0)
            {
                foreach (string file in package.LinkedFiles)
                {
                    if (!gameInstance.Files.OfType<SimsPackage>().Any(x => x.Location == file))
                    {
                        FileInfo fi = new(file);
                        SimsPackage subpackage = new();
                        string infoFile = string.Format("{0}.info", file);
                        if (File.Exists(infoFile))
                        {
                            XmlSerializer simsPackageSerializer = new XmlSerializer(typeof(SimsPackage));
                            using (FileStream fileStream = new(infoFile, FileMode.Open, System.IO.FileAccess.Read)){
                                using (StreamReader streamReader = new(fileStream)){
                                    subpackage = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                                    streamReader.Close();
                                }
                                fileStream.Close();
                            }
                        } else
                        {
                            subpackage.IsDirectory = false;
                            subpackage.StandAlone = false; 
                            subpackage.FileName = fi.Name;
                            subpackage.Game = gameInstance.GameChoice;
                            switch (subpackage.Game)
                            {
                                case SimsGames.Sims2:
                                subpackage.Sims2Data = new();
                                break;
                                case SimsGames.Sims3:
                                subpackage.Sims3Data = new();
                                break;
                                case SimsGames.Sims4:
                                subpackage.Sims4Data = new();
                                break;
                            }
                            subpackage.Location = file;
                            if (File.Exists(subpackage.Location))
                            {
                                SimsPackageReader simsPackageReader = new();
                                try { simsPackageReader.ReadPackage(subpackage.Location); } catch (Exception e)
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Couldn't read package {0}: {1} ({2})", subpackage.FileName, e.Message, e.StackTrace));
                }
                                subpackage.PackageData = simsPackageReader.SimsData;
                            }
                            subpackage.DateCreated = File.GetCreationTime(file);
                            subpackage.DateUpdated = DateTime.Now;
                            subpackage.PackageCategory = gameInstance.Categories.First(x => x.Name == "Default");
                            subpackage.WriteXML();
                        }
                        if (!package.LinkedPackages.Contains(subpackage) && !subpackage.StandAlone) package.LinkedPackages.Add(subpackage);
                        
                    } else
                    {
                        SimsPackage subpackage = gameInstance._packages.First(x => x.Location == file);
                        if (!package.LinkedPackages.Contains(subpackage) && !subpackage.StandAlone) package.LinkedPackages.Add(subpackage);
                    }                   
                }  
            }
            if (package.LinkedFolders.Count != 0)
            {
                foreach (string file in package.LinkedFolders)
                {
                    if (!gameInstance._packages.Any(x => x.Location == file))
                    {
                        DirectoryInfo fi = new(file);
                        SimsPackage subpackage = new();
                        string infoFile = string.Format("{0}.info", file);
                        if (File.Exists(infoFile))
                        {
                            XmlSerializer simsPackageSerializer = new XmlSerializer(typeof(SimsPackage));
                            using (FileStream fileStream = new(infoFile, FileMode.Open, System.IO.FileAccess.Read)){
                                using (StreamReader streamReader = new(fileStream)){
                                    subpackage = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                                    streamReader.Close();
                                }
                                fileStream.Close();
                            }
                        } else
                        {
                            subpackage.IsDirectory = true;
                            subpackage.StandAlone = false; 
                            subpackage.FileName = fi.Name;
                            subpackage.Game = gameInstance.GameChoice;
                            switch (subpackage.Game)
                            {
                                case SimsGames.Sims2:
                                subpackage.Sims2Data = new();
                                break;
                                case SimsGames.Sims3:
                                subpackage.Sims3Data = new();
                                break;
                                case SimsGames.Sims4:
                                subpackage.Sims4Data = new();
                                break;
                            }
                            subpackage.Location = file;
                            subpackage.DateCreated = File.GetCreationTime(file);
                            subpackage.DateUpdated = DateTime.Now;
                            subpackage.PackageCategory = gameInstance.Categories.First(x => x.Name == "Default");
                            subpackage.WriteXML();
                        }
                        if (!package.LinkedPackageFolders.Contains(subpackage) && !subpackage.StandAlone) package.LinkedPackageFolders.Add(subpackage);
                    } else
                    {
                        SimsPackage subpackage = gameInstance._packages.First(x => x.Location == file);
                        if (!package.LinkedPackageFolders.Contains(subpackage) && !subpackage.StandAlone) package.LinkedPackageFolders.Add(subpackage);
                    }                   
                }  
            }
            return gameInstance;
        }

        /*public static GameInstance GetSubDirectories(GameInstance gameInstance, string directory, SimsPackage folderPackage)
        {
            foreach (string file in Directory.GetFiles(directory))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                FileInfo fi = new(file);
                if (GlobalVariables.SimsFileExtensions.Contains(fi.Extension))
                {
                    SimsPackage simsPackage = ReadPackage(file, gameInstance, fi, true);                     
                    gameInstance._packages.Add(simsPackage); 
                    if (simsPackage.LinkedFiles.Count != 0 || simsPackage.LinkedFolders.Count != 0)
                    {
                        gameInstance = GetLinked(gameInstance, simsPackage);
                    }
                    if (!simsPackage.StandAlone)
                    {
                        if (!folderPackage.LinkedFiles.Contains(simsPackage.Location)) folderPackage.LinkedFiles.Add(simsPackage.Location);
                        folderPackage.LinkedPackages.Add(simsPackage);
                    }
                    
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));
                    int incBy = 1;
                    if (simsPackage.IsDirectory)
                    {
                        if (simsPackage.LinkedFiles.Count > 0) incBy += simsPackage.LinkedFiles.Count;
                        if (simsPackage.LinkedFolders.Count > 0) incBy += simsPackage.LinkedFolders.Count;
                    }
                    GlobalVariables.mainWindow.IncrementLoadingScreen(incBy, fi.Name.Replace(".info", ""), "Globals: ReadPackages 5");
                } else
                {                    
                    GlobalVariables.mainWindow.IncrementLoadingScreen(1, fi.Name.Replace(".info", ""), "Globals: ReadPackages 6");
                }               
                
            }
            foreach (string file in Directory.GetDirectories(directory))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                DirectoryInfo fi = new(file);
                SimsPackage simsPackage = ReadPackage(file, gameInstance, fi, true);                
                if (simsPackage.LinkedFiles.Count != 0 || simsPackage.LinkedFolders.Count != 0)
                {
                    gameInstance = GetLinked(gameInstance, simsPackage);
                }
                if (!simsPackage.StandAlone)
                {
                    if (!folderPackage.LinkedFolders.Contains(simsPackage.Location)) folderPackage.LinkedFolders.Add(simsPackage.Location);
                    folderPackage.LinkedPackageFolders.Add(simsPackage);
                }
                
                gameInstance._packages.Add(simsPackage);
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));            
                int incBy = 1;
                if (simsPackage.IsDirectory)
                {
                    if (simsPackage.LinkedFiles.Count > 0) incBy += simsPackage.LinkedFiles.Count;
                    if (simsPackage.LinkedFolders.Count > 0) incBy += simsPackage.LinkedFolders.Count;
                }
                GlobalVariables.mainWindow.IncrementLoadingScreen(incBy, fi.Name.Replace(".info", ""), "Globals: ReadPackages 7");
                if (!simsPackage.RootMod && simsPackage.StandAlone) gameInstance = GetSubDirectories(gameInstance, file, simsPackage);
            }
            return gameInstance;
        }

        public static SimsPackage GetSubDirectoriesPackage(GameInstance gameInstance, string directory, SimsPackage folderPackage)
        {
            foreach (string file in Directory.GetFiles(directory))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                FileInfo fi = new(file);
                if (GlobalVariables.SimsFileExtensions.Contains(fi.Extension))
                {
                    SimsPackage simsPackage = ReadPackage(file, gameInstance, fi, true);                     
                    gameInstance._packages.Add(simsPackage); 
                    if (simsPackage.LinkedFiles.Count != 0 || simsPackage.LinkedFolders.Count != 0)
                    {
                        gameInstance = GetLinked(gameInstance, simsPackage);
                    }
                    if (!simsPackage.StandAlone)
                    {
                        if (!folderPackage.LinkedFiles.Contains(simsPackage.Location)) folderPackage.LinkedFiles.Add(simsPackage.Location);
                        folderPackage.LinkedPackages.Add(simsPackage);
                    }
                    
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));
                }
                //GlobalVariables.mainWindow.IncrementLoadingScreen(1, fi.Name);
            }
            foreach (string file in Directory.GetDirectories(directory))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                DirectoryInfo fi = new(file);
                SimsPackage simsPackage = ReadPackage(file, gameInstance, fi, true);                
                if (simsPackage.LinkedFiles.Count != 0 || simsPackage.LinkedFolders.Count != 0)
                {
                    gameInstance = GetLinked(gameInstance, simsPackage);
                }
                if (!simsPackage.StandAlone)
                {
                    if (!folderPackage.LinkedFolders.Contains(simsPackage.Location)) folderPackage.LinkedFolders.Add(simsPackage.Location);
                    folderPackage.LinkedPackageFolders.Add(simsPackage);
                }
                
                gameInstance._packages.Add(simsPackage);
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));            
                //GlobalVariables.mainWindow.IncrementLoadingScreen(1, fi.Name);
                if (!simsPackage.RootMod && simsPackage.StandAlone) gameInstance = GetSubDirectories(gameInstance, file, simsPackage);
            }
            return folderPackage;
        }*/

        public static SimsPackage GetSubDirectories(GameInstance gameInstance, string directory, SimsPackage folderPackage)
        {
            
            foreach (string file in Directory.GetFiles(directory))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                FileInfo fi = new(file);
                if (GlobalVariables.SimsFileExtensions.Contains(fi.Extension))
                {
                    SimsPackage simsPackage = new();
                    simsPackage = ReadPackage(simsPackage, file, gameInstance, fi, true);                     
                    //gameInstance._packages.Add(simsPackage); 
                    
                    if (simsPackage.LinkedFiles.Count != 0 || simsPackage.LinkedFolders.Count != 0)
                    {
                        simsPackage = GetLinked(gameInstance, simsPackage);
                    }
                    if (!simsPackage.StandAlone)
                    {
                        if (!folderPackage.LinkedFiles.Contains(simsPackage.Location)) folderPackage.LinkedFiles.Add(simsPackage.Location);
                        folderPackage.LinkedPackages.Add(simsPackage);
                    }

                    //output.Add(simsPackage);                    
                }                
            }
            foreach (string file in Directory.GetDirectories(directory))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                DirectoryInfo fi = new(file);
                SimsPackage simsPackage = new();
                simsPackage = ReadPackage(simsPackage, file, gameInstance, fi, true);                
                if (simsPackage.LinkedFiles.Count != 0 || simsPackage.LinkedFolders.Count != 0)
                {
                    simsPackage = GetLinked(gameInstance, simsPackage);
                }
                if (!simsPackage.StandAlone)
                {
                    if (!folderPackage.LinkedFolders.Contains(simsPackage.Location)) folderPackage.LinkedFolders.Add(simsPackage.Location);
                    folderPackage.LinkedPackageFolders.Add(simsPackage);
                }
                
                
                if (!simsPackage.RootMod && simsPackage.StandAlone) simsPackage = GetSubDirectories(gameInstance, file, simsPackage);
                //output.Add(simsPackage);
            }
            return folderPackage;
        }

        public static SimsPackage GetSubDirectoriesPackage(GameInstance gameInstance, string directory, SimsPackage folderPackage)
        {
            foreach (string file in Directory.GetFiles(directory))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                FileInfo fi = new(file);
                if (GlobalVariables.SimsFileExtensions.Contains(fi.Extension))
                {
                    SimsPackage simsPackage = new();
                    simsPackage = ReadPackage(simsPackage, file, gameInstance, fi, true);                     
                    //gameInstance._packages.Add(simsPackage); 
                    if (simsPackage.LinkedFiles.Count != 0 || simsPackage.LinkedFolders.Count != 0)
                    {
                        simsPackage = GetLinked(gameInstance, simsPackage);
                    }
                    if (!simsPackage.StandAlone)
                    {
                        if (!folderPackage.LinkedFiles.Contains(simsPackage.Location)) folderPackage.LinkedFiles.Add(simsPackage.Location);
                        folderPackage.LinkedPackages.Add(simsPackage);
                    }
                    
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));
                }
                //GlobalVariables.mainWindow.IncrementLoadingScreen(1, fi.Name);
            }
            foreach (string file in Directory.GetDirectories(directory))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                DirectoryInfo fi = new(file);
                SimsPackage simsPackage = new(); 
                simsPackage = ReadPackage(simsPackage, file, gameInstance, fi, true);                
                if (simsPackage.LinkedFiles.Count != 0 || simsPackage.LinkedFolders.Count != 0)
                {
                    simsPackage = GetLinked(gameInstance, simsPackage);
                }
                if (!simsPackage.StandAlone)
                {
                    if (!folderPackage.LinkedFolders.Contains(simsPackage.Location)) folderPackage.LinkedFolders.Add(simsPackage.Location);
                    folderPackage.LinkedPackageFolders.Add(simsPackage);
                }
                
                gameInstance._packages.Add(simsPackage);
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));            
                //GlobalVariables.mainWindow.IncrementLoadingScreen(1, fi.Name);
                if (!simsPackage.RootMod && simsPackage.StandAlone) simsPackage = GetSubDirectories(gameInstance, file, simsPackage);
            }
            return folderPackage;
        }


        public static SimsPackage ReadPackage(SimsPackage simsPackage, string file, GameInstance loadedinstance, DirectoryInfo fi, bool subfolder = false)
        {
            string infoFile = string.Format("{0}.info", file);
            XmlSerializer simsPackageSerializer = new XmlSerializer(typeof(SimsPackage));
            if (File.Exists(infoFile))
            {
                if (GlobalVariables.DebugMode)
                {                    
                    using (FileStream fileStream = new(infoFile, FileMode.Open, System.IO.FileAccess.Read)){
                        using (StreamReader streamReader = new(fileStream)){
                            simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                            streamReader.Close();
                        }
                        fileStream.Close();
                    }
                } else
                {
                    using (FileStream fs = File.OpenRead(infoFile))
                    using (ZipFile zipFile = new ZipFile(fs))
                    {
                        if (!Utilities.CheckSignature(infoFile, 4, Utilities.SignatureZip)){
                            using (FileStream fileStream = new(infoFile, FileMode.Open, System.IO.FileAccess.Read)){
                                using (StreamReader streamReader = new(fileStream)){
                                    simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                                    streamReader.Close();
                                }
                                fileStream.Close();
                            }
                        } else
                        {
                            foreach (ZipEntry entry in zipFile)
                            {
                                if (!entry.IsFile) continue; // Skip directories
                                
                                using (Stream zipStream = zipFile.GetInputStream(entry))
                                using (MemoryStream outputStream = new())
                                {
                                    zipStream.CopyTo(outputStream);
                                    simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(outputStream);
                                }
                            }
                        }                        
                    }
                }                
            } else
            {
                simsPackage.IsDirectory = true;
                if (!subfolder) { 
                    simsPackage.StandAlone = true; 
                } else
                {
                    simsPackage.StandAlone = false;
                }
                simsPackage.FileName = fi.Name;
                simsPackage.Game = loadedinstance.GameChoice;
                switch (simsPackage.Game)
                {
                    case SimsGames.Sims2:
                    simsPackage.Sims2Data = new();
                    break;
                    case SimsGames.Sims3:
                    simsPackage.Sims3Data = new();
                    break;
                    case SimsGames.Sims4:
                    simsPackage.Sims4Data = new();
                    break;
                }
                simsPackage.Location = file;
                simsPackage.DateCreated = Directory.GetCreationTime(file);
                simsPackage.DateUpdated = DateTime.Now;
                simsPackage.PackageCategory = loadedinstance.Categories.First(x => x.Name == "Default");
                simsPackage.HasBeenRead = true;
                simsPackage.WriteXML();                
            }
            return simsPackage;
        }

        public static SimsPackage ReadPackage(SimsPackage simsPackage, string file, GameInstance loadedinstance, FileInfo fi, bool subfolder = false)
        {
            string infoFile = string.Format("{0}.info", file);
            XmlSerializer simsPackageSerializer = new XmlSerializer(typeof(SimsPackage));
            if (File.Exists(infoFile))
            {
                if (GlobalVariables.DebugMode)
                {                    
                    using (FileStream fileStream = new(infoFile, FileMode.Open, System.IO.FileAccess.Read)){
                        using (StreamReader streamReader = new(fileStream)){
                            simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                            streamReader.Close();
                        }
                        fileStream.Close();
                    }
                } else
                {
                    using (FileStream fs = File.OpenRead(infoFile))
                    using (ZipFile zipFile = new ZipFile(fs))
                    {
                        if (!Utilities.CheckSignature(infoFile, 4, Utilities.SignatureZip)){
                            using (FileStream fileStream = new(infoFile, FileMode.Open, System.IO.FileAccess.Read)){
                                using (StreamReader streamReader = new(fileStream)){
                                    simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                                    streamReader.Close();
                                }
                                fileStream.Close();
                            }
                        } else
                        {
                            foreach (ZipEntry entry in zipFile)
                            {
                                if (!entry.IsFile) continue; // Skip directories
                                
                                using (Stream zipStream = zipFile.GetInputStream(entry))
                                using (MemoryStream outputStream = new())
                                {
                                    zipStream.CopyTo(outputStream);
                                    simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(outputStream);
                                }
                            }
                        }                        
                    }
                }                
            } else
            {
                simsPackage.IsDirectory = false; 
                if (!subfolder) { 
                    simsPackage.StandAlone = true; 
                } else
                {
                    simsPackage.StandAlone = false;
                }
                simsPackage.FileName = fi.Name;
                simsPackage.Game = loadedinstance.GameChoice;
                switch (simsPackage.Game)
                {
                    case SimsGames.Sims2:
                    simsPackage.Sims2Data = new();
                    break;
                    case SimsGames.Sims3:
                    simsPackage.Sims3Data = new();
                    break;
                    case SimsGames.Sims4:
                    simsPackage.Sims4Data = new();
                    break;
                }
                simsPackage.Location = file;
                SimsPackageReader simsPackageReader = new();
                try { 
                    simsPackageReader.ReadPackage(simsPackage.Location); 
                    simsPackage.PackageData = simsPackageReader.SimsData;    
                    if (simsPackageReader.PackageGame != loadedinstance.GameChoice)
                    {
                        if (simsPackageReader.PackageGame == SimsGames.Sims3 && loadedinstance.GameChoice == SimsGames.SimsMedieval)
                        {
                            simsPackage.Game = SimsGames.SimsMedieval;                        
                            simsPackage.WrongGame = false;
                        } else
                        {
                            simsPackage.Game = simsPackageReader.PackageGame;
                            simsPackage.WrongGame = true;
                        }
                    }
                } catch (Exception e)
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Couldn't read package {0}: {1} ({2})", simsPackage.FileName, e.Message, e.StackTrace));
                }               
                simsPackage.PackageCategory = loadedinstance.Categories.First(x => x.Name == "Default");                
                simsPackageReader.Dispose();
                simsPackage.DateCreated = File.GetCreationTime(file);
                simsPackage.DateUpdated = DateTime.Now;
                simsPackage.HasBeenRead = true;
                simsPackage = simsPackageReader.CheckOverrides(simsPackage);                
                simsPackage.WriteXML();
                new Thread(() => {
                    //simsPackageReader.CheckDuplicates(simsPackage, loadedinstance._packages.ToList());
                }){IsBackground = true}.Start(); 
            }

            return simsPackage;
        }

        public static SimsDownload ReadDownload(string file, FileInfo f)
        {
            SimsDownload simsDownload = new();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));                
            string infoFile = string.Format("{0}.info", file);
            if (File.Exists(infoFile))
            {
                XmlSerializer simsDownloadSerializer = new XmlSerializer(typeof(SimsDownload));
                using (FileStream fileStream = new(infoFile, FileMode.Open, System.IO.FileAccess.Read)){
                    using (StreamReader streamReader = new(fileStream)){
                        simsDownload = (SimsDownload)simsDownloadSerializer.Deserialize(streamReader);
                        streamReader.Close();
                    }
                    fileStream.Close();
                }
            } else
            {   
                simsDownload.Location = file;
                simsDownload.FileName = f.Name;
                simsDownload.DateCreated = File.GetCreationTime(file);
                simsDownload.DateUpdated = DateTime.Now;
                simsDownload.WriteXML();
            } 
            return simsDownload;               
        }
    }
}