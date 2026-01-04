using Godot;
using MoreLinq;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.OptionLists;
using SimsCCManager.PackageReaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

public partial class ThumbnailGrid : MarginContainer
{
    public PackageDisplay packageDisplay;
    [Export]
    GridContainer ThumbGrid;
    [Export]
    ColorRect BGColorRect;
    [Export]
    PackedScene ThumbnailGridItemPS;
    [Export]
    PackedScene SnapshotterPS;

    private Color _selectedcolor;
    public Color SelectedColor 
    { get { return _selectedcolor; }
    set { _selectedcolor = value; }}



    private Color _accentcolor;
    public Color AccentColor 
    { get { return _accentcolor; }
    set { _accentcolor = value; }}

    private Color _textcolor;
    public Color TextColor 
    { get { return _textcolor; }
    set { _textcolor = value; }}

    private Color _backgroundcolor;
    public Color BackgroundColor 
    { get { return _backgroundcolor; }
    set { _backgroundcolor = value; 
    BGColorRect.Color = value; }}
    
    public Color ItemBackgroundColor;

    List<ThumbnailGridItem> Items = new();

    Vector2 ItemSize = new(100,120);
    Vector2 PaneSize = new(0, 0);

    bool ShiftHeld = false;
    bool ControlHeld = false;
    int LastSelected = -1;

    int RowCount = 5;
    int ColumnCount = 5;
    int ItemCount { get { return RowCount * ColumnCount; }}

    List<SimsPackage> packages = new();    

    private void GetSettings()
    {
        double itemCount = Math.Floor(Size.X / ItemSize.X);
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0} items will fit in the grid.", itemCount));

        float itemWidth = (float)(Size.X / itemCount);
        float scale = ItemSize.X / itemWidth; 
        ItemSize = new(ItemSize.X * scale, ItemSize.Y * scale);

        if (Items.Count > 0)
        {
            foreach (ThumbnailGridItem tgi in Items)
            {
                tgi.CustomMinimumSize = ItemSize;
            }
        }        
        //ThumbGrid.Columns = (int)itemCount;
        //ColumnCount = ThumbGrid.Columns;
        RowCount = (int)Math.Ceiling(Size.Y / ItemSize.Y);  
        MakeItemsVisible();      
    }

    public void MakeItemList()
    {
        packages = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList();
        foreach (SimsPackage package in packages)
        {
            if (package.PackageData != null && package.Game == packageDisplay.ThisInstance.GameChoice)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Creating thumbitem for {0}", package.FileName));
                ThumbnailGridItem tgi = ThumbnailGridItemPS.Instantiate() as ThumbnailGridItem;
                tgi.PackageReference = package;
                tgi.SelectedColor = SelectedColor;
                tgi.TextColor = TextColor;
                tgi.AccentColor = AccentColor;
                tgi.BackgroundColor = ItemBackgroundColor;
                tgi.LabelText = package.FileName;
                tgi.ItemSelected += (x, y, z) => GridItemSelectionChanged(x, y, z);
                tgi.CustomMinimumSize = ItemSize;
                if (package.Mesh || package.Recolor)
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Thumbitem {0} is a mesh or a recolor", package.FileName));
                    tgi.ViewportVersion = true;
                    tgi = MakeViewportItem(tgi, package);
                } else if (package.Sims2Data.TXTRDataBlock.Count > 0)
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Thumbitem {0} is just a texture", package.FileName));
                    tgi.ViewportVersion = false;
                    tgi = MakeTextureItem(tgi, package);
                } else
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Thumbitem {0} uses a placeholder image", package.FileName));
                    switch (package.Game)
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
                Items.Add(tgi);
            }            
        }
        if (Items.Count != 0) MakeItemsVisible();
    }

    public void MakeItems()
    {
        //if (PaneSize == Vector2.Zero)
        //{
        //    WaitMakeItems();
        //} else
        //{
            MakeItemList();
        //}        
    }

    private void WaitMakeItems()
    {
        new Thread(() => {
            while (PaneSize == Vector2.Zero)
            {
                //wait
            }
            CallDeferred(nameof(MakeItemList));
        }){IsBackground = true}.Start();
    }

    private void MakeItemsVisible()
    {
        foreach (ThumbnailGridItem tgi in Items)
        {
            if (tgi.GetParent() != null)ThumbGrid.RemoveChild(tgi);
        }
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0} thumbgrid items available.", Items.Count));
        for (int i = 0; i < 10; i++)
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Adding {0} to thumbgrid.", Items[i].PackageReference.FileName));
            //ThumbGrid.AddChild(Items[i]);
            //Items[i].Visible = true;
        }
    }


    private void GridItemSelectionChanged(bool Selected, SimsPackage package, ThumbnailGridItem gridItem)
    {
        if ((!ShiftHeld && !ControlHeld) || LastSelected == -1){
			if (Selected)
            {
                foreach (ThumbnailGridItem item in Items)
                {
                    if (item != gridItem)
                    {
                        item.IsSelected = false;
                        LastSelected = Items.IndexOf(gridItem);
                    }
                }
            }		
		} else if (ShiftHeld){
            foreach (ThumbnailGridItem item in Items)
            {                
                item.IsSelected = false;
            }
            if (Items.IndexOf(gridItem) > LastSelected)
            {
                for (int i = LastSelected; i < Items.IndexOf(gridItem); i++)
                {
                    Items[i].IsSelected = Selected;
                }
            } else
            {
                for (int i = Items.IndexOf(gridItem); i < LastSelected; i++)
                {
                    Items[i].IsSelected = Selected;
                }
            }
		}                
    }

    public ThumbnailGridItem MakeViewportItem(ThumbnailGridItem tgi, SimsPackage package)
    {
        if (package.Mesh)
        {
            if (package.Game == SimsGames.Sims2)
            {
                Snapshotter snapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                tgi.subViewport.AddChild(snapshotter);
                snapshotter.BuildSims2Mesh(package);
                if ((package.PackageData as Sims2Data).MMATDataBlock.Count != 0)
                {
                    snapshotter.ApplyTextures(package);
                } else
                {
                    if (packages.Where(x => x.ObjectGUID == package.ObjectGUID).Any(p => p.Recolor))
                    {
                        SimsPackage matchingMesh = packages.Where(x => x.ObjectGUID == package.ObjectGUID).First(p => p.Recolor);
                        snapshotter.ApplyTextures(matchingMesh);
                    }
                    
                }
            }
        } else if (!package.Mesh && package.Recolor)
        {
            if (packages.Where(x => x.ObjectGUID == package.ObjectGUID).Any(p => p.Mesh))
            {
                SimsPackage matchingMesh = packages.Where(x => x.ObjectGUID == package.ObjectGUID).First(p => p.Mesh);
                Snapshotter snapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                tgi.subViewport.AddChild(snapshotter);
                snapshotter.BuildSims2Mesh(matchingMesh);
                snapshotter.ApplyTextures(package);    
            }            
        }
        return tgi;
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



    public override void _Process(double delta)
    {
		/*if (PaneSize == Vector2.Zero)
		{
			if (PaneSize != Size && Size != Vector2.Zero)
			{
				PaneSize = Size;
				GetSettings();
			}
		}	*/		
    }


    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key)
        {
            if (key.Keycode == Key.Shift)
            {
                ShiftHeld = key.IsPressed();
            } else if (key.Keycode == Key.Ctrl)
            {
                ControlHeld = key.IsPressed();
            }
        }
    }


}
