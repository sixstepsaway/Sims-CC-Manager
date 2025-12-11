using Godot;
using System;

public partial class CategoryOption : MarginContainer
{
    [Export]
    public Label label;
    [Export]
    public ColorRect BGColor;
    [Export]
    public CustomSquareCheckbox CheckBox;
    [Export]
    public Button button;

    public bool IsToggled { get {return CheckBox.IsChecked; }}
}
