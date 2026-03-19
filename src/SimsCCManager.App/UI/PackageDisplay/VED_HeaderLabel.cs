using Godot;
using System;

public partial class VED_HeaderLabel : MarginContainer
{
    [Export]
    Label TypeLabel;
    [Export]
    Label CountLabel;

    private string _typename;
    public string TypeName
    {
        get { return _typename; }
        set { _typename = value; 
        TypeLabel.Text = value; }
    }
    private int _countnum;
    public int CountNum
    {
        get { return _countnum; }
        set { _countnum = value; 
        CountLabel.Text = value.ToString(); }
    }
}
