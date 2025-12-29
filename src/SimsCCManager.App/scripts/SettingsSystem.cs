using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;

namespace SimsCCManager.SettingsSystem
{
    public class SCCMSettings
    {
        public bool DebugMode { get; set; } = false;
        public bool PortableMode { get; set; } = false;
        public bool CPURestrict { get; set; } = false;
        public bool AutoLoad { get; set; } = false;
        public bool Tali { get; set; } = true;
        public string LoadedTheme = Themes.DefaultThemes[0].ThemeName;
        [XmlIgnore]
        public List<string> ThemeOptions = new() { "Default Dark", "Default Light", "Blossom", "Land of Ice", "Green Queen", "Old Valyria", "Teal Snow" };
        public List<Instance> InstanceFolders = new();

        public void SaveSettings()
        {
            XmlSerializer SettingsSerializer = new XmlSerializer(typeof(SCCMSettings));
            using (var writer = new StreamWriter(GlobalVariables.SettingsFile))
            {
                SettingsSerializer.Serialize(writer, this);
            }
        }
    }

    public class Themes
    {
        public static List<SCCMTheme> AllThemes = new();
        public static List<SCCMTheme> DefaultThemes = [
        new()
        {
            ThemeName = "Default Dark",
            Identifier = Guid.Parse("4305e7e5-0a03-4625-a4ab-2a688ae9d9e3"),
            BackgroundColor = Godot.Color.FromHtml("262529"),
            ButtonMain = Godot.Color.FromHtml("8688a9"),
            ButtonHover = Godot.Color.FromHtml("ccabd8"),
            ButtonClick = Godot.Color.FromHtml("e0cce7"),
            DataGridA = Godot.Color.FromHtml("EBEDEF"),
            DataGridB = Godot.Color.FromHtml("C9D1D9"),
            DataGridSelected = Godot.Color.FromHtml("89AEDB"),
            AccentColor = Godot.Color.FromHtml("83d1cb"),
            DataGridTextA = Godot.Color.FromHtml("3B424A"),
            DataGridTextB = Godot.Color.FromHtml("2F3842"),
            MainTextColor = Godot.Color.FromHtml("D5D1EB"),
            HeaderTextColor = Godot.Color.FromHtml("D9ABD6")
        }, new() {
            ThemeName = "Blossom",
            Identifier = Guid.Parse("a92902ea-3d8c-44ec-b858-b6111c0afa50"),
            BackgroundColor = Godot.Color.FromHtml("EDE3E4"),
            ButtonMain = Godot.Color.FromHtml("91586d"),
            ButtonHover = Godot.Color.FromHtml("c46ca2"),
            ButtonClick = Godot.Color.FromHtml("cb8dd3"),
            DataGridA = Godot.Color.FromHtml("EAE1E6"),
            DataGridB = Godot.Color.FromHtml("E9E0E7"),
            DataGridSelected = Godot.Color.FromHtml("CABFC5"),
            AccentColor = Godot.Color.FromHtml("C24D76"),
            DataGridTextA = Godot.Color.FromHtml("DAA5C2"),
            DataGridTextB = Godot.Color.FromHtml("AB8AA4"),
            MainTextColor = Godot.Color.FromHtml("3F3536"),
            HeaderTextColor = Godot.Color.FromHtml("9A7E88")
        }, new()
        {
            ThemeName = "Land of Ice",
            Identifier = Guid.Parse("c17768b6-0f9f-407f-af95-8b50d3333365"),
            BackgroundColor = Godot.Color.FromHtml("3E442B"),
            ButtonMain = Godot.Color.FromHtml("8D91B5"),
            ButtonHover = Godot.Color.FromHtml("979BBC"),
            ButtonClick = Godot.Color.FromHtml("A0A4C2"),
            DataGridA = Godot.Color.FromHtml("8D909B"),
            DataGridB = Godot.Color.FromHtml("AAADC4"),
            DataGridSelected = Godot.Color.FromHtml("B3D5DA"),
            AccentColor = Godot.Color.FromHtml("6A7062"),
            DataGridTextA = Godot.Color.FromHtml("2E3962"),
            DataGridTextB = Godot.Color.FromHtml("545C8D"),
            MainTextColor = Godot.Color.FromHtml("B8BCAA"),
            HeaderTextColor = Godot.Color.FromHtml("5B6085")
        }, new()
        {
            ThemeName = "Green Queen",
            Identifier = Guid.Parse("a330b8f8-8ceb-412f-ad38-755506ce2da2"),
            BackgroundColor = Godot.Color.FromHtml("070c09"),
            ButtonMain = Godot.Color.FromHtml("35593d"),
            ButtonHover = Godot.Color.FromHtml("263f2c"),
            ButtonClick = Godot.Color.FromHtml("17261a"),
            DataGridA = Godot.Color.FromHtml("45724f"),
            DataGridTextA = Godot.Color.FromHtml("72bf83"),
            DataGridB = Godot.Color.FromHtml("548c60"),
            DataGridTextB = Godot.Color.FromHtml("82d895"),
            DataGridSelected = Godot.Color.FromHtml("35593d"),
            AccentColor = Godot.Color.FromHtml("91f2a6"),
            MainTextColor = Godot.Color.FromHtml("b9f2c6"),
            HeaderTextColor = Godot.Color.FromHtml("72bf83")
        }, new()
        {
            ThemeName = "Old Valyria",
            Identifier = Guid.Parse("3a18f7ef-ca20-47d4-909d-f45373a044b8"),
            BackgroundColor = Godot.Color.FromHtml("171614"),
            ButtonMain = Godot.Color.FromHtml("754043"),
            ButtonHover = Godot.Color.FromHtml("9A8873"),
            ButtonClick = Godot.Color.FromHtml("3A2618"),
            DataGridA = Godot.Color.FromHtml("673A39"),
            DataGridB = Godot.Color.FromHtml("58332E"),
            DataGridSelected = Godot.Color.FromHtml(""),
            AccentColor = Godot.Color.FromHtml("37423D"),
            DataGridTextA = Godot.Color.FromHtml("D48E8D"),
            DataGridTextB = Godot.Color.FromHtml("C5847B"),
            MainTextColor = Godot.Color.FromHtml("968C85"),
            HeaderTextColor = Godot.Color.FromHtml("B86062")
        }, new()
        {
            ThemeName = "Default Light",
            Identifier = Guid.Parse("affdd6c9-0b8d-4623-a814-d67e6931994b"),
            BackgroundColor = Godot.Color.FromHtml("B5D3DD"),
            ButtonMain = Godot.Color.FromHtml("EDA2C0"),
            ButtonHover = Godot.Color.FromHtml("BF9ACA"),
            ButtonClick = Godot.Color.FromHtml("8E4162"),
            DataGridA = Godot.Color.FromHtml("C7E8F3"),
            DataGridB = Godot.Color.FromHtml("C3C1DF"),
            DataGridSelected = Godot.Color.FromHtml("C1AED5"),
            AccentColor = Godot.Color.FromHtml("41393E"),
            DataGridTextA = Godot.Color.FromHtml("273A40"),
            DataGridTextB = Godot.Color.FromHtml("272539"),
            MainTextColor = Godot.Color.FromHtml("1F2F35"),
            HeaderTextColor = Godot.Color.FromHtml("D69EC5")
        }, new()
        {
            ThemeName = "Teal Snow",
            Identifier = Guid.Parse("01541480-e16a-4808-b719-992a69f03867"),
            BackgroundColor = Godot.Color.FromHtml("FFFFFA"),
            ButtonMain = Godot.Color.FromHtml("0D5C63"),
            ButtonHover = Godot.Color.FromHtml("44A1A0"),
            ButtonClick = Godot.Color.FromHtml("78CDD7"),
            DataGridA = Godot.Color.FromHtml("A0DADD"),
            DataGridB = Godot.Color.FromHtml("7ECACA"),
            DataGridSelected = Godot.Color.FromHtml("388787"),
            AccentColor = Godot.Color.FromHtml("6397BF"),
            DataGridTextA = Godot.Color.FromHtml("4F989C"),
            DataGridTextB = Godot.Color.FromHtml("3B9393"),
            MainTextColor = Godot.Color.FromHtml("04282B"),
            HeaderTextColor = Godot.Color.FromHtml("0D5C63")
        }];

        public static void CreateThemeFiles()
        {
            XmlSerializer ThemeWriter = new(typeof(SCCMTheme));
            foreach (SCCMTheme theme in DefaultThemes)
            {
                string themefile = Path.Combine(GlobalVariables.ThemesFolder, string.Format("{0}.xml", theme.ThemeName));
                using (var writer = new StreamWriter(themefile))
                {
                    ThemeWriter.Serialize(writer, theme);
                }
            }
            AllThemes = DefaultThemes;
        }
    }
    public class SCCMTheme
    {
        public string ThemeName { get; set; } = "";
        public Guid Identifier { get; set; } = Guid.NewGuid();
        [XmlIgnore]
        public Godot.Color BackgroundColor { get; set; } = Godot.Color.Color8(0, 0, 0);
        [XmlIgnore]
        public Godot.Color ButtonMain { get; set; } = Godot.Color.Color8(0, 0, 0);
        [XmlIgnore]
        public Godot.Color ButtonHover { get; set; } = Godot.Color.Color8(0, 0, 0);
        [XmlIgnore]
        public Godot.Color ButtonClick { get; set; } = Godot.Color.Color8(0, 0, 0);
        [XmlIgnore]
        public Godot.Color DataGridA { get; set; } = Godot.Color.Color8(0, 0, 0);
        [XmlIgnore]
        public Godot.Color DataGridTextA { get; set; } = Godot.Color.Color8(0, 0, 0);
        [XmlIgnore]
        public Godot.Color DataGridB { get; set; } = Godot.Color.Color8(0, 0, 0);
        [XmlIgnore]
        public Godot.Color DataGridTextB { get; set; } = Godot.Color.Color8(0, 0, 0);
        [XmlIgnore]
        public Godot.Color DataGridSelected { get; set; } = Godot.Color.Color8(0, 0, 0);
        [XmlIgnore]
        public Godot.Color AccentColor { get; set; } = Godot.Color.Color8(0, 0, 0);
        [XmlIgnore]
        public Godot.Color MainTextColor { get; set; } = Godot.Color.Color8(0, 0, 0);
        [XmlIgnore]
        public Godot.Color HeaderTextColor { get; set; } = Godot.Color.Color8(0, 0, 0);
        public string backgroundColor {get { return BackgroundColor.ToHtml(); } set { BackgroundColor = Godot.Color.FromHtml(value); } }
        public string buttonMain {get { return ButtonMain.ToHtml(); } set { ButtonMain = Godot.Color.FromHtml(value); } }
        public string buttonHover {get { return ButtonHover.ToHtml(); } set { ButtonHover = Godot.Color.FromHtml(value); } }
        public string buttonClick {get { return ButtonClick.ToHtml(); } set { ButtonClick = Godot.Color.FromHtml(value); } }
        public string dataGridA {get { return DataGridA.ToHtml(); } set { DataGridA = Godot.Color.FromHtml(value); } }
        public string dataGridTextA {get { return DataGridTextA.ToHtml(); } set { DataGridTextA = Godot.Color.FromHtml(value); } }
        public string dataGridB {get { return DataGridB.ToHtml(); } set { DataGridB = Godot.Color.FromHtml(value); } }
        public string dataGridTextB {get { return DataGridTextB.ToHtml(); } set { DataGridTextB = Godot.Color.FromHtml(value); } }
        public string dataGridSelected {get { return DataGridSelected.ToHtml(); } set { DataGridSelected = Godot.Color.FromHtml(value); } }
        public string accentColor {get { return AccentColor.ToHtml(); } set { AccentColor = Godot.Color.FromHtml(value); } }
        public string mainTextColor {get { return MainTextColor.ToHtml(); } set { MainTextColor = Godot.Color.FromHtml(value); } }
        public string headerTextColor {get { return HeaderTextColor.ToHtml(); } set { HeaderTextColor = Godot.Color.FromHtml(value); } }


        public dynamic GetProperty(string propName)
        {
            var prop = this.ProcessProperty(propName);
            if (prop.GetType() == typeof(string))
            {
                return prop.ToString();
            }
            else if (prop.GetType() == typeof(DateTime))
            {
                DateTime dt = (DateTime)prop;
                return dt.ToString("MM/dd/yyyy H:mm");
            }
            else if (prop.GetType() == typeof(bool))
            {
                return prop;
            }
            else if (prop.GetType() == typeof(SCCMTheme))
            {
                return prop;
            }
            else
            {
                return "";
            }
        }

        public void SetProperty(string propName, dynamic input)
        {
            Logging.WriteDebugLog(string.Format("Processing property {0}, value {1}", propName, input.GetType()));
            var prop = this.ProcessProperty(propName);
            PropertyInfo property = this.GetType().GetProperty(propName);
            Logging.WriteDebugLog(string.Format("Property type: {0}", property.PropertyType));
            if (property != null)
            {
                if (property.PropertyType == typeof(Godot.Color))
                {
                    Godot.Color newcolor = Godot.Color.FromHtml(input);
                    property.SetValue(this, newcolor);
                }
                else if (property.PropertyType == typeof(Guid))
                {
                    string inp = input as string;
                    property.SetValue(this, Guid.Parse(inp));
                }
                else if (property.PropertyType == typeof(string))
                {
                    property.SetValue(this, input as string);
                }
            }
        }

        public object ProcessProperty(string propName)
        {
            return this.GetType().GetProperty(propName).GetValue(this, null);
        }
    }
}