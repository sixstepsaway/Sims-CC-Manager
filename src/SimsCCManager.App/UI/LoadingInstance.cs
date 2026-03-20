using Godot;
using System;

public partial class LoadingInstance : Control
{
    [Export]
    public ProgressBar progressBar;
    [Export]
    public Label ProgressLabel;
    [Export]
    public Label StageLabel;
    [Export]
    Background background;
    private int _loadingstage;
    public int LoadingStages
    {
        get { return _loadingstage; }
        set { _loadingstage = value; }   
    }

    private int _stage;
    public int Stage
    {
        get { return _stage; }
        set { _stage = value; }
    }

    private int _maxval;
    public int MaxValue
    {
        get { return _maxval; }
        set { _maxval = value; 
        CallDeferred(nameof(SetPbarMax), value); }
    }
    private int _progress;
    public int Progress
    {
        get { return _progress; }
        set { _progress = value; 
        CallDeferred(nameof(SetPbar), value); }
    }

    private void SetPbarMax(int val)
    {
        if (IsInstanceValid(progressBar)) progressBar.MaxValue = val;
    }
    private void SetPbar(int val)
    {
        if (IsInstanceValid(progressBar)) progressBar.Value = val;
    }

    private string _stagetext;
    public string StageText
    {
        get { return _stagetext; }
        set { _stagetext = value; 
            CallDeferred(nameof(UpdateStageLabel), value); 
        }
    }
    private string _progresstext;
    public string ProgressText
    {
        get { return _progresstext; }
        set { _progresstext = value; 
            CallDeferred(nameof(UpdateProgressLabel), value); 
        }
    }

    public void UpdateStageLabel(string text)
    {
        if (IsInstanceValid(StageLabel)) StageLabel.Text = string.Format("Stage {0}/{1}: {2}", Stage, LoadingStages, text);
    }
    public void UpdateProgressLabel(string text)
    {
        if (IsInstanceValid(ProgressLabel)) ProgressLabel.Text = string.Format("{0}/{1} - {2}", progressBar.Value, progressBar.MaxValue, text);
    }

    public override void _Ready()
    {
        background.UpdateTheme();
        LoadingStages = 2;
        Stage = 1;
    }
}
