using Godot;
using MoreLinq;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.SettingsSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Xml.Serialization;

public partial class MainWindow : MarginContainer
{
    /// <summary>
    /// The main program window. 
    /// </summary>
    [Export]
    PackedScene SplashPS;
    [Export]
    PackedScene MainMenuPS;
    [Export]
    PackedScene PackageDisplayPS;
    [Export]
    PackedScene LoadingScreenPS;
    [Export]
    PackedScene TooltipPS;
    public delegate void ThemeChangedEvent();
    public ThemeChangedEvent SCCMThemeChanged;
    [Export]
    SocialsButton[] SocialsButtons;
    [Export]
    Label[] Labels;
    [Export]
    ColorRect[] MainColors;

    [Export]
    MarginContainer Footer;
    [Export]
    Label FooterVersionLabel;
    [Export]
    Label FooterStateLabel;

    [ExportCategory("Files Detected Window")]
    [Export]
    Window FDPopupWindow;
    [Export]
    Button FDMoreInfoButton;
    [Export]
    Button FDYesButton;
    
    [Export]
    Button FDNoButton;
    [Export]
    public Snapshotter snapshotter;



    bool WaitingPopupWindow = false;







    SplashScreen splash;
    MainMenu mainMenu;
    LoadingInstance loadingInstance;
    PackageDisplay packageDisplay;

    Stopwatch stopwatch = new();
    Godot.Timer tooltipTimer = new();

    public string TooltipMessage = "";

    ToolTip tooltip;

    bool tooltipVisible = false;

    public override void _Ready()
    {
        GlobalVariables.mainWindow = this;
        stopwatch.Start();
        Footer.Visible = false;
        splash = SplashPS.Instantiate() as SplashScreen;
        splash.SplashScreenLoadingFinished += () => FinishedLoading();
        AddChild(splash);
        RunProgressBar();
        AddChild(tooltipTimer);
        tooltipTimer.WaitTime = 0.5f;
        tooltipTimer.Timeout += () => SpawnTooltip();

        FDYesButton.Pressed += () => RemoveResidualFiles(true);
        FDNoButton.Pressed += () => RemoveResidualFiles(false);

        FooterVersionLabel.Text = GlobalVariables.CurrentVersion;
        FooterStateLabel.Text = GlobalVariables.State;        
    }

    private void RemoveResidualFiles(bool Delete)
    {
        FDPopupWindow.Visible = false;
        if (Delete)
        {
            InstanceControllers.ClearInstance();
        } else
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Not removing the residual files!"));
            string movedfile = string.Format("{0}__backup", GlobalVariables.MovedItemsFile);
            if (File.Exists(movedfile))
            {
                File.Delete(movedfile);
            }
            File.Move(GlobalVariables.MovedItemsFile, movedfile);
        }
        WaitingPopupWindow = false;
    }

    public void PopupRemoveFilesWindow()
    {
        FDPopupWindow.Visible = true;
    }


    public void AnnounceChangedTheme()
    {
        SCCMThemeChanged.Invoke();
        UpdateColors();
    }

    private void UpdateColors()
    {
        foreach (ColorRect colorRect in MainColors)
        {
            colorRect.Color = GlobalVariables.LoadedTheme.AccentColor;
        }
        foreach (Label l in Labels)
        {
            l.AddThemeColorOverride("font_color", GlobalVariables.LoadedTheme.MainTextColor);
        }
        foreach (SocialsButton socialsButton in SocialsButtons)
        {
            socialsButton.UpdateColors();
        }

        SCCMTheme theme = GlobalVariables.LoadedTheme;
        bool textLight = false;
        StyleBoxFlat normalbox = FDYesButton.GetThemeStylebox("normal") as StyleBoxFlat;
        StyleBoxFlat hoverbox = FDYesButton.GetThemeStylebox("hover") as StyleBoxFlat;
        StyleBoxFlat clickedbox = FDYesButton.GetThemeStylebox("pressed") as StyleBoxFlat;
        
        if (theme.ButtonMain.V > 0.5)
        {
            textLight = true;
        }

        normalbox.BorderColor = theme.AccentColor;

        if (theme.AccentColor.V > 0.5)
        {
            hoverbox.BorderColor = theme.AccentColor.Darkened(0.2f);
            clickedbox.BorderColor = theme.AccentColor.Darkened(0.2f);
        } else
        {
            hoverbox.BorderColor = theme.AccentColor.Lightened(0.2f);
            clickedbox.BorderColor = theme.AccentColor.Darkened(0.2f);
        }

        
        normalbox.BgColor = theme.BackgroundColor;
        hoverbox.BgColor = theme.BackgroundColor.Darkened(0.2f);
        clickedbox.BgColor = theme.BackgroundColor.Darkened(0.2f);

        List<Button> buttons = new() { FDYesButton, FDNoButton };

        foreach (Button button in buttons)
        {
            button.AddThemeColorOverride("font_color", theme.ButtonMain);
            button.AddThemeColorOverride("font_hover_color", theme.ButtonHover);
            button.AddThemeColorOverride("font_hover_pressed", theme.ButtonClick);
            button.AddThemeStyleboxOverride("normal", normalbox);
            button.AddThemeStyleboxOverride("hover", hoverbox);
            button.AddThemeStyleboxOverride("pressed", clickedbox);
        }

    }

    private void RunProgressBar()
    {
        int maxval = 100;
        splash.SetPbarMax(maxval);
        new Thread(() =>
        {
            CallDeferred(nameof(IncrementPbar), 1);
            CallDeferred(nameof(ChangePbarLabel), string.Format("Checking for environment"));
            Thread.Sleep(20);
            CheckLatestVersion();
            if (!Directory.Exists(GlobalVariables.AppFolder))
            {
                Directory.CreateDirectory(GlobalVariables.AppFolder);
            }
            if (!Directory.Exists(GlobalVariables.TempFolder))
            {
                Directory.CreateDirectory(GlobalVariables.TempFolder);
            }
            if (!Directory.Exists(GlobalVariables.LogFolder))
            {
                Directory.CreateDirectory(GlobalVariables.LogFolder);
            }
            if (!Directory.Exists(GlobalVariables.DataFolder))
            {
                Directory.CreateDirectory(GlobalVariables.DataFolder);
            }
            if (!Directory.Exists(GlobalVariables.OverridesFolder))
            {
                Directory.CreateDirectory(GlobalVariables.OverridesFolder);
            }
            if (!Directory.Exists(GlobalVariables.ThemesFolder))
            {
                CallDeferred(nameof(IncrementPbar), 1);
                CallDeferred(nameof(ChangePbarLabel), string.Format("Creating themes."));
                Thread.Sleep(20);
                Directory.CreateDirectory(GlobalVariables.ThemesFolder);
                Themes.CreateThemeFiles();
            }
            XmlSerializer SettingsSerializer = new XmlSerializer(typeof(SCCMSettings));
            if (File.Exists(GlobalVariables.SettingsFile))
            {
                CallDeferred(nameof(IncrementPbar), 1);
                CallDeferred(nameof(ChangePbarLabel), string.Format("Creating settings."));
                Thread.Sleep(20);
                using (FileStream fileStream = new(GlobalVariables.SettingsFile, FileMode.Open, System.IO.FileAccess.Read))
                {
                    using (StreamReader streamReader = new(fileStream))
                    {
                        GlobalVariables.LoadedSettings = (SCCMSettings)SettingsSerializer.Deserialize(streamReader);
                        streamReader.Close();
                    }
                    fileStream.Close();
                }
            }
            else
            {
                GlobalVariables.LoadedSettings = new();
                GlobalVariables.LoadedSettings.SaveSettings();                
            }
            GlobalVariables.DebugMode = GlobalVariables.LoadedSettings.DebugMode;            

            if (Directory.Exists(GlobalVariables.ThemesFolder))
            {
                XmlSerializer ThemeReader = new XmlSerializer(typeof(SCCMTheme));
                Themes.AllThemes = new();
                foreach (string file in Directory.GetFiles(GlobalVariables.ThemesFolder))
                {
                    FileInfo fileinfo = new(file);
                    if (fileinfo.Extension == ".xml"){
                        SCCMTheme newtheme = new();
                        using (FileStream fileStream = new(file, FileMode.Open, System.IO.FileAccess.Read))
                        {
                            using (StreamReader streamReader = new(fileStream))
                            {
                                newtheme = (SCCMTheme)ThemeReader.Deserialize(streamReader);
                                streamReader.Close();
                            }
                            fileStream.Close();
                        }
                        Themes.AllThemes.Add(newtheme);
                        GlobalVariables.LoadedSettings.ThemeOptions.Clear();
                        foreach (SCCMTheme theme in Themes.AllThemes)
                        {
                            GlobalVariables.LoadedSettings.ThemeOptions.Add(theme.ThemeName);
                        }
                    }                    
                }
                GlobalVariables.LoadedTheme = Themes.AllThemes.Where(x => x.ThemeName == GlobalVariables.LoadedSettings.LoadedTheme).First();
                GlobalVariables.LoadedSettings.SaveSettings();
            }
            
            CallDeferred(nameof(UpdateColors));



            if (File.Exists(GlobalVariables.MovedItemsFile))
            {
                WaitingPopupWindow = true;
                CallDeferred(nameof(PopupRemoveFilesWindow));
            }

            while (WaitingPopupWindow)
            {
                //wait
            }



            for (int i = (int)splash.SplashProgressBar.Value; i < maxval; i++)
            {
                CallDeferred(nameof(IncrementPbar), 1);
                CallDeferred(nameof(ChangePbarLabel), string.Format("Organizing splines..."));
                Thread.Sleep(10);
            }
        })
        { IsBackground = true }.Start();
    }

    private void IncrementPbar(int Num)
    {
        splash.IncrementProgressBar(Num);
    }
    private void ChangePbarLabel(string Text)
    {
        splash.ChangeProgressBarText(Text);
    }
    private void CloseMainMenu()
    {
        mainMenu.CloseMainMenu();
    }

    private void CheckLatestVersion(){
        string VersionURL = "https://raw.githubusercontent.com/sixstepsaway/Sims-CC-Manager/refs/heads/main/Version.txt";

        WebClient VersionClient = new WebClient();
        Stream stream = VersionClient.OpenRead(VersionURL);
        StreamReader reader = new StreamReader(stream);
        String content = reader.ReadToEnd();

        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Latest version: {0}", content));
        if (GlobalVariables.CurrentVersion != content) {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("New version available!"));
        } else
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Running latest version."));
        }
    }

    private void FinishedLoading()
    {        
        stopwatch.Stop();
        mainMenu = MainMenuPS.Instantiate() as MainMenu;
        splash.QueueFree();
        Footer.Visible = true;
        AddChild(mainMenu);
        MoveChild(Footer, GetChildCount());
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Sims CC Manager loaded in {0}", stopwatch.Elapsed));
        GetWindow().Borderless = false;
    } 

    public void LoadingPackageDisplayStart(int maxvalue = 100)
    {               
        loadingInstance = LoadingScreenPS.Instantiate() as LoadingInstance; 
        loadingInstance.progressBar.MaxValue = maxvalue;
        AddChild(loadingInstance);
        MoveChild(Footer, GetChildCount());
    }

    public void LoadPackageDisplay(GameInstance instanceData, bool load = false)
    {
        if (load)
        {
            GlobalVariables.LoadedSettings.InstanceFolders.First(x => x.InstanceID == instanceData.InstanceID).InstanceLastModified = DateTime.Now;
            GlobalVariables.LoadedSettings.SaveSettings();
        } else
        {
            GlobalVariables.LoadedSettings.InstanceFolders.Add(new Instance() { InstanceLocation = instanceData.InstanceFolder, InstanceCreated = DateTime.Now, InstanceLastModified = DateTime.Now, InstanceName = instanceData.InstanceName, InstanceID = instanceData.InstanceID, Game = instanceData.GameChoice});
            GlobalVariables.LoadedSettings.SaveSettings();
        }
        
        mainMenu.QueueFree();
        packageDisplay = PackageDisplayPS.Instantiate() as PackageDisplay;
        packageDisplay.ThisInstance = instanceData;
        AddChild(packageDisplay);
        MoveChild(Footer, GetChildCount());
        loadingInstance.QueueFree();
    }

    public void IncrementLoadingScreen(int amount, string text)
    {
        CallDeferred(nameof(DeferredLoadingScreen), amount, text);
    }

    private void DeferredLoadingScreen(int amount, string text)
    {
        loadingInstance.progressBar.Value += amount;
        loadingInstance.ProgressLabel.Text = text;
    }

    public void WriteGDPrint(string text)
    {
        CallDeferred(nameof(DeferredGDPrint), text);
    }

    private void DeferredGDPrint(string text)
    {
        GD.Print(text);
    }

    public void InstantiateTooltip(string text)
    {
        if (!tooltipVisible)
        {
            tooltipVisible = true;
            tooltipTimer.Start();
            TooltipMessage = text;
        }
        
    }
    private void SpawnTooltip()
    {
        if (IsInstanceValid(tooltip))
        {
            tooltip?.QueueFree();
        }
        tooltip = TooltipPS.Instantiate() as ToolTip;
        tooltip.TooltipText.Text = TooltipMessage;
        Vector2 mouse = GetGlobalMousePosition();
        tooltip.GlobalPosition = mouse;
        AddChild(tooltip);
        float tooltipwidth = tooltip.TooltipText.Size.X;
        Rect2 rect2 = GetGlobalRect();
        Rect2 rect2Right = rect2; 
        rect2Right.Size = new(rect2Right.Size.X - tooltipwidth, rect2Right.Size.Y);
        if (!rect2Right.HasPoint(mouse))
        {
            tooltip.GlobalPosition = new(mouse.X - tooltipwidth, mouse.Y);
        }
    }
    public void CancelTooltip()
    {
        tooltipVisible = false;
        if (IsInstanceValid(tooltipTimer))
        {
            tooltipTimer.Stop();
        }
        if (IsInstanceValid(tooltip))
        {
            tooltip?.QueueFree();
        }
    }

    public void ReturnToMain()
    {
        Instance data = GlobalVariables.LoadedSettings.InstanceFolders.Where(x => x.InstanceID == packageDisplay.ThisInstance.InstanceID).First();
        packageDisplay.ThisInstance.WriteXML();
        data.InstanceLastModified = DateTime.Now;
        GlobalVariables.LoadedSettings.SaveSettings();
        packageDisplay.QueueFree();
        mainMenu = MainMenuPS.Instantiate() as MainMenu;
        AddChild(mainMenu);
        MoveChild(Footer, GetChildCount());
    }
}
