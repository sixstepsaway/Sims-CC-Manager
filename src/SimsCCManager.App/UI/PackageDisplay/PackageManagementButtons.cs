using Godot;
using System;
using System.Linq;

public partial class PackageManagementButtons : MarginContainer
{
    [Export]
    TopbarButton AddFiles;
    [Export]
    TopbarButton AddFolder;
    [Export]
    TopbarButton Refresh;
    [Export]
    TopbarButton SortSubfolders;
    [Export]
    TopbarButton ExportProfile;
    [Export]
    TopbarButton ManageCategories;
    [Export]
    TopbarButton EditExes;

    public delegate void TopBarButtonEvent(int button);
    public TopBarButtonEvent TopBarButtonPressed;

    [Export]
    TopbarButton[] buttons;
 

    public override void _Ready()
    {
        AddFiles.ButtonClicked += () => ButtonPressed(0);
        AddFolder.ButtonClicked += () => ButtonPressed(1);
        Refresh.ButtonClicked += () => ButtonPressed(2);
        SortSubfolders.ButtonClicked += () => ButtonPressed(3);
        ExportProfile.ButtonClicked += () => ButtonPressed(4);
        ManageCategories.ButtonClicked += () => ButtonPressed(5);
        EditExes.ButtonClicked += () => ButtonPressed(6);
    }

    private void ButtonPressed(int i)
    {
        TopBarButtonPressed.Invoke(i);
    }
}
