using DataGridContainers;
using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

public partial class DataGridCell : Control
{
	/// <summary>
	/// An individual cell for the datagrid. 
	/// </summary>
	
	//public SceneTree ThisSceneTree;

	public delegate void ToggleHoverEvent(bool Hover);
	public ToggleHoverEvent ToggleHovered;
	public delegate void ToggleFlippedEvent(bool Toggled);
	public ToggleFlippedEvent ToggleFlipped;
	public delegate void ShowNumberAdjusterEvent(bool Bool);
	public ShowNumberAdjusterEvent ShowNumberAdjuster;
	public delegate void NumberAdjustedEvent(int num, bool solo);
	public NumberAdjustedEvent NumAdjusted;
	public delegate void ProduceTooltipEvent(string Tooltip);
	public ProduceTooltipEvent ProduceTooltip;

	PackedScene DataGridIcon = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/DataGridIconOption.tscn");
	PackedScene HoverThumbnail = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/DataGridFloatingThumbnail.tscn");

	[ExportGroup("Controls")]
	[Export]
	public Button ClickButton;
	[ExportGroup("Text")]
	[Export]
	public MarginContainer TextContainer;
	[Export]
	public LineEdit TextHolder;
	[ExportGroup("NumberAdjuster")]
	[Export]
	public MarginContainer NumContainer;
	[Export]
	public MarginContainer NumAdjusterPlaceholder;
	[Export]
	public Button NumUp;
	[Export]
	public Button NumDown;
	[Export]
	public ColorRect NumUpColor;
	[Export]
	public ColorRect NumDownColor;
	[Export]
	public LineEdit NumberHolder;
	[Export]
	public ColorRect AdjusterBg;
	[ExportGroup("Icons")]
	[Export]
	public MarginContainer IconsContainer;
	[Export]
	public HBoxContainer IconsRow;
	[ExportGroup("Bool")]
	[Export]
	public MarginContainer TrueFalseContainer;
	[Export]
	public TextureRect TrueImage;
	[Export]
	public TextureRect FalseImage;
	[Export]
	public ColorRect TrueColor;
	[Export]
	public ColorRect FalseColor;
	[ExportGroup("Color")]
	[Export]
	public MarginContainer ColorContainer;
	[ExportGroup("Image")]
	[Export]
	public MarginContainer ImageContainer;
	[Export]
	public MarginContainer ImageSizer;
	[Export]
	public TextureRect ImageTextureRect;
	[ExportGroup("Number Adjuster")]
	[Export]
	public MarginContainer NumberAdjusterContainer;
	[ExportGroup("Toggle")]
	[Export]
	public MarginContainer ToggleContainer;
	[Export]
	public TextureRect ToggleSelectedImage;
	[Export]
	public TextureRect ToggleUnselectedImage;
	[Export]
	public TextureRect ToggleHoverImage;
	[Export]
	public ColorRect ToggleSelectedColor;
	[Export]
	public ColorRect ToggleUnselectedColor;
	[Export]
	public ColorRect ToggleHoverColor;
	[ExportGroup("Custom")]
	[Export]
	public MarginContainer CustomContainer;

	private string _textcontent;
	public string TextContent
	{
		get {return _textcontent; }
		set { _textcontent = value; 
		TextHolder.Text = value;
			if (CellOptions == CellOptions.Text)
			{
				PotentialTooltip = value;
			}
		}
	}
	private int _numbercontent;
	public int NumberContent
	{
		get { return _numbercontent; }
		set
		{
			_numbercontent = value;
			NumberHolder.Text = value.ToString();
			if (CellOptions == CellOptions.AdjustableNumber)
			{		
				if (value == -1)
				{
					NumberHolder.Text = string.Empty;
				}
				else
				{
					NumberHolder.Text = value.ToString();
				}		
				//GD.Print(string.Format("Row {0} adj number: {1}", thisRow.OverallIndex, value));
			}
			if (CellOptions == CellOptions.Int || CellOptions == CellOptions.AdjustableNumber)
			{
				PotentialTooltip = NumberContent.ToString();
			}
		}
	}
	private bool _toggletoggled;
	public bool ToggleToggled {
		get { return _toggletoggled; }
		set { _toggletoggled = value; 
			ToggleSelectedImage.Visible = value;
			ToggleUnselectedImage.Visible = !value;
		}
	}
	public bool Resizeable = false;
	private bool FirstSet = false;
	public Vector2 StartSize = new();
	public Vector2 CellSize = new();
	public List<DataGridCellIcons> Icons = new();
	public CellOptions CellOptions = CellOptions.Text;
	public dynamic CustomContainerItem;
	public Color FontColor;
	public Color AccentColor;
	public int FontSize;
	public Font MainFont;
	public DataGridHeaderCell AssociatedHeader;
	public DataGridHeaderSizeAdjuster AssociatedHeaderSizer;
	public List<DataGridIconOption> IconList = new();
	public bool FirstLoaded = false;
	private bool _hovering;
	public bool Hovering { get {return _hovering; }
	set {_hovering = value;
	ToggleHoverImage.Visible = value;}}



	public bool Editable = false;
	public string ImageLocation;
	ImageTexture ThumbnailImage;
	public DataGrid dataGrid;
	DataGridFloatingThumbnail floatingThumbnail;
	bool thumbnailvisible = false;
	private bool _shownumberadjustercontrols;
	public bool ShowNumberAdjusterControls
	{
		get { return _shownumberadjustercontrols; }
		set { _shownumberadjustercontrols = value; 
			NumberAdjusterContainer.Visible = value;
			NumAdjusterPlaceholder.Visible = !value;
			NumberHolder.Editable = value;
			ClickButton.Disabled = value;
			if (value)
			{
				Rect2 rect = new(this.GlobalPosition, this.Size);
				AdjusterBg.Position = new(rect.Position.X - 2, rect.Position.Y - 2);
			}
		}
	}
	public bool NumAdjusterAllowDupes = false;

	public bool AllowThumbs = true;
	public string PotentialTooltip = "";
	public DataGridRowUi thisRow;
	public bool ToggleLinked;
	public bool NumberAsBytes = false;

	public bool IsVisibleInDataGrid { get { return thisRow.VisibleOnScreen; }}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		if (NumberAsBytes)
		{
			CellOptions = CellOptions.Text;
			TextContent = SizeSuffix(NumberContent, 2);
		}
		if (CellOptions == CellOptions.Icons)
		{
			IconsContainer.Visible = true;
			foreach (DataGridCellIcons icon in Icons)
			{
				DataGridIconOption dgicon = DataGridIcon.Instantiate() as DataGridIconOption;
				IconList.Add(dgicon);
				dgicon.IconVisible = icon.IconVisible;
				dgicon.CustomMinimumSize = new(icon.IconSize, icon.IconSize);
				dgicon.IconName = icon.IconName;
				dgicon.IconImage = icon.IconImage;
				dgicon.IconColor.Color = FontColor;
				dgicon.IconTooltip = icon.TooltipMessage;
				dgicon.ProduceTooltip += (t) => TooltipFromIcon(t);
				IconsRow.AddChild(dgicon);
			}

			if (!FirstLoaded)
			{
				AssociatedHeader.SetSize(new(Icons.Count * (Icons[0].IconSize + 5), 0));
				AssociatedHeaderSizer.HeaderResized?.Invoke(AssociatedHeaderSizer.AttachedHeaderCell.HeaderIndex);
			}

		}
		else if (CellOptions == CellOptions.TrueFalse)
		{
			TrueFalseContainer.Visible = true;
		}
		else if (CellOptions == CellOptions.Toggle)
		{
			ToggleContainer.Visible = true;
		}
		else if (CellOptions == CellOptions.Int)
		{
			NumContainer.Visible = true;
			NumberHolder.Text = NumberContent.ToString();
		}
		else if (CellOptions == CellOptions.Custom)
		{
			CustomContainer.Visible = true;
			if (CustomContainerItem != null) CustomContainer.AddChild(CustomContainerItem);
		}
		else if (CellOptions == CellOptions.Picture)
		{
			ImageContainer.Visible = true;
			if (File.Exists(ImageLocation))
			{
				Image newimage = new();
				newimage = Image.LoadFromFile(ImageLocation);
				ThumbnailImage = ImageTexture.CreateFromImage(newimage);
				ImageTextureRect.Texture = ThumbnailImage;
				float proportions = newimage.GetWidth() / newimage.GetHeight();
				Vector2 imagesize = new(0, 0);
				if (proportions < 1)
				{
					proportions = newimage.GetHeight() / newimage.GetWidth();
					imagesize = new(StartSize.X, StartSize.X * proportions);
				}
				else
				{
					proportions = newimage.GetHeight() / newimage.GetWidth();
					imagesize = new(StartSize.X, StartSize.X * proportions);
				}
				ImageContainer.CustomMinimumSize = imagesize;
			}
		}
		else if (CellOptions == CellOptions.AdjustableNumber)
		{
			NumContainer.Visible = true;
			NumDownColor.Color = FontColor;
			NumUpColor.Color = FontColor;
			NumUp.Pressed += () => NumChanged(true);
			NumDown.Pressed += () => NumChanged(false);
			NumUp.MouseEntered += () => NumUpHovered(true);
			NumUp.MouseExited += () => NumUpHovered(false);
			NumDown.MouseEntered += () => NumDownHovered(true);
			NumDown.MouseExited += () => NumDownHovered(false);
		}
		else
		{
			TextContainer.Visible = true;
			TextHolder.Text = TextContent;
		}

		ShowNumberAdjusterControls = false;

		SetTextOptions();
	}

    private void NumDownHovered(bool v)
	{
		if (v)
		{
			NumDownColor.Color = AccentColor;
		}
		else
		{
			NumDownColor.Color = FontColor;
		}
	}


	private void NumUpHovered(bool v)
	{
		if (v)
		{
			NumUpColor.Color = AccentColor;
		}
		else
		{
			NumUpColor.Color = FontColor;
		}
	}


	private void NumChanged(bool Up)
	{		
		if (!dataGrid.IsThereMultipleAdjustmentNumbers())
		{
			if (Up)
			{
				NumberContent++;
				NumberHolder.Text = NumberContent.ToString();
			}
			else
			{
				if (NumberContent >= 1)
				{
					NumberContent--;
					NumberHolder.Text = NumberContent.ToString();
				}
			}
			NumAdjusted?.Invoke(NumberContent, true);
		}
		else
		{
			int num = NumberContent;
			if (Up)
			{
				num++;
			}
			else
			{
				if (NumberContent >= 1)
				{
					num--;
				}
			}
			NumAdjusted?.Invoke(num, false);
		}				
	}

	public void Toggle()
	{
		ToggleToggled = !ToggleToggled;
		ToggleFlipped?.Invoke(ToggleToggled);
	}

	public void SetToggle(bool WhichWay)
	{
		ToggleToggled = WhichWay;
		ToggleFlipped?.Invoke(ToggleToggled);
	}

	private void SetTextOptions()
	{
		TextHolder.AddThemeColorOverride("font_uneditable_color", FontColor);
		NumberHolder.AddThemeColorOverride("font_uneditable_color", FontColor);
		TextHolder.AddThemeColorOverride("font_color", FontColor);
		NumberHolder.AddThemeColorOverride("font_color", FontColor);
		TextHolder.AddThemeConstantOverride("font_size", FontSize);
		NumberHolder.AddThemeConstantOverride("font_size", FontSize);
	}

	public void SetSize(Vector2 size)
	{
		if (CellOptions == CellOptions.Text)
		{
			size = new(size.X + 1, size.Y);
		}
		else if (CellOptions == CellOptions.TrueFalse)
		{
			size = new(size.X + 1, size.Y);
		}
		else if (CellOptions == CellOptions.Icons)
		{
			size = new(size.X + 1, size.Y);
		}
		else
		{
			size = new(size.X + 2, size.Y);
		}
		if (!FirstSet)
		{
			Size = size;
			Set("custom_minimum_size", size);
			CellSize = Size;
			FirstSet = true;
		}
		else
		{
			if (IsVisibleInDataGrid)
			{
				Tween tween = GetTree().CreateTween();
				if (size.X > CellSize.X)
				{
					tween.TweenProperty(this, "custom_minimum_size", size, 0.2f).SetTrans(Tween.TransitionType.Spring).SetEase(Tween.EaseType.Out);
					tween.TweenProperty(this, "size", size, 0.2f).SetTrans(Tween.TransitionType.Spring).SetEase(Tween.EaseType.Out);
				}
				else
				{
					tween.TweenProperty(this, "custom_minimum_size", size, 0.2f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
					tween.TweenProperty(this, "size", size, 0.2f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
				}
				tween.Play();
				CellSize = size;
			} else
			{
				CellSize = size;
				CustomMinimumSize = CellSize;
				Size = CellSize;
			}			
		}
	}

	private void TooltipFromIcon(string t)
	{
		PotentialTooltip = t;
		ProduceTooltip?.Invoke(PotentialTooltip);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion movement)
		{
			if (CellOptions == CellOptions.Toggle || CellOptions == CellOptions.AdjustableNumber)
			{
				if (IsMouseInCell())
				{
					if (!Hovering)
					{
						ToggleHovered?.Invoke(true);
					}
					Hovering = true;
				}
				else
				{
					if (Hovering)
					{
						ToggleHovered?.Invoke(false);
					}
					Hovering = false;
				}
			}
			if (IsMouseInCell() && CellOptions != CellOptions.Icons)
			{
				ProduceTooltip?.Invoke(PotentialTooltip);
			}
		}

		if (IsMouseInCell() && CellOptions == CellOptions.Toggle)
		{
			if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				Toggle();
			}
		}
		/*else if (IsMouseInCell() && CellOptions == CellOptions.Picture && !thumbnailvisible)
		{
			if (ThumbnailImage != null && AllowThumbs)
			{
				thumbnailvisible = true;
				SpawnThumbnail();
			}
		}*/
		else if (IsMouseInCell() && CellOptions == CellOptions.AdjustableNumber && !ShowNumberAdjusterControls)
		{
			if (@event is InputEventMouseButton mouseEvent)
			{
				if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.DoubleClick)
				{
					if (ToggleLinked && thisRow.Toggled)
					{
						ShowNumberAdjusterControls = true;
						ShowNumberAdjuster?.Invoke(ShowNumberAdjusterControls);
					}
					else if (!ToggleLinked)
					{
						ShowNumberAdjusterControls = true;
						ShowNumberAdjuster?.Invoke(ShowNumberAdjusterControls);
					}

				}
			}
		}

		if (!IsMouseInCell() && CellOptions == CellOptions.Picture && thumbnailvisible)
		{
			KillThumbnail();
		}

		if (!IsMouseInCell() && CellOptions == CellOptions.AdjustableNumber && ShowNumberAdjusterControls)
		{
			if (@event is InputEventMouseButton mouseEvent)
			{
				if (mouseEvent.ButtonIndex == MouseButton.Left)
				{
					if (int.Parse(NumberHolder.Text) != NumberContent)
					{
						if (!dataGrid.IsThereMultipleAdjustmentNumbers())
						{
							NumberContent = int.Parse(NumberHolder.Text);
							NumAdjusted(int.Parse(NumberHolder.Text), true);
						}
						else
						{
							//NumberContent = int.Parse(NumberHolder.Text);
							NumAdjusted(int.Parse(NumberHolder.Text), false);
						}
						
					}
					ShowNumberAdjusterControls = false;
					ShowNumberAdjuster?.Invoke(ShowNumberAdjusterControls);
				}
			}
		}

		
	}

	private void SpawnThumbnail()
	{
		floatingThumbnail = HoverThumbnail.Instantiate() as DataGridFloatingThumbnail;
		floatingThumbnail.ThumbnailImage = ThumbnailImage;
		floatingThumbnail.ThumbnailHolder.Position = GetGlobalMousePosition();
		dataGrid.AddChild(floatingThumbnail);
		Rect2 rect = new(floatingThumbnail.ThumbnailHolder.GlobalPosition, floatingThumbnail.ThumbnailHolder.Size);
		Rect2 dgrect = new(dataGrid.GlobalPosition, dataGrid.Size);
		if (!dgrect.Encloses(rect))
		{
			Vector2 currpos = floatingThumbnail.ThumbnailHolder.Position;
			Vector2 rectend = rect.End;
			Vector2 mainend = dgrect.End;
			float y = currpos.Y;
			float x = currpos.X;
			Rect2 intersect = dgrect.Intersection(rect);
			if (rectend.Y > mainend.Y)
			{
				float yy = rectend.Y - mainend.Y;
				y = currpos.Y - yy;
			}
			if (rectend.X > mainend.X)
			{
				float xx = rectend.X - mainend.X;
				x = currpos.X - xx;
			}
			floatingThumbnail.ThumbnailHolder.Position = new(x, y);
		}
	}

	public void KillThumbnail()
	{
		if (thumbnailvisible)
		{
			floatingThumbnail?.QueueFree();
			thumbnailvisible = false;
		}
	}


	private void MakeEditable()
	{
		TextHolder.Editable = true;
	}

	private bool IsMouseInCell()
	{
		Vector2 mousepos = GetGlobalMousePosition();

		Rect2 rect = new(this.GlobalPosition, this.Size);

		return rect.HasPoint(mousepos);
	}
	

	static readonly string[] SizeSuffixes = 
                   { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
	static string SizeSuffix(Int64 value, int decimalPlaces = 1)
	{
		//From https://stackoverflow.com/a/14488941 
		if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
		if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); } 
		if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

		// mag is 0 for bytes, 1 for KB, 2, for MB, etc.
		int mag = (int)Math.Log(value, 1024);

		// 1L << (mag * 10) == 2 ^ (10 * mag) 
		// [i.e. the number of bytes in the unit corresponding to mag]
		decimal adjustedSize = (decimal)value / (1L << (mag * 10));

		// make adjustment when the value is large enough that
		// it would round up to 1000 or more
		if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
		{
			mag += 1;
			adjustedSize /= 1024;
		}

		return string.Format("{0:n" + decimalPlaces + "} {1}", 
			adjustedSize, 
			SizeSuffixes[mag]);
        }


}
