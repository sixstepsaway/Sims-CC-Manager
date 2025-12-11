using Godot;
using System;

public partial class GameRunningPopup : MarginContainer
{
    [Export]
    Button DisconnectButton;

    public delegate void DisconnectFromGameEvent();
    public DisconnectFromGameEvent DisconnectFromGame;

    public override void _Ready()
    {
        DisconnectButton.Pressed += () => DisconnectPanel();
    }

    private void DisconnectPanel()
    {
        DisconnectFromGame.Invoke();
    }
}
