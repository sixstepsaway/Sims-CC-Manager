using Godot;
using SimsCCManager.Containers;
using System;

public partial class ExeOption : MarginContainer
{
    public Executable ThixExe;
    [Export]
    public TextureRect ExeIcon;
    [Export]
    public Label ExeName;
    [Export]
    public Label ExeExe;
    [Export]
    public Label ExeLocation;
    [Export]
    public Button button;

    [Export]
    Panel Normalpanel;
    [Export]
    Panel SelectedPanel;

    private bool _isselected;
    public bool IsSelected {
        get { return _isselected; } 
        set { _isselected = value; 
            SelectedPanel.Visible = value; 
            Normalpanel.Visible = !value;
            }
        }

}
