using Godot;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using System;

public partial class LoadOrderItem : Control
{
    public Guid Identifier;
    [Export]
    public MarginContainer Item;
    [Export]
    public MarginContainer ItemContents;
    [Export] 
    Label ItemNameLabel;
    [Export]
    Label LoadOrderLabel;
    [Export]
    Panel NormalPanel;
    [Export]
    Panel SelectedPanel;
    [Export]
    public Button button;



    private bool _ismoving;
    public bool IsMoving
    {
        get { return _ismoving; }
        set { _ismoving = value; 
        ItemContents.Visible = !IsMoving;}
    }

    private string _nametext;
    public string NameText
    {
        get { return _nametext; }
        set { _nametext = value; 
            ItemNameLabel.Text = value; }
    }
    private int _loadorder;
    public int LoadOrder
    {
        get { return _loadorder; }
        set { _loadorder = value; 
            LoadOrderLabel.Text = value.ToString(); }
    }
    
    private bool _inbox;
    public bool InBox
    {
        get { return _inbox; }
        set { _inbox = value; }
    }

    public bool IsInBox(Vector2 pos)
    {
        Rect2 thisbox = new(Item.GlobalPosition, Item.Size);
        return thisbox.HasPoint(pos);
    }

    private bool _isselected;
    public bool IsSelected
        {
            get { return _isselected; }
            set { _isselected = value;     
                NormalPanel.Visible = !value;
                SelectedPanel.Visible = value;
        }
    }

    public delegate void LoadOrderItemSelectedEvent(bool Selected);
    public LoadOrderItemSelectedEvent LoadOrderItemSelected;

    public override void _Ready()
    {
        button.MouseEntered += () => { InBox = true; };
        button.MouseExited += () => { InBox = false; }; 

        /*button.Pressed += () => { 
            //GD.Print("Pressed!");
            IsSelected = !IsSelected; 
            LoadOrderItemSelected?.Invoke(IsSelected);
            GD.Print(string.Format("Item {0} is selected.", GetIndex()));
        };*/
        
        IsSelected = false;
        //Floater.GlobalPosition = this.GlobalPosition;

        //xCenter = (Item.GlobalPosition.X + Item.Size.X) - Item.GlobalPosition.X;
    }
    
    /*float xCenter = 0;
    Vector2 itempos = new();

    public override void _Input(InputEvent @event)
    {        
        
    }*/



    //public delegate void ItemMovingEvent(Vector2 pos);
    //public ItemMovingEvent ItemMoving;

}
