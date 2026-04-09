using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.OptionLists;
using SimsCCManager.SettingsSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

public partial class ViewErrorsDisplay : MarginContainer
{
    [Export]
    PackedScene VEDHeaderPS;
    [Export]
    PackedScene VEDItemPS;
    [Export]
    VBoxContainer ErrorItemsBox;
    [Export]
    Button RemoveWrongButton;
    [Export]
    Button MoveWrongButton;
    [Export]
    Button RemoveBrokenButton;
    [Export]
    Button RemoveOrphansButton;
    [Export]
    public Button CloseButton;
    [Export]
    Label FixErrorsHeader;
    [Export]
    Label Subheading;
    
    [Export]
    Panel SecondBackground;
    [Export]
    Panel InternalBackground;

    public PackageDisplay packageDisplay;

    public delegate void PackagesMovedOrDeletedEvent();
    public PackagesMovedOrDeletedEvent PackagesMovedOrDeleted;

    List<VED_HeaderLabel> HeaderLabels = new();
    List<VedItem> VedItems = new();

    public List<SimsPackage> packages = new();

    List<Button> Buttons = new();

    public override void _Ready()
    {
        UpdateTheme();

        Buttons.Add(RemoveBrokenButton);
        Buttons.Add(RemoveWrongButton);
        Buttons.Add(RemoveOrphansButton);
        Buttons.Add(MoveWrongButton);
        Buttons.Add(CloseButton);

        RemoveBrokenButton.Pressed += () => RemoveBrokenPackages();
        RemoveWrongButton.Pressed += () => RemoveWrongPackages();
        RemoveOrphansButton.Pressed += () => RemoveOrphanPackages();
        MoveWrongButton.Pressed += () => MoveWrongPackages();
        //CloseButton.Pressed += () => Close();
    }

    private void Close()
    {
        GetParent().RemoveChild(this);
        QueueFree();
    }


    private void MoveWrongPackages()
    {
        List<VedItem> items = [..VedItems.Where(x => x.type == 1)];
        StringBuilder sb = new();
        List<SimsGames> games = [..items.Select(x => x.Package.Game)];
        foreach (SimsGames item in games.Distinct())
        {            
            List<VedItem> packages = [..items.Where(x => x.Package.Game == item)];
            if (GlobalVariables.LoadedSettings.InstanceFolders.Any(x => x.Game == item))
            {
                Instance instance = GlobalVariables.LoadedSettings.InstanceFolders.First(x => x.Game == item);
                GameInstance gameinstance = new() { InstanceFolder = instance.InstanceLocation };
                gameinstance.LoadInstance();
                foreach (VedItem p in packages) {
                    p.Package.MovePackage(gameinstance.InstanceFolders.InstancePackagesFolder);
                }
            } else
            {
                string MovedFiles = Path.Combine(packageDisplay.ThisInstance.InstanceFolder, "Moved Files");
                string gamefolder = "";
                if (!Directory.Exists(MovedFiles))
                {
                    Directory.CreateDirectory(MovedFiles);
                } 
                switch (item)
                {
                    case SimsGames.Sims1: 
                        gamefolder = Path.Combine(MovedFiles, "Sims 1");
                    break;
                    case SimsGames.Sims2: 
                        gamefolder = Path.Combine(MovedFiles, "Sims 2");
                    break;
                    case SimsGames.Sims3: 
                        gamefolder = Path.Combine(MovedFiles, "Sims 3");
                    break;
                    case SimsGames.Sims4: 
                        gamefolder = Path.Combine(MovedFiles, "Sims 4");
                    break;
                    case SimsGames.SimsMedieval: 
                        gamefolder = Path.Combine(MovedFiles, "Sims Medieval");
                    break;
                    case SimsGames.SimCity4: 
                        gamefolder = Path.Combine(MovedFiles, "SimCity4");
                    break;
                    case SimsGames.SimCity5: 
                        gamefolder = Path.Combine(MovedFiles, "SimCity5");
                    break;
                    case SimsGames.Spore: 
                        gamefolder = Path.Combine(MovedFiles, "Spore");
                    break;
                }
                if (!Directory.Exists(gamefolder))
                {
                    Directory.CreateDirectory(gamefolder);
                } 
                foreach (VedItem p in packages) {
                    p.Package.MovePackage(gamefolder);
                }                
            }
                
        }
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Will be moving these files: {0}", sb.ToString()));
    }


    private void RemoveWrongPackages()
    {
        List<VedItem> items = [..VedItems.Where(x => x.type == 1)];
        //StringBuilder sb = new();
        foreach (VedItem item in items)
        {
            Utilities.MoveToRecycleBin(item.Package.InfoFile);
            Utilities.MoveToRecycleBin(item.Package.FileName);
        }
        //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Will be removing these files: {0}", sb.ToString()));
    }


    private void RemoveOrphanPackages()
    {
        List<VedItem> items = [..VedItems.Where(x => x.type == 2)];
        //StringBuilder sb = new();
        foreach (VedItem item in items)
        {            
            Utilities.MoveToRecycleBin(item.Package.InfoFile);
            Utilities.MoveToRecycleBin(item.Package.FileName);
        }
        //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Will be removing these files: {0}", sb.ToString()));
    }


    private void RemoveBrokenPackages()
    {
        List<VedItem> items = [..VedItems.Where(x => x.type == 3)];
        //StringBuilder sb = new();
        foreach (VedItem item in items)
        {            
            Utilities.MoveToRecycleBin(item.Package.InfoFile);
            Utilities.MoveToRecycleBin(item.Package.FileName);
        }
        //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Will be removing these files: {0}", sb.ToString()));
    }


    public void ShowErrors()
    {
        List<SimsPackage> wronggames = [..packages.Where(x => x.WrongGame)];
        List<SimsPackage> orphan = [..packages.Where(x => x.Orphan)];
        List<SimsPackage> broken = [..packages.Where(x => x.Broken)];
        List<SimsPackage> outofdate = [..packages.Where(x => x.OutOfDate)];

        if (wronggames.Count > 0)
        {
            VED_HeaderLabel header = VEDHeaderPS.Instantiate() as VED_HeaderLabel;
            header.TypeName = "INCORRECT GAME";
            header.CountNum = wronggames.Count;
            ErrorItemsBox.AddChild(header);
            HeaderLabels.Add(header);
            foreach (SimsPackage p in wronggames)
            {
                VedItem item = VEDItemPS.Instantiate() as VedItem;
                item.packageDisplay = packageDisplay;
                item.Package = p;
                item.type = 1;
                VedItems.Add(item);
                ErrorItemsBox.AddChild(item);
            }
        }
        if (orphan.Count > 0)
        {
            VED_HeaderLabel header = VEDHeaderPS.Instantiate() as VED_HeaderLabel;
            header.TypeName = "ORPHANS";
            header.CountNum = orphan.Count;
            ErrorItemsBox.AddChild(header);
            HeaderLabels.Add(header);
            foreach (SimsPackage p in orphan)
            {
                VedItem item = VEDItemPS.Instantiate() as VedItem;
                item.packageDisplay = packageDisplay;
                item.Package = p;
                VedItems.Add(item);
                item.type = 2;
                ErrorItemsBox.AddChild(item);
            }
        }
        if (broken.Count > 0)
        {
            VED_HeaderLabel header = VEDHeaderPS.Instantiate() as VED_HeaderLabel;
            header.TypeName = "BROKEN";
            header.CountNum = broken.Count;
            ErrorItemsBox.AddChild(header);
            HeaderLabels.Add(header);
            foreach (SimsPackage p in broken)
            {
                VedItem item = VEDItemPS.Instantiate() as VedItem;
                item.packageDisplay = packageDisplay;
                item.Package = p;
                VedItems.Add(item);
                item.type = 3;
                ErrorItemsBox.AddChild(item);
            }
        }
        if (outofdate.Count > 0)
        {
            VED_HeaderLabel header = VEDHeaderPS.Instantiate() as VED_HeaderLabel;
            header.TypeName = "OUT OF DATE";
            header.CountNum = outofdate.Count;
            ErrorItemsBox.AddChild(header);
            HeaderLabels.Add(header);
            foreach (SimsPackage p in outofdate)
            {
                VedItem item = VEDItemPS.Instantiate() as VedItem;
                item.packageDisplay = packageDisplay;
                item.Package = p;
                VedItems.Add(item);
                item.type = 4;
                item.IsSelected = false;
                ErrorItemsBox.AddChild(item);
            }
        }
    }

    public void UpdateTheme()
    {
        SCCMTheme theme = GlobalVariables.LoadedTheme;
        bool textLight = false;
        StyleBoxFlat normalbox = RemoveBrokenButton.GetThemeStylebox("normal") as StyleBoxFlat;
        StyleBoxFlat hoverbox = RemoveBrokenButton.GetThemeStylebox("hover") as StyleBoxFlat;
        StyleBoxFlat clickedbox = RemoveBrokenButton.GetThemeStylebox("pressed") as StyleBoxFlat;
        
        if (theme.ButtonMain.V > 0.5)
        {
            textLight = true;
        }

        normalbox.BorderColor = theme.AccentColor;

        if (theme.AccentColor.V > 0.5)
        {
            hoverbox.BorderColor = theme.AccentColor.Darkened(0.2f);
            clickedbox.BorderColor = theme.AccentColor.Darkened(0.2f);
        } else
        {
            hoverbox.BorderColor = theme.AccentColor.Lightened(0.2f);
            clickedbox.BorderColor = theme.AccentColor.Darkened(0.2f);
        }

        
        normalbox.BgColor = theme.BackgroundColor;
        hoverbox.BgColor = theme.BackgroundColor.Darkened(0.2f);
        clickedbox.BgColor = theme.BackgroundColor.Darkened(0.2f);
        

        foreach (Button button in Buttons)
        {
            button.AddThemeColorOverride("font_color", theme.ButtonMain);
            button.AddThemeColorOverride("font_hover_color", theme.ButtonHover);
            button.AddThemeColorOverride("font_hover_pressed", theme.ButtonClick);
            button.AddThemeStyleboxOverride("normal", normalbox);
            button.AddThemeStyleboxOverride("hover", hoverbox);
            button.AddThemeStyleboxOverride("pressed", clickedbox);
        }
        FixErrorsHeader.AddThemeColorOverride("font_color", theme.HeaderTextColor);
        Subheading.AddThemeColorOverride("font_color", theme.HeaderTextColor);



        StyleBoxFlat sb = SecondBackground.GetThemeStylebox("panel") as StyleBoxFlat;
        sb.BgColor = theme.BackgroundColor;
        sb.BorderColor = theme.AccentColor;

        StyleBoxFlat ib = InternalBackground.GetThemeStylebox("panel") as StyleBoxFlat;
        ib.BgColor = theme.BackgroundColor;
        ib.BorderColor = theme.AccentColor;







    }
}
