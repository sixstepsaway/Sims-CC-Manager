using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.OptionLists;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

public partial class PackageDisplay : MarginContainer
{
    public GameInstance ThisInstance;
    public InstanceProfile CurrentProfile { get { return ThisInstance.LoadedProfile; } set {ThisInstance.LoadedProfile = value; ThisInstance.WriteXML(); EnabledFromProfile(); } }
    [Export]
    public AllModsContainer UIAllModsContainer;
    [Export]
    public NewDownloadsContainer UIDownloadsContainer;
    [Export]
    public PackageViewerContainer UIPackageViewerContainer;
    [Export]
    public PackageManagementButtons UIPackageManagementButtons;
    [Export]
    public ProfilesManagement UIProfilesManagement;
    [Export]
    public GameStartControls UIGameStartControls;
    [Export]
    public SettingsAndHelpControls UISettingsAndHelpControls;
    [Export]
    public GameRunningPopup GameRunningPopup;
    [Export]
    public ExeChoicePopupPanel ExeChoicePopupPanel;
    [ExportCategory("PackedScene")]
    [Export]
    PackedScene ProfilesManagementWindowPS;
    [Export]
    PackedScene CategoryManagementWindowPS;
    [ExportCategory("Dialogs")]
    [Export]
    public FileDialog AddFilesDialog;
    [Export]
    public FileDialog AddExeDialog;
    [Export]
    public FileDialog AddFolderDialog;

    public ProfileManagement ProfileManagementWindow;


    

    List<string> Sims2DataFolders = new() { "Collections","LockedBins","Logs","PetBreeds","PetCoats","SC4Terrains","Teleport","LotCatalog" };
    List<string> Sims2DataFiles = new() { "Accessory.cache", "Groups.cache"};
    List<string> Sims2SavesFolders = new() { "Neighborhoods", "PackagedLots" };
    List<string> Sims2SettingsFolders = new() { "Cameras", "Config" };
    List<string> Sims2MediaFolders = new() { "Movies", "Music", "Paintings", "Screenshots", "Storytelling", "Thumbnails" };

    List<string> Sims3DataFolders = new() { "Collections", "ContentPatch", "DCBackup", "DCCache", "Downloads", "FeaturedItems", "IGACache", "SigsCache", "Thumbnails"  };
    List<string> Sims3DataFiles = new() { "CASPartCache.package", "compositorCache.package", "DeviceConfig.log", "scriptCache.package", "simCompositorCache.package", "Sims3LauncherLogFile.log", "Sims3Logs.xml", "Version.tag" };
    List<string> Sims3SavesFolders = new() { "CurrentGame.sims3", "Saves", "Exports", "SavedOutfits" };
    List<string> Sims3SettingsFiles = new() { "options.ini" };
    List<string> Sims3MediaFolders = new() { "Custom Music", "Recorded Videos", "Screenshots" };

    List<string> Sims4DataFolders = new() { "cachestr",  "Content", "onlinethumbnailcache", "ReticulatedSplinesView"};
    List<string> Sims4DataFiles = new() { "accountDataDB.package", "avatarcache.package", "clientDB.package", "ConnectionStatus.txt","GameVersion.txt", "houseDescription-client.package", "localsimtexturecache.package", "localthumbcache.package", "notify.glob", "UserData.lock"};
    List<string> Sims4SavesFolders = new() { "Saves", "Tray" };
    List<string> Sims4SettingsFolders = new() { "ConfigOverride"};
    List<string> Sims4SettingsFiles = new() { "UserSetting.ini", "Options.ini", "Events.ini", "GameConfig.ini", "Config.log"};
    List<string> Sims4MediaFolders = new() { "Recorded Videos", "Screenshots", "Custom Music" };


    public VFSFiles VFSFileList = new();



    public bool _gamerunning;
    public bool GameRunning
    {
        get { return _gamerunning; }
        set { _gamerunning = value; 
        CallDeferred(nameof(GameRunningFlip)); 
        if (value == false)
            {
                InstanceControllers.ClearInstance();
            }
        }
    }

    public override void _Ready()
    {
        UIGameStartControls.PackageDisplay = this;
        GameRunningPopup.DisconnectFromGame += () => DisconnectGame();
        UIAllModsContainer.packageDisplay = this;
        EnabledFromProfile();
        UIProfilesManagement.packageDisplay = this;
        UIProfilesManagement.UpdateProfileOptions();
        UIProfilesManagement.ManageProfilesOpen += () => OpenProfileManagementWindow();
        UIProfilesManagement.ProfileChanged += (profile, idx) => ProfileChanged(profile, idx);
        ThisInstance.InstanceInformationChanged += () => InstanceEdited();

        UIPackageManagementButtons.TopBarButtonPressed += (but) => PackageManagementButtonPressed(but);
       
        ExeChoicePopupPanel.packageDisplay = this;
        UIGameStartControls.GameChoiceDropDownButton.Pressed += () => ShowExeChoices();
        ExeChoicePopupPanel.GetExes();

        AddFilesDialog.FilesSelected += (items) => AddFilesToInstance(items);
        AddFilesDialog.FileSelected += (item) => AddFileToInstance(item);

        AddFolderDialog.DirSelected += (directory) => AddFolderToInstance(directory);

        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("This instance has found {0} files.", ThisInstance.Files.Count));


        UIAllModsContainer.CreateDataGrid();


    }

    private void AddFolderToInstance(string directory)
    {
        DirectoryInfo directoryInfo = new(directory);
        string name = directoryInfo.Name;
        string newloc = Path.Combine(ThisInstance.InstanceFolders.InstancePackagesFolder, name);
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Moving {0} to {1}", directory, newloc));
        Directory.Move(directory, newloc);
    }


    private void AddFileToInstance(string item)
    {
        FileInfo fileInfo = new(item);
        string name = fileInfo.Name;
        string newloc = Path.Combine(ThisInstance.InstanceFolders.InstancePackagesFolder, name);
        File.Move(item, newloc);
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Moving {0} to {1}", item, newloc));
    }

    private void AddFilesToInstance(string[] items)
    {
        foreach (string file in items)
        {
            AddFileToInstance(file);
        }
    }


    private void ShowExeChoices()
    {
        ExeChoicePopupPanel.Visible = true;
    }


    private void PackageManagementButtonPressed(int but)
    {
        switch (but)
        {
            case 0:
                AddFilesDialog.Visible = true; 
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: Add files pressed.", but));
            break;
            case 1:
                AddFolderDialog.Visible = true;
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: Add folder pressed.", but));
            break;
            case 2:
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: Refresh files pressed.", but));
            break;
            case 3:
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: Sort subfolders pressed.", but));
            break;
            case 4:
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: Export profile pressed.", but));
            break;
            case 5:
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: Manage categories pressed.", but));
                OpenManageCategoriesWindow();
            break;
            case 6:
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: Edit exes pressed.", but));
            break;
        }
    }

    private void OpenManageCategoriesWindow()
    {
        CategoryManagement categorymanagement = CategoryManagementWindowPS.Instantiate() as CategoryManagement;
        categorymanagement.packageDisplay = this;
        AddChild(categorymanagement);
        categorymanagement.CategoriesUpdated += () => CategoriesUpdated();
    }

    private void CategoriesUpdated()
    {
        
    }

    private void InstanceEdited()
    {
        ThisInstance.WriteXML();
        UIAllModsContainer.UpdateProfilePackages();
    }


    private void ProfileChanged(string profile, int idx)
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Profile changed to {0}: {1}", idx, profile));
        InstanceProfile pickedprofile = ThisInstance.InstanceProfiles.Where(x => x.ProfileName == profile).First();
        CurrentProfile = pickedprofile;        
        //TO DO: ADD PROFILE CHANGE HERE.
    }

    private void EnabledFromProfile()
    {
        foreach (SimsPackage pack in ThisInstance.Files.OfType<SimsPackage>())
        {
            if (ThisInstance.LoadedProfile.EnabledPackages.Where(x => x.PackageIdentifier == pack.Identifier).Any())
            {
                EnabledPackages ep = ThisInstance.LoadedProfile.EnabledPackages.Where(x => x.PackageIdentifier == pack.Identifier).First();
                pack.IsEnabled = true;
                pack.LoadOrder = ep.LoadOrder;                
            } else
            {
                pack.IsEnabled = false;
                pack.LoadOrder = -1; 
            }
        }
        UIAllModsContainer.UpdateProfilePackages();
    }

    private void OpenProfileManagementWindow()
    {
        ProfileManagement prof = ProfilesManagementWindowPS.Instantiate() as ProfileManagement;
        prof.packageDisplay = this;
        prof.ProfilesUpdated += () => ProfilesUpdated();
        AddChild(prof);
    }

    private void ProfilesUpdated()
    {
        UIProfilesManagement.UpdateProfileOptions();
    }

    private void DisconnectGame()
    {
        GameRunningPopup.Visible = false;
        if (File.Exists(GlobalVariables.MovedItemsFile))
        {
            InstanceControllers.ClearInstance();
        }
    }


    public bool LinkFiles()
    {
        if (File.Exists(GlobalVariables.MovedItemsFile))
        {
            InstanceControllers.ClearInstance();
        }

        string packageFolderLocation = "";

        switch (ThisInstance.GameChoice)
        {
            case SimsGames.Sims2:
                packageFolderLocation = ThisInstance.Sims2Folders.DownloadsFolder;
                if (ThisInstance.LoadedProfile.LocalData)
                {
                    foreach (string folder in Sims2DataFolders)
                    {
                        string folderPath = Path.Combine(ThisInstance.InstanceFolders.InstanceDataFolder, folder);
                        string destinationPath = Path.Combine(ThisInstance.GameDocumentsFolder, folder);
                        if (Directory.Exists(folderPath))
                        {
                            if (Directory.Exists(destinationPath))
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, folder);
                                Directory.Move(destinationPath, moveloc);
                                VFSFileList.ItemsMoved.Add(new(){ IsFolder = true, MovedTo = moveloc, OriginalLocation = destinationPath});
                            }
                        }
                        
                        VirtualFileSystem.MakeJunction(folderPath, destinationPath);
                        VFSFileList.ItemsLinked.Add(new () { LinkLocation = destinationPath, IsFolder = true });
                    }
                    foreach (string file in Sims2DataFiles)
                    {                        
                        string filePath = Path.Combine(ThisInstance.InstanceFolders.InstanceDataFolder, file);
                        string destinationPath = Path.Combine(ThisInstance.GameDocumentsFolder, file);
                        if (File.Exists(filePath))
                        {
                            if (File.Exists(destinationPath))
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, file);
                                Directory.Move(destinationPath, moveloc);
                                VFSFileList.ItemsMoved.Add(new(){ IsFolder = false, MovedTo = moveloc, OriginalLocation = destinationPath});
                            }
                        }
                        
                        VirtualFileSystem.MakeSymbolicLink(filePath, destinationPath);
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Linking: {0}", destinationPath));
                        VFSFileList.ItemsLinked.Add(new () { LinkLocation = destinationPath, IsFolder = false });
                    }
                } 
                if (ThisInstance.LoadedProfile.LocalMedia)
                {
                    foreach (string folder in Sims2MediaFolders)
                    {
                        string folderPath = Path.Combine(ThisInstance.InstanceFolders.InstanceDataFolder, folder);
                        string destinationPath = Path.Combine(ThisInstance.GameDocumentsFolder, folder);
                        if (Directory.Exists(folderPath))
                        {
                            if (Directory.Exists(destinationPath))
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, folder);
                                Directory.Move(destinationPath, moveloc);
                                VFSFileList.ItemsMoved.Add(new(){ IsFolder = true, MovedTo = moveloc, OriginalLocation = destinationPath});
                            }
                        }
                        
                        VirtualFileSystem.MakeJunction(folderPath, destinationPath);
                        VFSFileList.ItemsLinked.Add(new () { LinkLocation = destinationPath, IsFolder = true });
                    }      
                }
                if (ThisInstance.LoadedProfile.LocalSaves)
                {
                    foreach (string folder in Sims2SavesFolders)
                    {
                        string folderPath = Path.Combine(ThisInstance.InstanceFolders.InstanceDataFolder, folder);
                        string destinationPath = Path.Combine(ThisInstance.GameDocumentsFolder, folder);
                        if (Directory.Exists(folderPath))
                        {
                            if (Directory.Exists(destinationPath))
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, folder);
                                Directory.Move(destinationPath, moveloc);
                                VFSFileList.ItemsMoved.Add(new(){ IsFolder = true, MovedTo = moveloc, OriginalLocation = destinationPath});
                            }
                        }
                        
                        VirtualFileSystem.MakeJunction(folderPath, destinationPath);
                        VFSFileList.ItemsLinked.Add(new () { LinkLocation = destinationPath, IsFolder = true });
                    } 
                }
                if (ThisInstance.LoadedProfile.LocalSettings)
                {
                    foreach (string folder in Sims2SettingsFolders)
                    {
                        string folderPath = Path.Combine(ThisInstance.InstanceFolders.InstanceDataFolder, folder);
                        string destinationPath = Path.Combine(ThisInstance.GameDocumentsFolder, folder);
                        if (Directory.Exists(folderPath))
                        {
                            if (Directory.Exists(destinationPath))
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, folder);
                                Directory.Move(destinationPath, moveloc);
                                VFSFileList.ItemsMoved.Add(new(){ IsFolder = true, MovedTo = moveloc, OriginalLocation = destinationPath});
                            }
                        }
                        
                        VirtualFileSystem.MakeJunction(folderPath, destinationPath);
                        VFSFileList.ItemsLinked.Add(new () { LinkLocation = destinationPath, IsFolder = true });
                    } 
                }


            break;
            case SimsGames.Sims3:
                packageFolderLocation = ThisInstance.Sims3Folders.ModsFolder;
                if (ThisInstance.LoadedProfile.LocalData)
                {
                    
                } 
                if (ThisInstance.LoadedProfile.LocalMedia)
                {
                    
                }
                if (ThisInstance.LoadedProfile.LocalSaves)
                {
                    
                }
                if (ThisInstance.LoadedProfile.LocalSettings)
                {
                    
                }


            break;
            case SimsGames.Sims4:
                packageFolderLocation = ThisInstance.Sims4Folders.ModsFolder;
                if (ThisInstance.LoadedProfile.LocalData)
                {
                    /* dont forget to retrieve ALL OTHER .txt files on close */  
                } 
                if (ThisInstance.LoadedProfile.LocalMedia)
                {
                    
                }
                if (ThisInstance.LoadedProfile.LocalSaves)
                {
                    
                }
                if (ThisInstance.LoadedProfile.LocalSettings)
                {
                    
                }


            break;

            
        }
        
        

        List<EnabledPackages> LoadOrder = ThisInstance.LoadedProfile.EnabledPackages.OrderBy(x => x.LoadOrder).ToList();

        if (!Directory.Exists(packageFolderLocation))
        {
            Directory.CreateDirectory(packageFolderLocation);
            VFSFileList.FoldersCreated.Add(packageFolderLocation);
        }

        foreach (EnabledPackages package in LoadOrder)
        {
            SimsPackage packageInfo = ThisInstance.Files.OfType<SimsPackage>().First(x => x.Identifier == package.PackageIdentifier);

            if (packageInfo.RootMod)
            {
                string root = ThisInstance.GameInstallFolder;
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Game install folder: {0}", ThisInstance.GameInstallFolder));
                
                foreach (string directory in Directory.EnumerateDirectories(packageInfo.Location, "*.*", SearchOption.AllDirectories))
                {
                    string path = directory.Replace(packageInfo.Location, "");
                    path = path[1..];
                    string pathcheck = Path.Combine(ThisInstance.GameInstallFolder, path);                    
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking path: {0}", pathcheck));
                    if (!Directory.Exists(pathcheck))
                    {
                        VFSFileList.FoldersCreated.Add(pathcheck);
                        Directory.CreateDirectory(pathcheck);
                    }
                }

                foreach (string file in Directory.EnumerateFiles(packageInfo.Location, "*.*", SearchOption.AllDirectories))
                {
                    string path = file.Replace(packageInfo.Location, "");
                    path = path[1..];
                    string pathcheck = Path.Combine(ThisInstance.GameInstallFolder, path);
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking path: {0}", pathcheck)); 
                    if (!File.Exists(pathcheck))
                    {
                        VFSFileList.ItemsLinked.Add(new() { IsFolder = false, LinkLocation = pathcheck});
                        VirtualFileSystem.MakeSymbolicLink(file, pathcheck);
                    } else
                    {
                        if (File.Exists(file))
                        {
                            if (File.Exists(pathcheck))
                            {
                                FileInfo f = new(file);
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", pathcheck));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, f.Name);
                                Directory.Move(file, moveloc);
                                VFSFileList.ItemsMoved.Add(new(){ IsFolder = false, MovedTo = moveloc, OriginalLocation = pathcheck});
                            }
                        }
                    }
                }
                
            }
            else if (packageInfo.IsDirectory)
            {
                LinkDirectory(package.PackageLocation, packageFolderLocation, package.LoadOrder);

            } else
            {
                FileInfo fileInfo = new(package.PackageLocation);
                string NewName = NameLoadOrder(fileInfo.Name.Replace(fileInfo.Extension, ""), package.LoadOrder, fileInfo.Extension);
                string newpath = Path.Combine(packageFolderLocation, NewName);

                if (File.Exists(newpath))
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", newpath));
                    string moveloc = Path.Combine(GlobalVariables.TempFolder, fileInfo.Name);
                    Directory.Move(newpath, moveloc);
                    VFSFileList.ItemsMoved.Add(new(){ IsFolder = false, MovedTo = moveloc, OriginalLocation = newpath});
                }

                VirtualFileSystem.MakeSymbolicLink(package.PackageLocation, newpath);

                VFSFileList.ItemsLinked.Add(new() { LinkLocation = newpath, IsFolder = false } );
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Linking: {0}", newpath));

                if (packageInfo.LinkedFiles.Count != 0)
                {
                    foreach (string file in packageInfo.LinkedFiles)
                    {
                        FileInfo fileInf = new(file);
                        string NewN = NameLoadOrder(fileInf.Name.Replace(fileInf.Extension, ""), package.LoadOrder, fileInf.Extension);
                        string newp = Path.Combine(packageFolderLocation, NewN);
                        if (File.Exists(newp))
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", newp));
                            string moveloc = Path.Combine(GlobalVariables.TempFolder, fileInf.Name);
                            Directory.Move(newp, moveloc);
                            VFSFileList.ItemsMoved.Add(new(){ IsFolder = false, MovedTo = moveloc, OriginalLocation = newp});
                        }
                        VirtualFileSystem.MakeSymbolicLink(file, newp); 

                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Linking: {0}", newp));
                    }
                }

                if (packageInfo.LinkedFolders.Count != 0)
                {
                    foreach (string folder in packageInfo.LinkedFolders)
                    {
                        LinkDirectory(folder, packageFolderLocation, package.LoadOrder);
                    }
                }
            }

            
        }

        XmlSerializer movedItemsSerializer = new XmlSerializer(typeof(VFSFiles));
        using (var writer = new StreamWriter(GlobalVariables.MovedItemsFile))
        {
            movedItemsSerializer.Serialize(writer, VFSFileList);
        }
        VFSFileList = new();

        return true;
    }


    private void LinkDirectory(string FolderLocation, string packageFolderLocation, int LoadOrder)
    {
        DirectoryInfo directoryInfo = new(FolderLocation);                
        string NewName = NameLoadOrder(directoryInfo.Name, LoadOrder, "");
        string linkPath = Path.Combine(packageFolderLocation, NewName);

        StringBuilder sb = new();
        int i = 0;
        Directory.CreateDirectory(linkPath);
        VFSFileList.FoldersCreated.Add(linkPath);
        foreach (string file in Directory.EnumerateFiles(FolderLocation, "*.*", SearchOption.AllDirectories))
        {
            FileInfo fileinf = new(file);
            if (GlobalVariables.SimsFileExtensions.Contains(fileinf.Extension))
            {
                string fileLink = Path.Combine(linkPath, fileinf.Name);
                VFSFileList.ItemsLinked.Add(new() { LinkLocation = fileLink, IsFolder = false } );
                VirtualFileSystem.MakeSymbolicLink(file, fileLink);

                sb.AppendLine(string.Format("File {0}: {1}", i, fileLink));
                i++;
            }                 
        }

        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Linking: \n Directory: {0}\n Files: {1}", linkPath, sb.ToString()));
    }

    private string NameLoadOrder(string input, int loadorder, string extension)
    {
        string leading = "0";
        if (loadorder < 10)
        {
            leading = "000";
        } else if (loadorder < 100)
        {
            leading = "00";
        } else if (loadorder < 1000)
        {
            leading = "0";
        } else
        {
            leading = "";
        }

        input = Regex.Replace(input, "[^0-9a-zA-Z]+", "");
        
        return string.Format("{0}{1}{2}{3}", leading, loadorder, input, extension);
    }

    private void GameRunningFlip()
    {
        GameRunningPopup.Visible = GameRunning;
    }

}

public class VirtualFileSystem
{
    //https://stackoverflow.com/questions/11777924/how-to-make-a-read-only-file
    //https://stackoverflow.com/questions/1199571/how-to-hide-file-in-c
    //https://learn.microsoft.com/en-us/dotnet/api/system.io.file.createsymboliclink?view=net-7.0
    //https://stackoverflow.com/questions/3387690/how-to-create-a-hardlink-in-c
    //https://github.com/usdAG/SharpLink  
        
    
    public static void MakeSymbolicLink(string Original, string Destination){
        FileInfo fileInfo = new(Original);
        //FileInfo destinfo = new(Destination);
        //Destination = Path.Combine(Destination, fileInfo.Name);        
        try {
            File.CreateSymbolicLink(Destination, Original);                 
        } catch (Exception e) {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Caught exception making symbolic link: {0}\nException: {1}", fileInfo.Name, e.Message));
        }            
    }
    
    public static void RemoveSymbolicLink(string Item){
        if (File.Exists(Item)){
            try { 
                File.Delete(Item); 
            } catch (Exception e) {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Caught exception deleting symbolic link: {0}\nException: {1}", Item, e.Message));
            }
        }
        if (File.Exists(string.Format("{0}.disabled", Item))){
            string og = string.Format("{0}.disabled", Item);
            string ren = og.Replace(".disabled", "");
            try {
                File.Move(og, ren);
            } catch (Exception e) {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Caught exception renaming disabled file: {0}\nException: {1}", Item, e.Message));
            }
        }
    }

    public static void MakeJunction(string Original, string Destination){  
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Creating junction from {0} to {1}", Destination, Original));
        DirectoryInfo destinfo = new(Destination);
        try {
            Directory.CreateSymbolicLink(Destination, Original);
        } catch (Exception e) {  
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXCEPTION: Caught exception making junction: {0}\nException: {1}", destinfo.Name, e.Message));                      
        }
    }

    public static void RemoveJunction(string Item){
        if (Directory.Exists(Item)){
            try {
                Directory.Delete(Item);
            } catch (Exception e) {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXCEPTION: Caught exception removing junction: {0}\nException: {1}", Item, e.Message));
            }
        }            
        if (Directory.Exists(string.Format("{0}--DISABLED", Item))){
            string og = string.Format("{0}--DISABLED", Item);
            string ren = og.Replace("--DISABLED", "");
            try {
                if(new DirectoryInfo(Item).Attributes.HasFlag(FileAttributes.ReparsePoint)){
                    Directory.Move(og, ren);
                } else {
                    File.Move(og, ren);
                }                    
            } catch (Exception e) {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Caught exception renaming disabled folder: {0}\nException: {1}", Item, e.Message));                    
            }
        }
    }
}
