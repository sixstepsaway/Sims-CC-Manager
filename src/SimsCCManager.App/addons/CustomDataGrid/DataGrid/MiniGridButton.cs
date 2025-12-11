using Godot;
using System;

public partial class MiniGridButton : MarginContainer
{
    [Export]
    public ColorRect ButtonColor;
    [Export]
    public ColorRect BorderColor;
    [Export]
    public ColorRect ButtonColorHover;
    [Export]
    public ColorRect BorderColorHover;
    [Export]
    public Button ButtonClicker;
    [Export]
    public Label ButtonLabel;

    private bool _ishovered;
    public bool IsHovered
    {
        get { return _ishovered; }
        set { _ishovered = value; 
        ButtonColorHover.Visible = value; 
        BorderColorHover.Visible = value;
        ButtonColor.Visible = !value; 
        BorderColor.Visible = !value;}
    }

    public delegate void ButtonClickedEvent();
    public ButtonClickedEvent ButtonClicked;

    public override void _Ready()
    {        
        ButtonClicker.Pressed += () => Clicked();
        ButtonClicker.MouseEntered += () => Hovering(true);
        ButtonClicker.MouseExited += () => Hovering(false);
    }

    private void Hovering(bool Hover)
    {
        IsHovered = Hover;
    }

    private void Clicked()
    {
        ButtonClicked?.Invoke();   
    }
}
