using Godot;
using SimsCCManager.Globals;
using SimsCCManager.SettingsSystem;
using System;
using System.Collections.Generic;

public partial class RenameItemsBox : MarginContainer
{
    [Export]
    public VBoxContainer ItemContainer;
    [Export]
    Label[] labels;
    [Export]
    Button[] buttons;
    [Export]
    Button GetInternals;
    [Export]
    Node2D ButtonSpin;
    [Export]
    public Button ConfirmButton;
    [Export]
    public Button CancelButton;
    public List<PackageListItem> Items = new();

    public override void _Ready()
    {
        GetInternals.Pressed += () => GetInternalsPressed();
    }

    private void GetInternalsPressed()
    {
        Spin();
        foreach (PackageListItem pli in Items)
        {
            pli.GetInternalName();
        }
    }

    public void Spin()
    {
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(ButtonSpin, "rotation_degrees", 360f, 0.5f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        tween.Play();
    }

    public void UpdateTheme()
    {
        SCCMTheme theme = GlobalVariables.LoadedTheme;
        bool textLight = false;
        StyleBoxFlat normalbox = buttons[0].GetThemeStylebox("normal") as StyleBoxFlat;
        StyleBoxFlat hoverbox = buttons[0].GetThemeStylebox("hover") as StyleBoxFlat;
        StyleBoxFlat clickedbox = buttons[0].GetThemeStylebox("pressed") as StyleBoxFlat;
        
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
        

        foreach (Button button in buttons)
        {
            button.AddThemeColorOverride("font_color", theme.ButtonMain);
            button.AddThemeColorOverride("font_hover_color", theme.ButtonHover);
            button.AddThemeColorOverride("font_hover_pressed", theme.ButtonClick);
            button.AddThemeStyleboxOverride("normal", normalbox);
            button.AddThemeStyleboxOverride("hover", hoverbox);
            button.AddThemeStyleboxOverride("pressed", clickedbox);
        }
        foreach (Label label in labels)
        {
            label.AddThemeColorOverride("font_color", theme.HeaderTextColor);
        }
    }
}
