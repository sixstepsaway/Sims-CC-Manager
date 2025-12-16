using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Win32;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.OptionLists;
using SimsCCManager.SettingsSystem;

namespace SimsCCManager.Globals
{
    public class GlobalVariables
    {
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

        public static bool DebugMode = true;
        public static bool PortableMode = false;
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
            increment++;
            if (directory)
            {
                string testLocation = string.Format("{0} ({1})", inputLocation, increment);
                if (Directory.Exists(testLocation))
                {
                    inputLocation = IncrementName(inputLocation, directory, increment);
                } else
                {
                    inputLocation = testLocation;
                }
            } else
            {
                FileInfo fileInfo = new(inputLocation);
                string noextension = fileInfo.FullName.Replace(fileInfo.Extension, "");
                string testLocation = string.Format("{0} ({1}){2}", inputLocation, increment, fileInfo.Extension);
                if (Directory.Exists(testLocation))
                {
                    inputLocation = IncrementName(inputLocation, directory, increment);
                } else
                {
                    inputLocation = testLocation;
                }
            }
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

        /*public static Texture2D ExtractIcon(Executable exe, string datafolder){
            string exelocation = Path.Combine(exe.Path, exe.Exe);
            System.Drawing.Bitmap icon = (System.Drawing.Bitmap)null;
            try
            {
                icon = Icon.ExtractAssociatedIcon(exelocation).ToBitmap();
            }
            catch (System.Exception)
            {
                // swallow and return nothing. You could supply a default Icon here as well
                return new Texture2D();
            }
            string saveloc = ExeIconName(exe, datafolder);
            icon.Save(saveloc, ImageFormat.Png);
            Godot.Image image = Godot.Image.LoadFromFile(saveloc);
            return ImageTexture.CreateFromImage(image);
        }    */    

        /*public static string ExeIconName(Executable exe, string datafolder){
            string exeloc = Path.Combine(exe.Path, exe.Exe);
            FileInfo exeinf = new(exeloc);
            string exename = exeinf.Name.Replace(exeinf.Extension, "");
            string iconname = string.Format("{0}.png", exename);
            string exedir = Path.Combine(datafolder, "executables");
            if (!Directory.Exists(exedir)) Directory.CreateDirectory(exedir);
            return Path.Combine(exedir, iconname);
        }*/

        /*public static string RunNonSimsProcess(string process, string parameters)
        {
            string result = String.Empty;
            FileInfo exe = new(process);
            string exename = exe.Name;

            if (!File.Exists(process)){
                //Logging.WriteDebugLog("Process was not found.");
            } else {

                //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Process: {0}", process));
                //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Params: {0}", parameters));
                string testresult = string.Empty;
                //Console.WriteLine(parameters);

                using (Process p = new Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = process;
                    p.StartInfo.Arguments = parameters;
                    p.StartInfo.WorkingDirectory = new FileInfo(process).DirectoryName;
                    
                    
                    p.Start();
                    while (p.HasExited == false){
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(p.StandardOutput.Read().ToString());
                    }
                    p.WaitForExit();
                    result = p.StandardOutput.ReadToEnd();
                }
            }
            return result;
        }*/

        /*public static string RunProcess(string process, string parameters, Games game)
        {
            string result = String.Empty;
            FileInfo exe = new(process);
            string exename = exe.Name;

            if (!File.Exists(process)){
                //Logging.WriteDebugLog("Process was not found.");
            } else {

                //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Process: {0}", process));
                //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Params: {0}", parameters));
                string testresult = string.Empty;
                //Console.WriteLine(parameters);

                using (Process p = new Process())
                {
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.FileName = process;
                    p.StartInfo.Arguments = parameters;
                    p.StartInfo.WorkingDirectory = new FileInfo(process).DirectoryName;
                    
                    
                    p.Start();
                    while (p.HasExited == false){
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(p.StandardOutput.Read().ToString());
                    }
                    p.WaitForExit();
                    result = p.StandardOutput.ReadToEnd();
                }
            }
            if (game == Games.Sims4){
                while (!CheckForProcess(game)){
                    //
                }                
                while (CheckForProcess(game)){
                    if (GlobalVariables.GameRunning == false) GlobalVariables.GameRunning = true;
                }
            } else { 
                GlobalVariables.GameRunning = true;
            }            

            

            //if (GlobalVariables.DebugMode) Logging.WriteDebugLog("Sims no longer running!");
            return result;
            //return result;
        }*/

        /*private static bool CheckForProcess(Games game){
            bool anything = false;
            if (game == Games.Sims2){
                foreach (string exe in GlobalVariables.Sims2Exes){
                    if (Process.GetProcessesByName(exe).Length == 0){
                        anything = false;
                    } else {
                        anything = true;
                    }
                }

            } else if (game == Games.Sims3){
                foreach (string exe in GlobalVariables.Sims3Exes){
                    if (Process.GetProcessesByName(exe).Length == 0){
                        anything = false;
                    } else {
                        anything = true;
                    }
                }

            } else if (game == Games.Sims4){
                foreach (string exe in GlobalVariables.Sims4Exes){
                    if (Process.GetProcessesByName(exe).Length == 0){
                        anything = false;
                    } else {
                        anything = true;
                    }
                }
            }
            
            return anything;            
        }*/

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
    }

    public class InstanceControllers
    {
        public static void ClearInstance()
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
        }

        public static GameInstance LoadInstanceFiles(GameInstance gameInstance)
        {
            foreach (string file in Directory.GetFiles(gameInstance.InstanceFolders.InstancePackagesFolder))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                FileInfo fi = new(file);
                if (GlobalVariables.SimsFileExtensions.Contains(fi.Extension))
                {
                    SimsPackage simsPackage = ReadPackage(file, gameInstance, fi);                    
                    gameInstance.Files.Add(simsPackage);
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));
                }
                GlobalVariables.mainWindow.IncrementLoadingScreen(1, fi.Name);
            }
            foreach (string file in Directory.GetDirectories(gameInstance.InstanceFolders.InstancePackagesFolder))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                DirectoryInfo fi = new(file);
                SimsPackage simsPackage = ReadPackage(file, gameInstance, fi);                
                gameInstance.Files.Add(simsPackage);
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));
                
                if (!simsPackage.RootMod) gameInstance = GetSubDirectories(gameInstance, file, simsPackage);
                
                GlobalVariables.mainWindow.IncrementLoadingScreen(1, fi.Name);
            }
            foreach (string file in Directory.GetFiles(gameInstance.InstanceFolders.InstanceDownloadsFolder))
            {
                FileInfo f = new(file);
                SimsDownload simsDownload = ReadDownload(file, f);
                gameInstance.Files.Add(simsDownload);
                GlobalVariables.mainWindow.IncrementLoadingScreen(1, f.Name);
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
                            subpackage.IsDirectory = true;
                            subpackage.StandAlone = false; 
                            subpackage.FileName = fi.Name;
                            subpackage.Game = gameInstance.GameChoice;
                            subpackage.Location = file;
                            subpackage.DateAdded = DateTime.Now;
                            subpackage.DateUpdated = DateTime.Now;
                            subpackage.WriteXML();
                        }
                        if (!package.LinkedPackages.Contains(subpackage) && !subpackage.StandAlone) package.LinkedPackages.Add(subpackage);
                        
                    } else
                    {
                        SimsPackage subpackage = gameInstance.Files.OfType<SimsPackage>().First(x => x.Location == file);
                        if (!package.LinkedPackages.Contains(subpackage) && !subpackage.StandAlone) package.LinkedPackages.Add(subpackage);
                    }                   
                }  
            }
            if (package.LinkedFolders.Count != 0)
            {
                foreach (string file in package.LinkedFolders)
                {
                    if (!gameInstance.Files.OfType<SimsPackage>().Any(x => x.Location == file))
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
                            subpackage.Location = file;
                            subpackage.DateAdded = DateTime.Now;
                            subpackage.DateUpdated = DateTime.Now;
                            subpackage.WriteXML();
                        }
                        if (!package.LinkedPackageFolders.Contains(subpackage) && !subpackage.StandAlone) package.LinkedPackageFolders.Add(subpackage);
                    } else
                    {
                        SimsPackage subpackage = gameInstance.Files.OfType<SimsPackage>().First(x => x.Location == file);
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
                    gameInstance.Files.Add(simsPackage); 
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
                GlobalVariables.mainWindow.IncrementLoadingScreen(1, fi.Name);
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
                
                gameInstance.Files.Add(simsPackage);
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));            
                GlobalVariables.mainWindow.IncrementLoadingScreen(1, fi.Name);
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
                    gameInstance.Files.Add(simsPackage); 
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
                
                gameInstance.Files.Add(simsPackage);
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
                simsPackage.Location = file;
                simsPackage.DateAdded = DateTime.Now;
                simsPackage.DateUpdated = DateTime.Now;
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
                simsPackage.Location = file;
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