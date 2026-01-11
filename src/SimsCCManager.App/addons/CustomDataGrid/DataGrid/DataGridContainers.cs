using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace DataGridContainers
{
    /// <summary>
    /// Class that holds the cell item of the data grid. Contains the row number, cell number, and the expected content. 
    /// </summary>
    public class DataGridCellItem
    {
        /// <summary>
        /// Information for a Data Grid Cell. 
        /// </summary>
        
        public int RowNum { get; set; }
        public int ColumnNum {get; set;}
        public string ItemName {get; set;}
        private string _itemcontent;
        public string ItemContent {
            get { return _itemcontent; }
            set { _itemcontent = value; 
            ItemContentChanged(value);}
        }

        private void ItemContentChanged(string value)
        {
            ItemChanged?.Invoke(value, RowNum, ColumnNum);
        }

        public delegate void ItemChangedEvent(string item, int row, int column);
        public ItemChangedEvent ItemChanged;

        public CellOptions CellType {get; set;}
        public DataGridContentType ContentType {get; set;}
        public List<DataGridCellIcons> IconOptions {get; set;} 
        public bool HasCustomChild {get; set;}  
        public PackedScene CustomCellChild {get; set;} //if the cell has a "custom" content - this is what will be put into it.      
        public bool NumberAsBytes { get; set; }
        
        public DataGridCellItem()
        {
            ItemName = "";
            ItemContent = "";
            IconOptions = new();
            CellType = CellOptions.Text;
            HasCustomChild = false;
            ContentType = DataGridContentType.Text;
        }
    }

    /// <summary>
    /// Class that holds the data for each row 
    /// </summary>

    public class DataGridRow {
        public string RowRef {get; set;}
        public Guid Identifier {get; set;}
        public int OverallIdx {get; set;}
        public int PopulatedIdx {get; set;}
        public List<DataGridHeader> Headers {get; set;}
        public bool SubRow {get; set;} = false;

        public string SubrowTextFirst {get; set;}
        public string SubrowTextSecond {get; set;}

        public List<DataGridRow> SubRowItems {get; set;} = new();

        private ObservableCollection<DataGridCellItem> _items;
        public ObservableCollection<DataGridCellItem> Items {
            get { return _items; }
            set { _items = value; 
                if (_items.Where(x => x.CellType == CellOptions.Toggle).Any()) {
                    ToggledIdx = _items.IndexOf(_items.Where(x => x.CellType == CellOptions.Toggle).First());
                }
                if (_items.Where(x => x.CellType == CellOptions.AdjustableNumber).Any()) {
                    AdjustmentNumber = _items.IndexOf(_items.Where(x => x.CellType == CellOptions.AdjustableNumber).First());
                }
            }
        }

        
        public bool Selected {get; set;}
        public List<DataGridCellIcons> RowIcons {get; set;} 

        private Color _backgroundcolor;
        public Color BackgroundColor {get; set;}

        public Color TextColor {get; set;}
        public bool UseCategoryColor {get; set;}

        private bool _toggled;
        public bool Toggled {
            get { return _toggled; }
            set { _toggled = value; 
                if (Items.Where(x => x.CellType == CellOptions.Toggle).Any()) {
                    if (Items.Count >= ToggledIdx && ToggledIdx != -1) Items[ToggledIdx].ItemContent = value.ToString();
                }
            }
        }       
        private int _adjustmentnumber;        

        public int AdjustmentNumber {
            get { return _adjustmentnumber; }
            set { _adjustmentnumber = value; 
                if (Items.Where(x => x.CellType == CellOptions.AdjustableNumber).Any()) {
                    if (Items.Count >= AdjustmentIdx && AdjustmentIdx != -1) Items[AdjustmentIdx].ItemContent = value.ToString();
                    int idx = Items.IndexOf(Items.Where(x => x.CellType == CellOptions.AdjustableNumber).First());
                    Items[idx].ItemContent = AdjustmentNumber.ToString();
                }
            }
        }
        public int AdjustmentIdx {get; set;} = -1;
        public int ToggledIdx {get; set;} = -1;

        public DataGridRow(){
            Headers = new();
            Items = new();
            Selected = false;
            RowIcons = new();
            AdjustmentNumber = -1;
            OverallIdx = -1;
            Items.CollectionChanged += (x, y) => ItemsChanged(x, y);
        }

        private void ItemsChanged(object x, NotifyCollectionChangedEventArgs y)
        {
            foreach (DataGridCellItem item in y.NewItems){                
                item.ItemChanged += (i, r, c) => ItemChangedReturn(i, r, c);
            }
        }

        private void ItemChangedReturn(string i, int r, int c)
        {
            ItemsWereChanged?.Invoke(this, i, r, c, OverallIdx);
        }

        public delegate void ItemsWereChangedEvent (DataGridRow row, string i, int r, int c, int idx);
        public ItemsWereChangedEvent ItemsWereChanged;

        public static Godot.Color GetFGColor(Godot.Color colorinput){
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

            Godot.Color newcolor = Godot.Color.FromHsv(h, s, newv);
            return newcolor;
        }
    }

    /// <summary>
    /// Class that holds the data for the header cells
    /// </summary>

    public class DataGridHeader {
        public string Title {get; set;}
        public string Data {get; set;}
        public bool Blank {get; set;}
        public bool Resizeable {get; set;}
        public int StartingWidth {get; set;}
        public DataGridContentType ContentType {get; set;} 
        public CellOptions CellType {get; set;}
        public bool ShowHeader {get; set;} = false;
        public bool ContentEditable {get; set;} = false; 
        public Godot.Vector2 PictureCellSize {get; set;} = new(0, 0);  
        public bool NumAdjusterAllowDuplicates {get; set;} = false;
        public bool NumberAsBytes { get; set; } = false;
        public DataGridHeader()
        {
            Title = "";
            Blank = false;
            Resizeable = false;
            ContentType = DataGridContentType.Null;
            CellType = CellOptions.Text;
        }
        
    }

    /// <summary>
    /// Class that holds the information on the individual categories for the rows
    /// </summary>

    public class DataGridCategory {
        public string Name {get; set;} = "";
        public Guid Identifier {get; set;} = Guid.NewGuid();
        public string Description {get; set;} = "";
        public Godot.Color Background {get; set;} = Godot.Color.FromHtml("FFFFFF");
        public Godot.Color TextColor {get; set;} = Godot.Color.FromHtml("000000");
        public int Packages {get; set;} = 0;
    }

    public class DataGridCellIcons : ICloneable {
        public string IconName {get; set;} = "";
        public string IconData {get; set;} = "";
        public int IconSize {get; set;} =  15;
        public bool IconVisible {get; set;} = false;
        public int IconIndex {get; set;} = -1;
        public Texture2D IconImage {get; set;}
        public string TooltipMessage {get; set;}
        
        public static string ListToString(List<DataGridCellIcons> list)
        {
            StringBuilder sb = new();
            foreach (DataGridCellIcons icon in list){
                sb.AppendLine(string.Format("{0}: {1} - {2}", icon.IconName, icon.IconData, icon.IconVisible));
            }
            return sb.ToString();
        }

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion


    }

    public enum CellOptions {
        /// <summary>
        /// Options for the data grid cell. 
        /// </summary>
        Text,
        Icons,
        TrueFalse,
        Toggle,
        Int,
        Picture,
        AdjustableNumber,
        Custom
    }

    public enum SortingOptions {
        /// <summary>
        /// Options for sorting the data grid. 
        /// </summary>
        NotSorted,
        Ascending,
        Descending
    }
    public enum DataGridContentType {
        /// <summary>
        /// Content types for the data grid cell. I'm not sure why I have two of these.
        /// </summary>
        Null,
        Name,
        Icons,
        Date,
        Toggle, 
        Text,
        Bool,
        Int
    }
}