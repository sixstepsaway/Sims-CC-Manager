using Godot;
using System;
using System.Collections.Generic;

public partial class HeaderClickMenuHolder : Node2D
{
	/// <summary>
	/// Menu displayed when header is right-clicked.
	/// </summary>
	
	public delegate void OptionClickedEvent(string Option, bool Selected);
	public OptionClickedEvent OptionClicked;
	public delegate void CloseMenuEvent();
	public CloseMenuEvent CloseMenu;
	PackedScene HeaderOptionPS = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/HeaderOption.tscn");
	[Export]
	public MarginContainer MenuContainer;
	[Export]
	public ColorRect BackgroundColor;
	[Export]
	public VBoxContainer HeaderOptionsContainer;
	[Export]
	public Panel ColorPanel;
	public Dictionary<string, bool> HeaderOptions = new();
	public Color BGColor;
	public Color TextColor;
	public Color AccentColor;
	public Color AltColor;
	public Color TrimColor;

	public int TextSize;
	public Vector2 MyPosition;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MenuContainer.Position = MyPosition;
		foreach (KeyValuePair<string, bool> item in HeaderOptions){
			HeaderOption ho = HeaderOptionPS.Instantiate() as HeaderOption;
			ho.HeaderName = item.Key;
			ho.Selected = item.Value;
			ho.HeaderLabel.AddThemeColorOverride("font_color", TextColor);
			ho.OptionClicked += (x, y) => HeaderOptionClicked(x, y);
			ho.TextSize = TextSize;
			ho.HighlightPanelColor.Color = AccentColor;
			HeaderOptionsContainer.AddChild(ho);
		}
		BackgroundColor.Color = BGColor;
		StyleBoxFlat box = (StyleBoxFlat)ColorPanel.GetThemeStylebox("panel");
		TrimColor = AccentColor;
		GD.Print(TrimColor.ToHtml());
		/*if (IsLight(AccentColor)){
			TrimColor = AccentColor.Darkened(0.1f);
		} else {
			TrimColor = AccentColor.Lightened(0.1f);
		}*/
		box.BorderColor = TrimColor;
		ColorPanel.AddThemeStyleboxOverride("panel", box);
	}

    private void HeaderOptionClicked(string x, bool y)
    {
        OptionClicked.Invoke(x, y);
    }

	public override void _Input(InputEvent @event)
    {
        if (!IsMouseInMenu()){ 
			if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed){
				if (mouseEvent.ButtonIndex == MouseButton.Left){
					CloseMenu.Invoke();
				}
			} 
			if (@event is InputEventMouseButton mouseClick && mouseClick.Pressed){
				if (mouseClick.ButtonIndex == MouseButton.Right){
					CloseMenu.Invoke();
				}
			}
		}		
    }

	private bool IsMouseInMenu(){
		Vector2 mousepos = GetGlobalMousePosition(); 

		Rect2 rect = new (MenuContainer.GlobalPosition, MenuContainer.Size); 

		return rect.HasPoint(mousepos); 
	}

	public static bool IsLight(Color colorinput){
		float h;
		float s;
		float v;
		colorinput.ToHsv(out h, out s, out v);
		
		//float newv;
		
		if (v > 0.5){
			return true; 
		} else {
			return false;
		}
	}

}
