using Godot;
using System;

public partial class CustomPopupWindow : Window
{
    [Export]
    ColorRect BGColor;
    [Export]
    public RichTextLabel WindowMessage;
    [Export]
    public Button YesButton; 
    [Export]
    public Button NoButton; 
    private string _windowtitle;
    public string WindowTitle
    {
        get { return _windowtitle; }
        set { _windowtitle = value; 
        this.Title = value;}
    }
}
