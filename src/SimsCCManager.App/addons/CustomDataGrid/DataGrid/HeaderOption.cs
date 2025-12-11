using Godot;
using System;

public partial class HeaderOption : MarginContainer
{
	public delegate void OptionClickedEvent(string Option, bool selected);
	public OptionClickedEvent OptionClicked;
	[Export]
	public Label HeaderLabel;
	[Export]
	public Panel HighlightPanel;
	[Export]
	public ColorRect HighlightPanelColor;
	private string _headername;
	public string HeaderName
	{
		get { return _headername; }
		set { 
			_headername = value; 
			HeaderLabel.Text = value;
		}
	}
	[Export]
	public CheckBox HeaderCheck;
	[Export]
	public Button OptionButton;
	private bool _selected;
	public bool Selected { 
		get { return _selected; }
		set { _selected = value; 
			HeaderCheck.ButtonPressed = value;
		}
	}
	public int TextSize;
	public bool MouseInOption = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		OptionButton.Pressed += () => OptionPressed();
		HeaderLabel.AddThemeConstantOverride("font_size", TextSize);
		OptionButton.MouseEntered += () => MouseInOptionToggle(true);
		OptionButton.MouseExited += () => MouseInOptionToggle(false);
	}

    private void MouseInOptionToggle(bool v)
    {
        MouseInOption = v;
		HighlightPanel.Visible = v;
    }

    private void OptionPressed()
    {
        Selected = !Selected;
		OptionClicked.Invoke(HeaderName, Selected);		
    }
}
