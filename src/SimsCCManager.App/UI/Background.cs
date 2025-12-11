using Godot;
using SimsCCManager.Globals;
using System;

public partial class Background : Control
{
    [Export]
    ColorRect MainColor;
    [Export]
    ColorRect SecondaryColor;
    private MainWindow.ThemeChangedEvent ThemeUpdateEventHandler;

    public override void _Ready()
    {
        TreeExiting += () => UnhookHandlers();
        ThemeUpdateEventHandler = UpdateTheme;
        GlobalVariables.mainWindow.SCCMThemeChanged += ThemeUpdateEventHandler;
        UpdateTheme();
    }

    public void UpdateTheme()
    {
        MainColor.Color = GlobalVariables.LoadedTheme.BackgroundColor;
        SecondaryColor.Color = GlobalVariables.LoadedTheme.AccentColor;
    }

    public void UnhookHandlers()
    {
        GlobalVariables.mainWindow.SCCMThemeChanged -= ThemeUpdateEventHandler;
    }


}
