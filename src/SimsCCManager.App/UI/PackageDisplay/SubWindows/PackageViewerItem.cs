using Godot;
using System;

public partial class PackageViewerItem : MarginContainer
{
    [Export]
    Label Key;
    [Export]
    Label Value;
    [Export]
    Panel BG;
    [Export]
    HBoxContainer Short;
    [Export]
    VBoxContainer Long;
    [Export]
    RichTextLabel LongValue;
    [Export]
    Label LongKey;

    private bool _islong;
    public bool IsLong
    {
        get { return _islong; }
        set { _islong = value;
        Long.Visible = value;
        Short.Visible = !value; }
    }

    private Color _bgColor;
    public Color BGColor
    {
        get { return _bgColor; }
        set { _bgColor = value; 
        StyleBoxFlat sb = BG.GetThemeStylebox("Panel") as StyleBoxFlat; 
        sb.BgColor = value; 
        BG.AddThemeStyleboxOverride("Panel", sb); }
    }
    private Color _borderColor;
    public Color BorderColor
    {
        get { return _borderColor; }
        set { _borderColor = value; 
        StyleBoxFlat sb = BG.GetThemeStylebox("Panel") as StyleBoxFlat; 
        sb.BorderColor = value; 
        BG.AddThemeStyleboxOverride("Panel", sb); }
    }

    private string _keyText;
    public string KeyText {
        get { return _keyText; }
        set { _keyText = value; 
        Key.Text = value; 
        LongKey.Text = value; }
    }
    private string _valueText;
    public string ValueText {
        get { return _valueText; }
        set { _valueText = value; 
        Value.Text = value;
        LongValue.Text = value; }
    }
}
