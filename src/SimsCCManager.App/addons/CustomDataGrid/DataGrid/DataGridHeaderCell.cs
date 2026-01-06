using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DataGridHeaderCell : Control
{
	///<summary>
	/// The header cell for the data grid. 
	/// </summary>
	public DataGrid dataGrid;
	PackedScene HeaderDraggerPS = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/HeaderDragger.tscn");
	public delegate void HeaderClickedEvent(int idx, bool SortAscending, bool Reset);
	public event HeaderClickedEvent HeaderClicked;
	public delegate void ColumnMovedEvent(int headerIdx, int HeaderNewLocation);
	public ColumnMovedEvent ColumnMoved;
	public string LabelText = "";
	public bool Blank = false;
	private bool _sorted;
	public bool Sorted { get { return _sorted; }
	set {_sorted = value; 
	SortedIcon.Visible = value;}}
	private bool _sortascending;
	public bool SortAscending {get {return _sortascending; }
	set { _sortascending = value; 
		if (value) {
			Arrow.Texture = ArrowUp;
		} else {
			Arrow.Texture = ArrowDown;
		}
	}}

	private Color _textcolor;
	public Color TextColor
	{
		get { return _textcolor; }
		set { _textcolor = value; 
		HeaderLabel.AddThemeColorOverride("font_color", value); }
	}

	[Export]
	public Label HeaderLabel;
	[Export]
	public Button Clicker;
	[Export]
	public TextureRect Arrow;
	[Export]
	public ColorRect[] Accents;
	[Export]
	public MarginContainer SortedIcon;
	[Export]
	public Texture2D ArrowDown;	
	[Export]
	public Texture2D ArrowUp;
	[Export]
	public MarginContainer Container;
	[Export]
	public Button ClickHoldButton;
	[Export]
	public Color DraggerColor;
	public Node2D Node2DFloater;
	public DataGrid MainGrid;
	public HeaderDragger HeaderDraggerThingy;
	public Vector2 CellSize = new();
	public int HeaderIndex;
	public string HeaderData;
	public int StartingWidth;
	public int FontSize;
	public Godot.Timer HeldClickTimer;
	public bool HoldingHeader = false;

	
	public int CurrentSlot = -1;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
Clicker.Pressed += () => HeaderPressed();		
		if (!string.IsNullOrWhiteSpace(LabelText) && !Blank) {
HeaderLabel.Text = LabelText;
		}
		SetStartingSize();
ClickHoldButton.ButtonDown += () => HeaderClickInteraction(true);
		ClickHoldButton.ButtonUp += () => HeaderClickInteraction(false);
	}

    private void HeaderClickInteraction(bool ButtonDown)
    {
        if (ButtonDown){
			ListenForHeldKeyTimer();
		} else {
			HeldClickTimer?.QueueFree();
			HeldClickTimer = null;
			if (!HoldingHeader){
				HeaderPressed();
			} else {
				if (HeaderDraggerThingy != null) { 
					HeaderDraggerThingy.QueueFree();
					HeaderDraggerThingy = null; 
				}
				ColumnMoved.Invoke(HeaderIndex, CurrentSlot + 1);
			}
			HoldingHeader = false;
		}
    }

    public void SetStartingSize(){
		if (StartingWidth == 0){
if (!string.IsNullOrWhiteSpace(LabelText) && !Blank){
				int x = (LabelText.Length * FontSize) + 5;
CellSize = new(x, Size.Y);
				SetSize(CellSize);
			} else {
CellSize = new(30, Size.Y);
				SetSize(CellSize);
			}
		} else {
CellSize = new(StartingWidth, Size.Y);
			SetSize(CellSize);
		}
		
	}

    private void HeaderPressed()
    {
		bool reset = false;
		if (Sorted && SortAscending){
			SortAscending = false;
		} else if (Sorted && !SortAscending){
			Sorted = false;
			reset = true;
			SortAscending = false;
		} else if (!Sorted){
			Sorted = true;
			SortAscending = true;
		}
		dataGrid.PassLog(string.Format("Sorting {0} by Ascending: {1}", this.Name, SortAscending));
        HeaderClicked.Invoke(HeaderIndex, SortAscending, reset);
    }

	public void ChangeSize(Vector2 NewSize){
		if (Size.Y > NewSize.Y){
			NewSize = new(NewSize.X, Size.Y);
		}
		Tween tween = GetTree().CreateTween();
		if (NewSize.X > CellSize.X){
			tween.TweenProperty(this, "custom_minimum_size", NewSize, 0.2f).SetTrans(Tween.TransitionType.Spring).SetEase(Tween.EaseType.Out);
			tween.TweenProperty(this, "size", NewSize, 0.2f).SetTrans(Tween.TransitionType.Spring).SetEase(Tween.EaseType.Out);
		} else {
			tween.TweenProperty(this, "custom_minimum_size", NewSize, 0.2f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
			tween.TweenProperty(this, "size", NewSize, 0.2f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		}		
		tween.Play();
		CellSize = NewSize;
	}
	public void SetSize(Vector2 NewSize){
		if (Size.Y > NewSize.Y){
			NewSize = new(NewSize.X, Size.Y);
		}
		Set("custom_minimum_size", NewSize);
		Size = NewSize;
		CellSize = NewSize;
	}

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMovement && HoldingHeader)
		{
			List<float> DistancesTo = new();
			foreach (DataGridHeaderSizeAdjuster sizeadjuster in MainGrid.HeaderRow.HeaderSliders){
				DistancesTo.Add(sizeadjuster.Position.DistanceTo(mouseMovement.Position));
			}
			CurrentSlot = DistancesTo.IndexOf(DistancesTo.Min());		
			HeaderDraggerThingy.Position = new(MainGrid.HeaderRow.HeaderSliders[CurrentSlot].Position.X, this.Position.Y);
		}
    }


	private void ListenForHeldKeyTimer(){
		if (HeldClickTimer != null) { 
			//ScrollHeldTimer.Timeout -= () => ResetKeyPress();
			HeldClickTimer.QueueFree();	
			HeldClickTimer = null;		
		}
		HeldClickTimer = new();
		AddChild(HeldClickTimer);
		HeldClickTimer.OneShot = true;
		HeldClickTimer.WaitTime = 0.15;
		HeldClickTimer.Timeout += () => ResetHeldKeyTimer();
		HeldClickTimer.Start();		
	}

	private void ResetHeldKeyTimer()
    {
        HoldingHeader = true;
		HeaderDraggerThingy = HeaderDraggerPS.Instantiate() as HeaderDragger;
		HeaderDraggerThingy.DraggerColor = DraggerColor;
		HeaderDraggerThingy.Height = MainGrid.HeaderRow.Size.Y;
		
		MainGrid.AddChild(HeaderDraggerThingy);
    }

	private bool IsMouseInHeader(){
		Vector2 mousepos = GetGlobalMousePosition(); 

		Rect2 rect = new (this.GlobalPosition, this.Size); 

		return rect.HasPoint(mousepos); 
	}

}
