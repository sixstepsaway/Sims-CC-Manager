using Godot;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.OptionLists;
using SimsCCManager.SettingsSystem;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

public partial class GameStartControls : MarginContainer
{
    public PackageDisplay PackageDisplay;
    GameInstance ThisInstance { 
        get { return PackageDisplay.ThisInstance; } 
    }

    [Export]
    Label ExeNameLabel;
    [Export]
    Label ExePathLabel;
    
    [Export]
    TextureRect ExeIcon;
    [Export]
    TopbarButton PlayButton;
    [Export]
    public Button GameChoiceDropDownButton;

    [ExportCategory("Theme")]
    [Export]
    ColorRect BorderColor;
    [Export]
    ColorRect BGColor;
    [Export]
    ColorRect IconColor;

    bool CheckForGame = false;

    public string RunningProcess = string.Empty;

    private bool _gamegoing;
    public bool GameGoing
    {
        get { return _gamegoing; }
        set { _gamegoing = value; 
            if (value)
            {
                CallDeferred(nameof(StopTimer));
            } else
            {
                RunningProcess = string.Empty;
            }
        }
    }

    Godot.Timer LauncherTimer = new();

    bool _fileslinked;
    bool FilesLinked {
        get { return _fileslinked; }
        set { _fileslinked = value;
        if (value == true) {
                new Thread(() => {
                    RunProcess(ThisInstance.ExecutableNamePath, ThisInstance.ExecutableArgs); 
                }){IsBackground = true}.Start();   
                LauncherTimer.Start();
            }           
        }
    }


    public override void _Ready()
    {
        GameGoing = false;
        PlayButton.ButtonClicked += () => Play();
        LauncherTimer.WaitTime = 90;
        LauncherTimer.Timeout += () => SimsDidntLoad();
        AddChild(LauncherTimer);
    }

    private void StopTimer()
    {
        LauncherTimer.Stop();
    }

    private void Play()
    {
        FilesLinked = PackageDisplay.LinkFiles();
    }


    public override void _Process(double delta)
    {
        ExeNameLabel.Text = ThisInstance.CurrentExecutable.Name;
        ExePathLabel.Text = ThisInstance.CurrentExecutable.ExeName;
        string exelocation = Path.Combine(ThisInstance.ExecutablePath, ThisInstance.ExecutableName);
            
        ExeIcon.Texture = ExtractIcon(ThisInstance.ExecutableNamePath, ThisInstance.ExecutableName);
    }


    public Texture2D ExtractIcon(string exelocation, string ExeName){
        string saveloc = Path.Combine(GlobalVariables.DataFolder, string.Format("{0}.png", ExeName));
        if (!File.Exists(saveloc))
        {
            
            System.Drawing.Bitmap icon = (System.Drawing.Bitmap)null;
            try
            {
                icon = Icon.ExtractAssociatedIcon(exelocation).ToBitmap();
            }
            catch (System.Exception)
            {
                // swallow and return nothing. You could supply a default Icon here as well
                return new Texture2D();
            }        
            icon.Save(saveloc, ImageFormat.Png);
        }        
        Godot.Image image = Godot.Image.LoadFromFile(saveloc);
        return ImageTexture.CreateFromImage(image);
    }



    public static string RunNonSimsProcess(string process, string parameters)
    {
        string result = String.Empty;
        FileInfo exe = new(process);
        string exename = exe.Name;

        if (!File.Exists(process)){
            //Logging.WriteDebugLog("Process was not found.");
        } else {

            //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Process: {0}", process));
            //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Params: {0}", parameters));
            string testresult = string.Empty;
            /*Console.WriteLine(parameters);*/

            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = process;
                p.StartInfo.Arguments = parameters;
                p.StartInfo.WorkingDirectory = new FileInfo(process).DirectoryName;
                
                
                p.Start();
                while (p.HasExited == false){
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(p.StandardOutput.Read().ToString());
                }
                p.WaitForExit();
                result = p.StandardOutput.ReadToEnd();
            }
        }
        return result;
    }

    public string RunProcess(string process, string parameters)
    {
        CheckForGame = true;
        string result = String.Empty;
        FileInfo exe = new(process);
        string exename = exe.Name;

        PackageDisplay.GameRunning = true;
        if (!File.Exists(process)){
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Process not found."));
        } else {
            //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Process: {0}", process));
            //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Params: {0}", parameters));
            string testresult = string.Empty;
            /*Console.WriteLine(parameters);*/

            new Thread(() => {
                while (CheckForGame){
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Looking to see if game is running."));
                    GameGoing = CheckForProcess(ThisInstance.GameChoice);
                    if (GameGoing) break;
                }
                while (GameGoing && CheckForGame){
                    GameGoing = CheckRunningProcess(RunningProcess);
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Game is running."));
                }

                PackageDisplay.GameRunning = false;
            }){IsBackground = true}.Start();

            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = process;
                p.StartInfo.Arguments = parameters;
                p.StartInfo.WorkingDirectory = new FileInfo(process).DirectoryName;
                
                
                p.Start();
                while (p.HasExited == false){
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(p.StandardOutput.Read().ToString());
                }
                p.WaitForExit();
                result = p.StandardOutput.ReadToEnd();
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(result);
                //if (ThisInstance.GameChoice != SimsGames.Sims4) PackageDisplay.GameRunning = false;
            
                
            }
        }        
        return result;
    }

    private void SimsDidntLoad()
    {
        CheckForGame = false;
        PackageDisplay.GameRunning = false;
    }

    private bool CheckRunningProcess(string exe)
    {
        bool anything = false;
        if (Process.GetProcessesByName(exe).Length == 0){
            //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Didn't find {0}", exe));
            anything = false;
        } else {
            //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found {0}", exe));
            RunningProcess = exe;
            anything = true;
        }
        return anything;
    }

    private bool CheckForProcess(SimsGames game){
        bool anything = false;
        if (game == SimsGames.Sims2){
            foreach (string exe in GlobalVariables.Sims2Exes){
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking for {0}", exe));
                if (Process.GetProcessesByName(exe).Length == 0){
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Didn't find {0}", exe));
                    anything = false;
                } else {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found {0}", exe));
                    RunningProcess = exe;
                    anything = true;
                }
            }
        } else if (game == SimsGames.Sims3){
            foreach (string exe in GlobalVariables.Sims3Exes){
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking for {0}", exe));
                if (Process.GetProcessesByName(exe).Length == 0){
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Didn't find {0}", exe));
                    anything = false;
                } else {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found {0}", exe));
                    RunningProcess = exe;
                    anything = true;
                }
            }

        } else if (game == SimsGames.Sims4){
            foreach (string exe in GlobalVariables.Sims4Exes){
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking for {0}", exe));
                if (Process.GetProcessesByName(exe).Length == 0){
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Didn't find {0}", exe));
                    anything = false;
                } else {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found {0}", exe));
                    RunningProcess = exe;
                    anything = true;
                }
            }
        }
        
        return anything;            
    }

    private static void StartProcess(string processname){
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog("Process started!");
        Process[] runninggame = Process.GetProcessesByName(processname);
        if (runninggame.Length == 0){
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog("No game... Waiting!");
            StartProcess(processname);
        } else {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog("Found the game!");
            return;
        }
    }

    private string WaitProcess(string processname, string result){
        Process[] runninggame = Process.GetProcessesByName(processname);
        if (runninggame.Length != 0){
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog("Game is running!");
            return WaitProcess(processname, result);
        } else {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog("Looks like the game closed!");
            return result;
        }
    }

    private void UpdateTheme()
    {
        SCCMTheme theme = GlobalVariables.LoadedTheme;
        BGColor.Color = theme.BackgroundColor;
        BorderColor.Color = theme.AccentColor;
        IconColor.Color = theme.AccentColor;
        ExeNameLabel.AddThemeColorOverride("font_color", theme.MainTextColor);
        ExePathLabel.AddThemeColorOverride("font_color", theme.MainTextColor);
    }
}
