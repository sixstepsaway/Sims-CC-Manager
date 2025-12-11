using Godot;
using System;

public partial class DataGridHeaderSizeAdjuster : MarginContainer
{
	/// <summary>
	/// The sliding adjuster for the data grid headers, to change cell width.
	/// </summary>
	public DataGridHeaderCell AttachedHeaderCell;
	[Export]
	public Button ClickDetector;
	bool HoldingButton = false;
	bool Enabled = false;
	Vector2 MousePosition = new();
	public delegate void HeaderResizedEvent(int index);
	public HeaderResizedEvent HeaderResized;
	Vector2 SliderPosition;
	public bool Resizeable = false;
	public int HeaderMinsize = 25;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Resizeable){
			ClickDetector.ButtonUp += () => ReleasedButton();
			ClickDetector.ButtonDown += () => HeldButton();
		} else {
			ClickDetector.MouseDefaultCursorShape = CursorShape.Arrow;
		}
	}

    private void ClickedButton()
    {
        throw new NotImplementedException();
    }


    private void ReleasedButton()
    {
        HoldingButton = false;
    }


    private void HeldButton()
    {
        MousePosition = GetGlobalMousePosition();
		SliderPosition = GetGlobalMousePosition();
		HoldingButton = true;
    }



    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		if (HoldingButton){
			if (AttachedHeaderCell.CellSize.X <= 25 && GetGlobalMousePosition().X <= SliderPosition.X){
				//
			} else if (GetGlobalMousePosition().X > MousePosition.X){
				float x = AttachedHeaderCell.CellSize.X;
				float dif = GetGlobalMousePosition().X - MousePosition.X;
				x += dif;
				if (x < HeaderMinsize){
					x = HeaderMinsize;
				}
				AttachedHeaderCell.ChangeSize(new (x, AttachedHeaderCell.CellSize.Y));
				HeaderResized.Invoke(AttachedHeaderCell.HeaderIndex);
			} else if (GetGlobalMousePosition().X < MousePosition.X){
				float x = AttachedHeaderCell.CellSize.X;
				float dif = MousePosition.X - GetGlobalMousePosition().X;
				x -= dif;
				if (x < HeaderMinsize){
					x = HeaderMinsize;
				}
				AttachedHeaderCell.ChangeSize(new (x, AttachedHeaderCell.CellSize.Y));
				HeaderResized.Invoke(AttachedHeaderCell.HeaderIndex);
			}
			MousePosition = GetGlobalMousePosition();
		}
	}
}
