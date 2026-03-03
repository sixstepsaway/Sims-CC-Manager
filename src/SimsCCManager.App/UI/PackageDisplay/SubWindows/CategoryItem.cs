using Godot;
using SimsCCManager.Globals;
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
    public Label CategoryTextColor;
    [Export]
    public Button button;
    [Export]
    public Button enablebutton;
    [Export]
    public Button disablebutton;
    [Export]
    CustomCheckButton CheckButton;
    [Export]
    public Control NameContainer;
    [Export]
    public Control CountContainer;
    [Export]
    public Control ColorContainer;
    [Export]
    public Control HideContainer;
    [Export]
    public Control EDContainer;
    [Export]
    MarginContainer EnableContainer;
    [Export]
    MarginContainer DisableContainer;
    [Export]
    ColorRect EnableButtonColor;
    [Export]
    ColorRect DisableButtonColor;
    public Color ButtonColor; 
    public Color ButtonHoverColor;

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
    public bool IsCursorInEnabled()
    {
        Rect2 rect = new(EnableContainer.GlobalPosition, EnableContainer.Size);
        Vector2 cursor = GetGlobalMousePosition();
        if (rect.HasPoint(cursor))
        {
            return true;
        } else
        {
            return false;
        }
    }
    public bool IsCursorInDisabled()
    {
        Rect2 rect = new(DisableContainer.GlobalPosition, DisableContainer.Size);        
        Vector2 cursor = GetGlobalMousePosition();
        if (rect.HasPoint(cursor))
        {
            return true;
        } else
        {
            return false;
        }
    }

    public override void _Ready()
    {
        enableHovered = false;
        disableHovered = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motion)
        {
            if (IsCursorInDisabled())
            {
                disableHovered = true;
                enableHovered = false;
            } else if (IsCursorInEnabled())
            {
                enableHovered = true;
                disableHovered = false;
            } else
            {
                enableHovered = false;
                disableHovered = false;
            }
        }
    }


    bool _enablehovered;
    bool enableHovered
    {
        get {return _enablehovered; }
        set { _enablehovered = value; 
        switch (value)
            {
                case true:
                    EnableButtonColor.Color = ButtonHoverColor;
                break;
                case false:
                    EnableButtonColor.Color = ButtonColor;
                break;
            }
        }
    }
    bool _disablehovered;
    bool disableHovered
    {
        get {return _disablehovered; }
        set { _disablehovered = value; 
        switch (value)
            {
                case true:
                    DisableButtonColor.Color = ButtonHoverColor;
                break;
                case false:
                    DisableButtonColor.Color = ButtonColor;
                break;
            }
        }
    }
    private Color _textcolor;

    public Color TextColor
    {
        get { return _textcolor; }
        set { _textcolor = value; 
        CategoryName.AddThemeColorOverride("font_color", value);
        PackageCount.AddThemeColorOverride("font_color", value); }
    }
}
