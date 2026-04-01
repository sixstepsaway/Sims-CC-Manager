using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SimsCCManager.Globals;
using SimsCCManager.SettingsSystem;

namespace SimsCCManager.ThemeUtilities
{
    public class ThemeUpdater
    {
        public static SCCMTheme LoadedTheme { get { return GlobalVariables.LoadedTheme; } }

        public static void UpdateBasicButtons(List<Button> buttons)
        {
            bool textLight = false;
            StyleBoxFlat normalbox = buttons[0].GetThemeStylebox("normal") as StyleBoxFlat;
            StyleBoxFlat hoverbox = buttons[0].GetThemeStylebox("hover") as StyleBoxFlat;
            StyleBoxFlat clickedbox = buttons[0].GetThemeStylebox("pressed") as StyleBoxFlat;

            if (LoadedTheme.ButtonMain.V > 0.5)
            {
                textLight = true;
            }

            normalbox.BorderColor = LoadedTheme.AccentColor;

            if (LoadedTheme.AccentColor.V > 0.5)
            {
                hoverbox.BorderColor = LoadedTheme.AccentColor.Darkened(0.2f);
                clickedbox.BorderColor = LoadedTheme.AccentColor.Darkened(0.2f);
            } else
            {
                hoverbox.BorderColor = LoadedTheme.AccentColor.Lightened(0.2f);
                clickedbox.BorderColor = LoadedTheme.AccentColor.Darkened(0.2f);
            }

            
            normalbox.BgColor = LoadedTheme.BackgroundColor;
            hoverbox.BgColor = LoadedTheme.BackgroundColor.Darkened(0.2f);
            clickedbox.BgColor = LoadedTheme.BackgroundColor.Darkened(0.2f);

            foreach (Button button in buttons)
            {
                button.AddThemeColorOverride("font_color", LoadedTheme.ButtonMain);
                button.AddThemeColorOverride("font_hover_color", LoadedTheme.ButtonHover);
                button.AddThemeColorOverride("font_hover_pressed", LoadedTheme.ButtonClick);
                button.AddThemeStyleboxOverride("normal", normalbox);
                button.AddThemeStyleboxOverride("hover", hoverbox);
                button.AddThemeStyleboxOverride("pressed", clickedbox);
            }
        }

        public static void UpdateHeaderLabels(List<Label> labels)
        {
            foreach (Label label in labels)
            {
                label.AddThemeColorOverride("font_color", LoadedTheme.HeaderTextColor);
            }
        }

        public static void UpdateBGPanelColors(List<Panel> panels)
        {
            foreach (Panel panel in panels) {
                StyleBoxFlat sb = panel.GetThemeStylebox("panel") as StyleBoxFlat;
                sb.BgColor = LoadedTheme.BackgroundColor;
                sb.BorderColor = LoadedTheme.AccentColor;
            }
        }

        public static void UpdateBigTextBoxes(List<TextEdit> boxes)
        {
            foreach (TextEdit box in boxes)
            {
                Theme boxtheme = box.Theme;
                StyleBoxFlat tbf = box.GetThemeStylebox("focus") as StyleBoxFlat;
                StyleBoxFlat tbn = box.GetThemeStylebox("normal") as StyleBoxFlat;
                if (LoadedTheme.IsThemeLight())
                {
                    tbn.BgColor = LoadedTheme.BackgroundColor.Darkened(0.05f);
                    tbn.BorderColor = Color.FromHsv(LoadedTheme.AccentColor.H, LoadedTheme.AccentColor.S - 0.25f, LoadedTheme.AccentColor.V);
                    tbf.BgColor = tbn.BgColor.Darkened(0.1f);
                } else
                {
                    tbn.BgColor = LoadedTheme.BackgroundColor.Lightened(0.05f);
                    tbn.BorderColor = Color.FromHsv(LoadedTheme.AccentColor.H, LoadedTheme.AccentColor.S - 0.25f, LoadedTheme.AccentColor.V);
                    tbf.BgColor = tbn.BgColor.Darkened(0.1f);
                }
                tbf.BorderColor = tbn.BorderColor.Lightened(0.1f);

                boxtheme.SetStylebox("normal", "TextEdit", tbn);
                boxtheme.SetStylebox("focus", "TextEdit", tbf);
                boxtheme.SetStylebox("normal", "LineEdit", tbn);
                boxtheme.SetStylebox("focus", "LineEdit", tbf);

                boxtheme.SetColor("font_color", "TextEdit", LoadedTheme.MainTextColor);
                boxtheme.SetColor("font_color", "LineEdit", LoadedTheme.MainTextColor);
                boxtheme.SetColor("font_placeholder_color", "TextEdit", LoadedTheme.MainTextColor.Lightened(0.2f));
                boxtheme.SetColor("font_placeholder_color", "LineEdit", LoadedTheme.MainTextColor.Lightened(0.2f));
            }
        }
        public static void UpdateBigTextBoxes(List<LineEdit> boxes)
        {
            foreach (LineEdit box in boxes)
            {
                Theme boxtheme = box.Theme;
                StyleBoxFlat tbf = box.GetThemeStylebox("focus") as StyleBoxFlat;
                StyleBoxFlat tbn = box.GetThemeStylebox("normal") as StyleBoxFlat;
                if (LoadedTheme.IsThemeLight())
                {
                    tbn.BgColor = LoadedTheme.BackgroundColor.Darkened(0.05f);
                    tbn.BorderColor = Color.FromHsv(LoadedTheme.AccentColor.H, LoadedTheme.AccentColor.S - 0.25f, LoadedTheme.AccentColor.V);
                    tbf.BgColor = tbn.BgColor.Darkened(0.1f);
                } else
                {
                    tbn.BgColor = LoadedTheme.BackgroundColor.Lightened(0.05f);
                    tbn.BorderColor = Color.FromHsv(LoadedTheme.AccentColor.H, LoadedTheme.AccentColor.S - 0.25f, LoadedTheme.AccentColor.V);
                    tbf.BgColor = tbn.BgColor.Darkened(0.1f);
                }
                tbf.BorderColor = tbn.BorderColor.Lightened(0.1f);

                boxtheme.SetStylebox("normal", "TextEdit", tbn);
                boxtheme.SetStylebox("focus", "TextEdit", tbf);
                boxtheme.SetStylebox("normal", "LineEdit", tbn);
                boxtheme.SetStylebox("focus", "LineEdit", tbf);

                boxtheme.SetColor("font_color", "TextEdit", LoadedTheme.MainTextColor);
                boxtheme.SetColor("font_color", "LineEdit", LoadedTheme.MainTextColor);
                boxtheme.SetColor("font_placeholder_color", "TextEdit", LoadedTheme.MainTextColor.Lightened(0.2f));
                boxtheme.SetColor("font_placeholder_color", "LineEdit", LoadedTheme.MainTextColor.Lightened(0.2f));
            }
        }

        public static void UpdateLoadOrderItemColors(Panel normal, Panel selected, List<Label> texts)
        {
            Theme normaltheme = normal.Theme;
            StyleBoxFlat normalsb = normal.GetThemeStylebox("panel") as StyleBoxFlat;
            normalsb.BgColor = LoadedTheme.DataGridA;
            normalsb.BorderColor = LoadedTheme.DataGridTextA;

            normaltheme.SetStylebox("panel", "Panel", normalsb);


            
            Theme selectedtheme = selected.Theme;
            StyleBoxFlat selectedsb = selected.GetThemeStylebox("panel") as StyleBoxFlat;
            selectedsb.BgColor = LoadedTheme.DataGridSelected;
            selectedsb.BorderColor = LoadedTheme.DataGridTextA;

            selectedtheme.SetStylebox("panel", "Panel", selectedsb);

            foreach (Label label in texts)
            {
                label.AddThemeColorOverride("font_color", LoadedTheme.DataGridTextA);
            }
        }
        
        public static void UpdateLoadOrderItemColors(Panel[] panels, Label text)
        {
            foreach (Panel panel in panels)
            {
                Theme normaltheme = panel.Theme;
                StyleBoxFlat normalsb = panel.GetThemeStylebox("panel") as StyleBoxFlat;
                normalsb.BgColor = LoadedTheme.DataGridA;
                normalsb.BorderColor = LoadedTheme.DataGridTextA;

                normaltheme.SetStylebox("panel", "Panel", normalsb);
            }
            

            
            text.AddThemeColorOverride("font_color", LoadedTheme.DataGridTextA);
            
        }
    }
}