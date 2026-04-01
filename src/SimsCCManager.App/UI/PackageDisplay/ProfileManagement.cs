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
using System.Reflection.Metadata.Ecma335;

public partial class ProfileManagement : MarginContainer
{
    public PackageDisplay packageDisplay;
    [ExportCategory("PackedScenes")]
    [Export]
    PackedScene ProfileItemPS;
    [ExportCategory("Nodes")]
    [Export]
    Panel BgPanel;
    [Export]
    Label ProfilesLabel;
    [Export]
    Panel ListBg;
    [Export]
    VBoxContainer ProfilesList;
    [Export]
    Button NewProfileButton;
    [Export]
    Button EditProfileButton;
    [Export]
    Button DupeProfileButton;
    [Export]
    Button DeleteProfileButton;
    [Export]
    Button CloseButton;
    [ExportCategory("MiniWindow")]
    [Export]
    MarginContainer MiniWindow;
    [Export]
    LineEdit ProfileNameBox;
    [Export]
    TextEdit ProfileDescriptionBox;
    [Export]
    Button ConfirmButton;
    [Export]
    Button CancelButton;
    [Export]
    Label LocalSavesLabel;
    [Export]
    CustomCheckButton LocalSavesCheck;
    [Export]
    Label LocalSettingsLabel;
    [Export]
    CustomCheckButton LocalSettingsCheck;
    [Export]
    Label LocalMediaLabel;
    [Export]
    CustomCheckButton LocalMediaCheck;
    [Export]
    Label LocalDataLabel;
    [Export]
    CustomCheckButton LocalDataCheck;
    [Export]
    Label AutoBackupsLabel;
    [Export]
    CustomCheckButton AutoBackupsCheck;


    [Export]
    Panel SecondBackground;
    [Export]
    Panel InternalBackground;

    List<Button> Buttons = new(); 

    List<ProfileItem> ProfileItems = new();

    public delegate void ProfilesUpdatedEvent();
    public ProfilesUpdatedEvent ProfilesUpdated;


    bool MakingNew = false;
    bool EditingOld = false;

    public override void _Ready()
    {
        Buttons.Add(NewProfileButton);
        Buttons.Add(EditProfileButton);
        Buttons.Add(DupeProfileButton);
        Buttons.Add(DeleteProfileButton);
        Buttons.Add(CloseButton);
        Buttons.Add(ConfirmButton);
        Buttons.Add(CancelButton);
        UpdateTheme();

        foreach (InstanceProfile Profile in packageDisplay.ThisInstance.InstanceProfiles)
        {
            AddProfileItem(Profile);
        }

        NewProfileButton.Pressed += () => NewProfile();
        EditProfileButton.Pressed += () => EditProfile();
        DupeProfileButton.Pressed += () => DupeProfile();
        DeleteProfileButton.Pressed += () => DeleteProfile();

        CloseButton.Pressed += () => ClosePanel();

        ConfirmButton.Pressed += () => ProfileChangeConfirm();
        CancelButton.Pressed += () => ProfileChangeCancel();
    }

    private void ProfileChangeCancel()
    {
        MiniWindow.Visible = false;
        MakingNew = false;
        EditingOld = false;
    }


    private void ProfileChangeConfirm()
    {
        if (MakingNew)
        {
        
            string name = ProfileNameBox.Text;
            if (packageDisplay.ThisInstance.InstanceProfiles.Where(x => x.ProfileName == name).Any()) 
            {
                name = IncProfileName(name);
            }
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format(name));
            InstanceProfile profile = new();
            profile.ProfileName = name;
            profile.EnabledPackages = new();
            profile.ProfileDescription = ProfileDescriptionBox.Text;    
            profile.ProfileFolder = Path.Combine(packageDisplay.ThisInstance.InstanceFolders.InstanceProfilesFolder, profile.SafeFileName());
            Directory.CreateDirectory(profile.ProfileFolder);
            profile.LocalData = LocalDataCheck.IsToggled;
            profile.LocalSaves = LocalSavesCheck.IsToggled;
            profile.LocalMedia = LocalMediaCheck.IsToggled;
            profile.LocalSettings = LocalSettingsCheck.IsToggled;  
            profile.AutoBackups = AutoBackupsCheck.IsToggled;        
            packageDisplay.ThisInstance.AddProfile(profile);
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format(profile.ToString()));
            packageDisplay.ThisInstance.WriteXML();
            AddProfileItem(profile);            
        } else if (EditingOld)
        {
            ProfileItem pi = ProfileItems.Where(x => x.IsSelected).First();
            ProfileItems.Remove(pi);
            pi.QueueFree();
            InstanceProfile profile = packageDisplay.ThisInstance.InstanceProfiles.First(x => x.ProfileName == pi.ProfileName.Text);
            bool current = false;
            if (packageDisplay.ThisInstance.LoadedProfile.ProfileIdentifier == profile.ProfileIdentifier) current = true;
            if (current)
            {
                string ogname = packageDisplay.ThisInstance.LoadedProfile.ProfileName;
                string ogfolder = packageDisplay.ThisInstance.LoadedProfile.ProfileFolder;
                packageDisplay.ThisInstance.LoadedProfile.ProfileName = ProfileNameBox.Text;
                packageDisplay.ThisInstance.LoadedProfile.ProfileDescription = ProfileDescriptionBox.Text;
                packageDisplay.ThisInstance.LoadedProfile.ProfileFolder = Path.Combine(packageDisplay.ThisInstance.InstanceFolders.InstanceProfilesFolder, profile.SafeFileName());
                if (Directory.Exists(ogfolder))
                {
                    Directory.Move(ogfolder, packageDisplay.ThisInstance.LoadedProfile.ProfileFolder);
                }
                packageDisplay.ThisInstance.LoadedProfile.LocalData = LocalDataCheck.IsToggled;
                packageDisplay.ThisInstance.LoadedProfile.LocalSaves = LocalSavesCheck.IsToggled;
                packageDisplay.ThisInstance.LoadedProfile.LocalMedia = LocalMediaCheck.IsToggled;
                packageDisplay.ThisInstance.LoadedProfile.AutoBackups = AutoBackupsCheck.IsToggled;     
                packageDisplay.ThisInstance.LoadedProfile.LocalSettings = LocalSettingsCheck.IsToggled;                
                switch (packageDisplay.ThisInstance.GameChoice)
                {
                    case SimsGames.Sims2:
                        InstanceControllers.GetSims2LocalFiles(packageDisplay.ThisInstance);
                    break;
                    case SimsGames.Sims3:
                        InstanceControllers.GetSims3LocalFiles(packageDisplay.ThisInstance);
                    break;
                    case SimsGames.Sims4:
                        InstanceControllers.GetSims4LocalFiles(packageDisplay.ThisInstance);
                    break;
                }
            } else
            {
                packageDisplay.ThisInstance.InstanceProfiles.Remove(profile);
                profile.ProfileName = ProfileNameBox.Text;
                profile.ProfileDescription = ProfileDescriptionBox.Text;
                profile.ProfileFolder = Path.Combine(packageDisplay.ThisInstance.InstanceFolders.InstanceProfilesFolder, profile.SafeFileName());
                Directory.CreateDirectory(profile.ProfileFolder);
                profile.LocalData = LocalDataCheck.IsToggled;
                profile.LocalSaves = LocalSavesCheck.IsToggled;
                profile.LocalMedia = LocalMediaCheck.IsToggled;
                profile.AutoBackups = AutoBackupsCheck.IsToggled;     
                profile.LocalSettings = LocalSettingsCheck.IsToggled;                
                packageDisplay.ThisInstance.AddProfile(profile);
            }
            AddProfileItem(profile);                       
        }
        MakingNew = false;
        EditingOld = false;
        MiniWindow.Visible = false;
        ProfilesUpdated.Invoke();
    }

    private string IncProfileName(string name, int inc = 0)
    {
        inc++;
        name = string.Format("{0} ({1})", name, inc);
        if (packageDisplay.ThisInstance.InstanceProfiles.Any(x => x.ProfileName == name))
        {
            name = IncProfileName(name, inc);
        }
        return name;
    }


    private void DeleteProfile()
    {
        if (ProfileItems.Count == 1)
        {
            //
        } else
        {
            ProfileItem pi = ProfileItems.First(x => x.IsSelected);            
            InstanceProfile profile = packageDisplay.ThisInstance.InstanceProfiles.First(x => x.ProfileName == pi.ProfileName.Text);
            ProfileItems.Remove(pi);
            pi.QueueFree();
            packageDisplay.ThisInstance.RemoveProfile(profile);
            ProfilesUpdated.Invoke();            
        }
    }


    private void DupeProfile()
    {
        ProfileItem pi = ProfileItems.Where(x => x.IsSelected).First();
        InstanceProfile profile = packageDisplay.ThisInstance.InstanceProfiles.Where(x => x.ProfileName == pi.ProfileName.Text).First();
        InstanceProfile profileCopy = new();
        profileCopy.EnabledPackages.AddRange(profile.EnabledPackages);
        profileCopy.ProfileDescription = profile.ProfileDescription;
        profileCopy.ProfileName = string.Format("{0} - Copy", profile.ProfileName);
        profileCopy.LocalData = profile.LocalData;
        profileCopy.LocalSaves = profile.LocalSaves;
        profileCopy.LocalMedia = profile.LocalMedia;
        profileCopy.LocalSettings = profile.LocalSettings;
        profileCopy.AutoBackups = profile.AutoBackups;
        packageDisplay.ThisInstance.AddProfile(profileCopy);
        AddProfileItem(profileCopy);
        ProfilesUpdated.Invoke();
    }

    private void AddProfileItem(InstanceProfile Profile)
    {
        ProfileItem profileItem = ProfileItemPS.Instantiate() as ProfileItem;
        profileItem.TextColor = GlobalVariables.LoadedTheme.MainTextColor;
        ProfileItems.Add(profileItem);
        profileItem.ProfileButton.Pressed += () => ProfileItemClicked(profileItem);
        profileItem.ProfileName.Text = Profile.ProfileName;
        ProfilesList.AddChild(profileItem);        
    }


    private void EditProfile()
    {
        MakingNew = false;
        EditingOld = true;
        MiniWindow.Visible = true;
        ProfileItem pi = ProfileItems.First(x => x.IsSelected);
        InstanceProfile profile = packageDisplay.ThisInstance.InstanceProfiles.First(x => x.ProfileName == pi.ProfileName.Text);
        ProfileNameBox.Text = profile.ProfileName;
        ProfileDescriptionBox.Text = profile.ProfileDescription;
        LocalSavesCheck.IsToggled = profile.LocalSaves;
        LocalSettingsCheck.IsToggled = profile.LocalSettings;
        LocalMediaCheck.IsToggled = profile.LocalMedia;
        LocalDataCheck.IsToggled = profile.LocalData;
        AutoBackupsCheck.IsToggled = profile.AutoBackups;
    }

    private void NewProfile()
    {
        MakingNew = true;
        EditingOld = false;
        MiniWindow.Visible = true;
        ProfileNameBox.Text = string.Empty;
        ProfileDescriptionBox.Text = string.Empty;
    }


    private void ClosePanel()
    {
        packageDisplay.LockInput = false;
        QueueFree();
    }


    private void ProfileItemClicked(ProfileItem profileItem)
    {
        profileItem.IsSelected = !profileItem.IsSelected; 
        foreach (ProfileItem item in ProfileItems)
        {
            if (item != profileItem)
            {
                item.IsSelected = false;
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
        ProfilesLabel.AddThemeColorOverride("font_color", theme.HeaderTextColor);
        LocalDataLabel.AddThemeColorOverride("font_color", theme.HeaderTextColor);
        LocalSettingsLabel.AddThemeColorOverride("font_color", theme.HeaderTextColor);
        LocalSavesLabel.AddThemeColorOverride("font_color", theme.HeaderTextColor);
        LocalMediaLabel.AddThemeColorOverride("font_color", theme.HeaderTextColor);
        AutoBackupsLabel.AddThemeColorOverride("font_color", theme.HeaderTextColor);


        StyleBoxFlat sb = SecondBackground.GetThemeStylebox("panel") as StyleBoxFlat;
        sb.BgColor = theme.BackgroundColor;
        sb.BorderColor = theme.AccentColor;

        StyleBoxFlat ib = InternalBackground.GetThemeStylebox("panel") as StyleBoxFlat;
        ib.BgColor = theme.BackgroundColor;
        ib.BorderColor = theme.AccentColor;
        
        Theme boxtheme = ProfileDescriptionBox.Theme;


        StyleBoxFlat tbf = ProfileDescriptionBox.GetThemeStylebox("focus") as StyleBoxFlat;
        StyleBoxFlat tbn = ProfileDescriptionBox.GetThemeStylebox("normal") as StyleBoxFlat;

        if (theme.IsThemeLight())
        {
            tbn.BgColor = theme.BackgroundColor.Darkened(0.05f);
            tbn.BorderColor = Color.FromHsv(theme.AccentColor.H, theme.AccentColor.S - 0.25f, theme.AccentColor.V);
            tbf.BgColor = tbn.BgColor.Darkened(0.1f);
        } else
        {
            tbn.BgColor = theme.BackgroundColor.Lightened(0.05f);
            tbn.BorderColor = Color.FromHsv(theme.AccentColor.H, theme.AccentColor.S - 0.25f, theme.AccentColor.V);
            tbf.BgColor = tbn.BgColor.Darkened(0.1f);
        }
        tbf.BorderColor = tbn.BorderColor.Lightened(0.1f);

        boxtheme.SetStylebox("normal", "TextEdit", tbn);
        boxtheme.SetStylebox("focus", "TextEdit", tbf);
        boxtheme.SetStylebox("normal", "LineEdit", tbn);
        boxtheme.SetStylebox("focus", "LineEdit", tbf);

        boxtheme.SetColor("font_color", "TextEdit", theme.MainTextColor);
        boxtheme.SetColor("font_color", "LineEdit", theme.MainTextColor);
        boxtheme.SetColor("font_placeholder_color", "TextEdit", theme.MainTextColor.Lightened(0.2f));
        boxtheme.SetColor("font_placeholder_color", "LineEdit", theme.MainTextColor.Lightened(0.2f));
        









    }

}
