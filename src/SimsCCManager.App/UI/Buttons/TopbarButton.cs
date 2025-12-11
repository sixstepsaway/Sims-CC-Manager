using Godot;
using SimsCCManager.Globals;
using System;

public partial class TopbarButton : MarginContainer
{
    [Export]
    public Texture2D Texture;
    [Export]
    TextureRect ButtonTexture;
    [Export]
    ColorRect ButtonColorMain;
    [Export]
    ColorRect ButtonColorHover;
    [Export]
    ColorRect ButtonColorClick;
    [Export]
    Button ButtonClicker;
    [Export]
    public string ToolTipText;

    public delegate void ButtonClickedEvent();
    public ButtonClickedEvent ButtonClicked;

    public override void _Ready()
    {
        ButtonClicker.Pressed += () => ButtonPressed();
        ButtonClicker.MouseEntered += () => HoverButton(true);
        ButtonClicker.MouseExited += () => HoverButton(false);
        ButtonTexture.Texture = Texture;
        UpdateColors();
    }

    private void HoverButton(bool v)
    {
        ButtonColorHover.Visible = v;
        if (v && !string.IsNullOrEmpty(ToolTipText)) { 
            GlobalVariables.mainWindow.InstantiateTooltip(ToolTipText);
        } else if (!v && !string.IsNullOrEmpty(ToolTipText)) GlobalVariables.mainWindow.CancelTooltip();
    }


    private void ButtonPressed()
    {
        ButtonClicked.Invoke();        
    }

    public void UpdateColors()
    {
        ButtonColorMain.Color = GlobalVariables.LoadedTheme.ButtonMain;
        ButtonColorHover.Color = GlobalVariables.LoadedTheme.ButtonHover;
        ButtonColorClick.Color = GlobalVariables.LoadedTheme.ButtonClick;
    }

}
