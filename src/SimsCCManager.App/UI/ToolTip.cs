using Godot;
using System;

public partial class ToolTip : Node2D
{
    [Export]
    public Label TooltipText;
    public override void _Ready()
    {
        base._Ready();
    }

}
