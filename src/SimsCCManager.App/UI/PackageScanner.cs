using Godot;
using SimsCCManager.PackageReaders;
using SimsCCManager.Packages.Containers;
using System;
using System.Threading;

public partial class PackageScanner : Control
{
	public delegate void PackageScannedEvent(string id);
	public PackageScannedEvent PackageScanned;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	public void Scan(SimsPackage package){
		new Thread(() => {
			if (package.Game == Games.Sims2){
				Sims2PackageReader.ReadSims2Package(package);
			} else if (package.Game == Games.Sims3){

			} else if (package.Game == Games.Sims4){

			}
			CallDeferred(nameof(EmitScanned), package.Identifier.ToString());
		}){IsBackground = true}.Start();
	}

	private void EmitScanned(string identifier){
		PackageScanned.Invoke(identifier);
		//EmitSignal("PackageScanned", identifier);
	}
}
