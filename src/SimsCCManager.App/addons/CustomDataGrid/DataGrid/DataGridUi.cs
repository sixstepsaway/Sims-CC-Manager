using DataGridContainers;
using Godot;
using System;
using System.Collections.Generic;

public partial class DataGridUi : MarginContainer
{
	/// <summary>
	/// The UI for the data grid. 
	/// </summary>
	
	[ExportCategory("Nodes")]
	[Export]
	public ScrollContainer ScrollContainer;
	[Export]
	public VBoxContainer AllRowsContainer;
	[Export]
	public VBoxContainer HeaderHolder;
	[Export]
	public VBoxContainer RowsHolder;
	[Export]
	public HScrollBar hScrollBar;
	[Export]
	public MarginContainer VScrollContainer;
	[Export]
	public VScrollBar vScrollBar;
	[ExportCategory("Buttons")]
	[Export]
	public Button ScrollUpButton;
	[Export]
	public Button ScrollDownButton;
	[Export]
	public Button ScrollLeftButton;
	[Export]
	public Button ScrollRightButton;
	[ExportCategory("Colors")]
	[Export]
	public ColorRect[] ColorRects_BackgroundColors;
	[Export]
	public ColorRect[] ColorRects_AccentColors;
	[Export]
	public ColorRect[] ColorRects_SecondaryColors;
	[ExportCategory("Fonts")]
	[Export]
	public Font DefaultFont;
	
	

	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//GD.Print("Initializing data grid.");
		//CreateFramework();		
	}	
	
}
