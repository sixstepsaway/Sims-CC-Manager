using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;

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

    [Export]
    Control MultipleItemsPlaceholder;
    [Export]
    Label MultipleItemsLabel;
    [Export]
    MarginContainer PlaceForItem;

    VScrollBar Vscroll;

    public bool IsDragging = false;

    bool LeftHeld = false;

    List<CurrentLois> currentLoadOrderItems = new();
    Control currentLoi;

    public List<LoadOrderItem> LoadOrderItems = new();

    List<LoadOrderItem> OriginalLoadOrder = new();

    Godot.Timer timer;
    public PackageDisplay packageDisplay;

    public delegate void CloseEditLoadOrderEvent(bool Save);
    public CloseEditLoadOrderEvent CloseEditLoadOrder;

    private  List<SimsPackage> _packagelist;
    public  List<SimsPackage> PackageList 
    {
        get { return  _packagelist; }
        set {  _packagelist = value; 
        MakeList();}
    }

    public override void _Ready()
    {
        Vscroll = Scrollcontainer.GetVScrollBar();
        //TestRun();
        timer = new()
        {
            WaitTime = 0.2,
            Autostart = false,
            OneShot = false
        };
        timer.Timeout += () => CheckIfMouseHold();
        AddChild(timer);

        ResetButton.Pressed += () =>
        {
            foreach (LoadOrderItem item in LoadOrderItems)
            {
                ItemsBox.RemoveChild(item);
            }  
            int i = 0;
            LoadOrderItems.Clear();
            foreach (LoadOrderItem item in OriginalLoadOrder)
            {
                item.LoadOrder = i;
                ItemsBox.AddChild(item);
                LoadOrderItems.Add(item);
                i++;
            }
        };

        CloseButton.Pressed += () =>
        {
            CloseEditLoadOrder?.Invoke(false);
        };
        SaveButton.Pressed += () =>
        {
            CloseEditLoadOrder?.Invoke(true);
        };

    }

    private void MakeList()
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Listing {0} packages.", PackageList.Count));
        foreach (SimsPackage package in PackageList.OrderBy(x => x.LoadOrder))
        {
            MakeLoadOrderItem(package);
        }
    }

    bool ShiftHeld = false;
    bool ControlHeld = false;

    private void MakeLoadOrderItem(SimsPackage package)
    {
        LoadOrderItem loi = LoadOrderItemPS.Instantiate() as LoadOrderItem;
        loi.LoadOrder = package.LoadOrder;
        loi.Identifier = package.Identifier;
        loi.NameText = package.FileName;
        loi.button.Pressed += () =>
        {
            ItemSelectionChanged(loi.IsSelected, loi);
        };
        loi.LoadOrderItemSelected += (s) => ItemSelectionChanged(s, loi);
        LoadOrderItems.Add(loi);
        OriginalLoadOrder.Add(loi);
        ItemsBox.AddChild(loi);
    }

    

    LoadOrderItem PreviousItemSelection;

    bool UsingMultiMove = false;

    private void ItemSelectionChanged(bool s, LoadOrderItem loi)
    {
        if (!IsDragging)
        {
            if (!ShiftHeld && !ControlHeld)
            {
                foreach (LoadOrderItem l in LoadOrderItems)
                {
                    if (l != loi) l.IsSelected = false;
                }
                loi.IsSelected = true;            
            } else if (ShiftHeld)
            {
                if (PreviousItemSelection != null)
                {
                    if (LoadOrderItems.Any(x => x.IsSelected))
                    {                       
                        //GD.Print(string.Format("There are {0} items selected.", LoadOrderItems.Count(x => x.IsSelected)));
                        int SelectedItem = LoadOrderItems.IndexOf(loi);
                        if (LoadOrderItems.Count(x => x.IsSelected) > 1)
                        {
                            int LastItem = LoadOrderItems.IndexOf(LoadOrderItems.Last(x => x.IsSelected));
                            int FirstItem = LoadOrderItems.IndexOf(LoadOrderItems.First(x => x.IsSelected));
                            int PreviousSelection = LoadOrderItems.IndexOf(PreviousItemSelection);
                            
                            
                            //GD.Print(string.Format("LastItem: {0}, FirstItem: {1}, Previous Item: {2}, This Item: {3}", LastItem, FirstItem, PreviousSelection, SelectedItem));

                            if (PreviousSelection == LastItem){
                                if (SelectedItem < FirstItem) { 
                                    ChangeSelection(SelectedItem, FirstItem, true);
                                } else if (SelectedItem > LastItem){
                                    ChangeSelection(FirstItem, SelectedItem, true);
                                } else if (SelectedItem < LastItem && SelectedItem > FirstItem) { 
                                    ChangeSelection(FirstItem, SelectedItem, true);
                                }  else if (SelectedItem < LastItem) {
                                    ChangeSelection(SelectedItem, LastItem, true);
                                }
                            } else if (PreviousSelection == FirstItem){
                                if (SelectedItem < FirstItem) {
                                    ChangeSelection(SelectedItem, FirstItem, true);
                                } else if (SelectedItem > FirstItem && SelectedItem < LastItem) {
                                    ChangeSelection(FirstItem, SelectedItem, true);
                                } else if (SelectedItem > LastItem){
                                    ChangeSelection(FirstItem, SelectedItem, true);
                                } 
                            }
                        } else
                        {
                            int SingleSelected = LoadOrderItems.IndexOf(LoadOrderItems.Last(x => x.IsSelected));
                            if (SingleSelected > SelectedItem){
                                ChangeSelection(SelectedItem, SingleSelected, true);
                            } else {
                                ChangeSelection(SingleSelected, SelectedItem, true);
                            }
                        }           
                        
                    } 
                }
            } else
            {
                loi.IsSelected = !loi.IsSelected;
            }
            PreviousItemSelection = loi;
        }
        
    }

    private void ChangeSelection(int FirstItem, int LastItem, bool Selected)
    {
        for (int i = 0; i < LoadOrderItems.Count; i++){
			if(i <= LastItem && i >= FirstItem){
				LoadOrderItems[i].IsSelected = Selected;												
			} else {
				LoadOrderItems[i].IsSelected = false;
			}		
		}
    }


    private void MoveItem(List<LoadOrderItem> items)
    {
        if (!IsDragging)
        {
            IsDragging = true;
            currentLoadOrderItems.Clear();
            FloaterVbox.Size = ItemsBox.Size;
            Floater.Size = new(ItemsBox.Size.X, 25);
            MultipleItemsPlaceholder.Size = ItemsBox.Size;
            if (items.Count > 3)
            {
                UsingMultiMove = true;
                MultipleItemsPlaceholder.Visible = true;
                MultipleItemsLabel.Text = string.Format("... +{0} other items.", items.Count - 1);
                
                foreach (LoadOrderItem item in items)
                {
                    CurrentLois loi = new();
                    loi.OriginalLoi = item;
                    loi.FloatingLoi = item.Item.Duplicate() as Control;
                    if (items.Count > 1 && item != items.First()) { 
                        loi.OriginalLoi.Visible = false;
                        loi.FloatingLoi.Visible = false;       
                    }
                    if (item == items.First())                  
                        PlaceForItem.AddChild(loi.FloatingLoi);
                    loi.OriginalLoi.IsMoving = true;
                    loi.FloatingLoi.Size = Floater.Size;
                    loi.FloatingLoi.Position = new(0, 0);
                    
                    currentLoadOrderItems.Add(loi);
                }
            } else
            {
                foreach (LoadOrderItem item in items)
                {
                    if (items.Count > 1 && item != items.First()) item.Visible = false;
                    CurrentLois loi = new();
                    loi.OriginalLoi = item;
                    loi.FloatingLoi = item.Item.Duplicate() as Control;
                    loi.OriginalLoi.IsMoving = true;

                    loi.FloatingLoi.Size = Floater.Size;
                    loi.FloatingLoi.Position = new(0, 0);
                    
                    currentLoadOrderItems.Add(loi);
                    FloaterVbox.AddChild(loi.FloatingLoi);
                }
            }
            
        }
    }    

    private void ResetItem()
    {
        IsDragging = false;
        MultipleItemsPlaceholder.Visible = false;
        foreach (CurrentLois item in currentLoadOrderItems)
        {
            if (IsInstanceValid(item.FloatingLoi)) {
                FloaterVbox.RemoveChild(item.FloatingLoi);
                item.FloatingLoi.QueueFree();
                item.OriginalLoi.IsMoving = false;
                item.OriginalLoi.Visible = true; 
            }
        }
        LoadOrderItems.Clear();
        int i = 0; 
        foreach (LoadOrderItem item in ItemsBox.GetChildren())
        {
            if (SelectedItems.Any(x => x.Identifier == item.Identifier)) item.IsSelected = true;
            LoadOrderItems.Add(item);
            item.LoadOrder = i;
            i++;
        }
        currentLoadOrderItems.Clear();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton button)
        {
            if (button.ButtonIndex == MouseButton.Left && button.Pressed)
            {
                LeftHeld = true;
                timer.Start();                
                
            }
            if (button.ButtonIndex == MouseButton.Left && button.IsReleased())
            {
                LeftHeld = false;
                if (IsDragging) {
                    ResetItem();
                    IsDragging = false;
                }
            }
        }
        if (@event is InputEventKey key)
        {
            if (key.Keycode == Key.Ctrl && key.Pressed)
            {
                ControlHeld = true;
            } else if (key.Keycode == Key.Ctrl && key.IsReleased())
            {
                ControlHeld = false;
            }
            if (key.Keycode == Key.Shift && key.Pressed)
            {
                ShiftHeld = true;
            } else if (key.Keycode == Key.Shift && key.IsReleased())
            {
                ShiftHeld = false;
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
                foreach (CurrentLois loi in currentLoadOrderItems)
                {
                    ItemsBox.MoveChild(loi.OriginalLoi, CurrentSlot);
                    CurrentSlot++;
                }
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
        /*timer = new();      
        AddChild(timer);
        timer.Start(0.1f);
        timer.Timeout += () => CheckIfMouseHold(item);*/
    }

    private void CheckIfMouseHold()
    {
        if (LeftHeld && LoadOrderItems.Any(x => x.IsSelected)) { 
            SelectedItems = LoadOrderItems.Where(x => x.IsSelected).ToList();
            MoveItem(SelectedItems);
        }
    }

    List<LoadOrderItem> SelectedItems;


}

public class LoiDistances
{
    public LoadOrderItem Item {get; set;}
    public float Distance {get; set;}
}

public class CurrentLois
{
    public LoadOrderItem OriginalLoi {get; set;}
    public Control FloatingLoi {get; set;}
}
