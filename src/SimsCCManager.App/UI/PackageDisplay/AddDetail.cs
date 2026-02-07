using Godot;
using SimsCCManager.Globals;
using SimsCCManager.SettingsSystem;
using System;

public partial class AddDetail : MarginContainer
{
    public PackageDisplay packageDisplay;
    [Export]
    ColorRect BGColorRect;
    [Export]
    ColorRect BorderColorRect;
    [Export]
    HBoxContainer TextEditVersion;
    [Export]
    VBoxContainer LineEditVersion;
    [Export]
    public LineEdit LineEdit;
    [Export]
    public TextEdit TextEdit;
    [Export]
    public Button LESaveButton;
    [Export]
    public Button TESaveButton;
    [Export]
    TopbarButton CancelButton;

    private Color _backgroundcolor;
    public Color BackgroundColor
    {
        get { return _backgroundcolor; }
        set { _backgroundcolor = value; 
        BGColorRect.Color = value; }
    }
    private Color _accentcolor;
    public Color AccentColor
    {
        get { return _accentcolor; }
        set { _accentcolor = value; 
        BorderColorRect.Color = value; }
    }

    private string _placeholdertext;
    public string PlaceholderText
    {
        get { return _placeholdertext; }
        set { _placeholdertext = value; 
        LineEdit.PlaceholderText = value; 
        TextEdit.PlaceholderText = value; }
    }

    private bool _longtextversion;
    public bool LongTextVersion
    {
        get { return _longtextversion; }
        set { _longtextversion = value; 
            TextEditVersion.Visible = !value;
            LineEditVersion.Visible = value; }
    }

    public override void _Ready()
    {
        UpdateTheme();
        LESaveButton.Pressed += () => TextSaved();
        TESaveButton.Pressed += () => TextSaved();
        LineEdit.TextSubmitted += (t) => TextSaved();
        CancelButton.ButtonClicked += () => Cancel();
    }

    private void Cancel()
    {
        GetParent().RemoveChild(this);
        QueueFree();
    }


    public delegate void TextSubmittedEvent(string text);
    public TextSubmittedEvent TextSubmitted;

    private void TextSaved()
    {
        packageDisplay.LockInput = false;
        if (LongTextVersion)
        {
            TextSubmitted?.Invoke(TextEdit.Text);
        } else
        {
            TextSubmitted?.Invoke(LineEdit.Text);
        }
        GetParent().RemoveChild(this);
        QueueFree();
    }

    public void UpdateTheme()
    {
        CancelButton.UpdateColors();
        SCCMTheme theme = GlobalVariables.LoadedTheme;
        bool textLight = false;
        StyleBoxFlat normalbox = LESaveButton.GetThemeStylebox("normal") as StyleBoxFlat;
        StyleBoxFlat hoverbox = LESaveButton.GetThemeStylebox("hover") as StyleBoxFlat;
        StyleBoxFlat clickedbox = LESaveButton.GetThemeStylebox("pressed") as StyleBoxFlat;
        
        
        LESaveButton.AddThemeColorOverride("font_color", theme.ButtonMain);
        LESaveButton.AddThemeColorOverride("font_hover_color", theme.ButtonHover);
        LESaveButton.AddThemeColorOverride("font_hover_pressed", theme.ButtonClick);
        LESaveButton.AddThemeStyleboxOverride("normal", normalbox);
        LESaveButton.AddThemeStyleboxOverride("hover", hoverbox);
        LESaveButton.AddThemeStyleboxOverride("pressed", clickedbox);
        
        TESaveButton.AddThemeColorOverride("font_color", theme.ButtonMain);
        TESaveButton.AddThemeColorOverride("font_hover_color", theme.ButtonHover);
        TESaveButton.AddThemeColorOverride("font_hover_pressed", theme.ButtonClick);
        TESaveButton.AddThemeStyleboxOverride("normal", normalbox);
        TESaveButton.AddThemeStyleboxOverride("hover", hoverbox);
        TESaveButton.AddThemeStyleboxOverride("pressed", clickedbox);        
        
    }

}
