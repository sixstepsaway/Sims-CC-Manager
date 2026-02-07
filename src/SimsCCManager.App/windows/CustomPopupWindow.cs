using Godot;
using SimsCCManager.Globals;
using SimsCCManager.SettingsSystem;
using System;

public partial class CustomPopupWindow : Window
{
    [Export]
    ColorRect BGColor;
    [Export]
    public RichTextLabel WindowMessage;
    [Export]
    public Button YesButton; 
    [Export]
    public Button NoButton; 
    [Export]
    Button BrowseButton; 
    [Export]
    public FileDialog BrowseDialog; 
    [Export]
    HBoxContainer BrowseOption;
    [Export]
    public LineEdit BrowseLineEdit;
    private bool _usebrowseoption;
    public bool UseBrowseOption
    {
        get { return _usebrowseoption; }
        set { _usebrowseoption = value; 
        BrowseOption.Visible = value;
        }
    }
    private string _windowtitle;
    public string WindowTitle
    {
        get { return _windowtitle; }
        set { _windowtitle = value; 
        this.Title = value;}
    }

    private string _placeholdertext;
    public string PlaceholderText
    {
        get { return _placeholdertext; }
        set { _placeholdertext = value; 
        BrowseLineEdit.PlaceholderText = value;}
    }

    public void UpdateTheme()
    {
        SCCMTheme theme = GlobalVariables.LoadedTheme;
        bool textLight = false;
        StyleBoxFlat normalbox = YesButton.GetThemeStylebox("normal") as StyleBoxFlat;
        StyleBoxFlat hoverbox = YesButton.GetThemeStylebox("hover") as StyleBoxFlat;
        StyleBoxFlat clickedbox = YesButton.GetThemeStylebox("pressed") as StyleBoxFlat;
        
        
        YesButton.AddThemeColorOverride("font_color", theme.ButtonMain);
        YesButton.AddThemeColorOverride("font_hover_color", theme.ButtonHover);
        YesButton.AddThemeColorOverride("font_hover_pressed", theme.ButtonClick);
        YesButton.AddThemeStyleboxOverride("normal", normalbox);
        YesButton.AddThemeStyleboxOverride("hover", hoverbox);
        YesButton.AddThemeStyleboxOverride("pressed", clickedbox);

        NoButton.AddThemeColorOverride("font_color", theme.ButtonMain);
        NoButton.AddThemeColorOverride("font_hover_color", theme.ButtonHover);
        NoButton.AddThemeColorOverride("font_hover_pressed", theme.ButtonClick);
        NoButton.AddThemeStyleboxOverride("normal", normalbox);
        NoButton.AddThemeStyleboxOverride("hover", hoverbox);
        NoButton.AddThemeStyleboxOverride("pressed", clickedbox);
        
        BrowseButton.AddThemeColorOverride("font_color", theme.ButtonMain);
        BrowseButton.AddThemeColorOverride("font_hover_color", theme.ButtonHover);
        BrowseButton.AddThemeColorOverride("font_hover_pressed", theme.ButtonClick);
        BrowseButton.AddThemeStyleboxOverride("normal", normalbox);
        BrowseButton.AddThemeStyleboxOverride("hover", hoverbox);
        BrowseButton.AddThemeStyleboxOverride("pressed", clickedbox);
        
    }

    public override void _Ready()
    {
        UseBrowseOption = false;
        BrowseButton.Pressed += () => BrowseFolder();
        BrowseDialog.DirSelected += (f) => SelectedFolder(f);
        
    }

    private void SelectedFolder(string f)
    {
        BrowseLineEdit.Text = f;
    }


    private void BrowseFolder()
    {
        BrowseDialog.Visible = true;
    }

    public void NoClosesWindow()
    {
        NoButton.Pressed += () => CloseWindow();
    }

    private void CloseWindow()
    {
        GetParent().RemoveChild(this);
        QueueFree();
    }
}
