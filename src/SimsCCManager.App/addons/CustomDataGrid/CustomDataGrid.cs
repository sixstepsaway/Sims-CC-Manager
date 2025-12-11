#if TOOLS
using Godot;
using System;

[Tool]
public partial class CustomDataGrid : EditorPlugin
{
	/// <summary>
	/// The editor plugin data for the custom data grid
	/// </summary>
	//PackedScene DataGrid = GD.Load<PackedScene>("res://addons/CustomDataGrid/DataGrid/DataGrid.tscn");
	public override void _EnterTree()
	{
		var script = GD.Load<Script>("res://addons/CustomDataGrid/DataGrid/DataGrid.cs");
		var texture = GD.Load<Texture2D>("res://addons/CustomDataGrid/Icon.png");
		AddCustomType("DataGrid", "Control", script, texture);
		//GetParent().AddChild(DataGrid.Instantiate());
	}

	public override void _ExitTree()
	{
		RemoveCustomType("DataGrid");
	}
}
#endif
