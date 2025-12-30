using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.OptionLists;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Threading;
using System.Xml.Serialization;

public partial class LoadInstance : MarginContainer
{
    [Export]
    ColorRect ListBGColor;
    [Export]
    VBoxContainer InstanceList;
    [Export]
    MMButton CancelButton;
    [Export]
    MMButton LoadButton;
    [Export]
    PackedScene InstancePickerPS;
    List<InstancePicker> InstanceOptions = new();

    [Export]
    Control AcceptDialogHolder;
    [Export]
    Button AcceptDialogOK;
    [Export]
    Button AcceptDialogCANCEL;

    Guid selectedInstance;
    Guid InstanceToDelete;

    GameInstance gameInstance = new();

    public override void _Ready()
    {
        CancelButton.ButtonClicked += () => CloseWindow();
        LoadButton.ButtonClicked += () => LoadPickedInstance();

        AcceptDialogOK.Pressed += () => YesDelete();
        AcceptDialogCANCEL.Pressed += () => NoDelete();

        RefreshList();        
    }

    private void NoDelete()
    {
        AcceptDialogHolder.Visible = false;
    }


    private void YesDelete()
    {        
        GlobalVariables.LoadedSettings.InstanceFolders.Remove(GlobalVariables.LoadedSettings.InstanceFolders.Where(x => x.InstanceID == InstanceToDelete).First());
        GlobalVariables.LoadedSettings.SaveSettings();
        RefreshList();
        AcceptDialogHolder.Visible = false;
    }


    private void DeleteAnInstance(Guid instance)
    {
        AcceptDialogHolder.Visible = true;
        InstanceToDelete = instance;
    }

    private void RefreshList()
    {
        foreach (InstancePicker ip in InstanceOptions)
        {
            ip.QueueFree();
        }
        InstanceOptions = new();
        List<Instance> toremove = new();

        foreach (Instance instance in GlobalVariables.LoadedSettings.InstanceFolders)
        {
            if (!Directory.Exists(instance.InstanceLocation))
            {
                toremove.Add(instance);                
            } else
            {               
                InstancePicker instancepicker = InstancePickerPS.Instantiate() as InstancePicker;
                instancepicker.SelectionColor.Color = GlobalVariables.LoadedTheme.AccentColor;

                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Creating pickerbox for Instance {0}:\n{1}\nCreated: {2}\nModified: {3}\nFor Game: {4}", instance.InstanceName, instance.InstanceID, instance.InstanceCreated, instance.InstanceLastModified, instance.Game.ToString()));

                instancepicker.PickedInstance += (instance, selected) => InstancePicked(instance, selected);
                instancepicker.InstanceIdentifier = instance.InstanceID;
                instancepicker.GameLabel.Text = instance.InstanceName;
                switch (instance.Game)
                {
                    case SimsGames.Sims1:
                        instancepicker.GameIcon.Texture = instancepicker.Sims1Icon;
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Setting icon to Sims 1."));
                    break;
                    case SimsGames.Sims2:
                        instancepicker.GameIcon.Texture = instancepicker.Sims2Icon;
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Setting icon to Sims 2."));
                    break;
                    case SimsGames.Sims3:
                        instancepicker.GameIcon.Texture = instancepicker.Sims3Icon;
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Setting icon to Sims 3."));
                    break;
                    case SimsGames.Sims4:
                        instancepicker.GameIcon.Texture = instancepicker.Sims4Icon;
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Setting icon to Sims 4."));
                    break;
                    case SimsGames.SimCity4:
                        instancepicker.GameIcon.Texture = instancepicker.SimCity4Icon;
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Setting icon to SC4."));
                    break;
                    case SimsGames.Spore:
                        instancepicker.GameIcon.Texture = instancepicker.SporeIcon;
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Setting icon to spore."));
                    break;
                    case SimsGames.SimsMedieval:
                        instancepicker.GameIcon.Texture = instancepicker.SimsMedievalIcon;
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Setting icon to Sims Medieval."));
                    break;
                }
                instancepicker.DateCreated = instance.InstanceCreated;
                instancepicker.DateModified = instance.InstanceLastModified;
                instancepicker.DeleteInstance += (instance) => DeleteAnInstance(instance);
                InstanceOptions.Add(instancepicker);
                InstanceList.AddChild(instancepicker);
            }

            foreach (Instance inst in toremove)
            {
                GlobalVariables.LoadedSettings.InstanceFolders.Remove(inst);
            }
        }
    }


    private void InstancePicked(Guid instance, bool selected)
    {
        foreach (InstancePicker ip in InstanceOptions)
        {
            if (ip.InstanceIdentifier != instance)
            {
                ip.SwapSelection(false);
            }
        }

        selectedInstance = instance;
    }


    private void LoadPickedInstance()
    {
        Instance loadedinstance = GlobalVariables.LoadedSettings.InstanceFolders.Where(x => x.InstanceID == selectedInstance).First();

        gameInstance = new();
        gameInstance.InstanceFolder = loadedinstance.InstanceLocation;        
        
        int pbarval = 0;
        if (File.Exists(gameInstance.XMLfile()))
        {
            XmlSerializer InstanceSerializer = new XmlSerializer(typeof(GameInstance));
            using (FileStream fileStream = new(gameInstance.XMLfile(), FileMode.Open, System.IO.FileAccess.Read)){
                using (StreamReader streamReader = new(fileStream)){
                    gameInstance = (GameInstance)InstanceSerializer.Deserialize(streamReader);
                    streamReader.Close();
                }
                fileStream.Close();
            }
        }

        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Loaded instance {0}. Current profile: {1}", gameInstance.InstanceName, gameInstance.LoadedProfile.ProfileName));

        List<string> files = Directory.EnumerateFiles(gameInstance.InstanceFolders.InstancePackagesFolder, "*.*", SearchOption.AllDirectories).Where(x => x.Contains(".package") || x.Contains(".ts4script")).ToList();
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found {0} files to be read into grid.", files.Count));
        List<string> downloadfiles = Directory.EnumerateFiles(gameInstance.InstanceFolders.InstanceDownloadsFolder, "*.*", SearchOption.AllDirectories).ToList();
        
        int fileCount = files.Count;
        int dFileCount = downloadfiles.Count;
        
        int pbarmax = fileCount + 50 + 30;

        



        GlobalVariables.mainWindow.LoadingPackageDisplayStart(pbarmax);

        new Thread(() => {
        //content
            GlobalVariables.mainWindow.IncrementLoadingScreen(10, "Loading instance...", "LoadInstance: First"); 
            pbarval += 10;
            GlobalVariables.mainWindow.IncrementLoadingScreen(10, "Loading packages...", "LoadInstance: Second");    
            pbarval += 10;
            gameInstance = InstanceControllers.LoadInstanceFiles(gameInstance);
            CallDeferred(nameof(FinishLoading));
        }){IsBackground = true}.Start();
    }

    

    private void FinishLoading()
    {
        GlobalVariables.mainWindow.LoadPackageDisplay(gameInstance, true);
    }

    private void CloseWindow()
    {
        this.QueueFree();
    }
}
