using DataGridContainers;
using Godot;
using System;

public partial class DataGridIconOption : TextureRect
{
	/// <summary>
	/// An icon displayed in an icon cell.
	/// </summary>
	
	public delegate void ProduceTooltipEvent(string Tooltip);
	public ProduceTooltipEvent ProduceTooltip;
	[Export]
	public Texture2D IconImage;
	private bool _iconvisible;
	[Export]
	public bool IconVisible {get { return _iconvisible; } 
	set {_iconvisible = value; 
		Visible = value; }}
	[Export]
	public string IconName = "Icon name must match node name";
	public int IconIndex = -1;
	public DataGridCellIcons IconInfo = new();
	[Export]
	public ColorRect IconColor;
	[Export]
	Button hoverbutton;
	public string IconTooltip = "";


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (Texture != null) Texture = IconImage;
		hoverbutton.MouseEntered += () => Hover(true);
		hoverbutton.MouseExited += () => Hover(false);
	}

    private void Hover(bool v)
    {
        if (v) ProduceTooltip.Invoke(IconTooltip);
    }
	private bool IsMouseInIcon(){
		Vector2 mousepos = GetGlobalMousePosition(); 

		Rect2 rect = new (this.GlobalPosition, this.Size); 

		return rect.HasPoint(mousepos); 
	}
}
