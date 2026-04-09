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
using CsvHelper;
using System.Globalization;
using System.Net.Http.Headers;
using MoreLinq;
using System.Collections.ObjectModel;
using System.Security.Principal;

public partial class PackageDisplay : MarginContainer
{
    
    [Export]
    Godot.FileAccess CAWResourceFile;
    string CAWResource = @"# ResourceConfig file for platform-independent resources
#
# See ResourceConfig/doc/ResourceConfig.htm for details on format.
# Intranet link as of this moment:
#  http://ears-wiki/ctg/_docs/depot/UTF/HTML/UTFApp/Resource/ResourceConfig/doc/ResourceConfig.htm

#
#  UI
#
Priority -29
PackedFile UI/UI.package
PackedFile Automation/AutomationData.package
PackedFile Jazz/JazzData.package
PackedFile Misc/fallback.package

#
# Folder associations using Group IDs
#
#Group 0x001407ec Audio
#Group 0x0051185b EffectsBinary

#
# File associations using FileType IDs. Put them here instead of in a DDFMap.txt
#
#FileType 0x0175e5cd script
#FileType 0x0175e5d9 scriptsym
#FileType 0x8eaf13de rig
#FileType 0x6b20c4f3 clip
#FileType 0x00b2d882 dds
#FileType 0x025ed6f4 simoutfit
#FileType 0xd55f7caf lightrigs
FileType 0xf0ff5598 triggers
#FileType 0x11c258c0 ctriggers
FileType 0x0333406c xml
FileType 0x1a3201cd mod
#FileType 0xd3044521 slot
#FileType 0x00b552ea spt
#FileType 0x021d7e8c spt2
#FileType 0x1f886ead ini
FileType 0x025c95b6 layout
FileType 0x025c90a6 css
FileType 0x062e9ee0 ttf
FileType 0x062e9ee0 otf
FileType 0x062e9ee0 ttc
#FileType 0x2f7d0006 tga
#FileType 0x2f7d0004 png
#FileType 0x2f7d0002 jpg 
#FileType 0x2f7d0002 jpeg 
#FileType 0x03b4c61d lightingdata
#FileType 0xea5118b0 swb
#Audio files
#FileType 0x02b9f662 prop
#FileType 0x010077c4 wav
#FileType 0x010077bb mp3
#FileType 0x010077ca xa	   
#FileType 0x01a527db snr
#FileType 0x01eef63a sns
#FileType 0x0181b0d2 abk
#FileType 0x02c9eff2 submix  
#FileType 0x029e333b voice
FileType 0x03b33ddf mod
FileType 0x0604abda dreamtree

Priority 501
DirectoryFiles Files/… autoupdate
Priority 500
PackedFile Packages/.package
PackedFile Packages//.package
PackedFile Packages///.package
PackedFile Packages////.package
PackedFile Packages/////*.package
";
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
    [Export]
    PackedScene RefreshingPS;
    [Export]
    PackedScene ErrorListPS;
    [Export]
    PackedScene EditLoadOrderPS;
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

    public int FilesInPackagesFolder = 0;
    
    private  bool _stoprunning;
    public  bool StopRunning 
    {
        get { return  _stoprunning; }
        set {  _stoprunning = value; 
        if (value) if (LoopState != null) if (!LoopState.IsStopped) LoopState.Stop(); }
    }
    


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

    ConcurrentBag<SimsPackage> readPackages = new();

    public override void _Ready()
    {
        AdminWarningWindow.Visible = false;
        AdminWarningWindow.NoButton.Visible = false;
        AdminWarningWindow.YesButton.Pressed += () => AcceptAdminWarning();
        AdminWarningWindow.WindowMessage.Text = "SCCM requires administrator access to your game folder to deploy root mods. Either reload the app with elevated permissions, disable root mods, or move your install out of protected folders.";
        AdminWarningWindow.WindowTitle = "Elevated Permissions Required for Root";
        UIGameStartControls.PackageDisplay = this;
        UISettingsAndHelpControls.packageDisplay = this;
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



        PackagesFolderWatchTimer = new()
        {
            WaitTime = 25,
            OneShot = false,
            Autostart = false           
        };
        AddChild(PackagesFolderWatchTimer);
        PackagesFolderWatchTimer.Timeout += PackagesFolderWatch;



        AddFolderDialog.DirSelected += (directory) => AddFolderToInstance(directory);

        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("This instance has found {0} files.", ThisInstance.Files.Count));
        if (ThisInstance.Headers != null && ThisInstance.Headers.Count > 0) { 
            
            
            UIAllModsContainer.DataGridHeaders = ThisInstance.Headers;
        } else
        {            
            ThisInstance.Headers = UIAllModsContainer.DataGridHeaders;
            ThisInstance.WriteXML();
        }
        for (int i = 0; i < UIAllModsContainer.DataGridHeaders.Count; i++)
        {
            UIAllModsContainer.DataGridHeaders[i].HeaderIdx = i;
            UIAllModsContainer.DataGridHeaders[i].ItemIndex = i;
        }

        FilesInPackagesFolder = Directory.EnumerateFiles(ThisInstance.InstanceFolders.InstancePackagesFolder, "*.*", SearchOption.AllDirectories).Count();

        InitializeUIAllMods();
        UIAllModsContainer.PackagesDataChanged += () => PackagesChanged();
        
        PackagesChanged();
    }


    public void LinkPackageDataChanged()
    {
        foreach (SimsPackage package in ThisInstance.Packages)
        {
            package.DataChanged += (data) => UIAllModsContainer.UpdateItem(package, data);
        }
    }

    Godot.Timer PackagesFolderWatchTimer;

    public void PackagesFolderWatch()
    {
        new Thread(() => {
            int files = Directory.EnumerateFiles(ThisInstance.InstanceFolders.InstancePackagesFolder, "*.*", SearchOption.AllDirectories).Count();
            if (files != FilesInPackagesFolder)
            {
                FilesInPackagesFolder = files;
                RefreshFiles();
            } else
            {
                CallDeferred(nameof(StartPackageFolderTimer));
            } 
        }){IsBackground = true}.Start();
               
    }

    ParallelLoopState LoopState;

    bool ReadingPackageDetails = false;
    int modsread = 0;

    private void ReportPackageStatus()
    {
        new Thread(() => {
            while (ReadingPackageDetails)
            {
                if (UIAllModsContainer.ModsRead != modsread)
                {
                    modsread = UIAllModsContainer.ModsRead;
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("PACKAGE READING STATUS UPDATE: Packages: {0}, Read: {1} (UI says {2}), left to read: {3}.", ThisInstance.Packages.Count, ThisInstance.Packages.Count(x => x.HasBeenRead), UIAllModsContainer.ModsRead, ThisInstance.Packages.Count(x => !x.HasBeenRead)));
                }
            }
        }){IsBackground = true}.Start();
    }

    public void ReadPackageDetails()
    {   
        ReadingPackageDetails = true;
        ReportPackageStatus();
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Reading the package details for {0} packages!", ThisInstance.Packages.Count));
        runningTasks = new();
        new Thread(() => {    

            if (ThisInstance.LoadedProfile.EnabledPackages.Count > 0)
            {
                for (int i = 0; i < ThisInstance.LoadedProfile.EnabledPackages.Count; i++)
                {                    
                    int idx = ThisInstance.Packages.IndexOf(ThisInstance.Packages.First(x => x.Identifier == ThisInstance.LoadedProfile.EnabledPackages[i].PackageIdentifier));
                    ThisInstance.Packages[idx].IsEnabled = true;
                    ThisInstance.Packages[idx].LoadOrder = ThisInstance.LoadedProfile.EnabledPackages[i].LoadOrder;
                }
            } 

            UIAllModsContainer.ModsTotal = ThisInstance.Packages.Count;

            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("There are {0} packages and {1} of them have already been read", ThisInstance.Packages.Count(), ThisInstance.Packages.Count(x => x.HasBeenRead)));

            if (ThisInstance.Packages.Any(x => !x.HasBeenRead))
            {
                UIAllModsContainer.ModReadingStage = 0;       
                UIAllModsContainer.ModsRead = ThisInstance.Packages.Count(x => x.HasBeenRead);
                
                int toread = (int)UIAllModsContainer.DataGrid.RowsOnScreen;
                if (toread > ThisInstance.Packages.Count) toread = ThisInstance.Packages.Count;

                Parallel.For(0, toread, GlobalVariables.ParallelSettings, (p, loopState) =>
                {
                    if (LoopState == null) LoopState = loopState; else loopState = LoopState;
                    
                    if (!ThisInstance.Packages[p].HasBeenRead)
                    {
                        if (ThisInstance.Packages[p].IsDirectory)
                        {                            
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Reading: {0}", ThisInstance.Packages[p].FileName));
                            DirectoryInfo fi = new(ThisInstance.Packages[p].Location);                    
                            if (File.Exists(ThisInstance.Packages[p].InfoFile))
                            {
                                bool ReadInfo = ThisInstance.Packages[p].ReadInfoFile();
                                if (!ReadInfo)
                                {
                                    ThisInstance.Packages[p].ReadPackageDetails(ThisInstance);
                                }
                            } else
                            {
                                ThisInstance.Packages[p].ReadPackageDetails(ThisInstance);
                            }                            
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Finished reading {0}", ThisInstance.Packages[p].FileName));
                            UIAllModsContainer.ModsRead++;
                        } else
                        {                            
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Reading details of: {0}", ThisInstance.Packages[p].FileName));
                            FileInfo fi = new(ThisInstance.Packages[p].Location);
                            if (File.Exists(ThisInstance.Packages[p].InfoFile))
                            {
                                bool ReadInfo = ThisInstance.Packages[p].ReadInfoFile();
                                if (!ReadInfo)
                                {
                                    ThisInstance.Packages[p].ReadPackageDetails(ThisInstance);
                                }
                            } else
                            {
                                ThisInstance.Packages[p].ReadPackageDetails(ThisInstance);
                            }
                            InstanceControllers.CheckOrphanSingle(ThisInstance.Packages[p], ThisInstance.Files.OfType<SimsPackage>().Where(x => x.HasBeenRead).ToList(), ThisInstance);
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Finished reading {0}", ThisInstance.Packages[p].FileName));
                            
                            UIAllModsContainer.ModsRead++;
                        }
                    }             
                });

                
                if (toread < ThisInstance.Packages.Count)
                {
                    Parallel.For((int)UIAllModsContainer.DataGrid.RowsOnScreen, ThisInstance.Packages.Count, GlobalVariables.ParallelSettings, (p, loopState) =>
                    {
                        if (LoopState == null) LoopState = loopState; else loopState = LoopState;
                        if (!ThisInstance.Packages[p].HasBeenRead)
                        {
                            if (ThisInstance.Packages[p].IsDirectory)
                            {
                                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Reading: {0}", ThisInstance.Packages[p].FileName));
                                    DirectoryInfo fi = new(ThisInstance.Packages[p].Location); 
                                    ThisInstance.Packages[p].ReadPackageDetails(ThisInstance);
                                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Finished reading {0}", ThisInstance.Packages[p].FileName));
                                    if (IsInstanceValid(UIAllModsContainer)) UIAllModsContainer.ModsRead++;
                            } else
                            {
                                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Reading details of: {0}", ThisInstance.Packages[p].FileName));
                                    FileInfo fi = new(ThisInstance.Packages[p].Location);
                                    ThisInstance.Packages[p].ReadPackageDetails(ThisInstance);
                                    InstanceControllers.CheckOrphanSingle(ThisInstance.Packages[p], ThisInstance.Files.OfType<SimsPackage>().Where(x => x.HasBeenRead).ToList(), ThisInstance);
                                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Finished reading {0}", ThisInstance.Packages[p].FileName));
                                    if (IsInstanceValid(UIAllModsContainer)) UIAllModsContainer.ModsRead++;
                            }
                        }
                    });
                }                

                UIAllModsContainer.ModReadingStage = 1;

                if (ThisInstance.Packages.Any(x => x.Orphan))
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("There are still {0} orphans. Doing a quick check...", ThisInstance.Files.OfType<SimsPackage>().Count(x => x.Orphan)));
                    Parallel.For(0, ThisInstance.Packages.Count, GlobalVariables.ParallelSettings, (i, loopState) =>
                    {
                        if (LoopState == null) LoopState = loopState; else loopState = LoopState;
                        if (!ThisInstance.Packages[i].IsDirectory)
                        {               
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking for matching meshes or recolors for: {0}", ThisInstance.Packages[i].FileName));
                            InstanceControllers.CheckOrphanSingle(ThisInstance.Packages[i], ThisInstance.Files.OfType<SimsPackage>().Where(x => x.HasBeenRead).ToList(), ThisInstance);
                        }
                    });
                }

                Parallel.For(0, ThisInstance.Packages.Count, GlobalVariables.ParallelSettings, (p, loopState) =>
                {
                    if (LoopState == null) LoopState = loopState; else loopState = LoopState;
                    if (ThisInstance.Packages[p].MatchingRecolors != null) if (ThisInstance.Packages[p].MatchingRecolors.Contains(ThisInstance.Packages[p].FileName)) { 
                        ThisInstance.Packages[p].MatchingRecolors.Remove(ThisInstance.Packages[p].FileName);                        
                        if (ThisInstance.Packages[p].MatchingRecolors.Count == 0) ThisInstance.Packages[p].MatchingRecolors = null;
                    }
                    if (ThisInstance.Packages[p].Type == "Unknown" && ThisInstance.Packages[p].MatchingRecolors != null)
                    {
                        if (ThisInstance.Packages[p].MatchingRecolors.Count > 0) if (ThisInstance.Packages.Any(x => x.FileName == ThisInstance.Packages[p].MatchingRecolors[0])) ThisInstance.Packages[p].Type = ThisInstance.Packages.First(x => x.FileName == ThisInstance.Packages[p].MatchingRecolors[0]).Type;
                    } else if (ThisInstance.Packages[p].Type == "Unknown" && string.IsNullOrEmpty(ThisInstance.Packages[p].MatchingMesh))
                    {
                        if (ThisInstance.Packages.Any(x => x.FileName == ThisInstance.Packages[p].MatchingMesh)) ThisInstance.Packages[p].Type = ThisInstance.Packages.First(x => x.FileName == ThisInstance.Packages[p].MatchingMesh).Type;
                    }
                    if (ThisInstance.Packages[p].Mesh && ThisInstance.Packages[p].Recolor)
                    {
                        ThisInstance.Packages[p].Orphan = false;
                    }
                    UIAllModsContainer.UpdateItem(ThisInstance.Packages[p], null);
                    ThisInstance.Packages[p].WriteXML();
                });

                Parallel.For(0, ThisInstance.Packages.Count, GlobalVariables.ParallelSettings, (p, loopState) =>
                {
                    if (LoopState == null) LoopState = loopState; else loopState = LoopState;
                    try { 
                        if (ThisInstance.Packages[p].PackageData.IndexEntries.Any(ie => ThisInstance.Packages.Any(i => i.PackageData.IndexEntries.Any(ip => ip.TypeID == ie.TypeID && ip.GroupID == ie.GroupID && ie.InstanceID == ip.InstanceID && i.Identifier != ThisInstance.Packages[p].Identifier)))){
                            ThisInstance.Packages[p].CheckDuplicates(ThisInstance);
                        }    
                    } catch (Exception e)
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Caught exception checking {0} for duplicates. Error: {1} - {2}", ThisInstance.Packages[p].FileName, e.Message, e.StackTrace));
                    }
                    ThisInstance.Packages[p].WriteXML();
                });
            }            
            if (IsInstanceValid(UIAllModsContainer)) UIAllModsContainer.ModReadingStage = 2;            
            ReadingPackageDetails = false;
            CallDeferred(nameof(StartPackageFolderTimer));
        }){IsBackground = true}.Start();
    }

    private void StartPackageFolderTimer()
    {
        PackagesFolderWatchTimer.Start();
    }

    private void CheckRemainingOrphans()
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking remaining orphans."));
        for (int i = 0; i < ThisInstance.Packages.Count; i++)
        {
            if (!ThisInstance.Packages[i].IsDirectory)
            {
                Task t = new Task(() => {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking for matching meshes or recolors for: {0}", ThisInstance.Packages[i].FileName));
                    InstanceControllers.CheckOrphanSingle(ThisInstance.Packages[i], ThisInstance.Files.OfType<SimsPackage>().Where(x => x.HasBeenRead).ToList(), ThisInstance);                    
                });
                runningTasks.Add(t);
            }
        }
        Task w = new Task(() => {
            Thread.Sleep(5);
        });
        runningTasks.Add(w);  
    }

    
    private void ReadPackageDetailsGroup(int startidx, int num)
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Reading packages from {0} to {1} ({2} packages).", startidx, startidx + num, num));
        for (int i = startidx; i < (startidx + num); i++)
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Reading package {0}: {1}", i, ThisInstance.Packages[i].FileName));
            if (ThisInstance.Packages[i].IsDirectory)
            {
                Task t = new Task(() => {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Reading: {0}", ThisInstance.Packages[i].FileName));
                    DirectoryInfo fi = new(ThisInstance.Packages[i].Location);                    
                    ThisInstance.Packages[i].ReadPackageDetails(ThisInstance);
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Finished reading {0}", ThisInstance.Packages[i].FileName));
                    UIAllModsContainer.ModsRead++;
                });
                runningTasks.Add(t);
            } else
            {
                Task t = new Task(() => {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Reading details of: {0}", ThisInstance.Packages[i].FileName));
                    FileInfo fi = new(ThisInstance.Packages[i].Location);
                    ThisInstance.Packages[i].ReadPackageDetails(ThisInstance);
                    InstanceControllers.CheckOrphanSingle(ThisInstance.Packages[i], ThisInstance.Files.OfType<SimsPackage>().Where(x => x.HasBeenRead).ToList(), ThisInstance);
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Finished reading {0}", ThisInstance.Packages[i].FileName));
                    UIAllModsContainer.ModsRead++;
                });
                runningTasks.Add(t);
            }
        }
        Task w = new Task(() => {
            Thread.Sleep(5);
        });
        runningTasks.Add(w);   
    }

    

    private void UpdateInitialRead(SimsPackage simsPackage)
    {   
        /*while (UIAllModsContainer.DoingInitialUpdate)
        {
            //wait
        }*/
        ThisInstance.Files[ThisInstance.Files.IndexOf(ThisInstance.Files.OfType<SimsPackage>().First(x => x.Identifier == simsPackage.Identifier))] = simsPackage;
        UIAllModsContainer.InitialReadUpdate(simsPackage);        
    }

    /*private void UpdateItem(string id, string name)
    {
        Guid Id = Guid.Parse(id);        
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("ID of {0} for updating: {1}", name, Id));
        SimsPackage package = readPackages.First(x => x.Identifier == Id);
        ThisInstance.Files[ThisInstance.Files.IndexOf(ThisInstance.Files.OfType<SimsPackage>().First(x => x.Identifier == Id))] = package;
        UIAllModsContainer.UpdateItem(package);
    }*/

    private void PackagesChanged()
    {
        SetErrors();
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
        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(directory, newloc);
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

    public ViewErrorsDisplay VED;

    public void ShowErrorsScreen()
    {
        VED = ErrorListPS.Instantiate() as ViewErrorsDisplay;
        VED.PackagesMovedOrDeleted += () => PackagesAdjusted();
        VED.packages = [..ThisInstance.Files.OfType<SimsPackage>()];
        VED.packageDisplay = this;
        VED.CloseButton.Pressed += () => CloseErrorsScreen();
        VED.ShowErrors();

        AddChild(VED);
        LockInput = true;
    }

    private void PackagesAdjusted()
    {        
        RefreshFiles();
        PackagesChanged();
    }


    public void CloseErrorsScreen()
    {
        RemoveChild(VED);
        VED.QueueFree();
        LockInput = false;
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
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: Export csv pressed.", but));
                ExportCSV();
            break;
            case 4:
                ExportProfile();
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
            case 7:
                EditLoadOrder();
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}: Edit loadorder pressed.", but));
            break;
        }
    }

    EditLoadOrder EDO;

    private void EditLoadOrder()
    {
        LockInput = true;
        EDO = EditLoadOrderPS.Instantiate() as EditLoadOrder;
        EDO.PackageList = ThisInstance.Packages.Where(x => x.IsEnabled).ToList();
        EDO.CloseEditLoadOrder += (b) => CloseEditLoadOrder(b);
        AddChild(EDO);
    }

    private void CloseEditLoadOrder(bool Save)
    {
        if (!Save)
        {
            RemoveChild(EDO);
            EDO.QueueFree();
        } else
        {
            foreach (LoadOrderItem item in EDO.LoadOrderItems)
            {
                int idx = ThisInstance.Packages.IndexOf(ThisInstance.Packages.First(x => x.Identifier == item.Identifier));
                ThisInstance.Packages[idx].AllowInvokeDataChanged = true;
                ThisInstance.Packages[idx].LoadOrder = item.LoadOrder;                
                ThisInstance.LoadedProfile.EnabledPackages.First(x => x.PackageIdentifier == item.Identifier).LoadOrder = item.LoadOrder;
            }
            ThisInstance.WriteXML();
            RemoveChild(EDO);
            EDO.QueueFree();
        }
        LockInput = false;
    }

    private void ExportProfile()
    {
        List<SimsPackage> packages = new();
        foreach (SimsPackage package in ThisInstance.Files.OfType<SimsPackage>())
        {
            if (package.IsEnabled)
            {
                packages.Add(package);
            }
        }

        string folder = Path.Combine(ThisInstance.LoadedProfile.ProfileFolder, "Exports");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        folder = Path.Combine(folder, ThisInstance.LoadedProfile.ProfileName);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        
        foreach (SimsPackage package in packages)
        {
            string rename = string.Format("{0}_{1}", package.LoadOrder, package.FileName);
            string moveto = Path.Combine(folder, rename);
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Copying {0} to export folder: {1}", package.FileName, moveto));
            
            if (File.Exists(package.Location))
            {
                File.Copy(package.Location, moveto);
            } else if (Directory.Exists(package.Location))
            {
                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(package.Location, moveto);
            }
            
        }

    }


    public void ExportCSV()
    {
        List<SimsCSV> CSVFile = new();
        foreach (SimsPackage package in ThisInstance.Files.OfType<SimsPackage>())
        {
            SimsCSV csv = new();
            csv.GetFromPackage(package);
            CSVFile.Add(csv);
        }
        
        using (var writer = new StreamWriter(Path.Combine(ThisInstance.LoadedProfile.ProfileFolder, "PackageList.csv")))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(CSVFile);
        }
    }

    public ConcurrentBag<Task> runningTasks = new();

    public void RefreshFiles()
    {
        new Thread(() => {
        runningTasks = new();        
        
            TogglePleaseWait(true);

            //List<SimsPackage> newpackages = [];

            List<SimsPackage> gonepackages = [];
            List<SimsDownload> newdownloads = [];

            List<SimsDownload> gonedownloads = [];

            ThisInstance._packages = new();
            ThisInstance._downloads = new();
            
            List<string> allLocations = [..ThisInstance.Files.Select(x => x.Location)];
            List<string> allfound = new();

            List<string> files = Directory.GetFiles(ThisInstance.InstanceFolders.InstancePackagesFolder).ToList();
            List<string> folders = Directory.GetDirectories(ThisInstance.InstanceFolders.InstancePackagesFolder).ToList();

            List<string> catFolders = folders.Where(x => x.Contains("__CATEGORY_")).ToList();
            foreach (string dir in catFolders)
            {
                files.AddRange(Directory.GetFiles(dir));
                folders.AddRange(Directory.GetDirectories(dir));
            }

            foreach (string file in files)
            {
                allfound.Add(file);
                FileInfo fi = new(file);
                if (!ThisInstance.Files.Any(x=>x.Location == file) && GlobalVariables.SimsFileExtensions.Contains(fi.Extension))
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found new file: {0}", file));
                                        
                    SimsPackage simsPackage = new();
                    simsPackage.Location = file; 
                    ThisInstance._packages.Add(simsPackage);
                }
            }

            foreach (string file in folders)
            {            
                allfound.Add(file);
                DirectoryInfo di = new(file);
                if (!di.Name.StartsWith("__CATEGORY_"))
                {
                    if (!ThisInstance.Files.Any(x=>x.Location == file))
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                        DirectoryInfo fi = new(file);
                        SimsPackage simsPackage = new();
                        simsPackage.Location = file;                        
                        ThisInstance._packages.Add(simsPackage);
                    }
                }
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
            UIAllModsContainer.ReplaceFiles(newPackages, gonepackages);
            ReadPackageDetails();
        }){IsBackground = true}.Start();
    }

    public void TogglePleaseWait(bool Show)
    {
        CallDeferred(nameof(DeferredPleaseWait), Show);
    }

    public void SetErrors()
    {        
        CallDeferred(nameof(SetErrorsDeferred),ThisInstance.Packages.Any(x => x.Orphan || x.Broken || x.OutOfDate || x.WrongGame));
    }

    private void SetErrorsDeferred(bool errors)
    {
        UISettingsAndHelpControls.ErrorCount = ThisInstance._packages.Count(x => x.Broken || x.WrongGame || x.Orphan || x.OutOfDate);
        UISettingsAndHelpControls.ErrorsDetected = errors;
    }

    private void DeferredPleaseWait(bool Show)
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
            List<Guid> hide = new();
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
                    matching.AddRange(ThisInstance.Packages.Where(x => x.PackageCategory.Identifier == cat.Identifier));
                }
                foreach (SimsPackage p in matching)
                {
                    hide.Add(p.Identifier);
                }
            }
            UIAllModsContainer.HiddenRows = hide;
            categorymanagement.QueueFree();
        }        
    }

    private void InstanceEdited()
    {
        ThisInstance.WriteXML();
        //UIAllModsContainer.UpdateProfilePackages();
    }


    private void ProfileChanged(string profile, int idx)
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Profile changed to {0}: {1}", idx, profile));
        InstanceProfile pickedprofile = ThisInstance.InstanceProfiles.First(x => x.ProfileName == profile);
        CurrentProfile = pickedprofile;       
        
    }

    private void EnabledFromProfile()
    {
        
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Running Enabled from Profile"));
        new Thread(() => {
            UIAllModsContainer.DataGrid.DontAnnounceEdit = true;
            
            if (!ThisInstance.Packages.Any(x => ThisInstance.LoadedProfile.EnabledPackages.Any(p => p.PackageIdentifier == x.Identifier)))
            {
                List<EnabledPackages> ep = ThisInstance.LoadedProfile.EnabledPackages.Where(x => !ThisInstance.Packages.Any(p => p.Identifier == x.PackageIdentifier)).ToList();
                foreach (EnabledPackages en in ep)
                {
                    ThisInstance.LoadedProfile.EnabledPackages.Remove(en);
                }
                ThisInstance.WriteXML();
            }

            for (int p = 0; p < ThisInstance.Packages.Count; p++){
                if (ThisInstance.LoadedProfile.EnabledPackages.Any(x => x.PackageIdentifier == ThisInstance.Packages[p].Identifier))
                {
                    EnabledPackages ep = ThisInstance.LoadedProfile.EnabledPackages.First(x => x.PackageIdentifier == ThisInstance.Packages[p].Identifier);
                    ThisInstance.Packages[p].IsEnabled = true;
                    ThisInstance.Packages[p].LoadOrder = ep.LoadOrder;     
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("PROFILE CHANGE: {0} has package {1} enabled with load order {2}", CurrentProfile.ProfileName, ThisInstance.Packages[p].FileName, ep.LoadOrder));  
                } else
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("PROFILE CHANGE: {0} has package {1} is not enabled.", CurrentProfile.ProfileName, ThisInstance.Packages[p].FileName));  
                    ThisInstance.Packages[p].LoadOrder = -1;
                    ThisInstance.Packages[p].IsEnabled = false;
                }
            }
            //CallDeferred(nameof(DeferredUpdatePP));
            UIAllModsContainer.DataGrid.DontAnnounceEdit = false;
        }){IsBackground = true}.Start();
        
    }

    private void DeferredUpdatePP()
    {
        //UIAllModsContainer.UpdateProfilePackages();
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


    public bool LinkFiles(string exe)
    {
        if (File.Exists(GlobalVariables.MovedItemsFile))
        {
            InstanceControllers.ClearInstance();
        }
        if (!GlobalVariables.IsElevated)
        {
            if (ThisInstance.Files.OfType<SimsPackage>().Any(x => x.RootMod) || ThisInstance.GameChoice == SimsGames.SimsMedieval)
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
                        return false;
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
                        string folderPath = Path.Combine(ThisInstance.LoadedProfile.LocalDataFolder, folder);
                        string destinationPath = Path.Combine(ThisInstance.GameDocumentsFolder, folder);
                        if (Directory.Exists(folderPath))
                        {
                            if (Directory.Exists(destinationPath))
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, folder);
                                Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                                VFSFileList.ItemsMoved.Add(new(){ IsFolder = true, MovedTo = moveloc, OriginalLocation = destinationPath});
                            }
                        }
                        
                        VirtualFileSystem.MakeJunction(folderPath, destinationPath);
                        VFSFileList.ItemsLinked.Add(new () { LinkLocation = destinationPath, IsFolder = true });
                    }
                    foreach (string file in GlobalVariables.Sims2DataFiles)
                    {                        
                        string filePath = Path.Combine(ThisInstance.LoadedProfile.LocalDataFolder, file);
                        string destinationPath = Path.Combine(ThisInstance.GameDocumentsFolder, file);
                        if (File.Exists(filePath))
                        {
                            if (File.Exists(destinationPath))
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, file);
                                Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
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
                        string folderPath = Path.Combine(ThisInstance.LoadedProfile.LocalMediaFolder, folder);
                        string destinationPath = Path.Combine(ThisInstance.GameDocumentsFolder, folder);
                        if (Directory.Exists(folderPath))
                        {
                            if (Directory.Exists(destinationPath))
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, folder);
                                Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
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
                        string folderPath = Path.Combine(ThisInstance.LoadedProfile.LocalSaveFolder, folder);
                        string destinationPath = Path.Combine(ThisInstance.GameDocumentsFolder, folder);
                        if (Directory.Exists(folderPath))
                        {
                            if (Directory.Exists(destinationPath))
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, folder);
                                Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
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
                        string folderPath = Path.Combine(ThisInstance.LoadedProfile.LocalSettingsFolder, folder);
                        string destinationPath = Path.Combine(ThisInstance.GameDocumentsFolder, folder);
                        if (Directory.Exists(folderPath))
                        {
                            if (Directory.Exists(destinationPath))
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, folder);
                                Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                                VFSFileList.ItemsMoved.Add(new(){ IsFolder = true, MovedTo = moveloc, OriginalLocation = destinationPath});
                            }
                        }
                        
                        VirtualFileSystem.MakeJunction(folderPath, destinationPath);
                        VFSFileList.ItemsLinked.Add(new () { LinkLocation = destinationPath, IsFolder = true });
                    } 
                }


            break;
            case SimsGames.Sims3:
                if (exe.Contains("Sims 3 Create A World"))
                {
                    packageFolderLocation = Path.Combine(exe, "Packages");
                    if (!Directory.Exists(packageFolderLocation))
                    {
                        Directory.CreateDirectory(packageFolderLocation);
                    }
                    string resourcecfg = Path.Combine(exe, "Resource.cfg");
                    if (File.Exists(resourcecfg))
                    {
                        string move = Path.Combine(exe, "Resource__Backup.cfg.bk");
                        File.Move(resourcecfg, move);
                    }
                    File.WriteAllText(resourcecfg, CAWResource);
                } else
                {                    
                    packageFolderLocation = ThisInstance.Sims3Folders.ModsFolder;
                }
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
            case SimsGames.SimsMedieval:
                packageFolderLocation = ThisInstance.SimsMedievalFolders.ModsFolder;
                
                if (ThisInstance.LoadedProfile.LocalMedia)
                {
                    foreach (string folder in GlobalVariables.SimsMedievalMediaFolders)
                    {
                        string folderPath = Path.Combine(ThisInstance.LoadedProfile.LocalMediaFolder, folder);
                        string destinationPath = Path.Combine(ThisInstance.GameDocumentsFolder, folder);
                        if (Directory.Exists(folderPath))
                        {
                            if (Directory.Exists(destinationPath))
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, folder);
                                Utilities.MoveExisting(moveloc, true);
                                Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                                VFSFileList.ItemsMoved.Add(new(){ IsFolder = true, MovedTo = moveloc, OriginalLocation = destinationPath});
                            }
                        }
                        
                        VirtualFileSystem.MakeJunction(folderPath, destinationPath);
                        VFSFileList.ItemsLinked.Add(new () { LinkLocation = destinationPath, IsFolder = true });
                    }  
                }
                if (ThisInstance.LoadedProfile.LocalSaves)
                {
                    foreach (string folder in GlobalVariables.SimsMedievalSavesFolders)
                    {
                        string folderPath = Path.Combine(ThisInstance.LoadedProfile.LocalSaveFolder, folder);
                        string destinationPath = Path.Combine(ThisInstance.GameDocumentsFolder, folder);
                        if (Directory.Exists(folderPath))
                        {
                            if (Directory.Exists(destinationPath))
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, folder);
                                Utilities.MoveExisting(moveloc, true);
                                Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(destinationPath, moveloc);
                                VFSFileList.ItemsMoved.Add(new(){ IsFolder = true, MovedTo = moveloc, OriginalLocation = destinationPath});
                            }
                        }
                        
                        VirtualFileSystem.MakeJunction(folderPath, destinationPath);
                        VFSFileList.ItemsLinked.Add(new () { LinkLocation = destinationPath, IsFolder = true });
                    }
                    foreach (string file in GlobalVariables.SimsMedievalSavesFiles)
                    {                        
                        string filePath = Path.Combine(ThisInstance.LoadedProfile.LocalSaveFolder, file);
                        string destinationPath = Path.Combine(ThisInstance.GameDocumentsFolder, file);
                        if (File.Exists(filePath))
                        {
                            if (File.Exists(destinationPath))
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, file);
                                Utilities.MoveExisting(moveloc);
                                File.Move(destinationPath, moveloc);
                                VFSFileList.ItemsMoved.Add(new(){ IsFolder = false, MovedTo = moveloc, OriginalLocation = destinationPath});
                            }
                        }
                        
                        VirtualFileSystem.MakeSymbolicLink(filePath, destinationPath);
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Linking: {0}", destinationPath));
                        VFSFileList.ItemsLinked.Add(new () { LinkLocation = destinationPath, IsFolder = false });
                    }
                }
                if (ThisInstance.LoadedProfile.LocalSettings)
                {
                     foreach (string file in GlobalVariables.SimsMedievalSettingsFiles)
                    {                        
                        string filePath = Path.Combine(ThisInstance.LoadedProfile.LocalSettingsFolder, file);
                        string destinationPath = Path.Combine(ThisInstance.GameDocumentsFolder, file);
                        if (File.Exists(filePath))
                        {
                            if (File.Exists(destinationPath))
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", destinationPath));
                                string moveloc = Path.Combine(GlobalVariables.TempFolder, file);
                                Utilities.MoveExisting(moveloc);
                                File.Move(destinationPath, moveloc);
                                VFSFileList.ItemsMoved.Add(new(){ IsFolder = false, MovedTo = moveloc, OriginalLocation = destinationPath});
                            }
                        }                        
                        VirtualFileSystem.MakeSymbolicLink(filePath, destinationPath);
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Linking: {0}", destinationPath));
                        VFSFileList.ItemsLinked.Add(new () { LinkLocation = destinationPath, IsFolder = false });
                    }
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
                    if (File.Exists(pathcheck))
                    {                 
                        FileInfo f = new(file);
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("EXISTS: {0}", pathcheck));
                        string moveloc = Path.Combine(GlobalVariables.TempFolder, f.Name);
                        Utilities.MoveExisting(moveloc);
                        File.Move(pathcheck, moveloc);
                        VFSFileList.ItemsMoved.Add(new(){ IsFolder = false, MovedTo = moveloc, OriginalLocation = pathcheck});
                    }
                    VFSFileList.ItemsLinked.Add(new() { IsFolder = false, LinkLocation = pathcheck});
                    VirtualFileSystem.MakeSymbolicLink(file, pathcheck);
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
                    Utilities.MoveExisting(moveloc);
                    File.Move(newpath, moveloc);
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
                            Utilities.MoveExisting(moveloc);
                            File.Move(newp, moveloc);
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

    public RefreshingWait RW;

    private void DisplayRefresher()
    {
        RW = RefreshingPS.Instantiate() as RefreshingWait;
        AddChild(RW);
    }

    private void RemoveRefresher()
    {
        RemoveChild(RW);
        RW.QueueFree();
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
                    Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(og, ren);
                } else {
                    File.Move(og, ren);
                }                    
            } catch (Exception e) {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Caught exception renaming disabled folder: {0}\nException: {1}", Item, e.Message));                    
            }
        }
    }
}
