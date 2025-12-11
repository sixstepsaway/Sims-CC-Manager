using Godot;
using System;

public partial class HeaderDragger : Node2D
{
	/// <summary>
	/// The dragger for changing the column positions. 
	/// </summary>
	[Export]
	public Line2D VerticalLine;
	[Export]
	public Line2D TopLine;
	[Export]
	public Line2D BottomLine;
	[Export]
	public Line2D TopCurve;
	[Export]
	public Line2D BottomCurve;
	public Color DraggerColor;

	public float Height;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		float height = Height - 2;
		
		VerticalLine.SetPointPosition(1, new(VerticalLine.GetPointPosition(1).X, height));
		BottomLine.SetPointPosition(0, new(BottomLine.GetPointPosition(0).X, height));
		BottomLine.SetPointPosition(1, new(BottomLine.GetPointPosition(1).X, height));
		BottomCurve.SetPointPosition(0, new(BottomCurve.GetPointPosition(0).X, height));
		BottomCurve.SetPointPosition(1, new(BottomCurve.GetPointPosition(1).X, height-2));
		BottomCurve.SetPointPosition(2, new(BottomCurve.GetPointPosition(2).X, height));
		BottomLine.DefaultColor = DraggerColor;
		VerticalLine.DefaultColor = DraggerColor;
		TopLine.DefaultColor = DraggerColor;
		TopCurve.DefaultColor = DraggerColor;
		BottomCurve.DefaultColor = DraggerColor;
	}
}
