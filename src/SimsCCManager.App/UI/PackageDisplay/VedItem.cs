using Godot;
using SimsCCManager.Containers;
using System;
using System.IO;

public partial class VedItem : MarginContainer
{
    public PackageDisplay packageDisplay;
    private SimsPackage _package; 
    public SimsPackage Package
    {
        get { return _package; }
        set { _package = value; 
        SetPackageInfo(); }
    }

    private void SetPackageInfo()
    {        
        FileInfo f = new(Package.Location);
        
        DisplayLabel.Text = f.Name;
    }


    [Export]
    Panel SelectedPanel;
    [Export]
    Panel NormalPanel;
    [Export]
    Label DisplayLabel;
    [Export]
    Button button;

    public int type = -1;

    public override void _Ready()
    {
        button.Pressed += () => { IsSelected = !IsSelected; };
    }

    private bool _isselected;
    public bool IsSelected
    {
        get { return _isselected; }
        set { _isselected = value;         
            SelectedPanel.Visible = value;
            NormalPanel.Visible = !value;
        }
    }

}
