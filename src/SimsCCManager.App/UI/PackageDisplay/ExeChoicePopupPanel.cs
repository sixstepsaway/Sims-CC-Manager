using Godot;
using SimsCCManager.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class ExeChoicePopupPanel : Control
{
    public PackageDisplay packageDisplay;
    [Export]
    PackedScene ExeOptionItemPS;
    [Export]
    Control ClickyClock;
    [Export]
    public VBoxContainer ExeOptionHolder;
    [Export]
    TopbarButton AddExe;
    [Export]
    TopbarButton EditExe;
    [Export]
    TopbarButton RemoveExe;

    List<ExeOption> ExeOptions = new();

    

    public override void _Ready()
    {
        
        
        AddExe.ButtonClicked += () => AddNewExe();
        RemoveExe.ButtonClicked += () => DeleteExe();
        
    }

    private void DeleteExe()
    {
        if (ExeOptions.Count > 1)
        {
            ExeOption option = ExeOptions.Where(x => x.IsSelected).First();
            ExeOptions.Remove(option);
            Executable exe = packageDisplay.ThisInstance.Executables.Where(x => x.ExeName == option.ExeExe.Text).First();
            packageDisplay.ThisInstance.Executables.Remove(exe);
            option.QueueFree();
            ExePressed(ExeOptions[0]);            
            packageDisplay.ThisInstance.WriteXML();
        }        
    }


    public void GetExes()
    {
        foreach (Executable exe in packageDisplay.ThisInstance.Executables)
        {
            AddExeOption(exe);
        }
        packageDisplay.AddExeDialog.FileSelected += (file) => ExeAddPicked(file);
    }

    private void AddExeOption(Executable exe)
    {
        ExeOption exeOption = ExeOptionItemPS.Instantiate() as ExeOption;
        exeOption.button.Pressed += () => ExePressed(exeOption);
        exeOption.ExeName.Text = exe.Name;
        exeOption.ExeExe.Text = exe.ExeName;
        exeOption.ExeLocation.Text = exe.Path;            
        exeOption.ExeIcon.Texture = packageDisplay.UIGameStartControls.ExtractIcon(Path.Combine(exe.Path, exe.ExeName), exe.ExeName);
        ExeOptions.Add(exeOption);
        exeOption.ThixExe = exe;
        exeOption.IsSelected = exe.Selected;
        ExeOptionHolder.AddChild(exeOption);
    }

    private void ExeAddPicked(string file)
    {
        FileInfo fi = new(file);
        Executable exe = new();
        exe.ExeName = fi.Name;
        exe.Path = fi.DirectoryName;
        exe.Name = fi.Name.Replace(fi.Extension, "");
        packageDisplay.ThisInstance.Executables.Add(exe);
        packageDisplay.ThisInstance.WriteXML();
        AddExeOption(exe);
    }


    private void AddNewExe()
    {
        packageDisplay.AddExeDialog.Visible = true;
    }


    private void ExePressed(ExeOption exeOption)
    {
        foreach (ExeOption option in ExeOptions)
        {
            if (option != exeOption) option.IsSelected = false;
        }
        exeOption.IsSelected = true;
        packageDisplay.ThisInstance.CurrentExecutableIndex = packageDisplay.ThisInstance.Executables.IndexOf(exeOption.ThixExe);
        //packageDisplay.UIGameStartControls
    }


    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (!IsMouseInThis())
            {
                Visible = false;
            }
        }
    }

    private bool IsMouseInThis()
    {
        Rect2 rect2 = ClickyClock.GetRect();
        Vector2 mouse = GetGlobalMousePosition();
        if (rect2.HasPoint(mouse))
        {
            return true;
        } else
        {
            return false;
        }
    }


}
