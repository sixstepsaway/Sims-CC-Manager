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
using System.Threading;

public partial class NewInstance : MarginContainer
{
    [Export]
    Background BackgroundNode;
    [Export]
    MarginContainer Page1;
    [Export]
    MarginContainer Page2;
    [Export]
    MMButton Pg1ConfirmButton;
    [Export]
    MMButton Pg1CancelButton;
    [Export]
    MMButton Pg2ConfirmButton;
    [Export]
    MMButton Pg2CancelButton;
    [Export]
    CustomCheckButton FromDownloadsCheck;
    [Export]
    GamePickerBox Sims2;
    [Export]
    GamePickerBox Sims3;
    [Export]
    GamePickerBox Sims4;
    [Export]
    LineEdit InstanceName;

    [Export]
    LineEdit InstallFolderLE;
    [Export]
    Button InstallFolderBrowse;
    

    [Export]
    LineEdit DocsFolderLE;
    [Export]
    Button DocsFolderBrowse;
    

    [Export]
    LineEdit InstanceFolderLE;
    [Export]
    Button InstanceFolderBrowse;
    

    [Export]
    LineEdit PackagesFolderLE;
    [Export]
    Button PackagesFolderBrowse;    

    [Export]
    LineEdit DownloadsFolderLE;
    [Export]
    Button DownloadsFolderBrowse;    

    [Export]
    LineEdit ProfilesFolderLE;
    [Export]
    Button ProfilesFolderBrowse;    

    [Export]
    LineEdit DataCacheFolderLE;
    [Export]
    Button DataCacheFolderBrowse;

    [Export]
    FileDialog fileDialog;
    [Export]
    ConfirmationDialog confirmationDialog;

    bool CreateFromCurrent = false;

    SimsGames GameSelected = new();

    string instanceName = "";
    [Export]
    MarginContainer acceptDialog;
    [Export]
    Label acceptDialogText;
    [Export]
    Button acceptDialogOK;
    [Export]
    Button acceptDialogBack;
    [Export]
    Background acceptDialogBG;
    [Export]
    Label acceptDialogLabelA;
    [Export]
    Label acceptDialogLabelB;
    [Export]
    Panel acceptDialogButtonPanelMain;
    [Export]
    Panel acceptDialogButtonPanelHover;
    [Export]
    MarginContainer acceptDialogButtonContainerOK;
    [Export]
    MarginContainer acceptDialogButtonContainerBack;
    [Export]
    Control BGControl;

    int AcceptDialogVersion = -1;

    GameInstance currentInstance;
    
    public override void _Ready()
    {        
        Page1.Visible = true;
        Page2.Visible = false;
        FromDownloadsCheck.CheckToggled += (val) => FromDownloadsCheckToggled(val);
        Sims2.thisGame = SimsGames.Sims2;
        Sims3.thisGame = SimsGames.Sims3;
        Sims4.thisGame = SimsGames.Sims4;
        Sims2.GamePicked += (game, selected) => GamePicked(game, selected);
        Sims3.GamePicked += (game, selected) => GamePicked(game, selected);
        Sims4.GamePicked += (game, selected) => GamePicked(game, selected);
        Pg1ConfirmButton.ButtonClicked += () => Pg1ConfirmClicked();
        
        acceptDialogOK.Pressed += () => CloseAcceptDialog();
        acceptDialogBack.Pressed += () => AcceptDialogGoBack();
        UpdateDialogThemes();


        acceptDialogOK.MouseEntered += () => MouseHoverAcceptDialogOK(true);
        acceptDialogOK.MouseExited += () => MouseHoverAcceptDialogOK(false);

        Pg1CancelButton.ButtonClicked += () => Pg1Close();
        Pg2CancelButton.ButtonClicked += () => BackToPg1();
        Pg2ConfirmButton.ButtonClicked += () => Pg2ConfirmClicked();
    }
    private void ShowAcceptDialog(int Version)
    {
        AcceptDialogVersion = Version; 
        acceptDialog.Visible = true;
        BGControl.MouseFilter = MouseFilterEnum.Stop;
        if (Version == 1)
        {
            acceptDialogButtonContainerBack.Visible = false;
            acceptDialogText.Text = "Please choose a game to create an instance for.";
        } else if (Version == 2)
        {
            acceptDialogButtonContainerBack.Visible = true;
            acceptDialogText.Text = "This will create an instance from the game folder that already exists. Press OK if you're sure, or back to change your mind.";
        }
    }

    private void AcceptDialogGoBack()
    {
        acceptDialog.Visible = false;
        BGControl.MouseFilter = MouseFilterEnum.Ignore;    
    }

    private void MouseHoverAcceptDialogOK(bool v)
    {
        
        acceptDialogButtonPanelHover.Visible = v;
        acceptDialogButtonPanelMain.Visible = !v;
    }


    private void CloseAcceptDialog()
    {
        if (AcceptDialogVersion == 2)
        {
            acceptDialog.Visible = false;
            BGControl.MouseFilter = MouseFilterEnum.Ignore;
            CreateInstance();
        } else
        {
            acceptDialog.Visible = false;
            BGControl.MouseFilter = MouseFilterEnum.Ignore;
        }        
    }

    private void UpdateDialogThemes()
    {
        acceptDialogBG.UpdateTheme();
        acceptDialogLabelA.AddThemeColorOverride("font_color", GlobalVariables.LoadedTheme.MainTextColor);
        acceptDialogLabelB.AddThemeColorOverride("font_color", GlobalVariables.LoadedTheme.MainTextColor);
        StyleBoxFlat sb = acceptDialogButtonPanelMain.GetThemeStylebox("panel") as StyleBoxFlat;
        sb.BgColor = GlobalVariables.LoadedTheme.BackgroundColor;
        sb.BorderColor = GlobalVariables.LoadedTheme.AccentColor;
        acceptDialogButtonPanelMain.AddThemeStyleboxOverride("panel", sb);
        StyleBoxFlat sbh = acceptDialogButtonPanelHover.GetThemeStylebox("panel") as StyleBoxFlat;
        sbh.BgColor = GlobalVariables.LoadedTheme.DataGridSelected;
        sbh.BorderColor = GlobalVariables.LoadedTheme.AccentColor;
        acceptDialogButtonPanelHover.AddThemeStyleboxOverride("panel", sbh);
    }

    private void Pg1ConfirmClicked()
    {
        if (GameSelected == SimsGames.Null)
        {
            ShowAcceptDialog(1);
        }
        else
        {
            if (InstanceName.Text != "" && !string.IsNullOrEmpty(InstanceName.Text) && !string.IsNullOrWhiteSpace(InstanceName.Text))
            {
                instanceName = InstanceName.Text;
            }
            else
            {
                instanceName = GameSelected.GetDescription();
            }

            ShowPage2();
        }
    }

    private void Pg2ConfirmClicked()
    {
        if (CreateFromCurrent) { 
            ShowAcceptDialog(2); 
        } else {
            CreateInstance();
        }
    }

    private void CreateInstance()
    {
        currentInstance.GameChoice = GameSelected;
        
        GlobalVariables.mainWindow.LoadingPackageDisplayStart();
        new Thread(() => {
        //content
            GlobalVariables.mainWindow.IncrementLoadingScreen(10, "Creating folders...", "New Instance: First");
            currentInstance.InstanceFolders = new();
            if (PackagesFolderLE.Text.Contains("%INSTANCE%"))
            {
                currentInstance.InstanceFolders.InstancePackagesFolder = InstancedFolderFromPlaceholder(PackagesFolderLE.Text);
            } else
            {
                currentInstance.InstanceFolders.InstancePackagesFolder = PackagesFolderLE.Text;
            }            
            if (DownloadsFolderLE.Text.Contains("%INSTANCE%"))
            {
                currentInstance.InstanceFolders.InstanceDownloadsFolder = InstancedFolderFromPlaceholder(DownloadsFolderLE.Text);
            } else
            {
                currentInstance.InstanceFolders.InstanceDownloadsFolder = DownloadsFolderLE.Text;
            }
            if (ProfilesFolderLE.Text.Contains("%INSTANCE%"))
            {
                currentInstance.InstanceFolders.InstanceProfilesFolder = InstancedFolderFromPlaceholder(ProfilesFolderLE.Text);
            } else
            {
                currentInstance.InstanceFolders.InstanceProfilesFolder = ProfilesFolderLE.Text;
            }
            if (DataCacheFolderLE.Text.Contains("%INSTANCE%"))
            {
                currentInstance.InstanceFolders.InstanceDataFolder = InstancedFolderFromPlaceholder(DataCacheFolderLE.Text);
            } else
            {
                currentInstance.InstanceFolders.InstanceDataFolder = DataCacheFolderLE.Text;
            }
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Instance Packages Folder: {0}", currentInstance.InstanceFolders.InstancePackagesFolder));
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Instance Downloads Folder: {0}", currentInstance.InstanceFolders.InstanceDownloadsFolder));
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Instance Profiles Folder: {0}", currentInstance.InstanceFolders.InstanceProfilesFolder));
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Instance DataCache Folder: {0}", currentInstance.InstanceFolders.InstanceDataFolder));
            currentInstance.CheckExisting();   
            currentInstance.BuildInstanceFolders();
            GlobalVariables.mainWindow.IncrementLoadingScreen(10, "Checking game data...", "New Instance: Game Data Check");
            currentInstance.MakeFolderTree();
            GlobalVariables.mainWindow.IncrementLoadingScreen(10, "Checking game version...", "New Instance: Game Version Check");
            currentInstance.GetGameVersion();            
            GlobalVariables.mainWindow.IncrementLoadingScreen(10, "Saving instance information...", "New Instance: Instanec Info Check");
            currentInstance.CreateDefaults();
            GlobalVariables.mainWindow.IncrementLoadingScreen(10, "Retrieving current data...", "New Instance: Current Data Check");  
            if (CreateFromCurrent) { 
                currentInstance.LoadedProfile.LocalData = true;
                currentInstance.LoadedProfile.LocalSaves = true;
                currentInstance.LoadedProfile.LocalSettings = true;
                currentInstance.LoadedProfile.LocalMedia = true;
                RetrieveFiles(); 
            }
            currentInstance.WriteXML();
            for (int i = 60; i < 100; i++)
            {
                Thread.Sleep(10);
                GlobalVariables.mainWindow.IncrementLoadingScreen(i, "Final checks...", "New Instance: Final Checks");
            }             
            CallDeferred(nameof(FinishLoading));
        }){IsBackground = true}.Start();
    }

    public Dictionary<string, bool> Sims2Folders = new() { { "Cameras", true},{ "Collections", true},{ "Config", true},{ "Downloads", false},{ "LockedBins", true},{ "Logs", true},{ "LotCatalog", true},{ "Movies", true},{ "Music", true},{ "Neighborhoods", true},{ "PackagedLots", true},{ "Paintings", true},{ "PetBreeds", true},{ "PetCoats", true},{ "SC4Terrains", true},{ "Screenshots", true},{ "Storytelling", true},{ "Teleport", true},{ "Thumbnails", true} };
    
    public Dictionary<string,bool> Sims3Folders = new() { { "Collections", true},{ "ContentPatch", true},{ "CurrentGame.sims3", true},{ "Custom Music", true},{ "DCBackup", true},{ "DCCache", true},{ "Downloads", true},{ "Exports", true},{ "FeaturedItems", true},{ "IGACache", true},{ "InstalledWorlds", true},{ "Mods", false},{ "Recorded Videos", true},{ "SavedOutfits", true},{ "Screenshots", true},{ "SigsCache", true},{ "Thumbnails", true}};

    public Dictionary<string,bool> Sims4Folders = new() { { "cachestr", true},{ "ConfigOverride", true},{ "Content", true},{ "Custom Music", true},{ "Mods", false},{ "onlinethumbnailcache", true},{ "Recorded Videos", true},{ "ReticulatedSplinesView", true},{ "Saves", true},{ "Screenshots", true},{ "Tray", true} };

    

    private void RetrieveFiles()
    {
        if (Directory.Exists(currentInstance.GameDocumentsFolder))
        {
            switch (GameSelected)
            {
                case SimsGames.Sims2:                
                    foreach (string folder in Directory.GetDirectories(currentInstance.GameDocumentsFolder))
                    {
                        DirectoryInfo directoryInfo = new(folder);
                        bool move = false;
                        Sims2Folders.TryGetValue(directoryInfo.Name, out move);
                        if (move)
                        {
                            MoveFolder(folder, currentInstance.InstanceFolders.InstanceDataFolder);
                        } else
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("NOT moving {0}!", directoryInfo.Name));
                        }
                    }
                    if (Directory.Exists(currentInstance.Sims2Folders.DownloadsFolder))
                    {
                        foreach (string folder in Directory.GetDirectories(currentInstance.Sims2Folders.DownloadsFolder))
                        {
                            MoveFolder(folder, currentInstance.InstanceFolders.InstancePackagesFolder);                          
                        }
                        foreach (string file in Directory.GetFiles(currentInstance.Sims2Folders.DownloadsFolder))
                        {
                            MoveFile(file, currentInstance.InstanceFolders.InstancePackagesFolder);
                        }
                        Directory.Delete(currentInstance.Sims2Folders.DownloadsFolder);
                    }
                    
                break;
                case SimsGames.Sims3:
                    foreach (string folder in Directory.GetDirectories(currentInstance.GameDocumentsFolder))
                    {
                        DirectoryInfo directoryInfo = new(folder);
                        bool move = false;
                        Sims3Folders.TryGetValue(directoryInfo.Name, out move);
                        if (move)
                        {
                            MoveFolder(folder, currentInstance.InstanceFolders.InstanceDataFolder);
                        } else
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("NOT moving {0}!", directoryInfo.Name));
                        }
                    }
                    if (Directory.Exists(currentInstance.Sims3Folders.DownloadsFolder))
                    {
                        foreach (string folder in Directory.GetDirectories(currentInstance.Sims3Folders.DownloadsFolder))
                        {
                            MoveFolder(folder, currentInstance.InstanceFolders.InstancePackagesFolder);                          
                        }
                        foreach (string file in Directory.GetFiles(currentInstance.Sims3Folders.DownloadsFolder))
                        {
                            MoveFile(file, currentInstance.InstanceFolders.InstancePackagesFolder);
                        }
                    }
                    if (Directory.Exists(currentInstance.Sims3Folders.ModsFolder))
                    {
                        foreach (string folder in Directory.GetDirectories(currentInstance.Sims3Folders.ModsFolder))
                        {
                            MoveFolder(folder, currentInstance.InstanceFolders.InstancePackagesFolder);                           
                        }
                        foreach (string file in Directory.GetFiles(currentInstance.Sims3Folders.ModsFolder))
                        {
                            MoveFile(file, currentInstance.InstanceFolders.InstancePackagesFolder);
                        }
                    }
                    Directory.Delete(currentInstance.Sims3Folders.DownloadsFolder);
                    Directory.Delete(currentInstance.Sims3Folders.ModsFolder);
                    
                break;
                case SimsGames.Sims4: 
                    foreach (string folder in Directory.GetDirectories(currentInstance.GameDocumentsFolder))
                    {
                        DirectoryInfo directoryInfo = new(folder);
                        bool move = false;
                        Sims4Folders.TryGetValue(directoryInfo.Name, out move);
                        if (move)
                        {
                            MoveFolder(folder, currentInstance.InstanceFolders.InstanceDataFolder);
                        } else
                        {
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("NOT moving {0}!", directoryInfo.Name));
                        }
                    }
                    if (Directory.Exists(currentInstance.Sims4Folders.ModsFolder))
                    {
                        foreach (string folder in Directory.GetDirectories(currentInstance.Sims4Folders.ModsFolder))
                        {
                            MoveFolder(folder, currentInstance.InstanceFolders.InstancePackagesFolder);
                        }
                        foreach (string file in Directory.GetFiles(currentInstance.Sims4Folders.ModsFolder))
                        {
                            MoveFile(file, currentInstance.InstanceFolders.InstancePackagesFolder);
                        }
                    }   
                    Directory.Delete(currentInstance.Sims4Folders.ModsFolder);                 
                break;

            }

            currentInstance = InstanceControllers.LoadInstanceFiles(currentInstance);

            /*foreach (string file in Directory.GetFiles(currentInstance.InstanceFolders.InstancePackagesFolder))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                FileInfo fi = new(file);
                if (GlobalVariables.SimsFileExtensions.Contains(fi.Extension))
                {
                    SimsPackage simsPackage = InstanceControllers.ReadPackage(file, currentInstance, fi);                    
                    currentInstance.Files.Add(simsPackage);
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));
                    
                }
                GlobalVariables.mainWindow.IncrementLoadingScreen(1, fi.Name);
            }
            foreach (string file in Directory.GetDirectories(currentInstance.InstanceFolders.InstancePackagesFolder))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found: {0}", file));
                DirectoryInfo fi = new(file);
                SimsPackage simsPackage = InstanceControllers.ReadPackage(file, currentInstance, fi);                
                currentInstance.Files.Add(simsPackage);
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Added: {0}", file));
                   
                
                GlobalVariables.mainWindow.IncrementLoadingScreen(1, fi.Name);
            }
            foreach (string file in Directory.GetFiles(currentInstance.InstanceFolders.InstanceDownloadsFolder))
            {
                FileInfo f = new(file);
                SimsDownload simsDownload = InstanceControllers.ReadDownload(file, f);                
                currentInstance.Files.Add(simsDownload);
                GlobalVariables.mainWindow.IncrementLoadingScreen(1, f.Name);
            } */
        }
    }

    private void MoveFile(string file, string newDirectory)
    {
        FileInfo fileInfo = new(file);
        string newloc = Path.Combine(newDirectory, fileInfo.Name);
        File.Move(file, newloc);
    }

    private void MoveFolder(string folder, string newDirectory)
    {
        DirectoryInfo directoryInfo = new(folder);
        string newloc = Path.Combine(newDirectory, directoryInfo.Name);
        Directory.Move(folder, newloc);   
    }


    private string InstancedFolderFromPlaceholder(string path)
    {
        return path.Replace("%INSTANCE%", currentInstance.InstanceFolder);
    }    

    private void FinishLoading()
    {
        GlobalVariables.mainWindow.LoadPackageDisplay(currentInstance);
    }


    private void ShowPage2()
    {
        GetInstanceInfo();
        InstallFolderLE.Text = currentInstance.GameInstallFolder;
        DocsFolderLE.Text = currentInstance.GameDocumentsFolder;

        if (Directory.Exists(currentInstance.InstanceFolder))
        {
            currentInstance.InstanceFolder = Utilities.IncrementName(currentInstance.InstanceFolder, true);
        }

        InstanceFolderLE.Text = currentInstance.InstanceFolder;
        PackagesFolderLE.Text = @"%INSTANCE%\Packages";
        DownloadsFolderLE.Text = @"%INSTANCE%\Downloads";
        ProfilesFolderLE.Text = @"%INSTANCE%\Profiles";
        DataCacheFolderLE.Text = @"%INSTANCE%\Data";
        Page1.Visible = false;
        Page2.Visible = true;
    }

    private void GetInstanceInfo()
    {
        currentInstance = new();
        currentInstance.InstanceName = instanceName;
        Executable currentExe = new();
        currentInstance.InstanceFolder = Path.Combine(GlobalVariables.AppFolder, string.Format(@"Instances\{0}", instanceName));
        if (GameSelected == SimsGames.Sims2)
        {
            string gameloc = @"SOFTWARE\WOW6432Node\EA GAMES\The Sims 2";
            
            string loc = Utilities.GetPathForExe(gameloc);
            DirectoryInfo fullloc = new(loc);
            FileInfo installexe = GetSims2Exe(fullloc);
            string installdirectory = installexe.DirectoryName;
            string exe = installexe.Name;
            if (Directory.Exists(Path.Combine(GlobalVariables.MyDocuments, @"EA Games\The Sims 2")))
            {
                currentInstance.GameDocumentsFolder = Path.Combine(GlobalVariables.MyDocuments, @"EA Games\The Sims 2");
            }
            else if (Directory.Exists(Path.Combine(GlobalVariables.MyDocuments, @"EA Games\The Sims 2 Legacy")))
            {
                currentInstance.GameDocumentsFolder = Path.Combine(GlobalVariables.MyDocuments, @"EA Games\The Sims 2 Legacy");
            } else
            {
                currentInstance.GameDocumentsFolder = Path.Combine(GlobalVariables.MyDocuments, @"EA Games\The Sims 2 Ultimate Collection");
            }
            currentInstance.GameInstallFolder = fullloc.Parent.FullName;
            currentExe.Path = installdirectory;
            currentExe.ExeName = exe;
        }
        else if (GameSelected == SimsGames.Sims3)
        {
            string gameloc = @"SOFTWARE\WOW6432Node\Sims\The Sims 3";
            string docloc = Path.Combine(GlobalVariables.MyDocuments, @"Electronic Arts\The Sims 3");
            string loc = Utilities.GetPathForExe(gameloc);
            currentInstance.GameInstallFolder = loc;
            loc = Path.Combine(loc, "Game");
            loc = Path.Combine(loc, "Bin");
            currentExe.Path = loc;
            currentExe.ExeName = "TS3W.Exe";
            currentInstance.GameDocumentsFolder = docloc;

        }
        else if (GameSelected == SimsGames.Sims4)
        {
            string gameloc = @"SOFTWARE\Maxis\The Sims 4";
            string loc = Utilities.GetPathForExe(gameloc);
            string docloc = Path.Combine(GlobalVariables.MyDocuments, @"Electronic Arts\The Sims 4");
            currentInstance.GameDocumentsFolder = docloc;
            currentInstance.GameInstallFolder = loc;
            loc = Path.Combine(loc, "Game");
            loc = Path.Combine(loc, "Bin");
            currentExe.Path = loc;
            currentExe.ExeName = "TS4_x64.exe";
        }
        currentExe.Name = "Default";
        currentExe.Selected = true;
        currentInstance.CurrentExecutableIndex = 0;
        currentInstance.Executables = new() {currentExe};
        if (File.Exists(Path.Combine(currentInstance.ExecutablePath, currentInstance.ExecutableName))) if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("The exe \"{0}\" exists!", Path.Combine(currentInstance.ExecutablePath, currentInstance.ExecutableName)));
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Instance: {0}\nInstall Location: {1}\nDocs Folder: {2}\nExe Name: {3}\nInstance Folder: {4}", currentInstance.InstanceName, currentInstance.GameInstallFolder, currentInstance.GameDocumentsFolder, currentInstance.ExecutableName, currentInstance.InstanceFolder));   
    }
    
    private FileInfo GetSims2Exe(DirectoryInfo directory)
    {
        //passed in will be "The Sims 2" so... parent
        directory = directory.Parent;
        //this should be the sims 2 main folder, so Ultimate Collection etc
        List<DirectoryInfo> directories = directory.GetDirectories().ToList();
        //subfolders of this folder 
        if (directories.Where(x => x.Name.Equals("The Sims 2 Mansion and Garden Stuff")).Any())
        {
            string mag = directories.Where(x => x.Name.Equals("The Sims 2 Mansion and Garden Stuff")).First().FullName;
            mag = Path.Combine(mag, "TSBin");
            List<FileInfo> files = new DirectoryInfo(mag).GetFiles().ToList();
            if (files.Where(x => x.Name == "Sims2RPC.exe").Any())
            {
                return new FileInfo(files.Where(x => x.Name == "Sims2RPC.exe").First().FullName);
            }
            else
            {
                return new FileInfo(files.Where(x => x.Name == "Sims2EP9.exe").First().FullName);
            }
        }
        else if (directories.Where(x => x.Name.Equals("Fun with Pets")).Any())
        {
            string fwp = directories.Where(x => x.Name.Equals("Fun with Pets")).First().FullName;
            fwp = Path.Combine(fwp, "SP9");
            fwp = Path.Combine(fwp, "TSBin");
            List<FileInfo> files = new DirectoryInfo(fwp).GetFiles().ToList();
            if (files.Where(x => x.Name == "Sims2RPC.exe").Any())
            {
                return new FileInfo(files.Where(x => x.Name == "Sims2RPC.exe").First().FullName);
            }
            else
            {
                return new FileInfo(files.Where(x => x.Name == "Sims2EP9.exe").First().FullName);
            }
        }
        else if (directories.Where(x => x.Name.Equals("EP9")).Any())
        {
            string fwp = directories.Where(x => x.Name.Equals("EP9")).First().FullName;
            fwp = Path.Combine(fwp, "TSBin");
            List<FileInfo> files = new DirectoryInfo(fwp).GetFiles().ToList();
            if (files.Where(x => x.Name == "Sims2RPC.exe").Any())
            {
                return new FileInfo(files.Where(x => x.Name == "Sims2RPC.exe").First().FullName);
            }
            else
            {
                return new FileInfo(files.Where(x => x.Name == "Sims2EP9.exe").First().FullName);
            }
        } else {
            return new FileInfo("");
        }	
    }

    private void GamePicked(SimsGames game, bool selected)
    {
        GameSelected = game;
        if (selected)
        {
            if (game == SimsGames.Sims2)
            {
                Sims3.Selected = false;
                Sims4.Selected = false;
            }
            else if (game == SimsGames.Sims3)
            {
                Sims2.Selected = false;
                Sims4.Selected = false;
            }
            else if (game == SimsGames.Sims4)
            {
                Sims2.Selected = false;
                Sims3.Selected = false;
            }
        }
    }


    private void FromDownloadsCheckToggled(bool val)
    {
        CreateFromCurrent = val;
        if (GlobalVariables.DebugMode)
        {
            if (CreateFromCurrent){
                Logging.WriteDebugLog("Creating from current.");
            } else {
                Logging.WriteDebugLog("Not creating from current.");
            }
        }
    }


    private void BackToPg1()
    {
        Page1.Visible = true;
        Page2.Visible = false;
    }


    private void Pg1Close()
    {
        QueueFree();
    }
}

