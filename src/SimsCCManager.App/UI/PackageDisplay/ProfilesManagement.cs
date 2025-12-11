using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Globals;
using SimsCCManager.SettingsSystem;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ProfilesManagement : MarginContainer
{
    public PackageDisplay packageDisplay;
    [Export]
    TopbarButton ManageProfilesButton;
    [Export]
    OptionButton ProfileOptions;

    private List<string> Profiles = new();

    public delegate void ManageProfilesOpenEvent();
    public ManageProfilesOpenEvent ManageProfilesOpen;

    public delegate void ProfileChangedEvent(string profile, int idx);
    public ProfileChangedEvent ProfileChanged;

    public override void _Ready()
    {
        ManageProfilesButton.ButtonClicked += () => Manage();
        UpdateTheme();
        ProfileOptions.ItemSelected += (item) => ProfileSelected(item);
    }

    private void ProfileSelected(long item)
    {
        ProfileChanged.Invoke(Profiles[(int)item], (int)item);
    }


    public void UpdateProfileOptions()
    {
        Profiles.Clear();
        ProfileOptions.Clear();
        List<InstanceProfile> profiles = packageDisplay.ThisInstance.InstanceProfiles.ToList();
        ProfileOptions.AddItem(packageDisplay.ThisInstance.LoadedProfile.ProfileName);        
        Profiles.Add(packageDisplay.ThisInstance.LoadedProfile.ProfileName);
        foreach (InstanceProfile profile in profiles)
        {
            if (profile != packageDisplay.ThisInstance.LoadedProfile)
            {
                ProfileOptions.AddItem(profile.ProfileName);
                Profiles.Add(profile.ProfileName);
            }
        }

        ProfileOptions.Select(0);
    }

    private void Manage()
    {
        ManageProfilesOpen.Invoke();
    }

    private void UpdateTheme()
    {
        bool textLight = false;
        SCCMTheme theme = GlobalVariables.LoadedTheme;
        ProfileOptions.AddThemeColorOverride("font_color", theme.ButtonMain);
        ProfileOptions.AddThemeColorOverride("font_hover_color", theme.ButtonHover);
        StyleBoxFlat normalbox = ProfileOptions.GetThemeStylebox("normal") as StyleBoxFlat;
        StyleBoxFlat hoverbox = ProfileOptions.GetThemeStylebox("hover") as StyleBoxFlat;
        StyleBoxFlat focusbox = ProfileOptions.GetThemeStylebox("focus") as StyleBoxFlat;
        
        if (theme.ButtonMain.V > 0.5)
        {
            textLight = true;
        }

        normalbox.BorderColor = theme.AccentColor;

        if (theme.AccentColor.V > 0.5)
        {
            hoverbox.BorderColor = theme.AccentColor.Darkened(0.2f);
            focusbox.BorderColor = theme.AccentColor.Darkened(0.2f);
        } else
        {
            hoverbox.BorderColor = theme.AccentColor.Lightened(0.2f);
            focusbox.BorderColor = theme.AccentColor.Lightened(0.2f);
        }

        if (textLight)
        {
            normalbox.BgColor = theme.DataGridA.Darkened(0.2f);
            hoverbox.BgColor = theme.DataGridB.Darkened(0.2f);
            focusbox.BgColor = theme.DataGridB.Darkened(0.2f);
        } else
        {
            normalbox.BgColor = theme.DataGridA.Lightened(0.2f);
            hoverbox.BgColor = theme.DataGridB.Lightened(0.2f);
            focusbox.BgColor = theme.DataGridB.Lightened(0.2f);
        }

        ProfileOptions.AddThemeStyleboxOverride("normal", normalbox);
        ProfileOptions.AddThemeStyleboxOverride("focus", focusbox);
        ProfileOptions.AddThemeStyleboxOverride("hover", hoverbox);

    }
}
