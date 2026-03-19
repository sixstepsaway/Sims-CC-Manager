using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

public partial class EditLoadOrder : MarginContainer
{
    [Export]
    PackedScene LoadOrderItemPS;
    [Export]
    VBoxContainer ItemsBox;
    [Export]
    Button ResetButton;
    [Export]
    Button SaveButton;
    [Export]
    Button CloseButton;
    [Export]
    Label Header;
    [Export]
    Label Subheading;
    
    [Export]
    Panel SecondBackground;
    [Export]
    Panel InternalBackground;

    [Export]
    Control Floater;
    [Export]
    VBoxContainer FloaterVbox;
    [Export]
    ScrollContainer Scrollcontainer;

    VScrollBar Vscroll;

    public bool IsDragging = false;

    bool LeftHeld = false;

    LoadOrderItem currentLoadOrderItem;
    Control currentLoi;

    List<LoadOrderItem> LoadOrderItems = new();

    Timer timer;
    public PackageDisplay packageDisplay;

    public override void _Ready()
    {
        Vscroll = Scrollcontainer.GetVScrollBar();
        TestRun();

    }

    private void TestRun()
    {
        for (int i = 0; i < 75; i++)
        {
            LoadOrderItem loi = LoadOrderItemPS.Instantiate() as LoadOrderItem;
            loi.LoadOrder = i;
            loi.NameText = Guid.NewGuid().ToString();
            loi.button.Pressed += () => TimeMouseHold(loi);
            LoadOrderItems.Add(loi);
            ItemsBox.AddChild(loi);
        }
    }

    private void MoveItem(LoadOrderItem item)
    {
        if (!IsDragging)
        {
            currentLoadOrderItem = item;
            currentLoi = item.Item.Duplicate() as Control;
            currentLoadOrderItem.IsMoving = true;
            FloaterVbox.AddChild(currentLoi);
            FloaterVbox.Size = ItemsBox.Size;
            Floater.Size = new(ItemsBox.Size.X, 25);
            currentLoi.Size = Floater.Size;
            currentLoi.Position = new(0, 0);
            IsDragging = true;
        }
    }

    private void ResetItem()
    {
        if (IsInstanceValid(currentLoi)) {
            FloaterVbox.RemoveChild(currentLoi);
            currentLoi.QueueFree();
            currentLoadOrderItem.IsMoving = false;
            IsDragging = false;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton button)
        {
            if (button.ButtonIndex == MouseButton.Left)
            {
                if (!LeftHeld) LeftHeld = true; 
            }
            if (button.IsReleased())
            {
                LeftHeld = false;
                if (IsDragging) {
                    ResetItem();
                }
            }
        }
        if (@event is InputEventMouseMotion motion)
        {
            if (IsDragging) {
                Floater.GlobalPosition = new(Floater.GlobalPosition.X, motion.GlobalPosition.Y);
                List<LoiDistances> DistancesTo = new();
                foreach (LoadOrderItem item in LoadOrderItems){
                    DistancesTo.Add(new() { Distance = Floater.GlobalPosition.DistanceTo(item.GlobalPosition), Item = item});
                }
                DistancesTo = DistancesTo.OrderBy(x => x.Distance).ToList();
                int CurrentSlot = DistancesTo.First().Item.GetIndex();
                ItemsBox.MoveChild(currentLoadOrderItem, CurrentSlot);  

                float bottom = ItemsBox.GlobalPosition.Y + ItemsBox.Size.Y;
                if (motion.GlobalPosition.Y >= bottom)
                {
                    Vscroll.Value++;
                }
            }
        }
    }

    private void TimeMouseHold(LoadOrderItem item)
    {
        timer = new();      
        AddChild(timer);
        timer.Start(0.1f);
        timer.Timeout += () => CheckIfMouseHold(item);
    }

    private void CheckIfMouseHold(LoadOrderItem item)
    {
        if (LeftHeld) IsDragging = true;
        if (IsInstanceValid(timer)) {
            RemoveChild(timer);
            timer.QueueFree();
        }
        MoveItem(item);
    }


}

public class LoiDistances
{
    public LoadOrderItem Item {get; set;}
    public float Distance {get; set;}
}
