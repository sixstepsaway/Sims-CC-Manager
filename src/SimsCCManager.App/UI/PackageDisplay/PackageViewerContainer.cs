using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.OptionLists;
using SimsCCManager.PackageReaders;
using SimsCCManager.SettingsSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

public partial class PackageViewerContainer : MarginContainer
{
    public PackageDisplay packageDisplay;
    [Export]
    VBoxContainer ItemList;
    [Export]
    Label HeaderLabel;
    [Export]
    TextureRect ImageTextureRect;
    [Export]
    TextureRect SubviewportTexture;
    [Export]
    VBoxContainer ImageContainer;
    [Export]
    SubViewport Subviewport;
    [Export]
    MarginContainer EnclosingBox;
    [Export]
    HBoxContainer ImageControls;
    [Export]
    Button ImageBack;
    [Export]
    Button ImageFw;
    [Export]
    ColorRect ButtonBackColor;
    [Export]
    ColorRect ButtonFwColor;
    [Export]
    PackedScene PackageItemPS;
    [Export]
    PackedScene SnapshotterPS;    

    private Color _buttoncolor;
    public Color ButtonColor
    {
        get { return _buttoncolor; }
        set { _buttoncolor = value; 
        ButtonBackColor.Color = value; 
        ButtonFwColor.Color = value;}
    }
    public Color ButtonHoverColor;

    private bool _fwhovered;
    public bool FwHovered
    {
        get { return _fwhovered; }
        set { _fwhovered = value; 
        if (value) ButtonFwColor.Color = ButtonHoverColor; else ButtonFwColor.Color = ButtonColor; }
    }
    private bool _bkhovered;
    public bool BkHovered
    {
        get { return _bkhovered; }
        set { _bkhovered = value; 
        if (value) ButtonBackColor.Color = ButtonHoverColor;  else ButtonBackColor.Color = ButtonColor; }
    }


    List<PackageViewerItem> PVIs = new();

    Snapshotter currSnapshotter;

    List<string> ImgTypes = new() { "*.png", "*.jpg", "*.bmp"};

    List<Image> Images = new();

    private int _currimage;

    bool SnapshotterActive = false;

    int CurrrentImage { get {return _currimage;}
    set { _currimage = value; 
            if (value == -1)
            {
                SwapSnapshotterAndImage();
            } else
            {
               ImageTextureRect.Texture = ImageTexture.CreateFromImage(Images[value]); 
            }                
        }    
    }
    
    private SimsPackage _package;
    public SimsPackage package
    {
        get { return _package; }
        set { _package = value; 
        DisplayPackage();}
    }

    public override void _Ready()
    {
        this.Resized += () => WindowResized();
        ImageBack.Pressed += () => SwapImages(-1);
        ImageFw.Pressed += () => SwapImages(1);
        UpdateTheme();

        ImageBack.MouseEntered += () => HoverImageBack(true);
        ImageBack.MouseExited += () => HoverImageBack(false);

        ImageFw.MouseEntered += () => HoverImageFw(true);
        ImageFw.MouseExited += () => HoverImageFw(false);
    }

    private void HoverImageFw(bool v)
    {
        FwHovered = v;
    }


    private void HoverImageBack(bool v)
    {
        BkHovered = v;
    }


    private void UpdateTheme()
    {
        SCCMTheme theme = GlobalVariables.LoadedTheme;
        bool textLight = false;       
        
        HeaderLabel.AddThemeColorOverride("font_color", theme.HeaderTextColor);
        ButtonColor = theme.ButtonMain;
        ButtonHoverColor = theme.ButtonHover;
    }


    private void SwapImages(int v)
    {
        if (MultipleSnapshotters)
        {
            if (v == 1)
            {
                currSnapshotter.NextBodyOption();
            } else
            {
                currSnapshotter.PreviousBodyOption();
            }
            
        } else if (SnapshotterActive)
        {
            if (CurrrentImage == -1 && v == 1)
            {
                SwapSnapshotterAndImage();
            } else if (CurrrentImage == 0 && v == -1)
            {
                CurrrentImage = -1;
            } else if (CurrrentImage == Images.Count && v == 1)
            {
                CurrrentImage = -1;
            } else if (v == -1) { CurrrentImage--; } else if (v == 1) { CurrrentImage++; }
        } else
        {
            if (CurrrentImage == 0 && v == -1) return;
            if (CurrrentImage == Images.Count && v == 1) return;
            if (v == -1) CurrrentImage--;
            if (v == 1) CurrrentImage++;
        }        
    }


    private void WindowResized()
    {
        ImageContainer.CustomMinimumSize = new((Size.X / 2) - 10, 0);
    }

    private void ClearContents()
    {
        foreach (PackageViewerItem pvi in PVIs)
        {
            pvi.QueueFree();
        }
        PVIs.Clear();
        if (SnapshotterActive)currSnapshotter?.QueueFree();
        SnapshotterActive = false;
        Images.Clear();
    }


    bool MultipleSnapshotters = false;

    private void DisplayPackage()
    {
        ClearContents();
        ImageContainer.CustomMinimumSize = new((Size.X / 2) - 10, 0);
        AddPVI("File Name:", package.FileName);
        AddPVI("File Size:", package.FileSize);
        AddPVI("Directory:", package.IsDirectory.ToString());
        AddPVI("Type:", package.Type.ToString());
        if (package.Creator != null) AddPVI("Creator:", package.Creator.ToString());
        if (package.Source != null) AddPVI("Source:", package.Source.ToString());
        
        
        
        
        AddPVI("Category:", package.CategoryName.ToString());
        AddPVI("Root Mod:", package.RootMod.ToString());
        AddPVI("Added:", package.DateAdded.ToShortDateString());
        AddPVI("Modified:", package.DateUpdated.ToShortDateString());
        AddPVI("For Game:", package.Game.ToString()); 
        AddPVI("Mesh:", package.Mesh.ToString());     
        AddPVI("Recolor:", package.Recolor.ToString()); 
        if (package.MatchingRecolors.Any())
        {
            StringBuilder sb = new();
            foreach (string mr in package.MatchingRecolors.OrderBy(x => x))
            {
                sb.AppendLine(string.Format(" - {0}", mr));
            }
            if (package.Recolor)
            {
                AddPVI("Other Recolors:", sb.ToString(), true);
            } else
            {
                AddPVI("Recolors:", sb.ToString(), true);
            }                
        } 
        if (package.Recolor && !package.Mesh)
        {
            if (!package.Orphan)
            {
                if (package.MatchingMesh != null) AddPVI("Mesh:", package.MatchingMesh);
            }
        }   
        AddPVI("Orphan:", package.Orphan.ToString());   
        if (package.Notes != null) AddPVI("Notes:", package.Orphan.ToString(), true);        

        if (package.IsDirectory || package.RootMod)
        {              
            List<string> files = new();
            foreach (string i in ImgTypes)
            {
                files.AddRange(Directory.EnumerateFiles(package.Location, i, SearchOption.AllDirectories).ToList());
            }            
            foreach (string file in files)
            {
                Images.Add(Image.LoadFromFile(file));                     
            }                      
        }

        if (package.PackageData != null)
        {
            if (package.Mesh || !string.IsNullOrEmpty(package.MatchingMesh))
            {
                ImageContainer.Visible = true;
                ImageTextureRect.Visible = false;
                SubviewportTexture.Visible = true;
                if (package.Recolor)
                {
                    currSnapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                    currSnapshotter.Packages = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList();
                    SnapshotterActive = true;
                    Subviewport.AddChild(currSnapshotter);
                    currSnapshotter.BuildSims2Mesh(package);
                    if (!package.Type.Contains("Hair")) {
                        if (package.Game == SimsGames.Sims2) currSnapshotter.GetTexturesForS2Meshes(package);
                    }
                }
                else if (!string.IsNullOrEmpty(package.MatchingMesh))
                {
                    SimsPackage matchingMesh = new();
                    SimsPackage color = new();
                    if (package.Mesh)
                    {
                        matchingMesh = package;
                        if (packageDisplay.ThisInstance.Files.OfType<SimsPackage>().Any(x => x.FileName == package.MatchingRecolors[0])) 
                        { 
                            color = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().First(x => x.FileName == package.MatchingRecolors[0]);
                        } else
                        {
                            color = package;
                        }
                    } else
                    {
                        color = package;
                        matchingMesh = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().First(x => x.FileName == package.MatchingMesh);
                    }                    
                    currSnapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                    currSnapshotter.Packages = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList();
                    SnapshotterActive = true;                     
                    Subviewport.AddChild(currSnapshotter);
                    if (package.Game == SimsGames.Sims2)
                    {
                        currSnapshotter.BuildSims2Mesh(matchingMesh);
                        if (!package.Type.Contains("Hair")) {
                            currSnapshotter.GetTexturesForS2Meshes(color); 
                        } else { 
                            currSnapshotter.ApplyS2Textures(color); 
                        }
                    }
                } else
                {
                    currSnapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                    currSnapshotter.Packages = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList();
                    SnapshotterActive = true;
                    Subviewport.AddChild(currSnapshotter);
                    currSnapshotter.BuildSims2Mesh(package);
                    if (!package.Type.Contains("Hair")) {
                        if (package.Game == SimsGames.Sims2) currSnapshotter.GetTexturesForS2Meshes(package);
                    }
                }
            } else if (!package.Mesh && package.Recolor)
            {
                ImageContainer.Visible = true;
                ImageTextureRect.Visible = false;
                SubviewportTexture.Visible = true;
                if (packageDisplay.ThisInstance.Files.OfType<SimsPackage>().Any(x => x.ObjectGUID == package.ObjectGUID && x.Mesh))
                {
                    SimsPackage matchingMesh = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().Where(x => x.ObjectGUID == package.ObjectGUID).First(p => p.Mesh);
                    currSnapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                    currSnapshotter.Packages = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList();
                    SnapshotterActive = true;                     
                    Subviewport.AddChild(currSnapshotter);
                    if (package.Game == SimsGames.Sims2)
                    {
                        currSnapshotter.BuildSims2Mesh(matchingMesh);
                        currSnapshotter.ApplyS2Textures(package);
                    }
                    MultipleSnapshotters = currSnapshotter.MultipleOptions;
                                           
                } else if (packageDisplay.ThisInstance.Files.OfType<SimsPackage>().Any(x => x.Sims2Data.XNGBDataBlock.ModelName.Contains(package.Sims2Data.TXTRDataBlock[0].TextureNoSuffix) && x.Mesh))
                {
                    SimsPackage matchingMesh = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().Where(x => x.Sims2Data.XNGBDataBlock.ModelName.Contains(package.Sims2Data.TXTRDataBlock[0].TextureNoSuffix)).First(p => p.Mesh);
                    currSnapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                    currSnapshotter.Packages = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList();
                    SnapshotterActive = true; 
                    Subviewport.AddChild(currSnapshotter);
                    currSnapshotter.BuildSims2Mesh(matchingMesh);
                    if (!package.Type.Contains("Hair")) {
                        if (package.Game == SimsGames.Sims2) currSnapshotter.ApplyS2Textures(package);
                    }
                    MultipleSnapshotters = currSnapshotter.MultipleOptions;
                }
            } else if (package.PackageImage != null)
            {
                ImageContainer.Visible = true;
                ImageTextureRect.Visible = true;
                SubviewportTexture.Visible = false;
                Images.Insert(0, package.PackageImage);
                ImageTextureRect.Texture = ImageTexture.CreateFromImage(package.PackageImage);
                SnapshotterActive = false;
            } else
            {
                ImageContainer.Visible = false;
                SnapshotterActive = false;
            }
            
        }
        if (MultipleSnapshotters)
        {
            ImageControls.Visible = true;
        } else if (Images.Count > 1 || (Images.Count > 0 && SnapshotterActive))
        {
            ImageContainer.Visible = true;
            ImageTextureRect.Visible = true; 
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("There are {0} images for file {1}, and a mesh.", Images.Count, package.FileName));
            ImageControls.Visible = true;
            CurrrentImage = 0;
        } else if (Images.Count > 0)
        {
            if (Images.Count > 1) ImageControls.Visible = true;
            ImageContainer.Visible = true;
            ImageTextureRect.Visible = true; 
            SubviewportTexture.Visible = false;
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("There are {0} images for file {1}", Images.Count, package.FileName));
            
            CurrrentImage = 0;
        } else
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("There are no images for file {0}", package.FileName));
            ImageControls.Visible = false;
        }
    }



    private void SwapSnapshotterAndImage()
    {
        ImageTextureRect.Visible = !ImageTextureRect.Visible;
        SubviewportTexture.Visible = !SubviewportTexture.Visible;
    }

    private void AddPVI(string key, string value, bool islong = false)
    {
        PackageViewerItem pvi = PackageItemPS.Instantiate() as PackageViewerItem;
        pvi.KeyText = key;
        pvi.ValueText = value;
        pvi.IsLong = islong;
        if (!string.IsNullOrEmpty(value))
        {
            ItemList.AddChild(pvi);
            PVIs.Add(pvi);            
        }
    }

}
