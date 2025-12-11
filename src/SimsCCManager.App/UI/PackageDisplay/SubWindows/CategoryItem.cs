using Godot;
using System;

public partial class CategoryItem : MarginContainer
{
    [Export]
    Panel Normal;
    [Export]
    Panel Highlight;
    [Export]
    public Label CategoryName;
    [Export]
    public Label PackageCount;
    [Export]
    public ColorRect CategoryColor;
    [Export]
    public Button button;

    private bool _isselected;
    public bool IsSelected { 
        get { return _isselected; } 
        set { 
            _isselected = value; 
            Normal.Visible = !value;
            Highlight.Visible = value;
            }
        }
}
