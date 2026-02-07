using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Globals;
using System;
using System.IO;

public partial class PackageListItem : HBoxContainer
{
    public SimsPackage packageItem;
    [Export]
    public LineEdit NameBox;
    [Export]
    public Button Button;
    [Export]
    Node2D ButtonSpin;
    [Export]
    Panel WarningPanel;

    private bool _warning;
    public bool Warning
    {
        get { return _warning; }
        set { _warning = value; 
        WarningPanel.Visible = value;}
    }


    bool InternalName = false;

    public override void _Ready()
    {
        Button.Pressed += () => PressedTheButton();
        Warning = false;
    }

    public delegate void GotNamesEvent();
    public GotNamesEvent GotNames;

    private void PressedTheButton()
    {
        Spin();
        FlipName();
    }

    private void FlipName()
    {        
        if (InternalName)
        {
            ResetName();
        } else
        {
            GetInternalName();
        }
        InternalName = !InternalName;
    }

    public void GetInternalName()
    {
        NameBox.Text = Utilities.SafeFileName(packageItem.PackageData.Title); 
        GotNames?.Invoke();       
    }

    public void ResetName()
    {
        NameBox.Text = packageItem.FileName;
    }

    public void Spin()
    {
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(ButtonSpin, "rotation_degrees", 360f, 0.5f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        tween.Play();
    }
}
