using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.PackageReaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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


    private void DevClicked()
    {
        //List<string> files = Directory.EnumerateFiles(@"O:\Godot Projects\SimsCCManager\Test", "*.package", SearchOption.AllDirectories).ToList();
       // for (int i = 0; i < files.Count; i++)
        //{
            SimsPackageReader sims2PackageReader = new();            
            //sims2PackageReader.ReadPackage(files[i]);           
            sims2PackageReader.ReadPackage(@"O:\Godot Projects\SimsCCManager\Test\MLC-KFCBucketToilet.package");
            GlobalVariables.mainWindow.snapshotter.BuildMesh((sims2PackageReader.SimsData as Sims2Data).GMDCDataBlock);
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}", sims2PackageReader.ToString()));
        //}
        
        
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
