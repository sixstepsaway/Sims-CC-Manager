using Godot;
using SimsCCManager.Globals;
using System;

public partial class SettingsAndHelpControls : MarginContainer
{
    [Export]
    TopbarButton Settings;
    [Export]
    TopbarButton BackToMainMenu;
    [Export]
    TopbarButton ViewErrors;
    [Export]
    TopbarButton Help;
    [Export]
    MarginContainer ErrorsDetectedCircle;
    public int ErrorCount = 0;

    private bool _errorsdetected;
    public bool ErrorsDetected
    {
        get {return _errorsdetected;}
        set {_errorsdetected = value;
        ErrorsDetectedCircle.Visible = value;
        if (value) ViewErrors.TooltipText = string.Format("{0} errors detected", ErrorCount); else ViewErrors.TooltipText = "No errors found."; }
    }


    public override void _Ready()
    {
        Settings.ButtonClicked += () => ButtonPressed(0);
        BackToMainMenu.ButtonClicked += () => ButtonPressed(1);
        ViewErrors.ButtonClicked += () => ButtonPressed(2);
        Help.ButtonClicked += () => ButtonPressed(3);
        ErrorsDetected = false;
    }

    private void ButtonPressed(int buttonID)
    {
        switch (buttonID)
        {
            case 0: 
                //open settings menu.
            break;
            case 1: 
                ReturnToMainMenu();
            break;
            case 2: 
                //open errors screen
            break;
            case 3: 
                //open help screen
            break;
        }
    }

    private void ReturnToMainMenu()
    {
        GlobalVariables.mainWindow.ReturnToMain();
    }
}
