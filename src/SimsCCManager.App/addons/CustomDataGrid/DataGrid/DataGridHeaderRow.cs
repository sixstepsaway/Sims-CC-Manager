using Godot;
using System;
using System.Collections.Generic;

public partial class DataGridHeaderRow : MarginContainer
{
	/// <summary>
	/// The entire header row for the data grid.
	/// </summary>
	[Export]
	public HBoxContainer HeaderRow;
	[Export]
	public ColorRect BackgroundColor1;
	[Export]
	public ColorRect BackgroundColor2;
	public Vector2 CurrentSize;
	public List<DataGridHeaderCell> HeaderCells = new();
	public List<DataGridHeaderSizeAdjuster> HeaderSliders = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CurrentSize = Size;
	}

	public void SetSize(Vector2 NewSize){
		if (Size.Y > NewSize.Y){
			NewSize = new(NewSize.X, Size.Y);
		}
		Set("custom_minimum_size", NewSize);
		Size = NewSize;
		CurrentSize = NewSize;
	}
}
