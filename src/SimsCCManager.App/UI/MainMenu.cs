using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.Settings.Loaded;
using SimsCCManager.UI.Utilities;
using System;
using System.Linq;

public partial class MainMenu : MarginContainer
{
	PackedScene settingswindow = GD.Load<PackedScene>("res://UI/MainMenu_Elements/main_settings.tscn");
	MarginContainer NewInstanceMenu;
	MarginContainer MainMenuContainer;
	public delegate void MainMenuStartInstanceEvent(Guid instance);
	public MainMenuStartInstanceEvent MainMenuStartInstance;
	// Called when the node enters the scene tree for the first time.
	PackedScene newinstancemenu = GD.Load<PackedScene>("res://UI/MainMenu_Elements/new_instance.tscn");
	PackedScene loadinstancemenu = GD.Load<PackedScene>("res://UI/MainMenu_Elements/load_instance.tscn");
	public override void _Ready()
	{
		GetNode<MarginContainer>("Tali").Visible = LoadedSettings.SetSettings.ShowTali;
		GetNode<MarginContainer>("Menu/MarginContainer/VBoxContainer/MMButton_DevTest").Visible = LoadedSettings.SetSettings.DebugMode;
		//NewInstanceMenu = GetNode<MarginContainer>("NewInstance");
		MainMenuContainer = GetNode<MarginContainer>("Menu");
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("There are {0} instances loaded", LoadedSettings.SetSettings.Instances.Count));
		if (LoadedSettings.SetSettings.Instances.Count != 0) GetNode<MarginContainer>("Menu/MarginContainer/VBoxContainer/MMButton_LoadInstance").Visible = true;
	}

	private void _on_tali_mouse_entered(){
		string text = "In memory of my beloved Tali. My blanket will always have an empty space where you used to curl up while we coded this app.";
		ToolTip tooltip = UIUtilities.CustomTooltip(text, GetGlobalMousePosition());
		GetWindow().AddChild(tooltip);
	}
	
	private void _on_mm_button_new_instance_button_clicked(){
		var newinstance = newinstancemenu.Instantiate() as NewInstance;
		//newinstance.Connect("tree_exited", Callable.From(CancelledInstance));
		newinstance.TreeExited += () => CancelledInstance();

		//newinstance.Connect("NewInstanceStartPackageManager", new Callable(this, "LoadInstance"));
		newinstance.NewInstanceStart += (instance) => LoadInstance(instance);
		MainMenuContainer.Visible = false;		
		AddChild(newinstance);
	}
	private void _on_mm_button_load_instance_button_clicked(){
		MainMenuContainer.Visible = false;
		var loadinstance = loadinstancemenu.Instantiate() as LoadInstance;
		loadinstance.TreeExited += () => CancelledInstance();
		loadinstance.LoadInstanceStartPackageManager += (instance) => LoadInstance(instance);
		//loadinstance.Connect("tree_exited", Callable.From(CancelledInstance));
		//loadinstance.Connect("LoadInstanceStartPackageManager", new Callable(this, "LoadInstance"));
		AddChild(loadinstance);
	}

	private void LoadInstance(Guid instance){
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Heard the signal to load instance on Main Menu!"));
		MainMenuStartInstance.Invoke(instance);
	}
	private void _on_mm_button_settings_button_clicked(){
		var swindow = settingswindow.Instantiate();
		swindow.Connect("Tali", Callable.From(TaliEmitted));
		swindow.Connect("Debug", Callable.From(DebugEmitted));
		AddChild(swindow);
	}
	private void _on_mm_button_help_button_clicked(){
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog("Help");
	}
	private void _on_mm_button_quit_button_clicked(){
		GetTree().Quit();
	}

	private void _on_mm_button_dev_test_button_clicked(){
		
	}

	private void TaliEmitted(){
		GetNode<MarginContainer>("Tali").Visible = LoadedSettings.SetSettings.ShowTali;
	}
	private void DebugEmitted(){
		GetNode<MarginContainer>("Menu/MarginContainer/VBoxContainer/MMButton_DevTest").Visible = LoadedSettings.SetSettings.DebugMode;
	}
	private void CancelledInstance(){
		MainMenuContainer.Visible = true;
	}
}
