using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.OptionLists;
using SimsCCManager.PackageReaders;

namespace SimsCCManager.InfoFiles
{
    public class InfoFileUtilities
    {
        public static async Task<SimsPackage> ReadBasicData(FileInfo fi)
        {
            SimsPackage simsPackage = new();
            XDocument xdoc = LoadData(fi.FullName).Result;

            simsPackage = GetData(xdoc, simsPackage).Result;

            /*simsPackage.Identifier = Guid.Parse(xdoc.Descendants().First(x => x.Name == "Identifier").Value);
            simsPackage.Location = xdoc.Descendants().First(x => x.Name == "Location").Value;
            simsPackage.HasBeenRead = bool.Parse(xdoc.Descendants().First(x => x.Name == "HasBeenRead").Value);
            simsPackage.StandAlone = bool.Parse(xdoc.Descendants().First(x => x.Name == "StandAlone").Value);
            */
            return simsPackage;
        }
        public static async Task<SimsPackage> ReadBasicData(DirectoryInfo fi)
        {
            SimsPackage simsPackage = new();
            XDocument xdoc = LoadData(fi.FullName).Result;

            simsPackage = GetData(xdoc, simsPackage).Result;

            /*simsPackage.Identifier = Guid.Parse(xdoc.Descendants().First(x => x.Name == "Identifier").Value);
            simsPackage.Location = xdoc.Descendants().First(x => x.Name == "Location").Value;
            simsPackage.HasBeenRead = bool.Parse(xdoc.Descendants().First(x => x.Name == "HasBeenRead").Value);
            simsPackage.StandAlone = bool.Parse(xdoc.Descendants().First(x => x.Name == "StandAlone").Value);
            */
            return simsPackage;
        }

        public static async Task<SimsPackage> GetData(XDocument xdoc, SimsPackage simsPackage)
        {
            simsPackage.Identifier = Guid.Parse(xdoc.Descendants().First(x => x.Name == "Identifier").Value);
            simsPackage.Location = xdoc.Descendants().First(x => x.Name == "Location").Value;
            simsPackage.HasBeenRead = bool.Parse(xdoc.Descendants().First(x => x.Name == "HasBeenRead").Value);
            simsPackage.IsDirectory = bool.Parse(xdoc.Descendants().First(x => x.Name == "IsDirectory").Value);
            simsPackage.StandAlone = bool.Parse(xdoc.Descendants().First(x => x.Name == "StandAlone").Value);
            simsPackage.RootMod = bool.Parse(xdoc.Descendants().First(x => x.Name == "RootMod").Value);
            simsPackage.FileName = xdoc.Descendants().First(x => x.Name == "FileName").Value;
            simsPackage.Type = xdoc.Descendants().First(x => x.Name == "Type").Value;
            simsPackage.DateCreated = DateTime.Parse(xdoc.Descendants().First(x => x.Name == "DateCreated").Value);
            Category pc = new();
            Guid categoryIdentifier = Guid.Parse(xdoc.Descendants().First(x => x.Name == "PackageCategory").Descendants().First(p => p.Name == "Identifier").Value);
            pc = LoadedData.LoadedInstance.Categories.First(x => x.Identifier == categoryIdentifier);
            

            simsPackage.PackageCategory = pc;
            simsPackage.Game = InterpretGameEnum(xdoc.Descendants().First(x => x.Name == "Game").Value);

            return simsPackage;
        }

        public static SimsGames InterpretGameEnum(string enumString)
        {
            switch (enumString)
            {
                case "Sims1":
                return SimsGames.Sims1;
                case "Sims2":
                return SimsGames.Sims2;
                case "Sims3":
                return SimsGames.Sims3;
                case "Sims4":
                return SimsGames.Sims4;
                case "SimsMedieval":
                return SimsGames.SimsMedieval;
                case "Spore":
                return SimsGames.Spore;
                case "SimCity4":
                return SimsGames.SimCity4;
                case "SimCity5":
                return SimsGames.SimCity5;
                default:
                return SimsGames.Null;
            }
        }

        public static async Task UpdateCategory(SimsPackage package, Category cat)
        {
            XDocument xdoc = LoadData(package.Location).Result;
            XElement category = xdoc.Descendants().First(x => x.Name == "PackageCategory");
            category.Descendants().First(x => x.Name == "Name").Value = cat.Name;
            category.Descendants().First(x => x.Name == "Identifier").Value = cat.Identifier.ToString();
            category.Descendants().First(x => x.Name == "Description").Value = cat.Description;
            //category.Descendants().First(x => x.Name == "Background").Value = cat.Background;
            //category.Descendants().First(x => x.Name == "Name").Value = cat.Name;
            

        }
        public static async Task UpdateElement(SimsPackage package, string element, string value)
        {
            XDocument xdoc = LoadData(package.Location).Result;
            xdoc.Descendants().First(x => x.Name == element).Value = value;
            xdoc.Save(package.InfoFile);
        }
    
        public async static Task<XDocument> LoadData(string fileLocation)
        {
            XDocument xdoc = new();
            if (File.Exists(fileLocation))
            {             
                /*using (StreamReader reader = new(fileLocation))
                {
                    xdoc = XDocument.Parse(reader.ReadToEnd());
                }  
                //
                /*using (StreamReader reader = new(fileLocation, Encoding.UTF8))
                {
                    xdoc = XDocument.LoadAsync(reader, LoadOptions.None, CancellationToken.None).Result;
                }  */  
                return XDocument.Load(fileLocation);              
            }   
            return xdoc;
        }

        public static SimsPackage ReadPackageDetails(SimsPackage package, GameInstance instance, bool SubFolder = false)
        {
            if (package.IsDirectory)
            {
                return ReadPackageDetails(package, instance, new DirectoryInfo(package.Location), SubFolder);
            }
            else
            {
                return ReadPackageDetails(package, instance, new FileInfo(package.Location), SubFolder);
            }
        }

        public static SimsPackage ReadPackageDetails(SimsPackage package, GameInstance instance, FileInfo fi, bool SubFolder)
        {
            SimsPackageReader simsPackageReader = new();
            try
            {
                simsPackageReader.ReadPackage(package.Location);
                package.PackageData = simsPackageReader.SimsData;
                if (simsPackageReader.PackageGame != instance.GameChoice)
                {
                    if (simsPackageReader.PackageGame == SimsGames.Sims3 && instance.GameChoice == SimsGames.SimsMedieval)
                    {
                        package.Game = SimsGames.SimsMedieval;
                        package.WrongGame = false;
                    }
                    else
                    {
                        package.Game = simsPackageReader.PackageGame;
                        package.WrongGame = true;
                    }
                }
                if (!SubFolder) package.StandAlone = true;
                simsPackageReader.Dispose();
                package.DateCreated = File.GetCreationTime(package.Location);
                package.DateUpdated = DateTime.Now;
                simsPackageReader.CheckOverrides(package);
                package.HasBeenRead = true;
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Returning {0} as read.", package.FileName));
            }
            catch (Exception e)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Couldn't read package {0}: {1} ({2})", package.FileName, e.Message, e.StackTrace));
            } 
            package.WriteXML();           
            return package;
        }

        public static SimsPackage ReadPackageDetails(SimsPackage package, GameInstance instance, DirectoryInfo Dir, bool SubFolder = false)
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Checking {0} for folders matching neigborhoods or lighting.", package.FileName));
            List<string> folders = [..Directory.EnumerateDirectories(package.Location, "*.*", SearchOption.AllDirectories)];
            List<string> files = [..Directory.EnumerateFiles(package.Location, "*.*", SearchOption.AllDirectories)];

            string foldersList = "";
            string filesList = "";

            for (int i = 0; i < folders.Count; i++)
            {
                DirectoryInfo d = new(folders[i]);
                folders[i] = d.Name;
                foldersList += string.Format("{0}, ", folders[i]);
            }
            for (int i = 0; i < files.Count; i++)
            {
                FileInfo f = new(files[i]);
                files[i] = f.Name;
                filesList += string.Format("{0}, ", files[i]);
            }

            if (folders.Any(x => x.Contains("Lots", StringComparison.InvariantCultureIgnoreCase) || files.Any(x => x.Contains("Neighborhood", StringComparison.InvariantCultureIgnoreCase))))
            {                
                package.Type = "Neighborhood Template";                
            } else if (files.Any(x => x.Contains("RLS-dD-BON", StringComparison.InvariantCultureIgnoreCase) || files.Any(x => x.Contains("RLS-Shaders", StringComparison.InvariantCultureIgnoreCase) || files.Any(x => x.Contains("Lighting", StringComparison.InvariantCultureIgnoreCase)))))
            {
                package.Type = "Lighting Mod";
            } else if (folders.Any(x => x.Contains("STORE_LIGHTS", StringComparison.InvariantCultureIgnoreCase)))
            {
                package.Type = "Lighting Mod";
            } else
            {
                filesList = filesList.Trim();
                filesList = filesList.Trim(',');
                foldersList = foldersList.Trim();
                foldersList = foldersList.Trim(',');
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Folders list - {0} - and files list {1} - came up with no matches for lighting or neighborhoods in {2}", foldersList, filesList, package.FileName));
            }
            
            
            package.IsDirectory = true;
            if (!SubFolder)
            {
                package.StandAlone = true;
            }
            else
            {
                package.StandAlone = false;
            }
            package.FileName = Dir.Name;
            package.Game = instance.GameChoice;
            switch (package.Game)
            {
                case SimsGames.Sims2:
                    package.Sims2Data = new();
                    break;
                case SimsGames.Sims3:
                    package.Sims3Data = new();
                    break;
                case SimsGames.Sims4:
                    package.Sims4Data = new();
                    break;
            }
            package.DateCreated = Directory.GetCreationTime(package.Location);
            package.DateUpdated = DateTime.Now;
            package.HasBeenRead = true;
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Returning {0} as read.", package.FileName));
            package.WriteXML();
            return package;
        }

        public static SimsPackage ReadFullInfoFile(SimsPackage simsPackage, GameInstance instance)
        {
            XmlSerializer simsPackageSerializer = new XmlSerializer(typeof(SimsPackage));
            if (File.Exists(simsPackage.InfoFile))
            {                    
                try {                    
                    using (FileStream fileStream = new(simsPackage.InfoFile, FileMode.Open, System.IO.FileAccess.Read)){                       
                        
                        simsPackage = (SimsPackage)simsPackageSerializer.Deserialize(fileStream);
                                                  
                        fileStream.Close();                  
                    }
                } catch (Exception e)
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Hit an error reading {0} - deleting. Error: {1} - Trace: {2} - Inner: {3}", simsPackage.InfoFile, e.Message, e.StackTrace, e.InnerException));
                    Utilities.MoveToRecycleBin(simsPackage.InfoFile);
                    simsPackage = ReadPackageDetails(simsPackage, instance, new FileInfo(simsPackage.Location), false);
                }
            }
            return simsPackage;
        }   


        public static bool ReReadPackageDetails(SimsPackage package, bool SubFolder = false)
        {
            if (!package.IsDirectory)
            {
                ReReadPackageDetails(package, new FileInfo(package.Location), SubFolder);
                return true;
            } else return false;
        }

        public static void ReReadPackageDetails(SimsPackage package, FileInfo fi, bool SubFolder)
        {
            SimsPackageReader simsPackageReader = new();
            try
            {
                simsPackageReader.ReadPackage(package.Location);
                package.PackageData = simsPackageReader.SimsData;                
            }
            catch (Exception e)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Couldn't read package {0}: {1} ({2})", package.FileName, e.Message, e.StackTrace));
            }            
        } 
    }


}