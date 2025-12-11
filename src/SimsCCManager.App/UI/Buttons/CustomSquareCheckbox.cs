using Godot;
using SimsCCManager.Globals;
using System;

public partial class CustomSquareCheckbox : MarginContainer
{
    [Export]
    ColorRect BackgroundColor;
    [Export]
    ColorRect SelectedColor;
    [Export]
    ColorRect Box;
    [Export]
    ColorRect Selected2Color;
    [Export]
    MarginContainer CheckedBox;
    [Export]
    MarginContainer HoveredBox;
    [Export]
    Button button;

    private bool _ischecked;

    public bool IsChecked
    {
        get {return _ischecked;}
        set { _ischecked = value; 
            CheckedBox.Visible = value;
        }

    }



    public override void _Ready()
    {
        UpdateTheme();
        button.MouseEntered += () => Hovering(true);
        button.MouseExited += () => Hovering(false);
    }

    private void Hovering(bool v)
    {
        HoveredBox.Visible = v;
    }


    private void UpdateTheme()
    {
        BackgroundColor.Color = GlobalVariables.LoadedTheme.ButtonMain;
        Box.Color = GlobalVariables.LoadedTheme.ButtonHover;
        SelectedColor.Color = GlobalVariables.LoadedTheme.ButtonClick;
    }

}
