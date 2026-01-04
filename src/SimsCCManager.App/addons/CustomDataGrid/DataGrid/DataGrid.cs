using DataGridExceptions;
using CustomDataGridUniversals;
using DataGridContainers;
using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Data;
using MoreLinq;


public partial class DataGrid : Control
{
	/// <summary>
	/// The Data Grid object.
	/// </summary>
	 
	public string MinigridPath = "";

	DataGridUi dataGridUi;	

	
	// events
	public delegate void SelectedItemEvent(List<DataGridRow> rows);
	public SelectedItemEvent ItemSelected;
	public delegate void ToggledCellOptionEvent(string identifer, string option, int idx, bool toggled);
	public ToggledCellOptionEvent ToggledCellOption;
	public delegate void NumberChangedEvent(string identifer, int val);
	public NumberChangedEvent NumberChanged;
	public delegate void HeaderSortedEvent(int idx, SortingOptions sortingrule);
	public event HeaderSortedEvent HeaderSortedSignal;
	public delegate void DataChangedEvent(DataGridRow rowIdx, string dataChanged, int Item);
	public event DataChangedEvent DataChanged;

	public delegate void MouseAffectingEvent(bool inside, int idx);
	public MouseAffectingEvent MouseAffectingGrid;

	public delegate void DoneLoadingEvent();
	public DoneLoadingEvent DoneLoading;

	public delegate void SelectionChangedEvent(List<DataGridRow> SelectedRows, List<int> SelectedRowsIdxs);
	public SelectionChangedEvent SelectionChanged;

	public delegate void MakeTooltipEvent(string tooltip);
	public MakeTooltipEvent MakeTooltip;
	public delegate void MakeRCMenuEvent(HeaderClickMenuHolder headermenu);
	public MakeRCMenuEvent MakeRCMenu;

	//packed scenes
	PackedScene HeaderRowPS = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/DataGridHeaderRow.tscn");
	//PackedScene RowPS = GD.Load<PackedScene>("");
	PackedScene HeaderCellPS = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/DataGridHeaderCell.tscn");
	PackedScene DataGridCellPS = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/DataGridCell.tscn");
	PackedScene SizeAdjusterPS = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/DataGridHeaderSizeAdjuster.tscn");
	PackedScene IconOptionPS = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/DataGridIconOption.tscn");
	PackedScene DataGridUIScene = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/DataGridUI.tscn");
	PackedScene DataGridRowUIScene = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/DataGridRowUI.tscn");
	PackedScene HeaderMenuHolder = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/HeaderClickMenuHolder.tscn");
	PackedScene DataGridSubRowUIScene = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/DataGridSubRowUI.tscn");
	PackedScene MinigridScene = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/MiniGrid.tscn");
	PackedScene MinigridItemPS = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/MiniGriditem.tscn");

	//Nodes
	public VBoxContainer HeaderHolder;
	public VBoxContainer RowsHolder;
	public VScrollBar vScrollBar;
	public HScrollBar hScrollBar;
	public DataGridHeaderRow HeaderRow;
	public VScrollBar ScrollContainerScrollBarV;
	public HScrollBar ScrollContainerScrollBarH;
	public HeaderClickMenuHolder HeaderMenu;	

	//Controls
	private bool ShiftHeld = false;
	private bool ControlHeld = false;
	private int PreviousSelection = -1;
	private bool ListenForKeyPress = true;
	private Godot.Timer KeyPressTimer;
	private Godot.Timer ScrollHeldTimer;
	private Godot.Timer ScrollDownTimer;
	private double HScrollStep;
	private int MaxScroll;
	private bool ScrollDownHeld = false;
	private bool ScrollUpHeld = false;
	private bool ScrollReleased = false;
	public ConcurrentQueue<Task> ScrollerTasks = new();
	private bool ScrollerTasksRunning = false;

	//Color theme
	List<ColorRect> Backgrounds = new();
	//scrollbar button colors + scrollbar itself
	List<ColorRect> AccentColors = new();
	List<ColorRect> HeaderAccentColors = new();
	//background of the scrollbar for example
	List<ColorRect> SecondaryColors = new();
	public List<Label> HeaderLabels = new();
	public Vector2 PaneSize = new();
	private float PaneHeight;
	public float RowsOnScreen;
	private float ScrollBarHeight;
	[Export]
	public int DefaultRowHeight = 25;
	[Export]
	public bool LinkToggleToAdjustmentNumber;
	private int RowHeight = 25;
	private int _scrollposition;
	private int _headery;
	private int HeaderY 
	{
		get { return _headery; }
		set { _headery = value; }
	}
	private int AdjustmentCellIdx;
	public int ScrollPosition {
		get { return _scrollposition; }
		set { _scrollposition = value; }		
	}

	private bool _blockinput;
	public bool BlockInput { get { return _blockinput; } 
	set {_blockinput = value; 
	PleasePassLog(string.Format("Input blocked: {0}", value));}}


	Func<DataGridRow, Object> orderByFunc = null;

	//Data
	private List<DataGridHeader> _headers;
	public List<DataGridHeader> Headers {
		get { return _headers; }
		set { _headers = value; 
		CheckHeaderValidity();
		UpdateHeaderRow();}
	}

	private List<DataGridRow> _selectedrows;
	public List<DataGridRow> SelectedRows {
		get { return _selectedrows; }
		set { _selectedrows = value; 
			SelectionChanged?.Invoke(value, _selectedrows.Select(x => x.Idx).ToList());
		}
	}

    private void CheckHeaderValidity()
    {
        if (Headers.Where(x => x.CellType == CellOptions.Toggle).Any()){
			if (Headers.Where(x => x.CellType == CellOptions.Toggle).Count() > 1){
				throw new DataGridHeadersIncorrectException("Headers can only contain one toggle cell.");
			}
		} 
		if (Headers.Exists(x => x.CellType == CellOptions.Picture)){
			if (Headers.Where(x => x.CellType == CellOptions.Picture).Count() > 1){
				throw new DataGridHeadersIncorrectException("Headers can only contain one picture cell.");
			} else if (Headers.Where(x => x.CellType == CellOptions.Picture).First().PictureCellSize == new Vector2(0, 0)){
				throw new DataGridHeadersIncorrectException("Picture cell size must be set.");
			}
		} 
		if (Headers.Where(x => x.CellType == CellOptions.AdjustableNumber).Any()){
			if (Headers.Where(x => x.CellType == CellOptions.AdjustableNumber).Count() > 1){
				throw new DataGridHeadersIncorrectException("Headers can only contain one adjustable number cell.");
			}
		}
    }

    public List<DataGridHeaderCell> HeaderCells = new();

	private bool Sorted = false;
	private bool Searched = false;
	private List<DataGridRow> CellsUnsorted = new();
	private List<DataGridRow> CellsUnsearched = new();
	//the data of all the rows
	private List<DataGridRow> _rowdata;
	public List<DataGridRow> RowData {
		get {return _rowdata; }
		set { _rowdata = value; 
			CallDeferred(nameof(GetPaneSizes));
		}
	}
	public bool FirstLoaded = false;
	private bool HeaderMenuShowing = false;

	//rows currently visible on the screen
	public List<DataGridRowUi> VisibleRows = new();
	public List<DataGridRowUi> AllRows = new();
	private DataGridRowUi[] _allrows
	{
		get { return new DataGridRowUi[RowData.Count]; }
	}
	private List<DataGridCellIcons> IconOptions = new();

	//accessible settings
	[ExportCategory("Text")]
	[Export]
	public int HeaderTextSize = 100;
	[Export]
	public int CellTextSize = 100;
	[Export]
	public int MenusTextSize = 100;
	[Export]
	public Font MainFont;
	public int DefaultHeaderTextSize = 14;
	public int DefaultCellTextSize = 12;
	public int DefaultMenuTextSize = 10;

	private int VisibleHeaders = 0;

	public Font DefaultFont;

	
	[ExportCategory("Colors")]
	[Export]
	public Color BackgroundColor = Color.FromHtml("D7D2E5");
	[Export]
	public Color SecondaryColor = Color.FromHtml("7DAEEB");
	[Export]
	public Color AccentColor = Color.FromHtml("9ED1F8");
	[ExportCategory("Rows")]
	[ExportGroup("Colors")]
	[Export]
	public Color MainRowColor = Color.FromHtml("BFE1FA");
	[Export]
	public Color SelectedRowColor = Color.FromHtml("6DB2E3");
	[Export]
	public Color AlternateRowColor = Color.FromHtml("D4EBFB");
	[Export]
	public Color MainRowTextColor = Color.FromHtml("354D5E");
	[Export]
	public Color SelectedRowTextColor = Color.FromHtml("264358");
	[Export]
	public Color AlternateRowTextColor = Color.FromHtml("25343F");
	[Export]
	public Color Alert1RowColor = Color.FromHtml("6A8CA5");
	[Export]
	public Color Alert2RowColor = Color.FromHtml("9BB2C3");
	[Export]
	public Color Alert3RowColor = Color.FromHtml("7E95A6");
	[ExportGroup("Alerts")]
	[Export]
	public string Alert1 = "";
	[Export]
	public string Alert2 = "";
	[Export]
	public string Alert3 = "";

	[ExportCategory("IconOptions")]
	[Export]
	public string[] IconNames;
	[Export]
	public Texture2D[] IconImages;

	
	public delegate void PassLogEvent(string Log);
	public PassLogEvent PassLog;

	public List<MiniGrid> MiniGrids = new();

	public void PleasePassLog(string Log, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = "")
	{
		string filepath = "";
		if (filePath != "") {
			filepath = new FileInfo(filePath).Name;
		} else {
			filepath = "unknown";
		}    
		string time = DateTime.Now.ToString("h:mm:ss tt");
        string statement = string.Format("[L{0} | {1} {2}]: {3}", lineNumber, filepath, time, Log);
            
		PassLog?.Invoke(statement);
	}


	public delegate void MiniGridItemRemoveEvent(DataGridRow rowdata, int RowIdx, List<Guid> Items);
	public MiniGridItemRemoveEvent MiniGridItemRemove;
	public delegate void MiniGridItemAddEvent(DataGridRow rowdata, int RowIdx, List<string> Items);
	public MiniGridItemAddEvent MiniGridItemAdd;

	bool Populating = false;

	public delegate void DataGridFinishedFirstLoadEvent();
	public DataGridFinishedFirstLoadEvent DataGridFinishedFirstLoad;





	
	// Called when the node enters the scene tree for the first time.
	public override void _EnterTree()
	{
		HeaderY = 21;
		dataGridUi = DataGridUIScene.Instantiate() as DataGridUi;
		AddChild(dataGridUi);
		HeaderHolder = dataGridUi.HeaderHolder;
		RowsHolder = dataGridUi.RowsHolder;
		vScrollBar = dataGridUi.vScrollBar;
		hScrollBar = dataGridUi.hScrollBar;
		DefaultFont = dataGridUi.DefaultFont;
		dataGridUi.ScrollLeftButton.Pressed += () => OnScrollLeftPressed();
		dataGridUi.ScrollRightButton.Pressed += () => OnScrollRightPressed();
		dataGridUi.ScrollUpButton.ButtonDown += () => OnScrollUp_ButtonUsed(true);
		dataGridUi.ScrollDownButton.ButtonDown += () => OnScrollDown_ButtonUsed(true);
		dataGridUi.ScrollUpButton.ButtonUp += () => OnScrollUp_ButtonUsed(false);
		dataGridUi.ScrollDownButton.ButtonUp += () => OnScrollDown_ButtonUsed(false);

		vScrollBar.ValueChanged += (v) => OnScrollScrolled(v);

		

		ChildEnteredTree += (i) => SetThemeColors();
		ChildOrderChanged += () => SetThemeColors();
		Resized += () => ScreenResized();
		ScrollContainerScrollBarH = dataGridUi.ScrollContainer.GetHScrollBar();
		ScrollContainerScrollBarV = dataGridUi.ScrollContainer.GetVScrollBar();

		GetThemeColors();
		SetThemeColors();


		if (IconNames != null){
			if (IconNames.Length != IconImages.Length){
				} else {
				for (int i = 0; i < IconNames.Length; i++){
					IconOptions.Add(new() { IconName = IconNames[i], IconImage = IconImages[i] });			
				}
			}
		}
	}

	public bool ScrollChecker = false;

    private void OnScrollScrolled(double val)
    {
		//PleasePassLog(string.Format("Scroll is at {0}", val));
		new Thread(() => {		
			if (ScrollChecker) return;
			ScrollChecker = true;
			//PreviousSelection = -1;

			int currentValue = (int)val;

			if (currentValue != ScrollPosition + 1 && currentValue != ScrollPosition - 1)
			{
				//if (currentValue > ScrollPosition)
				//{
					/*int amountToIncrement = currentValue - ScrollPosition;
					int incrementing = currentValue;
					for (int i = 0; i < amountToIncrement; i++)
					{
						OnScrollBarDown(incrementing);
						incrementing++;
					}*/
					ScrollRepopulateRows(currentValue);
				/*} else
				{
					/*int amountToIncrement = ScrollPosition - currentValue;
					int incrementing = ScrollPosition;
					for (int i = 0; i < amountToIncrement; i++)
					{
						OnScrollBarUp(incrementing);
						incrementing--;
					}*/
					/*RepopulateRows(currentValue);
					ScrollPosition = currentValue;
				}*/
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

	private void ScrollRepopulateRows(int currentValue){
		ScrollerTasks.Enqueue(new (() => { 
			PopulateRows(currentValue);
		}));
	}

	private void OnScrollBarDown(int pos)
    {			
		ScrollerTasks.Enqueue(new (async () => { 
			await ScrollDownTask(pos);
		}));
    }

	private Task ScrollUpTask(int scrollPos)
	{
		int newRow = scrollPos;
		if (newRow >= 0)
		{
			int lastRow = scrollPos + (int)RowsOnScreen -1;
			bool done = CreateRow(RowData[newRow], newRow);
			RemoveRow(lastRow);		
			AddRow(newRow, 0);
		}		
		return Task.CompletedTask;
	}

	private Task ScrollDownTask(int scrollPos)
	{		
		int newRow = scrollPos + (int)RowsOnScreen -1;
		if (newRow <= RowData.Count)
		{
			bool done = CreateRow(RowData[newRow], newRow);
			RemoveRow(scrollPos-1);
			AddRow(newRow, (int)RowsOnScreen);
		}
		return Task.CompletedTask;
	}
	
	private void RemoveRow(int rowRmvIdx)
	{
		PleasePassLog(string.Format("Removing row at {0}", rowRmvIdx));
		AllRows[rowRmvIdx].QueueFree();
		VisibleRows.Remove(AllRows[rowRmvIdx]);
		AllRows[rowRmvIdx] = new();
	}

	private void AddRow(int rowIdx, int pos, bool atIndex = false)
	{
		PleasePassLog(string.Format("Adding row at {0}, {1} RowData entry", pos, rowIdx));
		//DataGridRowUi rowui = row as DataGridRowUi;	
		if (atIndex)
		{
			VisibleRows.Insert(pos, AllRows[rowIdx]);
			RowsHolder.AddChild(AllRows[rowIdx]);
			RowsHolder.MoveChild(AllRows[rowIdx], pos);
		}	
		else if (pos == 0) {
			VisibleRows.Insert(0, AllRows[rowIdx]);
			RowsHolder.AddChild(AllRows[rowIdx]);
			RowsHolder.MoveChild(AllRows[rowIdx], 0);
		} else
		{
			VisibleRows.Add(AllRows[rowIdx]);
			RowsHolder.AddChild(AllRows[rowIdx]);
		}
	}

    private void OnScrollBarUp(int pos)
    {
		ScrollerTasks.Enqueue(new (async () => { 
			await ScrollUpTask(pos);
		}));
    }

    private void ScreenResized(){
		if (FirstLoaded) GetPaneSizes();
	}

    private void UpdateHeaderRow(bool rowychanged = false){
		if (HeaderHolder.GetChildCount() != 0){
			foreach (DataGridHeaderCell header in HeaderCells){
				Headers[Headers.IndexOf(Headers.Where(x => x.Title == header.LabelText).First())].StartingWidth = (int)header.CellSize.X;
			}
			HeaderRow.QueueFree();
			HeaderCells.Clear();
		}		
		HeaderAccentColors.Clear();
		HeaderLabels.Clear();
		HeaderY = DefaultHeaderTextSize * (HeaderTextSize/100) + 5;
		int i = 0;
		DataGridHeaderRow headerRow = HeaderRowPS.Instantiate() as DataGridHeaderRow;
		headerRow.SetSize(new(headerRow.CurrentSize.X, HeaderY + 2));
		headerRow.BackgroundColor1.Color = Color.FromHtml("ffffff");
		Color c = Color.FromHtml("9696968d");
		c.A = 141;
		headerRow.BackgroundColor2.Color = c;
		foreach (DataGridHeader header in Headers){
			if (header.ShowHeader){
				DataGridHeaderCell headercell = HeaderCellPS.Instantiate() as DataGridHeaderCell;
				headercell.FontSize = DefaultHeaderTextSize * (HeaderTextSize/100);
				headercell.dataGrid = this;
				headercell.Name = header.Title;
				HeaderAccentColors.AddRange(headercell.Accents);
				if (header.Blank){
					headercell.SetSize(new Vector2(25, HeaderY));
				} else {
					int g = header.Title.Length * 10;
					headercell.SetSize(new Vector2(g, HeaderY));
				}
				headercell.LabelText = header.Title;
				headercell.DraggerColor = AccentColor;
				headercell.HeaderIndex = i;
				headercell.HeaderData = header.Data;
				HeaderLabels.Add(headercell.HeaderLabel);
				headercell.Blank = header.Blank;
								
				headercell.HeaderClicked += (i, d, r) => HeaderSorted(i, d, r);		
				headercell.MainGrid = this;
				headercell.ColumnMoved += (headerIdx, HeaderNewLocation) => ColumnMoved(headerIdx, HeaderNewLocation);
				headerRow.HeaderCells.Add(headercell);	
				headerRow.HeaderRow.AddChild(headercell);
				i++;
				DataGridHeaderSizeAdjuster sizeAdjuster = SizeAdjusterPS.Instantiate() as DataGridHeaderSizeAdjuster;
				sizeAdjuster.AttachedHeaderCell = headercell;
				sizeAdjuster.HeaderResized += (idx) => HeaderResized(idx);
				sizeAdjuster.Resizeable = header.Resizeable;
				if (!header.Blank && header.CellType != CellOptions.Picture){
					int min = header.Title.Length * (headercell.FontSize / 2);
					if (header.StartingWidth < min){
						header.StartingWidth = min;
					}
					sizeAdjuster.HeaderMinsize = min;
				} else if (header.CellType == CellOptions.Picture){
					headercell.StartingWidth = (int)header.PictureCellSize.X;
					sizeAdjuster.HeaderMinsize = headercell.StartingWidth;
				} else {
					headercell.StartingWidth = header.StartingWidth;
					sizeAdjuster.HeaderMinsize = 35;
				}
				headerRow.HeaderSliders.Add(sizeAdjuster);
				headerRow.HeaderRow.AddChild(sizeAdjuster);	
				HeaderCells.Add(headercell);
			}						
		}
		HeaderHolder.AddChild(headerRow);
		HeaderRow = headerRow;
		SetThemeColors();
		dataGridUi.VScrollContainer.AddThemeConstantOverride("margin_top", HeaderY);
		if (!FirstLoaded || rowychanged) GetPaneSizes(); //CreateAllRows();
		if (FirstLoaded) PopulateRows();
		VisibleHeaders = Headers.Where(x => x.ShowHeader).Count();
	}

	public delegate void HeadersChangedEvent(List<DataGridHeader> newHeaders);
	public HeadersChangedEvent HeadersChanged;

    private void ColumnMoved(int headerIdx, int headerNewLocation)
    {
		List<DataGridHeader> __headers = Headers;
		List<DataGridHeader> __visibleheaders = Headers.Where(x => x.ShowHeader).ToList();
		DataGridHeader header = __visibleheaders[headerIdx];
		int totalidx = __headers.IndexOf(header);
		__headers.Remove(header);
		__headers.Insert(headerNewLocation, header);
		HeadersChanged(__headers);
		Headers = __headers;
		List<DataGridRow> __rows = new();
		__rows = RowData;
		foreach (DataGridRow row in RowData){
			DataGridCellItem item = row.Items[totalidx];
			row.Items.Remove(item);
			row.Items.Insert(headerNewLocation, item);
		}
		RowData = __rows;
	}

	public void AddNewRows(int start)
	{
		/*PleasePassLog(string.Format("Adding new rows starting at {0}", start));
		for (int i = start; i < RowData.Count; i++)
		{
			PleasePassLog(string.Format("Adding row at {0}", i));	
			AllRows.Add(CreateRow(RowData[i], i));
		}*/
		SetScrollBar();
	}

	public void RemoveRow(DataGridRow data)
	{
		if (VisibleRows.Any(x => x.RowData.Identifier == data.Identifier)){
			RowData.Remove(RowData.First(x => x.Identifier == data.Identifier));
			PopulateRows();
		}
		SetScrollBar();
	}

	public void UpdateRow(DataGridRow importedRowData, bool newSubrow = false)
	{		
		if (VisibleRows.Any(x => x.RowData.Identifier == importedRowData.Identifier))
		{
			DataGridRow ogdata = RowData.First(x => x.Identifier == importedRowData.Identifier);
			importedRowData.Selected = ogdata.Selected;
			RowData[RowData.IndexOf(ogdata)] = importedRowData;			
			int idx = AllRows[importedRowData.Idx].GetIndex();
			RemoveRow(importedRowData.Idx);
			CreateRow(importedRowData, importedRowData.Idx);	
			AddRow(importedRowData.Idx, idx, true);
		}
	}

    public void PopulateRows(int from = -1){
		if (from == -1) from = ScrollPosition;
		new Thread(() => {
			PleasePassLog(string.Format("Attempting to populate with rows from {0} rowdata.", RowData.Count));
			if (Populating) { 
				PleasePassLog(string.Format("(Rows: {0}): Already populating!", RowData.Count));
				while (Populating)
				{
					//
				}				
			}
			if (!FirstLoaded) AllRows.AddRange(_allrows);

			PleasePassLog(string.Format("All rows length: {0}", AllRows.Count));
			
			Populating = true;			
			ConcurrentBag<Task> runningTasks = new();
			int completedTasks = 0;
			CallDeferred(nameof(SetScrollBar));			
			
			if (LinkToggleToAdjustmentNumber){
				CheckToggleNumbers();
			}
			
			int rowsend = 0;
			int rowsstart = 0;
			rowsstart = from;
			int rowsonscreenint = (int)RowsOnScreen;
			rowsend = from + rowsonscreenint;		

			
			if (RowsOnScreen > RowData.Count){
				rowsend = RowData.Count;
			} else if (from <= RowsOnScreen){
				rowsstart = from;
				rowsend = (int)RowsOnScreen + from;
			}
			
			PleasePassLog(string.Format("Rows start: {0}, Rows End: {1}", rowsstart, rowsend));

			List<DataGridRow> _rowdata = RowData.GetRange(rowsstart, (int)RowsOnScreen);

			PleasePassLog(string.Format("Items in _rowdata holder: {0}", _rowdata.Count));

			foreach (DataGridRow row in _rowdata){
				Task t = Task.Run( () => {
					PleasePassLog(string.Format("Creating row {0}: {1}", row.Idx, _rowdata.IndexOf(row)));
					bool r = CreateRow(row, row.Idx);
				});
				runningTasks.Add(t);
			}


			while (runningTasks.Any(x => !x.IsCompleted)){
				if (completedTasks != runningTasks.Count(x => x.IsCompleted)) {
					completedTasks = runningTasks.Count(x => x.IsCompleted);
					PleasePassLog(string.Format("{0} tasks in DataGrid runningTasks, {1} completed", runningTasks.Count, completedTasks));
				}
			}

			/*while (rowsForVisible.Count < (int)RowsOnScreen-1)
			{
				PleasePassLog(string.Format("Rows for visible: {0}, Expected: {1}", rowsForVisible.Count, (int)RowsOnScreen-1));
			}*/

			//PleasePassLog(string.Format("Pop rows: {0}, Ordered: {1}", rowsForVisible.Count, popRowOrdered.Count));
			CallDeferred(nameof(PopRowsDeferred), rowsstart, (int)RowsOnScreen);

			

			ScrollPosition = from;
		}){IsBackground = true}.Start();	
			
	}

	private void PopRowsDeferred(int start, int amt){	
		//ClearVisibleRows();	
		int end = start + amt;
		//List<DataGridRowUi> rows = AllRows.GetRange(start, amt).ToList();
		PleasePassLog(string.Format("Row selection for populating: {0} - {1}, from list of {2}", start, end, amt));
		//int i = start; 
		foreach (Node node in RowsHolder.GetChildren())
		{
			RowsHolder.RemoveChild(node);
		}
		VisibleRows.Clear();
		for (int i = start; i < end; i++){
			PleasePassLog(string.Format("Adding row {0} to display.", i));
			if (AllRows[i] == null) PleasePassLog(string.Format("Unfortunately row {0} was null.", i));
			RowsHolder.AddChild(AllRows[i]);
			VisibleRows.Add(AllRows[i]);			
		}
		if (RowsOnScreen < RowData.Count){
			dataGridUi.AllRowsContainer.SizeFlagsVertical = SizeFlags.ExpandFill;
			RowsHolder.SizeFlagsVertical = SizeFlags.ExpandFill;
		} else {
			dataGridUi.AllRowsContainer.SizeFlagsVertical = SizeFlags.ShrinkBegin;
			RowsHolder.SizeFlagsVertical = SizeFlags.ShrinkBegin;
		}
		PleasePassLog(string.Format("{0} rows populated!", VisibleRows.Count));
		if (!FirstLoaded) {
			FirstLoaded = true;
			DataGridFinishedFirstLoad?.Invoke();
		}
		StringBuilder sb = new();
		for (int i = 0; i < VisibleRows.Count; i++)
		{
			if (i < VisibleRows.Count)
			{
				sb.Append(string.Format("Row {0}, ", VisibleRows[i].OverallIndex));
			} else
			{
				sb.Append(string.Format("Row {0}.", VisibleRows[i].OverallIndex));
			}
		}
		PleasePassLog(string.Format("Visible rows contains: {0}", sb.ToString()));
		Populating = false;
	}

	private void ClearVisibleRows(){
		foreach (DataGridRowUi rmv in VisibleRows){
			rmv.QueueFree();
		}
		VisibleRows.Clear();
	}

	private bool CreateRow(DataGridRow importedRowData, int num, int AtPosition = -1)
	{
		PleasePassLog(string.Format("CreateRow: Creating row {0}", num));
		importedRowData.ItemsWereChanged += (row, i, r, c, idx) => RowDataEdited(row, i, r, c, idx);

		DataGridRowUi row = DataGridRowUIScene.Instantiate() as DataGridRowUi;
		if (RowsOnScreen < RowData.Count)
		{
			row.RowsHolder.SizeFlagsVertical = SizeFlags.ExpandFill;
			row.SizeFlagsVertical = SizeFlags.ExpandFill;
		}

		row.RowData = importedRowData;
		row.Datagrid = this;

		row.Name = importedRowData.RowRef;		

		row.RowEdited += (r, c) => RowWasEdited(r, c);
		row.AdjustmentNumberChanged += (x, c, n) => AdjustableNumberOneNumberChanged(x, c, n);
		row.AdjustmentNumberChangedPropagate += (x, c) => AdjustmentNumberAdjusted(x, c);
		row.OverallIndex = num;
		row.RowSelected += (s) => RowSelected(num, s);

		if (importedRowData.UseCategoryColor)
		{
			row.BackgroundColor = importedRowData.BackgroundColor;
			row.TextColor = importedRowData.TextColor; 
		} else if (UniversalMethods.IsEven(num))
		{				
			row.BackgroundColor = AlternateRowColor;
			row.TextColor = AlternateRowTextColor; //GetFGColor(BackgroundColor);
		} else
		{			
			row.BackgroundColor = MainRowColor;
			row.TextColor = MainRowTextColor; //GetFGColor(BackgroundColor);
		}
		row.SelectedColor = SelectedRowColor;	
		row.TextSize = DefaultCellTextSize * (CellTextSize / 100);	
		row.Selected = importedRowData.Selected;
		//row.NumBeingAdjusted += (r) => NumberAdjustmentCheck(r);
		if (Headers.Any(x => x.CellType == CellOptions.Toggle))
		{
			row.ToggleData = Headers.First(x => x.CellType == CellOptions.Toggle).Data;
		}
		row.AdjustNumberOnlyIfToggled = LinkToggleToAdjustmentNumber;
		int adjnum = 0;
		int hc = 0;
		for (int i = 0; i < Headers.Count; i++)
		{
			if (Headers[i].ShowHeader)
			{
				Vector2 size = new(0, 0);
				DataGridHeaderCell header = HeaderRow.HeaderCells[hc];
				DataGridHeaderSizeAdjuster sizeadjuster = HeaderRow.HeaderSliders[hc];
				DataGridCell cell = DataGridCellPS.Instantiate() as DataGridCell;
				cell.CellOptions = Headers[i].CellType;
				cell.thisRow = row;
				cell.FirstLoaded = FirstLoaded;
				cell.dataGrid = this;
				cell.AccentColor = AccentColor;
				cell.NumberAsBytes = Headers[i].NumberAsBytes;
				if (cell.CellOptions == CellOptions.Icons)
				{
					cell.Icons = importedRowData.RowIcons;
				}
				if (cell.CellOptions == CellOptions.Toggle)
				{
					row.ToggleCell = cell;
					bool toggleresult = ToBool(importedRowData.Items[i].ItemContent);
					row.Toggled = toggleresult;
					cell.ToggleToggled = toggleresult;
					cell.ToggleFlipped += (w) => row.CellWasToggled(w);
				}
				if (cell.CellOptions == CellOptions.Picture)
				{
					row.ImageCell = cell;
					size = new(Headers[i].PictureCellSize.X - 1, RowHeight);
					cell.StartSize = size;
					cell.SetSize(size);
					cell.ImageLocation = importedRowData.Items[i].ItemContent;
					cell.ImageSizer.Size = new(cell.StartSize.X - 15, cell.StartSize.Y - 15);
					cell.ImageSizer.CustomMinimumSize = new(cell.StartSize.X - 15, cell.StartSize.Y - 15);
					cell.ImageSizer.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
				}
				else if (cell.CellOptions == CellOptions.AdjustableNumber)
				{
					row.AdjustmentNumberCell = cell;
					size = new(header.CellSize.X - 1, RowHeight);
					cell.StartSize = size;
					cell.SetSize(size);
				}
				else
				{
					size = new(header.CellSize.X, RowHeight);
					cell.StartSize = size;
					cell.SetSize(size);
				}
				if (cell.CellOptions == CellOptions.AdjustableNumber)
				{
					cell.NumberContent = importedRowData.AdjustmentNumber;
					row.AdjustmentNumber = importedRowData.AdjustmentNumber;
					row.NumAdjustmentCell = cell;
					AdjustmentCellIdx = i;
					cell.ToggleLinked = LinkToggleToAdjustmentNumber;
				}
				if (cell.CellOptions == CellOptions.Int)
				{
					cell.NumberContent = int.Parse(importedRowData.Items[i].ItemContent);
				}
				PleasePassLog(string.Format("Cell {0} for row {1} size: {2}", i, num, size));
				cell.TextContent = importedRowData.Items[i].ItemContent;
				cell.Editable = Headers[i].ContentEditable;
				cell.ToggleFlipped += (Toggle) => ToggleFlipped(Toggle, row.OverallIndex);
				cell.Resizeable = Headers[i].Resizeable;
				cell.AssociatedHeader = header;
				cell.AssociatedHeaderSizer = sizeadjuster;
				cell.ProduceTooltip += (t) => TooltipProduced(t);
				row.AddCell(cell);
				hc++;
			}
		}

		if (importedRowData.SubRow)
		{		
			row.IsSubGrid = true;
			row.ShowSubgrid += (grid, row) => DisplaySubgrid(grid, row);
			MiniGrid minigrid = MinigridScene.Instantiate() as MiniGrid;
			minigrid.Size = new(PaneSize.X - (PaneSize.X * 0.1f), PaneSize.Y - (PaneSize.Y * 0.1f));
			row.SubGrid = minigrid;
			minigrid.addFiles.CurrentDir = MinigridPath;
			minigrid.MiniGridRowIdx = row.OverallIndex;
			minigrid.Row = row;
			minigrid.CloseButton.Pressed += () => CloseMinigrid(minigrid);
			int i = 0; 
			minigrid.AddButton.ButtonClicked += () => MinigridAddItemToGroup(importedRowData, minigrid);
			minigrid.RemoveButton.ButtonClicked += () => MinigridRemoveItemFromGroup(importedRowData, minigrid);
				
			foreach (DataGridRow rowitem in importedRowData.SubRowItems)
			{
				MiniGriditem mgi = MinigridItemPS.Instantiate() as MiniGriditem;
				mgi.SelectButton.Pressed += () => MinigridItemPressed(minigrid, mgi);
				mgi.RowData = rowitem;
				minigrid.MinigridItems.Add(mgi);
				mgi.TextOne.Text = rowitem.SubrowTextFirst;
				mgi.TextTwo.Text = rowitem.SubrowTextSecond;
				mgi.MgiIndex = i;
				minigrid.ItemContainer.AddChild(mgi);		
				i++;		
			}
			dataGridUi.AddMinigrid(minigrid);
			minigrid.addFiles.FileSelected += (files) => MinigridFileSelected(files, minigrid); 
			minigrid.addFiles.FilesSelected += (files) => MinigridFilesSelected(files, minigrid); 
			MiniGrids.Add(minigrid);
			minigrid.Visible = false;
		}


		AllRows[num] = row;
		PleasePassLog(string.Format("Created a row and put it into AllRows at {0}", num));

		return true;
	}

	public void RefreshMinigrid(MiniGrid grid)
	{
		grid.Size = new(PaneSize.X - (PaneSize.X * 0.1f), PaneSize.Y - (PaneSize.Y * 0.1f));
		
		grid.MiniGridRowIdx = grid.Row.OverallIndex;
		int i = 0; 
		foreach (MiniGriditem miniGriditem in grid.MinigridItems)
		{
			miniGriditem.QueueFree();
		}
		grid.MinigridItems.Clear();
		foreach (DataGridRow rowitem in grid.Row.RowData.SubRowItems)
		{
			MiniGriditem mgi = MinigridItemPS.Instantiate() as MiniGriditem;
			mgi.SelectButton.Pressed += () => MinigridItemPressed(grid, mgi);
			mgi.RowData = rowitem;
			grid.MinigridItems.Add(mgi);
			mgi.TextOne.Text = rowitem.SubrowTextFirst;
			mgi.TextTwo.Text = rowitem.SubrowTextSecond;
			mgi.MgiIndex = i;
			grid.ItemContainer.AddChild(mgi);		
			i++;		
		}
	}

    private void MinigridFilesSelected(string[] files, MiniGrid minigrid)
    {
		MiniGridItemAdd?.Invoke(minigrid.Row.RowData, minigrid.MiniGridRowIdx, files.ToList());        
    }


    private void MinigridFileSelected(string files, MiniGrid minigrid)
    {
		MiniGridItemAdd?.Invoke(minigrid.Row.RowData, minigrid.MiniGridRowIdx, new List<string>() {files});        
    }

    private void MinigridRemoveItemFromGroup(DataGridRow importedRowData, MiniGrid minigrid)
	{
		List<Guid> SelectedItems = new();
		if (minigrid.MinigridItems.Where(x => x.IsSelected).Any())
		{
			if (minigrid.MinigridItems.Where(x => x.IsSelected).Count() == 1)
			{
				SelectedItems.Add(minigrid.MinigridItems.Where(x => x.IsSelected).First().RowData.Identifier);
			} else
			{
				SelectedItems = minigrid.MinigridItems.Where(x => x.IsSelected).Select(v => v.RowData.Identifier).ToList();
			}

			MiniGridItemRemove?.Invoke(importedRowData, minigrid.MiniGridRowIdx, SelectedItems);
		}
		PleasePassLog(string.Format("Removing an item from a minigrid."));
	}

	private void MinigridAddItemToGroup(DataGridRow importedRowData, MiniGrid minigrid)
	{
		
		minigrid.addFiles.Visible = true;
		PleasePassLog(string.Format("Adding an item to a minigrid."));
	}

    private void MinigridItemPressed(MiniGrid minigrid, MiniGriditem mgi)
    {
		PleasePassLog(string.Format("Item {0} ({1}) selected in grid {2}", mgi.RowData.RowRef, mgi.MgiIndex, minigrid.Row.RowData.RowRef));
		if (ControlHeld)
		{
			mgi.IsSelected = true;
		} else
		{
			foreach (MiniGriditem item in minigrid.MinigridItems)
			{
				if (item != mgi)
				{
					item.IsSelected = false;
				} else
				{
					item.IsSelected = true;
				}
			}
		}
        
    }


    private void DisplaySubgrid(MiniGrid grid, DataGridRowUi row)
    {
        grid.Visible = true;
		BlockInput = true;
    }

	private void CloseMinigrid(MiniGrid minigrid)
	{
		minigrid.Visible = false;
		BlockInput = false;
	}

    public bool IsThereMultipleAdjustmentNumbers()
	{
		if (RowData.Count(x => x.Toggled) > 1)
		{
			return true;
		}
		return false;
	}

	private void InvokeDataChanged(DataGridRow row, string headerData, int headerIdx)
	{
		if (!Populating) DataChanged?.Invoke(row, headerData, headerIdx);
	}

    private void AdjustmentNumberAdjusted(DataGridRowUi x, int c)
	{
		int headeridx = RowData[x.OverallIndex].Items.IndexOf(RowData[x.OverallIndex].Items.Where(x => x.CellType == CellOptions.AdjustableNumber).First());
		RowData[x.OverallIndex].Items[headeridx].ItemContent = c.ToString();
		InvokeDataChanged(RowData[x.OverallIndex], Headers[headeridx].Data, headeridx);
	}


    private void RowWasEdited(DataGridRowUi r, DataGridCell c)
    {
        if (c.CellOptions == CellOptions.AdjustableNumber){
			//int an = RowData[0].Items.IndexOf(RowData[0].Items.Where(x => x.CellType == CellOptions.AdjustableNumber).First());
			
			List<DataGridRow> reorderlist = RowData;

			reorderlist = reorderlist.OrderBy(x => x.AdjustmentNumber).ToList();
			
			if(RowData.Where(x => x.AdjustmentNumber != -1).Any()){
				for (int i = 0; i < RowData.Count; i++){
					if (RowData[i].AdjustmentNumber == c.NumberContent){
						//TODO: what the fuck was this D:
					}
				}
			}
			
		}
    }

	private void AdjustableNumberEnabledAdd(DataGridRow row, bool remove){
		List<DataGridRow> reorderlist = [.. RowData.Where(x => x.Toggled)];

		if (remove){			
			row.AdjustmentNumber = -1;	
			RowData.Where(x => x.Identifier == row.Identifier).First().AdjustmentNumber = -1;
			reorderlist.Remove(RowData.Where(x => x.Identifier == row.Identifier).First());
			reorderlist = [.. reorderlist.OrderBy(x => x.AdjustmentNumber)];
			for (int c = 0; c < reorderlist.Count; c++) {
				reorderlist[c].AdjustmentNumber = c;
				RowData.Where(x => x.Identifier == reorderlist[c].Identifier).First().AdjustmentNumber = c;
				if (VisibleRows.Any(x => x.RowData.Identifier == reorderlist[c].Identifier)){
					VisibleRows.Where(x => x.RowData.Identifier == reorderlist[c].Identifier).First().AdjustmentNumber = c;
					UpdateRow(reorderlist[c]);
				}
			}
		} else {
			reorderlist = reorderlist.OrderBy(x => x.AdjustmentNumber).ToList();			
			row.AdjustmentNumber = reorderlist.Count-1;	
			if (VisibleRows.Any(x => x.RowData.Identifier == row.Identifier)){
				VisibleRows.Where(x => x.RowData.Identifier == row.Identifier).First().AdjustmentNumber = row.AdjustmentNumber;
				UpdateRow(row);
			}	
		}
				
	}

	private void AdjustableNumberOneNumberChanged(DataGridRowUi changed, int oldnum, int newnum)
	{
		/*List<DataGridRow> reorderlist = RowData;
		reorderlist = reorderlist.Where(x => x.AdjustmentNumber != -1).ToList();
		reorderlist = reorderlist.OrderBy(x => x.AdjustmentNumber).ToList();*/

		int cellidx = RowData[0].Items.IndexOf(RowData[0].Items.Where(x => x.CellType == CellOptions.AdjustableNumber).First());

		foreach (DataGridRow row in RowData)
		{
			PleasePassLog(string.Format("Row {0}: {1}", row.Idx, row.AdjustmentNumber));
		}

		if (RowData.Where(x => x.AdjustmentNumber == newnum).Any())
		{
			DataGridRow newnumrow = RowData.Where(x => x.AdjustmentNumber == oldnum).First();
			DataGridRow oldnumrow = RowData.Where(x => x.AdjustmentNumber == newnum).First();
			oldnumrow.AdjustmentNumber = oldnum;			
			newnumrow.AdjustmentNumber = newnum;
			VisibleRows.Where(x => x.OverallIndex == oldnumrow.Idx).First().AdjustmentNumber = oldnum;
			VisibleRows.Where(x => x.OverallIndex == newnumrow.Idx).First().AdjustmentNumber = newnum;
			InvokeDataChanged(oldnumrow, Headers[cellidx].Data, cellidx);
			InvokeDataChanged(newnumrow, Headers[cellidx].Data, cellidx);
		}		
	}

	private void AdjustReorderList(List<DataGridRow> reorderlist, int newnum, bool Up)
	{
		DataGridRow row = reorderlist.Where(x => x.AdjustmentNumber == newnum).First();
		if (Up)
		{
			row.AdjustmentNumber--;
			VisibleRows.Where(x => x.OverallIndex == row.Idx).First().AdjustmentNumber = row.AdjustmentNumber;
			if (reorderlist.Where(x => x.AdjustmentNumber == newnum).Any())
			{
				AdjustReorderList(reorderlist, newnum, true);
			}
		}
		else
		{
			row.AdjustmentNumber++;
			VisibleRows.Where(x => x.OverallIndex == row.Idx).First().AdjustmentNumber = row.AdjustmentNumber;
			if (reorderlist.Where(x => x.AdjustmentNumber == newnum).Any())
			{
				AdjustReorderList(reorderlist, newnum, false);
			}
		}
		
	}

	private void CheckToggleNumbers()
	{

		List<DataGridRow> reorderlist = RowData;

		reorderlist = reorderlist.Where(x => x.Toggled).ToList();
		reorderlist = reorderlist.OrderBy(x => x.AdjustmentNumber).ToList();

		if (reorderlist.Where(x => x.AdjustmentNumber != -1).Any())
		{
			int c = 0;
			List<DataGridRow> justminus = reorderlist.Where(x => x.AdjustmentNumber == -1).ToList();
			List<DataGridRow> alreadynumbered = reorderlist.Where(x => x.AdjustmentNumber != -1).ToList();
			reorderlist.Clear();
			foreach (DataGridRow r in alreadynumbered)
			{
				if (r.AdjustmentNumber != c)
				{
					r.AdjustmentNumber = c;
				}
				c++;
			}
			foreach (DataGridRow r in justminus)
			{
				r.AdjustmentNumber = c;
				c++;
			}
		}
		else
		{
			int c = 0;
			foreach (DataGridRow r in reorderlist)
			{
				r.AdjustmentNumber = c;
				c++;
			}
		}
	}

    


    private void ToggleFlipped(bool toggle, int rowIndex)
    {
		DataGridRow thisRow = RowData[rowIndex];
		int togglecell = thisRow.Items.IndexOf(thisRow.Items.Where(x => x.CellType == CellOptions.Toggle).First());
		thisRow.Items[togglecell].ItemContent = toggle.ToString();
		thisRow.Toggled = toggle;
		if (LinkToggleToAdjustmentNumber){
			if (toggle){
				AdjustableNumberEnabledAdd(thisRow, false);				
			} else {
				AdjustableNumberEnabledAdd(thisRow, true);
			}
		}
    }

	public bool AreMultipleRowsSelected(){
		if (RowData.Any(x => x.Selected)){
			return true;
		} else {
			return false;
		}
	}

	private void RowDataEdited(DataGridRow row, string i, int r, int c, int idx){
		int itemidx = row.Items.IndexOf(row.Items.First(x => x.ColumnNum == c));		
		InvokeDataChanged(row, Headers[itemidx].Data, itemidx);
	}

    private void RowSelected(int SelectedItem, bool Selected)
    {
		PleasePassLog(string.Format("Row selected: {0}", SelectedItem));
		if (!ShiftHeld && !ControlHeld){
			if (RowData[SelectedItem].Selected && RowData.Count(x => x.Selected) > 1){
				//int s = 0;
				for (int i = 0; i < RowData.Count; i++){
					if(i != SelectedItem){
						RowData[i].Selected = false;
						if (VisibleRows.Any(x => x.OverallIndex == i)){
							VisibleRows.First(x => x.OverallIndex == i).Selected = false;
						}
					} else {
						RowData[i].Selected = true;
						if (VisibleRows.Any(x => x.OverallIndex == i)){
							VisibleRows.First(x => x.OverallIndex == i).Selected = true;
						}
					}
				}
			} else {
				//int s = 0;
				for (int i = 0; i < RowData.Count; i++){
					if(i != SelectedItem){
						RowData[i].Selected = false;
						if (VisibleRows.Any(x => x.OverallIndex == i)){
							VisibleRows.First(x => x.OverallIndex == i).Selected = false;
						}
					} else {
						RowData[i].Selected = Selected;
						if (VisibleRows.Any(x => x.OverallIndex == i)){
							VisibleRows.First(x => x.OverallIndex == i).Selected = Selected;
						}
					}
				}
			}			
		} else if (ShiftHeld){
			if (RowData.Any(x => x.Selected)){
				if (RowData.Count(x => x.Selected) > 1){
					int LastItem = RowData.IndexOf(RowData.Last(x => x.Selected));
					int FirstItem = RowData.IndexOf(RowData.First(x => x.Selected));
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
				} else {
					int SingleSelected = RowData.IndexOf(RowData.Last(x => x.Selected));
					if (SingleSelected > SelectedItem){
						ChangeSelection(SelectedItem, SingleSelected, true);
					} else {
						ChangeSelection(SingleSelected, SelectedItem, true);
					}
				}
			} else {
				RowData[SelectedItem].Selected = true;
			}
		} else {
			RowData[SelectedItem].Selected = true;	
		}

		PreviousSelection = SelectedItem;
		if (RowData.Any(x => x.Selected)) {
			SelectedRows = RowData.Where(x => x.Selected).ToList();
		} else {
			SelectedRows = new();
		}		
    }

	private void ChangeSelection(int FirstRow, int LastRow, bool Selected){
		//int s = 0;
		for (int i = 0; i < RowData.Count; i++){
			if(i <= LastRow && i >= FirstRow){
				RowData[i].Selected = Selected;	
				if (VisibleRows.Any(x => x.OverallIndex == i)){
					VisibleRows.First(x => x.OverallIndex == i).Selected = Selected;
				}								
			} else {
				RowData[i].Selected = false;
				if (VisibleRows.Any(x => x.OverallIndex == i)){
					VisibleRows.First(x => x.OverallIndex == i).Selected = false;
				}
			}		
		}
	}

    private void WaitPopulateRows(){
		new Thread(() => {			
			if (RowsOnScreen == 0){
				WaitPopulateRows();
			} else {
				CallDeferred(nameof(PopulateRows));
			}			
		}){IsBackground = true}.Start();
	}

	private void GetPaneSizes()
	{
		if (PaneSize != Vector2.Zero)
		{
			//PaneSize = Size;
			PaneHeight = PaneSize.Y - HeaderY;
			if (Headers != null)
			{
				if (Headers.Where(x => x.CellType == CellOptions.Picture).Any()){
					DataGridHeader h = Headers.Where(x => x.CellType == CellOptions.Picture).First();
					RowHeight = (int)h.PictureCellSize.Y;
					if (h.ShowHeader){
						RowHeight = (int)h.PictureCellSize.Y;
					} else {
						RowHeight = DefaultRowHeight;
					}
				}
			}		
			ScrollBarHeight = (hScrollBar.GetParent() as HBoxContainer).Size.Y;
			PleasePassLog(string.Format("PaneSize: {0}, PaneHeight: {1}, RowHeight: {2}", PaneSize, PaneHeight, RowHeight));
			RowsOnScreen = PaneHeight / RowHeight;
			
			RowHeight = (int)((PaneHeight - HeaderY) / Math.Floor(RowsOnScreen));
			
			
			if (RowData != null && RowData.Count > 0) { 
				PopulateRows(); 
				SetScrollBar();
				PleasePassLog("Populating.");
			} else
			{
				PleasePassLog("Rowdata has no data.");
			}
		}		
	}

	

	private void SetScrollBar(){
		if (RowData == null){
			PleasePassLog(string.Format("Rowdata is null or 0 or there are fewer rows than max visible."));
			vScrollBar.Page = RowsOnScreen;
			vScrollBar.MaxValue = 0;
			vScrollBar.MinValue = 0;
		} else if (RowData.Count == 0 || RowsOnScreen >= RowData.Count)
		{
			PleasePassLog(string.Format("Rowdata is null or 0 or there are fewer rows than max visible."));
			vScrollBar.Page = RowsOnScreen;
			vScrollBar.MaxValue = 0;
			vScrollBar.MinValue = 0;		
		} else {		
			PleasePassLog(string.Format("Rows: {0}. Rows to show: {1}.", RowData.Count, (int)RowsOnScreen+1));	
			MaxScroll = RowData.Count;// - (int)RowsOnScreen + 1;
			PleasePassLog(string.Format("Max Scroll: {0}, RowData Count: {1}, RowsOnScreen: {2}", MaxScroll, RowData.Count, RowsOnScreen));
			int pages = (MaxScroll / (int)RowsOnScreen) * 2;
			int minpage = (int)RowsOnScreen;
			int pageunclamped = minpage - pages;
			double page = Math.Clamp(pageunclamped, 1, minpage);
			vScrollBar.Step = 1;
			vScrollBar.MaxValue = MaxScroll;
			vScrollBar.MinValue = 0;
			vScrollBar.Page = minpage;

			PleasePassLog(string.Format("Step: {0}, MaxVal: {1}, VPage: {2}, Page: {3}", vScrollBar.Step, vScrollBar.MaxValue, vScrollBar.Page, page));
		}
		//vScrollBar.Value = ScrollPosition;

		//grabber length is (page * sizeY) / (max - min)?
	}

	private void WriteConsole(string statement, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string filePath = ""){
		
		PleasePassLog(string.Format("[L{0}: {1}]: {2}", lineNumber, new FileInfo(filePath).Name, statement));
	}

	private void WriteConsoleDeferred(string statement){
		PleasePassLog(statement);
	}

	private void HeaderResized(int idx){
		DataGridHeaderCell header = HeaderRow.HeaderCells[idx] as DataGridHeaderCell;			
		foreach (DataGridRowUi row in VisibleRows){
			row.Cells[idx].SetSize(new(header.CellSize.X, RowHeight));
		}
	}

	private void HeaderSorted(int index, bool SortAscending, bool Reset){
		if (!Sorted) CellsUnsorted = RowData;
		if (Reset){
			RowData = CellsUnsorted;
		} else if (SortAscending){
			Sorted = true;
			if (index == AdjustmentCellIdx){
				List<DataGridRow> rowdataadj = RowData.Where(x => x.AdjustmentNumber != -1).ToList();
				List<DataGridRow> rowdatarest = RowData.Where(x => x.AdjustmentNumber == -1).ToList();
				rowdataadj = rowdataadj.OrderBy(x => x.AdjustmentNumber).ToList();
				List<DataGridRow> rowdatass = rowdataadj;
				rowdatass.AddRange(rowdatarest);
				RowData = rowdatass;
			} else {
				RowData = RowData.OrderBy(x => x.Items[index].ItemContent).ToList();
			}			
		} else {
			Sorted = true;
			if (index == AdjustmentCellIdx){
				List<DataGridRow> rowdataadj = RowData.Where(x => x.AdjustmentNumber != -1).ToList();
				List<DataGridRow> rowdatarest = RowData.Where(x => x.AdjustmentNumber == -1).ToList();
				rowdataadj = rowdataadj.OrderByDescending(x => x.AdjustmentNumber).ToList();
				List<DataGridRow> rowdatass = rowdataadj;
				rowdatass.AddRange(rowdatarest);
				RowData = rowdatass; 
			} else {
				RowData = RowData.OrderByDescending(x => x.Items[index].ItemContent).ToList();
			}			
		}
		}

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

	//MAYBE QUEUE IT AS TASKS SO IT DOES IT ONE AT A TIME?? 

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

	private void ReleaseScroll(){
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

    private void OnScrollDown()
    {		
		if (ScrollPosition != MaxScroll && ScrollPosition != MaxScroll - 1 && !VisibleRows.Any(x => x.RowData.Identifier == RowData[^1].Identifier))
		{
			vScrollBar.Value++;
		}
    }



    private void OnScrollUp()
    {
		if (!VisibleRows.Any(x => x.RowData.Identifier == RowData[0].Identifier))
		{			
			vScrollBar.Value--;
		}
    }


    private void OnScrollRightPressed()
    {
        hScrollBar.Value += HScrollStep;
		Math.Clamp(hScrollBar.Value, hScrollBar.MinValue, hScrollBar.MaxValue);
    }


    private void OnScrollLeftPressed()
    {
        hScrollBar.Value -= HScrollStep;
		Math.Clamp(hScrollBar.Value, hScrollBar.MinValue, hScrollBar.MaxValue);
    }

	

    private void GetThemeColors(){
		Backgrounds.AddRange(dataGridUi.ColorRects_BackgroundColors);
		AccentColors.AddRange(dataGridUi.ColorRects_AccentColors);	
		SecondaryColors.AddRange(dataGridUi.ColorRects_SecondaryColors);
	}

	private void SetThemeColors(){
		bool secondaryislight = false;
		bool bgislight = false;
		bool accentislight = false;
		if (AccentColor.Luminance < 0.5) accentislight = true;
		if (SecondaryColor.Luminance < 0.5) secondaryislight = true;
		if (BackgroundColor.Luminance < 0.5) bgislight = true;
		foreach (ColorRect colorr in Backgrounds){
			colorr.Color = BackgroundColor;
		}
		foreach (ColorRect accol in AccentColors){
			accol.Color = AccentColor;
		}
		foreach (ColorRect accol in HeaderAccentColors){
			accol.Color = AccentColor;
		}
		foreach (ColorRect seccol in SecondaryColors){
			seccol.Color = SecondaryColor;
		}


		StyleBoxFlat scrollstyle = vScrollBar.GetThemeStylebox("scroll") as StyleBoxFlat;
		scrollstyle.BgColor = SecondaryColor;
		vScrollBar.AddThemeStyleboxOverride("scroll", scrollstyle);
		hScrollBar.AddThemeStyleboxOverride("scroll", scrollstyle);
		/*Color focusedscrollcolor = SecondaryColor;
		if (SecondaryColor.V > 0.5){
			focusedscrollcolor = focusedscrollcolor.Darkened(0.2f);
		} else {
			focusedscrollcolor = focusedscrollcolor.Lightened(0.2f);
		}
		scrollstyle.BgColor = focusedscrollcolor;	*/
		
		vScrollBar.AddThemeStyleboxOverride("scroll_focus", scrollstyle);
		hScrollBar.AddThemeStyleboxOverride("scroll_focus", scrollstyle);
		
		
		
		
		StyleBoxFlat grabberV = vScrollBar.GetThemeStylebox("grabber") as StyleBoxFlat;
		StyleBoxFlat grabberH = hScrollBar.GetThemeStylebox("grabber") as StyleBoxFlat;
		Color grabbercolor = SecondaryColor;
		if (SecondaryColor.V > 0.5){
			grabbercolor = grabbercolor.Darkened(0.2f);
		} else {
			grabbercolor = grabbercolor.Lightened(0.2f);
		}
		grabberV.BgColor = grabbercolor;
		grabberH.BgColor = grabbercolor;

		vScrollBar.AddThemeStyleboxOverride("grabber", grabberV);
		hScrollBar.AddThemeStyleboxOverride("grabber", grabberH);

		if (SecondaryColor.V > 0.5){
			grabbercolor = grabbercolor.Darkened(0.1f);
		} else {
			grabbercolor = grabbercolor.Lightened(0.1f);
		}
		StyleBoxFlat grabberpressedV = vScrollBar.GetThemeStylebox("grabber_pressed") as StyleBoxFlat;
		StyleBoxFlat grabberpressedH = hScrollBar.GetThemeStylebox("grabber_pressed") as StyleBoxFlat;
		grabberpressedV.BgColor = grabbercolor;
		grabberpressedH.BgColor = grabbercolor;
		
		vScrollBar.AddThemeStyleboxOverride("grabber_pressed", grabberpressedV);
		hScrollBar.AddThemeStyleboxOverride("grabber_pressed", grabberpressedH);
		
		StyleBoxFlat grabberhighlightV = vScrollBar.GetThemeStylebox("grabber_highlight") as StyleBoxFlat;
		StyleBoxFlat grabberhighlightH = hScrollBar.GetThemeStylebox("grabber_highlight") as StyleBoxFlat;
		
		if (grabbercolor.V > 0.75){
			grabbercolor = grabbercolor.Darkened(0.7f);
		} else {
			grabbercolor = grabbercolor.Lightened(0.3f);
		}
		grabberhighlightV.BgColor = grabbercolor;
		grabberhighlightH.BgColor = grabbercolor;
		
		vScrollBar.AddThemeStyleboxOverride("grabber_highlight", grabberhighlightV);
		hScrollBar.AddThemeStyleboxOverride("grabber_highlight", grabberhighlightH);
		

		StyleBoxFlat scrollbuttonsnormal = dataGridUi.ScrollUpButton.GetThemeStylebox("normal") as StyleBoxFlat;
		scrollbuttonsnormal.BgColor = SecondaryColor;
		Color bordercol = SecondaryColor;
		if (SecondaryColor.Luminance > 0.5){
			bordercol = bordercol.Darkened(0.15f);
		} else {
			bordercol = bordercol.Lightened(0.15f);
		}		
		scrollbuttonsnormal.BorderColor = bordercol;
		dataGridUi.ScrollUpButton.AddThemeStyleboxOverride("normal", scrollbuttonsnormal);		
		dataGridUi.ScrollRightButton.AddThemeStyleboxOverride("normal", scrollbuttonsnormal);
		dataGridUi.ScrollLeftButton.AddThemeStyleboxOverride("normal", scrollbuttonsnormal);
		dataGridUi.ScrollDownButton.AddThemeStyleboxOverride("normal", scrollbuttonsnormal);


		StyleBoxFlat scrollbuttonshover = dataGridUi.ScrollUpButton.GetThemeStylebox("hover") as StyleBoxFlat;
		
		Color buttonhighlight = SecondaryColor;		
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
		dataGridUi.ScrollUpButton.AddThemeStyleboxOverride("hover", scrollbuttonshover);		
		dataGridUi.ScrollRightButton.AddThemeStyleboxOverride("hover", scrollbuttonshover);
		dataGridUi.ScrollLeftButton.AddThemeStyleboxOverride("hover", scrollbuttonshover);
		dataGridUi.ScrollDownButton.AddThemeStyleboxOverride("hover", scrollbuttonshover);

		StyleBoxFlat scrollbuttonspressed = dataGridUi.ScrollUpButton.GetThemeStylebox("pressed") as StyleBoxFlat;
		
		Color buttonpressed = SecondaryColor;		
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
		dataGridUi.ScrollUpButton.AddThemeStyleboxOverride("pressed", scrollbuttonspressed);		
		dataGridUi.ScrollRightButton.AddThemeStyleboxOverride("pressed", scrollbuttonspressed);
		dataGridUi.ScrollLeftButton.AddThemeStyleboxOverride("pressed", scrollbuttonspressed);
		dataGridUi.ScrollDownButton.AddThemeStyleboxOverride("pressed", scrollbuttonspressed);

		StyleBoxFlat scrollbuttondisabled = dataGridUi.ScrollUpButton.GetThemeStylebox("disabled") as StyleBoxFlat;
		
		Color buttonoff = SecondaryColor;		
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
		dataGridUi.ScrollUpButton.AddThemeStyleboxOverride("disabled", scrollbuttondisabled);		
		dataGridUi.ScrollRightButton.AddThemeStyleboxOverride("disabled", scrollbuttondisabled);
		dataGridUi.ScrollLeftButton.AddThemeStyleboxOverride("disabled", scrollbuttondisabled);
		dataGridUi.ScrollDownButton.AddThemeStyleboxOverride("disabled", scrollbuttondisabled);

		StyleBoxEmpty focusedtheme = dataGridUi.ScrollUpButton.GetThemeStylebox("focus") as StyleBoxEmpty;
		dataGridUi.ScrollUpButton.AddThemeStyleboxOverride("focus", focusedtheme);		
		dataGridUi.ScrollRightButton.AddThemeStyleboxOverride("focus", focusedtheme);		
		dataGridUi.ScrollLeftButton.AddThemeStyleboxOverride("focus", focusedtheme);		
		dataGridUi.ScrollDownButton.AddThemeStyleboxOverride("focus", focusedtheme);		
		if (MainFont == null) MainFont = DefaultFont;		
		foreach (Label label in HeaderLabels){
			label.AddThemeFontSizeOverride("font_size", DefaultHeaderTextSize * (HeaderTextSize/100));
			label.AddThemeFontOverride("font", MainFont);
		}
		
	}	

	public static Color GetFGColor(Color colorinput){
		float h;
		float s;
		float v;
		colorinput.ToHsv(out h, out s, out v);
		
		float newv;
		
		if (v > 0.5){
			newv = 0.05f; // bright colors - black font
		} else {
			newv = 0.95f; // dark colors - white font
		}

		Color newcolor = Color.FromHsv(h, s, newv);
		return newcolor;
	}

	public static object ProcessProperty(dynamic owner, string propName){
		try {
			return owner.GetType().GetProperty(propName).GetValue (owner, null);
		} catch {
			return null;
		}
	}

	public static dynamic GetProperty(dynamic owner, string propName){
		if (ProcessProperty(owner, propName) == null){
			return null;
		}
		var prop = ProcessProperty(owner, propName);
		if (prop.GetType() == typeof(string)){
			return prop.ToString();
		} else if (prop.GetType() == typeof(DateTime)){
			DateTime dt = (DateTime)prop;                
			return dt.ToString("MM/dd/yyyy H:mm");
		} else if (prop.GetType() == typeof(Guid) || prop.GetType() == typeof(int)){ 
			return prop.ToString();
		} else if (prop.GetType() == typeof(Enum)){
			return nameof(prop);
		} else if (prop.GetType() == typeof(bool)) { 
			return prop;
		} else {
			return prop;
		}
	}

    public override void _Process(double delta)
    {
		if (PaneSize == Vector2.Zero)
		{
			if (PaneSize != Size && Size != Vector2.Zero)
			{
				PaneSize = Size;
				GetPaneSizes();
			}
		}
			
		hScrollBar.MaxValue = ScrollContainerScrollBarH.MaxValue;
		hScrollBar.Step = ScrollContainerScrollBarH.Step;
		hScrollBar.MinValue = ScrollContainerScrollBarH.MinValue;
		hScrollBar.Page = ScrollContainerScrollBarH.Page;
		ScrollContainerScrollBarH.Value = hScrollBar.Value;
		HScrollStep = hScrollBar.MaxValue / VisibleHeaders;
		new Thread(() => {
			if (ListenForKeyPress) {
				if (ScrollUpHeld){
					ListenForKeyPress = false;
					CallDeferred(nameof(OnScrollUp));
					CallDeferred(nameof(ListenForHeldKeyTimer));
				} else if (ScrollDownHeld){
					ListenForKeyPress = false;
					CallDeferred(nameof(OnScrollDown));
					CallDeferred(nameof(ListenForHeldKeyTimer));
				}
			}
		}){IsBackground = true}.Start();

		if (!ScrollerTasks.IsEmpty){
			RunScrollerTasks();
		}
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

    public override void _Input(InputEvent @event)
    {
		if (!BlockInput)
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
					if (ListenForKeyPress && IsMouseInGrid())
					{
						if (PreviousSelection != 0){ 
							ListenForKeyPress = false;
							int currentSelection = PreviousSelection - 1;
							//CallDeferred(nameof(WriteConsoleDeferred), string.Format("Rows count: {0}", Rows.Count));
							//CallDeferred(nameof(WriteConsoleDeferred), string.Format("Prev selected: {0}: {1}", PreviousSelection, RowData[PreviousSelection].Items[2].ItemContent));
							//CallDeferred(nameof(WriteConsoleDeferred), string.Format("Curr selected: {0}: {1}", currentSelection, RowData[currentSelection].Items[2].ItemContent));
							
							if (VisibleRows.Any(x => x.OverallIndex == PreviousSelection)){
								VisibleRows.First(x => x.OverallIndex == PreviousSelection).RowSelectedButtonPressed(false);
							}							
							if (IsRowOnScreen(currentSelection)){
								VisibleRows.First(x => x.OverallIndex == currentSelection).RowSelectedButtonPressed(true);
								PreviousSelection = currentSelection;
							} else {
								//VisibleRows.First(x => x.OverallIndex == currentSelection).First().RowSelectedButtonPressed(true);
								CallDeferred(nameof(OnScrollUp));
								PreviousSelection = currentSelection;
							}
							CallDeferred(nameof(ListenForKeyPressTimer));
						}
					}
				} else if (keyboardKey.Keycode == Key.Down)
				{
					if (ListenForKeyPress && IsMouseInGrid())
					{
						if (PreviousSelection != RowData.Count -1){ 
							ListenForKeyPress = false;
							int currentSelection = PreviousSelection + 1;
							//CallDeferred(nameof(WriteConsoleDeferred), string.Format("Rows count: {0}", Rows.Count));
							//CallDeferred(nameof(WriteConsoleDeferred), string.Format("Prev selected: {0}: {1}", PreviousSelection, RowData[PreviousSelection].Items[2].ItemContent));
							//CallDeferred(nameof(WriteConsoleDeferred), string.Format("Curr selected: {0}: {1}", currentSelection, RowData[currentSelection].Items[2].ItemContent));
							
							if (VisibleRows.Any(x => x.OverallIndex == PreviousSelection)){
								VisibleRows.First(x => x.OverallIndex == PreviousSelection).RowSelectedButtonPressed(false);
							}	
							if (IsRowOnScreen(currentSelection)){
								VisibleRows.First(x => x.OverallIndex == currentSelection).RowSelectedButtonPressed(true);
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
					if (ListenForKeyPress && IsMouseInGrid() && ControlHeld)
					{
						ListenForKeyPress = false;
						foreach (DataGridRow row in RowData){
							row.Selected = true;
						}
						foreach (DataGridRowUi row in VisibleRows){
							row.Selected = true;
							RowData[row.OverallIndex].Selected = true;
						}
						CallDeferred(nameof(ListenForKeyPressTimer));
					}
				}
			}	



			if (FirstLoaded) {
				if (ListenForKeyPress && IsMouseInGrid()){ 
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
				if (@event is InputEventMouseButton mouseClick && mouseClick.Pressed){
					if (mouseClick.ButtonIndex == MouseButton.Right){
						if (IsMouseInHeaderBar() && !HeaderMenuShowing){
							foreach (DataGridRowUi row in VisibleRows){
								row.KillThumbnail(true);							
								//row.ImageCell.AllowThumbs = false;
							}
							AddHeaderMenu();
						}
					} else if (IsMouseInGrid())
					{
						
					}
				}
				if (@event is InputEventKey key && key.Pressed){
					if (key.Keycode == Key.Space){
						if (RowData.Where(x => x.Selected).Any()){
							bool toggle = false;
							if (RowData.Where(x => x.Selected).Count() == 1){		
								int idx = RowData.First(x => x.Selected).Idx;
								toggle = !RowData[idx].Toggled;											
								if (VisibleRows.Any(x => x.OverallIndex == idx)){
									VisibleRows.First(x => x.OverallIndex == idx).Toggle(toggle);
								}
								RowData[idx].Toggled = toggle;
								AdjustableNumberEnabledAdd(RowData[idx], !toggle);
							} else {
								List<DataGridRow> selectedRows = RowData.Where(x => x.Selected).ToList();
								List<DataGridRow> toggled = selectedRows.Where(x => x.Toggled).ToList();
								List<DataGridRow> nottoggled = selectedRows.Where(x => !x.Toggled).ToList();
								if (toggled.Count > 0){						
									if (toggled.Count > nottoggled.Count){
										toggle = false;
									} else {
										toggle = true;
									}
								} else {
									toggle = true;
								}
								for (int i = 0; i < RowData.Count; i++){
									if (RowData[i].Selected){
										RowData[i].Toggled = toggle;
										if (VisibleRows.Any(x => x.OverallIndex == i)) VisibleRows.First(x => x.OverallIndex == i).Toggle(toggle);
										AdjustableNumberEnabledAdd(RowData[i], !toggle);
									}						
								}
							}
							
						}
					}
					if (RowData.Where(x => x.Selected).Any()) SelectedRows = RowData.Where(x => x.Selected).ToList();
				}
			}    
		} else
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
				}
			}
		}

		if (@event is InputEventMouseButton button && button.IsReleased())
		{
			if (vScrollBar.Value != ScrollPosition)
			{
				PleasePassLog(string.Format("Scrollbar Value: {0}, ScrollPosition: {1}", vScrollBar.Value, ScrollPosition));
				OnScrollScrolled(vScrollBar.Value);
			}
		}
    }

	private void AddHeaderMenu(){
		if (HeaderMenuShowing){
			HeaderMenu?.QueueFree();
		}
		HeaderMenuShowing = true;
		HeaderMenu = HeaderMenuHolder.Instantiate() as HeaderClickMenuHolder;
		HeaderMenu.Position = GetGlobalMousePosition();
		Dictionary<string, bool> headerchoices = new();
		foreach (DataGridHeader h in Headers){
			if (string.IsNullOrEmpty(h.Title)){
				throw new DataGridHeadersIncorrectException("Header must contain a title.");
			}
			try {
				headerchoices.Add(h.Title, h.ShowHeader);
			} catch {
				throw new DataGridHeadersIncorrectException("Headers must not be duplicated.");
			}
		}
		HeaderMenu.HeaderOptions = headerchoices;
		HeaderMenu.TextColor = GetFGColor(MainRowColor);
		HeaderMenu.BGColor = BackgroundColor;
		HeaderMenu.AccentColor = AccentColor;
		HeaderMenu.AltColor = SecondaryColor;
		HeaderMenu.TextSize = DefaultMenuTextSize * (MenusTextSize/100);
		HeaderMenu.OptionClicked += (x, y) => HeaderOptionsChanged(x, y);
		HeaderMenu.CloseMenu += () => CloseHeaderMenu();
		//this.AddChild(HeaderMenu);
		MakeRCMenu?.Invoke(HeaderMenu);
	}

    private void CloseHeaderMenu()
    {
        HeaderMenu.QueueFree();
		HeaderMenuShowing = false;
		foreach (DataGridRowUi row in VisibleRows){
			row.KillThumbnail(false);
		}
    }


    private void HeaderOptionsChanged(string Title, bool Selected)
    {
		bool rowychanged = false;
		DataGridHeader thisheader = Headers.Where(x => x.Title == Title).First();
		thisheader.ShowHeader = Selected;
		if (thisheader.CellType == CellOptions.Picture){
			rowychanged = true;
		}
		/*foreach (DataGridHeader header in Headers){
			if (header.Title == Title){
				header.ShowHeader = Selected;
				if (header.CellType == CellOptions.Picture){
					
				}				
			}
		}*/
		CloseHeaderMenu();
		UpdateHeaderRow(true);
    }


	public void Search(string SearchTerm, string Header = ""){
		if (string.IsNullOrEmpty(SearchTerm) || string.IsNullOrWhiteSpace(SearchTerm)) { 
			if (Searched){
				ResetSearch();
			}
		} else if (Header == ""){
			if (!Searched){
				CellsUnsearched = RowData;
			}
			Searched = true;
			List<DataGridRow> searched = new();
			for (int i = 0; i < Headers.Count; i++) {
				if (Headers[i].CellType == CellOptions.Text || Headers[i].CellType == CellOptions.Int){
					if (CellsUnsearched.Where(x => x.Items[i].ItemContent.ToLower().Contains(SearchTerm.ToLower())).Any()){
						searched.AddRange(CellsUnsearched.Where(x => x.Items[i].ItemContent.ToLower().Contains(SearchTerm.ToLower())).ToList());
					}					
				}
			}
			searched = searched.Distinct().ToList();
			RowData = searched;
		} else {
			if (!Searched){
				CellsUnsearched = RowData;
			}
			List<DataGridRow> searched = new();
			int column = Headers.IndexOf(Headers.First(x => x.Title == Header));			
			if (CellsUnsearched.Any(x => x.Items[column].ItemContent.ToLower().Contains(SearchTerm.ToLower()))){
				searched = CellsUnsearched.Where(x => x.Items[column].ItemContent.ToLower().Contains(SearchTerm.ToLower())).ToList();
			}			
			RowData = searched;
			Searched = true;
		}
	}

	public void ResetSearch(){
		RowData = CellsUnsearched;
		Searched = false;
	}

	public void TooltipProduced(string t){
		MakeTooltip?.Invoke(t);
	}

	public void UpdateRowData(List<DataGridRow> rowData){
		RowData = rowData;
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

	
    private void ListenForHeldKeyTimer(){
		ScrollHeldTimer?.QueueFree();
		ScrollHeldTimer = new();
		AddChild(ScrollHeldTimer);
		ScrollHeldTimer.OneShot = true;
		ScrollHeldTimer.WaitTime = 0.01;
		ScrollHeldTimer.Timeout += () => ResetKeyPress();
		ScrollHeldTimer.Start();		
	}

    private void ResetKeyPress()
    {
        ListenForKeyPress = true;
    }

	private bool IsRowOnScreen(int idx){
		return VisibleRows.Any(x => x.OverallIndex == idx);
	}

	public bool IsMouseInGrid(){
		Vector2 mousepos = GetGlobalMousePosition(); 

		Rect2 rect = new (this.GlobalPosition, this.Size); 

		return rect.HasPoint(mousepos); 
	}
	private bool IsMouseInHeaderBar(){
		Vector2 mousepos = GetGlobalMousePosition(); 

		Rect2 rect = new (HeaderHolder.GlobalPosition, HeaderHolder.Size); 

		return rect.HasPoint(mousepos); 
	}
	private bool ToBool(string Input){
		if (Input.ToLower() == "true"){
			return true;
		} else {
			return false;
		}
	}
	
	private string DumpRows(){
		StringBuilder sb = new();
		foreach (DataGridRowUi row in VisibleRows){
			sb.AppendLine(string.Format("Row {0}:\n--- Selected: {1}\n--- Toggled: {2}", row.OverallIndex, row.Selected, row.Toggled));
		}
		return sb.ToString();
	}

	public static List<DataGridCellIcons> GetIcons(List<DataGridCellIcons> icons){
		List<DataGridCellIcons> newicons = new();
		foreach (DataGridCellIcons icon in icons){
			newicons.Add((DataGridCellIcons)icon.Clone());
		}
		return newicons;
	}

}
