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
    [Export]
    CustomCheckButton CheckButton;

    public Guid Identifier;

    private bool _ischecked;
    public bool IsChecked
    {
        set { _ischecked = value; 
        CheckButton.IsToggled = value;
        DontShowInGrid?.Invoke(value); }
        get { return _ischecked; }
    }

    public delegate void DontShowInGridEvent(bool Checked);
    public DontShowInGridEvent DontShowInGrid;

    public void FlipCheck()
    {
        IsChecked = !IsChecked;
    }

    private bool _isselected;
    public bool IsSelected { 
        get { return _isselected; } 
        set { 
            _isselected = value; 
            Normal.Visible = !value;
            Highlight.Visible = value;
            }
    }

    public bool IsCursorInCheck()
    {
        Rect2 rect = new((CheckButton as MarginContainer).GlobalPosition, (CheckButton as MarginContainer).Size);
        Vector2 cursor = GetGlobalMousePosition();
        if (rect.HasPoint(cursor))
        {
            return true;
        } else
        {
            return false;
        }
    }

}
