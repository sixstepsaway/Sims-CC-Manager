using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.PackageReaders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using System.Xml.Serialization;

public partial class MainMenu : MarginContainer
{
    PackedScene SettingsPS = GD.Load<PackedScene>("res://UI/MainMenu/MainSettings.tscn");
    PackedScene NewInstancePS = GD.Load<PackedScene>("res://UI/MainMenu/NewInstance.tscn");
    PackedScene LoadInstancePS = GD.Load<PackedScene>("res://UI/MainMenu/LoadInstance.tscn");
    [Export]
    MMButton NewInstanceButton;
    [Export]
    MMButton LoadInstanceButton;
    [Export]
    MMButton SettingsButton;
    [Export]
    MMButton HelpButton;
    [Export]
    MMButton CreditsButton;
    [Export]
    MMButton QuitButton;
    [Export]
    MMButton DevButton;
    [Export]
    Background BackgroundNode;
    [Export]
    Logo LogoNode;
    [Export]
    MarginContainer Tali;
    MainSettings SettingsWindow;
    private MainWindow.ThemeChangedEvent ThemeUpdateEventHandler;
    public NewInstance newInstance;
    public LoadInstance loadInstance;
    [Export]
    public PackedScene SnapshotterPS;
    [Export]
    SubViewport subViewport;
    [Export]
    Window window;
    

    public override void _Ready()
    {
        ThemeUpdateEventHandler = UpdateTheme;
        GlobalVariables.mainWindow.SCCMThemeChanged += ThemeUpdateEventHandler;
        NewInstanceButton.ButtonClicked += () => NewInstanceClicked();
        LoadInstanceButton.ButtonClicked += () => LoadInstanceClicked();
        SettingsButton.ButtonClicked += () => SettingsClicked();
        HelpButton.ButtonClicked += () => HelpClicked();
        DevButton.ButtonClicked += () => DevClicked();
        QuitButton.ButtonClicked += () => ExitClicked();
        CreditsButton.ButtonClicked += () => CreditsClicked();
        if (GlobalVariables.LoadedSettings.InstanceFolders.Count == 0) LoadInstanceButton.Visible = false;
    }

    private void CreditsClicked()
    {
        //
    }


    private void UpdateTheme()
    {
        DevButton.UpdateColors();
        QuitButton.UpdateColors();
        HelpButton.UpdateColors();
        CreditsButton.UpdateColors();
        SettingsButton.UpdateColors();
        LoadInstanceButton.UpdateColors();
        NewInstanceButton.UpdateColors();
        
    }

    private void ExitClicked()
    {
        GetTree().Quit();
    }

    List<SimsPackageReader> sims2packagereaders = new();
    private void DevClicked()
    {     
          
        /*new Thread(() => {
            List<string> files = Directory.EnumerateFiles(@"O:\Godot Projects\SimsCCManager\Test", "*.package", SearchOption.AllDirectories).ToList();
            for (int i = 0; i < files.Count; i++)
            {
                SimsPackageReader sims2PackageReader = new();            
                sims2PackageReader.ReadPackage(files[i]);           
                
                //sims2PackageReader.ReadPackage(@"O:\Godot Projects\SimsCCManager\Test\MLC-KFCBucketToilet.package");
                sims2packagereaders.Add(sims2PackageReader);
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}", sims2PackageReader.ToString()));                
            }
            MakeScreencaps();
        }){IsBackground = true}.Start(); */    

        DirectoryInfo di = new(@"C:\Program Files (x86)\Mr DJ\The Sims 2 Ultimate Collection");
        try { var fs = new FileSecurity(di.FullName, AccessControlSections.All); 
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Allowed!"));}
        catch (Exception e)
        {
            if (e.GetBaseException().GetType() == typeof(PrivilegeNotHeldException))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("womp womp"));
            }  
        }        
    }

    

    private void MakeScreencaps()
    {
        /*for (int i = 0; i < sims2packagereaders.Count; i++){
            if (sims2packagereaders[i].PackageGame == SimsCCManager.OptionLists.SimsGames.Sims2) {
                snapShotted = false;
                CallDeferred(nameof(CreateSnapshotter), i, sims2packagereaders[i].fileinfo.FullName);
                while (!snapShotted)
                {
                    //wait..
                }
                CallDeferred(nameof(QueueSnapshotterFree));                
            }
        }  */
    }
    bool snapShotted = false;
    List<Snapshotter> snapshotters = new();
    Snapshotter currentSnapshotter;

    private void QueueSnapshotterFree()
    {
        currentSnapshotter.QueueFree();
    }

    private void CreateSnapshotter(int index, string flocaiton)
    {
        /*FileInfo fi = new(flocaiton);
        SimsPackageReader data = sims2packagereaders[index];
        Snapshotter snapshotter = SnapshotterPS.Instantiate() as Snapshotter;
        snapshotter.PackageReaders = sims2packagereaders;
        window.AddChild(snapshotter);
        snapshotters.Add(snapshotter);
        currentSnapshotter = snapshotter;
        snapshotter.SnapCompleted += () => SnapCompleted();
        snapshotter.BuildMesh(data, flocaiton);
        string outputloc = string.Format("{0}.png", flocaiton);
        snapshotter.Snapshot(window, fi.DirectoryName, fi.Name);*/
    }

    private void SnapCompleted()
    {
        snapShotted = true;
    }

    private void HelpClicked()
    {
        Process.Start(new ProcessStartInfo(@"https://github.com/sixstepsaway/Sims-CC-Manager/") { UseShellExecute = true });
    }


    private void SettingsClicked()
    {
        SettingsWindow = SettingsPS.Instantiate() as MainSettings;
        AddChild(SettingsWindow);
    }

    public void CloseMainMenu()
    {
        GlobalVariables.mainWindow.SCCMThemeChanged -= ThemeUpdateEventHandler;
        BackgroundNode.UnhookHandlers();
        QueueFree();
    }


    private void LoadInstanceClicked()
    {
        loadInstance = LoadInstancePS.Instantiate() as LoadInstance;
        AddChild(loadInstance);
    }


    private void NewInstanceClicked()
    {
        newInstance = NewInstancePS.Instantiate() as NewInstance;
        AddChild(newInstance);

    }

    public override void _Process(double delta)
    {
        Tali.Visible = GlobalVariables.LoadedSettings.Tali;
        DevButton.Visible = GlobalVariables.LoadedSettings.DebugMode;
    }


}
