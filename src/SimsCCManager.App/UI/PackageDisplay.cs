using DataGridContainers;
using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.OptionLists;
using SimsCCManager.PackageReaders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
    [Export]
    public HSplitContainer WhereAllModsContainerLives;
    [Export]
    public VSplitContainer VSplit;
    [ExportCategory("PackedScene")]
    [Export]
    PackedScene ProfilesManagementWindowPS;
    [Export]
    PackedScene CategoryManagementWindowPS;
    [Export]
    PackedScene UIAllModsPS;
    [ExportCategory("Dialogs")]
    [Export]
    public FileDialog AddFilesDialog;
    [Export]
    public FileDialog AddExeDialog;
    [Export]
    public FileDialog AddFolderDialog;
    [Export]
    public CustomPopupWindow AdminWarningWindow;
    [Export]
    MarginContainer PleaseWaitWindow;
    [Export]
    Label PleaseWaitLabel;
    [Export]
    ColorRect PleaseWaitBG;
    

    public ProfileManagement ProfileManagementWindow;


    

    


    public VFSFiles VFSFileList = new();

    public List<Category> HideCategoriesInGrid = new();


    private bool _lockinput;
    public bool LockInput { get { return _lockinput; } 
    set { _lockinput = value; 
    UIAllModsContainer.DataGrid.BlockInput = value; }}


    public bool _gamerunning;
    public bool GameRunning
    {
        get { return _gamerunning; }
        set { _gamerunning = value; 
        CallDeferred(nameof(GameRunningFlip)); 
        if (value == false)
            {
                InstanceControllers.ClearInstance(ThisInstance, false);
            }
        }
    }

    public override void _Ready()
    {
        AdminWarningWindow.Visible = false;
        AdminWarningWindow.NoButton.Visible = false;
        AdminWarningWindow.YesButton.Pressed += () => AcceptAdminWarning();
        AdminWarningWindow.WindowMessage.Text = "SCCM requires administrator access to your game folder to deploy root mods. Either reload the app with elevated permissions, disable root mods, or move your install out of protected folders.";
        AdminWarningWindow.WindowTitle = "Elevated Permissions Required for Root";
        UIGameStartControls.PackageDisplay = this;
        GameRunningPopup.DisconnectFromGame += () => DisconnectGame();
        
        ThisInstance.InstanceInformationChanged += () => InstanceEdited();

        UIPackageManagementButtons.TopBarButtonPressed += (but) => PackageManagementButtonPressed(but);
       
        ExeChoicePopupPanel.packageDisplay = this;
        UIGameStartControls.GameChoiceDropDownButton.Pressed += () => ShowExeChoices();
        ExeChoicePopupPanel.GetExes();

        AddFilesDialog.FilesSelected += (items) => AddFilesToInstance(items);
        AddFilesDialog.FileSelected += (item) => AddFileToInstance(item);


        PleaseWaitBG.Color = GlobalVariables.LoadedTheme.BackgroundColor;
        PleaseWaitLabel.AddThemeColorOverride("font_color", GlobalVariables.LoadedTheme.MainTextColor);




        AddFolderDialog.DirSelected += (directory) => AddFolderToInstance(directory);

        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("This instance has found {0} files.", ThisInstance.Files.Count));
        InitializeUIAllMods();
        UIAllModsContainer.PackagesDataChanged += () => PackagesChanged();
        PackagesChanged();
    }

    private void PackagesChanged()
    {
        if (ThisInstance.Files.OfType<SimsPackage>().Any(x => x.Broken || x.WrongGame || x.Orphan || x.OutOfDate))
        {
            UISettingsAndHelpControls.ErrorsDetected = true;
            UISettingsAndHelpControls.ErrorCount = ThisInstance.Files.OfType<SimsPackage>().Count(x => x.Broken || x.WrongGame || x.Orphan || x.OutOfDate);
        } else
        {
            UISettingsAndHelpControls.ErrorsDetected = false;
            UISettingsAndHelpControls.ErrorCount = 0;
        }
    }


    private bool InitializeUIAllMods()
    {
        UIAllModsContainer.packageDisplay = this;
        EnabledFromProfile();
        UIProfilesManagement.packageDisplay = this;
        UIProfilesManagement.UpdateProfileOptions();
        UIProfilesManagement.ManageProfilesOpen += () => OpenProfileManagementWindow();
        UIProfilesManagement.ProfileChanged += (profile, idx) => ProfileChanged(profile, idx);
        UIAllModsContainer.CreateDataGrid();
        UIPackageViewerContainer.packageDisplay = this;
        return true;
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
                RefreshFiles();
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
                LockInput = true;
                OpenManageCategoriesWindow();
            break;
            case 6:
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: Edit exes pressed.", but));
            break;
        }
    }

    public ConcurrentBag<Task> runningTasks = new();

    public void RefreshFiles()
    {
        TogglePleaseWait(true);

        //List<SimsPackage> newpackages = [];

        List<SimsPackage> gonepackages = [];
        List<SimsDownload> newdownloads = [];

        List<SimsDownload> gonedownloads = [];

        ThisInstance._packages = new();
        ThisInstance._downloads = new();
        
        List<string> allLocations = [..ThisInstance.Files.Select(x => x.Location)];
        List<string> allfound = new();

        foreach (string file in Directory.GetFiles(ThisInstance.InstanceFolders.InstancePackagesFolder))
        {
            allfound.Add(file);
            FileInfo fi = new(file);
            if (!ThisInstance.Files.Any(x=>x.Location == file) && GlobalVariables.SimsFileExtensions.Contains(fi.Extension))
            {
                Task t = Task.Run(() => {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found new file: {0}", file));
                                        
                    SimsPackage simsPackage = InstanceControllers.ReadPackage(file, ThisInstance, fi);                    
                    ThisInstance._packages.Add(simsPackage);                        
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));
                    int incBy = 1;
                    if (simsPackage.IsDirectory)
                    {
                        if (simsPackage.LinkedFiles.Count > 0) incBy += simsPackage.LinkedFiles.Count;
                        if (simsPackage.LinkedFolders.Count > 0) incBy += simsPackage.LinkedFolders.Count;
                    }
                    GlobalVariables.mainWindow.IncrementLoadingScreen(incBy, fi.Name.Replace(".info", ""), "Globals: ReadPackages 1");
                                        
                });
                runningTasks.Add(t);
            }            
        }

        foreach (string file in Directory.GetDirectories(ThisInstance.InstanceFolders.InstancePackagesFolder))
        {
            allfound.Add(file);
            if (!ThisInstance.Files.Any(x=>x.Location == file))
            {
                Task t = Task.Run(() => {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                    DirectoryInfo fi = new(file);
                    SimsPackage simsPackage = InstanceControllers.ReadPackage(file, ThisInstance, fi);                
                    ThisInstance._packages.Add(simsPackage);
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));
                    
                    if (!simsPackage.RootMod) ThisInstance = InstanceControllers.GetSubDirectories(ThisInstance, file, simsPackage);
                    
                    //GlobalVariables.mainWindow.IncrementLoadingScreen(incBy, fi.Name.Replace(".info", ""), "Globals: ReadPackages 3");
                });
                runningTasks.Add(t);
            }
        }
        foreach (string file in Directory.GetFiles(ThisInstance.InstanceFolders.InstanceDownloadsFolder))
        {
            allfound.Add(file);
            if (!ThisInstance.Files.Any(x=>x.Location == file))
            {
                Task t = Task.Run(() => {
                    FileInfo f = new(file);
                    SimsDownload simsDownload = InstanceControllers.ReadDownload(file, f);
                    ThisInstance._downloads.Add(simsDownload);                    
                });
                runningTasks.Add(t);
            }
        }   
        Task w = Task.Run(() => {
            Thread.Sleep(5);
        });
        runningTasks.Add(w);
        while (runningTasks.Any(x => !x.IsCompleted))
        {
            
        }

        if (allfound.Count != allLocations.Count)
        {
            List<string> gone = [..allLocations.Except(allfound)];
            foreach (string g in gone)
            {
                var s = ThisInstance.Files.First(x => x.Location == g);
                if (s.GetType() == typeof(SimsPackage))
                {
                    gonepackages.Add(s as SimsPackage);
                } else
                {
                    gonedownloads.Add(s as SimsDownload);
                }
                ThisInstance.Files.Remove(ThisInstance.Files.First(x => x.Location == g));
            }
        }

        List<SimsPackage> newPackages = [..ThisInstance._packages];
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0} files in newPackages", newPackages.Count));
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0} files in _packages", ThisInstance._packages.Count));

        if (!ThisInstance._packages.IsEmpty)
        {
            foreach (SimsPackage package in ThisInstance._packages.OrderBy(x=>x.FileName))
            {
                ThisInstance.Files.Add(package);
            }            
        }

        if (!ThisInstance._downloads.IsEmpty)
        {
           foreach (SimsDownload dl in ThisInstance._downloads.OrderBy(x=>x.FileName))
            {
                ThisInstance.Files.Add(dl);
            } 
        }

        ThisInstance._packages = [..ThisInstance.Files.OfType<SimsPackage>()];

        ThisInstance = InstanceControllers.FindOrphans(ThisInstance);
        

        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found {0} new packages, {1} new downloads, {2} packages that have been removed and {3} downloads that're gone", newPackages.Count, ThisInstance._downloads.Count, gonepackages.Count, gonedownloads.Count));

        UIAllModsContainer.ReplaceFiles(newPackages, gonepackages);
    }

    public void TogglePleaseWait(bool Show)
    {
        LockInput = Show;
        PleaseWaitWindow.Visible = Show;
    }


    private void OpenManageCategoriesWindow()
    {
        categorymanagement = CategoryManagementWindowPS.Instantiate() as CategoryManagement;
        categorymanagement.packageDisplay = this;
        AddChild(categorymanagement);
        categorymanagement.CategoriesUpdated += (b) => CategoriesUpdated(b);
    }

    public CategoryManagement categorymanagement;

    

    private void CategoriesUpdated(bool fromClose)
    {
        if (fromClose)
        {
            List<DataGridRow> rows = UIAllModsContainer.DataGrid.RowData;
            List<DataGridRow> hide = new();
            StringBuilder sb = new();
            foreach (Category c in HideCategoriesInGrid)
            {
                sb.AppendLine(c.Name);
            }
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Categories to hide: {0}", sb.ToString()));
            
            if (HideCategoriesInGrid.Count > 0)
            {
                List<SimsPackage> matching = new();
                foreach (Category cat in HideCategoriesInGrid)
                {
                    matching.AddRange(UIAllModsContainer.Packages.Where(x => x.PackageCategory.Identifier == cat.Identifier));
                }
                foreach (SimsPackage p in matching)
                {
                    hide.Add(rows.First(x=>x.Identifier == p.Identifier));
                }
            }
            UIAllModsContainer.HiddenRows = hide;
            categorymanagement.QueueFree();
        }        
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
        List<EnabledPackages> remove = new();
        foreach (EnabledPackages package in ThisInstance.LoadedProfile.EnabledPackages)
        {
            if (!ThisInstance.Files.Any(x => x.Identifier == package.PackageIdentifier))
            {
                remove.Add(package);
            }
        }
        foreach (EnabledPackages p in remove)
        {
            ThisInstance.LoadedProfile.EnabledPackages.Remove(p);
        }
        
        ThisInstance.WriteXML();
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
        LockInput = true;
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

    private void AcceptAdminWarning()
    {
        AdminWarningWindow.Visible = false;
    }


    public bool LinkFiles()
    {
        if (File.Exists(GlobalVariables.MovedItemsFile))
        {
            InstanceControllers.ClearInstance();
        }
        if (!GlobalVariables.IsElevated)
        {
            if (ThisInstance.Files.OfType<SimsPackage>().Any(x => x.RootMod))
            {            
                try { var fs = new FileSecurity(ThisInstance.GameInstallFolder, AccessControlSections.All); }
                catch (Exception e)
                {                
                    if (e.GetBaseException().GetType() == typeof(PrivilegeNotHeldException))
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Elevation required for root mod access."));
                        AdminWarningWindow.Visible = true;
                        return false;
                    } else
                    {
                        
                    }               
                }
            }
        }


        

        string packageFolderLocation = "";

        switch (ThisInstance.GameChoice)
        {
            case SimsGames.Sims2:
                packageFolderLocation = ThisInstance.Sims2Folders.DownloadsFolder;
                if (ThisInstance.LoadedProfile.LocalData)
                {
                    foreach (string folder in GlobalVariables.Sims2DataFolders)
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
                    foreach (string file in GlobalVariables.Sims2DataFiles)
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
                    foreach (string folder in GlobalVariables.Sims2MediaFolders)
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
                    foreach (string folder in GlobalVariables.Sims2SavesFolders)
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
                    foreach (string folder in GlobalVariables.Sims2SettingsFolders)
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
                    //TODO: dont forget to retrieve ALL OTHER .txt files on close 
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
