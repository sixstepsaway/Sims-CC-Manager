using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.OptionLists;
using SimsCCManager.SettingsSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

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
        if (SnapshotterActive)
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



    private void DisplayPackage()
    {
        ClearContents();
        ImageContainer.CustomMinimumSize = new((Size.X / 2) - 10, 0);
        AddPVI("File Name:", package.FileName);
        AddPVI("File Size:", package.FileSize);
        AddPVI("Directory:", package.IsDirectory.ToString());
        AddPVI("Root Mod:", package.RootMod.ToString());
        AddPVI("Added:", package.DateAdded.ToShortDateString());
        AddPVI("Modified:", package.DateUpdated.ToShortDateString());
        AddPVI("For Game:", package.Game.ToString());        

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
            if (package.Mesh)
            {
                ImageContainer.Visible = true;
                ImageTextureRect.Visible = false;
                SubviewportTexture.Visible = true;
                if (package.Game == SimsGames.Sims2)
                {
                    currSnapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                    SnapshotterActive = true;
                    Subviewport.AddChild(currSnapshotter);
                    currSnapshotter.BuildSims2Mesh(package);
                    if ((package.PackageData as Sims2Data).MMATDataBlock.Count != 0)
                    {
                        currSnapshotter.ApplyTextures(package);
                    } else
                    {
                        if (packageDisplay.ThisInstance.Files.OfType<SimsPackage>().Where(x => x.ObjectGUID == package.ObjectGUID).Any(p => p.Recolor))
                        {
                            SimsPackage matchingMesh = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().Where(x => x.ObjectGUID == package.ObjectGUID).First(p => p.Recolor);
                            currSnapshotter.ApplyTextures(matchingMesh);
                            
                        }                        
                    }
                }
            } else if (!package.Mesh && package.Recolor)
            {
                ImageContainer.Visible = true;
                ImageTextureRect.Visible = false;
                SubviewportTexture.Visible = true;
                if (packageDisplay.ThisInstance.Files.OfType<SimsPackage>().Where(x => x.ObjectGUID == package.ObjectGUID).Any(p => p.Mesh))
                {
                    SimsPackage matchingMesh = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().Where(x => x.ObjectGUID == package.ObjectGUID).First(p => p.Mesh);
                    currSnapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                    SnapshotterActive = true; 
                    Subviewport.AddChild(currSnapshotter);
                    currSnapshotter.BuildSims2Mesh(matchingMesh);
                    currSnapshotter.ApplyTextures(package);                       
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

        if (Images.Count > 1 || (Images.Count > 0 && SnapshotterActive))
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

    private void AddPVI(string key, string value)
    {
        PackageViewerItem pvi = PackageItemPS.Instantiate() as PackageViewerItem;
        pvi.KeyText = key;
        pvi.ValueText = value;
        ItemList.AddChild(pvi);
        PVIs.Add(pvi);
    }

}
