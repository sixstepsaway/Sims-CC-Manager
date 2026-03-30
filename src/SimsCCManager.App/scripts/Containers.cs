using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DataGridContainers;
using Godot;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.OptionLists;
using SimsCCManager.PackageReaders;
using static SimsCCManager.Containers.ISimsData;

namespace SimsCCManager.Containers
{
    public class GameInstance
    {
        public Guid InstanceID { get; set; } = Guid.NewGuid();
        public SimsGames GameChoice { get; set; }
        public string GameVersion { get; set; }
        public string GameInstallFolder { get; set; }
        public string GameDocumentsFolder { get; set; }
        public Executable CurrentExecutable { get { return Executables[CurrentExecutableIndex]; } }
        public string ExecutablePath { get { return CurrentExecutable.Path; } }
        public string ExecutableName { get { return CurrentExecutable.ExeName; } }
        public string ExecutableNamePath { get { return Path.Combine(CurrentExecutable.Path, CurrentExecutable.ExeName); } }
        public string ExecutableArgs { get { return CurrentExecutable.Arguments; } }
        public List<Executable> Executables { get; set; }
        private int _currentexecutableindex;
        public int CurrentExecutableIndex
        {
            get { return _currentexecutableindex; }
            set
            {
                _currentexecutableindex = value;
                InstanceInformationChanged?.Invoke();
            }
        }
        public string InstanceName { get; set; }
        public string InstanceFolder { get; set; }
        public string ActiveProfile { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public Sims2Folders Sims2Folders { get; set; }
        public Sims3Folders Sims3Folders { get; set; }
        public Sims4Folders Sims4Folders { get; set; }
        public SimsMedievalFolders SimsMedievalFolders { get; set; }
        public InstanceFolders InstanceFolders { get; set; }
        public List<string> InstanceProfileLocations { get; set; } = new();
        [XmlIgnore]
        public List<InstanceProfile> InstanceProfiles { get; set; } = new();
        public List<DataGridHeader> Headers { get; set; }

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
                if (File.Exists(path))
                {
                    using (FileStream fileStream = new(path, FileMode.Open, System.IO.FileAccess.Read))
                    {
                        using (StreamReader streamReader = new(fileStream))
                        {
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
            }
            else
            {
                InstanceProfiles = ip;
                LoadedProfile = InstanceProfiles.First(x => x.ProfileIdentifier == LoadedProfileID);
            }
        }

        private InstanceProfile _loadedprofile;
        [XmlIgnore]
        public InstanceProfile LoadedProfile
        {
            get { return _loadedprofile; }
            set
            {
                _loadedprofile = value;
                InstanceInformationChanged?.Invoke();
            }
        }
        private Guid _loadedprofileid;
        public Guid LoadedProfileID
        {
            get
            {
                if (LoadedProfile != null) return LoadedProfile.ProfileIdentifier; else return _loadedprofileid;
            }
            set
            {
                _loadedprofileid = value;
            }
        }
        public List<Category> Categories { get; set; }

        private IList<ISimsFile> _files;
        [XmlIgnore]
        public IList<ISimsFile> Files
        {
            get { return _files; }
            set { _files = value; }
        }

        [XmlIgnore]
        public List<SimsPackage> Packages { get { return Files.OfType<SimsPackage>().ToList(); }}

        [XmlIgnore]
        public ConcurrentBag<SimsDownload> _downloads = new();
        [XmlIgnore]
        public ConcurrentBag<SimsPackage> _packages = new();


        public delegate void FilesChangedEvent();
        [XmlIgnore]
        public FilesChangedEvent FilesChanged;

        public GameInstance()
        {
            Files = [];
        }

        public void CreateDefaults()
        {
            InstanceProfiles = new();
            DefaultProfile();
            Categories = new();
            Category category = new() { Name = "Default", Background = Godot.Colors.Transparent, TextColor = Godot.Colors.Transparent, Description = "The default category for all packages." };
            Categories.Add(category);
        }

        public void AddProfile(InstanceProfile profile)
        {
            InstanceProfiles = new() { profile };
            InstanceProfileLocations.Add(profile.ProfileFolder);
        }

        private void DefaultProfile()
        {
            InstanceProfile instanceProfile = new()
            {
                EnabledPackages = new(),
                ProfileDescription = "The default instance profile.",
                ProfileName = "Default",
            };
            instanceProfile.ProfileFolder = Path.Combine(InstanceFolders.InstanceProfilesFolder, instanceProfile.SafeFileName());
            Directory.CreateDirectory(instanceProfile.ProfileFolder);
            AddProfile(instanceProfile);
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



        public string XMLfile()
        {
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
                using (FileStream fileStream = new(this.XMLfile(), FileMode.Open, System.IO.FileAccess.Read))
                {
                    using (StreamReader streamReader = new(fileStream))
                    {
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
            if (Directory.Exists(InstanceFolder))
            {
                RenameExistingFolder(InstanceFolder);
            }
        }

        public static string RenameExistingFolder(string folder)
        {
            if (Directory.Exists(folder))
            {
                string renamed = string.Format("{0}_Backup", folder);
                return renamed = RenameExistingFolder(renamed);
            }
            else
            {
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
                case SimsGames.SimsMedieval:
                    SimsMedievalFolders = new();
                    SimsMedievalFolders.ArchiveFolder = Path.Combine(GameDocumentsFolder, "Archive");
                    SimsMedievalFolders.CurrentGameFolder = Path.Combine(GameDocumentsFolder, "CurrentGame.tsm");
                    SimsMedievalFolders.InstalledWorldsFolder = Path.Combine(GameDocumentsFolder, "InstalledWorlds");
                    SimsMedievalFolders.SavedSimsFolder = Path.Combine(GameDocumentsFolder, "SavedSims");
                    SimsMedievalFolders.SavesFolder = Path.Combine(GameDocumentsFolder, "Saves");
                    SimsMedievalFolders.ScreenshotsFolder = Path.Combine(GameDocumentsFolder, "Screenshots");
                    SimsMedievalFolders.ThumbnailsFolder = Path.Combine(GameDocumentsFolder, "Thumbnails");

                    SimsMedievalFolders.ModsFolder = Path.Combine(Path.Combine(GameInstallFolder, "Mods"), "Packages");

                    SimsMedievalFolders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, "CASPartCache.package"));
                    SimsMedievalFolders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, "compositorCache.package"));
                    SimsMedievalFolders.CacheFiles.Add(Path.Combine(GameDocumentsFolder, "simCompositorCache.package"));

                    break;
            }
        }

        public void GetGameVersion()
        {
            string ver = "";
            if (GameChoice == SimsGames.Sims2) ver = GetSims2Version(GameDocumentsFolder);
            if (GameChoice == SimsGames.Sims3) ver = GetSims3Version(GameDocumentsFolder);
            if (GameChoice == SimsGames.Sims4) ver = GetSims4Version(GameDocumentsFolder);
            if (GameChoice == SimsGames.SimsMedieval) ver = GetSimsMedievalVersion(GameDocumentsFolder);
            if (ver != "")
            {
                ver = Regex.Replace(ver, @"[\p{C}-[\t\r\n]]+", "");
            }

            GameVersion = ver;
        }

        public static string GetSims4Version(string docfolder)
        {
            string version = "";
            string versionfile = Path.Combine(docfolder, "GameVersion.txt");
            if (File.Exists(versionfile))
            {
                using (FileStream fileStream = new(versionfile, FileMode.Open, System.IO.FileAccess.Read))
                {
                    using (StreamReader streamReader = new(versionfile))
                    {
                        version = streamReader.ReadLine();
                        streamReader.Close();
                    }
                    fileStream.Close();
                }
            }
            return version;
        }
        public static string GetSimsMedievalVersion(string docfolder)
        {
            string version = "";
            string versionfile = Path.Combine(docfolder, "Version.tag");
            if (File.Exists(versionfile))
            {
                using (FileStream fileStream = new(versionfile, FileMode.Open, System.IO.FileAccess.Read))
                {
                    using (StreamReader streamReader = new(versionfile))
                    {
                        streamReader.ReadLine();
                        version = streamReader.ReadLine();
                        streamReader.Close();
                    }
                    fileStream.Close();
                }
            }
            version = version.Replace("Latest = ", "");
            return version;
        }
        public static string GetSims3Version(string docfolder)
        {
            string version = "";
            string versionfile = Path.Combine(docfolder, "Version.tag");
            if (File.Exists(versionfile))
            {
                using (FileStream fileStream = new(versionfile, FileMode.Open, System.IO.FileAccess.Read))
                {
                    using (StreamReader streamReader = new(versionfile))
                    {
                        if (streamReader.ReadLine() == "[Version]")
                        {
                            version = streamReader.ReadLine();
                            version = version.Replace("LatestBase = ", "");
                        }
                        ;
                        streamReader.Close();
                    }
                    fileStream.Close();
                }
            }
            return version;
        }

        public static string GetSims2Version(string docfolder)
        {
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
        public string InstancePackagesFolder { get; set; }
        public string InstanceDataFolder { get; set; }
        public string InstanceDownloadsFolder { get; set; }
        public string InstanceProfilesFolder { get; set; }
        public string InstanceSharedGameDataFolder { get; set; }
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

    public class SimsMedievalFolders
    {
        public string ArchiveFolder { get; set; }
        public string CurrentGameFolder { get; set; }
        public string InstalledWorldsFolder { get; set; }
        public string ModsFolder { get; set; }
        public string SavedSimsFolder { get; set; }
        public string SavesFolder { get; set; }
        public string ScreenshotsFolder { get; set; }
        public string ThumbnailsFolder { get; set; }

        public string InstanceThumbnailsFolder { get; set; }

        public List<string> CacheFiles { get; set; } = new();
        public List<string> ThumbnailsFiles { get; set; } = new();
    }





    public class Category
    {
        public string Name { get; set; }
        public Guid Identifier { get; set; } = Guid.NewGuid();
        public string Description { get; set; }
        public Godot.Color Background { get; set; } = Godot.Color.FromHtml("FFFFFF");
        public Godot.Color TextColor { get; set; } = Godot.Color.FromHtml("000000");
        public int Packages { get; set; }
        [XmlIgnore]
        public bool Hidden { get; set; }
        public string FolderLocation { get; set; }

        public void SetFolderLocation(string InstancePackagesFolder)
        {
            FolderLocation = Path.Combine(InstancePackagesFolder, string.Format("__CATEGORY_{0}", Name));

        }
    }

    public class Executable
    {
        public string Path { get; set; }
        public string ExeName { get; set; }
        public string Arguments { get; set; }
        public bool Selected { get; set; } = false;
        public string Name { get; set; }
    }

    public class Instance
    {
        public Guid InstanceID { get; set; }
        public string InstanceLocation { get; set; }
        public string InstanceName { get; set; }
        public DateTime InstanceCreated { get; set; }
        public DateTime InstanceLastModified { get; set; }
        public SimsGames Game { get; set; }
    }

    public class InstanceProfile
    {

        public string ProfileName { get; set; }
        public Guid ProfileIdentifier { get; set; } = Guid.NewGuid();
        public string ProfileDescription { get; set; }
        public string ProfileFolder { get; set; }
        private bool _localsaves;
        public bool LocalSaves
        {
            get { return _localsaves; }
            set
            {
                _localsaves = value;
                if (!Directory.Exists(LocalSaveFolder)) Directory.CreateDirectory(LocalSaveFolder);
            }
        }
        public string LocalSaveFolder { get { return Path.Combine(ProfileFolder, "Saves"); } }

        private bool _localsettings;
        public bool LocalSettings
        {
            get { return _localsettings; }
            set
            {
                _localsettings = value;
                if (!Directory.Exists(LocalSettingsFolder)) Directory.CreateDirectory(LocalSettingsFolder);
            }
        }
        public string LocalSettingsFolder { get { return Path.Combine(ProfileFolder, "Settings"); } }
        private bool _localmedia;
        public bool LocalMedia
        {
            get { return _localmedia; }
            set
            {
                _localmedia = value;
                if (!Directory.Exists(LocalMediaFolder)) Directory.CreateDirectory(LocalMediaFolder);
            }
        }
        public string LocalMediaFolder { get { return Path.Combine(ProfileFolder, "Media"); } }
        private bool _localdata;
        public bool LocalData
        {
            get { return _localdata; }
            set
            {
                _localdata = value;
                if (!Directory.Exists(LocalDataFolder)) Directory.CreateDirectory(LocalDataFolder);
            }
        }
        public string LocalDataFolder { get { return Path.Combine(ProfileFolder, "Data"); } }
        private bool _autobackups;
        public bool AutoBackups
        {
            get { return _autobackups; }
            set
            {
                _autobackups = value;
                if (!Directory.Exists(BackupFolder)) Directory.CreateDirectory(BackupFolder);
            }
        }
        public string BackupFolder { get { return Path.Combine(ProfileFolder, "Backups"); } }

        public List<EnabledPackages> EnabledPackages { get; set; } = new();

        public string XMLFile()
        {
            return Path.Combine(ProfileFolder, string.Format("{0}.xml", SafeFileName()));
        }

        public string SafeFileName()
        {
            var invalids = System.IO.Path.GetInvalidFileNameChars();
            return String.Join("_", ProfileName.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
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
                }
                else
                {
                    stringBuilder.Append(string.Format("{0} (Category: {1}, Load Order: {2})\n", package.PackageName, package.Category, package.LoadOrder));
                }
            }
            if (EnabledPackages.Count != 0)
            {
                return string.Format("Name: {0}, Description: {1}, ID: {2}, Enabled Packages: {3}", this.ProfileName, this.ProfileDescription, this.ProfileIdentifier.ToString(), stringBuilder.ToString());
            }
            else
            {
                return string.Format("Name: {0}, Description: {1}, ID: {2}", this.ProfileName, this.ProfileDescription, this.ProfileIdentifier.ToString());
            }

        }
    }



    public class EnabledPackages
    {
        public string PackageName { get; set; }
        public string PackageLocation { get; set; }
        public Guid PackageIdentifier { get; set; }
        public int LoadOrder { get; set; }
        public Category Category { get; set; }
    }

    public interface ISimsFile
    {
        public string InfoFile { get; }
        public Guid Identifier { get; set; }
        public string FileName { get; set; }
        public string Location { get; set; }
        public string FileSize { get; }
        public FileTypes FileType { get; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        [XmlIgnore]
        public bool Selected { get; set; }

        void WriteXML();
    }

    public class SimsPackage : ISimsFile
    {
        public void InvokeDataChanged(string Data)
        {
            if (AllowInvokeDataChanged) DataChanged?.Invoke(Data);
        }
        [XmlIgnore]
        public bool AllowInvokeDataChanged = true;
        [XmlIgnore]
        public DataChangedEvent DataChanged;
        public Guid Identifier { get; set; } = Guid.NewGuid();
        private string _filename;
        public string FileName
        {
            get { return _filename; }
            set
            {
                _filename = value;
                InvokeDataChanged(nameof(FileName));
            }
        }

        [XmlIgnore]
        public string InfoFile
        {
            get
            {
                return string.Format("{0}.info", Location);
            }
        }
        private string _location;
        public string Location
        {
            get { return _location; }
            set
            {
                _location = value;
                if (!IsDirectory && !RootMod)
                {
                    FileType = ContainerExtensions.TypeFromExtension(new FileInfo(Location).Extension);
                }
                else if (IsDirectory)
                {
                    FileType = FileTypes.Folder;
                }
                DataChanged?.Invoke(nameof(Location));
            }
        }
        public string FileSize
        {
            get
            {
                if (Directory.Exists(Location))
                {
                    return Utilities.SizeSuffix(Utilities.DirSize(new(Location)));
                }
                else if (File.Exists(Location))
                {
                    return Utilities.SizeSuffix(new FileInfo(Location).Length);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public void DeletePackage()
        {
            if (File.Exists(InfoFile))
            {
                Utilities.MoveToRecycleBin(InfoFile);
            }

            if (File.Exists(Location) || Directory.Exists(Location))
            {
                Utilities.MoveToRecycleBin(InfoFile);
            }
        }

        public void MovePackage(string newfolder)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!Directory.Exists(newfolder)) Directory.CreateDirectory(newfolder);
                FileInfo inf = new(InfoFile);
                if (File.Exists(Location))
                {
                    FileInfo fi = new(Location);
                    string newpath = Path.Combine(newfolder, fi.Name);
                    if (File.Exists(newpath))
                    {
                        newpath = Utilities.IncrementName(newpath);
                    }
                    File.Move(Location, newpath);
                    this.Location = newpath;
                }
                else if (Directory.Exists(Location))
                {
                    DirectoryInfo di = new(Location);
                    string newpath = Path.Combine(newfolder, di.Name);
                    if (Directory.Exists(newpath))
                    {
                        newpath = Utilities.IncrementName(newpath, true);
                    }
                    Directory.Move(Location, newpath);
                    this.Location = newpath;

                    List<string> files = Directory.EnumerateFiles(Location, "*.info", System.IO.SearchOption.AllDirectories).ToList();
                    foreach (string file in files)
                    {
                        SimsPackage p = new();
                        XmlSerializer simsPackageSerializer = new XmlSerializer(typeof(SimsPackage));
                        using (FileStream fileStream = new(file, FileMode.Open, System.IO.FileAccess.Read))
                        {
                            using (StreamReader streamReader = new(fileStream))
                            {
                                p = (SimsPackage)simsPackageSerializer.Deserialize(streamReader);
                                streamReader.Close();
                            }
                            fileStream.Close();
                        }
                        string plc = file.Replace(".info", "");
                        p.Location = plc;
                        p.WriteXML();
                    }
                    if (LinkedFiles.Any())
                    {
                        for (int i = 0; i < LinkedFiles.Count; i++)
                        {
                            if (LinkedFiles[i].StartsWith(di.FullName))
                            {
                                FileInfo fl = new(LinkedFiles[i]);
                                LinkedFiles[i] = Path.Combine(Location, fl.Name);
                            }
                        }
                    }
                    if (LinkedFolders.Any())
                    {
                        for (int i = 0; i < LinkedFolders.Count; i++)
                        {
                            if (LinkedFolders[i].StartsWith(di.FullName))
                            {
                                DirectoryInfo fl = new(LinkedFolders[i]);
                                LinkedFolders[i] = Path.Combine(Location, fl.Name);
                            }
                        }
                    }
                }

                if (File.Exists(inf.FullName))
                {
                    string newpath = Path.Combine(newfolder, inf.Name);
                    File.Move(inf.FullName, newpath);
                }
            }
        }


        public Task ReadPackageDetails(GameInstance instance, bool SubFolder = false)
        {
            if (IsDirectory)
            {
                return ReadPackageDetails(instance, new DirectoryInfo(Location), SubFolder);
            }
            else
            {
                return ReadPackageDetails(instance, new FileInfo(Location), SubFolder);
            }
        }

        public Task ReadPackageDetails(GameInstance instance, FileInfo fi, bool SubFolder)
        {
            SimsPackageReader simsPackageReader = new();
            try
            {
                simsPackageReader.ReadPackage(Location);
                PackageData = simsPackageReader.SimsData;
                if (simsPackageReader.PackageGame != instance.GameChoice)
                {
                    if (simsPackageReader.PackageGame == SimsGames.Sims3 && instance.GameChoice == SimsGames.SimsMedieval)
                    {
                        Game = SimsGames.SimsMedieval;
                        WrongGame = false;
                    }
                    else
                    {
                        Game = simsPackageReader.PackageGame;
                        WrongGame = true;
                    }
                }
            }
            catch (Exception e)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Couldn't read package {0}: {1} ({2})", FileName, e.Message, e.StackTrace));
            }
            PackageCategory = instance.Categories.First(x => x.Name == "Default");
            simsPackageReader.Dispose();
            DateCreated = File.GetCreationTime(Location);
            DateUpdated = DateTime.Now;
            simsPackageReader.CheckOverrides(this);
            if (!Orphan) HasBeenRead = true;
            //this.WriteXML();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Returning {0} as read.", FileName));
            return Task.CompletedTask;
        }

        public Task ReadPackageDetails(GameInstance instance, DirectoryInfo Dir, bool SubFolder = false)
        {
            IsDirectory = true;
            if (!SubFolder)
            {
                StandAlone = true;
            }
            else
            {
                StandAlone = false;
            }
            FileName = Dir.Name;
            Game = instance.GameChoice;
            switch (Game)
            {
                case SimsGames.Sims2:
                    Sims2Data = new();
                    break;
                case SimsGames.Sims3:
                    Sims3Data = new();
                    break;
                case SimsGames.Sims4:
                    Sims4Data = new();
                    break;
            }
            DateCreated = Directory.GetCreationTime(Location);
            DateUpdated = DateTime.Now;
            PackageCategory = instance.Categories.First(x => x.Name == "Default");
            HasBeenRead = true;
            //this.WriteXML();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Returning {0} as read.", FileName));
            return Task.CompletedTask;
        }

        private FileTypes _filetype;
        public FileTypes FileType
        {
            get { return _filetype; }
            set
            {
                _filetype = value;
                DataChanged?.Invoke(nameof(FileType));
            }
        }
        private DateTime _datecreated;
        public DateTime DateCreated
        {
            get { return _datecreated; }
            set
            {
                _datecreated = value;
                DataChanged?.Invoke(nameof(DateCreated));
            }
        }
        private DateTime _dateupdated;
        public DateTime DateUpdated
        {
            get { return _dateupdated; }
            set
            {
                _dateupdated = value;
                DataChanged?.Invoke(nameof(DateUpdated));
            }
        }
        private string _installedforversion;
        public string InstalledForVersion
        {
            get { return _installedforversion; }
            set
            {
                _installedforversion = value;
                DataChanged?.Invoke(nameof(InstalledForVersion));
            }
        }

        private string _source;
        public string Source
        {
            get { return _source; }
            set
            {
                _source = value;
                DataChanged?.Invoke(nameof(Source));
            }
        }
        private string _creator;
        public string Creator
        {
            get { return _creator; }
            set
            {
                _creator = value;
                DataChanged?.Invoke(nameof(Creator));
            }
        }
        private string _notes;
        public string Notes
        {
            get { return _notes; }
            set
            {
                _notes = value;
                DataChanged?.Invoke(nameof(Notes));
            }
        }
        [XmlIgnore]
        public bool Selected { get; set; }
        private bool _isdirectory;
        public bool IsDirectory
        {
            get { return _isdirectory; }
            set
            {
                _isdirectory = value;
                if (value)
                {
                    FileType = FileTypes.Folder;
                }
                DataChanged?.Invoke(nameof(IsDirectory));
            }
        }
        public bool StandAlone { get; set; }
        private bool _broken;
        public bool Broken
        {
            get { return _broken; }
            set
            {
                _broken = value;
                DataChanged?.Invoke(nameof(Broken));
            }
        }
        private bool _wronggame;
        public bool WrongGame
        {
            get { return _wronggame; }
            set
            {
                _wronggame = value;
                DataChanged?.Invoke(nameof(WrongGame));
            }
        }
        public bool Override
        {
            get
            {
                switch (Game)
                {
                    case SimsGames.Sims2:
                        return Sims2Data.Override;
                    case SimsGames.Sims3:
                        return Sims3Data.Override;
                    case SimsGames.Sims4:
                        return Sims4Data.Override;
                    default: return false;
                }
            }
            set
            {
                switch (Game)
                {
                    case SimsGames.Sims2:
                        Sims2Data.Override = value;
                        break;
                    case SimsGames.Sims3:
                        Sims3Data.Override = value;
                        break;
                    case SimsGames.Sims4:
                        Sims4Data.Override = value;
                        break;
                }
            }
        }
        public ObservableCollection<SimsOverrides> OverrideReference
        {
            get
            {
                switch (Game)
                {
                    case SimsGames.Sims2:
                        return Sims2Data.OverrideReference;
                    case SimsGames.Sims3:
                        return Sims3Data.OverrideReference;
                    case SimsGames.Sims4:
                        return Sims4Data.OverrideReference;
                    default: return null;
                }
            }
            set
            {
                switch (Game)
                {
                    case SimsGames.Sims2:
                        Sims2Data.OverrideReference = value;
                        break;
                    case SimsGames.Sims3:
                        Sims3Data.OverrideReference = value;
                        break;
                    case SimsGames.Sims4:
                        Sims4Data.OverrideReference = value;
                        break;
                }
                value.CollectionChanged += (x, y) => { DataChanged?.Invoke(nameof(OverrideReference)); };
            }
        }
        public SpecificOverrides SpecificOverride
        {
            get
            {
                switch (Game)
                {
                    case SimsGames.Sims2:
                        return Sims2Data.SpecificOverride;
                    case SimsGames.Sims3:
                        return Sims3Data.SpecificOverride;
                    case SimsGames.Sims4:
                        return Sims4Data.SpecificOverride;
                    default: return null;
                }
            }
            set
            {
                switch (Game)
                {
                    case SimsGames.Sims2:
                        Sims2Data.SpecificOverride = value;
                        break;
                    case SimsGames.Sims3:
                        Sims3Data.SpecificOverride = value;
                        break;
                    case SimsGames.Sims4:
                        Sims4Data.SpecificOverride = value;
                        break;
                }
                DataChanged?.Invoke(nameof(SpecificOverride));
            }
        }
        [XmlIgnore]
        public string OverrideRef { get { if (SpecificOverride != null) return string.Format("{0}: {1}", Type, SpecificOverride.Description); else return string.Empty; } }

        private bool _isduplicate;
        public bool IsDuplicate
        {
            get { return _isduplicate; }
            set
            {
                _isduplicate = value;
                DataChanged?.Invoke(nameof(IsDuplicate));
            }
        }
        private ObservableCollection<string> _duplicates;
        public ObservableCollection<string> Duplicates
        {
            get { return _duplicates; }
            set
            {
                _duplicates = value;
                value.CollectionChanged += (x, y) => { DataChanged?.Invoke(nameof(Duplicates)); };
            }
        }
        private ObservableCollection<string> _conflicts;
        public ObservableCollection<string> Conflicts
        {
            get { return _conflicts; }
            set
            {
                _conflicts = value;
                value.CollectionChanged += (x, y) => { DataChanged?.Invoke(nameof(Conflicts)); };
            }
        }

        private string _matchingmesh;
        public string MatchingMesh
        {
            get { return _matchingmesh; }
            set
            {
                _matchingmesh = value;
                DataChanged?.Invoke(nameof(MatchingMesh));
            }
        }
        private ObservableCollection<string> _matchingrecolors;
        public ObservableCollection<string> MatchingRecolors
        {
            get { return _matchingrecolors; }
            set
            {
                _matchingrecolors = value;
                value.CollectionChanged += (x, y) => { DataChanged?.Invoke(nameof(MatchingRecolors)); };
            }
        }

        [XmlIgnore]
        public string Type
        {
            get
            {
                switch (Game)
                {
                    case SimsGames.Sims2:
                        if (Sims2Data != null)
                        {
                            if (Sims2Data.FunctionSort.Any())
                            {
                                if (Sims2Data.FunctionSort[0] != null)
                                {
                                    if (!string.IsNullOrEmpty(Sims2Data.FunctionSort[0].Subcategory))
                                    {
                                        return string.Format("{0}/{1}", Sims2Data.FunctionSort[0].Category, Sims2Data.FunctionSort[0].Subcategory);
                                    }
                                    else
                                    {
                                        return Sims2Data.FunctionSort[0].Category;
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(Sims2Data.AltType))
                            {
                                return Sims2Data.AltType;
                            }
                        }
                        else return string.Empty;
                        break;
                    case SimsGames.Sims3:
                        if (Sims3Data != null)
                        {
                            if (Sims3Data.FunctionSort.Any())
                            {
                                if (!string.IsNullOrEmpty(Sims3Data.FunctionSort[0].Subcategory))
                                {
                                    return string.Format("{0}/{1}", Sims3Data.FunctionSort[0].Category, Sims3Data.FunctionSort[0].Subcategory);
                                }
                                else
                                {
                                    return Sims3Data.FunctionSort[0].Category;
                                }
                            }

                            if (!string.IsNullOrEmpty(Sims3Data.AltType))
                            {
                                return Sims3Data.AltType;
                            }
                        }
                        else return string.Empty;
                        break;
                    case SimsGames.Sims4:
                        if (Sims4Data != null)
                        {
                            if (Sims4Data.FunctionSort.Any())
                            {
                                if (!string.IsNullOrEmpty(Sims4Data.FunctionSort[0].Subcategory))
                                {
                                    return string.Format("{0}/{1}", Sims4Data.FunctionSort[0].Category, Sims4Data.FunctionSort[0].Subcategory);
                                }
                                else
                                {
                                    return Sims4Data.FunctionSort[0].Category;
                                }
                            }

                            if (!string.IsNullOrEmpty(Sims4Data.AltType))
                            {
                                return Sims4Data.AltType;
                            }
                        }
                        else return string.Empty;
                        break;
                }
                if (HasBeenRead) return "Unknown"; else return string.Empty;

            }
        }

        private string _packagegameversion;
        public string PackageGameVersion
        {
            get { return _packagegameversion; }
            set
            {
                _packagegameversion = value;
                DataChanged?.Invoke(nameof(PackageGameVersion));
            }
        }
        private ObservableCollection<string> _linkedfiles;
        public ObservableCollection<string> LinkedFiles
        {
            get { return _linkedfiles; }
            set
            {
                _linkedfiles = value;
                value.CollectionChanged += (x, y) => { DataChanged?.Invoke(nameof(LinkedFiles)); };
            }
        }
        private ObservableCollection<string> _linkedfolders;
        public ObservableCollection<string> LinkedFolders
        {
            get { return _linkedfolders; }
            set
            {
                _linkedfolders = value;
                value.CollectionChanged += (x, y) => { DataChanged?.Invoke(nameof(LinkedFolders)); };
            }
        }
        [XmlIgnore]
        public List<SimsPackage> LinkedPackages { get; set; } = new();
        [XmlIgnore]
        public List<SimsPackage> LinkedPackageFolders { get; set; } = new();
        private Category _packagecategory;
        public Category PackageCategory
        {
            get { return _packagecategory; }
            set
            {
                _packagecategory = value;
                DataChanged?.Invoke(nameof(PackageCategory));
            }
        }
        public string CategoryName { get { return PackageCategory.Name; } }
        public Godot.Color CategoryColor { get { return PackageCategory.Background; } }

        public string Image { get; set; }
        private SimsGames _game;
        public SimsGames Game
        {
            get { return _game; }
            set
            {
                _game = value;
                switch (value)
                {
                    case SimsGames.Sims2:
                        Sims2Data = new();
                        break;
                    case SimsGames.Sims3:
                        Sims3Data = new();
                        break;
                    case SimsGames.Sims4:
                        Sims4Data = new();
                        break;
                }
            }
        }
        private bool _rootmod;
        public bool RootMod
        {
            get { return _rootmod; }
            set
            {
                _rootmod = value;
                if (value)
                {
                    FileType = FileTypes.Root;
                }
                else
                {
                    Location = _location;
                }
                DataChanged?.Invoke(nameof(RootMod));
            }
        }
        public bool OutOfDate { get; set; }
        public bool GameMod
        {
            get
            {
                if (PackageData != null)
                {
                    if (Game == SimsGames.Sims2)
                    {
                        return Sims2Data.GameMod;
                    }
                    else if (Game == SimsGames.Sims3)
                    {
                        return Sims3Data.GameMod;
                    }
                    else if (Game == SimsGames.Sims4)
                    {
                        return Sims4Data.GameMod;
                    }
                    else
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
                    Sims2Data.GameMod = value;
                }
                else if (Game == SimsGames.Sims3)
                {
                    Sims3Data.GameMod = value;
                }
                else if (Game == SimsGames.Sims4)
                {
                    Sims4Data.GameMod = value;
                }
                else
                {
                    return;
                }
                DataChanged?.Invoke(nameof(GameMod));
            }
        }
        public bool Mesh
        {
            get
            {
                if (PackageData != null)
                {
                    if (Game == SimsGames.Sims2)
                    {
                        return Sims2Data.Mesh;
                    }
                    else if (Game == SimsGames.Sims3)
                    {
                        return Sims3Data.Mesh;
                    }
                    else if (Game == SimsGames.Sims4)
                    {
                        return Sims4Data.Mesh;
                    }
                    else
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
                }
                else if (Game == SimsGames.Sims3)
                {
                    Sims3Data.Mesh = value;
                }
                else if (Game == SimsGames.Sims4)
                {
                    Sims4Data.Mesh = value;
                }
                else
                {
                    return;
                }
                DataChanged?.Invoke(nameof(Mesh));
            }

        }
        public bool Recolor
        {
            get
            {
                if (PackageData != null)
                {
                    if (Game == SimsGames.Sims2)
                    {
                        return Sims2Data.Recolor;
                    }
                    else if (Game == SimsGames.Sims3)
                    {
                        return Sims3Data.Recolor;
                    }
                    else if (Game == SimsGames.Sims4)
                    {
                        return Sims4Data.Recolor;
                    }
                    else
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
                    Sims2Data.Recolor = value;
                }
                else if (Game == SimsGames.Sims3)
                {
                    Sims3Data.Recolor = value;
                }
                else if (Game == SimsGames.Sims4)
                {
                    Sims4Data.Recolor = value;
                }
                else
                {
                    return;
                }
                DataChanged?.Invoke(nameof(Recolor));
            }
        }
        public bool Orphan
        {
            get
            {
                if (PackageData != null)
                {
                    if (Game == SimsGames.Sims2)
                    {
                        return Sims2Data.Orphan;
                    }
                    else if (Game == SimsGames.Sims3)
                    {
                        return Sims3Data.Orphan;
                    }
                    else if (Game == SimsGames.Sims4)
                    {
                        return Sims4Data.Orphan;
                    }
                    else
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
                    Sims2Data.Orphan = value;
                }
                else if (Game == SimsGames.Sims3)
                {
                    Sims3Data.Orphan = value;
                }
                else if (Game == SimsGames.Sims4)
                {
                    Sims4Data.Orphan = value;
                }
                else
                {
                    return;
                }
                DataChanged?.Invoke(nameof(Orphan));
            }
        }
        public string ObjectGUID
        {
            get
            {
                switch (Game)
                {
                    case SimsGames.Sims2:
                        return Sims2Data.GUID;
                    case SimsGames.Sims3:
                        return Sims3Data.GUID;
                    case SimsGames.Sims4:
                        return Sims4Data.GUID;
                    default:
                        return string.Empty;
                }
            }
            set
            {
                switch (Game)
                {
                    case SimsGames.Sims2:
                        Sims2Data.GUID = value;
                        break;
                    case SimsGames.Sims3:
                        Sims3Data.GUID = value;
                        break;
                    case SimsGames.Sims4:
                        Sims4Data.GUID = value;
                        break;
                }
                DataChanged?.Invoke(nameof(ObjectGUID));
            }
        }
        private bool _favorite;
        public bool Favorite
        {
            get { return _favorite; }
            set
            {
                _favorite = value;
                DataChanged?.Invoke(nameof(Favorite));
            }
        }
        [XmlIgnore]
        public bool IsEnabled { get; set; }
        [XmlIgnore]
        private int _loadorder;
        [XmlIgnore]
        public int LoadOrder
        {
            get { return _loadorder; }
            set
            {
                _loadorder = value;
                DataChanged?.Invoke(nameof(LoadOrder));
            }
        }

        [XmlIgnore]
        private ISimsData _packagedata;
        [XmlIgnore]
        public ISimsData PackageData
        {
            get
            {
                if (Game == SimsGames.Sims2)
                {
                    return _packagedata as Sims2Data;
                }
                else if (Game == SimsGames.Sims3)
                {
                    return _packagedata as Sims3Data;
                }
                else if (Game == SimsGames.Sims4)
                {
                    return _packagedata as Sims4Data;
                }
                else
                {
                    return _packagedata;
                }
            }
            set
            {
                _packagedata = value;
                DataChanged?.Invoke(nameof(Type));
                DataChanged?.Invoke(nameof(Override));
                DataChanged?.Invoke(nameof(OverrideReference));
                DataChanged?.Invoke(nameof(SpecificOverride));
            }
        }

        [XmlIgnore]
        public Image PackageImage
        {
            get
            {
                if (Game == SimsGames.Sims2)
                {
                    if (Sims2Data.TXTRDataBlock != null)
                    {
                        if (Sims2Data.TXTRDataBlock.Count > 0)
                        {
                            if (Sims2Data.TXTRDataBlock[0].Texture != null)
                            {
                                return Sims2Data.TXTRDataBlock[0].Texture;
                            }
                        }
                    }
                }
                return null;
            }
        }


        public Sims2Data Sims2Data { get { return PackageData as Sims2Data; } set { PackageData = value; } }
        public Sims3Data Sims3Data { get { return PackageData as Sims3Data; } set { PackageData = value; } }
        public Sims4Data Sims4Data { get { return PackageData as Sims4Data; } set { PackageData = value; } }

        [XmlIgnore]
        private  bool _hasbeenread;
        public  bool HasBeenRead 
        {
            get { return  _hasbeenread; }
            set {  _hasbeenread = value; 
            DataChanged?.Invoke(nameof(Type)); }
        }

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

        public void UpdateFromData(SimsPackage package)
        {
            Broken = package.Broken;
            Game = package.Game;
            Source = package.Source;
            Creator = package.Creator;
            Notes = package.Notes;
            WrongGame = package.WrongGame;
            IsDuplicate = package.IsDuplicate;
            IsDirectory = package.IsDirectory;
            Duplicates = package.Duplicates;
            Conflicts = package.Conflicts;
            //MatchingMesh = package.MatchingMesh;
            //MatchingRecolors = package.MatchingRecolors;
            PackageGameVersion = package.PackageGameVersion;
            LinkedFiles = package.LinkedFiles;
            LinkedFolders = package.LinkedFolders;
            LinkedPackages = package.LinkedPackages;
            LinkedPackageFolders = package.LinkedPackageFolders;
            PackageCategory = package.PackageCategory;
            RootMod = package.RootMod;
            OutOfDate = package.OutOfDate;
            Favorite = package.Favorite;
            IsEnabled = package.IsEnabled;
            LoadOrder = package.LoadOrder;
            HasBeenRead = package.HasBeenRead;
        }

        public void UpdateFromOrphanCheck(SimsPackage package)
        {
            if (!package.Orphan) Orphan = package.Orphan;
            if (string.IsNullOrEmpty(MatchingMesh) && !string.IsNullOrEmpty(package.MatchingMesh)) MatchingMesh = package.MatchingMesh;
            foreach (string mr in package.MatchingRecolors)
            {
                MatchingRecolors.Add(mr);
            }
            if (Sims2Data.FunctionSort.Count == 0) Sims2Data.FunctionSort = package.Sims2Data.FunctionSort;
            if (string.IsNullOrEmpty(Sims2Data.AltType)) Sims2Data.AltType = package.Sims2Data.AltType;
        }


        public void WriteXML()
        {
            XmlSerializer InfoSerializer = new XmlSerializer(this.GetType());
            if (GlobalVariables.DebugMode)
            {
                try
                {
                    locker.AcquireWriterLock(int.MaxValue);
                    if (File.Exists(this.InfoFile))
                    {
                        File.Delete(this.InfoFile);
                    }
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
            else
            {
                if (File.Exists(this.InfoFile))
                {
                    File.Delete(this.InfoFile);
                }

                using (FileStream fs = File.Create(this.InfoFile))
                using (ZipOutputStream zipStream = new ZipOutputStream(fs))
                {
                    // Add a file entry to the ZIP archive
                    ZipEntry entry = new ZipEntry(this.FileName)
                    {
                        DateTime = this.DateUpdated
                    };

                    zipStream.PutNextEntry(entry);

                    // Write file data to the ZIP stream


                    byte[] buffer = new byte[4096];
                    using (MemoryStream memoryStream = new())
                    {
                        InfoSerializer.Serialize(memoryStream, this);
                        int bytesRead;
                        while ((bytesRead = memoryStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            zipStream.Write(buffer, 0, bytesRead);
                        }
                    }
                    zipStream.CloseEntry();
                }
            }
        }

        public void SetProperty(string propName, dynamic input)
        {
            var prop = this.ProcessProperty(propName);
            PropertyInfo property = this.GetType().GetProperty(propName);
            if (property != null)
            {
                if (property.PropertyType == typeof(Godot.Color))
                {
                    Godot.Color newcolor = Godot.Color.FromHtml(input);
                    property.SetValue(this, newcolor);
                }
                else if (property.PropertyType == typeof(Guid))
                {
                    if (input.GetType() == typeof(string))
                    {
                        string inp = input as string;
                        property.SetValue(this, Guid.Parse(inp));
                    }
                    else if (input.GetType() == typeof(Guid))
                    {
                        property.SetValue(this, input);
                    }
                }
                else if (property.PropertyType == typeof(string))
                {
                    property.SetValue(this, input as string);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    if (input.GetType() == typeof(bool))
                    {
                        property.SetValue(this, input);
                    }
                    else if (input.GetType() == typeof(string))
                    {
                        property.SetValue(this, bool.Parse(input));
                    }
                }
                else if (property.PropertyType == typeof(SimsGames))
                {

                }
            }
        }

        public object ProcessProperty(string propName)
        {
            return this.GetType().GetProperty(propName).GetValue(this, null);
        }

    }


    public interface ISimsData
    {
        public string FileLocation { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; }
        public string AltType { get; set; }

        public List<FunctionSortList> FunctionSort { get; set; }
        public string GUID { get; set; }
        public List<IndexEntry> IndexEntries { get; set; }
        public List<EntryCount> IndexEntryCounts { get; set; }

        public bool Recolor { get; set; }
        public bool Mesh { get; set; }
        public bool Orphan { get; set; }
        public bool GameMod { get; set; }


        public bool Override { get; set; }
        public ObservableCollection<SimsOverrides> OverrideReference { get; set; }
        public SpecificOverrides SpecificOverride { get; set; }


        public ClothingInfo ClothingInfo { get; set; }


        void Serialize();
        void GetPackageType();

        int EntryCount(string entry);

        void IsGameMod();

        public int EntryTypes();
        public void DictionaryEntries();

        public delegate void DataChangedEvent(string DataType);
    }

    public class ClothingInfo
    {
        public string Type { get; set; } //top bottom full
        public List<string> Category { get; set; } = new(); //formal atletic etc
        public List<string> Gender { get; set; } = new();
        public List<string> Age { get; set; } = new();
        public List<string> Species { get; set; } = new();
    }

    public class Sims2Data : ISimsData
    {
        [XmlIgnore]
        public DataChangedEvent DataChanged;
        [XmlIgnore]
        public string FileLocation { get; set; }
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                DataChanged?.Invoke(nameof(Title));
            }
        }
        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                DataChanged?.Invoke(nameof(Description));
            }
        }
        public string Type
        {
            get
            {
                if (!string.IsNullOrEmpty(FunctionSort[0].Subcategory))
                {
                    return string.Format("{0}/{1}", FunctionSort[0].Category, FunctionSort[0].Subcategory);
                }
                else if (!string.IsNullOrEmpty(FunctionSort[0].Category))
                {
                    return FunctionSort[0].Category;
                }
                else
                {
                    return AltType;
                }
            }
        }
        private string _alttype;
        public string AltType
        {
            get { return _alttype; }
            set
            {
                _alttype = value;
                DataChanged?.Invoke(nameof(AltType));
            }
        }

        private bool _override;
        public bool Override
        {
            get { return _override; }
            set
            {
                _override = value;
                DataChanged?.Invoke(nameof(Override));
            }
        }
        private ObservableCollection<SimsOverrides> _overriderefrence;
        public ObservableCollection<SimsOverrides> OverrideReference
        {
            get { return _overriderefrence; }
            set
            {
                _overriderefrence = value;
                OverrideReference.CollectionChanged += (x, y) => { DataChanged?.Invoke(nameof(OverrideReference)); };
            }
        }
        private SpecificOverrides _specificoverride;
        public SpecificOverrides SpecificOverride
        {
            get { return _specificoverride; }
            set
            {
                _specificoverride = value;
                DataChanged?.Invoke(nameof(SpecificOverride));
            }
        }

        public List<FunctionSortList> FunctionSort { get; set; } = new();
        private string _guid;
        public string GUID
        {
            get
            {
                if (!string.IsNullOrEmpty(_guid))
                {
                    if (_guid.StartsWith("0x"))
                    {
                        return _guid.TrimPrefix("0x");
                    }
                    else
                    {
                        return _guid;
                    }
                }
                else
                {
                    return string.Empty;
                }

            }
            set
            {
                _guid = value;
            }
        }
        private bool _recolor;
        public bool Recolor
        {
            get { return _recolor; }
            set
            {
                _recolor = value;
                DataChanged?.Invoke(nameof(Recolor));
            }
        }
        private bool _mesh;
        public bool Mesh
        {
            get { return _mesh; }
            set
            {
                _mesh = value;
                DataChanged?.Invoke(nameof(Mesh));
            }
        }
        private bool _orphan;
        public bool Orphan
        {
            get { return _orphan; }
            set
            {
                _orphan = value;
                DataChanged?.Invoke(nameof(Orphan));
            }
        }
        private bool _gamemod;
        public bool GameMod
        {
            get { return _gamemod; }
            set
            {
                _gamemod = value;
                DataChanged?.Invoke(nameof(GameMod));
            }
        }

        public List<Sims2Expansions> Expansions { get; set; } = new();

        public ClothingInfo ClothingInfo { get; set; } = new();

        public List<IndexEntry> IndexEntries { get; set; } = new();


        public void DictionaryEntries()
        {
            ConcurrentBag<EntryCount> entryCounts = new();
            if (GlobalVariables.DebugMode && SimsPackageReader.DebugPackageReader) Logging.WriteDebugLog(string.Format("Checking {0} indexentries against {1} types", IndexEntries.Count, Sims2PackageStatics.Sims2EntryTypes.Count));
            Parallel.ForEach(Sims2PackageStatics.Sims2EntryTypes, entryType =>
            {
                int count = IndexEntries.Count(x => x.TypeID == entryType.TypeID);
                if (GlobalVariables.DebugMode && SimsPackageReader.DebugPackageReader) Logging.WriteDebugLog(string.Format("File has {0} of entry {1}", count, entryType.Tag));
                if (count != 0) entryCounts.Add(new() { EntryTag = entryType.Tag.ToUpper(), Count = count });
            });
            IndexEntryCounts = entryCounts.ToList();
        }
        [XmlIgnore]
        public List<EntryCount> IndexEntryCounts { get; set; } = new();
        public List<CTSSData> CTSSDataBlock { get; set; } = new();
        public List<OBJDData> OBJDDataBlock { get; set; } = new();
        public List<XOBJData> XOBJDataBlock { get; set; } = new();
        public List<GMDCData> GMDCDataBlock { get; set; } = new();
        public List<MMATData> MMATDataBlock { get; set; } = new();
        public List<XFLRData> XFLRDataBlock { get; set; } = new();
        public List<TXTRData> TXTRDataBlock { get; set; } = new();
        public List<XNGBData> XNGBDataBlock { get; set; } = new();
        public List<GZPSData> GZPSDataBlock { get; set; } = new();
        public List<EIDRData> EIDRDataBlock { get; set; } = new();
        public List<SHPEData> SHPEDataBlock { get; set; } = new();
        public List<TXMTData> TXMTDataBlock { get; set; } = new();
        public List<XHTNData> XHTNDataBlock { get; set; } = new();
        public List<XTOLData> XTOLDataBlock { get; set; } = new();
        public List<GMNDData> GMNDDataBlock { get; set; } = new();
        public List<XMOLData> XMOLDataBlock { get; set; } = new();


        public void GetPackageType()
        {
            if (EntryCount("gmdc") > 0) Mesh = true;
            if (EntryCount("txtr") > 0) Recolor = true;
            if ((Mesh && !Recolor) || (!Recolor && Mesh)) Orphan = true;
            if (FunctionSort.Count > 0) return;

            if (IsMod())
            {
                IsGameMod();
                Orphan = false;
                AltType = "Game Mod/Hack";
            }
            else if (EntryCount("xmol") > 0)
            {
                AltType = "Accessory";
                FunctionSort.Clear();
                if (Mesh && !Recolor) Orphan = true;
            }
            else if (EntryCount("xhtn") > 0)
            {
                AltType = "Hair";
                FunctionSort.Clear();
                if (Mesh && !Recolor) Orphan = true;
            }
            else if (EntryCount("xstn") > 0 || TXMTDataBlock.Any(x => x.MaterialDescription.Contains("naked_nude_")))
            {
                AltType = "Skin";
                FunctionSort.Clear();
                Mesh = false;
                Recolor = false;
                Orphan = false;
            }
            else if (EntryCount("coll") > 0)
            {
                AltType = "Collection";
                FunctionSort.Clear();
                Mesh = false;
                Recolor = false;
                Orphan = false;
            }
            else if (EntryCount("xngb") > 0)
            {
                AltType = "Hood Deco";
                if (Mesh && !Recolor) Orphan = true;
                FunctionSort.Clear();
            }
            else if (EntryCount("lxnr") > 0
            && EntryCount("AGED") > 0
            && EntryCount("3IDR") > 0
            && EntryCount("IMG") == 0
            && EntryCount("gzps") > 0
            && EntryCount("binx") > 0
            && EntryCount("txmt") > 0)
            {
                AltType = "Face Template";
                FunctionSort.Clear();
                Mesh = true;
                Recolor = false;
                Orphan = false;
            }
            else if (EntryCount("lxnr") > 0
            && EntryCount("AGED") > 0
            && EntryCount("3IDR") > 0
            && EntryCount("IMG") > 0
            && EntryCount("gzps") > 0
            && EntryCount("binx") > 0
            && EntryCount("txmt") > 0
            && EntryCount("bhav") > 0)
            {
                AltType = "NPC";
                FunctionSort.Clear();
            }
            if (EntryCount("gmdc") > 0
            && EntryCount("gmnd") > 0
            && EntryCount("xfmd") > 0
            && EntryCount("coll") == 0)
            {
                AltType = "Slider";
                FunctionSort.Clear();
                Mesh = false;
                Recolor = false;
                Orphan = false;
            }
        }


        private bool IsMod()
        {
            if (EntryCount("bhav") > 0
            && EntryCount("dir") == 1
            && EntryCount("gmdc") == 0)
            {
                return true;
            }
            else if (EntryCount("bhav") > 0
            && EntryCount("bcon") > 0
            && EntryCount("dir") == 0
            && EntryCount("gmdc") == 0
            && EntryCount("objd") == 0)
            {
                return true;
            }
            else if (EntryCount("bhav") > 0
            && EntryCount("nref") > 0
            && EntryCount("objd") > 0
            && EntryCount("objf") > 0
            && EntryCount("slot") > 0
            && EntryTypes() == 5)
            {
                return true;
            }
            else if (EntryCount("ttab") > 0
            && EntryCount("ttas") > 0
            && EntryTypes() == 2)
            {
                return true;
            }
            else if (EntryCount("bhav") > 0
            && EntryCount("ttas") > 0
            && EntryCount("ttab") > 0
            && EntryTypes() == 3)
            {
                return true;
            }
            else if (EntryCount("bhav") > 0
            && EntryCount("ctss") > 0
            && EntryCount("STR#") > 0
            && EntryTypes() == 3)
            {
                return true;
            }
            else if (EntryCount("bcon") > 0
            && EntryTypes() == 1)
            {
                return true;
            }
            else if (EntryCount("bhav") > 0
            && EntryTypes() == 1)
            {
                return true;
            }
            else if (EntryCount("bhav") > 0
            && EntryCount("objf") > 0
            && EntryTypes() == 2)
            {
                return true;
            }
            else if (EntryCount("ttab") > 0
            && EntryTypes() == 1)
            {
                return true;
            }
            else if (EntryCount("cres") > 0
            && EntryCount("STR#") > 0
            && EntryTypes() == 2)
            {
                return true;
            }
            else if (EntryCount("STR#") > 0
            && EntryTypes() == 1)
            {
                return true;
            }
            else if (EntryCount("objd") > 0
            && EntryCount("objf") > 0
            && EntryCount("slot") > 0
            && EntryCount("nref") > 0
            && EntryTypes() == 4)
            {
                return true;
            }
            else if (EntryCount("bhav") > 0
            && EntryCount("ttab") > 0
            && EntryCount("ttas") > 0
            && EntryCount("STR#") > 0
            && EntryTypes() == 4)
            {
                return true;
            }
            else if (EntryCount("bhav") > 0
            && EntryCount("STR#") > 0
            && EntryCount("ttab") > 0
            && EntryCount("ttas") > 0
            && EntryCount("anim") > 0
            && EntryTypes() == 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }







        public int EntryCount(string entry)
        {
            if (IndexEntryCounts.Any(x => x.EntryTag.Equals(entry, StringComparison.CurrentCultureIgnoreCase)))
            {
                return IndexEntryCounts.First(x => x.EntryTag.Equals(entry, StringComparison.CurrentCultureIgnoreCase)).Count;
            }
            else
            {
                return 0;
            }
        }

        public int EntryTypes()
        {
            return IndexEntryCounts.Count;
        }

        public string WriteEntryList()
        {
            StringBuilder sb = new();
            foreach (EntryCount entry in IndexEntryCounts.Where(x => x.Count > 0))
            {
                sb.AppendLine(string.Format("{0}: {1}", entry.EntryTag, entry.Count));
            }
            string result = sb.ToString();
            return result;
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
                    }
                    else
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
        public string FileLocation { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type
        {
            get
            {
                if (!string.IsNullOrEmpty(FunctionSort[0].Subcategory))
                {
                    return string.Format("{0}/{1}", FunctionSort[0].Category, FunctionSort[0].Subcategory);
                }
                else
                {
                    return FunctionSort[0].Category;
                }
            }
        }
        public string AltType { get; set; } = "";


        public bool Override { get; set; }
        public ObservableCollection<SimsOverrides> OverrideReference { get; set; } = new();
        public SpecificOverrides SpecificOverride { get; set; }

        public List<FunctionSortList> FunctionSort { get; set; } = new();
        private List<IndexEntry> _indexentries;
        public List<IndexEntry> IndexEntries
        {
            get { return _indexentries; }
            set
            {
                _indexentries = value;
                /*foreach (EntryType entryType in Sims2PackageStatics.Sims2EntryTypes)
                {
                    IndexEntryCounts.Add(entryType.Tag, value.Count(x => x.TypeID == entryType.TypeID));
                }*/
            }
        }
        public List<EntryCount> IndexEntryCounts { get; set; }
        public string GUID { get; set; } = "";
        public bool Recolor { get; set; }
        public bool Mesh { get; set; }
        public bool Orphan { get; set; }
        public bool GameMod { get; set; }

        public List<Sims3Expansions> Expansions { get; set; } = new();

        public ClothingInfo ClothingInfo { get; set; } = new();
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

        public void GetPackageType()
        {
            throw new NotImplementedException();
        }
    }

    public class Sims4Data : ISimsData
    {
        public string FileLocation { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type
        {
            get
            {
                if (!string.IsNullOrEmpty(FunctionSort[0].Subcategory))
                {
                    return string.Format("{0}/{1}", FunctionSort[0].Category, FunctionSort[0].Subcategory);
                }
                else
                {
                    return FunctionSort[0].Category;
                }
            }
        }
        public string AltType { get; set; } = "";


        public bool Override { get; set; }
        public ObservableCollection<SimsOverrides> OverrideReference { get; set; } = new();
        public SpecificOverrides SpecificOverride { get; set; }

        public List<FunctionSortList> FunctionSort { get; set; } = new();
        private List<IndexEntry> _indexentries;
        public List<IndexEntry> IndexEntries
        {
            get { return _indexentries; }
            set
            {
                _indexentries = value;
                /*foreach (EntryType entryType in Sims2PackageStatics.Sims2EntryTypes)
                {
                    IndexEntryCounts.Add(entryType.Tag, value.Count(x => x.TypeID == entryType.TypeID));
                }*/
            }
        }
        public List<EntryCount> IndexEntryCounts { get; set; }
        public string GUID { get; set; } = "";
        public bool Recolor { get; set; }
        public bool Mesh { get; set; }
        public bool Orphan { get; set; }
        public bool GameMod { get; set; }

        public List<Sims4Expansions> Expansions { get; set; } = new();

        public ClothingInfo ClothingInfo { get; set; } = new();
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

        public void GetPackageType()
        {
            throw new NotImplementedException();
        }
    }








    public class SimsDownload : ISimsFile
    {

        public Guid Identifier { get; set; } = Guid.NewGuid();
        public string FileName { get; set; }
        public string InfoFile
        {
            get
            {
                return string.Format("{0}.info", new FileInfo(Location).FullName);
            }
        }
        public string Location { get; set; }
        public string FileSize
        {
            get
            {
                return Utilities.SizeSuffix(new FileInfo(Location).Length);
            }
        }
        public FileTypes FileType
        {
            get
            {
                return ContainerExtensions.TypeFromExtension(new FileInfo(Location).Extension);
            }
        }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        [XmlIgnore]
        public bool Selected { get; set; }
        public bool Installed { get; set; } = false;
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
        public static FileTypes TypeFromExtension(string extension)
        {
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

    public enum FileTypes
    {
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

    public enum Sims2Expansions
    {
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

    public enum Sims3Expansions
    {
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

    public enum Sims4Expansions
    {
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
        public List<MovedItems> ItemsMoved { get; set; } = new();
        public List<LinkedItems> ItemsLinked { get; set; } = new();

        public List<string> FoldersCreated { get; set; } = new();
    }

    public class MovedItems
    {
        public string OriginalLocation { get; set; }
        public string MovedTo { get; set; }
        public bool IsFolder { get; set; }
    }

    public class LinkedItems
    {
        public string LinkLocation { get; set; }
        public bool IsFolder { get; set; }
    }

    public class EntryCount
    {
        public string EntryTag { get; set; }
        public int Count { get; set; }
    }

    public class SimsOverrides
    {
        public string FileLocation { get; set; } //objects.package
        public List<SimsID> Entries { get; set; } = new(); // all the entry IDs
        public List<ItemOverride> GuidOverrides { get; set; } = new(); // 
        public List<ItemOverride> FileNameOverrides { get; set; } = new();
        public List<ItemOverride> TextureNameOverrides { get; set; } = new();
        public List<ItemOverride> References { get; set; } = new();

        public bool ShouldSerializeGuidOverrides()
        {
            return GuidOverrides.Count > 0;
        }
        public bool ShouldSerializeFileNameOverrides()
        {
            return FileNameOverrides.Count > 0;
        }
        public bool ShouldSerializeTextureNameOverrides()
        {
            return TextureNameOverrides.Count > 0;
        }
        public bool ShouldSerializeReferences()
        {
            return References.Count > 0;
        }
        public bool ShouldSerializeEntries()
        {
            return Entries.Count > 0;
        }
    }

    public class SimsID
    {
        public string TypeID { get; set; }
        public string GroupID { get; set; }
        public string InstanceID { get; set; }
        private string _fullkeyproxy;
        public string FullKey
        {
            get { return string.Format("{0}-{1}-{2}", TypeID, GroupID, InstanceID); }
            set { _fullkeyproxy = value; }
        }

        public void KeyFromEntry(IIndexEntry entry)
        {
            TypeID = entry.TypeID;
            GroupID = entry.GroupID;
            InstanceID = entry.InstanceID;
        }
        public void KeyFromIndexEntry(IndexEntry entry)
        {
            TypeID = entry.TypeID;
            GroupID = entry.GroupID;
            InstanceID = entry.InstanceID;
        }
    }

    public class ItemOverride
    {
        public string Overridden { get; set; }
        public SimsID ID { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - ID: {1}", Overridden, ID.FullKey);
        }
    }

    public class OverrideMatch
    {
        public SimsPackage Package { get; set; }
        public SimsOverrides OverrideReference { get; set; }

        public override string ToString()
        {
            return string.Format("{0} replaces something in {1}: {2}", Package.FileName, OverrideReference.FileLocation, OverrideReference.ToString());
        }
    }

    public class SpecificOverrides
    {
        public List<SimsID> Entries { get; set; } = new();
        public string Type { get; set; } //hair
        public string Description { get; set; } //afhaircorntuck
    }

    public class TempReads
    {
        public string file { get; set; }
        public string description { get; set; }
    }

    public class SimsCSV
    {
        public Guid Identifier { get; set; } = Guid.NewGuid();
        public string FileName { get; set; }
        public string Location { get; set; }
        private string _filesize;
        public string FileSize
        {
            get
            {
                if (Directory.Exists(Location))
                {
                    return Utilities.SizeSuffix(Utilities.DirSize(new(Location)));
                }
                else if (File.Exists(Location))
                {
                    return Utilities.SizeSuffix(new FileInfo(Location).Length);
                }
                else
                {
                    return "N/a";
                }
            }
            set { _filesize = value; }
        }

        public FileTypes FileType { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string InstalledForVersion { get; set; }
        public string Source { get; set; }
        public string Creator { get; set; }
        public string Notes { get; set; }
        private bool _isdirectory;
        public bool IsDirectory
        {
            get { return _isdirectory; }
            set
            {
                _isdirectory = value;
                if (value)
                {
                    FileType = FileTypes.Folder;
                }
            }
        }
        public bool Broken { get; set; }
        public bool WrongGame { get; set; }
        public bool Override { get; set; }
        public string Overriding { get; set; }
        public string MatchingMesh { get; set; }
        public ObservableCollection<string> MatchingRecolors { get; set; } = new();
        public string Type { get; set; }
        public string PackageGameVersion { get; set; }
        public SimsGames Game { get; set; }
        public bool RootMod { get; set; }
        public bool OutOfDate { get; set; }
        public bool GameMod { get; set; }
        public bool Mesh { get; set; }
        public bool Recolor { get; set; }
        public bool Orphan { get; set; }
        public string ObjectGUID { get; set; }
        public bool Favorite { get; set; }
        public bool IsEnabled { get; set; }
        public int LoadOrder { get; set; }

        public void GetFromPackage(SimsPackage package)
        {
            this.Identifier = package.Identifier;
            this.FileName = package.FileName;
            this.Location = package.Location;
            this.FileSize = package.FileSize;
            this.FileType = package.FileType;
            this.DateCreated = package.DateCreated;
            this.DateUpdated = package.DateUpdated;
            this.InstalledForVersion = package.InstalledForVersion;
            this.Source = package.Source;
            this.Creator = package.Creator;
            this.Notes = package.Notes;
            this.IsDirectory = package.IsDirectory;
            this.Broken = package.Broken;
            this.WrongGame = package.WrongGame;
            this.Override = package.Override;
            if (package.SpecificOverride != null) this.Overriding = package.SpecificOverride.Description;
            this.MatchingMesh = package.MatchingMesh;
            this.MatchingRecolors = package.MatchingRecolors;
            this.Type = package.Type;
            this.PackageGameVersion = package.PackageGameVersion;
            this.Game = package.Game;
            this.RootMod = package.RootMod;
            this.OutOfDate = package.OutOfDate;
            this.GameMod = package.GameMod;
            this.Mesh = package.Mesh;
            this.Recolor = package.Recolor;
            this.Orphan = package.Orphan;
            if (package.ObjectGUID != null) this.ObjectGUID = package.ObjectGUID;
            this.Favorite = package.Favorite;
            this.IsEnabled = package.IsEnabled;
            this.LoadOrder = package.LoadOrder;
        }
    }

    public class PotentialDuplicateSimsPackage
    {
        public SimsPackage Package { get; set; }
        public bool IsDuplicate { get; set; } = true;
        public bool IsConflict { get; set; }
        public List<SimsPackage> Conflicts { get; set; } = new();
    }
}
