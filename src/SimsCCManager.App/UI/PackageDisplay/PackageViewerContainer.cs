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
using System.Diagnostics;
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
    SubViewportContainer SubviewportContainer;
    [Export]
    MarginContainer EnclosingBox;
    [Export]
    MarginContainer OverrideBox;
    [Export]
    TextureRect OverrideImage;
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

    private bool _showoverridethumb;
    public bool ShowOverrideThumb
    {
        get { return _showoverridethumb; }
        set { _showoverridethumb = value; 
        OverrideBox.Visible = value; }
    }

    private Texture2D _overridetexture;
    public Texture2D OverrideTexture
    {
        get { return _overridetexture; }
        set { _overridetexture = value; 
        OverrideImage.Texture = value; }
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
        if (SnapshotterActive) {
            //currSnapshotter?.DisconnectContainer();
            Subviewport.RemoveChild(currSnapshotter);
            currSnapshotter?.QueueFree();
        }
        SnapshotterActive = false;
        Images.Clear();
    }


    bool MultipleSnapshotters = false;

    private void DisplayPackage()
    {
        ShowOverrideThumb = false;
        ClearContents();
        ImageContainer.CustomMinimumSize = new((Size.X / 2) - 10, 0);
        AddPVI("File Name:", package.FileName);
        AddPVI("File Size:", package.FileSize);
        AddPVI("Directory:", package.IsDirectory.ToString());
        AddPVI("Type:", package.Type.ToString());
        if (package.Override) AddPVI("Override:", "True");
        if (package.Override && package.OverrideReference.Count > 0) {
            if (package.SpecificOverride != null)
            {
                AddPVI("Overriding:", package.SpecificOverride.Description);

                string filename = package.SpecificOverride.Description;
                if (filename.Contains("CASIE"))
                {
                    filename = filename.Split("_")[1];
                    filename = filename.Split("_")[0];                    
                } else
                {
                    filename = filename.Split("_")[0];                    
                }
                if (filename.StartsWith("tm") ||filename.StartsWith("tf") || filename.StartsWith("af") || filename.StartsWith("am") || filename.StartsWith("ef") || filename.StartsWith("em")) filename = filename[1..];

                filename = filename[2..];

                //filename = Path.Combine(GlobalVariables.mainWindow.Sims2OverrideImagesDirectory, filename);

                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking for images containing {0}", filename));
                                
                if (GlobalVariables.Sims2OverrideImages.Any(x => x.Contains(filename)))
                {
                    ShowOverrideThumb = true;
                    string image = "";
                    image = GlobalVariables.Sims2OverrideImages.First(x => x.Contains(filename));
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found image containing {0}: {1}", filename, image));
                    
                    //Image newimage = Image.LoadFromFile(image);
                    OverrideTexture = (Texture2D)GD.Load(image);
                } else
                {                                      
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Failed to find image containing {0}", filename));
                }
                
            } else
            {
                StringBuilder sb = new(); 
                foreach (SimsOverrides ord in package.OverrideReference)
                {
                    if (ord.FileNameOverrides.Count > 0)
                    {
                        foreach (ItemOverride io in ord.FileNameOverrides)
                        {
                            sb.AppendLine(string.Format("FileName Override: {0}", io.ToString()));
                        }
                    }
                    if (ord.TextureNameOverrides.Count > 0)
                    {
                        foreach (ItemOverride io in ord.TextureNameOverrides)
                        {
                            sb.AppendLine(string.Format("TextureName Override: {0}", io.ToString()));
                        }
                    }
                    if (ord.GuidOverrides.Count > 0)
                    {
                        foreach (ItemOverride io in ord.GuidOverrides)
                        {
                            sb.AppendLine(string.Format("Guid Override: {0}", io.ToString()));
                        }
                    }
                    if (ord.References.Count > 0)
                    {
                        foreach (ItemOverride io in ord.References)
                        {
                            sb.AppendLine(string.Format("Reference Override: {0}", io.ToString()));
                        }
                    }
                    if (ord.Entries.Count > 0)
                    {
                        foreach (SimsID id in ord.Entries)
                        {
                            sb.AppendLine(string.Format("Entry Override: {0}", id.FullKey));
                        }
                    }                    
                }
                AddPVI("Overriding:", sb.ToString().Replace("/n/n", "/n"), true);
            }
            
        }
        
        switch (package.Game)
        {
            case SimsGames.Sims2: 
                if (package.Sims2Data != null)
                {
                    if (package.Sims2Data.Title != null) AddPVI("Title:", package.Sims2Data.Title);
                    if (package.Sims2Data.Description != null) AddPVI("Description:", package.Sims2Data.Description, true);
                }
            break;
        }
        
        
        
        
        if (package.Creator != null) AddPVI("Creator:", package.Creator.ToString());
        if (package.Source != null) AddPVI("Source:", package.Source.ToString());
        
        
        
        
        AddPVI("Category:", package.CategoryName.ToString());
        AddPVI("Root Mod:", package.RootMod.ToString());
        AddPVI("Created:", package.DateCreated.ToShortDateString());
        AddPVI("Modified:", package.DateUpdated.ToShortDateString());
        AddPVI("For Game:", package.Game.ToString()); 
        AddPVI("Mesh:", package.Mesh.ToString());     
        AddPVI("Recolor:", package.Recolor.ToString()); 
        if (package.MatchingRecolors != null) if (package.MatchingRecolors.Any())
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
            if (package.Type.Contains("Face Template"))
            {
                ImageContainer.Visible = true;
                ImageTextureRect.Visible = false;
                SubviewportTexture.Visible = true;
                currSnapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                currSnapshotter.Packages = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList();
                currSnapshotter.MyContainer = SubviewportContainer;
                SnapshotterActive = true; 
                Subviewport.AddChild(currSnapshotter);
                bool done = currSnapshotter.BuildSims2Mesh(package);
            } else if (package.Type.Contains("Skin"))
            {
                ImageContainer.Visible = true;
                ImageTextureRect.Visible = false;
                SubviewportTexture.Visible = true;
                currSnapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                currSnapshotter.Packages = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList();
                currSnapshotter.MyContainer = SubviewportContainer;
                SnapshotterActive = true; 
                Subviewport.AddChild(currSnapshotter);
                currSnapshotter.DisplaySkin(package);                
            } else if (package.Type.Contains("Eyecolor"))
            {
                ImageContainer.Visible = true;
                ImageTextureRect.Visible = false;
                SubviewportTexture.Visible = true;
                currSnapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                currSnapshotter.Packages = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList();
                currSnapshotter.MyContainer = SubviewportContainer;
                SnapshotterActive = true; 
                Subviewport.AddChild(currSnapshotter);
                currSnapshotter.DisplayEyes(package);  
            } else if (package.Type.Contains("Makeup"))
            {
                ImageContainer.Visible = true;
                ImageTextureRect.Visible = false;
                SubviewportTexture.Visible = true;
                currSnapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                currSnapshotter.Packages = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList();
                currSnapshotter.MyContainer = SubviewportContainer;
                SnapshotterActive = true; 
                Subviewport.AddChild(currSnapshotter);
                currSnapshotter.DisplayOverlay(package);  
            } else if ((package.Mesh && (package.MatchingRecolors != null || package.Recolor))|| !string.IsNullOrEmpty(package.MatchingMesh))
            {   
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Making snapshotter for {0}", package.FileName));
                ImageContainer.Visible = true;
                ImageTextureRect.Visible = false;
                SubviewportTexture.Visible = true;
                currSnapshotter = SnapshotterPS.Instantiate() as Snapshotter;
                currSnapshotter.Packages = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList();
                currSnapshotter.MyContainer = SubviewportContainer;
                SnapshotterActive = true; 
                Subviewport.AddChild(currSnapshotter);
                bool done = currSnapshotter.BuildSims2Mesh(package);
                if (!done)
                {
                    SnapshotterActive = false;
                    if (package.PackageImage != null)
                    {
                        ImageContainer.Visible = true;
                        ImageTextureRect.Visible = true;
                        SubviewportTexture.Visible = false;
                        if (package.Sims2Data != null)
                        {
                            if (package.Sims2Data.TXTRDataBlock.Count > 0)
                            {
                                foreach (TXTRData txtr in package.Sims2Data.TXTRDataBlock)
                                {
                                    Images.Add(txtr.Texture);                                    
                                }
                                ImageTextureRect.Texture = ImageTexture.CreateFromImage(Images[0]);
                            } else
                            {
                                Images.Insert(0, package.PackageImage);
                                ImageTextureRect.Texture = ImageTexture.CreateFromImage(package.PackageImage);   
                            }
                        } else
                        {
                            Images.Insert(0, package.PackageImage);
                            ImageTextureRect.Texture = ImageTexture.CreateFromImage(package.PackageImage);
                        }
                    } else
                    {
                        ImageContainer.Visible = false;
                    }
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
        if (MultipleSnapshotters && SnapshotterActive)
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
