using Godot;
using System;

public partial class LoadingInstance : Control
{
    [Export]
    public ProgressBar progressBar;
    [Export]
    public Label ProgressLabel;
    [Export]
    Background background;

    public override void _Ready()
    {
        background.UpdateTheme();
    }
}
