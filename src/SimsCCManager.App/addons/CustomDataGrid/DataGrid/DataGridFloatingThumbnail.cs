using Godot;
using System;

public partial class DataGridFloatingThumbnail : Node2D
{
	/// <summary>
	/// Hover thumbnail. 
	/// </summary>
	[Export]
	public MarginContainer ThumbnailHolder;
	[Export]
	public MarginContainer ThumbnailSizer;
	[Export]
	public TextureRect ActualThumbnail;
	public ImageTexture ThumbnailImage;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (ThumbnailImage != null){
			ActualThumbnail.Texture = ThumbnailImage;
		}		
	}

}
