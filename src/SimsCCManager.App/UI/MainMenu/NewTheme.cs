using Godot;
using SimsCCManager.Globals;
using SimsCCManager.SettingsSystem;
using System;
using System.IO;
using System.Xml.Serialization;

public partial class NewTheme : MarginContainer
{
    [ExportCategory("Colors List")]
    [Export]
    public LineEdit ThemeName;
    [ExportGroup("BackgroundColor")]
    [Export]
    LineEdit BackgroundColor_LE;
    [Export]
    ColorPickerButton BackgroundColor_ColorPicker;
    [ExportGroup("ButtonMain")]
    [Export]
    LineEdit ButtonMain_LE;
    [Export]
    ColorPickerButton ButtonMain_ColorPicker;
    [ExportGroup("ButtonHover")]
    [Export]
    LineEdit ButtonHover_LE;
    [Export]
    ColorPickerButton ButtonHover_ColorPicker;
    [ExportGroup("ButtonClick")]
    [Export]
    LineEdit ButtonClick_LE;
    [Export]
    ColorPickerButton ButtonClick_ColorPicker;
    [ExportGroup("DataGridA")]
    [Export]
    LineEdit DataGridA_LE;
    [Export]
    ColorPickerButton DataGridA_ColorPicker;
    [ExportGroup("DataGridTextA")]
    [Export]
    LineEdit DataGridTextA_LE;
    [Export]
    ColorPickerButton DataGridTextA_ColorPicker;
    [ExportGroup("DataGridB")]
    [Export]
    LineEdit DataGridB_LE;
    [Export]
    ColorPickerButton DataGridB_ColorPicker;
    [ExportGroup("DataGridTextB")]
    [Export]
    LineEdit DataGridTextB_LE;
    [Export]
    ColorPickerButton DataGridTextB_ColorPicker;
    [ExportGroup("DataGridSelected")]
    [Export]
    LineEdit DataGridSelected_LE;
    [Export]
    ColorPickerButton DataGridSelected_ColorPicker;
    [ExportGroup("AccentColor")]
    [Export]
    LineEdit AccentColor_LE;
    [Export]
    ColorPickerButton AccentColor_ColorPicker;
    [ExportGroup("MainTextColor")]
    [Export]
    LineEdit MainTextColor_LE;
    [Export]
    ColorPickerButton MainTextColor_ColorPicker;
    [ExportGroup("HeaderTextColor")]
    [Export]
    LineEdit HeaderTextColor_LE;
    [Export]
    ColorPickerButton HeaderTextColor_ColorPicker;

    [ExportCategory("Examples")]
    [ExportGroup("TopbarButton")]
    [Export]
    ColorRect TopBarMain;
    [Export]
    ColorRect TopBarHover;
    [Export]
    ColorRect TopBarClick;
    [Export]
    Button TopBarButton;
    [ExportGroup("BackgroundExample")]
    [Export]
    ColorRect BackgroundExampleColor;
    [ExportGroup("MMButton")]
    [Export]
    ColorRect MMButton_Main;
    [Export]
    ColorRect MMButton_Hover;
    [Export]
    ColorRect MMButton_Click;
    [Export]
    Panel MMButton_PanelA;
    [Export]
    Panel MMButton_PanelB;
    [Export]
    Label MMButton_Label;
    [ExportGroup("DataGridA")]
    [Export]
    ColorRect DataGridABG;
    [Export]
    LineEdit DataGridAText1;
    [Export]
    LineEdit DataGridAText2;
    [ExportGroup("DataGridB")]
    [Export]
    ColorRect DataGridBBG;
    [Export]
    LineEdit DataGridBText1;
    [Export]
    LineEdit DataGridBText2;
    [ExportGroup("DataGridSelected")]
    [Export]
    ColorRect DataGridSelectedBG;
    [Export]
    LineEdit DataGridSelectedText1;
    [Export]
    LineEdit DataGridSelectedText2;
    [ExportGroup("ProfileOptions")]
    [Export]
    OptionButton ProfileOptionsDropdown;
    [ExportGroup("NormalButton")]
    [Export]
    Button NormalButtonExample;
    [ExportCategory("Buttons")]
    [Export]
    Button SaveButton;
    [Export]
    Button CancelButton;



    private Color _BackgroundColor;
    public Color BackgroundColor_Color
    {
        get { return _BackgroundColor; }
        set
        {
            _BackgroundColor = value;
            BackgroundColor_LE.Text = BackgroundColor_Color.ToHtml();
        }
    }
    private Color _ButtonMain;
    public Color ButtonMain_Color
    {
        get { return _ButtonMain; }
        set
        {
            _ButtonMain = value;
            ButtonMain_LE.Text = ButtonMain_Color.ToHtml();
        }
    }
    private Color _ButtonHover;
    public Color ButtonHover_Color
    {
        get { return _ButtonHover; }
        set
        {
            _ButtonHover = value;
            ButtonHover_LE.Text = ButtonHover_Color.ToHtml();
        }
    }
    private Color _ButtonClick;
    public Color ButtonClick_Color
    {
        get { return _ButtonClick; }
        set
        {
            _ButtonClick = value;
            ButtonClick_LE.Text = ButtonClick_Color.ToHtml();
        }
    }
    private Color _DataGridA;
    public Color DataGridA_Color
    {
        get { return _DataGridA; }
        set
        {
            _DataGridA = value;
            DataGridA_LE.Text = DataGridA_Color.ToHtml();
        }
    }
    private Color _DataGridTextA;
    public Color DataGridTextA_Color
    {
        get { return _DataGridTextA; }
        set
        {
            _DataGridTextA = value;
            DataGridTextA_LE.Text = DataGridTextA_Color.ToHtml();
        }
    }
    private Color _DataGridB;
    public Color DataGridB_Color
    {
        get { return _DataGridB; }
        set
        {
            _DataGridB = value;
            DataGridB_LE.Text = DataGridB_Color.ToHtml();
        }
    }
    private Color _DataGridTextB;
    public Color DataGridTextB_Color
    {
        get { return _DataGridTextB; }
        set
        {
            _DataGridTextB = value;
            DataGridTextB_LE.Text = DataGridTextB_Color.ToHtml();
        }
    }
    private Color _DataGridSelected;
    public Color DataGridSelected_Color
    {
        get { return _DataGridSelected; }
        set
        {
            _DataGridSelected = value;
            DataGridSelected_LE.Text = DataGridSelected_Color.ToHtml();
        }
    }
    private Color _AccentColor;
    public Color AccentColor_Color
    {
        get { return _AccentColor; }
        set
        {
            _AccentColor = value;
            AccentColor_LE.Text = AccentColor_Color.ToHtml();
        }
    }
    private Color _MainTextColor;
    public Color MainTextColor_Color
    {
        get { return _MainTextColor; }
        set
        {
            _MainTextColor = value;
            MainTextColor_LE.Text = MainTextColor_Color.ToHtml();
        }
    }
    private Color _HeaderTextColor;
    public Color HeaderTextColor_Color
    {
        get { return _HeaderTextColor; }
        set
        {
            _HeaderTextColor = value;
            HeaderTextColor_LE.Text = HeaderTextColor_Color.ToHtml();
        }
    }

    public delegate void AddedThemeEvent();
    public AddedThemeEvent AddedTheme;

    public override void _Ready()
    {
        BackgroundColor_ColorPicker.Color = BackgroundColor_Color;
        BackgroundColor_ColorPicker.ColorChanged += (color) => ColorChanged(color, 0);
        ButtonMain_ColorPicker.Color = ButtonMain_Color;
        ButtonMain_ColorPicker.ColorChanged += (color) => ColorChanged(color, 1);
        ButtonHover_ColorPicker.Color = ButtonHover_Color;
        ButtonHover_ColorPicker.ColorChanged += (color) => ColorChanged(color, 2);
        ButtonClick_ColorPicker.Color = ButtonClick_Color;
        ButtonClick_ColorPicker.ColorChanged += (color) => ColorChanged(color, 3);
        DataGridA_ColorPicker.Color = DataGridA_Color;
        DataGridA_ColorPicker.ColorChanged += (color) => ColorChanged(color, 4);
        DataGridTextA_ColorPicker.Color = DataGridTextA_Color;
        DataGridTextA_ColorPicker.ColorChanged += (color) => ColorChanged(color, 5);
        DataGridB_ColorPicker.Color = DataGridB_Color;
        DataGridB_ColorPicker.ColorChanged += (color) => ColorChanged(color, 6);
        DataGridTextB_ColorPicker.Color = DataGridTextB_Color;
        DataGridTextB_ColorPicker.ColorChanged += (color) => ColorChanged(color, 7);
        DataGridSelected_ColorPicker.Color = DataGridSelected_Color;
        DataGridSelected_ColorPicker.ColorChanged += (color) => ColorChanged(color, 8);
        AccentColor_ColorPicker.Color = AccentColor_Color;
        AccentColor_ColorPicker.ColorChanged += (color) => ColorChanged(color, 9);
        MainTextColor_ColorPicker.Color = MainTextColor_Color;
        MainTextColor_ColorPicker.ColorChanged += (color) => ColorChanged(color, 10);
        HeaderTextColor_ColorPicker.Color = HeaderTextColor_Color;
        HeaderTextColor_ColorPicker.ColorChanged += (color) => ColorChanged(color, 11);

        BackgroundColor_LE.TextSubmitted += (text) => TxtSubmitted(text, 0);
        ButtonMain_LE.TextSubmitted += (text) => TxtSubmitted(text, 1);
        ButtonHover_LE.TextSubmitted += (text) => TxtSubmitted(text, 2);
        ButtonClick_LE.TextSubmitted += (text) => TxtSubmitted(text, 3);
        DataGridA_LE.TextSubmitted += (text) => TxtSubmitted(text, 4);
        DataGridTextA_LE.TextSubmitted += (text) => TxtSubmitted(text, 5);
        DataGridB_LE.TextSubmitted += (text) => TxtSubmitted(text, 6);
        DataGridTextB_LE.TextSubmitted += (text) => TxtSubmitted(text, 7);
        DataGridSelected_LE.TextSubmitted += (text) => TxtSubmitted(text, 8);
        AccentColor_LE.TextSubmitted += (text) => TxtSubmitted(text, 9);
        MainTextColor_LE.TextSubmitted += (text) => TxtSubmitted(text, 10);
        HeaderTextColor_LE.TextSubmitted += (text) => TxtSubmitted(text, 11);


        UpdateTheme();

        UpdateButton(SaveButton);
        UpdateButton(CancelButton);

        SaveButton.Pressed += () => SaveTheme();
        CancelButton.Pressed += () => Close();

        TopBarButton.MouseEntered += () => HoverTestTB(true);
        TopBarButton.MouseExited += () => HoverTestTB(false);
    }

    private void UpdateButton(Button button)
    {
        SCCMTheme theme = GlobalVariables.LoadedTheme;
        bool textLight = false;
        StyleBoxFlat normalbox = button.GetThemeStylebox("normal") as StyleBoxFlat;
        StyleBoxFlat hoverbox = button.GetThemeStylebox("hover") as StyleBoxFlat;
        StyleBoxFlat clickedbox = button.GetThemeStylebox("pressed") as StyleBoxFlat;
        
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

        button.AddThemeColorOverride("font_color", theme.ButtonMain);
        button.AddThemeColorOverride("font_hover_color", theme.ButtonHover);
        button.AddThemeColorOverride("font_hover_pressed", theme.ButtonClick);
        button.AddThemeStyleboxOverride("normal", normalbox);
        button.AddThemeStyleboxOverride("hover", hoverbox);
        button.AddThemeStyleboxOverride("pressed", clickedbox);        
    }

    private void TxtSubmitted(string text, int idx)
    {
        switch (idx)
        {
            case 0: 
            BackgroundColor_Color = Color.FromHtml(text);;
            break;

            case 1: 
            ButtonMain_Color = Color.FromHtml(text);;
            break;

            case 2: 
            ButtonHover_Color = Color.FromHtml(text);;
            break;

            case 3: 
            ButtonClick_Color = Color.FromHtml(text);;
            break;

            case 4: 
            DataGridA_Color = Color.FromHtml(text);;
            break;

            case 5: 
            DataGridTextA_Color = Color.FromHtml(text);;
            break;

            case 6: 
            DataGridB_Color = Color.FromHtml(text);;
            break;

            case 7: 
            DataGridTextB_Color = Color.FromHtml(text);;
            break;

            case 8: 
            DataGridSelected_Color = Color.FromHtml(text);;
            break;

            case 9: 
            AccentColor_Color = Color.FromHtml(text);;
            break;

            case 10: 
            MainTextColor_Color = Color.FromHtml(text);;
            break;

            case 11: 
            HeaderTextColor_Color = Color.FromHtml(text);;
            break;
        }
    }


    private void HoverTestTB(bool v)
    {
        TopBarHover.Visible = v;
    }


    private void Close()
    {
        QueueFree();
    }


    private void SaveTheme()
    {
        SCCMTheme theme = new();
        string themeSaveLoc = Path.Combine(GlobalVariables.ThemesFolder, string.Format("{0}.xml", ThemeName.Text));
        theme.ThemeName = ThemeName.Text;
        theme.BackgroundColor = BackgroundColor_Color;
        theme.ButtonMain = ButtonMain_Color;
        theme.ButtonHover = ButtonHover_Color;
        theme.ButtonClick = ButtonClick_Color;
        theme.DataGridA = DataGridA_Color;
        theme.DataGridTextA = DataGridTextA_Color;
        theme.DataGridB = DataGridB_Color;
        theme.DataGridTextB = DataGridTextB_Color;
        theme.DataGridSelected = DataGridSelected_Color;
        theme.AccentColor = AccentColor_Color;
        theme.MainTextColor = MainTextColor_Color;
        theme.HeaderTextColor = HeaderTextColor_Color;
        XmlSerializer ThemeWriter = new(typeof(SCCMTheme));
        string themefile = Path.Combine(GlobalVariables.ThemesFolder, string.Format("{0}.xml", theme.ThemeName));
        if (File.Exists(themefile))
        {
            string themefileRn = Utilities.IncrementName(string.Format("{0}_old", themefile));            
            File.Copy(themefile, themefileRn);
        } else
        {
            Themes.AllThemes.Add(theme);
            GlobalVariables.LoadedSettings.ThemeOptions.Add(ThemeName.Text);
            GlobalVariables.LoadedSettings.SaveSettings();
        }
        using (var writer = new StreamWriter(themefile))
        {
            ThemeWriter.Serialize(writer, theme);
        }        
        AddedTheme.Invoke();
        QueueFree();
    }

    


    private void ColorChanged(Color inputColor, int ColorIDX)
    {
        switch (ColorIDX)
        {
            case 0: 
            BackgroundColor_Color = inputColor;
            break;

            case 1: 
            ButtonMain_Color = inputColor;
            break;

            case 2: 
            ButtonHover_Color = inputColor;
            break;

            case 3: 
            ButtonClick_Color = inputColor;
            break;

            case 4: 
            DataGridA_Color = inputColor;
            break;

            case 5: 
            DataGridTextA_Color = inputColor;
            break;

            case 6: 
            DataGridB_Color = inputColor;
            break;

            case 7: 
            DataGridTextB_Color = inputColor;
            break;

            case 8: 
            DataGridSelected_Color = inputColor;
            break;

            case 9: 
            AccentColor_Color = inputColor;
            break;

            case 10: 
            MainTextColor_Color = inputColor;
            break;

            case 11: 
            HeaderTextColor_Color = inputColor;
            break;
        }
        UpdateTheme();
    }



    private void UpdateTheme()
    {
        BackgroundExampleColor.Color = BackgroundColor_Color;

        DataGridABG.Color = DataGridA_Color;
        DataGridBBG.Color = DataGridB_Color;
        DataGridSelectedBG.Color = DataGridSelected_Color;

        DataGridAText1.AddThemeColorOverride("font_color", DataGridTextA_Color);
        DataGridBText1.AddThemeColorOverride("font_color", DataGridTextB_Color);
        DataGridSelectedText1.AddThemeColorOverride("font_color", DataGridTextA_Color);
        DataGridAText2.AddThemeColorOverride("font_color", DataGridTextA_Color);
        DataGridBText2.AddThemeColorOverride("font_color", DataGridTextB_Color);
        DataGridSelectedText2.AddThemeColorOverride("font_color", DataGridTextA_Color);

        DataGridAText1.AddThemeColorOverride("font_uneditable_color", DataGridTextA_Color);
        DataGridBText1.AddThemeColorOverride("font_uneditable_color", DataGridTextB_Color);
        DataGridSelectedText1.AddThemeColorOverride("font_uneditable_color", DataGridTextA_Color);
        DataGridAText2.AddThemeColorOverride("font_uneditable_color", DataGridTextA_Color);
        DataGridBText2.AddThemeColorOverride("font_uneditable_color", DataGridTextB_Color);
        DataGridSelectedText2.AddThemeColorOverride("font_uneditable_color", DataGridTextA_Color);

        ProfileOptionsMenu();

        MMButton();

        TBButton();

        NormalButtonE();
    }

    private void NormalButtonE()
    {
        bool textLight = false;
        StyleBoxFlat normalbox = NormalButtonExample.GetThemeStylebox("normal") as StyleBoxFlat;
        StyleBoxFlat hoverbox = NormalButtonExample.GetThemeStylebox("hover") as StyleBoxFlat;
        StyleBoxFlat clickedbox = NormalButtonExample.GetThemeStylebox("pressed") as StyleBoxFlat;
        
        if (ButtonMain_Color.V > 0.5)
        {
            textLight = true;
        }

        normalbox.BorderColor = AccentColor_Color;

        if (AccentColor_Color.V > 0.5)
        {
            hoverbox.BorderColor = AccentColor_Color.Darkened(0.2f);
            clickedbox.BorderColor = AccentColor_Color.Darkened(0.2f);
        } else
        {
            hoverbox.BorderColor = AccentColor_Color.Lightened(0.2f);
            clickedbox.BorderColor = AccentColor_Color.Darkened(0.2f);
        }

        normalbox.BgColor = BackgroundColor_Color;
        hoverbox.BgColor = BackgroundColor_Color.Darkened(0.2f);
        clickedbox.BgColor = BackgroundColor_Color.Darkened(0.2f);

        NormalButtonExample.AddThemeColorOverride("font_color", ButtonMain_Color);
        NormalButtonExample.AddThemeColorOverride("font_hover_color", ButtonHover_Color);
        NormalButtonExample.AddThemeColorOverride("font_hover_pressed", ButtonClick_Color);
        NormalButtonExample.AddThemeStyleboxOverride("normal", normalbox);
        NormalButtonExample.AddThemeStyleboxOverride("hover", hoverbox);
        NormalButtonExample.AddThemeStyleboxOverride("pressed", clickedbox);
        
    }
    private void TBButton()
    {
        TopBarMain.Color = ButtonMain_Color;
        TopBarHover.Color = ButtonHover_Color;
        TopBarClick.Color = ButtonClick_Color;
    }

    private void MMButton()
    {
        StyleBoxTexture sbt = MMButton_PanelA.GetThemeStylebox("panel") as StyleBoxTexture;
        GradientTexture1D gradient = new();
        gradient = sbt.Texture as GradientTexture1D;
        gradient.Gradient.SetColor(1, DataGridTextA_Color);
        gradient.Gradient.SetColor(0, DataGridTextB_Color);
        sbt.Texture = gradient;
        MMButton_PanelA.AddThemeStyleboxOverride("panel", sbt);
        MMButton_Main.Color = ButtonMain_Color;
        MMButton_Hover.Color = ButtonHover_Color;
        MMButton_Click.Color = ButtonClick_Color;
        MMButton_Label.AddThemeColorOverride("font_color", MainTextColor_Color);
    }


    private void ProfileOptionsMenu()
    {
        bool textLight = false;
        ProfileOptionsDropdown.AddThemeColorOverride("font_color", ButtonMain_Color);
        ProfileOptionsDropdown.AddThemeColorOverride("font_hover_color", ButtonHover_Color);
        StyleBoxFlat normalbox = ProfileOptionsDropdown.GetThemeStylebox("normal") as StyleBoxFlat;
        StyleBoxFlat hoverbox = ProfileOptionsDropdown.GetThemeStylebox("hover") as StyleBoxFlat;
        StyleBoxFlat focusbox = ProfileOptionsDropdown.GetThemeStylebox("focus") as StyleBoxFlat;
        
        if (ButtonMain_Color.V > 0.5)
        {
            textLight = true;
        }

        normalbox.BorderColor = AccentColor_Color;

        if (AccentColor_Color.V > 0.5)
        {
            hoverbox.BorderColor = AccentColor_Color.Darkened(0.2f);
            focusbox.BorderColor = AccentColor_Color.Darkened(0.2f);
        } else
        {
            hoverbox.BorderColor = AccentColor_Color.Lightened(0.2f);
            focusbox.BorderColor = AccentColor_Color.Lightened(0.2f);
        }

        if (textLight)
        {
            normalbox.BgColor = DataGridA_Color.Darkened(0.2f);
            hoverbox.BgColor = DataGridB_Color.Darkened(0.2f);
            focusbox.BgColor = DataGridB_Color.Darkened(0.2f);
        } else
        {
            normalbox.BgColor = DataGridA_Color.Lightened(0.2f);
            hoverbox.BgColor = DataGridB_Color.Lightened(0.2f);
            focusbox.BgColor = DataGridB_Color.Lightened(0.2f);
        }

        ProfileOptionsDropdown.AddThemeStyleboxOverride("normal", normalbox);
        ProfileOptionsDropdown.AddThemeStyleboxOverride("focus", focusbox);
        ProfileOptionsDropdown.AddThemeStyleboxOverride("hover", hoverbox);
    }
    

}
