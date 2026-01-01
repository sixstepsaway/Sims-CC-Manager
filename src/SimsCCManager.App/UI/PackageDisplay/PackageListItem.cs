using Godot;
using SimsCCManager.Containers;
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
    bool InternalName = false;

    public override void _Ready()
    {
        Button.Pressed += () => PressedTheButton();
    }

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
        NameBox.Text = packageItem.PackageData.Title;        
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
