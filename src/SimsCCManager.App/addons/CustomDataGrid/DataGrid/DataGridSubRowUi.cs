using Godot;
using System;

public partial class DataGridSubRowUi : MarginContainer
{
    [Export]
    public HBoxContainer RowHolder;
    [Export]
    Button rowButton;
    [Export]
    ColorRect BGColor;
    private Color _backgroundcolor;
    public Color BackgroundColor { get {return _backgroundcolor; }
    set { _backgroundcolor = value; 
    BGColor.Color = value;}}
    public Color SelectedColor;
    public Color TextColor;
    public int TextSize;

    public string SubrowTextFirst;
    public string SubrowTextSecond;

    public int SubrowIdx = 0;
}
