using Godot;
using System;
using System.Collections.Generic;

public partial class MiniGrid : Control
{
    [Export]
    public FileDialog addFiles;
    [Export]
    MarginContainer MinigridHolder;
    [Export]
    public VBoxContainer ItemContainer;
    [Export]
    public Button CloseButton;
    [Export]
    public ColorRect[] MainColors;
    [Export]
    public ColorRect[] HoverColors;
    [Export]
    public MiniGridButton RemoveButton;
    [Export]
    public MiniGridButton AddButton;

    public int MiniGridRowIdx; 

    public List<MiniGriditem> MinigridItems = new();

    public DataGridRowUi Row;

    public override void _Ready()
    {
        CloseButton.MouseEntered += () => Hovering(true);
        CloseButton.MouseExited += () => Hovering(false);
    }

    private void Hovering(bool Hover)
    {
        foreach (ColorRect color in MainColors)
        {
            color.Visible = !Hover;
        }
        foreach (ColorRect color in HoverColors)
        {
            color.Visible = Hover;
        }
    }
}

