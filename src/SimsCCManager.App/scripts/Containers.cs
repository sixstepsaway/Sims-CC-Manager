using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Godot;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.OptionLists;
using SimsCCManager.PackageReaders;

namespace SimsCCManager.Containers
{
    public class GameInstance
    {
        public Guid InstanceID {get; set;} = Guid.NewGuid();        
        public SimsGames GameChoice { get; set; }
        public string GameVersion {get; set;}
        public string GameInstallFolder { get; set; }
        public string GameDocumentsFolder { get; set; }
        public Executable CurrentExecutable {get {return Executables[CurrentExecutableIndex]; }}
        public string ExecutablePath { get { return CurrentExecutable.Path; } }
        public string ExecutableName { get { return CurrentExecutable.ExeName; } }
        public string ExecutableNamePath { get { return Path.Combine(CurrentExecutable.Path, CurrentExecutable.ExeName); } }
        public string ExecutableArgs { get { return CurrentExecutable.Arguments; } }
        public List<Executable> Executables {get; set;}
        private int _currentexecutableindex;
        public int CurrentExecutableIndex {
            get {return _currentexecutableindex; } 
            set {_currentexecutableindex = value; 
            InstanceInformationChanged?.Invoke(); 
            }
        }
        public string InstanceName { get; set; }
        public string InstanceFolder { get; set; }
        public string ActiveProfile {get; set;}
        public DateTime DateCreated {get; set;}
        public DateTime DateModified {get; set;}
        public Sims2Folders Sims2Folders {get; set;}
        public Sims3Folders Sims3Folders {get; set;}
        public Sims4Folders Sims4Folders {get; set;}
        public InstanceFolders InstanceFolders {get; set;}
        public List<string> InstanceProfileLocations {get; set;}
        [XmlIgnore]
        public List<InstanceProfile> InstanceProfiles {get; set;}

        private void LoadProfiles()
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Loading {0} profiles.", InstanceProfileLocations.Count));
            XmlSerializer Deserializer = new XmlSerializer(typeof(InstanceProfile));
            List<InstanceProfile> ip = new();
            foreach (string p in InstanceProfileLocations)
            {
                string name = new DirectoryInfo(p).Name;
                string path = Path.Combine(p, string.Format("{0}.xml", name));
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Opening profile: {0}", path));
                if (File.Exists(path)){ 
                    using (FileStream fileStream = new(path, FileMode.Open, System.IO.FileAccess.Read)){
                        using (StreamReader streamReader = new(fileStream)){
                            ip.Add((InstanceProfile)Deserializer.Deserialize(streamReader));
                            streamReader.Close();
                        }
                        fileStream.Close();
                    }
                }
            }
            if (ip.Count == 0)
            {
                DefaultProfile();
            } else
            {
                InstanceProfiles = ip;
                LoadedProfile = InstanceProfiles.First(x => x.ProfileIdentifier == LoadedProfileID);
            }
        }

        private InstanceProfile _loadedprofile;
        [XmlIgnore]
        public InstanceProfile LoadedProfile {get { return _loadedprofile; } set { _loadedprofile = value; 
        InstanceInformationChanged?.Invoke(); }}
        private Guid _loadedprofileid;
        public Guid LoadedProfileID { 
            get { 
                    if (LoadedProfile != null) return LoadedProfile.ProfileIdentifier; else return _loadedprofileid;
                } 
        set
            {
                _loadedprofileid = value;
            }
        }
        public List<Category> Categories {get; set;}

        private IList<ISimsFile> _files;
        [XmlIgnore]
        public IList<ISimsFile> Files {
            get { return _files; }
            set { _files = value; }
        }

        [XmlIgnore]
        public ConcurrentBag<SimsDownload> _downloads = new();
        [XmlIgnore]
        public ConcurrentBag<SimsPackage> _packages = new();


        public delegate void FilesChangedEvent();
        [XmlIgnore]
        public FilesChangedEvent FilesChanged;

        public GameInstance(){
            Files = [];
        }

        public void CreateDefaults()
        {
            InstanceProfiles = new();
            DefaultProfile();
            Categories = new();
            Category category = new() { Name = "Default", Background = Godot.Colors.Transparent, TextColor = Godot.Colors.Transparent, Description = "The default category for all packages."};
            Categories.Add(category);
        }

        private void DefaultProfile()
        {
            InstanceProfile instanceProfile = new() {
                EnabledPackages = new(), 
                ProfileDescription = "The default instance profile.", 
                ProfileName = "Default",
            };
            instanceProfile.ProfileFolder = Path.Combine(InstanceFolders.InstanceProfilesFolder, instanceProfile.SafeFileName());
            Directory.CreateDirectory(instanceProfile.ProfileFolder);
            InstanceProfiles = new(){instanceProfile};
            LoadedProfile = instanceProfile;
        }

        public void BuildInstanceFolders()
        {
            if (!Directory.Exists(InstanceFolder)) Directory.CreateDirectory(InstanceFolder);
            if (!Directory.Exists(InstanceFolders.InstanceDataFolder)) Directory.CreateDirectory(InstanceFolders.InstanceDataFolder);
            if (!Directory.Exists(InstanceFolders.InstanceDownloadsFolder)) Directory.CreateDirectory(InstanceFolders.InstanceDownloadsFolder);
            if (!Directory.Exists(InstanceFolders.InstancePackagesFolder)) Directory.CreateDirectory(InstanceFolders.InstancePackagesFolder);
            if (!Directory.Exists(InstanceFolders.InstanceProfilesFolder)) Directory.CreateDirectory(InstanceFolders.InstanceProfilesFolder);
            //if (!Directory.Exists(InstanceFolders.InstanceSharedGameDataFolder)) Directory.CreateDirectory(InstanceFolders.InstanceSharedGameDataFolder);
            InstanceFolders.InstanceSharedGameDataFolder = Path.Combine(InstanceFolder, "SharedGameData");
        }
        
        public delegate void InstanceInformationChangedEvent();
        [XmlIgnore]
        public InstanceInformationChangedEvent InstanceInformationChanged;
         


        public string XMLfile(){
            return Path.Combine(InstanceFolder, "Instance.xml");
        }

        public void WriteXML()
        {
            if (File.Exists(this.XMLfile()))
            {
                File.Delete(this.XMLfile());
            }
            XmlSerializer InstanceSerializer = new XmlSerializer(this.GetType());
            using (var writer = new StreamWriter(this.XMLfile()))
            {
                InstanceSerializer.Serialize(writer, this);
            }

            XmlSerializer ProfileSerializer = new XmlSerializer(typeof(InstanceProfile));
            foreach (InstanceProfile profile in InstanceProfiles)
            {
                using (var writer = new StreamWriter(profile.XMLFile()))
                {
                    ProfileSerializer.Serialize(writer, profile);
                }
            }
        }

        public GameInstance LoadInstance()
        {
            GameInstance instance = new();
            if (File.Exists(this.XMLfile()))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Loading Instance from {0}", this.XMLfile()));
                XmlSerializer InstanceSerializer = new XmlSerializer(typeof(GameInstance));
                using (FileStream fileStream = new(this.XMLfile(), FileMode.Open, System.IO.FileAccess.Read)){
                    using (StreamReader streamReader = new(fileStream)){
                        instance = (GameInstance)InstanceSerializer.Deserialize(streamReader);
                        streamReader.Close();
                    }
                    fileStream.Close();
                }
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Loaded instance: {0} and its {1} profiles", instance.InstanceName, instance.InstanceProfileLocations.Count));
            }
            
            instance.LoadProfiles();


            return instance;
        }

        public void CheckExisting()
        {
            if (Directory.Exists(InstanceFolder)){
                RenameExistingFolder(InstanceFolder);
            }
        }

        public static string RenameExistingFolder(string folder){
            if (Directory.Exists(folder)){
                string renamed = string.Format("{0}_Backup", folder);
                return renamed = RenameExistingFolder(renamed);
            } else {
                return folder;
            }
        }

        public void MakeFolderTree()
        {
            switch (GameChoice)
            {
                case SimsGames.Sims2:
                    Sims2Folders = new();
                    Sims2Folders.DownloadsFolder = Path.Combine(GameDocumentsFolder, "Downloads");
                    Sims2Folders.CollectionsFolder = Path.Combine(GameDocumentsFolder, "Collections");
                    Sims2Folders.ScreenshotsFolder = Path.Combine(GameDocumentsFolder, "Screenshots");
                    Sims2Folders.MusicFolder = Path.Combine(GameDocumentsFolder, "Music");
                    Sims2Folders.TerrainsFolder = Path.Combine(GameDocumentsFolder, "SC4Terrains");
                    Sims2Folders.LotCatalogFolder = Path.Combine(GameDocumentsFolder, "LotCatalog");
                    Sims2Folders.ConfigFolder = Path.Combine(GameDocumentsFolder, "Config");
                    Sims2Folders.CamerasFolder = Path.Combine(GameDocumentsFolder, "Cameras");
                    Sims2Folders.NeighborhoodsFolder = Path.Combine(GameDocumentsFolder, "Neighborhoods");
                    Sims2Folders.MoviesFolder = Path.Combine(GameDocumentsFolder, "Movies");
                    Sims2Folders.PackagedLotsFolder = Path.Combine(GameDocumentsFolder, "PackagedLots");

                    Sims2Folders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, "Accessory.cache"));
                    Sims2Folders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, "Groups.cache"));

                    Sims2Folders.ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, @"Thumbnails\BuildModeThumbnails.package"));
                    Sims2Folders.ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, @"Thumbnails\CANHObjectsThumbnails.package"));
                    Sims2Folders.ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, @"Thumbnails\CASThumbnails.package"));
                    Sims2Folders.ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, @"Thumbnails\DesignModeThumbnails.package"));
                    Sims2Folders.ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, @"Thumbnails\ObjectThumbnails.package"));
                break;
                case SimsGames.Sims3:
                    Sims3Folders = new();
                    Sims3Folders.DownloadsFolder = Path.Combine(GameDocumentsFolder, "Downloads");
                    Sims3Folders.CollectionsFolder = Path.Combine(GameDocumentsFolder, "Collections");
                    Sims3Folders.ScreenshotsFolder = Path.Combine(GameDocumentsFolder, "Screenshots");
                    Sims3Folders.CustomMusicFolder = Path.Combine(GameDocumentsFolder, "CustomMusic");
                    Sims3Folders.InstalledWorldsFolder = Path.Combine(GameDocumentsFolder, "InstalledWorlds");
                    Sims3Folders.ExportsFolder = Path.Combine(GameDocumentsFolder, "Exports");
                    Sims3Folders.ModsFolder = Path.Combine(GameDocumentsFolder, "Mods");
                    Sims3Folders.VideosFolder = Path.Combine(GameDocumentsFolder, "Recorded Videos");
                    Sims3Folders.SavedOutfitsFolder = Path.Combine(GameDocumentsFolder, "SavedOutfits");
                    Sims3Folders.SavedSimsFolder = Path.Combine(GameDocumentsFolder, "SavedSims");
                    Sims3Folders.SavesFolder = Path.Combine(GameDocumentsFolder, "Saves");
                    Sims3Folders.ScreenshotsFolder = Path.Combine(GameDocumentsFolder, "Screenshots");
                    Sims3Folders.ThumbnailsFolder = Path.Combine(GameDocumentsFolder, "Thumbnails");

                    Sims3Folders.InstanceInstalledWorldsFolder = Path.Combine(InstanceFolders.InstanceSharedGameDataFolder, "InstalledWorldsFolder");
                    Sims3Folders.InstanceExportsFolder = Path.Combine(InstanceFolders.InstanceSharedGameDataFolder, "ExportsFolder");
                    Sims3Folders.InstanceThumbnailsFolder = Path.Combine(InstanceFolders.InstanceSharedGameDataFolder, "ThumbnailsFolder");
                    Sims3Folders.InstanceS3DownloadsFolder = Path.Combine(InstanceFolders.InstanceSharedGameDataFolder, "Downloads");

                    Sims3Folders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, "CASPartCache.package"));
                    Sims3Folders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, "compositorCache.package"));
                    Sims3Folders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, "scriptCache.package"));
                    Sims3Folders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, "simCompositorCache.package"));
                    Sims3Folders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, "socialCache.package"));
                    Sims3Folders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, @"DCCache\missingdeps.idx"));
                    Sims3Folders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, @"DCCache\dcc.ent"));
                    Sims3Folders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, @"IGACache"));

                    Sims3Folders.ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, "CASPartCache.package"));
                    Sims3Folders.ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, "compositorCache.package"));
                    Sims3Folders.ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, "simCompositorCache.package"));
                break;
                case SimsGames.Sims4:
                    Sims4Folders = new();
                    Sims4Folders.ContentFolder = Path.Combine(GameDocumentsFolder, "Content");
                    Sims4Folders.ConfigOverrideFolder = Path.Combine(GameDocumentsFolder, "ConfigOverride");
                    Sims4Folders.ScreenshotsFolder = Path.Combine(GameDocumentsFolder, "Screenshots");
                    Sims4Folders.ModsFolder = Path.Combine(GameDocumentsFolder, "Mods");
                    Sims4Folders.VideosFolder = Path.Combine(GameDocumentsFolder, "Recorded Videos");
                    Sims4Folders.TrayFolder = Path.Combine(GameDocumentsFolder, "Tray");
                    Sims4Folders.SavesFolder = Path.Combine(GameDocumentsFolder, "Saves");
                    Sims4Folders.InstanceContentFolder = Path.Combine(InstanceFolders.InstanceSharedGameDataFolder, "Content");
                    Sims4Folders.InstanceScreenshotsFolder = Path.Combine(InstanceFolders.InstanceSharedGameDataFolder, "Screenshots");
                    Sims4Folders.InstanceVideosFolder = Path.Combine(InstanceFolders.InstanceSharedGameDataFolder, "Videos");

                    Sims4Folders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, "avatarcache.package"));
                    Sims4Folders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, "localthumbcache.package"));
                    Sims4Folders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, "cachestr"));
                    Sims4Folders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, "onlinethumbnailcache"));

                    Sims4Folders.ThumbnailsFiles.Add(Path.Combine(GameDocumentsFolder, "localthumbcache.package"));
                break;
            }
        }

        public void GetGameVersion(){
            string ver = "";
            if (GameChoice == SimsGames.Sims2) ver = GetSims2Version(GameDocumentsFolder);
            if (GameChoice == SimsGames.Sims3) ver = GetSims3Version(GameDocumentsFolder);
            if (GameChoice == SimsGames.Sims4) ver = GetSims4Version(GameDocumentsFolder);
            if (ver != "") {
                ver = Regex.Replace(ver, @"[\p{C}-[\t\r\n]]+", "");
            } 
            
            GameVersion = ver;
        }

        public static string GetSims4Version(string docfolder){
            string version = "";
            string versionfile = Path.Combine(docfolder, "GameVersion.txt");
            if (File.Exists(versionfile)){
                using (FileStream fileStream = new(versionfile, FileMode.Open, System.IO.FileAccess.Read)){
                    using (StreamReader streamReader = new(versionfile)){
                        version = streamReader.ReadLine();
                        streamReader.Close();
                    }
                    fileStream.Close();
                }
            }
            return version;
        }
        public static string GetSims3Version(string docfolder){
            string version = "";
            string versionfile = Path.Combine(docfolder, "Version.tag");
            if (File.Exists(versionfile)){
                using (FileStream fileStream = new(versionfile, FileMode.Open, System.IO.FileAccess.Read)){
                    using (StreamReader streamReader = new(versionfile)){
                        if (streamReader.ReadLine() == "[Version]") {
                            version = streamReader.ReadLine();
                            version = version.Replace("LatestBase = ", "");
                        };                        
                        streamReader.Close();
                    }
                    fileStream.Close();
                }
            }
            return version;
        }

        public static string GetSims2Version(string docfolder){
            return "LatestVersion";
        }

        public void TestInstance()
        {
            StringBuilder sb = new();            
            sb.AppendLine(InstanceFolder);
            //sb.AppendLine(InstanceFolders.InstanceDataFolder);
            sb.AppendLine(Path.Combine(ExecutablePath, ExecutableName));
            if (GameChoice == SimsGames.Sims2)
            {
                sb.AppendLine(Sims2Folders.MoviesFolder);
                sb.AppendLine(Sims2Folders.DownloadsFolder);
                sb.AppendLine(Sims2Folders.TerrainsFolder);
                foreach (string cache in Sims2Folders.CacheFiles)
                {
                    sb.AppendLine(cache);
                }
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(sb.ToString());
            }            
        }
    }

    public class InstanceFolders
    {        
        public string InstancePackagesFolder {get; set;}
        public string InstanceDataFolder {get; set;}
        public string InstanceDownloadsFolder {get; set;}
        public string InstanceProfilesFolder {get; set;}
        public string InstanceSharedGameDataFolder {get; set;}
    }

    public class Sims2Folders
    {        
        public string DownloadsFolder { get; set; }

        //data

        public string TerrainsFolder { get; set; }
        public string LotCatalogFolder { get; set; }
        public string CollectionsFolder { get; set; }
        public string PackagedLotsFolder { get; set; }
        //media        
        public string ScreenshotsFolder { get; set; }
        public string MusicFolder { get; set; }
        public string MoviesFolder { get; set; }

        //settings
        public string ConfigFolder { get; set; }
        public string CamerasFolder { get; set; }

        //saves

        public string NeighborhoodsFolder { get; set; }

        public List<string> CacheFiles { get; set; } = new();
        public List<string> ThumbnailsFiles { get; set; } = new();

        
    }

    public class Sims3Folders
    {
        public string CollectionsFolder { get; set; }
        public string CustomMusicFolder { get; set; }
        public string InstalledWorldsFolder { get; set; }
        public string DownloadsFolder { get; set; }
        public string ExportsFolder { get; set; }
        public string ModsFolder { get; set; }
        public string VideosFolder { get; set; }
        public string SavedOutfitsFolder { get; set; }
        public string SavedSimsFolder { get; set; }
        public string SavesFolder { get; set; }
        public string ScreenshotsFolder { get; set; }
        public string ThumbnailsFolder { get; set; }

        public string InstanceInstalledWorldsFolder { get; set; }
        public string InstanceExportsFolder { get; set; }
        public string InstanceThumbnailsFolder { get; set; }
        public string InstanceS3DownloadsFolder { get; set; }

        public List<string> CacheFiles { get; set; } = new();
        public List<string> ThumbnailsFiles { get; set; } = new();
    }

    public class Sims4Folders
    {
        public string ContentFolder { get; set; }
        public string ConfigOverrideFolder { get; set; }
        public string ModsFolder { get; set; }
        public string VideosFolder { get; set; }
        public string ScreenshotsFolder { get; set; }
        public string TrayFolder { get; set; }
        public string SavesFolder { get; set; }

        public string InstanceContentFolder { get; set; }
        public string InstanceScreenshotsFolder { get; set; }
        public string InstanceVideosFolder { get; set; }

        public List<string> CacheFiles { get; set; } = new();
        public List<string> ThumbnailsFiles { get; set; } = new();
    }





    public class Category {
        public string Name {get; set;}
        public Guid Identifier {get; set;} = Guid.NewGuid();
        public string Description {get; set;}
        public Godot.Color Background {get; set;} = Godot.Color.FromHtml("FFFFFF");
        public Godot.Color TextColor {get; set;} = Godot.Color.FromHtml("000000");
        public int Packages {get; set;}
        [XmlIgnore]
        public bool Hidden {get; set;}
    }

    public class Executable {
        public string Path {get; set;}
        public string ExeName {get; set;}
        public string Arguments {get; set;}
        public bool Selected {get; set;} = false;
        public string Name {get; set;}
    }

    public class Instance
    {
        public Guid InstanceID {get; set;}
        public string InstanceLocation {get; set;}
        public string InstanceName {get; set;}
        public DateTime InstanceCreated {get; set;}
        public DateTime InstanceLastModified {get; set;}
        public SimsGames Game {get; set;}
    }

    public class InstanceProfile
    {
        
        public string ProfileName {get; set;}
        public Guid ProfileIdentifier {get; set;} = Guid.NewGuid();
        public string ProfileDescription {get; set;}
        public string ProfileFolder {get; set;}
        private bool _localsaves; 
        public bool LocalSaves {
            get { return _localsaves; }
            set { _localsaves = value; 
            if (!Directory.Exists(LocalSaveFolder))  Directory.CreateDirectory(LocalSaveFolder); }
        }
        public string LocalSaveFolder { get { return Path.Combine(ProfileFolder, "Saves"); }}
        
        private bool _localsettings; 
        public bool LocalSettings {
            get { return _localsettings; }
            set { _localsettings = value; 
            if (!Directory.Exists(LocalSettingsFolder))  Directory.CreateDirectory(LocalSettingsFolder); }
        }
        public string LocalSettingsFolder { get { return Path.Combine(ProfileFolder, "Settings"); }}
        private bool _localmedia;
        public bool LocalMedia {
            get { return _localmedia; }
            set { _localmedia = value; 
            if (!Directory.Exists(LocalMediaFolder))  Directory.CreateDirectory(LocalMediaFolder); }
        }
        public string LocalMediaFolder { get { return Path.Combine(ProfileFolder, "Media"); }}
        private bool _localdata;
        public bool LocalData {
            get { return _localdata; }
            set { _localdata = value; 
            if (!Directory.Exists(LocalDataFolder))  Directory.CreateDirectory(LocalDataFolder); }
        }
        public string LocalDataFolder { get { return Path.Combine(ProfileFolder, "Data"); }}
        private bool _autobackups;
        public bool AutoBackups {
            get { return _autobackups; }
            set { _autobackups = value; 
            if (!Directory.Exists(BackupFolder))  Directory.CreateDirectory(BackupFolder); }
        }
        public string BackupFolder { get { return Path.Combine(ProfileFolder, "Backups"); }}
        
        public List<EnabledPackages> EnabledPackages {get; set;} = new();

        public string XMLFile()
        {
            return Path.Combine(ProfileFolder, string.Format("{0}.xml", SafeFileName()));
        }

        public string SafeFileName()
        {
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            return String.Join("_", ProfileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries) ).TrimEnd('.');
        }

        public void RemoveEnabled(EnabledPackages pa)
        {
            EnabledPackages.Remove(pa);
            int i = 0;
            EnabledPackages = EnabledPackages.OrderBy(x => x.LoadOrder).ToList();
            foreach (EnabledPackages p in EnabledPackages)
            {
                p.LoadOrder = i;
                i++;
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new();
            foreach (EnabledPackages package in EnabledPackages)
            {
                if (package == EnabledPackages.Last())
                {
                    stringBuilder.Append(string.Format("{0} (Category: {1}, Load Order: {2})", package.PackageName, package.Category, package.LoadOrder));
                } else
                {
                    stringBuilder.Append(string.Format("{0} (Category: {1}, Load Order: {2})\n", package.PackageName, package.Category, package.LoadOrder));   
                }
            }
            if (EnabledPackages.Count != 0)
            {
                return string.Format("Name: {0}, Description: {1}, ID: {2}, Enabled Packages: {3}", this.ProfileName, this.ProfileDescription, this.ProfileIdentifier.ToString(), stringBuilder.ToString());
            } else
            {
                return string.Format("Name: {0}, Description: {1}, ID: {2}", this.ProfileName, this.ProfileDescription, this.ProfileIdentifier.ToString());
            }
            
        }
    }

    

    public class EnabledPackages
    {
        public string PackageName {get; set;}
        public string PackageLocation {get; set;}
        public Guid PackageIdentifier {get; set;}
        public int LoadOrder {get; set;} 
        public Category Category {get; set;}       
    }

    public interface ISimsFile
    {
        public string InfoFile {get;}
        public Guid Identifier {get; set;}      
        public string FileName {get; set;}
        public string Location {get; set;}
        public string FileSize {get;}
        public FileTypes FileType {get;}
        public DateTime DateAdded {get; set;}
        public DateTime DateUpdated {get; set;}
        [XmlIgnore]
        public bool Selected {get; set;}

        void WriteXML();
    }

    public class SimsPackage : ISimsFile
    {
        public Guid Identifier {get; set;} = Guid.NewGuid();   
        public string FileName {get; set;}
        public string InfoFile
        {
            get { 
                    
                        return string.Format("{0}.info", Location); 
                    
                }
        }
        private string _location;
        public string Location {
            get {return _location; } 
            set {
                _location = value;
                if (!IsDirectory && !RootMod)
                {
                    FileType = ContainerExtensions.TypeFromExtension(new FileInfo(Location).Extension);
                } else if (IsDirectory)
                {
                    FileType = FileTypes.Folder;
                }
            }
        }
        public string FileSize {
            get { 
                    if (Directory.Exists(Location))
                    {
                        return Utilities.SizeSuffix(Utilities.DirSize(new(Location))); 
                    } else if (File.Exists(Location)) { 
                        return Utilities.SizeSuffix(new FileInfo(Location).Length);
                    } else
                {
                    return "N/a";
                }
            }
        }
        public FileTypes FileType { get; set; }
        public DateTime DateAdded {get; set;}
        public DateTime DateUpdated {get; set;}
        public string InstalledForVersion {get; set;}
        public string Source {get; set;}
        public string Creator {get; set;}
        public string Notes {get; set;}
        [XmlIgnore]
        public bool Selected {get; set;}
        private bool _isdirectory;
        public bool IsDirectory {
            get { return _isdirectory; } 
            set { _isdirectory = value; 
            if (value)
                {
                    FileType = FileTypes.Folder;
                }
            }}        
        public bool StandAlone {get; set;}
        public bool Broken {get; set;}
        public bool WrongGame {get; set;}
        public string MatchingMesh {get; set;}
        public List<string> MatchingRecolors {get; set;} = new();

        [XmlIgnore]
        public string Type {get
            {
                switch(Game){
                    case SimsGames.Sims2:
                    if (Sims2Data.FunctionSort.Any())
                    {
                        if (Sims2Data.FunctionSort[0] != null)
                        {
                            if (!string.IsNullOrEmpty(Sims2Data.FunctionSort[0].Subcategory))
                            {
                                return string.Format("{0}/{1}", Sims2Data.FunctionSort[0].Category, Sims2Data.FunctionSort[0].Subcategory); 
                            } else
                            {
                                return Sims2Data.FunctionSort[0].Category;
                            }
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(Sims2Data.AltType))
                    {
                        return Sims2Data.AltType;
                    }
                    break;
                    case SimsGames.Sims3:
                    if (Sims3Data.FunctionSort.Any())
                    {
                        if (!string.IsNullOrEmpty(Sims3Data.FunctionSort[0].Subcategory))
                        {
                            return string.Format("{0}/{1}", Sims3Data.FunctionSort[0].Category, Sims3Data.FunctionSort[0].Subcategory); 
                        } else
                        {
                            return Sims3Data.FunctionSort[0].Category;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(Sims3Data.AltType))
                    {
                        return Sims3Data.AltType;
                    }
                    break;
                    case SimsGames.Sims4:
                    if (Sims4Data.FunctionSort.Any())
                    {
                        if (!string.IsNullOrEmpty(Sims4Data.FunctionSort[0].Subcategory))
                        {
                            return string.Format("{0}/{1}", Sims4Data.FunctionSort[0].Category, Sims4Data.FunctionSort[0].Subcategory); 
                        } else
                        {
                            return Sims4Data.FunctionSort[0].Category;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(Sims4Data.AltType))
                    {
                        return Sims4Data.AltType;
                    }
                    break;
                }  
                return "Unknown";            
            
            }
        }

        public string PackageGameVersion {get; set;}
        public List<string> LinkedFiles {get; set;} = new();
        public List<string> LinkedFolders {get; set;} = new();
        [XmlIgnore]
        public List<SimsPackage> LinkedPackages {get; set;} = new();
        [XmlIgnore]
        public List<SimsPackage> LinkedPackageFolders {get; set;} = new();
        public Category PackageCategory {get; set;} = new();
        public string CategoryName { get { return PackageCategory.Name; } }
        public Godot.Color CategoryColor { get { return PackageCategory.Background; }}

        public string Image {get; set;}
        public SimsGames Game {get; set;}
        private bool _rootmod;
        public bool RootMod {get { return _rootmod; } set
            {
                _rootmod = value;
                if (value) { 
                    FileType = FileTypes.Root;
                } else
                {
                    Location = _location;
                }
            }
        }
        public bool OutOfDate {get; set;}
        [XmlIgnore]
        public bool GameMod { get { 
            if (PackageData != null)
            {
            if (Game == SimsGames.Sims2)
                {
                    return Sims2Data.GameMod;
                } else if (Game == SimsGames.Sims3)
                {
                    return Sims3Data.GameMod;
                } else if (Game == SimsGames.Sims4)
                {
                    return Sims4Data.GameMod;
                } else
                {
                    return false;
                }
            } else {
                return false;
            }
        }
            set
            {                
                if (Game == SimsGames.Sims2)
                {
                    Sims2Data.GameMod = value;
                } else if (Game == SimsGames.Sims3)
                {
                    Sims3Data.GameMod = value;
                } else if (Game == SimsGames.Sims4)
                {
                    Sims4Data.GameMod = value;
                } else
                {
                    return;
                }               
            } 
        }
        [XmlIgnore]
        public bool Mesh { get { 
            if (PackageData != null)
            {
            if (Game == SimsGames.Sims2)
                {
                    return Sims2Data.Mesh;
                } else if (Game == SimsGames.Sims3)
                {
                    return Sims3Data.Mesh;
                } else if (Game == SimsGames.Sims4)
                {
                    return Sims4Data.Mesh;
                } else
                {
                    return false;
                }
            }
            else
                {
                    return false;
                }
        }
            set
            {                
                if (Game == SimsGames.Sims2)
                {
                    Sims2Data.Mesh = value;
                } else if (Game == SimsGames.Sims3)
                {
                    Sims3Data.Mesh = value;
                } else if (Game == SimsGames.Sims4)
                {
                    Sims4Data.Mesh = value;
                } else
                {
                    return;
                }               
            } 
        
        }
        [XmlIgnore]
        public bool Recolor { get { 
            if (PackageData != null)
            {
            if (Game == SimsGames.Sims2)
                {
                    return Sims2Data.Recolor;
                } else if (Game == SimsGames.Sims3)
                {
                    return Sims3Data.Recolor;
                } else if (Game == SimsGames.Sims4)
                {
                    return Sims4Data.Recolor;
                } else
                {
                    return false;
                }
            } else {
                return false;
            }
        }
            set
            {                
                if (Game == SimsGames.Sims2)
                {
                    Sims2Data.Recolor = value;
                } else if (Game == SimsGames.Sims3)
                {
                    Sims3Data.Recolor = value;
                } else if (Game == SimsGames.Sims4)
                {
                    Sims4Data.Recolor = value;
                } else
                {
                    return;
                }               
            } 
        }
        [XmlIgnore]
        public bool Orphan { get { 
            if (PackageData != null)
            {
                if (Game == SimsGames.Sims2)
                {
                    return Sims2Data.Orphan;
                } else if (Game == SimsGames.Sims3)
                {
                    return Sims3Data.Orphan;
                } else if (Game == SimsGames.Sims4)
                {
                    return Sims4Data.Orphan;
                } else
                {
                    return false;
                }
            } else { 
                return false; 
            }            
        }
            set
            {                
                if (Game == SimsGames.Sims2)
                {
                    Sims2Data.Orphan = value;
                } else if (Game == SimsGames.Sims3)
                {
                    Sims3Data.Orphan = value;
                } else if (Game == SimsGames.Sims4)
                {
                    Sims4Data.Orphan = value;
                } else
                {
                    return;
                }               
            }        
        }
        [XmlIgnore]
        public string ObjectGUID { get { 
            if (PackageData != null)
            {
            if (Game == SimsGames.Sims2)
                {
                    return Sims2Data.GUID;
                } else if (Game == SimsGames.Sims3)
                {
                    return Sims3Data.GUID;
                } else if (Game == SimsGames.Sims4)
                {
                    return Sims4Data.GUID;
                } else
                {
                    return string.Empty;
                }
            } else
                {
                    return "";
                }
        }}
        public bool Favorite {get; set;}
        [XmlIgnore]
        public bool IsEnabled { get; set; }
        [XmlIgnore]
        public int LoadOrder { get; set; }

        [XmlIgnore]
        private ISimsData _packagedata;
        [XmlIgnore]
        public ISimsData PackageData {
            get
            {
                if (Game == SimsGames.Sims2)
                {
                    return _packagedata as Sims2Data;
                } else if (Game == SimsGames.Sims3)
                {
                    return _packagedata as Sims3Data;
                } else if (Game == SimsGames.Sims4)
                {
                    return _packagedata as Sims4Data;
                } else
                {
                    return _packagedata;
                }                
            } 
            set
            {
                _packagedata = value;
            }
        }

        [XmlIgnore]
        public Image PackageImage { get { 
                if (Game == SimsGames.Sims2) {
                    if (Sims2Data.TXTRDataBlock != null) {
                        if (Sims2Data.TXTRDataBlock.Count > 0)
                        {
                            if (Sims2Data.TXTRDataBlock[0].Texture != null) {
                                return Sims2Data.TXTRDataBlock[0].Texture; 
                            }
                        }                        
                    }
                }
                return null;
            }
        }

        
        public Sims2Data Sims2Data { get { return PackageData as Sims2Data; } set { PackageData = value; }}
        public Sims3Data Sims3Data { get { return PackageData as Sims3Data; } set { PackageData = value; }}
        public Sims4Data Sims4Data { get { return PackageData as Sims4Data; } set { PackageData = value; }}
        
        public bool ShouldSerializeSims2Data()
        {            
            return Game == SimsGames.Sims2;
        }
        public bool ShouldSerializeSims3Data()
        {
            return Game == SimsGames.Sims3;
        }
        public bool ShouldSerializeSims4Data()
        {
            return Game == SimsGames.Sims4;
        }
        static ReaderWriterLock locker = new ReaderWriterLock();
        public void WriteXML()
        {
            try
            {
                locker.AcquireWriterLock(int.MaxValue); 
                if (File.Exists(this.InfoFile))
                {
                    File.Delete(this.InfoFile);
                }
                XmlSerializer InfoSerializer = new XmlSerializer(this.GetType());
                using (var writer = new StreamWriter(this.InfoFile))
                {
                    InfoSerializer.Serialize(writer, this);
                }
            }
            finally
            {
                locker.ReleaseWriterLock();
            }
               
        }

        public void SetProperty(string propName, dynamic input){
            var prop = this.ProcessProperty(propName);
            PropertyInfo property = this.GetType().GetProperty(propName);
            if (property != null){
                if (property.PropertyType == typeof(Godot.Color)){
                    Godot.Color newcolor = Godot.Color.FromHtml(input);
                    property.SetValue(this, newcolor);
                } else if (property.PropertyType == typeof(Guid)){
                    if (input.GetType() == typeof(string)){
                        string inp = input as string;
                        property.SetValue(this, Guid.Parse(inp));
                    } else if (input.GetType() == typeof(Guid)){
                        property.SetValue(this, input);
                    }
                } else if (property.PropertyType == typeof(string)) {
                    property.SetValue(this, input as string);
                } else if (property.PropertyType == typeof(bool)) {
                    if (input.GetType() == typeof(bool)) {
                        property.SetValue(this, input);
                    } else if (input.GetType() == typeof(string)){
                        property.SetValue(this, bool.Parse(input));
                    }                    
                } else if (property.PropertyType == typeof(SimsGames)){
                                 
                }
            }
        }

        public object ProcessProperty(string propName){
            return this.GetType().GetProperty(propName).GetValue (this, null);
        }

    }


    public interface ISimsData
    {
        public string FileLocation {get; set;}
        public string Title {get; set;}
        public string Description {get; set;}
        public string Type {get;}
        public string AltType {get; set;}

        public List<FunctionSortList> FunctionSort {get; set;}
        public string GUID {get; set;}
        public List<IndexEntry> IndexEntries {get; set;}
        public List<EntryCount> IndexEntryCounts {get; set;}

        public bool Recolor {get; set;}    
        public bool Mesh {get; set;}
        public bool Orphan {get; set;}
        public bool GameMod {get; set;}

        

        public ClothingInfo ClothingInfo {get; set;}
        

        void Serialize();

        int EntryCount(string entry);   

        void IsGameMod();
        
        public int EntryTypes();
        public void DictionaryEntries();

    }

    public class ClothingInfo
    {
        public string Type {get; set;} //top bottom full
        public List<string> Category {get; set;} = new(); //formal atletic etc
        public List<string> Gender {get; set;} = new(); 
        public List<string> Age {get; set;} = new();
        public List<string> Species {get; set;} = new(); 
    }

    public class Sims2Data : ISimsData
    {
        public string FileLocation {get; set;}
        public string Title {get; set;}
        public string Description {get; set;}
        public string Type {
            get { if (!string.IsNullOrEmpty(FunctionSort[0].Subcategory))
                {
                    return string.Format("{0}/{1}", FunctionSort[0].Category, FunctionSort[0].Subcategory);
                } else
                {
                    return FunctionSort[0].Category; 
                }
            }
        }
        public string AltType {get; set;}

        public List<FunctionSortList> FunctionSort {get; set;} = new();
        private string _guid;
        public string GUID {get
            {
                if (!string.IsNullOrEmpty(_guid))
                {
                    if (_guid.StartsWith("0x"))
                    {
                        return _guid.TrimPrefix("0x");
                    } else
                    {
                        return _guid;
                    }
                } else
                {
                    return "";
                }
                
            } set
            {
                _guid = value;
            }
        }
        public bool Recolor {get; set;}    
        public bool Mesh {get; set;}
        public bool Orphan {get; set;}
        public bool GameMod {get; set;}

        public List<Sims2Expansions> Expansions {get; set;} = new();

        public ClothingInfo ClothingInfo {get; set;} = new();
        
        public List<IndexEntry> IndexEntries {get; set;} = new();
        

        public void DictionaryEntries()
        {
            ConcurrentBag<EntryCount> entryCounts = new();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking {0} indexentries against {1} types", IndexEntries.Count, Sims2PackageStatics.Sims2EntryTypes.Count));
            Parallel.ForEach(Sims2PackageStatics.Sims2EntryTypes, entryType => 
            {                   
                int count = IndexEntries.Count(x => x.TypeID == entryType.TypeID);
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("File has {0} of entry {1}", count, entryType.Tag));
                if (count != 0) entryCounts.Add(new() { EntryTag = entryType.Tag.ToUpper(), Count = count});
            });
            IndexEntryCounts = entryCounts.ToList();
        }

        public List<EntryCount> IndexEntryCounts {get; set;} = new();
        public List<GMDCData> GMDCDataBlock {get; set;} = new();
        public List<MMATData> MMATDataBlock {get; set;} = new();
        public XFLRData XFLRDataBlock {get; set;} = new();
        public List<TXTRData> TXTRDataBlock {get; set;} = new();
        public XNGBData XNGBDataBlock {get; set;} = new();
        public List<GZPSData> GZPSDataBlock {get; set;} = new();
        public List<EIDRData> EIDRDataBlock {get; set;} = new();
        public List<SHPEData> SHPEDataBlock {get; set;} = new();
        public List<TXMTData> TXMTDataBlock {get; set;} = new();
        public List<XHTNData> XHTNDataBlock {get; set;} = new();

        public int EntryCount(string entry)
        {
            if (IndexEntryCounts.Any(x=>x.EntryTag.Equals(entry, StringComparison.CurrentCultureIgnoreCase)))
            {
                return IndexEntryCounts.First(x=>x.EntryTag.Equals(entry, StringComparison.CurrentCultureIgnoreCase)).Count;
            } else
            {
                return 0;
            }
        }

        public int EntryTypes()
        {
            return IndexEntryCounts.Count;
        }

        
        

        public void Serialize()
        {
            XmlSerializer InfoSerializer = new XmlSerializer(typeof(Sims2Data));
            using (var writer = new StreamWriter(string.Format("{0}.xml", FileLocation)))
            {
                InfoSerializer.Serialize(writer, this);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            if (!string.IsNullOrEmpty(Title)) sb.Append(string.Format("Title: {0}, ", Title));
            if (!string.IsNullOrEmpty(Description)) sb.Append(string.Format("Description: {0}, ", Description));
            if (FunctionSort.Count > 0)
            {
                sb.Append("Function: ");
                foreach (FunctionSortList function in FunctionSort)
                {
                    if (!string.IsNullOrEmpty(function.Subcategory))
                    {
                        sb.AppendLine(string.Format("{0}", function.Category));
                    } else
                    {
                        sb.AppendLine(string.Format("{0}/{1}", function.Category, function.Subcategory));
                    }
                    
                }
            }
            sb.AppendLine(string.Format("Recolor: {0}, ", Recolor));
            sb.Append(string.Format("Mesh: {0}, ", Mesh));
            sb.Append(string.Format("GUID: {0}, ", GUID));
            
            return sb.ToString();
        }

        public void IsGameMod()
        {
            Mesh = false;
            Recolor = false;
            GameMod = true;
        }
    }
    public class Sims3Data : ISimsData
    {
        public string FileLocation {get; set;}
        public string Title {get; set;}
        public string Description {get; set;}
        public string Type {
            get { if (!string.IsNullOrEmpty(FunctionSort[0].Subcategory))
                {
                    return string.Format("{0}/{1}", FunctionSort[0].Category, FunctionSort[0].Subcategory);
                } else
                {
                    return FunctionSort[0].Category; 
                }
            }
        }
        public string AltType {get; set;}

        public List<FunctionSortList> FunctionSort {get; set;} = new();
        private List<IndexEntry> _indexentries;
        public List<IndexEntry> IndexEntries {
            get { return _indexentries; }
            set { _indexentries = value; 
                /*foreach (EntryType entryType in Sims2PackageStatics.Sims2EntryTypes)
                {
                    IndexEntryCounts.Add(entryType.Tag, value.Count(x => x.TypeID == entryType.TypeID));
                }*/
            }
        }
        public List<EntryCount> IndexEntryCounts {get; set;}
        public string GUID {get; set;} = "";
        public bool Recolor {get; set;}    
        public bool Mesh {get; set;}
        public bool Orphan {get; set;}
        public bool GameMod {get; set;}

        public List<Sims3Expansions> Expansions {get; set;} = new();

        public ClothingInfo ClothingInfo {get; set;} = new();
        public void Serialize()
        {
            //throw new NotImplementedException();
        }

        public int EntryCount(string entry)
        {
            throw new NotImplementedException();
        }
        public void IsGameMod()
        {
            Mesh = false;
            Recolor = false;
            GameMod = true;
        }
        public int EntryTypes()
        {
            return IndexEntryCounts.Count;
        }

        public void DictionaryEntries()
        {
            throw new NotImplementedException();
        }
    }
    
    public class Sims4Data : ISimsData
    {
        public string FileLocation {get; set;}
        public string Title {get; set;}
        public string Description {get; set;}
        public string Type {
            get { if (!string.IsNullOrEmpty(FunctionSort[0].Subcategory))
                {
                    return string.Format("{0}/{1}", FunctionSort[0].Category, FunctionSort[0].Subcategory);
                } else
                {
                    return FunctionSort[0].Category; 
                }
            }
        }
        public string AltType {get; set;}

        public List<FunctionSortList> FunctionSort {get; set;} = new();
        private List<IndexEntry> _indexentries;
        public List<IndexEntry> IndexEntries {
            get { return _indexentries; }
            set { _indexentries = value; 
                /*foreach (EntryType entryType in Sims2PackageStatics.Sims2EntryTypes)
                {
                    IndexEntryCounts.Add(entryType.Tag, value.Count(x => x.TypeID == entryType.TypeID));
                }*/
            }
        }
        public List<EntryCount> IndexEntryCounts {get; set;}
        public string GUID {get; set;} = "";
        public bool Recolor {get; set;}    
        public bool Mesh {get; set;}
        public bool Orphan {get; set;}
        public bool GameMod {get; set;}

        public List<Sims4Expansions> Expansions {get; set;} = new();

        public ClothingInfo ClothingInfo {get; set;} = new();
        public void Serialize()
        {
            //throw new NotImplementedException();
        }

        public int EntryCount(string entry)
        {
            throw new NotImplementedException();
        }
        public void IsGameMod()
        {
            Mesh = false;
            Recolor = false;
            GameMod = true;
        }
        public int EntryTypes()
        {
            return IndexEntryCounts.Count;
        }

        public void DictionaryEntries()
        {
            throw new NotImplementedException();
        }
    }








    public class SimsDownload : ISimsFile
    {
        
        public Guid Identifier {get; set;} = Guid.NewGuid();
        public string FileName {get; set;}
        public string InfoFile
        {
            get { 
                    return string.Format("{0}.info", new FileInfo(Location).FullName);                     
                }
        }
        public string Location {get; set;}
        public string FileSize {
            get { 
                    return Utilities.SizeSuffix(new FileInfo(Location).Length);
                }            
        }
        public FileTypes FileType {
            get {
                    return ContainerExtensions.TypeFromExtension(new FileInfo(Location).Extension);
                }
        }
        public DateTime DateAdded {get; set;}
        public DateTime DateUpdated {get; set;}
        [XmlIgnore]
        public bool Selected {get; set;}        
        public bool Installed {get; set;} = false;
        public void WriteXML()
        {
            if (File.Exists(this.InfoFile))
            {
                File.Delete(this.InfoFile);
            }
            XmlSerializer InfoSerializer = new XmlSerializer(this.GetType());
            using (var writer = new StreamWriter(this.InfoFile))
            {
                InfoSerializer.Serialize(writer, this);
            }   
        }
    }

    public class ContainerExtensions
    {
        public static FileTypes TypeFromExtension(string extension){
            extension = extension.Replace(".", "").ToLower();
            return extension switch
            {
                "zip" => FileTypes.Zip,
                "rar" => FileTypes.Rar,
                "package" => FileTypes.Package,
                "ts4script" => FileTypes.TS4Script,
                "sims3pack" => FileTypes.Sims3Pack,
                "sims2pack" => FileTypes.Sims2Pack,
                "7z" => FileTypes.SevenZip,
                "pkg" => FileTypes.PKG,
                "jpg" => FileTypes.JPG,
                "jpeg" => FileTypes.JPG,
                "png" => FileTypes.PNG,
                "doc" => FileTypes.Doc,
                "txt" => FileTypes.Txt,
                "folder" => FileTypes.Folder,
                _ => FileTypes.Other,
            };
        }
    }

    public enum FileTypes {
        [Description("Package")]
        Package,
        [Description("TS4Script")]
        TS4Script,
        [Description("Sims3Pack")]
        Sims3Pack,
        [Description("Sims2Pack")]
        Sims2Pack,
        [Description("Zip")]
        Zip,
        [Description("7Zip")]
        SevenZip,
        [Description("Rar")]
        Rar,
        [Description("PKG")]
        PKG,
        [Description("JPG")]
        JPG,
        [Description("PNG")]
        PNG,
        [Description("Doc")]
        Doc,
        [Description("Txt")]
        Txt,
        [Description("Other")]
        Other,
        [Description("Folder")]
        Folder,
        [Description("Root")]
        Root,
        [Description("Null")]
        Null
    }

    public enum Sims2Expansions{
        [Description("Base Game")]
        BaseGame,
        [Description("University")]
        University,
        [Description("Nightlife")]
        Nightlife,
        [Description("Open for Business")]
        OpenforBusiness,
        [Description("Pets")]
        Pets,
        [Description("Seasons")]
        Seasons,
        [Description("BonVoyage")]
        BonVoyage,
        [Description("FreeTime")]
        FreeTime,
        [Description("Apartment Life")]
        ApartmentLife,
        [Description("Family Fun Stuff")]
        FamilyFunStuff,
        [Description("Glamour Life Stuff")]
        GlamourLifeStuff,
        [Description("Happy Holiday Stuff")]
        HappyHolidayStuff,
        [Description("Celebration Stuff")]
        CelebrationStuff,
        [Description("H&M Fashion Stuff")]
        HMFashionStuff,
        [Description("Teen Style Stuff")]
        TeenStyleStuff,
        [Description("Kitchen & Bath Interior Design Stuff")]
        KitchenBathInteriorDesignStuff,
        [Description("IKEA Home Stuff")]
        IKEAHomeStuff,
        [Description("Mansion Garden Stuff")]
        MansionGardenStuff
    }

    public enum Sims3Expansions{
        [Description("WorldAdventures")]
        WorldAdventures,
        [Description("Ambitions")]
        Ambitions,
        [Description("Late Night")]
        LateNight,
        [Description("Generations")]
        Generations,
        [Description("Pets")]
        Pets,
        [Description("Showtime")]
        Showtime,
        [Description("Supernatural")]
        Supernatural,
        [Description("Seasons")]
        Seasons,
        [Description("University Life")]
        UniversityLife,
        [Description("Island Paradise")]
        IslandParadise,
        [Description("Into the Future")]
        IntotheFuture,
        [Description("High End Loft Stuff")]
        HighEndLoftStuff,
        [Description("Fast Lane Stuff")]
        FastLaneStuff,
        [Description("Outdoor Living Stuff")]
        OutdoorLivingStuff,
        [Description("Town Life Stuff")]
        TownLifeStuff,
        [Description("Master Suite Stuff")]
        MasterSuiteStuff,
        [Description("Katy Perry's Sweet Treats")]
        KatyPerrysSweetTreats,
        [Description("Diesel Stuff")]
        DieselStuff,
        [Description("Decades Stuff")]
        DecadesStuff,
        [Description("Movie Stuff")]
        MovieStuff
    }

    public enum Sims4Expansions{
        [Description("Get to Work")]
        GettoWork,
        [Description("Get Together")]
        GetTogether,
        [Description("City Living")]
        CityLiving,
        [Description("Cats & Dogs")]
        CatsDogs,
        [Description("Seasons")]
        Seasons,
        [Description("Get Famous")]
        GetFamous,
        [Description("Island Living")]
        IslandLiving,
        [Description("Discover University")]
        DiscoverUniversity,
        [Description("Eco Lifestyle")]
        EcoLifestyle,
        [Description("Snowy Escape")]
        SnowyEscape,
        [Description("Cottage Living")]
        CottageLiving,
        [Description("High School Years")]
        HighSchoolYears,
        [Description("Growing Together")]
        GrowingTogether,
        [Description("Horse Ranch")]
        HorseRanch,
        [Description("For Rent")]
        ForRent,
        [Description("Lovestruck")]
        Lovestruck,
        [Description("Life & Death")]
        LifeDeath,
        [Description("Businesses & Hobbies")]
        BusinessesHobbies,
        [Description("Enchanted by Nature")]
        EnchantedbyNature,
        [Description("Adventure Awaits")]
        AdventureAwaits,
        [Description("Outdoor Retreat")]
        OutdoorRetreat,
        [Description("Spa Day")]
        SpaDay,
        [Description("Dine Out")]
        DineOut,
        [Description("Vampires")]
        Vampires,
        [Description("Parenthood")]
        Parenthood,
        [Description("Jungle Adventure")]
        JungleAdventure,
        [Description("StrangerVille")]
        StrangerVille,
        [Description("Realm of Magic")]
        RealmofMagic,
        [Description("Star Wars: Journey to Batuu")]
        StarWarsJourneytoBatuu,
        [Description("Dream Home Decorator")]
        DreamHomeDecorator,
        [Description("My Wedding Stories")]
        MyWeddingStories,
        [Description("Werewolves")]
        Werewolves,
        [Description("Luxury Party Stuff")]
        LuxuryPartyStuff,
        [Description("Perfect Patio Stuff")]
        PerfectPatioStuff,
        [Description("Cool Kitchen Stuff")]
        CoolKitchenStuff,
        [Description("Spooky Stuff")]
        SpookyStuff,
        [Description("Movie Hangout Stuff")]
        MovieHangoutStuff,
        [Description("Romantic Garden Stuff")]
        RomanticGardenStuff,
        [Description("Kids Room Stuff")]
        KidsRoomStuff,
        [Description("Backyard Stuff")]
        BackyardStuff,
        [Description("Vintage Glamour Stuff")]
        VintageGlamourStuff,
        [Description("Bowling Night Stuff")]
        BowlingNightStuff,
        [Description("Fitness Stuff")]
        FitnessStuff,
        [Description("Toddler Stuff")]
        ToddlerStuff,
        [Description("Laundry Day Stuff")]
        LaundryDayStuff,
        [Description("My First Pet Stuff")]
        MyFirstPetStuff,
        [Description("Moschino Stuff")]
        MoschinoStuff,
        [Description("Tiny Living Stuff")]
        TinyLivingStuff,
        [Description("Nifty Knitting Stuff")]
        NiftyKnittingStuff,
        [Description("Paranormal Stuff")]
        ParanormalStuff,
        [Description("Home Chef Hustle Stuff")]
        HomeChefHustleStuff,
        [Description("Crystal Creations Stuff")]
        CrystalCreationsStuff,
        [Description("Throwback Fit Kit")]
        ThrowbackFitKit,
        [Description("Country Kitchen Kit")]
        CountryKitchenKit,
        [Description("Bust the Dust Kit")]
        BusttheDustKit,
        [Description("Courtyard Oasis Kit")]
        CourtyardOasisKit,
        [Description("Industrial Loft Kit")]
        IndustrialLoftKit,
        [Description("Fashion Street Kit")]
        FashionStreetKit,
        [Description("Incheon Arrivals Kit")]
        IncheonArrivalsKit,
        [Description("Blooming Rooms Kit")]
        BloomingRoomsKit,
        [Description("Modern Menswear Kit")]
        ModernMenswearKit,
        [Description("Carnaval Streetwear Kit")]
        CarnavalStreetwearKit,
        [Description("Decor to the Max Kit")]
        DecortotheMaxKit,
        [Description("Moonlight Chic Kit")]
        MoonlightChicKit,
        [Description("Little Campers Kit")]
        LittleCampersKit,
        [Description("First Fits Kit")]
        FirstFitsKit,
        [Description("Desert Luxe Kit")]
        DesertLuxeKit,
        [Description("Pastel Pop Kit")]
        PastelPopKit,
        [Description("Everyday Clutter Kit")]
        EverydayClutterKit,
        [Description("Bathroom Clutter Kit")]
        BathroomClutterKit,
        [Description("Simtimates Collection Kit")]
        SimtimatesCollectionKit,
        [Description("Greenhouse Haven Kit")]
        GreenhouseHavenKit,
        [Description("Basement Treasures Kit")]
        BasementTreasuresKit,
        [Description("Book Nook Kit")]
        BookNookKit,
        [Description("Grunge Revival Kit")]
        GrungeRevivalKit,
        [Description("Poolside Splash Kit")]
        PoolsideSplashKit,
        [Description("Modern Luxe Kit")]
        ModernLuxeKit,
        [Description("Castle Estate Kit")]
        CastleEstateKit,
        [Description("Goth Galore Kit")]
        GothGaloreKit,
        [Description("Urban Homage Kit")]
        UrbanHomageKit,
        [Description("Party Essentials Kit")]
        PartyEssentialsKit,
        [Description("Riviera Retreat Kit")]
        RivieraRetreatKit,
        [Description("Cozy Bistro Kit")]
        CozyBistroKit,
        [Description("Storybook Nursery Kit")]
        StorybookNurseryKit,
        [Description("Artist Studio Kit")]
        ArtistStudioKit,
        [Description("Sweet Slumber Party Kit")]
        SweetSlumberPartyKit,
        [Description("Cozy Kitsch Kit")]
        CozyKitschKit,
        [Description("Secret Sanctuary Kit")]
        SecretSanctuaryKit,
        [Description("Comfy Gamer Kit")]
        ComfyGamerKit,
        [Description("Casanova Cave Kit")]
        CasanovaCaveKit,
        [Description("Refined Living Room Kit")]
        RefinedLivingRoomKit,
        [Description("Business Chic Kit")]
        BusinessChicKit,
        [Description("Sweet Allure Kit")]
        SweetAllureKit,
        [Description("Sleek Bathroom Kit")]
        SleekBathroomKit,
        [Description("Restoration Workshop Kit")]
        RestorationWorkshopKit,
        [Description("Kitchen Clutter Kit")]
        KitchenClutterKit,
        [Description("Golden Years Kit")]
        GoldenYearsKit,
        [Description("Grange Mudroom Kit")]
        GrangeMudroomKit,
        [Description("Essential Glam Kit")]
        EssentialGlamKit,
        [Description("Autumn Apparel Kit")]
        AutumnApparelKit
    }

    





    public class VFSFiles
    {
        public List<MovedItems> ItemsMoved {get; set;} = new();
        public List<LinkedItems> ItemsLinked {get; set;} = new();

        public List<string> FoldersCreated {get; set;} = new();
    }

    public class MovedItems
    {
        public string OriginalLocation {get; set;}
        public string MovedTo {get; set;}
        public bool IsFolder {get; set;}
    }

    public class LinkedItems
    {
        public string LinkLocation {get; set;}
        public bool IsFolder {get; set;}
    }

    public class EntryCount
    {
        public string EntryTag {get; set;}
        public int Count {get; set;}
    }

}