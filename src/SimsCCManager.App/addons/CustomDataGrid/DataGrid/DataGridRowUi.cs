using DataGridContainers;
using Godot;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

public partial class DataGridRowUi : MarginContainer
{
	/// <summary>
	/// The data grid row as displayed in the data grid.
	/// </summary>
	/// 
	
	public List<DataGridRowHeaders> RowHeaders = new();
	
	public delegate void RowSelectedEvent(int RowIdx, bool selected);
	public RowSelectedEvent RowSelected;
	public delegate void RowEditedEvent(DataGridRowUi thisRow, DataGridCell cell);
	public RowEditedEvent RowEdited;
	public delegate void RowNumAdjustingEvent(DataGridRowUi row);
	public RowNumAdjustingEvent NumBeingAdjusted;
	public delegate void AdjustmentNumberChangedEvent(DataGridRowUi row, int oldnum, int newnum);
	public AdjustmentNumberChangedEvent AdjustmentNumberChanged;
	//public delegate void AdjustmentNumberChangedPropagateEvent(DataGridRowUi row, int AdjNum);
	//public AdjustmentNumberChangedPropagateEvent AdjustmentNumberChangedPropagate;
	public delegate void CellToggledEvent(bool toggle, DataGridRowUi row);
	public CellToggledEvent CellToggled;
	[Export]	
	public HBoxContainer RowsHolder;
	[Export]
	public Button RowSelectionButton;
	[Export]
	public VBoxContainer SubRowContainer;
	[Export]
	public MarginContainer SubRowOuterContainer;
	public List<DataGridCell> Cells = new(); 
	public Color BackgroundColor;
	public Color SelectedColor;
	public Color SelectedTextColor;

	public bool DontAnnounceEdit { get { return Datagrid.DontAnnounceEdit; } set { Datagrid.DontAnnounceEdit = value; }}
	private bool _selected;
	public bool Selected
	{
		get { return _selected; }
		set { _selected = value; 
			CallDeferred(nameof(ChangeSelected));
		}
	}

	public void ChangeSelected()
	{
		if (Selected) {
			ColorHolder.Color = SelectedColor;
			foreach (DataGridCell cell in Cells)
			{
				cell.FontColor = SelectedTextColor;
			}
		} else {
			ColorHolder.Color = BackgroundColor;
			foreach (DataGridCell cell in Cells)
			{
				cell.FontColor = TextColor;
			}
		}
	}


	public delegate void ShowSubgridEvent(MiniGrid grid, DataGridRowUi row);
	public ShowSubgridEvent ShowSubgrid;

	public bool ClickToggle = false;
	public Color TextColor;
	public int TextSize;
	public int OverallIndex;
	public int PopulatedIndex;
	public Font MainFont;
	public DataGridCell ToggleCell;
	public DataGridCell ImageCell;
	public DataGridCell AdjustmentNumberCell;
	public string ToggleData = string.Empty;
	private bool _toggled;
	public bool Toggled
	{
		get { return ToggleCell.Toggled; }
		set { ToggleCell.Toggled = value; 
		}
	}
	bool ToggleButtonHovered = false;
	bool AdjustableNumberButtonHovered = false;
	public DataGrid Datagrid;
	private bool _adjustingnum; 
	public bool AdjustingNum
	{
		get { return _adjustingnum; }
		set { _adjustingnum = value; 
			RowSelectionButton.Visible = !value;
		}
	}
	private DataGridCell _numadjustmentcell;
	public DataGridCell NumAdjustmentCell
	{
		get { return _numadjustmentcell; }
		set { _numadjustmentcell = value; 
				
			}
	}
	private int _adjustmentnumber;
	public int AdjustmentNumber
	{
		get { return AdjustmentNumberCell.NumberContent; }
		set
		{			
			int oldv = _adjustmentnumber;
			_adjustmentnumber = value;
			AdjustmentNumberCell.NumberContent = value;
			if (!DontAnnounceEdit && !ToggledToo) AdjustmentNumberChanged?.Invoke(this, oldv, value);			
		}
	}
	public bool AdjustNumberOnlyIfToggled;

	[Export]
	ColorRect ColorHolder;

	public bool IsSubGrid = false;

	public List<DataGridRow> SubGridItems = new();

	public MiniGrid SubGrid;


	private  DataGridRow _rowdata;
	public  DataGridRow RowData 
	{
		get { return  _rowdata; }
		set {  _rowdata = value; 
		CallDeferred(nameof(ChangeRowName));}
	}

	private void ChangeRowName()
	{
		Name = RowData.RowRef;
	}

	public bool VisibleOnScreen = false;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (RowSelectionButton != null) RowSelectionButton.Pressed += () => RowSelectedButtonPressed();

	}

	

	public void RowSelectedButtonPressed(bool SelectedToggle = false)
    {
		if (!ToggleButtonHovered && !AdjustableNumberButtonHovered){
			if (SelectedToggle){
				Selected = true;
				RowSelected?.Invoke(RowData.PopulatedIdx, Selected);
				if (IsSubGrid && SubGrid != null)
				{
					if (!Datagrid.AreMultipleRowsSelected()) ShowSubgrid?.Invoke(SubGrid, this);
				}
			} else {
				Selected = !Selected;
				RowSelected?.Invoke(RowData.PopulatedIdx, Selected);
			}			
		} else if (ToggleButtonHovered)
		{
			Datagrid.PleasePassLog(string.Format("Setting row {0} toggle to {1}", PopulatedIndex, !Toggled));
			Toggle(!Toggled);			
		}
    }

	bool ToggledToo = false;

	public void Toggle(bool WhichWay){
		ToggledToo = true;
		Toggled = WhichWay;
		Datagrid.PleasePassLog(string.Format("Row {0} toggled as: {1}", PopulatedIndex, Toggled));
		RowData.Toggled = Toggled;
		
		if (!WhichWay && AdjustNumberOnlyIfToggled) 
		{
			AdjustmentNumberCell.NumberContent = -1; 
		} else { 
			CheckCellNumber();
		}
		ToggledToo = false;
		if (!DontAnnounceEdit) RowEdited?.Invoke(this, ToggleCell);	
	} 

	public void CheckCellNumber()
	{
		if (AdjustNumberOnlyIfToggled)
		{			
			if (!Toggled)
			{
				AdjustmentNumber = -1;
			} else
			{
				int max = Datagrid.RowData.Max(x => x.AdjustmentNumber);
				AdjustmentNumber = max + 1;
			}			
		}
	}

	public void KillThumbnail(bool BlockThumbs)
	{
		if (BlockThumbs)
		{
			if (ImageCell != null) ImageCell.AllowThumbs = false;
		} else
		{
			if (ImageCell != null) ImageCell.AllowThumbs = true;
		}
		ImageCell?.KillThumbnail();
	}

	

    public void AddCell(DataGridCell cell){
		cell.FontColor = TextColor;
		cell.FontSize = TextSize;
		cell.ToggleHovered += (h) => ToggleHovered(h);
		cell.AdjustableNumberHovered += (h) => AdjustableNumberHovered(h);
		cell.NumAdjusted += (u, b) => NumberAdjusted(u, b);
		Cells.Add(cell);
		RowsHolder.AddChild(cell);
	}

	public void ClearCells()
	{
		foreach (DataGridCell cell in Cells)
		{
			cell.QueueFree();
		}
		Cells.Clear();
	}

	public void SetCellSettings()
	{
		if (AdjustmentNumberCell != null) {
			if (AdjustmentNumberCell.ToggleLinked) {
				if (AdjustmentNumber == -1) AdjustmentNumberCell.NumberHolder.Text = string.Empty;
			}
		}
	}

    private void NumberAdjusted(int u, bool solo)
    {
		//int oldnum = AdjustmentNumber;
		AdjustmentNumber = u;
		//AdjustmentNumberChanged.Invoke(this, oldnum, u);
    }


    private void NumAdjusting(bool Adjusting)
    {
		if (AdjustNumberOnlyIfToggled){
			if (Toggled){
				if (Adjusting){
					AdjustingNum = true;
				} else {
					AdjustingNum = false; 
					RowData.AdjustmentNumber = AdjustmentNumberCell.NumberContent;
					if (!DontAnnounceEdit) RowEdited?.Invoke(this, AdjustmentNumberCell);
				}
			}	
		} else {
			if (Adjusting){
				AdjustingNum = true;
			} else {
				AdjustingNum = false; 
				RowData.AdjustmentNumber = AdjustmentNumberCell.NumberContent;
				if (!DontAnnounceEdit) RowEdited?.Invoke(this, AdjustmentNumberCell);
			}
		}        
    }

	public void NumAdjustingOff(){
		NumAdjustmentCell.ShowNumberAdjusterControls = false;
	}

    private void ToggleHovered(bool h)
    {
        if (h){
			ToggleButtonHovered = true;
		} else {
			ToggleButtonHovered = false;
		}
    }
    private void AdjustableNumberHovered(bool h)
    {
        if (h){
			AdjustableNumberButtonHovered = true;
		} else {
			AdjustableNumberButtonHovered = false;
		}
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton click)
		{
			if (click.ButtonIndex == MouseButton.Left && click.DoubleClick && AdjustableNumberButtonHovered)
			{
				if (AdjustNumberOnlyIfToggled && Toggled)
				{
					AdjustmentNumberCell.ShowNumberAdjusterControls = true;
					NumAdjusting(true);
					//ShowNumberAdjuster?.Invoke(ShowNumberAdjusterControls);
				}
				else if (!AdjustNumberOnlyIfToggled)
				{
					AdjustmentNumberCell.ShowNumberAdjusterControls = true;
					NumAdjusting(true);
					//ShowNumberAdjuster?.Invoke(ShowNumberAdjusterControls);
				}
			} else if (AdjustingNum && click.ButtonIndex == MouseButton.Left && click.Pressed && !AdjustableNumberButtonHovered)
			{
				if (int.Parse(AdjustmentNumberCell.NumberHolder.Text) != AdjustmentNumberCell.NumberContent)
				{
					if (!Datagrid.IsThereMultipleAdjustmentNumbers())
					{
						AdjustmentNumberCell.NumberContent = int.Parse(AdjustmentNumberCell.NumberHolder.Text);
						AdjustmentNumberCell.NumAdjusted(int.Parse(AdjustmentNumberCell.NumberHolder.Text), true);
					}
					else
					{
						//NumberContent = int.Parse(NumberHolder.Text);
						AdjustmentNumberCell.NumAdjusted(int.Parse(AdjustmentNumberCell.NumberHolder.Text), false);
					}
					
				}
				AdjustmentNumberCell.ShowNumberAdjusterControls = false;
				NumAdjusting(false);
			}			
		}
    }

}

public class DataGridRowHeaders
{
	public DataGridHeader Header {get; set;}
	public DataGridCell Cell {get; set;}
}