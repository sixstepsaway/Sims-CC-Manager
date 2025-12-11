using DataGridContainers;
using Godot;
using System;

public partial class MiniGriditem : MarginContainer
{
    [Export]
    ColorRect BGColor;
    [Export]
    ColorRect SelectedColor;
    [Export]
    public Label TextOne;
    [Export]
    public Label TextTwo;
    [Export]
    public Button SelectButton;

    public DataGridRow RowData;

    public int MgiIndex;


    private bool _isselected;
    public bool IsSelected
    {
        get { return _isselected; }
        set { _isselected = value; 
        SelectedColor.Visible = value; 
        BGColor.Visible = !value; }
    }
}
