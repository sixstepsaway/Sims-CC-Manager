using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.SettingsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public partial class CategoryManagement : MarginContainer
{
    public PackageDisplay packageDisplay;
    [ExportCategory("PackedScenes")]
    [Export]
    PackedScene CategoryItemPS;
    [ExportCategory("Nodes")]
    [Export]
    Panel BgPanel;
    [Export]
    Label CategoryLabel;
    [Export]
    Panel ListBg;
    [Export]
    VBoxContainer CategoryList;
    [Export]
    Button NewCategoryButton;
    [Export]
    Button EditCategoryButton;
    [Export]
    Button DupeCategoryButton;
    [Export]
    Button DeleteCategoryButton;
    [Export]
    Button CloseButton;
    [ExportCategory("MiniWindow")]
    [Export]
    MarginContainer MiniWindow;
    [Export]
    LineEdit CategoryNameBox;
    [Export]
    TextEdit CategoryDescriptionBox;
    [Export]
    Label BGColorPickerLabel;
    [Export]
    ColorPickerButton BGColorPicker;
    [Export]
    Label TextColorPickerLabel;
    [Export]
    ColorPickerButton TextColorPicker;
    [Export]
    Button ConfirmButton;
    [Export]
    Button CancelButton;

    List<Button> Buttons = new(); 


    bool ChangedHiddenCats = false;

    List<CategoryItem> CategoryItems = new();

    public delegate void CategoriesUpdatedEvent(bool andClose);
    public CategoriesUpdatedEvent CategoriesUpdated;


    bool MakingNew = false;
    bool EditingOld = false;

    List<SimsPackage> packages = new();

    public override void _Ready()
    {                
        Buttons.Add(NewCategoryButton);
        Buttons.Add(EditCategoryButton);
        Buttons.Add(DupeCategoryButton);
        Buttons.Add(DeleteCategoryButton);
        Buttons.Add(CloseButton);
        Buttons.Add(ConfirmButton);
        Buttons.Add(CancelButton);
        UpdateTheme();

        foreach (Category category in packageDisplay.ThisInstance.Categories)
        {
            AddCategoryItem(category);
        }

        NewCategoryButton.Pressed += () => NewCategory();
        EditCategoryButton.Pressed += () => EditCategory();
        DupeCategoryButton.Pressed += () => DupeCategory();
        DeleteCategoryButton.Pressed += () => DeleteCategory();

        CloseButton.Pressed += () => ClosePanel();

        ConfirmButton.Pressed += () => CategoryChangeConfirm();
        CancelButton.Pressed += () => CategoryChangeCancel();
        packages = [.. packageDisplay.ThisInstance.Files.OfType<SimsPackage>()];
    }

    private void CategoryChangeCancel()
    {
        MiniWindow.Visible = false;
        MakingNew = false;
        EditingOld = false;
    }


    private void CategoryChangeConfirm()
    {
        if (MakingNew)
        {   
            string name = CategoryNameBox.Text;
            if (packageDisplay.ThisInstance.Categories.Any(x => x.Name == name))
            {
                name = IncCategoryName(name);
            }
            Category category = new();
            category.Name = name;
            category.Description = CategoryDescriptionBox.Text;
            category.TextColor = TextColorPicker.Color;
            category.Background = BGColorPicker.Color;     
            packageDisplay.ThisInstance.Categories.Add(category);
            packageDisplay.ThisInstance.WriteXML();       
            AddCategoryItem(category);
        } else if (EditingOld)
        {
            CategoryItem ci = CategoryItems.First(x => x.IsSelected);
            CategoryItems.Remove(ci);
            ci.QueueFree();
            Category cat = packageDisplay.ThisInstance.Categories.First(x => x.Name == ci.CategoryName.Text);
            packageDisplay.ThisInstance.Categories.Remove(cat);
            cat.Name = CategoryNameBox.Text;
            cat.Description = CategoryDescriptionBox.Text;
            packageDisplay.ThisInstance.Categories.Add(cat);
            packageDisplay.ThisInstance.WriteXML();
            AddCategoryItem(cat); 
        }
        MakingNew = false;
        EditingOld = false;
        MiniWindow.Visible = false;
        CategoriesUpdated.Invoke(false);
    }

    private string IncCategoryName(string name, int inc = 0)
    {
        inc++;
        name = string.Format("{0} ({1})", name, inc);
        if (packageDisplay.ThisInstance.Categories.Any(x => x.Name == name))
        {
            name = IncCategoryName(name, inc);
        }
        return name;
    }


    private void DeleteCategory()
    {
        if (CategoryItems.Count == 1)
        {
            //
        } else
        {
            CategoryItem ci = CategoryItems.First(x => x.IsSelected);
            if (ci.CategoryName.Text != "Default")
            {
                Category defaultCat = packageDisplay.ThisInstance.Categories.First(x => x.Name == "Default");
                Category category = packageDisplay.ThisInstance.Categories.First(x => x.Name == ci.CategoryName.Text);
                //List<SimsPackage> packages = packageDisplay.ThisInstance.Files.OfType<SimsPackage>().ToList();
                List<SimsPackage> categorypackages = packages.Where(x => x.PackageCategory == category).ToList();
                foreach (SimsPackage package in categorypackages)
                {
                    package.PackageCategory = defaultCat;
                }
                CategoryItems.Remove(ci);
                ci.QueueFree();            
                packageDisplay.ThisInstance.Categories.Remove(category);
                packageDisplay.ThisInstance.WriteXML();
                CategoriesUpdated.Invoke(false);
            }            
        }
    }


    private void DupeCategory()
    {
        CategoryItem ci = CategoryItems.First(x => x.IsSelected);
        Category cat = packageDisplay.ThisInstance.Categories.First(x => x.Name == ci.CategoryName.Text);
        Category catCopy = new();
        catCopy.Background = cat.Background;
        catCopy.TextColor = cat.TextColor;
        catCopy.Name = string.Format("{0} - Copy", cat.Name);
        catCopy.Description = cat.Description;
        packageDisplay.ThisInstance.Categories.Add(catCopy);
        packageDisplay.ThisInstance.WriteXML();
        AddCategoryItem(catCopy);
        CategoriesUpdated.Invoke(false);
    }

    private void AddCategoryItem(Category category)
    {        
        
        int categorypackages = packages.Count(x => x.PackageCategory.Identifier == category.Identifier);
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Looking for packages in the {0} category. Checking for ID: {1}. Found {2}", category.Name, category.Identifier, categorypackages));
        CategoryItem ci = CategoryItemPS.Instantiate() as CategoryItem;
        if (packageDisplay.HideCategoriesInGrid.Contains(category))
        {
            ci.IsChecked = true;
        } else
        {
            ci.IsChecked = false;
        }
        ci.CategoryName.Text = category.Name;
        ci.Identifier = category.Identifier;
        ci.PackageCount.Text = categorypackages.ToString();
        ci.CategoryColor.Color = category.Background;
        ci.button.Pressed += () => CategoryItemClicked(ci);
        ci.DontShowInGrid += (s) => ItemDontShowInGrid(ci, s);
        CategoryItems.Add(ci);
        CategoryList.AddChild(ci);
    }

    private void ItemDontShowInGrid(CategoryItem ci, bool s)
    {
        Category cat = packageDisplay.ThisInstance.Categories.First(x => x.Name == ci.CategoryName.Text);
        if (s)
        {
            packageDisplay.HideCategoriesInGrid.Add(cat);
        } else
        {
            packageDisplay.HideCategoriesInGrid.Remove(cat);
        }
        ChangedHiddenCats = true;
    }

    private void EditCategory()
    {
        CategoryItem ci = CategoryItems.First(x => x.IsSelected);
        CategoryNameBox.Text = ci.Name;
        Category editingcat = packageDisplay.ThisInstance.Categories.First(x => x.Name == ci.CategoryName.Text);
        CategoryDescriptionBox.Text = editingcat.Description;
        BGColorPicker.Color = editingcat.Background;
        TextColorPicker.Color = editingcat.TextColor;
        MakingNew = false;
        EditingOld = true;
        MiniWindow.Visible = true;
    }

    private void NewCategory()
    {
        CategoryNameBox.Text = string.Empty;
        CategoryDescriptionBox.Text = string.Empty;
        BGColorPicker.Color = Colors.White;
        TextColorPicker.Color = Colors.Black;
        MakingNew = true;
        EditingOld = false;
        MiniWindow.Visible = true;
    }


    private void ClosePanel()
    {
        packageDisplay.LockInput = false;
        if (ChangedHiddenCats)
        {
            CategoriesUpdated.Invoke(true);
        } else
        {
            QueueFree();
        }
        CloseButton.Disabled = true;
        
    }


    private void CategoryItemClicked(CategoryItem categoryItem)
    {
        if (categoryItem.IsCursorInCheck())
        {
            categoryItem.FlipCheck();            
            packageDisplay.ThisInstance.Categories.First(x => x.Identifier == categoryItem.Identifier).Hidden = categoryItem.IsChecked;
            //packageDisplay.ThisInstance.WriteXML();
            
        } else
        {
            categoryItem.IsSelected = !categoryItem.IsSelected;
            foreach (CategoryItem item in CategoryItems)
            {
                if (item != categoryItem)
                {
                    item.IsSelected = false;
                }
            }
        }        
    }


    public void UpdateTheme()
    {
        SCCMTheme theme = GlobalVariables.LoadedTheme;
        bool textLight = false;
        StyleBoxFlat normalbox = ConfirmButton.GetThemeStylebox("normal") as StyleBoxFlat;
        StyleBoxFlat hoverbox = ConfirmButton.GetThemeStylebox("hover") as StyleBoxFlat;
        StyleBoxFlat clickedbox = ConfirmButton.GetThemeStylebox("pressed") as StyleBoxFlat;
        
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
        CategoryLabel.AddThemeColorOverride("font_color", theme.HeaderTextColor);
        BGColorPickerLabel.AddThemeColorOverride("font_color", theme.HeaderTextColor);
        TextColorPickerLabel.AddThemeColorOverride("font_color", theme.HeaderTextColor);
        
    }
}
