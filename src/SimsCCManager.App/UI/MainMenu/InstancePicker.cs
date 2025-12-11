using Godot;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using System;

public partial class InstancePicker : MarginContainer
{
    public Guid InstanceIdentifier;
    [Export]
    public TextureRect GameIcon;
    [Export]
    public Label GameLabel;
    [Export]
    public Button ButtonClicker;
    [Export]
    public Texture2D Sims2Icon;
    [Export]
    public Texture2D Sims3Icon;
    [Export]
    public Texture2D Sims4Icon;
    [Export]
    public Texture2D Sims1Icon;
    [Export]
    public Texture2D SporeIcon;
    [Export]
    public Texture2D SimCity4Icon;
    [Export]
    public Texture2D SimsMedievalIcon;
    [Export]
    MarginContainer SelectedHighlight;
    [Export]
    public ColorRect SelectionColor;


    [Export]
    MarginContainer DeleteInstanceButton_Container;

    [Export]
    Button DeleteInstanceButton;
    [Export]
    ColorRect DeleteInstanceButton_ColorMain;
    [Export]
    ColorRect DeleteInstanceButton_ColorHover;
    [Export]
    ColorRect DeleteInstanceButton_ColorClick;







    private bool _isselected;
    public bool isSelected
    {
        get { return _isselected; }
        set { _isselected = value;         
        SelectedHighlight.Visible = isSelected;}
    }

    public delegate void PickedInstanceEvent(Guid identifier, bool selected);
    public PickedInstanceEvent PickedInstance;
    public delegate void DeleteInstanceEvent(Guid identifier);
    public DeleteInstanceEvent DeleteInstance;
    bool Hovering = false;
    public DateTime DateCreated;
    public DateTime DateModified;

    public string HoverMessage
    {
        get {return string.Format("Created {0}\nLast Changed: {1}", DateCreated.ToShortDateString(), DateModified.ToShortDateString()); }
    }

    public override void _Ready()
    {
        ButtonClicker.Pressed += ButtonClicked;
        ButtonClicker.MouseEntered += () => HoveringBox(true);
        ButtonClicker.MouseExited += () => HoveringBox(false);
        DeleteInstanceButton.Pressed += DeleteThisInstance;
        DeleteInstanceButton.MouseEntered += () => HoveringDIButton(true);
        DeleteInstanceButton.MouseExited += () => HoveringDIButton(false);
    }

    private void HoveringDIButton(bool hover)
    {
        DeleteInstanceButton_ColorHover.Visible = hover;
        DeleteInstanceButton_ColorMain.Visible = !hover;        
    }


    private void DeleteThisInstance()
    {
        DeleteInstanceButton_ColorClick.Visible = true;
        DeleteInstance.Invoke(InstanceIdentifier);
    }


    private void HoveringBox(bool hover)
    {
        if (hover)
        {
            Hovering = true;
            GlobalVariables.mainWindow.InstantiateTooltip(HoverMessage);
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Button {0} is being hovered.", GameLabel.Text));
        } else
        {
            //if (!IsMouseInItem())
            //{
                Hovering = false;
                GlobalVariables.mainWindow.CancelTooltip();
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Button {0} is no longer being hovered.", GameLabel.Text));
            //}            
        }
    }

    private bool IsMouseInItem()
    {
        Vector2 mousepos = GetGlobalMousePosition(); 
		Rect2 rect = new (this.GlobalPosition, this.Size); 
		return rect.HasPoint(mousepos); 
    }


    private void ButtonClicked()
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Button {0} selected: {1}", GameLabel.Text, isSelected));
        isSelected = !isSelected;
        PickedInstance.Invoke(InstanceIdentifier, isSelected);        
    }

    public void SwapSelection(bool selected)
    {
        isSelected = selected;
        SelectedHighlight.Visible = isSelected;
    }





}
