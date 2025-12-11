using Godot;
using SimsCCManager.Globals;
using System;
using System.Diagnostics;

public partial class SocialsButton : MarginContainer
{
    /// <summary>
    /// Footer buttons.
    /// </summary>

    [Export]
    public TextureRect ButtonTexture;
    [Export]
    public ColorRect NormalColor;
    [Export]
    public ColorRect HoverColor;
    [Export]
    public ColorRect ClickColor;
    [Export]
    public string LinkString = "";
    [Export]
    Button ButtonClicker;

    bool MouseHover = false;


    public override void _Ready()
    {
        ButtonClicker.Pressed += () => ButtonPressed();
        ButtonClicker.MouseEntered += () => ButtonHover(true);
        ButtonClicker.MouseExited += () => ButtonHover(false);
    }

    private void ButtonHover(bool hovering)
    {
        if (hovering)
        {
            NormalColor.Visible = false;
            ClickColor.Visible = false;
            HoverColor.Visible = true;
        }
        else
        {
            NormalColor.Visible = true;
            ClickColor.Visible = false;
            HoverColor.Visible = false;
        }
    }


    private void ButtonPressed()
    {
        NormalColor.Visible = false;
        ClickColor.Visible = true;
        HoverColor.Visible = false;
        Process.Start(new ProcessStartInfo(LinkString) { UseShellExecute = true });
    }

    public void UpdateColors()
    {        
        NormalColor.Color = GlobalVariables.LoadedTheme.ButtonMain;
        HoverColor.Color = GlobalVariables.LoadedTheme.ButtonHover;
        ClickColor.Color = GlobalVariables.LoadedTheme.ButtonClick;
    }
    
}
