using Godot;
using SimsCCManager.Globals;
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

    private Color _textcolor;

    public Color TextColor
    {
        get { return _textcolor; }
        set { _textcolor = value; 
        ProfileName.AddThemeColorOverride("font_color", value); }
    }

    

    private bool _isselected;

    public bool IsSelected { get { return _isselected; } set { _isselected = value; 
    NormalPanel.Visible = !value; 
    SelectedPanel.Visible = value;}}
    private bool _isactive;
    public bool IsActive { get { return _isactive; } set { _isactive = value; 
    InactiveLabel.Visible = !value; 
    ActiveLabel.Visible = value;}}
}
