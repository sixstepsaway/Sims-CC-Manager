using Godot;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.Packages.Containers;
using SimsCCManager.Settings.Loaded;
using SimsCCManager.UI.Containers;
using SimsCCManager.UI.Themes;
using SimsCCManager.UI.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class CustomDataGrid : MarginContainer
{
	public delegate void SelectedItemEvent(string identifer, int idx);
	public SelectedItemEvent SelectedItem;
	public delegate void UnselectedItemEvent(string identifer, int idx);
	public UnselectedItemEvent UnselectedItem;
	public delegate void EnabledItemEvent(string identifer, int idx);
	public EnabledItemEvent EnabledItem;
	public delegate void DisabledItemEvent(string identifer, int idx);
	public DisabledItemEvent DisabledItem;
	public delegate void LoadOrderChangedEvent(string identifer, int val);
	public LoadOrderChangedEvent LoadOrderHasChanged;
	public delegate void HeaderSortedEvent(int idx, SortingOptions sortingrule);
	public event HeaderSortedEvent HeaderSortedSignal;

	public delegate void MouseAffectingEvent(bool inside, int idx);
	public MouseAffectingEvent MouseAffectingGrid;

	public delegate void DoneLoadingEvent();
	public DoneLoadingEvent DoneLoading;


	public List<HeaderInformation> Headers = new();
	public List<CellContent> Data = new();
	PackedScene divider = GD.Load<PackedScene>("res://UI/CustomDataGrid/DataGridHeaderResizer.tscn");
	PackedScene headercell = GD.Load<PackedScene>("res://UI/CustomDataGrid/DataGridHeaderCell.tscn");
	PackedScene header = GD.Load<PackedScene>("res://UI/CustomDataGrid/DataGridHeaderRow.tscn");
	PackedScene row = GD.Load<PackedScene>("res://UI/CustomDataGrid/DataGridRow.tscn");
	public List<Vector2> ColumnSizes = new();
	VBoxContainer GridContainer;
	DataGridHeaderRow HeaderRow;
	public SortingOptions sortingRule = SortingOptions.NotSorted;
	public List<DataGridRow> rows = new();

	Button ScrollUp; 
	Button ScrollDown;
	Panel ScrollBar;
	Panel ScrollBarFilled;
	Control ScrollControl;
	int MaxScroll = 0;
	float ScrollStep = 0;
	int _rowsonscreen;
	public int rowsonscreen {
		get { return _rowsonscreen; }
		set { _rowsonscreen = value; 
		rowsstep = value - 1;}
	}
	int rowsstep {get; set;}
	MarginContainer VertScrollContainer;
	float panesize = 0;
	bool mouseingrid = false;
	bool canscroll = true;
	bool scrollbarheld = false;
	public bool headersready = false;
	float rowheight = 0f;
	public delegate void PropertyChangedEvent();
	public event PropertyChangedEvent ScrollBarPositionChanged;
	public event PropertyChangedEvent MousePositionChanged;
	public event PropertyChangedEvent ScrollChanged;
	private double _scrollbarposition;
	public double ScrollBarPosition {
		get {return _scrollbarposition; }
		set {
			_scrollbarposition = value;
			ScrollBarPositionChanged.Invoke();
		}
	}

	double ScrollBarTemp = 0;
	private Vector2 _mouseposition;
	Vector2 MousePosition {
		get {return _mouseposition; }
		set {
			_mouseposition = value;
			MousePositionChanged.Invoke();
		}
	}

	private int _scrollposition;
	int ScrollPosition {
		get {return _scrollposition; }
		set {
			_scrollposition = value;
			ScrollChanged.Invoke();
		}
	}
	float sby = 0;

	VScrollBar vScrollBar;
	ScrollContainer MainScrollContainer;

	VBoxContainer HeaderRowContainer;
	VBoxContainer RowsContainer;

	double vScrollBarPosition;

	List<int> VisibleRows = new();


	public override void _Ready()	
	{	
		
		ScrollBarPositionChanged += () => ChooseScroll();
		//MousePositionChanged += () => MousePositionMoved();
		ScrollChanged += () => Scroll();
		WaitLoad();
	}

    private void ScrollbarMoved()
    {		
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Scrollbar position has changed!"));
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Scrollbar value: {0}", vScrollBar.Value));
		ScrollBarPosition = vScrollBar.Value;
    }

    private async Task WaitLoad(){
    	await ToSignal(RenderingServer.Singleton, RenderingServerInstance.SignalName.FramePostDraw);
		
		ContinueReady();
	}

	private void GetRowsScreen(){
		panesize = GridContainer.Size.Y;
		rowsonscreen = (int)Math.Floor(panesize / 25);
		rowheight = (float)Math.Ceiling(panesize / rowsonscreen);
	}

	private void ScreenSizeChanged(){
		GetRowsScreen();
		SetScrollBar();
		Scroll();
	}

	private void ContinueReady(){
		GridContainer = GetNode<VBoxContainer>("MarginContainer/GridContainer/VBoxContainer/DataGrid_Rows");
		panesize = GridContainer.Size.Y;
		rowsonscreen = (int)Math.Floor(panesize / 25);
		rowheight = (float)Math.Ceiling(panesize / rowsonscreen);
		GridContainer.Resized += () => ScreenSizeChanged();
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Data grid size: {0}.", panesize));
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Data grid will have {0} rows on screen and the window size is {1}.", rowsonscreen, panesize));
		HeaderRowContainer = GetNode<VBoxContainer>("MarginContainer/GridContainer/VBoxContainer/DataGrid_HeaderRow");
		RowsContainer = GetNode<VBoxContainer>("MarginContainer/GridContainer/VBoxContainer/DataGrid_Rows");
		vScrollBar = GetNode<VScrollBar>("VScroller/VScrollBar");
		vScrollBar.Scrolling += () => ScrollbarMoved();
		vScrollBarPosition = vScrollBar.Value;
		MainScrollContainer = GetNode<ScrollContainer>("MarginContainer/GridContainer");
		GetScrollBarStuff();
		var headerrow = header.Instantiate() as DataGridHeaderRow;
		foreach (HeaderInformation header in Headers){
			var cellinst = headercell.Instantiate() as DataGridHeaderCell;
			if (header.Blank == false){				
				cellinst.Label = header.HeaderTitle;
				cellinst.setSize = new Vector2(70, rowheight);
				cellinst.Size = new Vector2(70, rowheight);
				ColumnSizes.Add(new Vector2(70, rowheight));
			} else {
				cellinst.setSize = new Vector2(30, rowheight);
				cellinst.Size = new Vector2(30, rowheight);
				ColumnSizes.Add(new Vector2(30, rowheight));
			}
			cellinst.HeaderResizedEvent += (idx) => HeaderResized(idx); 
			cellinst.HeaderSortedEvent += (idx) => HeaderSorted(idx);
			
			headerrow.GetNode<HBoxContainer>("Row").AddChild(cellinst);
		}
		HeaderRowContainer.AddChild(headerrow);
		HeaderRowContainer.MoveChild(headerrow, 0);
		HeaderRow = headerrow;
		headersready = true;
	}

	private void GetScrollBarStuff(){
		ScrollUp = GetNode<Button>("VScroller/ScrollUp/ScrollUp_Button");
		ScrollDown = GetNode<Button>("VScroller/ScrollDown/ScrollDown_Button");
		ScrollUp.Pressed += () => ScrollUpButtonClick();
		ScrollDown.Pressed += () => ScrollDownButtonClick();
	}

	private void ChooseScroll(){
		ChangeScroll();
		/*if (ScrollBarPosition > vScrollBarPosition){			
			ScrollDownClick((int)(ScrollBarPosition - vScrollBarPosition));
		} else if (ScrollBarPosition < vScrollBarPosition){
			ScrollUpClick((int)(ScrollBarPosition + vScrollBarPosition));
		}*/
		
	}

	private void ChangeScroll(){
		int prevstartindex = (int)vScrollBarPosition;
		int prevendindex = prevstartindex + rowsonscreen;
		int startindex = (int)ScrollBarPosition;
		int endindex = startindex + rowsonscreen;
		vScrollBarPosition = ScrollBarPosition;
		for (int i = prevstartindex; i < prevendindex; i++){
			if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Setting row {0} to hidden.", i));
			rows[i].Visible = false;
		}
		for (int i = startindex; i < endindex; i++){
			if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Setting row {0} to visible.", i));
			rows[i].Visible = true;
		}
	}
    
    private void ScrollUpButtonClick(){
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Scroll up clicked."));
		vScrollBar.Value -= vScrollBar.Step;
		ScrollUpClick();
	}
	private void ScrollDownButtonClick(){
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Scroll down clicked."));
		vScrollBar.Value += vScrollBar.Step;
		ScrollDownClick();
	}

	private void ScrollUpClick(int scrollby = 1){
		if (ScrollPosition != 0){			
			ScrollPosition -= scrollby;
		} 
		//canscroll = true;	
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Scroll position: {0}.", ScrollPosition));	
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Visible rows: {0}.", rows.Where(x => x.Visible).Count()));
	}

	private void ScrollDownClick(int scrollby = 1){
		if (ScrollPosition != MaxScroll){
			ScrollPosition += scrollby;
		}
		//canscroll = true;
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Scroll position: {0}.", ScrollPosition));
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Visible rows: {0}.", rows.Where(x => x.Visible).Count()));
	}

	private void Scroll(){
		//if (canscroll){		
		foreach (DataGridRow row in rows.Where(x => x.Visible).ToList()){
			rows[rows.IndexOf(row)].Visible = false;
		}
		for (int i = ScrollPosition; i < (ScrollPosition + rowsonscreen); i++){
			if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Setting row {0} to visible.", i));
			rows[i].Visible = true;
		}
		//}		
	}

	private void SetScrollBar(){
		int y = (int)panesize;
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Rows on screen: {0}", rowsonscreen));
		if (rowsonscreen >= GridContainer.GetChildCount()){
			vScrollBar.Page = 1;
			vScrollBar.MaxValue = 1;
			vScrollBar.MinValue = 0;			
		} else {
			int items = GridContainer.GetChildCount();
			MaxScroll = items - rowsonscreen;
			vScrollBar.Step = 1;
			if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Scrollbar Max Value: {0}", MaxScroll));
			vScrollBar.MaxValue = MaxScroll;
			vScrollBar.MinValue = 0;
			vScrollBar.Page = 0;
			vScrollBar.Value = 0;
		}
	}

    private void HeaderSorted(int idx)
    {
		if (sortingRule == SortingOptions.NotSorted){
			sortingRule = SortingOptions.Ascending;
		} else if (sortingRule == SortingOptions.Ascending){
			sortingRule = SortingOptions.Descending;
		} else if (sortingRule == SortingOptions.Descending){
			sortingRule = SortingOptions.NotSorted;
		}
		DataGridHeaderCell headercell = HeaderRow.GetNode<HBoxContainer>("Row").GetChild(idx) as DataGridHeaderCell;
		headercell.ToggleSorting();		
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Sorting by header {0}: {1}", idx, Headers[idx].HeaderTitle));
		HeaderSortedSignal.Invoke(idx, sortingRule);
    }

	private void MouseAffectingRow(bool inside, int idx){	
		mouseingrid = inside;
		MouseAffectingGrid.Invoke(inside, idx);
	}

	private void ClearChildren(){
		if (GridContainer.GetChildCount() > 0){
			for (int i = 0; i < GridContainer.GetChildCount(); i++){
				GridContainer.GetChild(i).QueueFree();
			}
		}
	}

    public void RowsFromData(){
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Setting rows from data!"));
		if (Data.Count != 0){
			CallDeferred(nameof(ClearChildren));
			DataGridRow dataGridRow = row.Instantiate() as DataGridRow;			
			Color ColorA = LoadedSettings.SetSettings.LoadedTheme.DataGridA;
			Color ColorB = LoadedSettings.SetSettings.LoadedTheme.DataGridB;
			Color SelectedColor = LoadedSettings.SetSettings.LoadedTheme.DataGridSelected;		
			int rowscount = 0;
			int currentrow = -1;		
			foreach (CellContent item in Data){					
				if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Adding row {0}, column {1}: {2} as cell item.", item.RowNum, item.ColumnNum, item.Content));
				if (currentrow != item.RowNum){
					if (currentrow != -1) rows.Add(dataGridRow);
					dataGridRow = row.Instantiate() as DataGridRow;	
					dataGridRow.MouseAffected += (inside, idx) => MouseAffectingRow(inside, idx);
					dataGridRow.ItemSelected += (ident, idx) => ItemSelected(ident, idx);
					dataGridRow.ItemDeselected += (ident, idx) => ItemUnselected(ident, idx);
					dataGridRow.ItemEnabled += (ident, idx) => ItemEnabled(ident, idx);
					dataGridRow.ItemDisabled += (ident, idx) => ItemDisabled(ident, idx);
					dataGridRow.IntChanged += (ident, val) => LoadOrderChanged(ident, val);	
					dataGridRow.rowcolor = ColorA;
					dataGridRow.selectedcolor = SelectedColor;
					dataGridRow.Identifier = item.RowIdentifier;
					currentrow = item.RowNum;
					dataGridRow.Index = item.RowNum;
					if (Utilities.IsEven(currentrow)){
						dataGridRow.rowcolor = ColorA;
					} else {
						dataGridRow.rowcolor = ColorB;
					}
					dataGridRow.selectedcolor = SelectedColor;
					dataGridRow.rowheight = rowheight;
				}
				dataGridRow.AddCell(item, ColumnSizes[item.ColumnNum]);	
			}
			//dataGridRow.MouseAffected += (inside, idx) => MouseAffectingRow(inside, idx);			
			rows.Add(dataGridRow);			
			CallDeferred(nameof(PopulateRows));			
			CallDeferred(nameof(SetScrollBar));	
		} else {
			if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Oops! No data."));
		}
	}

	private void PopulateRows(){
		for (int i = 0; i < rows.Count; i++){
			GridContainer.AddChild(rows[i]);			
		}
		bool scroller = false;
		for (int i = 0; i < rows.Count; i++){
			if (i == ScrollPosition){
				scroller = true;
			} else if (i == ScrollPosition + rowsonscreen){
				scroller = false;
			}
			if (scroller){
				if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Making row {0} visible.", i));
				rows[i].Visible = true;
			} else {
				if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Making row {0} hidden.", i));
				rows[i].Visible = false;
			}
		}
		
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Grid container children: {0}", GridContainer.GetChildCount()));
		for (int i = 0; i < ColumnSizes.Count; i++){
			HeaderResized(i);
		}
	}

    public override void _Process(double delta)
    {
        //if (HeaderScroll != null) HeaderScroll.ScrollHorizontal = RowsScroll.ScrollHorizontal;
		//if (scrollbarheld) MousePosition = GetLocalMousePosition();
		
		//if (headersready) sby = ScrollBar.GlobalPosition.Y;
		/*new Thread( () => {
			while (scrollbarheld){
				//if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Scroll bar is being held..."));
				//if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mouse is at: {0}, Scrollbar is at global: {1}", MousePosition, ScrollBarGlobalTopPos));
				if (MousePosition.Y > ScrollBarGlobalTopPos.Y && MousePosition.Y < MaxScroll){
					if (ScrollPosition != 0){
						//float sby = ScrollBar.GlobalPosition.Y;
						float sbystep = sby;
						float mpy = MousePosition.Y;
						bool waiting = true;
						int steps = 0;
						if (mpy > sby){
							while (waiting){
								sbystep += ScrollStep;
								if (sbystep < mpy) { 
									sby = sbystep; 
									steps++;
								}
								if (sbystep > mpy) waiting = false;
								ScrollPosition += steps;
							}
						} else if (mpy < sby){
							while (waiting){
								sbystep -= ScrollStep;
								if (sbystep > mpy) { 
									sby = sbystep; 
									steps++;
								}
								if (sbystep < mpy) waiting = false;
								ScrollPosition -= steps;
							}
						}
						float pos = ScrollBarPosition.Y + (ScrollStep * steps);
						ScrollBarPosition = new(ScrollBarPosition.X, pos);
						//ChangeScroll(newscroll);
					}
				}
			}
		}){IsBackground = true}.Start();*/
    }

    public override void _Input(InputEvent @event)
    {
		if (mouseingrid /*&& canscroll*/){		
			if (@event.IsActionPressed("ScrollUp")){
				//canscroll = false;
				if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Scrolling datagrid up."));
				ScrollUpClick();
			}
			if (@event.IsActionPressed("ScrollDown")){
				//canscroll = false;
				if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Scrolling datagrid down."));
				ScrollDownClick();
			}	
			if (@event.IsActionPressed("ui_up")){
				SelectUp();
			}
			if (@event.IsActionPressed("ui_down")){
				SelectDown();
			}
		}

		
		// else if (scrollbarheld && @event.IsActionReleased("LeftClick")){
		//	if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Released the scroll bar."));
		//	scrollbarheld = false;
		//}
    }

	private void SelectUp(){
		int idx = rows.IndexOf(rows.Where(x => x.Selected).First());
		List<DataGridRow> selected = rows.Where(x => x.Selected).ToList();
		if (selected.Any()){
			foreach (DataGridRow row in selected){
				rows[rows.IndexOf(row)].Selected = false;
				UnselectedItem.Invoke(row.Identifier, rows.IndexOf(row));
			}
		}
		idx--;
		rows[idx].Selected = true;
		SelectedItem.Invoke(rows[idx].Identifier, idx);		
	}

	private void SelectDown(){
		int idx = rows.IndexOf(rows.Where(x => x.Selected).Last());
		List<DataGridRow> selected = rows.Where(x => x.Selected).ToList();
		if (selected.Any()){
			foreach (DataGridRow row in selected){
				rows[rows.IndexOf(row)].Selected = false;
				UnselectedItem.Invoke(row.Identifier, rows.IndexOf(row));
			}
		}
		idx++;
		rows[idx].Selected = true;
		SelectedItem.Invoke(rows[idx].Identifier, idx);		
	}

    private void ItemSelected(string Identifier, int idx){
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Selected item {0}", Identifier));
		SelectedItem.Invoke(Identifier, idx);
	}
	private void ItemUnselected(string Identifier, int idx){
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Unselected item {0}", Identifier));
		UnselectedItem.Invoke(Identifier, idx);
	}
	private void ItemEnabled(string Identifier, int idx){
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Enabled item {0}", Identifier));
		EnabledItem.Invoke(Identifier, idx);		
	}
	private void ItemDisabled(string Identifier, int idx){
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Disabled item {0}", Identifier));
		DisabledItem.Invoke(Identifier, idx);
	}

	private void LoadOrderChanged(string ident, int newvalue){
		LoadOrderHasChanged.Invoke(ident, newvalue);
	}

	public void UpdateLoadOrder(int idx, int loadorder){
		rows[idx].ChangeLoadOrder(loadorder);
	}

	private void HeaderResized(int column){
		//if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Column {0} was resized", column));
		DataGridHeaderCell header = HeaderRow.GetChild(1).GetChild(column) as DataGridHeaderCell;
		var size = header.Size;
		int rows = GridContainer.GetChildCount();
		for (int i = 0; i < rows; i++){
			SetSize(size, GridContainer.GetChild(i).GetChild(1).GetChild(column) as DataGridCell);
		}
	}

	private void SetSize(Vector2 size, DataGridCell cell){
		size = new Vector2(size.X - 1, size.Y);
		cell.Set("custom_minimum_size", size);
		cell.Size = size;
	}

	
}

public class HeaderInformation {
	public string HeaderTitle {get; set;} = "";
	public bool Blank {get; set;} = false;
	public bool Resizeable {get; set;} = false;
	public DataGridContentType ContentType {get; set;} = DataGridContentType.Null;
	public string ColumnData {get; set;} = "";
}

public class CellContent {
	public int RowNum {get; set;} = 0;
	public int ColumnNum {get; set;} = 0;
	public string Content {get; set;} = "";
	public CellOptions CellType {get; set;} = CellOptions.Text;
	public bool Icons {get; set;} = false;
	public IconOptions IconOptions {get; set;}
	public string RowIdentifier {get; set; } = "";
	public bool Selected {get; set;} = false;
	public string ContentType {get; set;} = "";
	private Color _backgroundcolor;
	public Color BackgroundColor {
		get { return _backgroundcolor; }
		set { _backgroundcolor = value; 
		TextColor = UIUtilities.GetFGColor(value);}
	}
	public Color TextColor {get; set;}

	public CellContent(){
		IconOptions = new();
	}
}

public class IconOptions {
	public bool Root {get; set;} = false;
	public bool OutOfDate {get; set;} = false;
	public bool Broken {get; set;} = false;
	public bool Conflicts {get; set;} = false;
	public bool WrongGame {get; set;} = false;
	public bool Override {get; set;} = false;
	public bool Orphan {get; set;} = false;
	public bool Fave {get; set;} = false;
	public bool Folder {get; set;} = false;	
	public bool LoadAsFolder {get; set;} = false;
	public bool LinkedFiles {get; set;} = false;
	public bool MiscFile {get; set;} = false;

	public int GetCount(){
		int count = 0;
		if (Root) count += 1;
		if (OutOfDate) count += 1;
		if (Broken) count += 1;
		if (Conflicts) count += 1;
		if (WrongGame) count += 1;
		if (Override) count += 1;
		if (Orphan) count += 1;
		if (Fave) count += 1;
		if (Folder) count += 1;
		if (LoadAsFolder) count += 1;
		if (LinkedFiles) count += 1;
		if (MiscFile) count += 1;
		return count;
	}

	public string GetIcons(){
		StringBuilder sb = new();
		if (Root) sb.AppendLine("Root");
		if (OutOfDate) sb.AppendLine("Out of Date");
		if (Broken) sb.AppendLine("Broken");
		if (Conflicts) sb.AppendLine("Has Conflicts");
		if (WrongGame) sb.AppendLine("Wrong Game");
		if (Override) sb.AppendLine("Override");
		if (Orphan) sb.AppendLine("Orphan");
		if (Fave) sb.AppendLine("Favorite");
		if (Folder) sb.AppendLine("Folder");
		if (LoadAsFolder) sb.AppendLine("Load as Folder");
		if (LinkedFiles) sb.AppendLine("Has Linked Files");
		if (MiscFile) sb.AppendLine("Misc File");
		return sb.ToString();
	}
}

public enum CellOptions {
	Text,
	Icons,
	TrueFalse,
	Int
}
