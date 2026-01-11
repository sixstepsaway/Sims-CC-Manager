using DataGridContainers;
using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.SettingsSystem;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class AllModsContainer : MarginContainer
{
    public PackageDisplay packageDisplay;
    [Export]
    Label AllModsHeader;
    [Export]
    Label AllModsModNumber;
    [Export]
    LineEdit SearchBox;
    [Export]
    OptionButton SearchOptions;
    [Export]
    public DataGrid DataGrid;
    [Export]
    MarginContainer GridContainer;
    [Export]
    PackedScene RightClickMenuPS;
    [Export]
    PackedScene ThumbgridPS;
    [Export]
    CustomCheckButton SwapDisplaysCheck;
    ThumbnailGrid ThumbGrid;
    [Export]
    PackedScene RenameItemsBoxPS;
    [Export]
    PackedScene PackageListItemPS;

    public bool FirstLoaded = false;

    public bool ThumbDisplayLoaded = false;

    private bool _viewswap;
    public bool ViewSwap
    {
        get { return _viewswap; }
        set { _viewswap = value; 

            if (ThumbDisplayLoaded)
            {
                ThumbGrid.Visible = value;
                DataGrid.Visible = !value;
            } else if (value)
            {
                LoadThumbGrid();
            }
            
        }
    }

    

    public List<Category> Categories { get { return packageDisplay.ThisInstance.Categories; }}
    /*public List<SimsPackage> Packages { 
        get { 
            return packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList(); 
            }
    }*/

    public InstanceProfile CurrentProfile {get {return packageDisplay.ThisInstance.LoadedProfile; }}

    public List<SimsPackage> Packages { 
        get { return packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList(); }
    }

    
	private List<DataGridRow> _hiddenrows;
	public List<DataGridRow> HiddenRows
	{
		get { return _hiddenrows; }
		set { _hiddenrows = value; 
		DataGrid.HiddenRows = value; }
	}

    private List<DataGridRow> _selecteditems;

    public List<DataGridRow> SelectedItems { get { return _selecteditems; }
    set { _selecteditems = value; } }

    RightClickMenu rcm;

    RenameItemsBox rib;

    public List<string> Headers = new()
    {
        "IsEnabled",
        "FileName",
        "LoadOrder",
        "Location",
        "Image",
        "FileSize",
        "FileType",
        "",
        "DateUpdated",
        "DateAdded",
        "Game"
    };

    public List<DataGridHeader> DataGridHeaders = new() {
            new DataGridHeader() { 
                ContentType = DataGridContentType.Toggle,
                Data = "IsEnabled",
                Blank = true,
                Title = "Enabled",
                Resizeable = false,
                CellType = CellOptions.Toggle,
                ShowHeader = true},
			new DataGridHeader() { 
                ContentType = DataGridContentType.Text,
                StartingWidth = 150,
                Data = "FileName",
                Title = "Title",
                Resizeable = true,
                CellType = CellOptions.Text,
                ShowHeader = true,
                ContentEditable = true},
			new DataGridHeader() { 
                ContentType = DataGridContentType.Text,
                Data = "LoadOrder",
                Title = "Load Order",
                Resizeable = false,
                CellType = CellOptions.AdjustableNumber,
                ShowHeader = true,
                Blank = true},
			new DataGridHeader() { 
                ContentType = DataGridContentType.Text,
                StartingWidth = 150,
                Data = "Location",
                Title = "Location",
                Resizeable = true,
                CellType = CellOptions.Text,
                ShowHeader = true},
			new DataGridHeader() { 
                ContentType = DataGridContentType.Text,
                StartingWidth = 150,
                Data = "CategoryName",
                Title = "Category",
                Resizeable = true,
                CellType = CellOptions.Text,
                ShowHeader = true},
			new DataGridHeader() { 
                ContentType = DataGridContentType.Icons,
                Title = "Problems",
                Data = "",
                Resizeable = false,
                CellType = CellOptions.Icons,
                ShowHeader = true,
			    Blank = true},		
			new DataGridHeader() { 
                ContentType = DataGridContentType.Text,
                Data = "Image",
                Title = "Image",
                Resizeable = true,
                CellType = CellOptions.Picture,
                ShowHeader = false,
                PictureCellSize = new(100,
                115)},
			new DataGridHeader() { 
                ContentType = DataGridContentType.Text,
                Data = "FileSize",
                Title = "File Size",
                Resizeable = true,
                CellType = CellOptions.Text,
                ShowHeader = true},
			new DataGridHeader() { 
                ContentType = DataGridContentType.Text,
                StartingWidth = 100,
                Data = "FileType",
                Title = "File Type",
                Resizeable = true,
                CellType = CellOptions.Text,
                ShowHeader = true},	
			new DataGridHeader() { 
                ContentType = DataGridContentType.Date,
                Data = "DateUpdated",
                StartingWidth = 150,
                Title = "Date Modified",
                Resizeable = true,
                CellType = CellOptions.Text,
                ShowHeader = false},
			new DataGridHeader() { 
                ContentType = DataGridContentType.Date,
                Data = "DateAdded",
                StartingWidth = 150,
                Title = "Date Created",
                Resizeable = true,
                CellType = CellOptions.Text,
                ShowHeader = false},
			new DataGridHeader() { 
                ContentType = DataGridContentType.Text,
                Data = "Game",
                StartingWidth = 150,
                Title = "Game",
                Resizeable = true,
                CellType = CellOptions.Text,
                ShowHeader = false}
            };
	
    List<DataGridCellIcons> icons = new() {
        new() { TooltipMessage = "Root Mod", IconData = "RootMod", IconName = "Root", IconImage = GD.Load<Texture2D>("res://assets/icons/materialicons/twotone_build_circle_black_48dp.png")},
        new() { TooltipMessage = "Out of Date", IconData = "OutOfDate", IconName = "Out of Date", IconImage = GD.Load<Texture2D>("res://assets/icons/materialicons/twotone_cancel_black_48dp.png")},
        new() { TooltipMessage = "Orphan", IconData = "Orphan", IconName = "Orphan", IconImage = GD.Load<Texture2D>("res://assets/icons/materialicons/twotone_child_care_black_48dp.png")},
        new() { TooltipMessage = "Favorite", IconData = "Favorite", IconName = "Favorite", IconImage = GD.Load<Texture2D>("res://assets/icons/materialicons/twotone_favorite_black_48dp.png")},
        new() { TooltipMessage = "Broken", IconData = "Broken", IconName = "Broken", IconImage = GD.Load<Texture2D>("res://assets/icons/materialicons/twotone_broken_image_black_48dp.png")},
        new() { TooltipMessage = "Wrong Game", IconData = "WrongGame", IconName = "Wrong Game", IconImage = GD.Load<Texture2D>("res://assets/icons/materialicons/twotone_warning_black_48dp.png")}
    };

    List<DataGridHeaderCell> HeaderCells = new();


    private int _packagesnumber;
    public int PackagesNumber
    {
        get { return _packagesnumber; }
        set { _packagesnumber = value; 
        AllModsModNumber.Text = value.ToString(); }
    }

    public override void _Ready()
    {
        UpdateTheme();      
        DataGrid.LinkToggleToAdjustmentNumber = true;
        DataGrid.MiniGridItemAdd += ChangeLinkedItemsAdd;
        DataGrid.MiniGridItemRemove += ChangeLinkedItemsRemove;
        DataGrid.MakeRCMenu += (menu) => ShowRCMenu(menu); 
        DataGrid.DataGridFinishedFirstLoad += () => DataGridLoaded();
        DataGrid.HeadersChanged += (h) => HeadersChanged(h);

        SwapDisplaysCheck.CheckToggled += (v) => ViewFlipped(v);

        
        
        

    }

    private void LoadThumbGrid()
    {
        ThumbGrid = ThumbgridPS.Instantiate() as ThumbnailGrid;        
        ThumbGrid.BackgroundColor = GlobalVariables.LoadedTheme.BackgroundColor;
        ThumbGrid.ItemBackgroundColor = GlobalVariables.LoadedTheme.DataGridA;
        ThumbGrid.AccentColor = GlobalVariables.LoadedTheme.AccentColor;
        ThumbGrid.TextColor = GlobalVariables.LoadedTheme.MainTextColor;
        ThumbGrid.SelectedColor = GlobalVariables.LoadedTheme.DataGridSelected;
        ThumbGrid.packageDisplay = packageDisplay;        
        ThumbGrid.PaneSize = GridContainer.Size;
        GridContainer.AddChild(ThumbGrid);

        ThumbGrid.MakeItems();
        ThumbGrid.Visible = true;
        DataGrid.Visible = false;
    }

    private void HeadersChanged(List<DataGridHeader> h)
    {
        DataGridHeaders = h;
    }


    private void DataGridLoaded()
    {
        GlobalVariables.mainWindow.DataGridFinishedLoading();
        DataGrid.DataChanged += (rowIdx, dataChanged, Item) => DataChanged(rowIdx, dataChanged, Item);
    }

    private void ViewFlipped(bool v)
    {
        ViewSwap = v;
    }


    private void ShowRCMenu(HeaderClickMenuHolder menu)
    {
        packageDisplay.AddChild(menu);
    }


    private void ChangeLinkedItemsRemove(DataGridRow row, int idx, List<Guid> items)
    {
        int rowpackage = Packages.IndexOf(Packages.Where(x => x.Identifier == row.Identifier).First());
        int rowstart = DataGrid.RowData.Count;
        StringBuilder sb = new();
        foreach (Guid item in items)
        {
            if (item == items.Last())
            {
                sb.Append(item.ToString());
            } else
            {
                sb.Append(string.Format("{0}, ", item.ToString()));
            }
        }
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Removing {0} items ({1}) from row {2}", items.Count, sb.ToString(), row.RowRef));

        foreach (Guid item in items)
        {
            row.SubRowItems.Remove(row.SubRowItems.Where(x => x.Identifier == item).First());
            //SimsPackage package = Packages[rowpackage].LinkedPackages.Where(x => x.Identifier == item).First();
            SimsPackage package = Packages.Where(x => x.Identifier == item).First();
            
            if (package.IsDirectory)
            {                
                Packages[rowpackage].LinkedFolders.Remove(package.Location);
                Packages[rowpackage].LinkedPackageFolders.Remove(package);
            } else
            {
                Packages[rowpackage].LinkedFiles.Remove(package.Location);
                Packages[rowpackage].LinkedPackages.Remove(package);
            } 
            Packages[rowpackage].WriteXML();
            package.StandAlone = true; 
            package.WriteXML();
            int rows = DataGrid.RowData.Count;
            DataGridRow newrow = CreateRow(package, rows);
            DataGrid.RowData.Add(newrow);
        }
        DataGrid.AddNewRows(rowstart);
        DataGrid.UpdateRow(row);
    }

    private void ChangeLinkedItemsAdd(DataGridRow row, int idx, List<string> items)
    {
        int rowpackage = Packages.IndexOf(Packages.Where(x => x.Identifier == row.Identifier).First());
        

        foreach (string item in items)
        {
            SimsPackage package = new();
            FileInfo file = new(item);
            if (Packages.Where(x => x.Location == item).Any())
            {
                package = Packages.Where(x => x.Location == item).First();
                package.StandAlone = false;
                package.WriteXML();            
            } else
            {
                package = InstanceControllers.ReadPackage(item, packageDisplay.ThisInstance, file);
            }
            Packages[rowpackage].LinkedPackages.Add(package);
            Packages[rowpackage].LinkedFiles.Add(item);

            if (DataGrid.RowData.Where(x => x.Identifier == package.Identifier).Any())
            {
                DataGridRow rd = DataGrid.RowData.Where(x => x.Identifier == package.Identifier).First();
                DataGrid.RemoveRow(rd);
            }

            row.SubRowItems.Add(CreateRow(package, row.SubRowItems.Count));            
        }
        DataGrid.UpdateRow(row);
    }

    private void PackagesChanged()
    {
        //throw new NotImplementedException();
    }

    public void UpdateProfilePackages()
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Updating profile packages."));
        if (DataGrid.VisibleRows.Count != 0)
        { 
            new Thread(() => {

            List<EnabledPackages> enabled = packageDisplay.ThisInstance.LoadedProfile.EnabledPackages;
            List<Guid> IDs = enabled.Select(x => x.PackageIdentifier).ToList();

            Parallel.ForEach(DataGrid.VisibleRows, row =>
            {
                if (IDs.Contains(row.RowData.Identifier))
                {
                    int loadorder = enabled.Where(x => x.PackageIdentifier == row.RowData.Identifier).First().LoadOrder;
                    CallDeferred(nameof(ChangeRowInfo), row, true, loadorder);
                    /* row.Toggle(true);
                    row.RowData.Toggled = true;
                    row.AdjustmentNumber = loadorder;
                    row.RowData.AdjustmentNumber = loadorder;*/
                } else
                {
                    CallDeferred(nameof(ChangeRowInfo), row, false, -1);
                    /*row.Toggle(false);
                    row.RowData.Toggled = false;
                    row.AdjustmentNumber = -1;
                    row.RowData.AdjustmentNumber = -1;*/
                }
            });            
        }){IsBackground = true}.Start();

        }// else
            //CreateRows();
        
    }

    private void ChangeRowInfo(DataGridRowUi row, bool toggle, int num)
    {
        row.Toggle(toggle);
        row.RowData.Toggled = toggle;
        row.AdjustmentNumber = num;
        row.RowData.AdjustmentNumber = num;
    }

    public DataGridRow CreateSubRow(DataGridRow newrow, SimsPackage pack)
    {
        List<SimsPackage> subpacks = new();
        newrow.SubRow = true;
        newrow.Identifier = pack.Identifier;
        newrow.SubRowItems = new();
        if (pack.LinkedFiles.Count != 0)
        {
            foreach (string linkedfile in pack.LinkedFiles)
            {
                if (!subpacks.Where(x => x.Location == linkedfile).Any())
                {
                    FileInfo fi = new(linkedfile);
                    SimsPackage simsPackage = InstanceControllers.ReadPackage(linkedfile, packageDisplay.ThisInstance, fi);                    
                    pack.LinkedPackages.Add(simsPackage);
                    subpacks.Add(simsPackage); 
                }                
            }
        }
        if (pack.LinkedFolders.Count != 0)
        {
            foreach (string linkedfile in pack.LinkedFolders)
            {
                if (!subpacks.Where(x => x.Location == linkedfile).Any())
                {
                    DirectoryInfo fi = new(linkedfile);
                    SimsPackage simsPackage = InstanceControllers.ReadPackage(linkedfile, packageDisplay.ThisInstance, fi);                    
                    pack.LinkedPackages.Add(simsPackage);
                    subpacks.Add(simsPackage); 
                }                
            }
        }        
        int i = 0;
        foreach (SimsPackage package in subpacks)
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Creating {0} in row {1} subrow ({2})", package.FileName, i, pack.FileName));
            newrow.SubRowItems.Add(CreateRow(package, i));
            i++;
        }
        return newrow;
    }

    public DataGridRow CreateRow(SimsPackage pack, int i)
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Adding {0} to DataGrid", pack.FileName));
        DataGridRow newrow = new();      
        newrow.RowRef = pack.FileName;
        newrow.SubrowTextFirst = pack.FileName;
        newrow.SubrowTextSecond = pack.Location;
        newrow.Identifier = pack.Identifier;
        newrow.Headers = DataGridHeaders;     
        newrow.OverallIdx = i;  
        newrow.PopulatedIdx = i;      
        if (pack.IsEnabled){
            newrow.Toggled = true;
            newrow.AdjustmentNumber = pack.LoadOrder;
        }
        if (pack.RootMod)
        {
            newrow.SubRow = false;
        } else if (pack.FileType == FileTypes.Folder || pack.LinkedFiles.Count != 0 || pack.LinkedFolders.Count != 0)
        {
            newrow = CreateSubRow(newrow, pack);
        }

        
        //icons must be cloned for this to work, so this method is the best way of doing it
        newrow.RowIcons = DataGrid.GetIcons(icons);			
        int c = 0;
        for (int o = 0; o < newrow.RowIcons.Count; o++) {						
            newrow.RowIcons[o].IconVisible = DataGrid.GetProperty(pack, newrow.RowIcons[o].IconData);
        }
        foreach (DataGridHeader header in DataGridHeaders){				
            DataGridCellItem item = new() {RowNum = i, ColumnNum = c};
            item.CellType = header.CellType;
            item.ContentType = header.ContentType;
            if (header.NumberAsBytes) item.NumberAsBytes = true;
            if (header.Data == "Image")
            {
                string imagefile = pack.Location.Replace(".info", "");
                imagefile = string.Format("{0}.png", imagefile);
                if (File.Exists(imagefile))
                {
                    item.ItemContent = imagefile;
                }
            }
            else if (!string.IsNullOrEmpty(header.Data))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Finding data for {0}", header.Data));
                item.ItemContent = DataGrid.GetProperty(pack, header.Data).ToString();
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Got it! Data: {0}", DataGrid.GetProperty(pack, header.Data).ToString()));
            }
            item.ItemName = header.Data;
            c++;
            newrow.Items.Add(item);
        }

        if (pack.CategoryName != "Default")
        {
            newrow.UseCategoryColor = true;
            newrow.BackgroundColor = pack.PackageCategory.Background;
            newrow.TextColor = pack.PackageCategory.TextColor;
        }
        if (!FirstLoaded) GlobalVariables.mainWindow.IncrementLoadingScreen(1, string.Format("Creating data for {0}", pack.FileName), "All Mods: Making Data");
        return newrow;
    }

    private DataGridRow UpdateRow(DataGridRow newrow, SimsPackage pack, bool newsubrow = false)
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Updating {0} in DataGrid", pack.FileName));  
        newrow.RowRef = pack.FileName;
        newrow.SubrowTextFirst = pack.FileName;
        newrow.SubrowTextSecond = pack.Location;
        newrow.Identifier = pack.Identifier;
        newrow.Headers = DataGridHeaders;             
        if (pack.IsEnabled){
            newrow.Toggled = true;
            newrow.AdjustmentNumber = pack.LoadOrder;
        }

        if (pack.RootMod)
        {
            newrow.SubRow = false;
        } else if (pack.FileType == FileTypes.Folder || pack.LinkedFiles.Count != 0 || pack.LinkedFolders.Count != 0)
        {
            newrow.SubRow = true;
            newrow = CreateSubRow(newrow, pack);
        }
        //icons must be cloned for this to work, so this method is the best way of doing it
        newrow.RowIcons = DataGrid.GetIcons(icons);			
        int c = 0;
        for (int o = 0; o < newrow.RowIcons.Count; o++) {						
            newrow.RowIcons[o].IconVisible = DataGrid.GetProperty(pack, newrow.RowIcons[o].IconData);
        }
        foreach (DataGridHeader header in DataGridHeaders){				
            DataGridCellItem item = new() {RowNum = newrow.OverallIdx, ColumnNum = c};
            item.CellType = header.CellType;
            item.ContentType = header.ContentType;
            if (header.NumberAsBytes) item.NumberAsBytes = true;
            if (header.Data == "Image")
            {
                string imagefile = pack.Location.Replace(".info", "");
                imagefile = string.Format("{0}.png", imagefile);
                if (File.Exists(imagefile))
                {
                    item.ItemContent = imagefile;
                }
            }
            else if (!string.IsNullOrEmpty(header.Data))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Finding data for {0}", header.Data));
                item.ItemContent = DataGrid.GetProperty(pack, header.Data).ToString();
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Got it! Data: {0}", DataGrid.GetProperty(pack, header.Data).ToString()));
            }
            item.ItemName = header.Data;
            c++;
            newrow.Items.Add(item);
        }
        return newrow;
    }

    
    
    public void CreateRows()
    {
        ConcurrentBag<Task> runningTasks = new();
        int completedTasks = 0;
        ConcurrentBag<DataGridRow> rows = new();
        new Thread(() => {        
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Creating rows"));
            
            int i = 0;
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Packages: {0}", Packages.Count));
            foreach (SimsPackage pack in Packages.OrderBy(x => x.IsDirectory).ThenBy(x => x.FileName)){
                if (pack.StandAlone)
                {
                    if (!packageDisplay.HideCategoriesInGrid.Contains(pack.PackageCategory))
                    {
                        Task t = Task.Run( () => {
                            DataGridRow newrow = CreateRow(pack, i);
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Row {0}: {1}", i, pack.FileName));
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("-- Is Subrow: {0}\n -- Has Subitems: {1}", newrow.SubRow, newrow.SubRowItems.Count));
                            rows.Add(newrow);                        
                        });
                        i++;
                        runningTasks.Add(t);
                    }                     
                }            
            }

            while (runningTasks.Any(x => !x.IsCompleted)){
                if (completedTasks != runningTasks.Count(x => x.IsCompleted)) {
                    completedTasks = runningTasks.Count(x => x.IsCompleted);
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0} tasks in runningTasks, {1} completed", runningTasks.Count, completedTasks));
                }
            }

            if (rows.Count != 0) { 
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Adding {0} rows to grid.", rows.Count));
                List<DataGridRow> _rows = rows.OrderBy(x => x.RowRef).ToList();
                for (int r = 0; r < _rows.Count; r++){
                    _rows[r].OverallIdx = r;
                    _rows[r].PopulatedIdx = r;
                }
                DataGrid.RowData = _rows;
            }
            FirstLoaded = true;
        }){IsBackground = true}.Start();
    }

    

    public void CreateDataGrid()
    {   
        SetSearchOptions();
        DataGrid.MinigridPath = packageDisplay.ThisInstance.InstanceFolders.InstancePackagesFolder;
        SearchBox.TextSubmitted += (text) => SearchedMods(text);

        SCCMTheme theme = GlobalVariables.LoadedTheme;

        DataGrid.AccentColor = theme.AccentColor;
        DataGrid.BackgroundColor = theme.BackgroundColor;
        DataGrid.SecondaryColor = theme.DataGridB;
        DataGrid.MainRowColor = theme.DataGridA;
        DataGrid.AlternateRowColor = theme.DataGridB;
        DataGrid.SelectedRowColor = theme.DataGridSelected;
        DataGrid.Alert1RowColor = theme.AccentColor;
        DataGrid.Alert2RowColor = theme.AccentColor.Darkened(0.1f);
        DataGrid.Alert3RowColor = theme.AccentColor.Darkened(0.2f);

        DataGrid.Headers = DataGridHeaders;
        DataGrid.ItemSelected += (item) => ItemSelected(item);
		//DataGrid.DataChanged += (rowIdx, dataChanged, Item) => DataChanged(rowIdx, dataChanged, Item);
		DataGrid.SelectionChanged += (SelectedRows, SelectedRowsIdxs) => SelectionChanged(SelectedRows, SelectedRowsIdxs);
		DataGrid.MakeTooltip += (tooltip) => ShowTooltip(tooltip);
		DataGrid.MakeRCMenu += (headerMenu) => CreateHeaderClickMenu(headerMenu);
		DataGrid.LinkToggleToAdjustmentNumber = true;
        DataGrid.PassLog += (log) => LogPassed(log);
        //DataGrid.PaneSize = GridContainer.Size;
        CreateRows();
    }

    private void LogPassed(string log)
    {
        if (GlobalVariables.DebugMode) Logging.JustWriteLog(log);
    }


    private void CreateHeaderClickMenu(HeaderClickMenuHolder headermenu)
    {
        //throw new NotImplementedException();
    }


    private void ShowTooltip(string t)
    {
        //throw new NotImplementedException();
    }


    private void SelectionChanged(List<DataGridRow> SelectedRows, List<int> SelectedRowsIdxs)
    {
        if (SelectedRows.Count == 1)
        {
            DisplayPackageInfo(Packages.First(x => x.Identifier == SelectedRows[0].Identifier));
            packageDisplay.UIPackageViewerContainer.Visible = true;
        } else
        {
            packageDisplay.UIPackageViewerContainer.Visible = false;
        }
        SelectedItems = SelectedRows;
        StringBuilder sb = new();
        foreach (DataGridRow row in SelectedItems)
        {
            sb.Append(string.Format("{0}: {1}, ", row.OverallIdx, row.RowRef));
        }
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Rows selected: {0}", sb.ToString()));
    }

    private void DisplayPackageInfo(SimsPackage package)
    {
        packageDisplay.UIPackageViewerContainer.package = package;
    }


    private void DataChanged(DataGridRow rowIdx, string dataChanged, int Item)
    {
        if (Packages.Where(x => x.Identifier == rowIdx.Identifier).Any())
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Data changed: {0}, {1}. New value: {2}", dataChanged, Item, rowIdx.Items[Item].ItemContent));
            //SetProperty(pack, header.Data).ToString();
            
            SimsPackage package = Packages.Where(x => x.Identifier == rowIdx.Identifier).First();

            package.SetProperty(dataChanged, rowIdx.Items[Item].ItemContent);

            package.WriteXML();

            if (dataChanged == "IsEnabled")
            {
                if (rowIdx.Items[Item].ItemContent == "True")
                {
                    packageDisplay.ThisInstance.LoadedProfile.EnabledPackages.Add(new () { PackageIdentifier = package.Identifier, LoadOrder = 0, PackageLocation = package.Location, PackageName = package.FileName});
                } else
                {
                    EnabledPackages pa = packageDisplay.ThisInstance.LoadedProfile.EnabledPackages.Where(x => x.PackageIdentifier == package.Identifier).First();
                    packageDisplay.ThisInstance.LoadedProfile.RemoveEnabled(pa);
                }
                
            }
            if (dataChanged == "LoadOrder")
            {
                if (rowIdx.Items[Item].ItemContent == "-1")
                {
                    if (packageDisplay.ThisInstance.LoadedProfile.EnabledPackages.Where(x => x.PackageIdentifier == package.Identifier).Any()) packageDisplay.ThisInstance.LoadedProfile.EnabledPackages.Remove(packageDisplay.ThisInstance.LoadedProfile.EnabledPackages.Where(x => x.PackageIdentifier == package.Identifier).First());
                } else {
                    if (packageDisplay.ThisInstance.LoadedProfile.EnabledPackages.Where(x => x.PackageIdentifier == package.Identifier).Any())
                    {
                        packageDisplay.ThisInstance.LoadedProfile.EnabledPackages.Where(x => x.PackageIdentifier == package.Identifier).First().LoadOrder = int.Parse(rowIdx.Items[Item].ItemContent);
                    } else
                    {
                        packageDisplay.ThisInstance.LoadedProfile.EnabledPackages.Add(new () { PackageIdentifier = package.Identifier, LoadOrder = int.Parse(rowIdx.Items[Item].ItemContent), PackageLocation = package.Location, PackageName = package.FileName});
                    }
                }
                
            }
            packageDisplay.ThisInstance.WriteXML();
        } else
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Changed data doesn't exist?"));
        }
        
    }


    private void ItemSelected(List<DataGridRow> item)
    {
        //throw new NotImplementedException();
    }


    public void SetSearchOptions()
    {
        SearchOptions.Clear();
        SearchOptions.AddItem("All");
        foreach (DataGridHeader header in DataGridHeaders)
        {
            if (header.ContentType != DataGridContentType.Toggle && header.ContentType != DataGridContentType.Bool){
                SearchOptions.AddItem(header.Title);
            }
        }
        SearchOptions.Select(0);
    }

    private void SearchedMods(string text)
    {
        /*List<SimsPackage> package = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().Where(x => x.FileName.Contains(text)).ToList();
        DisplayedPackages.Clear();
        foreach (SimsPackage file in package)
        {
            DisplayedPackages.Add(file);
        }*/
        if (SearchOptions.Selected == 0)
        {
            DataGrid.Search(text);
        } else
        {
            DataGrid.Search(text, SearchOptions.GetItemText(SearchOptions.Selected));
        }
        
    }


    public void UpdateTheme()
    {
        SCCMTheme theme = GlobalVariables.LoadedTheme;
        bool textLight = false;       
        if (!theme.IsThemeLight()) DataGrid.DarkMode = true; else DataGrid.DarkMode = false;
        
        AllModsHeader.AddThemeColorOverride("font_color", theme.HeaderTextColor);
        AllModsModNumber.AddThemeColorOverride("font_color", theme.ButtonHover);
    }

    public override void _Input(InputEvent @event)
    {
        if (DataGrid.IsMouseInGrid())
        {
            if (@event is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Right && mouse.Pressed)
            {
                if (SelectedItems.Count != 0)
                {
                    SimsPackage package = Packages.First(x => x.Identifier == SelectedItems[0].Identifier);
                    rcm = RightClickMenuPS.Instantiate() as RightClickMenu;
                    if (SelectedItems.Count > 1) rcm.Plural = true;
                    rcm.CategorySelected += (c) => CategorySelected(c);
                    rcm.AllCategories = packageDisplay.ThisInstance.Categories;
                    rcm.packageDisplay = packageDisplay;
                    if (SelectedItems.Count == 1)
                    {
                        rcm.MostlyRoot = package.RootMod;
                        rcm.MostlyFave = package.Favorite;
                        rcm.MostlyUpdated = !package.OutOfDate;
                        if (package.Game != packageDisplay.ThisInstance.GameChoice)
                        {
                            rcm.MostlyWrongGame = true;
                        } else
                        {
                            rcm.MostlyWrongGame = false;
                        }
                        if (package.IsDirectory)
                        {
                            rcm.FolderSelected = true;
                        } else
                        {
                            rcm.FolderSelected = false;
                        }
                        
                    } else
                    {
                        List<SimsPackage> packages = new();
                        foreach (DataGridRow item in SelectedItems)
                        {
                            packages.Add(Packages.First(x => x.Identifier == item.Identifier));
                        }

                        rcm.FFromF.Visible = false;
                        if (packages.Count(x => x.RootMod) > packages.Count(x => x.RootMod == false))
                        {
                            rcm.MostlyRoot = true;
                        } else
                        {
                            rcm.MostlyRoot = false;
                        }
                        if (packages.Count(x => x.Favorite) > packages.Count(x => x.Favorite == false))
                        {
                            rcm.MostlyFave = true;
                        } else
                        {
                            rcm.MostlyFave = false;
                        }
                        if (packages.Count(x => x.OutOfDate) > packages.Count(x => x.OutOfDate == false))
                        {
                            rcm.MostlyUpdated = false;
                        } else
                        {
                            rcm.MostlyUpdated = true;
                        }
                        
                        if (packages.Count(x => x.Game == packageDisplay.ThisInstance.GameChoice) > packages.Count(x => x.Game != packageDisplay.ThisInstance.GameChoice))
                        {
                            rcm.MostlyWrongGame = false;
                        } else
                        {
                            rcm.MostlyWrongGame = true;
                        }
                        
                    }
                    rcm.ButtonPressed += (i) => RCMButtonPressed(i);



                    Vector2 mousePos = GetGlobalMousePosition();
                    rcm.GlobalPosition = mousePos;
                    
                    GlobalVariables.mainWindow.AddChild(rcm);
                    float x = 0;
                    float y = 0;
                    float rcmwidth = rcm.MainContainer.Size.X;
                    Rect2 rect2 = new(packageDisplay.GlobalPosition, packageDisplay.Size);
                    Rect2 rcmrect = new(rcm.Position, rcm.MainContainer.Size);
                    Vector2 bottomright = rcm.Position + rcm.MainContainer.Size; 

                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Windowrect: {0}", rect2.ToString()));
                    if (!rect2.HasPoint(bottomright))
                    {   
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("RCM not in window: {0}", bottomright));
                        Vector2 distance = bottomright - rect2.Size;
                        rcm.GlobalPosition = new(mousePos.X, mousePos.Y - distance.Y);
                    } else
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("RCM in window: {0}", bottomright));
                        rcm.GlobalPosition = mousePos;
                    } 



                    Vector2 edbottomright = rcm.EditDetails.GlobalPosition + rcm.EditDetails.Size; 
                    if (!rect2.HasPoint(edbottomright))
                    {   
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("RCM not in window: {0}", edbottomright));
                        Vector2 distance = edbottomright - rect2.Size;
                        rcm.EditDetails.GlobalPosition = new(rcm.EditDetails.GlobalPosition.X, rcm.EditDetails.GlobalPosition.Y - distance.Y);
                    }

                    
                    Vector2 cbottomright = rcm.CategoryOptions.GlobalPosition + rcm.CategoryOptions.Size; 
                    if (!rect2.HasPoint(cbottomright))
                    {   
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("RCM not in window: {0}", cbottomright));
                        Vector2 distance = cbottomright - rect2.Size;
                        rcm.CategoryOptions.GlobalPosition = new(rcm.CategoryOptions.GlobalPosition.X, rcm.CategoryOptions.GlobalPosition.Y - distance.Y);
                    }


                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mouse pos: {0}", mousePos));
                }
            }
        }
    }

    private void CategorySelected(Category c)
    {
        rcm.QueueFree();
        for (int i = 0; i < SelectedItems.Count; i++)
        {
            SimsPackage package = Packages.First(x => x.Identifier == SelectedItems[i].Identifier);
            package.PackageCategory = c;
            package.WriteXML();
            DataGridRow rowdata = DataGrid.RowData.First(x => x.Identifier == package.Identifier);
            rowdata.UseCategoryColor = true;
            rowdata.BackgroundColor = c.Background;
            rowdata.TextColor = c.TextColor;
            rowdata = CreateRow(package, rowdata.OverallIdx);
            DataGrid.UpdateRow(rowdata);
        }
    }


    private void ToggleRoot()
    {
        for (int i = 0; i < SelectedItems.Count; i++)
        {
            SimsPackage package = Packages.First(x => x.Identifier == SelectedItems[i].Identifier);
            
            if (Directory.Exists(package.Location))
            {
                package.RootMod = !rcm.MostlyRoot;
                if (package.RootMod)
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Changing package {0} to root. Searching {1} for info files to delete.", package.FileName, package.Location));
                    package.IsDirectory = false;
                    foreach (string info in Directory.EnumerateFiles(package.Location, "*.info", SearchOption.AllDirectories))
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Deleting {0}", info));
                        File.Delete(info);
                    }
                    package.LinkedFiles.Clear();
                    package.LinkedFolders.Clear();
                    package.LinkedPackageFolders.Clear();
                    package.LinkedPackages.Clear();
                    DataGridRow rowdata = DataGrid.RowData.First(x => x.Identifier == package.Identifier);
                    package.WriteXML();
                    rowdata = CreateRow(package, rowdata.OverallIdx);
                    DataGrid.UpdateRow(rowdata);
                    
                    
                } else
                {
                    SimsPackage pack = Packages.First(x => x.Identifier == SelectedItems[i].Identifier);
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Changing package {0} to not root.", pack.FileName));
                    if (Directory.Exists(pack.Location)) pack.IsDirectory = true;
                    pack = InstanceControllers.GetSubDirectoriesPackage(packageDisplay.ThisInstance, pack.Location, pack);
                    DataGridRow rowdata = DataGrid.RowData.First(x => x.Identifier == pack.Identifier);
                    pack.WriteXML();
                    rowdata = CreateRow(pack, rowdata.OverallIdx);
                    DataGrid.UpdateRow(rowdata);
                }
                
            } else
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Root mods should be folders."));
            }            
        }
    }

    private void RenameFiles()
    {
        packageDisplay.LockInput = true;
        rib = RenameItemsBoxPS.Instantiate() as RenameItemsBox;
        rib.CancelButton.Pressed += () => CloseRenamePane();
        rib.ConfirmButton.Pressed += () => RenameAllFiles();
        rib.UpdateTheme();
        foreach (DataGridRow row in SelectedItems)
        {
            SimsPackage package = Packages.First(x => x.Identifier == row.Identifier);
            PackageListItem pli = PackageListItemPS.Instantiate() as PackageListItem;
            pli.packageItem = package;
            pli.NameBox.Text = package.FileName;
            pli.GotNames += () => rib.CheckItems();
            rib.Items.Add(pli);
            rib.ItemContainer.AddChild(pli);            
        }
        packageDisplay.AddChild(rib);
    }

    private void RenameAllFiles()
    {        
        foreach (DataGridRow row in SelectedItems)
        {
            SimsPackage package = Packages.First(x => x.Identifier == row.Identifier);
            PackageListItem pli = rib.Items.First(x => x.packageItem == package);
            if (!pli.Warning)
            {
                string parent = "";
                string newname = "";
                string extension = "";
                if (package.IsDirectory)
                {
                    DirectoryInfo di = new(package.Location);
                    parent = di.Parent.FullName;
                } else
                {
                    FileInfo fi = new(package.Location);
                    parent = fi.DirectoryName;
                    extension = fi.Extension;
                }
                newname = string.Format("{0}{1}", pli.NameBox.Text, extension);
                package.FileName = newname;
                newname = Path.Combine(parent, newname);
                File.Delete(package.InfoFile);
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pretending to rename file {0} to {1}", package.Location, newname));
                File.Move(package.Location, newname);
                package.Location = newname;
                package.WriteXML();
                DataGridRow rowdata = DataGrid.RowData.First(x => x.Identifier == package.Identifier);
                rowdata = CreateRow(package, rowdata.OverallIdx);
                DataGrid.UpdateRow(rowdata);
            }            
        }
        packageDisplay.LockInput = false;
        rib.QueueFree();
    }


    private void CloseRenamePane()
    {
        packageDisplay.LockInput = false;
        rib.QueueFree();
    }


    private void ToggleOutOfDate()
    {
        for (int i = 0; i < SelectedItems.Count; i++)
        {
            SimsPackage package = Packages.First(x => x.Identifier == SelectedItems[i].Identifier);            
            package.OutOfDate = rcm.MostlyUpdated;            
            package.WriteXML();
            DataGridRow rowdata = DataGrid.RowData.First(x => x.Identifier == package.Identifier);
            rowdata = UpdateRow(rowdata, package);
            DataGrid.UpdateRow(rowdata);
        }
    }
    private void ToggleFave()
    {
        for (int i = 0; i < SelectedItems.Count; i++)
        {           
            SimsPackage package = Packages.First(x => x.Identifier == SelectedItems[i].Identifier);
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Marking item {0} as fave: {1}", i, package.FileName));
            package.Favorite = !rcm.MostlyFave;            
            package.WriteXML();
            DataGridRow rowdata = DataGrid.RowData.First(x => x.Identifier == package.Identifier);
            rowdata = UpdateRow(rowdata, package);
            DataGrid.UpdateRow(rowdata);
        }
    }

    private void RCMButtonPressed(int i)
    {
        switch (i)
        {
            case 0:
            //WrongGame
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pressed button {0}: WrongGame", i));
            break;

            case 1:
            ToggleOutOfDate();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pressed button {0}: UpdatedOOD", i));
            break;

            case 2:
            ToggleRoot();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pressed button {0}: MakeRoot", i));
            break;

            case 3:
            ToggleFave();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pressed button {0}: Fave", i));
            break;

            case 4:
            //FFromF
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pressed button {0}: FFromF", i));
            break;

            case 5:
            RenameFiles();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pressed button {0}: Rename", i));
            break;

            case 6:
            //Source
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pressed button {0}: Source", i));
            break;

            case 7:
            //Creator
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pressed button {0}: Creator", i));
            break;

            case 8:
            //Move
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pressed button {0}: Move", i));
            break;

            case 9:
            //Delete
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pressed button {0}: Delete", i));
            break;

            case 10:
            //Notes
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pressed button {0}: Notes", i));
            break;

            case 11:
            //Categories
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pressed button {0}: Categories", i));
            break;
            case 12:
            //edit details
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pressed button {0}: Edit details", i));
            break;
        }
        rcm.QueueFree();
    }
}
