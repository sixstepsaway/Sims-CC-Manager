using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using System;
using System.Collections.Generic;

public partial class RightClickMenu : Node2D
{
    private List<Category> _allcategories;
    public List<Category> AllCategories
    {
        get { return _allcategories; }
        set { _allcategories = value; 
        RefreshCategories();}
    }

    public PackageDisplay packageDisplay;

    [Export]
    public MarginContainer MainContainer;

    [Export]
    PackedScene CategoryOption;
    [Export]
    ColorRect[] bgcolor;
    [Export]
    ColorRect[] bordercolor;
    [Export]
    public MarginContainer CategoryOptions;
    [Export]
    public MarginContainer EditDetails;
    [Export]
    VBoxContainer Categorylist;
    [ExportGroup("Items")]
    [Export]
    RcmItem MakeRoot;
    [Export]
    RcmItem Fave;
    [Export]
    RcmItem WrongGame;
    [Export]
    RcmItem UpdatedOOD;
    [Export]
    RcmItem Details;
    [Export]
    public RcmItem FFromF;
    [Export]
    RcmItem Categories;
    [ExportGroup("EditDetailsOptions")]
    [Export]
    RcmItem Rename;
    [Export]
    RcmItem Creator;
    [Export]
    RcmItem Source;
    [Export]
    RcmItem Move;
    [Export]
    RcmItem Delete;
    [Export]
    RcmItem Notes;
    
    public bool Plural = false;

    private bool _folderselected;
    public bool FolderSelected {
    get { return _folderselected; }
    set { _folderselected = value; 
            if (value)
            {                
                FFromF.label.Text = "Files from Folder";
            } else
            {
                FFromF.label.Text = "Files to Folder";
            }
        }
    }

    private bool _mostlyroot;
    public bool MostlyRoot {
    get { return _mostlyroot; }
    set { _mostlyroot = value; 
            if (value)
            {                
                MakeRoot.label.Text = "Make Normal";
            } else
            {
                MakeRoot.label.Text = "Make Root";
            }
        }
    }

    private bool _mostlyfave;
    public bool MostlyFave {
    get { return _mostlyfave; }
    set { _mostlyfave = value; 
            if (value)
            {                
                Fave.label.Text = "Unset Favorite";
            } else
            {
                Fave.label.Text = "Set Favorite";
            }
        }
    }

    private bool _mostlywronggame;
    public bool MostlyWrongGame {
    get { return _mostlywronggame; }
    set { _mostlywronggame = value; 
            if (value)
            {                
                WrongGame.label.Text = "Set Correct Game";
            } else
            {
                WrongGame.label.Text = "Set Wrong Game";
            }
        }
    }

    private bool _mostlyupdated;
    public bool MostlyUpdated {
    get { return _mostlyupdated; }
    set { _mostlyupdated = value; 
            if (value)
            {                
                UpdatedOOD.label.Text = "Set Out of Date";
            } else
            {
                UpdatedOOD.label.Text = "Unset Out of Date";
            }
        }
    }

    List<RcmItem> items = new();

    List<CategoryOption> categoryoptions = new();

    public delegate void ButtonPressedEvent(int button);
    public ButtonPressedEvent ButtonPressed;


    public override void _Ready()
    {
        items.Add(WrongGame);
        items.Add(UpdatedOOD);
        items.Add(MakeRoot);
        items.Add(Fave);
        items.Add(FFromF);
        items.Add(Rename);
        items.Add(Source);
        items.Add(Creator);
        items.Add(Move);
        items.Add(Delete);
        items.Add(Notes);
        items.Add(Categories);

        if (Plural)
        {
            Rename.label.Text = "Rename Files";
            Source.label.Text = "Sources";
            Creator.label.Text = "Creators";
            Move.label.Text = "Move Files";
            Delete.label.Text = "Delete Files";
            Notes.label.Text = "Notes";
        } else
        {
            Rename.label.Text = "Rename";
            Source.label.Text = "Source";
            Creator.label.Text = "Creator";
            Move.label.Text = "Move";
            Delete.label.Text = "Delete";
            Notes.label.Text = "Notes";
        }

        
        Categories.label.Text = "Categories";
        Details.label.Text = "Edit Details";

        
        
        Categories.button.MouseEntered += () => CategoriesHover(true);
        Categories.button.MouseExited += () => CategoriesHover(false);
        
        Details.button.MouseEntered += () => DetailsHover(true);
        Details.button.MouseExited += () => DetailsHover(false);
        

        UpdateTheme();


        WrongGame.button.Pressed += () => PressedButton(0);
        UpdatedOOD.button.Pressed += () => PressedButton(1);
        MakeRoot.button.Pressed += () => PressedButton(2);
        Fave.button.Pressed += () => PressedButton(3);
        FFromF.button.Pressed += () => PressedButton(4);
        Rename.button.Pressed += () => PressedButton(5);
        Source.button.Pressed += () => PressedButton(6);
        Creator.button.Pressed += () => PressedButton(7);
        Move.button.Pressed += () => PressedButton(8);
        Delete.button.Pressed += () => PressedButton(9);
        Notes.button.Pressed += () => PressedButton(10);
        Categories.button.Pressed += () => PressedButton(11);
        Details.button.Pressed += () => PressedButton(12);


    }

    private void DetailsHover(bool v)
    {
        EditDetails.Visible = true;
        CategoryOptions.Visible = false;
    }


    private void PressedButton(int i)
    {
        ButtonPressed?.Invoke(i);
    }


    private void CategoriesHover(bool v)
    {
        CategoryOptions.Visible = true;
        EditDetails.Visible = false;
    }

    private void RefreshCategories()
    {
        foreach (CategoryOption catop in categoryoptions)
        {
            catop.QueueFree();
            categoryoptions.Remove(catop);
        }

        CategoryOption cop = CategoryOption.Instantiate() as CategoryOption;
        cop.label.Text = "Default";
        cop.BGColor.Color = Colors.White;
        cop.label.AddThemeColorOverride("font_color", Colors.Black);

        Categorylist.AddChild(cop);
        categoryoptions.Add(cop);
        foreach (Category cat in AllCategories)
        {
            if (cat.Name != "Default")
            {
                CategoryOption co = CategoryOption.Instantiate() as CategoryOption;
                co.label.Text = cat.Name;
                co.label.AddThemeColorOverride("font_color", cat.TextColor);
                co.BGColor.Color = cat.Background;
                co.button.Pressed += () => CategoryClicked(co);
                co.category = cat;

                Categorylist.AddChild(co);
                categoryoptions.Add(co);
            }            
        }
    }

    private void CategoryClicked(CategoryOption categoryOption)
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Category {0} selected: {1}", categoryOption.label.Name, categoryOption.IsToggled ));
        CategorySelected?.Invoke(categoryOption.category);
    }

    public delegate void CategorySelectedEvent(Category category);
    public CategorySelectedEvent CategorySelected;


    private void UpdateTheme()
    {
        foreach (RcmItem item in items)
        {
            item.label.AddThemeColorOverride("font_color", GlobalVariables.LoadedTheme.MainTextColor);
        }      

        foreach (ColorRect colorRect in bgcolor)
        {
            colorRect.Color = GlobalVariables.LoadedTheme.BackgroundColor;
        }  
        foreach (ColorRect colorRect in bordercolor)
        {
            colorRect.Color = GlobalVariables.LoadedTheme.AccentColor;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse && mouse.Pressed && !IsMouseInRCM())
        {
            QueueFree();
        }
    } 


    private bool IsMouseInRCM(){
        bool inRCM = false;
		Vector2 mousepos = GetGlobalMousePosition(); 

		Rect2 rect = new (MainContainer.GlobalPosition, MainContainer.Size); 
        Rect2 cats = new(CategoryOptions.GlobalPosition, CategoryOptions.Size);
        Rect2 deets = new(EditDetails.GlobalPosition, EditDetails.Size);

        if (CategoryOptions.Visible)
        {
            if (cats.HasPoint(mousepos)) inRCM = true; 
        }
        if (EditDetails.Visible)
        {
            if (deets.HasPoint(mousepos)) inRCM = true; 
        }
        if (rect.HasPoint(mousepos)) inRCM = true; 

		return inRCM; 
	}

}
