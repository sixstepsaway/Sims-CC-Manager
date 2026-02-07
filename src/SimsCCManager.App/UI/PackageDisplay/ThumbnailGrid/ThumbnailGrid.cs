using Godot;
using MoreLinq;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.OptionLists;
using SimsCCManager.PackageReaders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public partial class ThumbnailGrid : MarginContainer
{
    public PackageDisplay packageDisplay;
    public Vector2 PaneSize = new(0, 0);
    public MarginContainer PaneSizer;
    [Export]
    GridContainer ThumbGrid;
    [Export]
    ColorRect BGColorRect;
    [Export]
    PackedScene ThumbnailGridItemPS;
    [Export]
    PackedScene SnapshotterPS;
    [Export]
    MarginContainer GridHolder;

    private Color _selectedcolor;
    public Color SelectedColor 
    { get { return _selectedcolor; }
    set { _selectedcolor = value; }}

    private Color _accentcolor;
    public Color AccentColor 
    { get { return _accentcolor; }
    set { _accentcolor = value; 
    ScrollUpAccent.Color = value;
    ScrollDownAccent.Color = value;}}

    private Color _textcolor;
    public Color TextColor 
    { get { return _textcolor; }
    set { _textcolor = value; }}

    private Color _backgroundcolor;
    public Color BackgroundColor 
    { get { return _backgroundcolor; }
    set { _backgroundcolor = value; 
    BGColorRect.Color = value; 
    ScrollBackground.Color = value;}}
    
    public Color ItemBackgroundColor;

    Vector2 ItemSize = new(100,120);
    Vector2 OriginalItemSize = new(100,120);

    bool ShiftHeld = false;
    bool ControlHeld = false;
    int PreviousSelection = -1;

    int RowCount = 5;
    int ColumnCount = 5;
    int ItemCount { get { return RowCount * ColumnCount; } }    

    bool IsSorted = false;

    public delegate void ThumbGridItemsSelectedEvent(List<SimsPackage> items);
    public ThumbGridItemsSelectedEvent ThumbGridItemsSelected;

    [Export]
    VScrollBar vScrollBar;
    [Export]
    Button ScrollUpButton;
    [Export]
    Button ScrollDownButton;
    [Export]
    ColorRect ScrollUpAccent;
    [Export]
    ColorRect ScrollDownAccent;
    [Export]
    ColorRect ScrollBackground;



    public ConcurrentQueue<Task> ScrollerTasks = new();

    bool ListenForKeyPress = true;
    bool ScrollUpHeld = false;
    bool ScrollDownHeld = false;
    bool ScrollReleased = true;
    bool ScrollChecker = false;

    Godot.Timer ScrollDownTimer;
    Godot.Timer KeyPressTimer;

    bool FirstLoaded = false;

    int MaxScroll = 0;

    int ScrollPosition = 0; 

    List<ThumbnailGridItem> VisibleTGIs = new();
    private ThumbnailGridItem[] _visibletgis
	{
		get { return new ThumbnailGridItem[ItemCount]; }
	}
    List<ThumbnailGridItem> AllTGIs = new();
    
    private ThumbnailGridItem[] _alltgis
	{
		get { return new ThumbnailGridItem[packages.Count]; }
	}

    List<SimsPackage> packages = new();    
    List<GridItem> GridItems = new();


    public void MakeItems()
    {
        GetSettings();
        MakeGridItems();  
        MakeVisibleItems();
    }

    private void GetSettings(bool fromResized = false)
    {
        int newCount = ItemCount;
        if (Resizing) return;
        if (fromResized && PaneSize == new Vector2(GridHolder.Size.X - 15, GridHolder.Size.Y)) return;
        if (fromResized) { PaneSize = new Vector2(GridHolder.Size.X - 15, GridHolder.Size.Y); Resizing = true; }
        if (!FirstLoaded) { 
            packages = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList();
            PaneSizer.Resized += () => GetSettings(true); 
            ScrollUpButton.ButtonDown += () => OnScrollUp_ButtonUsed(true);
		    ScrollDownButton.ButtonDown += () => OnScrollDown_ButtonUsed(true);
		    ScrollUpButton.ButtonUp += () => OnScrollUp_ButtonUsed(false);
		    ScrollDownButton.ButtonUp += () => OnScrollDown_ButtonUsed(false);

		    vScrollBar.ValueChanged += OnScrollScrolled; 
            UpdateTheme();      
        }

        float dividedBy100 = PaneSize.X / 100f;
        int items = (int)Math.Ceiling(dividedBy100);

        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0} items will fit in the grid.", items));

        float itemWidth = PaneSize.X / items;
        float scale = 0;

        scale = itemWidth / ItemSize.X;

        ItemSize = new(ItemSize.X * scale, ItemSize.Y * scale);

        if (VisibleTGIs.Count > 0)
        {
            foreach (ThumbnailGridItem tgi in VisibleTGIs)
            {
                tgi.CustomMinimumSize = ItemSize;
            }
        }     

        ThumbGrid.Columns = items; 
        ColumnCount = ThumbGrid.Columns; // items per row
        RowCount = (int)Math.Ceiling(PaneSize.Y / ItemSize.Y); //rows per screen



        MaxScroll = (packages.Count / ColumnCount); // max rows
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Max Scroll: {0}", MaxScroll));
        int pages = (MaxScroll / RowCount) * 2;
        int minpage = RowCount;
        int pageunclamped = minpage - pages;
        double page = Math.Clamp(pageunclamped, 1, minpage);
        vScrollBar.Step = 1;
        vScrollBar.MaxValue = MaxScroll;
        vScrollBar.MinValue = 0;
        vScrollBar.Page = minpage;


        if (!FirstLoaded) FirstLoaded = true; 
        if (fromResized)
        {                        
            Resizing = false;
            CheckSize();
            if (ItemCount != newCount)
            {
                RemoveItems();
                MakeVisibleItems();
            }
        }
        newCount = ItemCount;
    }

    bool Resizing = false;


    private void CheckSize()
    {
        if (PaneSize != Size) GetSettings();
    }

    private void UpdateTheme()
    {
        bool selectedislight = false;
		bool bgislight = false;
		bool accentislight = false;
		if (AccentColor.Luminance < 0.5) accentislight = true;
		if (SelectedColor.Luminance < 0.5) selectedislight = true;
		if (BackgroundColor.Luminance < 0.5) bgislight = true;

		StyleBoxFlat scrollstyle = vScrollBar.GetThemeStylebox("scroll") as StyleBoxFlat;
		scrollstyle.BgColor = BackgroundColor;
		vScrollBar.AddThemeStyleboxOverride("scroll", scrollstyle);
		vScrollBar.AddThemeStyleboxOverride("scroll_focus", scrollstyle);		
		
		
		StyleBoxFlat grabberV = vScrollBar.GetThemeStylebox("grabber") as StyleBoxFlat;
		Color grabbercolor = SelectedColor;
		if (SelectedColor.V > 0.5){
			grabbercolor = grabbercolor.Darkened(0.2f);
		} else {
			grabbercolor = grabbercolor.Lightened(0.2f);
		}
		grabberV.BgColor = grabbercolor;

		vScrollBar.AddThemeStyleboxOverride("grabber", grabberV);

		if (SelectedColor.V > 0.5){
			grabbercolor = grabbercolor.Darkened(0.1f);
		} else {
			grabbercolor = grabbercolor.Lightened(0.1f);
		}
		StyleBoxFlat grabberpressedV = vScrollBar.GetThemeStylebox("grabber_pressed") as StyleBoxFlat;
		grabberpressedV.BgColor = grabbercolor;
		
		vScrollBar.AddThemeStyleboxOverride("grabber_pressed", grabberpressedV);
		
		StyleBoxFlat grabberhighlightV = vScrollBar.GetThemeStylebox("grabber_highlight") as StyleBoxFlat;
		
		if (grabbercolor.V > 0.75){
			grabbercolor = grabbercolor.Darkened(0.7f);
		} else {
			grabbercolor = grabbercolor.Lightened(0.3f);
		}
		grabberhighlightV.BgColor = grabbercolor;
		
		vScrollBar.AddThemeStyleboxOverride("grabber_highlight", grabberhighlightV);
		

		StyleBoxFlat scrollbuttonsnormal = ScrollUpButton.GetThemeStylebox("normal") as StyleBoxFlat;
		scrollbuttonsnormal.BgColor = SelectedColor;
		Color bordercol = SelectedColor;
		if (SelectedColor.Luminance > 0.5){
			bordercol = bordercol.Darkened(0.15f);
		} else {
			bordercol = bordercol.Lightened(0.15f);
		}		
		scrollbuttonsnormal.BorderColor = bordercol;
		ScrollUpButton.AddThemeStyleboxOverride("normal", scrollbuttonsnormal);		
		ScrollDownButton.AddThemeStyleboxOverride("normal", scrollbuttonsnormal);


		StyleBoxFlat scrollbuttonshover = ScrollUpButton.GetThemeStylebox("hover") as StyleBoxFlat;
		
		Color buttonhighlight = SelectedColor;		
		if (buttonhighlight.Luminance > 0.5) {
			buttonhighlight = buttonhighlight.Darkened(0.1f);
		} else {			
			buttonhighlight = buttonhighlight.Lightened(0.1f);
		}
		scrollbuttonshover.BgColor = buttonhighlight;
		Color highlightborder = buttonhighlight;
		if (buttonhighlight.Luminance > 0.5){
			highlightborder = highlightborder.Darkened(0.1f);
		} else {
			highlightborder = highlightborder.Lightened(0.1f);
		}
		scrollbuttonshover.BorderColor = highlightborder;
		ScrollUpButton.AddThemeStyleboxOverride("hover", scrollbuttonshover);		
		ScrollDownButton.AddThemeStyleboxOverride("hover", scrollbuttonshover);

		StyleBoxFlat scrollbuttonspressed = ScrollUpButton.GetThemeStylebox("pressed") as StyleBoxFlat;
		
		Color buttonpressed = SelectedColor;		
		if (buttonpressed.Luminance > 0.5) {
			buttonpressed = buttonpressed.Darkened(0.25f);
		} else {			
			buttonpressed = buttonpressed.Lightened(0.25f);
		}
		scrollbuttonspressed.BgColor = buttonpressed;
		Color pressedborder = buttonpressed;
		if (buttonpressed.Luminance > 0.5){
			pressedborder = pressedborder.Darkened(0.25f);
		} else {
			pressedborder = pressedborder.Lightened(0.25f);
		}
		scrollbuttonspressed.BorderColor = pressedborder;
		ScrollUpButton.AddThemeStyleboxOverride("pressed", scrollbuttonspressed);	
		ScrollDownButton.AddThemeStyleboxOverride("pressed", scrollbuttonspressed);

		StyleBoxFlat scrollbuttondisabled = ScrollUpButton.GetThemeStylebox("disabled") as StyleBoxFlat;
		
		Color buttonoff = SelectedColor;		
		if (buttonoff.Luminance > 0.5) {
			buttonoff = buttonoff.Darkened(0.25f);
			float s = buttonoff.S;
			s /= 4;
			buttonoff = Color.FromHsv(buttonoff.H, s, buttonoff.V);
		} else {			
			buttonoff = buttonoff.Lightened(0.25f);
			float s = buttonoff.S;
			s /= 4;
			buttonoff = Color.FromHsv(buttonoff.H, s, buttonoff.V);
		}
		scrollbuttondisabled.BgColor = buttonoff;
		Color disabledborder = buttonoff;
		if (buttonpressed.Luminance > 0.5){
			disabledborder = disabledborder.Darkened(0.25f);
			float s = disabledborder.S;
			s /= 4;
			disabledborder = Color.FromHsv(disabledborder.H, s, disabledborder.V);
		} else {
			disabledborder = disabledborder.Lightened(0.25f);
			float s = disabledborder.S;
			s /= 4;
			disabledborder = Color.FromHsv(disabledborder.H, s, disabledborder.V);
		}
		scrollbuttondisabled.BorderColor = disabledborder;
		ScrollUpButton.AddThemeStyleboxOverride("disabled", scrollbuttondisabled);		
		ScrollDownButton.AddThemeStyleboxOverride("disabled", scrollbuttondisabled);

		//StyleBoxEmpty focusedtheme = ScrollUpButton.GetThemeStylebox("focus") as StyleBoxEmpty;
		//ScrollUpButton.AddThemeStyleboxOverride("focus", focusedtheme);		
		//ScrollDownButton.AddThemeStyleboxOverride("focus", focusedtheme);
    }

    #region Scrolling


    private void OnScrollDown_ButtonUsed(bool Down){
		if (Down && ListenForKeyPress){
			ListenForKeyPress = false;
			ListenForKeyPressTimer();
			OnScrollDown();		
			ListenForScrollButtonDownTimer(true);
		} else {
			ReleaseScroll();			
		}
	}

	private void OnScrollUp_ButtonUsed(bool Down){
		if (Down && ListenForKeyPress){
			ListenForKeyPress = false;
			ListenForKeyPressTimer();
			OnScrollUp();
			ListenForScrollButtonDownTimer(false);
		} else {
			ReleaseScroll();
		}
	}

    private void OnScrollDown()
    {		
		double val = vScrollBar.Value;
		val++;
		if (val <= vScrollBar.MaxValue) { vScrollBar.Value++; }	
    }

    private void OnScrollUp()
    {
		double val = vScrollBar.Value;
		val--;
		if (val >= 0) vScrollBar.Value--;		
    }

    private void ListenForKeyPressTimer(){
		KeyPressTimer?.QueueFree();		
		KeyPressTimer = new();
		AddChild(KeyPressTimer);
		KeyPressTimer.OneShot = true;
		KeyPressTimer.WaitTime = 0.005;
		KeyPressTimer.Timeout += () => ResetKeyPress();
		KeyPressTimer.Start();
	}

    private void ResetKeyPress()
    {
        ListenForKeyPress = true;
    }

    private void ReleaseScroll()
    {
        ScrollUpHeld = false;
		ScrollDownHeld = false;
		ScrollReleased = true;
    }

    private void ListenForScrollButtonDownTimer(bool DownButton){
		if (ScrollDownTimer != null) { 
			ScrollDownTimer.QueueFree();			
		}
		ScrollDownTimer = new();
		AddChild(ScrollDownTimer);
		ScrollDownTimer.OneShot = true;
		ScrollDownTimer.WaitTime = 0.1;
		ScrollDownTimer.Timeout += () => ButtonDown(DownButton);
		ScrollDownTimer.Start();
	}

    private void ButtonDown(bool DownButton){
		if (!ScrollReleased){
			if (DownButton){
				ScrollDownHeld = true;
			} else {
				ScrollUpHeld = true;
			}
		} else {
			ScrollReleased = false;
		}
		
	}

    private void OnScrollScrolled(double val)
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Scroll moved! New position: {0}", val));
		new Thread(() => {		
			if (ScrollChecker) return;
			ScrollChecker = true;

			int currentValue = (int)val;
            if (currentValue == -1) { ScrollPosition = 0; return; }
            if (currentValue > MaxScroll) { ScrollPosition = (int)vScrollBar.MaxValue; return; }

			if (currentValue != ScrollPosition + 1 && currentValue != ScrollPosition - 1)
			{			
                ScrollPosition = currentValue;	
                ScrollRepopulateRows();
			} else
			{
				if (currentValue > ScrollPosition)
				{
					ScrollPosition = currentValue;
					OnScrollBarDown(currentValue);
				} else
				{
					ScrollPosition = currentValue;
					OnScrollBarUp(currentValue);
				}
			}
			ScrollChecker = false;
		}){IsBackground = true}.Start();		
    }

    private void ScrollRepopulateRows(){
		ScrollerTasks.Enqueue(new (() => { 
            RemoveItems();
			MakeVisibleItems();
		}));
	}

    private void RemoveItems()
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Removing all items."));
        if (VisibleTGIs.Count > 0)
        {
            foreach (ThumbnailGridItem tgi in VisibleTGIs)
            {
                ThumbGrid.RemoveChild(tgi);
			    tgi.QueueFree();
            }
        }
		VisibleTGIs.Clear();
    }

    private void OnScrollBarDown(int pos)
    {			
		ScrollerTasks.Enqueue(new (async () => { 
			await ScrollDownTask(pos);
		}));
    }

    private void OnScrollBarUp(int pos)
    {
		ScrollerTasks.Enqueue(new (async () => { 
			await ScrollUpTask(pos);
		}));
    }

	private Task ScrollUpTask(int scrollPos)
	{        
        //rows:
        int topRowStart = scrollPos * ColumnCount;
        int topRowEnd = topRowStart + ColumnCount;
        scrollPos++;
        int bottomRowEnd = (scrollPos * ColumnCount) + ItemCount;
        int bottomRowStart = bottomRowEnd - ColumnCount;
        
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("SCROLL UP: New items starts at {0} and ends at {1}, scrollpos {2}", topRowStart, topRowEnd, scrollPos));
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("SCROLL UP: Removal items starts at {0} and ends at {1}, scrollpos {2}", bottomRowStart, bottomRowEnd, scrollPos));
        int zeroidx = ItemCount - ColumnCount;
        for (int i = bottomRowStart; i < bottomRowEnd; i++)
        {
            ThumbGrid.RemoveChild(AllTGIs[i]);
            AllTGIs[i].QueueFree();
            //int idx = VisibleTGIs.IndexOf(AllTGIs[i]);
            VisibleTGIs.Remove(AllTGIs[i]);
            AllTGIs[i] = _alltgis[i];
            zeroidx++;
        }
        
        zeroidx = 0;
        for (int i = topRowStart; i < topRowEnd; i++)
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Adding item {0} to grid.", i));
            ThumbnailGridItem tgi = MakeTGIItem(GridItems[i]);
            VisibleTGIs.Insert(zeroidx, tgi);
            AllTGIs[i] = tgi;
            ThumbGrid.AddChild(tgi);
            ThumbGrid.MoveChild(tgi, zeroidx);            
            zeroidx++;
        }
			
		return Task.CompletedTask;
	}

	private Task ScrollDownTask(int scrollPos)
	{		
        scrollPos--;
        int topRowStart = scrollPos * ColumnCount;
        int topRowEnd = topRowStart + ColumnCount;
        scrollPos++;
        int bottomRowEnd = (scrollPos * ColumnCount) + ItemCount;
        int bottomRowStart = bottomRowEnd - ColumnCount;

        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("SCROLL DOWN: New items starts at {0} and ends at {1}, scrollpos {2}", bottomRowStart, bottomRowEnd, scrollPos));
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("SCROLL DOWN: Removal items starts at {0} and ends at {1}, scrollpos {2}", topRowStart, topRowEnd, scrollPos));
		int zeroidx = 0;
        for (int i = topRowStart; i < topRowEnd; i++)
        {
            ThumbGrid.RemoveChild(AllTGIs[i]);
            AllTGIs[i].QueueFree();
            //int idx = VisibleTGIs.IndexOf(AllTGIs[i]);
            VisibleTGIs.Remove(AllTGIs[i]);
            AllTGIs[i] = _alltgis[i];
            zeroidx++;
        }

        //int c = 0;
        zeroidx = 0;
        for (int i = bottomRowStart; i < bottomRowEnd; i++)
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Adding item {0} to grid.", i));
            ThumbnailGridItem tgi = MakeTGIItem(GridItems[i]);
            VisibleTGIs.Add(tgi);
            AllTGIs[i] = tgi;
            ThumbGrid.AddChild(tgi);
            //ThumbGrid.MoveChild(tgi, zeroidx);            
            zeroidx++;
        }
		return Task.CompletedTask;
	}

    
    private void RunScrollerTasks(){
    //if (!ScrollerTasksRunning){
    //	ScrollerTasksRunning = true;
        //for (int i = 0; i < ScrollerTasks.Count; i++){
            ScrollerTasks.TryDequeue(out Task task);
            task.Start(TaskScheduler.FromCurrentSynchronizationContext());
        //}
    //}
    //ScrollerTasksRunning = false;
    }

    #endregion


    private void MakeGridItems()
    {        
        //VisibleTGIs = [.._visibletgis];
        AllTGIs = [.._alltgis];
        int i = 0;
        foreach (SimsPackage package in packages)
        {
            GridItem gi = new();
            gi.Package = package;
            gi.OverallIdx = i;
            gi.GridIdx = i;
            gi.Identifier = package.Identifier;
            gi.IsOnScreen = false;
            gi.IsSelected = false;
            GridItems.Add(gi);
            i++;
        }
    }

    public void ReplaceTGI(SimsPackage package)
    {
        if (VisibleTGIs.Any(x => x.PackageReference.Identifier == package.Identifier))
        {
            GridItem item = GridItems.First(x => x.Package.Identifier == package.Identifier);
            ThumbnailGridItem tgi = VisibleTGIs.First(x => x.PackageReference.Identifier == package.Identifier);
            int vidx = VisibleTGIs.IndexOf(tgi);
            int idx = AllTGIs.IndexOf(AllTGIs.First(x => x.PackageReference.Identifier == package.Identifier));
            ThumbGrid.RemoveChild(tgi);
            tgi.QueueFree();
            //VisibleTGIs[vidx] = _visibletgis[vidx];
            AllTGIs[idx] = _alltgis[idx];
            item.Package = package;
            tgi = MakeTGIItem(item);
            VisibleTGIs[vidx] = tgi;
            ThumbGrid.AddChild(tgi);
            ThumbGrid.MoveChild(tgi, vidx);  
        }
    }

    private ThumbnailGridItem MakeTGIItem(GridItem item)
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Creating thumbitem for {0}", item.Package.FileName));
        ThumbnailGridItem tgi = ThumbnailGridItemPS.Instantiate() as ThumbnailGridItem;
        tgi.PackageReference = item;
        tgi.ItemSelected += (x, y, z) => GridItemSelectionChanged(x, y, z);
        tgi.CustomMinimumSize = ItemSize;
        if (item.Package.PackageData != null)
        {
            if (item.Package.Game == SimsGames.Sims2)
            {
                if (item.Package.Mesh || item.Package.Recolor)
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Thumbitem {0} is a mesh or a recolor", item.Package.FileName));
                    tgi.ViewportVersion = true;
                    ThumbnailGridItem t = MakeViewportItem(tgi, item.Package);
                    if (t == null)
                    {
                        tgi.ViewportVersion = false;
                        tgi.ImagePlaceholder = 2;
                    } else
                    {
                        tgi = t;
                    }
                } else if (item.Package.Sims2Data.TXTRDataBlock.Count > 0)
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Thumbitem {0} is just a texture", item.Package.FileName));
                    tgi.ViewportVersion = false;
                    ThumbnailGridItem t = MakeTextureItem(tgi, item.Package);
                    if (t == null)
                    {
                        tgi.ImagePlaceholder = 2;
                    } else
                    {
                        tgi = t;
                    }
                } else
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Thumbitem {0} uses a placeholder image", item.Package.FileName));
                    tgi.ImagePlaceholder = 2;
                }
            } else
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Thumbitem {0} uses a placeholder image", item.Package.FileName));
                switch (item.Package.Game)
                {
                    case SimsGames.Sims3:
                    tgi.ImagePlaceholder = 3;
                    break;
                    case SimsGames.Sims4:
                    tgi.ImagePlaceholder = 4;
                    break;                    
                }  
            }            
        } else
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Thumbitem {0} uses a placeholder image", item.Package.FileName));
            switch (item.Package.Game)
            {
                case SimsGames.Sims2:
                tgi.ImagePlaceholder = 2;
                break;
                case SimsGames.Sims3:
                tgi.ImagePlaceholder = 3;
                break;
                case SimsGames.Sims4:
                tgi.ImagePlaceholder = 4;
                break;                    
            }
        }
            tgi.AccentColor = AccentColor;
            tgi.SelectedColor = SelectedColor;
            tgi.LabelText = item.Package.FileName;

        if (item.Package.CategoryName != "Default")
        {                
            tgi.TextColor = item.Package.PackageCategory.TextColor;
            tgi.BackgroundColor = item.Package.PackageCategory.Background;
        } else
        {            
            tgi.TextColor = TextColor;
            tgi.BackgroundColor = ItemBackgroundColor;
        }
        
        //tgi.IsSelected = item.IsSelected;
        item.IsOnScreen = true;
        return tgi;
    }

    public void MakeVisibleItems()
    {        
        //VisibleTGIs = [..VisibleTGIs];
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Making {0} items for thumb grid. {1} rows, {2} items per row", ItemCount, RowCount, ColumnCount));
        int items = 0;
        int position = ScrollPosition * ColumnCount;
        for (int r = 0; r < RowCount; r++)
        {
            for (int c = 0; c < ColumnCount; c++)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Adding item {0} - row {1}, column {2}", position, r, c));
                ThumbnailGridItem tgi = MakeTGIItem(GridItems[position]);
                VisibleTGIs.Add(tgi);
                AllTGIs[position] = tgi;
                ThumbGrid.AddChild(tgi);
                items++;
                position++;
            }
        } 
    }   

    private void GridItemSelectionChanged(bool Selected, GridItem package, ThumbnailGridItem gridItem)
    {        
        if ((!ShiftHeld && !ControlHeld) || PreviousSelection == -1){			
            for (int i = 0; i < GridItems.Count; i++)
            {
                if (GridItems[i].Identifier != package.Identifier)
                {
                    GridItems[i].IsSelected = false;
                    if (VisibleTGIs.Any(x => x.PackageReference.Identifier == GridItems[i].Identifier))
                    {
                        VisibleTGIs.First(x => x.PackageReference.Identifier == GridItems[i].Identifier).IsSelected = false;
                    }
                }
            }            
		} else if (ShiftHeld){
            foreach (GridItem item in GridItems)
            {        
                if (VisibleTGIs.Any(x => x.PackageReference.Identifier == item.Identifier))
                {
                    VisibleTGIs.First(x => x.PackageReference.Identifier == item.Identifier).IsSelected = false;
                }        
                item.IsSelected = false;
            }
            //int firstSelected = GridItems.OrderBy(x => x.GridIdx).First(x => x.IsSelected).GridIdx;
            // lastSelected = GridItems.OrderBy(x => x.GridIdx).Last(x => x.IsSelected).GridIdx;
            if (package.GridIdx > PreviousSelection)
            {                
                for (int i = PreviousSelection; i <= package.GridIdx; i++)
                {                    
                    GridItems[i].IsSelected = Selected;
                    if (VisibleTGIs.Any(x => x.PackageReference.Identifier == GridItems[i].Identifier))
                    {
                        VisibleTGIs.First(x => x.PackageReference.Identifier == GridItems[i].Identifier).IsSelected = Selected;
                    }                    
                }
            } else
            {                
                for (int i = package.GridIdx; i <= PreviousSelection; i++)
                {                    
                    GridItems[i].IsSelected = Selected;
                    if (VisibleTGIs.Any(x => x.PackageReference.Identifier == GridItems[i].Identifier))
                    {
                        VisibleTGIs.First(x => x.PackageReference.Identifier == GridItems[i].Identifier).IsSelected = Selected;
                    }                    
                }
            }
		}
        PreviousSelection = package.GridIdx; 
        ThumbGridItemsSelected?.Invoke(GridItems.Where(x => x.IsSelected).Select(x => x.Package).ToList());
    }

    private bool IsTGIOnScreen(int idx){
		return VisibleTGIs.Any(x => x.PackageReference.GridIdx == idx);
	}

    public ThumbnailGridItem MakeViewportItem(ThumbnailGridItem tgi, SimsPackage package)
    {
        try {
            if (package.Mesh || package.Type.Contains("Hair"))
            {
                if (package.Game == SimsGames.Sims2)
                {
                    Snapshotter snapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                    snapshotter.Packages = packages;
                    tgi.subViewport.AddChild(snapshotter);
                    snapshotter.BuildSims2Mesh(package);

                    if (!package.Type.Contains("Hair")) snapshotter.GetTexturesForS2Meshes(package);
                }
            } else if (!package.Mesh && package.Recolor)
            {
                if (packages.Where(x => x.ObjectGUID == package.ObjectGUID).Any(p => p.Mesh))
                {
                    SimsPackage matchingMesh = packages.Where(x => x.ObjectGUID == package.ObjectGUID).First(p => p.Mesh);
                    Snapshotter snapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                    snapshotter.Packages = packages;
                    tgi.subViewport.AddChild(snapshotter);
                    if (package.Game == SimsGames.Sims2)
                    {
                        snapshotter.BuildSims2Mesh(matchingMesh);
                        snapshotter.ApplyS2Textures(package);
                    }
                } else if (packageDisplay.ThisInstance.Files.OfType<SimsPackage>().Where(x => x.Sims2Data.XNGBDataBlock.ModelName.Contains(package.Sims2Data.TXTRDataBlock[0].TextureNoSuffix)).Any(p => p.Mesh))
                {
                    SimsPackage matchingMesh = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().Where(x => x.Sims2Data.XNGBDataBlock.ModelName.Contains(package.Sims2Data.TXTRDataBlock[0].TextureNoSuffix)).First(p => p.Mesh);
                    Snapshotter snapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                    snapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                    snapshotter.Packages = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList();
                    
                    snapshotter.BuildSims2Mesh(matchingMesh);
                    if (!package.Type.Contains("Hair")) {
                        if (package.Game == SimsGames.Sims2) snapshotter.ApplyS2Textures(package);
                    }
                }
            }            
            return tgi;
        } catch {
            return null;
        }

    }

    private ThumbnailGridItem MakeTextureItem(ThumbnailGridItem tgi, SimsPackage package)
    {
        if (!package.Mesh && !package.Recolor)
        {
            if (package.Game == SimsGames.Sims2)
            {
                TXTRData txtrdata = (package.PackageData as Sims2Data).TXTRDataBlock[0];
                if (txtrdata != null) if (txtrdata.Texture != null) tgi.ThumbnailImage.Texture = ImageTexture.CreateFromImage(txtrdata.Texture);
            }
        }
        return tgi;
    }


    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyboardKey) {
            if (keyboardKey.Keycode == Key.Shift)
            {
                if (keyboardKey.Pressed)
                {
                    ShiftHeld = true;
                } else if (keyboardKey.IsReleased())
                {
                    ShiftHeld = false;
                }
            } else if (keyboardKey.Keycode == Key.Ctrl)
            {
                if (keyboardKey.Pressed){
                    ControlHeld = true;
                } else if (keyboardKey.IsReleased()) {
                    ControlHeld = false;
                }
            } else if (keyboardKey.Keycode == Key.Up)
            {
                if (ListenForKeyPress && IsMouseInThumbGrid())
                {
                    if (PreviousSelection != 0){ 
                        ListenForKeyPress = false;
                        int currentSelection = PreviousSelection - 1;
                        
                        if (VisibleTGIs.Any(x => x.PackageReference.GridIdx == PreviousSelection)){
                            VisibleTGIs.First(x => x.PackageReference.GridIdx == PreviousSelection).FlipSelected(false);
                        }							
                        if (IsTGIOnScreen(currentSelection)){
                            VisibleTGIs.First(x => x.PackageReference.GridIdx == PreviousSelection).FlipSelected(true);
                            PreviousSelection = currentSelection;
                        } else {
                            //VisibleRows.First(x => x.PopulatedIndex == currentSelection).First().RowSelectedButtonPressed(true);
                            CallDeferred(nameof(OnScrollUp));
                            PreviousSelection = currentSelection;
                        }
                        CallDeferred(nameof(ListenForKeyPressTimer));
                    }
                }
            } else if (keyboardKey.Keycode == Key.Down)
            {
                if (ListenForKeyPress && IsMouseInThumbGrid())
                {
                    if (PreviousSelection != GridItems.Count -1){ 
                        ListenForKeyPress = false;
                        int currentSelection = PreviousSelection + 1;
                        
                        if (VisibleTGIs.Any(x => x.PackageReference.GridIdx == PreviousSelection)){
                            VisibleTGIs.First(x => x.PackageReference.GridIdx == PreviousSelection).FlipSelected(false);
                        }							
                        if (IsTGIOnScreen(currentSelection)){
                            VisibleTGIs.First(x => x.PackageReference.GridIdx == PreviousSelection).FlipSelected(true);
                            PreviousSelection = currentSelection;
                        } else {
                            CallDeferred(nameof(OnScrollDown));
                            PreviousSelection = currentSelection;
                        }							
                        CallDeferred(nameof(ListenForKeyPressTimer));
                    }
                }
            } else if (keyboardKey.Keycode == Key.A)
            {
                if (ListenForKeyPress && IsMouseInThumbGrid() && ControlHeld)
                {
                    ListenForKeyPress = false;
                    foreach (GridItem item in GridItems){
                        item.IsSelected = true;
                    }
                    foreach (ThumbnailGridItem item in VisibleTGIs){
                        item.IsSelected = true;
                    }
                    CallDeferred(nameof(ListenForKeyPressTimer));
                }
            }
        }
        if (@event is InputEventMouseButton button && button.IsReleased())
		{
			if (vScrollBar.Value != ScrollPosition)
			{
				if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Scrollbar Value: {0}, ScrollPosition: {1}", vScrollBar.Value, ScrollPosition));
				OnScrollScrolled(vScrollBar.Value);
			}
		}
        if (FirstLoaded) {
            if (ListenForKeyPress && IsMouseInThumbGrid()){ 
                if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed){
                    if (mouseEvent.ButtonIndex == MouseButton.WheelUp){
                        ListenForKeyPress = false;
                        OnScrollUp();
                    } else if (mouseEvent.ButtonIndex == MouseButton.WheelDown){
                        ListenForKeyPress = false;
                        OnScrollDown();
                    }
                    CallDeferred(nameof(ListenForKeyPressTimer));
                }
            }
        }
        
    }

    public override void _Process(double delta)
    {
        if (!ScrollerTasks.IsEmpty){
			RunScrollerTasks();
		}
    }



    public bool IsMouseInThumbGrid()
    {
        Vector2 mousepos = GetGlobalMousePosition(); 

		Rect2 rect = new (this.GlobalPosition, this.Size); 

		return rect.HasPoint(mousepos); 
    }

}




public class GridItem
{
    public SimsPackage Package {get; set;}
    public int OverallIdx {get; set;}
    public int GridIdx {get; set;}
    public bool IsSelected {get; set;}
    public Guid Identifier {get; set;}
    public bool IsOnScreen {get; set;}
}