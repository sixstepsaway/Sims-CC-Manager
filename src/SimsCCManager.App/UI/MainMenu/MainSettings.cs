using Godot;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.SettingsSystem;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Serialization;

public partial class MainSettings : MarginContainer
{
    [Export]
    Background BackgroundNode;
    [Export]
    Label[] Labels;
    [Export]
    OptionButton ThemeOptions;
    [Export]
    CustomCheckButton DebugModeCheck;
    [Export]
    CustomCheckButton PortableModeCheck;
    [Export]
    CustomCheckButton RestrictCPUCheck;
    [Export]
    CustomCheckButton LoadLatestCheck;
    [Export]
    CustomCheckButton ShowTaliCheck;
    [Export]
    Button CloseSettingsButton;
    [Export]
    TopbarButton AddTheme;
    [Export]
    TopbarButton DeleteTheme;
    [Export]
    PackedScene NewThemePS;
    private MainWindow.ThemeChangedEvent ThemeUpdateEventHandler;


    public override void _Ready()
    {
        ThemeUpdateEventHandler = UpdateTheme;
        GlobalVariables.mainWindow.SCCMThemeChanged += ThemeUpdateEventHandler;
        CloseSettingsButton.Pressed += () => SettingsClosed();
        DebugModeCheck.CheckToggled += (d) => DebugModeToggled(d);
        RestrictCPUCheck.CheckToggled += (c) => CPURestrictToggled(c);
        LoadLatestCheck.CheckToggled += (l) => LoadLatestToggled(l);
        ShowTaliCheck.CheckToggled += (t) => TaliToggled(t);
        PortableModeCheck.CheckToggled += (p) => PortableChecked(p);
        ThemeOptions.AddItem(GlobalVariables.LoadedSettings.LoadedTheme);
        foreach (string th in GlobalVariables.LoadedSettings.ThemeOptions)
        {
            if (th != GlobalVariables.LoadedSettings.LoadedTheme)
            {
                ThemeOptions.AddItem(th);
            }
        }
        ThemeOptions.Select(0);
        ThemeOptions.ItemSelected += (item) => ThemeSelected(item);

        AddTheme.ButtonClicked += () => AddNewTheme();
        DeleteTheme.ButtonClicked += () => DeleteCurrentTheme();


        UpdateTheme();
    }

    private void PortableChecked(bool p)
    {
        //GlobalVariables.PortableMode = p;
        GlobalVariables.LoadedSettings.PortableMode = p;
        GlobalVariables.LoadedSettings.SaveSettings();
    }


    private void DeleteCurrentTheme()
    {
        ThemeOptions.ButtonPressed = false;
        int id = ThemeOptions.GetSelectedId();
        string theme = ThemeOptions.GetItemText(id);
        SCCMTheme themetoremove = Themes.AllThemes.Where(x => x.ThemeName == theme).First();
        Themes.AllThemes.Remove(themetoremove);
        GlobalVariables.LoadedSettings.ThemeOptions.Remove(theme);
        GlobalVariables.LoadedSettings.SaveSettings();
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Deleting theme {0}, which is one of {1} themes", id, ThemeOptions.ItemCount));
        if (id != ThemeOptions.ItemCount-1)
        {
            id++;
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Deleting theme, so swapping to theme after: {0}", id));
            ThemeSelected(id);
        } else
        {
            id--;
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Deleting theme, so swapping to theme before: {0}", id));
            ThemeSelected(id);
        }
        NewThemeAdded();
        string file = Path.Combine(GlobalVariables.ThemesFolder, string.Format("{0}.xml", theme));
        File.Delete(file);
    }


    private void AddNewTheme()
    {
        NewTheme newTheme = NewThemePS.Instantiate() as NewTheme;
        SCCMTheme theme = GlobalVariables.LoadedTheme;
        newTheme.ThemeName.Text = theme.ThemeName;
        newTheme.BackgroundColor_Color = theme.BackgroundColor;
        newTheme.ButtonMain_Color = theme.ButtonMain;
        newTheme.ButtonHover_Color = theme.ButtonHover;
        newTheme.ButtonClick_Color = theme.ButtonClick;
        newTheme.DataGridA_Color = theme.DataGridA;
        newTheme.DataGridTextA_Color = theme.DataGridTextA;
        newTheme.DataGridB_Color = theme.DataGridB;
        newTheme.DataGridTextB_Color = theme.DataGridTextB;
        newTheme.DataGridSelected_Color = theme.DataGridSelected;
        newTheme.AccentColor_Color = theme.AccentColor;
        newTheme.MainTextColor_Color = theme.MainTextColor;
        newTheme.HeaderTextColor_Color = theme.HeaderTextColor;
        newTheme.AddedTheme += () => NewThemeAdded();
        AddChild(newTheme);
    }

    private void NewThemeAdded()
    {
        ThemeOptions.Clear();
        ThemeOptions.AddItem(GlobalVariables.LoadedSettings.LoadedTheme);
        foreach (string th in GlobalVariables.LoadedSettings.ThemeOptions)
        {
            if (th != GlobalVariables.LoadedSettings.LoadedTheme)
            {
                ThemeOptions.AddItem(th);
            }
        }
        ThemeOptions.Select(0);
        ReloadThemes();
    }

    private void UpdateTheme()
    {
        DebugModeCheck.UpdateTheme();
        PortableModeCheck.UpdateTheme();
        RestrictCPUCheck.UpdateTheme();
        LoadLatestCheck.UpdateTheme();
        ShowTaliCheck.UpdateTheme();
        AddTheme.UpdateColors();
        DeleteTheme.UpdateColors();
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        SCCMTheme theme = GlobalVariables.LoadedTheme;
        bool textLight = false;
        StyleBoxFlat normalbox = CloseSettingsButton.GetThemeStylebox("normal") as StyleBoxFlat;
        StyleBoxFlat hoverbox = CloseSettingsButton.GetThemeStylebox("hover") as StyleBoxFlat;
        StyleBoxFlat clickedbox = CloseSettingsButton.GetThemeStylebox("pressed") as StyleBoxFlat;
        
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

        CloseSettingsButton.AddThemeColorOverride("font_color", theme.ButtonMain);
        CloseSettingsButton.AddThemeColorOverride("font_hover_color", theme.ButtonHover);
        CloseSettingsButton.AddThemeColorOverride("font_hover_pressed", theme.ButtonClick);
        CloseSettingsButton.AddThemeStyleboxOverride("normal", normalbox);
        CloseSettingsButton.AddThemeStyleboxOverride("hover", hoverbox);
        CloseSettingsButton.AddThemeStyleboxOverride("pressed", clickedbox);        
    }

    private void ThemeSelected(long item)
    {
        string theme = ThemeOptions.GetItemText((int)item);
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Switching to theme {0}: {1}", item, theme));
        GlobalVariables.LoadedTheme = Themes.AllThemes.Where(x => x.ThemeName == theme).First();
        GlobalVariables.LoadedSettings.LoadedTheme = GlobalVariables.LoadedTheme.ThemeName;
        GlobalVariables.LoadedSettings.SaveSettings();
        GlobalVariables.mainWindow.AnnounceChangedTheme();
    }

    private void ReloadThemes()
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


    private void TaliToggled(bool t)
    {
        GlobalVariables.LoadedSettings.Tali = t;
        GlobalVariables.LoadedSettings.SaveSettings();
    }


    private void LoadLatestToggled(bool l)
    {
        GlobalVariables.LoadedSettings.AutoLoad = l;
        GlobalVariables.LoadedSettings.SaveSettings();
    }


    private void CPURestrictToggled(bool c)
    {
        GlobalVariables.LoadedSettings.CPURestrict = c;
        GlobalVariables.LoadedSettings.SaveSettings();
    }


    private void DebugModeToggled(bool d)
    {
        //GlobalVariables.DebugMode = d;
        GlobalVariables.LoadedSettings.DebugMode = d;
        GlobalVariables.LoadedSettings.SaveSettings();
    }


    private void SettingsClosed()
    {
        GlobalVariables.mainWindow.SCCMThemeChanged -= ThemeUpdateEventHandler;
        QueueFree();
    }

    public override void _Process(double delta)
    {
        DebugModeCheck.IsToggled = GlobalVariables.LoadedSettings.DebugMode;
        ShowTaliCheck.IsToggled = GlobalVariables.LoadedSettings.Tali;
        RestrictCPUCheck.IsToggled = GlobalVariables.LoadedSettings.CPURestrict;
        LoadLatestCheck.IsToggled = GlobalVariables.LoadedSettings.AutoLoad;
        PortableModeCheck.IsToggled = GlobalVariables.LoadedSettings.PortableMode;
    }

}
