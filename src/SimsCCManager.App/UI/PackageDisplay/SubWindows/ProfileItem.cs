using Godot;
using System;

public partial class ProfileItem : MarginContainer
{
    [Export]
    Panel NormalPanel;
    [Export]
    Panel SelectedPanel;
    [Export]
    public Label ProfileName;
    [Export]
    Label ActiveLabel;
    [Export]
    Label InactiveLabel;
    [Export]
    public Button ProfileButton;

    

    private bool _isselected;

    public bool IsSelected { get { return _isselected; } set { _isselected = value; 
    NormalPanel.Visible = !value; 
    SelectedPanel.Visible = value;}}
    private bool _isactive;
    public bool IsActive { get { return _isactive; } set { _isactive = value; 
    InactiveLabel.Visible = !value; 
    ActiveLabel.Visible = value;}}
}
