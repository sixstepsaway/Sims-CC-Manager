using Godot;
using SimsCCManager.Globals;
using System;

public partial class RefreshingWait : Control
{
    [Export]
    ColorRect RotatorColor;
    [Export]
    Node2D Rotator;

    public override void _Ready()
    {
        RotatorColor.Color = GlobalVariables.LoadedTheme.AccentColor;        
    }

    public override void _Process(double delta)
    {
        Rotator.RotationDegrees += -5f;
    }


}
