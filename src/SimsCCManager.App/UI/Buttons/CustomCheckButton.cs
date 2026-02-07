using Godot;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.SettingsSystem;
using System;
using System.Linq;

public partial class CustomCheckButton : MarginContainer
{
   
    [Export]
    ColorRect ToggledBlipColor;
    [Export]
    ColorRect UntoggledBlipColor;
    [Export]
    MarginContainer ToggledMask;
    [Export]
    MarginContainer UntoggledMask;
    [Export]
    Button ButtonClicker;

    public delegate void CheckToggledEvent(bool Toggled);
    public CheckToggledEvent CheckToggled;
    private bool _istoggled;
    [Export]
    public bool IsToggled { 
        get { return _istoggled; }
        set { 
                _istoggled = value; 
                ToggledMask.Visible = value;
                UntoggledMask.Visible = !value;
            }
    }

    public override void _Ready()
    {
        ButtonClicker.Pressed += () => ButtonClicked();
        UpdateTheme();
    }

    private void ButtonClicked()
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checkbutton clicked!"));
        IsToggled = !IsToggled;
        CheckToggled?.Invoke(IsToggled);
    }

    public void UpdateTheme()
    {
        ToggledBlipColor.Color = GlobalVariables.LoadedTheme.AccentColor;
        UntoggledBlipColor.Color = GlobalVariables.LoadedTheme.DataGridA;
    }
}
