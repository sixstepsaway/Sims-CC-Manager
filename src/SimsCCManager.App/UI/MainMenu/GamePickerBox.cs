using Godot;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.OptionLists;
using System;

public partial class GamePickerBox : MarginContainer
{
    [Export]
    TextureRect GameIcon;
    [Export]
    Label GameLabel;
    [Export]
    Button ButtonClicker;
    [Export]
    MarginContainer SelectedBorder;
    [Export]
    string GameString = "";

    private bool _selected;
    public bool Selected
    {
        get { return _selected; }
        set
        {
            _selected = value;
            ChangeSelected();
        }
    }

    public SimsGames thisGame;

    public delegate void GamePickedEvent(SimsGames game, bool isSelected);
    public GamePickedEvent GamePicked;

    public override void _Ready()
    {
        ButtonClicker.Pressed += () => ButtonClicked(); 
        if (GameString == "Sims 2")
        {
            thisGame = SimsGames.Sims2;
        } else if (GameString == "Sims 3")
        {
            thisGame = SimsGames.Sims3;
        } else if (GameString == "Sims 4")
        {
            thisGame = SimsGames.Sims4;
        } 
    }

    private void ButtonClicked()
    {
        Selected = !Selected;
        GamePicked.Invoke(thisGame, Selected);
        if (GlobalVariables.DebugMode)
        {
            Logging.WriteDebugLog(string.Format("Game \"{0}\" selected.", thisGame));
        }
    }
    
    private void ChangeSelected()
    {
        SelectedBorder.Visible = Selected; 
    }
}
