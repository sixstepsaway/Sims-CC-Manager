using Godot;
using SimsCCManager.Containers;
using System;

public partial class ThumbnailGridItem : MarginContainer
{
    public SimsPackage PackageReference;
    [Export]
    public MarginContainer ThumbnailSubviewerVersion;
    [Export]
    public MarginContainer ThumbnailImageVersion;
    [Export]
    public MarginContainer SelectedBox;
    [Export]
    public MarginContainer HoveredBox;
    [Export]
    public Label ItemName;
    [Export]
    ColorRect BorderColorRect;
    [Export]
    ColorRect BGColorRect;
    [Export]
    ColorRect SelectedColorRect;
    [Export]
    ColorRect HoveredColorRect;

    [Export]
    MarginContainer ImageContainer;
    [Export]
    MarginContainer SubviewportContainer;
    [Export]
    Texture2D Sims2Image;
    [Export]
    Texture2D Sims3Image;
    [Export]
    Texture2D Sims4Image;
    [Export]
    public SubViewport subViewport;
    [Export]
    public TextureRect ThumbnailImage;

    [Export]
    Button button;

    private int _imageplaceholder;
    public int ImagePlaceholder
    {
        get { return _imageplaceholder; }
        set { _imageplaceholder = value;
        switch (value)
            {
                case 0:
                ThumbnailImage.Texture = ImageTexture;
                break;
                case 1: 
                ThumbnailImage.Texture = ImageTexture;
                break;
                case 2:
                ThumbnailImage.Texture = Sims2Image;
                break;
                case 3:
                ThumbnailImage.Texture = Sims3Image;
                break;
                case 4:
                ThumbnailImage.Texture = Sims4Image;
                break;
            }
        }
    }

    public Texture2D ImageTexture;

    private string _labeltext;
    public string LabelText
    {
        get { return _labeltext; }
        set { _labeltext = value; 
        ItemName.Text = value; }
    }

    private Color _selectedcolor;
    public Color SelectedColor 
    { get { return _selectedcolor; }
    set { _selectedcolor = value; 
    SelectedColorRect.Color = value; }}

    private Color _accentcolor;
    public Color AccentColor 
    { get { return _accentcolor; }
    set { _accentcolor = value; 
    BorderColorRect.Color = value; 
    HoveredColorRect.Color = value;}}

    private Color _textcolor;
    public Color TextColor 
    { get { return _textcolor; }
    set { _textcolor = value; 
    ItemName.AddThemeColorOverride("font_color", value); }}

    private Color _backgroundcolor;
    public Color BackgroundColor 
    { get { return _backgroundcolor; }
    set { _backgroundcolor = value; 
    BGColorRect.Color = value; }}

    public delegate void ItemSelectedEvent(bool Selected, SimsPackage package, ThumbnailGridItem item);
    public ItemSelectedEvent ItemSelected;

    private bool _isselected;
    public bool IsSelected { get { return _isselected; } set { _isselected = value; SelectedBox.Visible = value; ItemSelected?.Invoke(value, PackageReference, this); }}

    private bool _ishovered;
    public bool IsHovered { get { return _ishovered; } set { _ishovered = value; HoveredBox.Visible = value;}}


    private bool _viewportversion;
    public bool ViewportVersion { get { return _viewportversion; } set { _viewportversion = value; SubviewportContainer.Visible = value; ImageContainer.Visible = !value;}}

    public override void _Ready()
    {
        button.Pressed += () => ItemPressed();
        button.MouseEntered += () => ItemHovered(true);
        button.MouseExited += () => ItemHovered(false);
    }

    private void ItemHovered(bool v)
    {
        IsHovered = v;
    }

    private void ItemPressed()
    {
        IsSelected = !IsSelected;
    }

}
