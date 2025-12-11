using Godot;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using System;

public partial class MMButton : MarginContainer
{
    [Export]
    TextureRect Icon;
    [Export]
    ColorRect MainColor;
    [Export]
    ColorRect HoverColor;
    [Export]
    ColorRect ClickColor;
    [Export]
    Button ButtonClicker;
    [Export]
    Label TextLabel;
    [Export]
    Node2D Wiggler;
    [Export]
    Panel BGColor;

    public delegate void ButtonClickedEvent();
    public ButtonClickedEvent ButtonClicked;
    internal object presssd;


    public override void _Ready()
    {
        ButtonClicker.Pressed += () => ButtonPressed();
        ButtonClicker.MouseEntered += () => ButtonHovered(true);
        ButtonClicker.MouseExited += () => ButtonHovered(false);
        UpdateColors();
    }

    public void UpdateColors()
    {
        StyleBoxTexture sbt = BGColor.GetThemeStylebox("panel") as StyleBoxTexture;
        GradientTexture1D gradient = new();
        gradient = sbt.Texture as GradientTexture1D;
        gradient.Gradient.SetColor(1, GlobalVariables.LoadedTheme.DataGridTextA);
        gradient.Gradient.SetColor(0, GlobalVariables.LoadedTheme.DataGridTextB);
        sbt.Texture = gradient;
        BGColor.AddThemeStyleboxOverride("panel", sbt);
        MainColor.Color = GlobalVariables.LoadedTheme.ButtonMain;
        HoverColor.Color = GlobalVariables.LoadedTheme.ButtonHover;
        ClickColor.Color = GlobalVariables.LoadedTheme.ButtonClick;
        TextLabel.AddThemeColorOverride("font_color", GlobalVariables.LoadedTheme.MainTextColor);
    }

    private void ButtonHovered(bool Hovering)
    {
        if (Hovering)
        {
            MainColor.Visible = false;
            ClickColor.Visible = false;
            HoverColor.Visible = true;
            Vector2 scale = new(1.05f, 1.05f);
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(Wiggler, "scale", scale, 0.5f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            tween.Play();
        }
        else
        {

            MainColor.Visible = true;
            ClickColor.Visible = false;
            HoverColor.Visible = false;
            Vector2 scale = new(1f, 1f);
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(Wiggler, "scale", scale, 0.5f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
            tween.Play();
        }

    }


    private void ButtonPressed()
    {
        
        MainColor.Visible = false;
        ClickColor.Visible = true;
        HoverColor.Visible = false;
        ButtonClicked.Invoke();
        if (GlobalVariables.DebugMode)
        {
            Logging.WriteDebugLog(string.Format("Button \"{0}\" pressed.", TextLabel.Text));
        }
    }
}

