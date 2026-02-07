using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Godot;
using Microsoft.Win32;
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












        public static MainWindow mainWindow;

        public static SCCMSettings LoadedSettings;
        public static SCCMTheme LoadedTheme = Themes.DefaultThemes[0];


        public static List<string> SimsFileExtensions = new(){
            ".package",
            ".sims3pack",
            ".sims2pack",
            "ts4script"
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
                        Directory.Move(item.MovedTo, item.OriginalLocation);
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
                            Directory.Move(destinationPath, moveloc);
                            
                        }
                        Directory.Move(folderPath, destinationPath);
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
                            Directory.Move(destinationPath, moveloc);
                            
                        }
                        Directory.Move(folderPath, destinationPath);
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
                            Directory.Move(destinationPath, moveloc);
                            
                        }
                        Directory.Move(folderPath, destinationPath);
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
                            Directory.Move(destinationPath, moveloc);
                            
                        }
                        Directory.Move(folderPath, destinationPath);
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
                            Directory.Move(destinationPath, moveloc);
                            
                        }
                        Directory.Move(folderPath, destinationPath);
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
                            Directory.Move(destinationPath, moveloc);
                            
                        }
                        Directory.Move(folderPath, destinationPath);
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
                            Directory.Move(destinationPath, moveloc);
                            
                        }
                        Directory.Move(folderPath, destinationPath);
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
                            Directory.Move(destinationPath, moveloc);
                            
                        }
                        Directory.Move(folderPath, destinationPath);
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
                            Directory.Move(destinationPath, moveloc);
                            
                        }
                        Directory.Move(folderPath, destinationPath);
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
                            Directory.Move(destinationPath, moveloc);
                            
                        }
                        Directory.Move(folderPath, destinationPath);
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
                            Directory.Move(destinationPath, moveloc);
                            
                        }
                        Directory.Move(folderPath, destinationPath);
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
                            Directory.Move(destinationPath, moveloc);
                            
                        }
                        Directory.Move(folderPath, destinationPath);
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
                            Directory.Move(destinationPath, moveloc);
                            
                        }
                        Directory.Move(folderPath, destinationPath);
                    }
                }
            }
        }

        public static ConcurrentBag<Task> runningTasks = new();

        public static GameInstance LoadInstanceFiles(GameInstance gameInstance)
        {
            foreach (string file in Directory.GetFiles(gameInstance.InstanceFolders.InstancePackagesFolder))
            {
                Task t = Task.Run(() => {
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
            foreach (string file in Directory.GetDirectories(gameInstance.InstanceFolders.InstancePackagesFolder))
            {
                Task t = Task.Run(() => {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                    DirectoryInfo fi = new(file);
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
                });
                runningTasks.Add(t);
            }
            foreach (string file in Directory.GetFiles(gameInstance.InstanceFolders.InstanceDownloadsFolder))
            {
                Task t = Task.Run(() => {
                    FileInfo f = new(file);
                    SimsDownload simsDownload = ReadDownload(file, f);
                    gameInstance._downloads.Add(simsDownload);
                    GlobalVariables.mainWindow.IncrementLoadingScreen(1, f.Name.Replace(".info", ""), "Globals: ReadPackages 4");
                });
                runningTasks.Add(t);
            }   

            while (runningTasks.Any(x => !x.IsCompleted))
            {
                
            }

            gameInstance = FindOrphans(gameInstance);


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
            List<SimsPackage> meshes = [..gameInstance._packages.Where(x => x.Mesh && !x.Recolor && x.Game == SimsGames.Sims2)];
            List<SimsPackage> recolors = [..gameInstance._packages.Where(x => x.Recolor && !x.Mesh && x.Game == SimsGames.Sims2)];
            List<SimsPackage> both = [..gameInstance._packages.Where(x => x.Recolor && x.Mesh && x.Game == SimsGames.Sims2)];
            meshes.AddRange([..gameInstance._packages.Where(x => (x.Sims2Data.SHPEDataBlock.Count > 0 || x.Sims2Data.GMDCDataBlock.Count > 0) && !x.Recolor && x.Game == SimsGames.Sims2)]);
            meshes = meshes.Distinct().ToList();
            recolors.AddRange([..gameInstance._packages.Where(x => (x.Sims2Data.TXTRDataBlock.Count > 0 || x.Sims2Data.MMATDataBlock.Count > 0 || x.Sims2Data.XHTNDataBlock.Count > 0) && !x.Mesh && x.Game == SimsGames.Sims2)]);
            recolors = recolors.Distinct().ToList();

            both.AddRange([..gameInstance._packages.Where(x => (x.Sims2Data.TXTRDataBlock.Count > 0 || x.Sims2Data.MMATDataBlock.Count > 0 || x.Sims2Data.XHTNDataBlock.Count > 0) && x.Mesh && x.Game == SimsGames.Sims2)]);
            both.AddRange([..gameInstance._packages.Where(x => (x.Sims2Data.SHPEDataBlock.Count > 0 || x.Sims2Data.GMDCDataBlock.Count > 0) && x.Recolor && x.Game == SimsGames.Sims2)]);
            both = both.Distinct().ToList();

            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Finding orphans for {0} meshes, {1} recolors and {2} who have both", meshes.Count, recolors.Count, both.Count));

            List<SimsPackage> orphans = [..meshes.Where(x => !recolors.Any(r => r.ObjectGUID == x.ObjectGUID) && !both.Any(b => b.ObjectGUID == x.ObjectGUID && !string.IsNullOrEmpty(b.ObjectGUID) && !string.IsNullOrEmpty(x.ObjectGUID)))];
            orphans.AddRange([..recolors.Where(x => !meshes.Any(r => r.ObjectGUID == x.ObjectGUID) && !both.Any(b => b.ObjectGUID == x.ObjectGUID && !string.IsNullOrEmpty(b.ObjectGUID) && !string.IsNullOrEmpty(x.ObjectGUID)))]);
            

            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found {0} orphans", orphans.Count));

            foreach (SimsPackage orphan in orphans)
            {          
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking orphan file {0}", orphan.FileName));
                if (orphan.Sims2Data != null)
                {
                    orphan.Orphan = true;
                    if (orphan.Type != "Slider" || orphan.Type != "Face Template" || !orphan.GameMod)
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0} is Slider, FaceTemplate or Mod, and is not orphan", orphan.FileName));
                        gameInstance._packages.First(x => x.Identifier == orphan.Identifier).Orphan = true;
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
                                    //gameInstance._packages.First(x => x.Identifier == package.Identifier).WriteXML();
                                    gameInstance._packages.First(x => x.Identifier == mesh.Identifier).MatchingRecolors.Add(package.FileName);
                                }
                                //gameInstance._packages.First(x => x.Identifier == mesh.Identifier).WriteXML();
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
                                //gameInstance._packages.First(x => x.Identifier == package.Identifier).WriteXML();
                                gameInstance._packages.First(x => x.Identifier == orphan.Identifier).MatchingRecolors.Add(package.FileName);
                            }
                            //gameInstance._packages.First(x => x.Identifier == orphan.Identifier).WriteXML();                            
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
                                    //gameInstance._packages.First(p => p.Identifier == package.Identifier).WriteXML();
                                }
                            }
                        } else
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: Texture name was blank: {1}", orphan.FileName, orphan.Sims2Data.MMATDataBlock[0].Name));
                        }
                                                
                    } 
                    
                    if ((orphan.PackageData as Sims2Data).XNGBDataBlock != null && orphan.Orphan)
                    {                        
                        string TextureName = orphan.Sims2Data.XNGBDataBlock.ModelName;
                        if (TextureName.Contains('!')) TextureName = TextureName.Split('!')[^1];
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh package has no textures. Searching for {0}", TextureName));

                        if(gameInstance._packages.Any(x => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName.Contains(TextureName, StringComparison.CurrentCultureIgnoreCase)))){
                            gameInstance._packages.First(x => x.Identifier == orphan.Identifier).Orphan = false;
                            gameInstance._packages.First(x => x.Identifier == orphan.Identifier).MatchingRecolors.Add(gameInstance._packages.First(x => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName.Contains(TextureName, StringComparison.CurrentCultureIgnoreCase))).FileName);
                            gameInstance._packages.First(x => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName.Contains(TextureName, StringComparison.CurrentCultureIgnoreCase))).MatchingMesh = orphan.FileName;
                            //gameInstance._packages.First(x => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName.Contains(TextureName, StringComparison.CurrentCultureIgnoreCase))).WriteXML();
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
                                //gameInstance._packages.First(p => p.Identifier == package.Identifier).WriteXML();
                            }
                            gameInstance._packages.First(x => x.Identifier == orphan.Identifier).Orphan = false;
                            gameInstance._packages.First(x => x.Identifier == orphan.Identifier).MatchingRecolors.AddRange(texturePackages.Select(x => x.FileName));
                        }
                                            
                    }
                    //gameInstance._packages.First(x => x.Identifier == orphan.Identifier).MatchingRecolors = gameInstance._packages.First(x => x.Identifier == orphan.Identifier).MatchingRecolors.Distinct().ToList();
                    //gameInstance._packages.First(x => x.Identifier == orphan.Identifier).WriteXML(); 
                }                                
            }

            foreach (SimsPackage recolor in recolors)
            {      
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking recolor file {0}", recolor.FileName));
                if (recolor.Sims2Data != null)
                {
                    // find non-guid recolors - hairs for example
                
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking recolor GUID {0} for a match.", recolor.ObjectGUID));
                    if (!string.IsNullOrEmpty(recolor.ObjectGUID) && meshes.Any(x => x.ObjectGUID == recolor.ObjectGUID))
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found a matching guid for {0} in meshes!", recolor.ObjectGUID));
                        
                        SimsPackage mesh = gameInstance._packages.First(x => x.ObjectGUID == recolor.ObjectGUID);
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
                            //gameInstance._packages.First(x => x.Identifier == package.Identifier).WriteXML();
                            gameInstance._packages.First(x => x.Identifier == mesh.Identifier).MatchingRecolors.Add(package.FileName);
                        }
                        //gameInstance._packages.First(x => x.Identifier == mesh.Identifier).WriteXML();
                            
                            
                            
                                
                    } else if (!string.IsNullOrEmpty(recolor.ObjectGUID) && both.Any(x => x.ObjectGUID == recolor.ObjectGUID))
                    {
                        SimsPackage mesh = gameInstance._packages.First(x => x.ObjectGUID == recolor.ObjectGUID);
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
                        //gameInstance._packages.First(x => x.Identifier == mesh.Identifier).WriteXML();   
                    } else if (recolor.Sims2Data.EIDRDataBlock.Count > 0)
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
                                    //gameInstance._packages.First(p => p.Identifier == package.Identifier).WriteXML();
                                }
                                gameInstance._packages.First(x => x.Identifier == mesh.Identifier).Orphan = false;
                                gameInstance._packages.First(x => x.Identifier == mesh.Identifier).MatchingRecolors.AddRange(matches.Select(x => x.FileName));
                            }
                        }
                    }
                    //gameInstance._packages.First(x => x.Identifier == recolor.Identifier).WriteXML();
                }                
            }

            foreach (SimsPackage mesh in meshes)
            {  
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
                                //gameInstance._packages.First(p => p.Identifier == package.Identifier).WriteXML();
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






        private static GameInstance GetLinked(GameInstance gameInstance, SimsPackage package)
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
                            subpackage.DateAdded = DateTime.Now;
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
                            subpackage.DateAdded = DateTime.Now;
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

        public static GameInstance GetSubDirectories(GameInstance gameInstance, string directory, SimsPackage folderPackage)
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
        }


        public static SimsPackage ReadPackage(string file, GameInstance loadedinstance, DirectoryInfo fi, bool subfolder = false)
        {
            SimsPackage simsPackage = new();
            string infoFile = string.Format("{0}.info", file);
            if (File.Exists(infoFile))
            {
                XmlSerializer simsPackageSerializer = new XmlSerializer(typeof(SimsPackage));
                using (FileStream fileStream = new(infoFile, FileMode.Open, System.IO.FileAccess.Read)){
                    using (StreamReader streamReader = new(fileStream)){
                        simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                        streamReader.Close();
                    }
                    fileStream.Close();
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
                simsPackage.DateAdded = DateTime.Now;
                simsPackage.DateUpdated = DateTime.Now;
                simsPackage.PackageCategory = loadedinstance.Categories.First(x => x.Name == "Default");
                simsPackage.WriteXML();
            }
            return simsPackage;
        }

        public static SimsPackage ReadPackage(string file, GameInstance loadedinstance, FileInfo fi, bool subfolder = false)
        {
            SimsPackage simsPackage = new();
            string infoFile = string.Format("{0}.info", file);
            if (File.Exists(infoFile))
            {
                XmlSerializer simsPackageSerializer = new XmlSerializer(typeof(SimsPackage));
                using (FileStream fileStream = new(infoFile, FileMode.Open, System.IO.FileAccess.Read)){
                    using (StreamReader streamReader = new(fileStream)){
                        simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                        streamReader.Close();
                    }
                    fileStream.Close();
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
                try { simsPackageReader.ReadPackage(simsPackage.Location); } catch (Exception e)
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Couldn't read package {0}: {1} ({2})", simsPackage.FileName, e.Message, e.StackTrace));
                }
                simsPackage.PackageData = simsPackageReader.SimsData;    
                simsPackage.PackageCategory = loadedinstance.Categories.First(x => x.Name == "Default");            
                if (simsPackageReader.PackageGame != loadedinstance.GameChoice)
                {
                    simsPackage.Game = simsPackageReader.PackageGame;
                    simsPackage.WrongGame = true;
                }
                simsPackageReader.Dispose();
                simsPackage.DateAdded = DateTime.Now;
                simsPackage.DateUpdated = DateTime.Now;
                simsPackage.WriteXML();
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
                simsDownload.DateAdded = DateTime.Now;
                simsDownload.DateUpdated = DateTime.Now;
                simsDownload.WriteXML();
            } 
            return simsDownload;               
        }
    }
}