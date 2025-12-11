using Godot;
using System;

public partial class SplashScreen : MarginContainer
{
    public delegate void SplashScreenLoadingFinishedEvent();
    public SplashScreenLoadingFinishedEvent SplashScreenLoadingFinished;

    [Export]
    public ProgressBar SplashProgressBar;
    [Export]
    public Label SplashProgrssBarLabel;

    public void IncrementProgressBar(int ByHowMuch)
    {
        SplashProgressBar.Value += ByHowMuch;
        if (SplashProgressBar.Value == SplashProgressBar.MaxValue)
        {
            SplashScreenLoadingFinished.Invoke();
        }
    }

    public void SetPbarMax(int Maximum)
    {
        SplashProgressBar.MaxValue = Maximum;
    }

    public void ChangeProgressBarText(string Text)
    {
        SplashProgrssBarLabel.Text = Text;
    }
}
