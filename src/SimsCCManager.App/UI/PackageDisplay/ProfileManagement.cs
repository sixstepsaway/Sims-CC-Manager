using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.SettingsSystem;
using System;
using System.Collections.Generic;
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
            profile.LocalData = LocalDataCheck.IsToggled;
            profile.LocalSaves = LocalSavesCheck.IsToggled;
            profile.LocalMedia = LocalMediaCheck.IsToggled;
            profile.LocalSettings = LocalSettingsCheck.IsToggled;        
            packageDisplay.ThisInstance.InstanceProfiles.Add(profile);
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format(profile.ToString()));
            packageDisplay.ThisInstance.WriteXML();
            AddProfileItem(profile);            
        } else if (EditingOld)
        {
            ProfileItem pi = ProfileItems.Where(x => x.IsSelected).First();
            ProfileItems.Remove(pi);
            pi.QueueFree();
            InstanceProfile profile = packageDisplay.ThisInstance.InstanceProfiles.Where(x => x.ProfileName == pi.ProfileName.Text).First();
            packageDisplay.ThisInstance.InstanceProfiles.Remove(profile);
            profile.ProfileName = ProfileNameBox.Text;
            profile.ProfileDescription = ProfileDescriptionBox.Text;
            profile.LocalData = LocalDataCheck.IsToggled;
            profile.LocalSaves = LocalSavesCheck.IsToggled;
            profile.LocalMedia = LocalMediaCheck.IsToggled;
            profile.LocalSettings = LocalSettingsCheck.IsToggled;
            packageDisplay.ThisInstance.InstanceProfiles.Add(profile);
            packageDisplay.ThisInstance.WriteXML();
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
        if (packageDisplay.ThisInstance.InstanceProfiles.Where(x => x.ProfileName == name).Any())
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
            ProfileItem pi = ProfileItems.Where(x => x.IsSelected).First();
            if (pi.ProfileName.Text != "Default")
            {
                InstanceProfile profile = packageDisplay.ThisInstance.InstanceProfiles.Where(x => x.ProfileName == pi.ProfileName.Text).First();
                ProfileItems.Remove(pi);
                pi.QueueFree();
                packageDisplay.ThisInstance.InstanceProfiles.Remove(profile);
                packageDisplay.ThisInstance.WriteXML();
                ProfilesUpdated.Invoke();
            }            
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
        packageDisplay.ThisInstance.InstanceProfiles.Add(profileCopy);
        packageDisplay.ThisInstance.WriteXML();
        AddProfileItem(profileCopy);
        ProfilesUpdated.Invoke();
    }

    private void AddProfileItem(InstanceProfile Profile)
    {
        ProfileItem profileItem = ProfileItemPS.Instantiate() as ProfileItem;
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
        ProfileItem pi = ProfileItems.Where(x => x.IsSelected).First();
        InstanceProfile profile = packageDisplay.ThisInstance.InstanceProfiles.Where(x => x.ProfileName == pi.ProfileName.Text).First();
        ProfileNameBox.Text = profile.ProfileName;
        ProfileDescriptionBox.Text = profile.ProfileDescription;
        LocalSavesCheck.IsToggled = profile.LocalSaves;
        LocalSettingsCheck.IsToggled = profile.LocalSettings;
        LocalMediaCheck.IsToggled = profile.LocalMedia;
        LocalDataCheck.IsToggled = profile.LocalData;
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
    }

}
