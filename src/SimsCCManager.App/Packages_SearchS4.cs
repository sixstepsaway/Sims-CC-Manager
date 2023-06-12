/* 
    Credits:
        - WandaSoule who helped me crack getting into the S4 entries of it all. 
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using System.Xml;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using SSAGlobals;
using SimsCCManager.Packages.Containers;
using SimsCCManager.Packages.Decryption;
using System.Data.SQLite;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System.Collections.Concurrent;
using SimsCCManager.Packages.Sorting;
using System.Windows.Input;
using SimsCCManager.Packages.Orphans;

namespace SimsCCManager.Packages.Sims4Search
{
    public static class extensions {
        /// <summary>
        /// Adds an incremental extension for Dictionaries, so that each time an item is added that already exists, rather than simply rejecting that item, it increments its associated int by one.
        /// </summary>
        public static void Increment<T>(this Dictionary<T, int> dictionary, T key)
        {
            int count;
            dictionary.TryGetValue(key, out count);
            dictionary[key] = count + 1;
        }
    }

    /*
            None = 0x0000,
            Zlib = 0x5A42,
            RefPack = 0xFFFF
    */

    public class EntryHolder {
        public int[] entry {get; set;}
        public IReadOnlyList<int> header {get; set;}        
    }

    public class Readers {
        static LoggingGlobals log = new LoggingGlobals();
        public static List<TagsList> GetTagInfo(CASTag16Bit tags, uint count, StringBuilder LogFile){
            string LogMessage = "";
            
            List<TagsList> taglist = new List<TagsList>();
            TagsList tagg = new TagsList();
            List<TagsList> tl = new List<TagsList>();
            List<TagsList> tag = new List<TagsList>();
            for (int i = 0; i < count; i++)
            {   
                if (tags.tagKey[i] != 0){
                    tag = GlobalVariables.S4FunctionTypesConnection.Query<TagsList>(string.Format("SELECT * FROM S4CategoryTags where TypeID='{0}'", tags.tagKey[i]));
                    if (tag.Any()){                    
                        tl.AddRange(tag);
                        tagg.TypeID = tl[0].TypeID;
                        tagg.Description = tl[0].Description;
                        taglist.Add(tagg);
                        LogMessage = string.Format("Tag {0} was matched to {1}.", i, tagg.TypeID, tagg.Description);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                    } else {
                        LogMessage = string.Format("Tag {0} matched nothing.", tags.tagKey[i]);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                    }  
                }
                if (tags.catKey[i] != 0){
                    tag = GlobalVariables.S4FunctionTypesConnection.Query<TagsList>(string.Format("SELECT * FROM S4CategoryTags where TypeID='{0}'", tags.catKey[i]));
                    if (tag.Any()){                    
                        tl.AddRange(tag);
                        tagg.TypeID = tl[0].TypeID;
                        tagg.Description = tl[0].Description;
                        taglist.Add(tagg);
                        LogMessage = string.Format("Tag {0} was matched to {1}.", i, tagg.TypeID, tagg.Description);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                    } else {
                        LogMessage = string.Format("Tag {0} matched nothing.", tags.catKey[i]);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                    }  
                }                                            
            }
            tagg = new TagsList();
            tl = new List<TagsList>();
            tag = new List<TagsList>();
            return taglist;
        }
        
        public static List<TagsList> GetTagInfo(Tag tags, uint count, StringBuilder LogFile){
            string LogMessage = "";
            List<TagsList> taglist = new List<TagsList>();
            TagsList tagg = new TagsList();
            List<TagsList> tl = new List<TagsList>();
            List<TagsList> tag = new List<TagsList>();
            for (int i = 0; i < count; i++)
            {
                tag = GlobalVariables.S4FunctionTypesConnection.Query<TagsList>(string.Format("SELECT * FROM S4CategoryTags where TypeID='{0}'", tags.tagKey[i]));
                if (tag.Any()){
                    
                    tl.AddRange(tag);
                    tagg.TypeID = tl[0].TypeID;
                    tagg.Description = tl[0].Info;
                    taglist.Add(tagg);
                    LogMessage = string.Format("Tag {0} ({1}) was matched to {2}.", i, tags.tagKey[i], tagg.Info);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else {
                    LogMessage = string.Format("Tag {0} matched nothing.", tags.tagKey[i]);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                }
            }
            tagg = new TagsList();
            tl = new List<TagsList>();
            tag = new List<TagsList>();
            return taglist;
        }        
        
        public static List<TagsList> GetTagInfo(List<uint> tags, uint count, StringBuilder LogFile){
            string LogMessage = "";
            List<TagsList> taglist = new List<TagsList>();
            TagsList tagg = new TagsList();
            List<TagsList> tl = new List<TagsList>();
            List<TagsList> tag = new List<TagsList>();            
            for (int i = 0; i < count; i++)
            {
                tag = GlobalVariables.S4FunctionTypesConnection.Query<TagsList>(string.Format("SELECT * FROM S4CategoryTags where TypeID='{0}'", tags[i]));
                if (tag.Any()){
                    
                    tl.AddRange(tag);
                    tagg.TypeID = tl[0].TypeID;
                    tagg.Description = tl[0].Info;
                    taglist.Add(tagg);
                    LogMessage = string.Format("Tag {0} ({2}) was matched to {1}.", i, taglist[i].Info, tags[i]);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else {
                    LogMessage = string.Format("Tag {0} matched nothing.", taglist[i]);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                }
            }
            tagg = new TagsList();
            tl = new List<TagsList>();
            tag = new List<TagsList>();
            return taglist;
        }
    }


    public struct ResourceKeyITG {
        /// <summary>
        /// Reads resource keys and does not flip the instance ID.
        /// </summary>
        LoggingGlobals log = new LoggingGlobals();
        public ulong instance;
        public uint type;
        public uint group;
        
        public ResourceKeyITG(BinaryReader reader, StringBuilder LogFile){
            string LogMessage = "";
            this.instance = reader.ReadUInt64(); 
            LogMessage = string.Format("GUID Instance: {0}", this.instance);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            this.type = reader.ReadUInt32(); 
            LogMessage = string.Format("GUID Type: {0}", this.type);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            this.group = reader.ReadUInt32();     
            LogMessage = string.Format("GUID Group: {0}", this.group);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));  
        }

        public override string ToString() => $"{type.ToString("X8")}-{group.ToString("X8")}-{instance.ToString("X16")}";
        
    }
    public struct ResourceKeyITGFlip {
        /// <summary>
        /// Reads resource keys and flips the instance ID.
        /// </summary>
        LoggingGlobals log = new LoggingGlobals();
        public ulong instance;
        public uint type;
        public uint group;
        
        public ResourceKeyITGFlip(BinaryReader reader, StringBuilder LogFile){
            string LogMessage = "";
            uint left = reader.ReadUInt32();
            uint right = reader.ReadUInt32();
            ulong longleft = left;
            longleft = (longleft << 32);
            this.instance = longleft | right;
            LogMessage = string.Format("GUID Instance: {0}", this.instance);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            this.type = reader.ReadUInt32(); 
            LogMessage = string.Format("GUID Type: {0}", this.type);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            this.group = reader.ReadUInt32();     
            LogMessage = string.Format("GUID Group: {0}", this.group);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));    
        }

        public override string ToString() => $"{type.ToString("X8")}-{group.ToString("X8")}-{instance.ToString("X16")}";
    }

    public struct Tag {
        /// <summary>
        /// Reads tags.
        /// </summary>
        public int[] tagKey;  
        public int[] empty; 

        public Tag(BinaryReader reader, uint count){
            if (count == 1){
                this.tagKey = new int[count];
                this.empty = new int[count];
                this.tagKey[0] = reader.ReadUInt16();
                this.empty[0] = 0;
            } else {
                this.tagKey = new int[count];
                this.empty = new int[count];
                for (int i = 0; i < count; i++){
                    this.tagKey[i] = reader.ReadUInt16(); 
                    this.empty[i] = reader.ReadUInt16(); 
                }
            }
        }
    }
    public struct CASTag16Bit {
        /// <summary>
        /// Gets CAS Tags from package. 16bit version.
        /// </summary>
        public int[] tagKey;  
        public int[] catKey;
        public int[] empty; 

        public CASTag16Bit(BinaryReader reader, uint count){
            if (count == 1){
                this.tagKey = new int[count];
                this.catKey = new int[count];
                this.empty = new int[count];
                this.empty[0] = reader.ReadUInt16();
                this.catKey[0] = reader.ReadUInt16();
                this.tagKey[0] = reader.ReadUInt16();
                
            } else {
                this.tagKey = new int[count];
                this.catKey = new int[count];
                this.empty = new int[count];
                for (int i = 0; i < count; i++){
                    this.empty[i] = reader.ReadUInt16();
                    this.catKey[i] = reader.ReadUInt16(); 
                    this.tagKey[i] = reader.ReadUInt16(); 
                     
                }
            }
        }
    }

    public struct CASTag32Bit {
        /// <summary>
        /// Gets CAS Tags from package. 32bit version.
        /// </summary>
        public uint[] tagKey;  
        public uint[] catKey;
        public uint[] empty; 

        public CASTag32Bit(BinaryReader reader, int count){
            if (count == 1){
                this.tagKey = new uint[count];
                this.catKey = new uint[count];
                this.empty = new uint[count];
                this.empty[0] = reader.ReadUInt32();
                this.catKey[0] = reader.ReadUInt32();
                this.tagKey[0] = reader.ReadUInt32();
                
            } else {
                this.tagKey = new uint[count];
                this.catKey = new uint[count];
                this.empty = new uint[count];
                for (int i = 0; i < count; i++){
                    this.empty[i] = reader.ReadUInt32();
                    this.catKey[i] = reader.ReadUInt32(); 
                    this.tagKey[i] = reader.ReadUInt32(); 
                     
                }
            }
        }
    }

    public struct ReadCOBJ{
        /// <summary>
        /// Reads COBJD entries.
        /// </summary>
        public string GUID = "null";
        public List<TagsList> itemtags = new List<TagsList>();
        LoggingGlobals log = new LoggingGlobals();
        GlobalVariables globals = new GlobalVariables();
        public ulong instanceid;
        public uint typeid;
        public uint groupid;

        public ReadCOBJ(BinaryReader readFile, int packageparsecount, int e, List<TagsList> itemtags, StringBuilder LogFile){
            string LogMessage = "";
            uint version = readFile.ReadUInt32();
            LogMessage = string.Format("P{0}/E{1} - Version: {2} \n-- As hex: {3}", packageparsecount, e, version, version.ToString("X8"));
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            uint commonblockversion = readFile.ReadUInt32();
            LogMessage = string.Format("P{0}/E{1} - Common Block Version: {2} \n-- As hex: {3}", packageparsecount, e, commonblockversion, commonblockversion.ToString("X8"));
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            uint namehash = readFile.ReadUInt32();
            LogMessage = string.Format("P{0}/E{1} - Name Hash: {2} \n-- As hex: {3}", packageparsecount, e, namehash, namehash.ToString("X8"));
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            uint descriptionhash = readFile.ReadUInt32();
            LogMessage = string.Format("P{0}/E{1} - Description Hash: {2} \n-- As hex: {3}", packageparsecount, e, descriptionhash, descriptionhash.ToString("X8"));
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            uint price = readFile.ReadUInt32();
            LogMessage = string.Format("P{0}/E{1} - Price: {2} \n-- As hex: {3}", packageparsecount, e, price, price.ToString("X8"));
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

            ulong thumbhash = readFile.ReadUInt64();
            LogMessage = string.Format("P{0}/E{1} - Thumb Hash: {2} \n-- As hex: {3}", packageparsecount, e, thumbhash, thumbhash.ToString("X8"));
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

            uint devcatflags = readFile.ReadUInt32();
            LogMessage = string.Format("P{0}/E{1} - Dev Cat Flags: {2} \n-- As hex: {3}", packageparsecount, e, devcatflags, devcatflags.ToString("X8"));
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            
            int tgicount = readFile.ReadByte();
            LogMessage = string.Format("P{0}/E{1}, - TGI Count: {2}", packageparsecount, e, tgicount);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            instanceid = 0;
            typeid = 0;
            groupid = 0;

            if (tgicount != 0){
                LogMessage = string.Format("P{0}/E{1}, - TGI Count is not zero. Reading Resources.", packageparsecount, e);
                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                ResourceKeyITG resourcekey = new ResourceKeyITG(readFile, LogFile);
                LogMessage = string.Format("P{0}/E{1}, - GUID: {2}", packageparsecount, e, resourcekey.ToString());
                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                instanceid = resourcekey.instance;
                typeid = resourcekey.type;
                groupid = resourcekey.group;
            }
            
            if (commonblockversion >= 10)
            {
                int packId = readFile.ReadInt16();
                LogMessage = string.Format("P{0}/E{1}, - Pack ID: {2}", packageparsecount, e, packId);
                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                int packFlags = readFile.ReadByte();
                LogMessage = string.Format("P{0}/E{1}, - PackFlags: {2}", packageparsecount, e, packFlags);
                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                readFile.ReadBytes(9);
            } else {
                int unused2 = readFile.ReadByte();
                if (unused2 > 0)
                {
                    readFile.ReadByte();
                }
            }

            if (commonblockversion >= 11){
                uint count = readFile.ReadUInt32();
                LogMessage = string.Format("P{0}/E{1}, - Tags Count: {2}", packageparsecount, e, count);
                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                Tag tags = new Tag(readFile, count);

                List<TagsList> gottags = Readers.GetTagInfo(tags, count, LogFile);
                foreach (TagsList tag in gottags){
                    if (!itemtags.Exists(x => x.TypeID == tag.TypeID) && !itemtags.Exists(x => x.Description == tag.Description)){
                        itemtags.Add(tag);  
                    }
                }
                
            } else {
                uint count = readFile.ReadUInt32();
                List<uint> tagsread = new List<uint>(); 
                LogMessage = string.Format("P{0}/E{1}, - Num Tags: {2}", packageparsecount, e, count);
                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                for (int t = 0; t < count; t++){                    
                    uint tagvalue = readFile.ReadUInt16();
                    tagsread.Add(tagvalue);
                }
                List<TagsList> gottags = Readers.GetTagInfo(tagsread, count, LogFile);
                foreach (TagsList tag in gottags){
                    if (!itemtags.Exists(x => x.TypeID == tag.TypeID) && !itemtags.Exists(x => x.Description == tag.Description)){
                        itemtags.Add(tag);  
                    }
                }
            }
            long location = readFile.BaseStream.Position;
            uint count2 = readFile.ReadUInt32();
            LogMessage = string.Format("P{0}/E{1}, - Selling Point Count: {2}", packageparsecount, e, count2);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            if (count2 > 5){
                log.MakeLog("Selling point count is too high, something went wrong.", true);
            } else {
                Tag sellingtags = new Tag(readFile, count2);

                List<TagsList> gottags = Readers.GetTagInfo(sellingtags, count2, LogFile);
                foreach (TagsList tag in gottags){
                    if (!itemtags.Exists(x => x.TypeID == tag.TypeID) && !itemtags.Exists(x => x.Description == tag.Description)){
                        itemtags.Add(tag);  
                    }
                }
                
            }            

            uint unlockByHash = readFile.ReadUInt32();
            LogMessage = string.Format("P{0}/E{1}, - UnlockBy Hash: {2}", packageparsecount, e, unlockByHash);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            
            uint unlockedByHash = readFile.ReadUInt32();
            LogMessage = string.Format("P{0}/E{1}, - UnlockedBy Hash: {2}", packageparsecount, e, unlockedByHash);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

            int swatchColorSortPriority = readFile.ReadUInt16();
            LogMessage = string.Format("P{0}/E{1}, - Swatch Sort Priority: {2}", packageparsecount, e, swatchColorSortPriority);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

            ulong varientThumbImageHash = readFile.ReadUInt64();
            LogMessage = string.Format("P{0}/E{1}, - Varient Thumb Image Hash: {2}", packageparsecount, e, varientThumbImageHash);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
        }            
    }
    
    public struct ReadOBJDIndex {
        /// <summary>
        /// Retrieves OBJD index.
        /// </summary>
        LoggingGlobals log = new LoggingGlobals();
        public int version;
        public long refposition;
        public int count;
        public uint[] entrytype;
        public uint[] position;

        public ReadOBJDIndex(BinaryReader reader, int packageparsecount, int e, StringBuilder LogFile){
            long entrystart = reader.BaseStream.Position;
            string LogMessage = "";
            this.version = reader.ReadUInt16();
            log.MakeLog("Version: " + this.version, true);
            LogMessage = string.Format("P{0}/OBJD{1}, - Version: {2}", packageparsecount, e, this.version);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

            if (this.version > 150){
                log.MakeLog("Version is not legitimate.", true);
                this.refposition = 0; 
                this.count = (int)0;
                this.entrytype = new uint[0];
                this.position = new uint[0];
            } else {
                this.refposition = reader.ReadUInt32();
                this.refposition = refposition + entrystart;
                reader.BaseStream.Position = refposition;
                this.count = reader.ReadUInt16();
                this.entrytype = new uint[count];
                this.position = new uint[count];
                for (int i = 0; i < count; i++){
                    this.entrytype[i] = reader.ReadUInt32();
                    this.position[i] = reader.ReadUInt32();
                }
            }   
        }
    }

    public struct ReadOBJDEntry {
        /// <summary>
        /// Reads OBJD entries.
        /// </summary>
        LoggingGlobals log = new LoggingGlobals();
        public int namelength;
        public byte[] namebit;
        public string name;
        
        public int tuningnamelength;
        public byte[] tuningbit;
        public string tuningname;        
        public ulong tuningid;
        public uint componentcount;
        public uint[] components;        
        public int materialvariantlength;
        public byte[] materialvariantbyte;
        public string materialvariant;
        public uint price;
        public string[] icon;
        public string[] rig;
        public string[] slot;
        public string[] model;
        public string[] footprint;
        private bool tuningidmissing = false;
        public uint type;
        public uint group;
        public ulong instance; 
        public ReadOBJDEntry(BinaryReader reader, string[] entries, int[] positions, int packageparsecount, int e, StringBuilder LogFile){
            string LogMessage = "";
            uint preceeding;
            uint preceedingDiv;

            namelength = 0;
            namebit = new byte[0];
            name = "";
            tuningnamelength = 0;
            tuningbit = new byte[0];
            tuningname = "";
            tuningid = 0;
            componentcount = 0;
            components = new uint[0];
            materialvariantlength = 0;
            materialvariantbyte = new byte[0];
            materialvariant = "";
            price = 0;
            icon = new string[0];
            rig = new string[0];
            slot = new string[0];
            model = new string[0];
            footprint = new string[0];
            tuningidmissing = false;  
            instance = 0;
            group = 0;
            type = 0;          

            for (int j = 0; j < entries.Length; j++){
                string entryid = entries[j];
                int entrypos = positions[j];
                switch (entryid)
                {
                    case "E7F07786": // name
                        reader.BaseStream.Position = entrypos;
                        this.namelength = reader.ReadByte();
                        LogMessage = string.Format("P{0}/OBJD{1}, - Name Length: {2}", packageparsecount, e, namelength);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        reader.BaseStream.Position = reader.BaseStream.Position + 3;
                        //LogMessage = "Byte 1: " + reader.ReadByte().ToString();
                        //if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        //if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        //LogMessage = "Byte 2: " + reader.ReadByte().ToString();
                        //if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        //if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        //LogMessage = "Byte 3: " + reader.ReadByte().ToString();
                        //if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        //if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        this.namebit = reader.ReadBytes(namelength);
                        this.name = Encoding.UTF8.GetString(namebit);
                        LogMessage = string.Format("P{0}/OBJD{1}, - Name: {2}", packageparsecount, e, name);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        break;
                    case "790FA4BC": //tuning
                        reader.BaseStream.Position = entrypos;
                        this.tuningnamelength = reader.ReadByte();
                        LogMessage = string.Format("P{0}/OBJD{1}, - Tuning Name Length: {2}", packageparsecount, e, tuningnamelength);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        reader.BaseStream.Position = reader.BaseStream.Position + 3;
                        //log.MakeLog("Reading three empty bytes.", true);
                        //LogMessage = "Byte 1: " + reader.ReadByte().ToString();
                        //if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        //if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        //LogMessage = "Byte 2: " + reader.ReadByte().ToString();
                        //if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        //if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        //LogMessage = "Byte 3: " + reader.ReadByte().ToString();
                        //if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        //if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        this.tuningbit = reader.ReadBytes(tuningnamelength);
                        LogMessage = string.Format("P{0}/OBJD{1}, - Tuning Name Length: {2}", packageparsecount, e, tuningbit);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        this.tuningname = Encoding.UTF8.GetString(tuningbit);
                        LogMessage = string.Format("P{0}/OBJD{1}, - Tuning Name: {2}", packageparsecount, e, tuningname);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        break;
                    case "B994039B": //TuningID
                        reader.BaseStream.Position = entrypos;
                        this.tuningid = reader.ReadUInt64(); 
                        break;
                    case "CADED888": //Icon
                        reader.BaseStream.Position = entrypos;
                        preceeding = reader.ReadUInt32();
                        LogMessage = string.Format("P{0}/OBJD{1}, - Reading Preceeding UInt32: {2}", packageparsecount, e, preceeding);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        preceedingDiv = preceeding / 4;            
                        LogMessage = string.Format("P{0}/OBJD{1}, - Number of Icon GUIDs: {2}", packageparsecount, e, preceedingDiv);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        this.icon = new string[preceedingDiv];
                        for (int p = 0; p < preceedingDiv; p++){
                            ResourceKeyITGFlip ricon = new ResourceKeyITGFlip(reader, LogFile);
                            LogMessage = string.Format("P{0}/OBJD{1}, - Icon GUID: {2}", packageparsecount, e, ricon.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            this.icon[p] = ricon.ToString();
                            this.type = ricon.type;
                            this.group = ricon.group;
                            this.instance = ricon.instance;
                        }
                        break;
                    case "E206AE4F": //Rig
                        reader.BaseStream.Position = entrypos;
                        preceeding = reader.ReadUInt32();
                        LogMessage = string.Format("P{0}/OBJD{1}, - Reading preceeding UInt32: {2}", packageparsecount, e, preceeding);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));                        
                        preceedingDiv = preceeding / 4;
                        LogMessage = string.Format("P{0}/OBJD{1}, - Number of Rig GUIDs: {2}", packageparsecount, e, preceedingDiv);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        this.rig = new string[preceedingDiv];
                        for (int p = 0; p < preceedingDiv; p++){
                            ResourceKeyITGFlip rkrig = new ResourceKeyITGFlip(reader, LogFile);
                            LogMessage = string.Format("P{0}/OBJD{1}, - Rig GUID: {2}", packageparsecount, e, rkrig.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            this.rig[p] = rkrig.ToString();
                            this.type = rkrig.type;
                            this.group = rkrig.group;
                            this.instance = rkrig.instance;
                        }   
                        break;
                    case "8A85AFF3": //Slot
                        reader.BaseStream.Position = entrypos;
                        preceeding = reader.ReadUInt32();
                        LogMessage = string.Format("P{0}/OBJD{1}, - Reading preceeding UInt32: {2}", packageparsecount, e, preceeding);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));                        
                        preceedingDiv = preceeding / 4;
                        LogMessage = string.Format("P{0}/OBJD{1}, - Number of Slot GUIDs: {2}", packageparsecount, e, preceedingDiv);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        this.slot = new string[preceedingDiv];
                        for (int p = 0; p < preceedingDiv; p++){
                            ResourceKeyITGFlip rkslot = new ResourceKeyITGFlip(reader, LogFile);
                            LogMessage = string.Format("P{0}/OBJD{1}, - Slot GUID: {2}", packageparsecount, e, rkslot.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            this.slot[p] = rkslot.ToString();                            
                            this.type = rkslot.type;
                            this.group = rkslot.group;
                            this.instance = rkslot.instance;
                        }            
                        break;
                    case "8D20ACC6": //Model
                        reader.BaseStream.Position = entrypos;
                        preceeding = reader.ReadUInt32();
                        LogMessage = string.Format("P{0}/OBJD{1}, - Reading preceeding UInt32: {2}", packageparsecount, e, preceeding);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));                        
                        preceedingDiv = preceeding / 4;
                        LogMessage = string.Format("P{0}/OBJD{1}, - Number of Model GUIDs: {2}", packageparsecount, e, preceedingDiv);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        this.model = new string[preceedingDiv];
                        for (int p = 0; p < preceedingDiv; p++){
                            ResourceKeyITGFlip rkmodel = new ResourceKeyITGFlip(reader, LogFile);                            
                            LogMessage = string.Format("P{0}/OBJD{1}, - Model GUID: {2}", packageparsecount, e, rkmodel.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            this.model[p] = rkmodel.ToString();
                            this.type = rkmodel.type;
                            this.group = rkmodel.group;
                            this.instance = rkmodel.instance;
                        }
                        break;
                    case "6C737AD8": //Footprint
                        reader.BaseStream.Position = entrypos;
                        preceeding = reader.ReadUInt32();
                        LogMessage = string.Format("P{0}/OBJD{1}, - Reading preceeding UInt32: {2}", packageparsecount, e, preceeding);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));                        
                        preceedingDiv = preceeding / 4;
                        LogMessage = string.Format("P{0}/OBJD{1}, - Number of Footprint GUIDs: {2}", packageparsecount, e, preceedingDiv);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        this.footprint = new string[preceedingDiv];
                        for (int p = 0; p < preceedingDiv; p++){
                            ResourceKeyITGFlip rkft = new ResourceKeyITGFlip(reader, LogFile);
                            LogMessage = string.Format("P{0}/OBJD{1}, - Footprint GUID: {2}", packageparsecount, e, rkft.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            this.footprint[p] = rkft.ToString();
                            this.type = rkft.type;
                            this.group = rkft.group;
                            this.instance = rkft.instance;                
                        }
                        break;
                    case "E6E421FB": //Components
                        reader.BaseStream.Position = entrypos;
                        this.componentcount = reader.ReadUInt32();                        
                        LogMessage = string.Format("P{0}/OBJD{1}, - Component Count: {2}", packageparsecount, e, componentcount);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        this.components = new uint[this.componentcount];
                        for (int i = 0; i < this.componentcount; i++){
                            components[i] = reader.ReadUInt32();
                        }
                        break;
                    case "ECD5A95F": //MaterialVariant
                        reader.BaseStream.Position = entrypos;
                        this.materialvariantlength = reader.ReadByte();                        
                        LogMessage = string.Format("P{0}/OBJD{1}, - Material Variant Length: {2}", packageparsecount, e, materialvariantlength);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        reader.BaseStream.Position = reader.BaseStream.Position + 3;
                        //log.MakeLog("Reading three empty bytes.", true);
                        //LogMessage = "Byte 1: " + reader.ReadByte().ToString();
                        //if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        //if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        //LogMessage = "Byte 2: " + reader.ReadByte().ToString();
                        //if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        //if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        //LogMessage = "Byte 3: " + reader.ReadByte().ToString();
                        //if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        //if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        this.materialvariantbyte = reader.ReadBytes(materialvariantlength);
                        this.materialvariant = Encoding.UTF8.GetString(materialvariantbyte);
                        LogMessage = string.Format("P{0}/OBJD{1}, - Material Variant: {2}", packageparsecount, e, materialvariant);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        break;
                    case "AC8E1BC0": //Unknown1
                        reader.BaseStream.Position = entrypos;

                        break;
                    case "E4F4FAA4": //SimoleonPrice
                        reader.BaseStream.Position = entrypos;
                        this.price = reader.ReadUInt32();                        
                        LogMessage = string.Format("P{0}/OBJD{1}, - Price: {2}", packageparsecount, e, price);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        break;
                    case "7236BEEA": //PositiveEnvironmentScore
                        reader.BaseStream.Position = entrypos;

                        break;
                    case "44FC7512": //NegativeEnvironmentScore
                        reader.BaseStream.Position = entrypos;

                        break;
                    case "4233F8A0": //ThumbnailGeometryState
                        reader.BaseStream.Position = entrypos;

                        break;
                    case "EC3712E6": //Unknown2
                        reader.BaseStream.Position = entrypos;

                        break;
                    case "2172AEBE": //EnvironmentScoreEmotionTags
                        reader.BaseStream.Position = entrypos;

                        break;
                    case "DCD08394": //EnvironmentScores
                        reader.BaseStream.Position = entrypos;

                        break;
                    case "52F7F4BC": //Unknown3
                        reader.BaseStream.Position = entrypos;

                        break;
                    case "AEE67A1C": //IsBaby
                        reader.BaseStream.Position = entrypos;

                        break;
                    case "F3936A90": //Unknown4
                        reader.BaseStream.Position = entrypos;
                        
                        break;
                }
            }
        }

        public override string ToString(){
            return string.Format("Name: {0} \n Tuning Name: {1} \n TuningID: {2} \n Components: {3} \n Material Variant: {4} \n Price: {5} \n Icon: {6} \n Rig: {7}, Model: {8}, Slot: {9}, Footprint: {10}", this.name, this.tuningname, this.tuningid.ToString("X16"), GetFormatUIntArray(this.components), this.materialvariant, this.price.ToString("X8"), this.icon, this.rig, this.model, this.slot, this.footprint);
        }

        public static string GetFormatUIntArray(uint[] ints){
            string retVal = string.Empty;
            foreach (uint number in ints){
                if (string.IsNullOrEmpty(retVal)){
                    retVal += number.ToString("X8");
                } else {
                    retVal += string.Format(", {0}", number.ToString("X8"));
                }
                
            }
            return retVal;
        }
    }

    public struct Components {
        /// <summary>
        /// Retrieves "components" from a file.
        /// </summary>
        public int[] component;  
        public int[] empty; 

        public Components(BinaryReader reader, uint count){
            if (count == 1){
                this.component = new int[count];
                this.empty = new int[count];
                this.component[0] = reader.ReadUInt16();
                this.empty[0] = 0;
            } else {
                this.component = new int[count];
                this.empty = new int[count];
                for (int i = 0; i < count; i++){
                    this.component[i] = reader.ReadUInt16(); 
                    this.empty[i] = reader.ReadUInt16(); 
                }
            }
        }
    }


    public class indexEntry
    {
        public string typeID;
        public string groupID;
        public string instanceID;
        public long position;
        public uint fileSize;
        public uint memSize;
        public string compressionType;
        
        

    }	
    class S4PackageSearch
    {
        /// <summary>
        /// Sims 4 package reading. Gets all the information from inside S4 Package files and returns it for use.
        /// </summary>
        
        // Class References
        LoggingGlobals log = new LoggingGlobals();
        System.Text.Encoding encoding = System.Text.Encoding.BigEndianUnicode;
        FilesSort filesort = new FilesSort();
        FindOrphans findOrphans = new FindOrphans();

        //lists

        public static bool hasrunbefore;  
        private int ovcapacity = 1781483;


        //Vars
        uint chunkOffset = 0;
        public string[] parameters = {"DefaultForBodyType","DefaultThumbnailPart","AllowForCASRandom","ShowInUI","ShowInSimInfoPanel","ShowInCasDemo","AllowForLiveRandom","DisableForOppositeGender","DisableForOppositeFrame","DefaultForBodyTypeMale","DefaultForBodyTypeFemale","Unk","Unk","Unk","Unk","Unk"};
        

        public void SearchS4Packages(FileStream dbpfFile, FileInfo packageinfo, int packageparsecount, StringBuilder LogFile) {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string LogMessage = "";
            //StringBuilder LogFile = new();
            LogMessage = string.Format("File {0} arrived for processing as Sims 4 file.", packageinfo.Name);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));            
            /*string txt = string.Format("SELECT * FROM Processing_Reader where Name='{0}'", Methods.FixApostrophesforSQL(packageinfo.Name));
            List<PackageFile> queries = GlobalVariables.DatabaseConnection.Query<PackageFile>(txt);
            PackageFile query = queries[0];
            GlobalVariables.DatabaseConnection.Delete(query);
            queries = new List<PackageFile>();
            query = new PackageFile();
            PackageFile pk = new PackageFile { ID = query.ID, Name = packageinfo.Name, Location = packageinfo.FullName, Game = 4, Broken = false, Status = "Processing"};
            GlobalVariables.DatabaseConnection.Insert(pk);
            pk = new PackageFile();*/
                        
            //locations

            long entrycountloc = 36;
            long indexRecordPositionloc = 64;

            SimsPackage thisPackage = new SimsPackage();          

            //Lists 
            
            List<TagsList> itemtags = new List<TagsList>();
            List<TagsList> distinctItemTags = new List<TagsList>();
            List<string> allFlags = new List<string>();      
            List<string> distinctFlags = new List<string>(); 
            List<string> allGUIDS = new List<string>();      
            List<string> distinctGUIDS = new List<string>();  
            List<string> allInstanceIDs = new List<string>();      
            List<string> distinctInstanceIDs = new List<string>();           
            string[] objdentries;
            int[] objdpositions;   
            List<EntryHolder> entries = new List<EntryHolder>();
            List<fileHasList> fileHas = new List<fileHasList>();
            ArrayList linkData = new ArrayList();
            List<indexEntry> indexData = new List<indexEntry>();
            SimsPackage DataDelivery = new SimsPackage();

            itemtags = new List<TagsList>();
            distinctItemTags = new List<TagsList>();

            //create readers  
            //MemoryStream dbpfFile = Methods.ReadBytesToFile(packageinfo.FullName, (int)packageinfo.Length);
            BinaryReader readFile = new BinaryReader(dbpfFile);
            
            //log opening file
            LogMessage = string.Format("Reading package # {0}/{1}: {2}", packageparsecount, GlobalVariables.PackageCount, packageinfo.Name);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            thisPackage.PackageName = packageinfo.Name;
            thisPackage.Location = packageinfo.FullName;            
            thisPackage.Game = 4;
            thisPackage.GameString = "The Sims 4";
            thisPackage.FileSize = (int)packageinfo.Length;
            LogMessage = string.Format("Package #{0} registered as {1} and meant for Sims 4", packageparsecount, packageinfo.FullName);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

            readFile.BaseStream.Position = 0;

            readFile.BaseStream.Position = entrycountloc;

            uint entrycount = readFile.ReadUInt32();
            LogMessage = string.Format("Entry Count: {0}", entrycount.ToString());
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            
            //record position low
            uint indexRecordPositionLow = readFile.ReadUInt32();
            LogMessage = string.Format("IndexRecordPositionLow: {0}", indexRecordPositionLow.ToString());
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            
            //index record size
            uint indexRecordSize = readFile.ReadUInt32();
            LogMessage = string.Format("IndexRecordSize: {0}", indexRecordSize.ToString());
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            
            readFile.BaseStream.Position = indexRecordPositionloc;

            ulong indexRecordPosition = readFile.ReadUInt64();
            LogMessage = string.Format("Index Record Position: {0}", indexRecordPosition.ToString());
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            
                        
            byte[] headersize = new byte[96];
            long here = 100;
            long movedto = 0;

            /*            //dbpf
            test = Encoding.ASCII.GetString(readFile.ReadBytes(4));
            log.MakeLog("DBPF: " + test, true);
            
            //major
            uint testint = readFile.ReadUInt32();
            test = testint.ToString();
            log.MakeLog("Major :" + test, true);
            
            //minor
            testint = readFile.ReadUInt32();
            test = testint.ToString();
            log.MakeLog("Minor : " + test, true);
            
            testint = readFile.ReadUInt32();
            test = testint.ToString();
            log.MakeLog("Unknown : " + test, true);
            
            testint = readFile.ReadUInt32();
            test = testint.ToString();
            log.MakeLog("Unknown : " + test, true);
            
            testint = readFile.ReadUInt32();
            test = testint.ToString();
            log.MakeLog("Unknown : " + test, true);
            
            testint = readFile.ReadUInt32();
            test = testint.ToString();
            log.MakeLog("Created : " + test, true);
            testint = readFile.ReadUInt32();
            test = testint.ToString();
            log.MakeLog("Modified : " + test, true);
            
            testint = readFile.ReadUInt32();
            test = testint.ToString();
            log.MakeLog("Index Major : " + test, true);
            
            //entrycount
            uint entrycount = readFile.ReadUInt32();
            test = entrycount.ToString();
            log.MakeLog("Entry Count: " + test, true);
            
            //record position low
            uint indexRecordPositionLow = readFile.ReadUInt32();
            test = indexRecordPositionLow.ToString();
            log.MakeLog("indexRecordPositionLow: " + test, true);
            
            //index record size
            uint indexRecordSize = readFile.ReadUInt32();
            test = indexRecordSize.ToString();
            log.MakeLog("indexRecordSize: " + test, true);
            //unused
            testint = readFile.ReadUInt32();
            test = testint.ToString();
            log.MakeLog("Unused Trash Index offset: " + test, true);
            
            //unused
            testint = readFile.ReadUInt32();
            test = testint.ToString();
            log.MakeLog("Unused Trash Index size: " + test, true);
            
            //unused
            testint = readFile.ReadUInt32();
            test = testint.ToString();
            log.MakeLog("Unused Index Minor Version: " + test, true);
            
            //unused but 3 for historical reasons
            testint = readFile.ReadUInt32();
            test = testint.ToString();
            log.MakeLog("Unused, 3 for historical reasons: " + test, true);
            
            ulong indexRecordPosition = readFile.ReadUInt64();
            test = indexRecordPosition.ToString();
            log.MakeLog("Inded Record Position: " + test, true);
            //unused
            testint = readFile.ReadUInt32();
            test = testint.ToString();
            log.MakeLog("Unused Unknown:" + test, true);
            
            //unused six bytes
            test = Encoding.ASCII.GetString(readFile.ReadBytes(24));
            log.MakeLog("Unused: " + test, true);*/

            if (indexRecordPosition != 0){
                long indexseek = (long)indexRecordPosition - headersize.Length;
                movedto = here + indexseek;
                readFile.BaseStream.Position = here + indexseek;                
            } else {
                movedto = here + indexRecordPositionLow;
                readFile.BaseStream.Position = here + indexRecordPositionLow;
            }

            readFile.BaseStream.Position = (long)indexRecordPosition;
            uint indextype = readFile.ReadUInt32();            
            
            long streamsize = readFile.BaseStream.Length;
            int indexbytes = ((int)entrycount * 32);
            long indexpos = 0;
            LogMessage = string.Format("P{0} - Streamsize is {1}", packageparsecount, streamsize);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            if ((int)movedto + indexbytes != streamsize){
                int entriesfound = 0;
                LogMessage = string.Format("P{0} - Streamsize does not match!", packageparsecount);
                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                readFile.BaseStream.Position = movedto - 400;
                List<long> entrylocs = new List<long>();
                uint item;
                List<typeList> types = new List<typeList>();
                
                while(readFile.BaseStream.Length > readFile.BaseStream.Position){                    
                    item = readFile.ReadUInt32();
                    GlobalVariables.S4FunctionTypesConnection.Query<typeList>(string.Format("SELECT * FROM S4Types WHERE TypeID='{0}'", item.ToString("X8")));
                    //LogMessage = string.Format("Read byte at {0}: {1}", readFile.BaseStream.Position, item.ToString("X8"));
                    //if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    //if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                    if (types.Any()){
                        indexEntry holderEntry = new indexEntry();
                        holderEntry.typeID = item.ToString("X8");
                        LogMessage = string.Format("P{0}/E{1} - Index Entry TypeID: {2}", packageparsecount, entriesfound, holderEntry.typeID);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        LogMessage = string.Format("Found entry{2} - {0} at location {1}!", item, readFile.BaseStream.Position, entriesfound);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        entrylocs.Add(readFile.BaseStream.Position);
                        if (entriesfound == 0){
                            indexpos = readFile.BaseStream.Position - 4;
                        }
                        entriesfound++;
                        if (entriesfound == entrycount){
                            LogMessage = string.Format("Found {0} entries. Breaking.", entrycount);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            break;
                        }
                    }
                }

                types = new List<typeList>();

                readFile.BaseStream.Position = indexpos;
                entrycount = 0;
                foreach (int loc in entrylocs){
                    indexEntry holderEntry = new indexEntry(); 
                    readFile.BaseStream.Position = loc - 4;
                    if (indextype == 2){
                        holderEntry.typeID = readFile.ReadUInt32().ToString("X8");
                        LogMessage = string.Format("P{0}/E{1} - Index Entry TypeID: {2}", packageparsecount, entrycount, holderEntry.typeID);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        
                        List<typeList> type = GlobalVariables.S4FunctionTypesConnection.Query<typeList>(string.Format("SELECT * FROM S4Types where TypeID='{0}'", holderEntry.typeID));
                    
                        if(type.Any()){                        
                            fileHas.Add(new fileHasList {TypeID = type[0].desc, Location = (int)entrycount});
                            LogMessage = string.Format("File {0} has {1} at location {2}.", thisPackage.PackageName, type[0].desc, (int)entrycount);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        } else {
                            fileHas.Add(new fileHasList() { TypeID = holderEntry.typeID, Location = (int)entrycount});
                        }
                        type = new List<typeList>();                                    

                        string instanceid1 = (readFile.ReadUInt32() << 32).ToString("X8");
                        string instanceid2 = (readFile.ReadUInt32() << 32).ToString("X8");
                        holderEntry.instanceID = string.Format("{0}{1}", instanceid1,instanceid2);
                        if (holderEntry.instanceID != "0000000000000000"){
                            allInstanceIDs.Add(holderEntry.instanceID);
                        }
                        
                        LogMessage = string.Format("P{0}/E{1} - InstanceID: {2}", packageparsecount, entrycount, holderEntry.instanceID);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        
                        holderEntry.position = readFile.ReadUInt32();
                        LogMessage = string.Format("P{0}/E{1} - Index Entry Position: {2}", packageparsecount, entrycount, holderEntry.position.ToString("X8"));
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));                        

                        holderEntry.fileSize = readFile.ReadUInt32();
                        LogMessage = string.Format("P{0}/E{1} - File Size: {2}", packageparsecount, entrycount, holderEntry.fileSize.ToString("X8"));
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                        holderEntry.memSize = readFile.ReadUInt32();
                        LogMessage = string.Format("P{0}/E{1} - Mem Size: {2}", packageparsecount, entrycount, holderEntry.memSize.ToString("X8"));
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                        indexData.Add(holderEntry);

                        holderEntry = null;

                        entrycount++;
                    }
                }
            } else {
                log.MakeLog("Streamsize matches.", true);
                long movedhere = readFile.BaseStream.Position;
                uint testpos = readFile.ReadUInt32();
                if (testpos != 0){
                    LogMessage = string.Format("P{0} - Read first entry TypeID and it read as {1}, returning to read entries.", packageparsecount, testpos.ToString("X8"));
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                    readFile.BaseStream.Position = movedto;
                } else if (testpos == 80000000) {
                    long moveback = movedhere - 4;
                    readFile.BaseStream.Position = moveback;
                } else {
                    LogMessage = string.Format("P{0} - Read first entry TypeID and it read as {1}, moving forward.", packageparsecount, testpos.ToString("X8"));
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                }                
                for (int i = 0; i < entrycount; i++){                    
                    indexEntry holderEntry = new indexEntry();                
                    holderEntry.typeID = readFile.ReadUInt32().ToString("X8");
                    LogMessage = string.Format("P{0}/E{1} - Index Entry TypeID: {2}", packageparsecount, i, holderEntry.typeID);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                    if (holderEntry.typeID == "7FB6AD8A"){
                        thisPackage.Type = "Merged Package";
                        GlobalVariables.AddPackages.Enqueue(thisPackage); 
                        log.MakeLog(string.Format("Package {0} is a merged package, and cannot be processed in this manner right now. Package will either need unmerging or to be sorted manually.", thisPackage.Location), false);

                        /*GlobalVariables.DatabaseConnection.InsertWithChildren(thisPackage);
                        LogMessage = string.Format("Added {0} to packages database successfully.", thisPackage.PackageName);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        txt = string.Format("SELECT * FROM Processing_Reader where Name='{0}'", Methods.FixApostrophesforSQL(packageinfo.Name));
                        List<PackageFile> mergedquery = GlobalVariables.DatabaseConnection.Query<PackageFile>(txt);                    
                        GlobalVariables.DatabaseConnection.Delete(mergedquery[0]);
                        mergedquery = new List<PackageFile>();*/
                        readFile.Dispose();
                        sw.Stop();
                        TimeSpan tss = sw.Elapsed;
                        string elapsedtimee = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                            tss.Hours, tss.Minutes, tss.Seconds,
                                            tss.Milliseconds / 10);
                        GlobalVariables.packagesRead++;
                        LogMessage = string.Format("Reading file {0} took {1}", thisPackage.PackageName, elapsedtimee);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        LogMessage = string.Format("Closing package # {0}/{1}: {2}", packageparsecount, GlobalVariables.PackageCount, packageinfo.Name);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        return;
                    }

                    List<typeList> type = GlobalVariables.S4FunctionTypesConnection.Query<typeList>(string.Format("SELECT * FROM S4Types where TypeID='{0}'", holderEntry.typeID));
                    
                    if(type.Any()){                        
                        fileHas.Add(new fileHasList {TypeID = type[0].desc, Location = i});
                        LogMessage = string.Format("File {0} has {1} at location {2}.", thisPackage.PackageName, type[0].desc, i);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                    } else {
                        fileHas.Add(new fileHasList() { TypeID = holderEntry.typeID, Location = i});
                    }
                    type = new List<typeList>();

                    holderEntry.groupID = readFile.ReadUInt32().ToString("X8");
                    LogMessage = string.Format("P{0}/E{1} - Index Entry GroupID: {2}", packageparsecount, i, holderEntry.groupID);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                    
                    string instanceid1 = (readFile.ReadUInt32() << 32).ToString("X8");
                    string instanceid2 = (readFile.ReadUInt32() << 32).ToString("X8");
                    holderEntry.instanceID = string.Format("{0}{1}", instanceid1,instanceid2);
                    allInstanceIDs.Add(holderEntry.instanceID);
                    LogMessage = string.Format("P{0}/E{1} - InstanceID: {2}", packageparsecount, i, holderEntry.instanceID);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                    uint testin = readFile.ReadUInt32();
                    holderEntry.position = (long)testin;
                    LogMessage = string.Format("P{0}/E{1} - Position: {2}", packageparsecount, i, holderEntry.position);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                    holderEntry.fileSize = readFile.ReadUInt32();
                    LogMessage = string.Format("P{0}/E{1} - File Size: {2}", packageparsecount, i, holderEntry.fileSize);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                    holderEntry.memSize = readFile.ReadUInt32();
                    LogMessage = string.Format("P{0}/E{1} - Mem Size: {2}", packageparsecount, i, holderEntry.memSize);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                    holderEntry.compressionType = readFile.ReadUInt16().ToString("X4");
                    LogMessage = string.Format("P{0}/E{1} - Compression Type: {2}", packageparsecount, i, holderEntry.compressionType);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                    readFile.BaseStream.Position = readFile.BaseStream.Position + 2;

                    indexData.Add(holderEntry);

                    holderEntry = null;
                }
            }

            if (fileHas.Exists(x => x.TypeID == "CASP")){

                var entryspots = (from has in fileHas
                        where has.TypeID == "CASP"
                        select has.Location).ToList();

                thisPackage.Recolor = true;

                int caspc = 0;
                foreach (int e in entryspots){
                    LogMessage = string.Format("P{0} - Opening CASP #{1}", packageparsecount, e);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                    if (indexData[e].compressionType == "5A42"){
                        LogMessage = string.Format("P{0}/CASP{1} - Compression type is {2}.", packageparsecount, e, indexData[e].compressionType);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        readFile.BaseStream.Position = indexData[e].position;
                        long entryEnd = indexData[e].position + indexData[e].memSize;

                        LogMessage = string.Format("P{0}/CASP{1} - Position: {2}", packageparsecount, e, indexData[e].position);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        LogMessage = string.Format("P{0}/CASP{1} - Filesize: {2}", packageparsecount, e, indexData[e].fileSize);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        LogMessage = string.Format("P{0}/CASP{1} - Memsize: {2}", packageparsecount, e, indexData[e].memSize);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        LogMessage = string.Format("P{0}/CASP{1} - Ends at: {2}", packageparsecount, e, entryEnd);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                        Stream decomps = S4Decryption.Decompress(Methods.ReadEntryBytes(readFile, (int)indexData[e].memSize));
                        
                        BinaryReader decompbr = new BinaryReader(decomps);

                        uint version = decompbr.ReadUInt32();
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Version: {2}", packageparsecount, e, version);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        uint tgioffset = decompbr.ReadUInt32() +8;
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - TGI Offset: {2} \n -- As Hex: {3}", packageparsecount, e, tgioffset, tgioffset.ToString("X8"));
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        uint numpresets = decompbr.ReadUInt32();
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Number of Presets: {2} \n -- As Hex: {3}", packageparsecount, e, numpresets, numpresets.ToString("X8"));
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        using (BinaryReader reader = new BinaryReader(decomps, Encoding.BigEndianUnicode, true))
                        {
                            thisPackage.Title = reader.ReadString();
                        }
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Title: {2}", packageparsecount, e, thisPackage.Title);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                        float sortpriority = decompbr.ReadSingle();
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Sort Priority: {2}", packageparsecount, e, sortpriority);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                        int secondarySortIndex = decompbr.ReadUInt16();
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Secondary Sort Index: {2}", packageparsecount, e, secondarySortIndex);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                        uint propertyid = decompbr.ReadUInt32();
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Property ID: {2}", packageparsecount, e, propertyid);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        
                        uint auralMaterialHash = decompbr.ReadUInt32();
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Aural Material Hash: {2}", packageparsecount, e, sortpriority);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                        if (version <= 42){
                            LogMessage = string.Format("Version is <= 42: {0}", version);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            int[] parameterFlag = new int[1];
                            parameterFlag[0] = (int)decompbr.ReadUInt16();
                            BitArray parameterFlags = new BitArray(parameterFlag);
                            LogMessage = parameterFlags.Length.ToString();
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            for(int p = 0; p < 16; p++)
                            {
                                if (parameterFlags[p] == true) {
                                    allFlags.Add(parameters[p]);
                                }
                                LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Function Sort Flag [{2}]: {3}, {4}", packageparsecount, e, p, parameters[p], parameterFlags[p].ToString());
                                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            } 
                            
                        } else if (version >= 43){
                            LogMessage = string.Format("Version is >= 43: {0}", version);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            int[] parameterFlag = new int[1];
                            parameterFlag[0] = (int)decompbr.ReadUInt16();
                            BitArray parameterFlags = new BitArray(parameterFlag);

                            for(int pfc = 0; pfc < 16; pfc++){
                                if (parameterFlags[pfc] == true) {
                                    allFlags.Add(parameters[pfc]);
                                }
                                LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Function Sort Flag [{2}]: {3}, {4}", packageparsecount, e, pfc, parameters[pfc], parameterFlags[pfc].ToString());
                                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            }                            
                        }
                            ulong excludePartFlags = decompbr.ReadUInt64();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Exclude Part Flags: {2}", packageparsecount, e, excludePartFlags.ToString("X16"));
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                            ulong excludePartFlags2 = decompbr.ReadUInt64();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Exclude Part Flags 2: {2}", packageparsecount, e, excludePartFlags2.ToString("X16"));
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                            ulong excludeModifierRegionFlags = decompbr.ReadUInt64();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Exclude Modifier Region Flags: {2}", packageparsecount, e, excludeModifierRegionFlags.ToString("X16"));
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                        if (version >= 37){
                            LogMessage = string.Format(">= 37, Version: {0}", version);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                            uint count = decompbr.ReadByte();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Tag Count: {2}", packageparsecount, e, count.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            decompbr.ReadByte();

                            CASTag16Bit tags = new CASTag16Bit(decompbr, count);      


                            List<TagsList> gottags = Readers.GetTagInfo(tags, count, LogFile);
                            foreach (TagsList tag in gottags){
                                if (!itemtags.Exists(x => x.TypeID == tag.TypeID) && !itemtags.Exists(x => x.Description == tag.Description)){
                                    itemtags.Add(tag);  
                                }
                            }                                                        
                        } 
                        else 
                        {
                            uint count = decompbr.ReadByte();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Tag Count: {2}", packageparsecount, e, count.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            decompbr.ReadByte();
                            CASTag16Bit tags = new CASTag16Bit(decompbr, count);
                            
                            List<TagsList> gottags = Readers.GetTagInfo(tags, count, LogFile);
                            foreach (TagsList tag in gottags){
                                if (!itemtags.Exists(x => x.TypeID == tag.TypeID) && !itemtags.Exists(x => x.Description == tag.Description)){
                                    itemtags.Add(tag);  
                                }
                            }

                            }

                            uint simoleonprice = decompbr.ReadUInt32();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Simoleon Price: {2}", packageparsecount, e, simoleonprice.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            uint partTitleKey = decompbr.ReadUInt32();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Part Title Key: {2}", packageparsecount, e, partTitleKey.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            uint partDescriptionKey = decompbr.ReadUInt32();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Part Description Key: {2}", packageparsecount, e, partDescriptionKey.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));                            
                            if (version >= 43) {
                                uint createDescriptionKey = decompbr.ReadUInt32();
                            }
                            int uniqueTextureSpace = decompbr.ReadByte();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Unique Texture Space: {2}", packageparsecount, e, uniqueTextureSpace.ToString("X8"));
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            uint bodytype = decompbr.ReadUInt32();
                            bool foundmatch = false;

                            List<FunctionListing> bodytypes = GlobalVariables.S4FunctionTypesConnection.Query<FunctionListing>(string.Format("SELECT * FROM S4CASFunctions where BodyType='{0}'", bodytype)); 

                            if (bodytypes.Any()){
                                foundmatch = true;
                                thisPackage.Function = bodytypes[0].Function;
                                if (!String.IsNullOrWhiteSpace(bodytypes[0].Subfunction)) {
                                    thisPackage.FunctionSubcategory = bodytypes[0].Subfunction;
                                } 
                            }

                            if (foundmatch == false){
                                thisPackage.Function = string.Format("Unidentified function (contact SinfulSimming). Code: {0}", bodytype.ToString());
                            }                            
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Bodytype: {2}", packageparsecount, e, bodytype.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            uint bodytypesubtype = decompbr.ReadUInt16();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Body Sub Type: {2}", packageparsecount, e, bodytypesubtype.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            decompbr.ReadUInt32();                        
                            uint agflags = decompbr.ReadUInt32();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Ag Flags: {2}", packageparsecount, e, agflags.ToString("X8"));
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                            string AGFlag = agflags.ToString("X8");
                            
                            AgeGenderFlags agegenderset = new AgeGenderFlags();

                            if (AGFlag == "00000000") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = false, 
                                Male = false};
                            } else if (AGFlag == "00000020") {
                                agegenderset = new AgeGenderFlags{
                                Adult = true, 
                                Baby = false, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = false, 
                                Male = false};
                            } else if (AGFlag == "00002020") {
                                agegenderset = new AgeGenderFlags{
                                    Adult = true, 
                                Baby = false, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = true, 
                                Male = false};
                            } else if (AGFlag == "00020000") {
                                agegenderset = new AgeGenderFlags{
                                    Adult = true, 
                                    Baby = false, 
                                    Child = false, 
                                    Elder = false, 
                                    Infant = false, 
                                    Teen = false, 
                                    Toddler = false, 
                                    YoungAdult = false, 
                                    Female = false, 
                                    Male = true};
                            } else if (AGFlag == "00002078") {
                                agegenderset = new AgeGenderFlags{
                                    Adult = true, 
                                    Baby = false, 
                                    Child = false, 
                                    Elder = true, 
                                    Infant = false, 
                                    Teen = true, 
                                    Toddler = false, 
                                    YoungAdult = true, 
                                    Female = true, 
                                    Male = false};
                            } else if (AGFlag == "000030FF") {
                                agegenderset = new AgeGenderFlags{
                                Adult = true, 
                                Baby = true, 
                                Child = true, 
                                Elder = true, 
                                Infant = true, 
                                Teen = true, 
                                Toddler = true, 
                                YoungAdult = true, 
                                Female = true, 
                                Male = true};
                            } else if (AGFlag == "00003004") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = true, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = true, 
                                Male = true};
                            } else if (AGFlag == "00001078") {
                                agegenderset = new AgeGenderFlags{
                                Adult = true, 
                                Baby = false, 
                                Child = false, 
                                Elder = true, 
                                Infant = false, 
                                Teen = true, 
                                Toddler = false, 
                                YoungAdult = true, 
                                Female = false, 
                                Male = true};
                            } else if (AGFlag == "00003078") {
                                agegenderset = new AgeGenderFlags{
                                Adult = true, 
                                Baby = false, 
                                Child = false, 
                                Elder = true, 
                                Infant = false, 
                                Teen = true, 
                                Toddler = false, 
                                YoungAdult = true, 
                                Female = true, 
                                Male = true};
                            } else if (AGFlag == "000030BE") {
                                agegenderset = new AgeGenderFlags{
                                Adult = true, 
                                Baby = false, 
                                Child = true, 
                                Elder = false, 
                                Infant = true, 
                                Teen = true, 
                                Toddler = true, 
                                YoungAdult = true, 
                                Female = true, 
                                Male = true};
                            } else if (AGFlag == "00002002") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = true, 
                                YoungAdult = false, 
                                Female = true, 
                                Male = false};
                            }  else if (AGFlag == "00002004") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = true, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = true, 
                                Male = false};
                            }  else if (AGFlag == "00003002") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = true, 
                                YoungAdult = false, 
                                Female = true, 
                                Male = true};
                            }  else if (AGFlag == "00003004") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = true, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = true, 
                                Male = true};
                            }  else if (AGFlag == "00001002") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = true, 
                                YoungAdult = false, 
                                Female = false, 
                                Male = true};
                            }  else if (AGFlag == "00001004") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = true, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = false, 
                                Male = true};
                            } else if (AGFlag == "00100101") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = true, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = false, 
                                Male = false};                            
                            } else if (AGFlag == "0000307E"){
                                agegenderset = new AgeGenderFlags{
                                Adult = true, 
                                Baby = false, 
                                Child = true, 
                                Elder = true, 
                                Infant = false, 
                                Teen = true, 
                                Toddler = true, 
                                YoungAdult = true, 
                                Female = true, 
                                Male = true};
                            } else if (AGFlag == "000030FE"){
                                agegenderset = new AgeGenderFlags{
                                Adult = true, 
                                Baby = false, 
                                Child = true, 
                                Elder = true, 
                                Infant = true, 
                                Teen = true, 
                                Toddler = true, 
                                YoungAdult = true, 
                                Female = true, 
                                Male = true};
                            } else {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = false, 
                                Male = false};
                            }
                            
                            LogMessage = string.Format("P{0}/CASP{1} Ages:\n -- Adult: {0}, \n -- Baby: {1}, \n -- Child: {2}, \n -- Elder: {3}, \n -- Infant: {4}, \n -- Teen: {5}, \n -- Toddler: {6}, \n -- Young Adult: {7}\n\nGender: \n -- Male: {8}\n -- Female: {9}.", packageparsecount, e, agegenderset.Adult.ToString(), agegenderset.Baby.ToString(), agegenderset.Child.ToString(), agegenderset.Elder.ToString(), agegenderset.Infant.ToString(), agegenderset.Teen.ToString(), agegenderset.Toddler.ToString(), agegenderset.YoungAdult.ToString(), agegenderset.Female.ToString(), agegenderset.Male.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                            thisPackage.AgeGenderFlags = agegenderset;

                            


                            if (version >= 0x20)
                            {
                                uint species = decompbr.ReadUInt32();
                            }
                            if (version >= 34)
                            {
                                int packID = decompbr.ReadInt16();
                                int packFlags = decompbr.ReadByte();
                                for(int p = 0; p < packFlags; p++)
                                {
                                    bool check = decompbr.ReadBoolean();
                                    LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Packflag is: {2}", packageparsecount, e, check.ToString());
                                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                                } 
                                byte[] reserved2 = decompbr.ReadBytes(9);
                            }
                            else
                            {
                                byte unused2 = decompbr.ReadByte();
                                if (unused2 > 0) {
                                    int unused3 = decompbr.ReadByte();
                                }
                            }

                            uint buffResKey = decompbr.ReadByte();
                            uint varientThumbnailKey = decompbr.ReadByte();
                            if (version >= 28){
                                ulong voiceEffecthash = decompbr.ReadUInt64();
                            }
                            if (version >= 30){
                                uint usedMaterialCount = decompbr.ReadByte();
                                if (usedMaterialCount > 0){
                                    uint materialSetUpperBodyHash = decompbr.ReadUInt32();
                                    uint materialSetLowerBodyHash = decompbr.ReadUInt32();
                                    uint materialSetShoesBodyHash = decompbr.ReadUInt32();
                                }
                            }
                            if (version >= 38){
                                //idk
                            }

                            uint nakedKey = decompbr.ReadByte();
                            uint parentKey = decompbr.ReadByte();
                            uint sortLayer = decompbr.ReadUInt32();

                            decompbr.BaseStream.Position = tgioffset;
                            var tginum = decompbr.ReadByte();
                            for (int t = 0; t < tginum; t++){
                                ulong iid = decompbr.ReadUInt64();
                                uint gid = decompbr.ReadUInt32();
                                uint tid = decompbr.ReadUInt32();
                                string key = string.Format("{0}-{1}-{2}", tid.ToString("X8"), gid.ToString("X8"), iid.ToString("X16"));
                                if (key != "00000000-00000000-0000000000000000"){
                                    if (!thisPackage.CASPartKeys.Contains(key)){
                                        thisPackage.CASPartKeys.Add(key);
                                    } 
                                }  
                            }
                            decompbr.Dispose();
                            decomps.Dispose();

                    } else if (indexData[e].compressionType == "0000"){
                        LogMessage = string.Format("P{0}/CASP{1} - Compression type is {2}.", packageparsecount, e, indexData[e].compressionType);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        readFile.BaseStream.Position = indexData[e].position;
                        long entryEnd = indexData[e].position + indexData[e].memSize;

                        LogMessage = string.Format("P{0}/CASP{1} - Position: {2}", packageparsecount, e, indexData[e].position);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        LogMessage = string.Format("P{0}/CASP{1} - Filesize: {2}", packageparsecount, e, indexData[e].fileSize);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        LogMessage = string.Format("P{0}/CASP{1} - Memsize: {2}", packageparsecount, e, indexData[e].memSize);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        LogMessage = string.Format("P{0}/CASP{1} - Ends at: {2}", packageparsecount, e, entryEnd);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                        uint version = readFile.ReadUInt32();
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Version: {2}", packageparsecount, e, version);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        uint tgioffset = readFile.ReadUInt32() +8;
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - TGI Offset: {2} \n -- As Hex: {3}", packageparsecount, e, tgioffset, tgioffset.ToString("X8"));
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        uint numpresets = readFile.ReadUInt32();
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Number of Presets: {2} \n -- As Hex: {3}", packageparsecount, e, numpresets, numpresets.ToString("X8"));
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        using (BinaryReader reader = new BinaryReader(dbpfFile, Encoding.BigEndianUnicode, true))
                        {
                            thisPackage.Title = reader.ReadString();
                        }
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Title: {2}", packageparsecount, e, thisPackage.Title);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                        float sortpriority = readFile.ReadSingle();
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Sort Priority: {2}", packageparsecount, e, sortpriority);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                        int secondarySortIndex = readFile.ReadUInt16();
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Secondary Sort Index: {2}", packageparsecount, e, secondarySortIndex);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                        uint propertyid = readFile.ReadUInt32();
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Property ID: {2}", packageparsecount, e, propertyid);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        
                        uint auralMaterialHash = readFile.ReadUInt32();
                        LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Aural Material Hash: {2}", packageparsecount, e, sortpriority);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                        if (version <= 42){
                            LogMessage = string.Format("Version is <= 42: {0}", version);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            int[] parameterFlag = new int[1];
                            parameterFlag[0] = (int)readFile.ReadUInt16();
                            BitArray parameterFlags = new BitArray(parameterFlag);
                            LogMessage = parameterFlags.Length.ToString();
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            for(int p = 0; p < 16; p++)
                            {
                                if (parameterFlags[p] == true) {
                                    allFlags.Add(parameters[p]);
                                }
                                LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Function Sort Flag [{2}]: {3}, {4}", packageparsecount, e, p, parameters[p], parameterFlags[p].ToString());
                                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            } 
                            
                        } else if (version >= 43){
                            LogMessage = string.Format("Version is >= 43: {0}", version);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            int[] parameterFlag = new int[1];
                            parameterFlag[0] = (int)readFile.ReadUInt16();
                            BitArray parameterFlags = new BitArray(parameterFlag);

                            for(int pfc = 0; pfc < 16; pfc++){
                                if (parameterFlags[pfc] == true) {
                                    allFlags.Add(parameters[pfc]);
                                }
                                LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Function Sort Flag [{2}]: {3}, {4}", packageparsecount, e, pfc, parameters[pfc], parameterFlags[pfc].ToString());
                                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            }                            
                        }
                            ulong excludePartFlags = readFile.ReadUInt64();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Exclude Part Flags: {2}", packageparsecount, e, excludePartFlags.ToString("X16"));
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            ulong excludePartFlags2 = readFile.ReadUInt64();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Exclude Part Flags 2: {2}", packageparsecount, e, excludePartFlags2.ToString("X16"));
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            ulong excludeModifierRegionFlags = readFile.ReadUInt64();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Exclude Modifier Region Flags: {2}", packageparsecount, e, excludeModifierRegionFlags.ToString("X16"));
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                        if (version >= 37){
                            LogMessage = string.Format(">= 37, Version: {0}", version);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            uint count = readFile.ReadByte();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Tag Count: {2}", packageparsecount, e, count.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            readFile.ReadByte();
                            CASTag16Bit tags = new CASTag16Bit(readFile, count);                            
                            List<TagsList> gottags = Readers.GetTagInfo(tags, count, LogFile);
                            foreach (TagsList tag in gottags){
                                if (!itemtags.Exists(x => x.TypeID == tag.TypeID) && !itemtags.Exists(x => x.Description == tag.Description)){
                                    itemtags.Add(tag);  
                                }
                            }
                                                        
                        } 
                        else 
                        {
                            uint count = readFile.ReadByte();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Tag Count: {2}", packageparsecount, e, count.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            readFile.ReadByte();
                            CASTag16Bit tags = new CASTag16Bit(readFile, count);
                            
                            List<TagsList> gottags = Readers.GetTagInfo(tags, count, LogFile);
                            foreach (TagsList tag in gottags){
                                if (!itemtags.Exists(x => x.TypeID == tag.TypeID) && !itemtags.Exists(x => x.Description == tag.Description)){
                                    itemtags.Add(tag);  
                                }
                            }

                            }

                            uint simoleonprice = readFile.ReadUInt32();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Simoleon Price: {2}", packageparsecount, e, simoleonprice.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            uint partTitleKey = readFile.ReadUInt32();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Part Title Key: {2}", packageparsecount, e, partTitleKey.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            uint partDescriptionKey = readFile.ReadUInt32();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Part Description Key: {2}", packageparsecount, e, partDescriptionKey.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));                            
                            if (version >= 43) {
                                uint createDescriptionKey = readFile.ReadUInt32();
                            }
                            int uniqueTextureSpace = readFile.ReadByte();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Unique Texture Space: {2}", packageparsecount, e, uniqueTextureSpace.ToString("X8"));
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            uint bodytype = readFile.ReadUInt32();
                            bool foundmatch = false;

                            List<FunctionListing> bodytypes = GlobalVariables.S4FunctionTypesConnection.Query<FunctionListing>(string.Format("SELECT * FROM S4CASFunctions where BodyType='{0}'", bodytype)); 

                            if (bodytypes.Any()){
                                foundmatch = true;
                                thisPackage.Function = bodytypes[0].Function;
                                if (!String.IsNullOrWhiteSpace(bodytypes[0].Subfunction)) {
                                    thisPackage.FunctionSubcategory = bodytypes[0].Subfunction;
                                } 
                            }

                            if (foundmatch == false){
                                thisPackage.Function = string.Format("Unidentified function (contact SinfulSimming). Code: {0}", bodytype.ToString());
                            }                            
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Bodytype: {2}", packageparsecount, e, bodytype.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            uint bodytypesubtype = readFile.ReadUInt16();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Body Sub Type: {2}", packageparsecount, e, bodytypesubtype.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            readFile.ReadUInt32();                        
                            uint agflags = readFile.ReadUInt32();
                            LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Ag Flags: {2}", packageparsecount, e, agflags.ToString("X8"));
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                            string AGFlag = agflags.ToString("X8");
                            
                            AgeGenderFlags agegenderset = new AgeGenderFlags();

                            if (AGFlag == "00000000") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = false, 
                                Male = false};
                            } else if (AGFlag == "00000020") {
                                agegenderset = new AgeGenderFlags{
                                Adult = true, 
                                Baby = false, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = false, 
                                Male = false};
                            } else if (AGFlag == "00002020") {
                                agegenderset = new AgeGenderFlags{
                                    Adult = true, 
                                Baby = false, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = true, 
                                Male = false};
                            } else if (AGFlag == "00020000") {
                                agegenderset = new AgeGenderFlags{
                                    Adult = true, 
                                    Baby = false, 
                                    Child = false, 
                                    Elder = false, 
                                    Infant = false, 
                                    Teen = false, 
                                    Toddler = false, 
                                    YoungAdult = false, 
                                    Female = false, 
                                    Male = true};
                            } else if (AGFlag == "00002078") {
                                agegenderset = new AgeGenderFlags{
                                    Adult = true, 
                                    Baby = false, 
                                    Child = false, 
                                    Elder = true, 
                                    Infant = false, 
                                    Teen = true, 
                                    Toddler = false, 
                                    YoungAdult = true, 
                                    Female = true, 
                                    Male = false};
                            } else if (AGFlag == "000030FF") {
                                agegenderset = new AgeGenderFlags{
                                Adult = true, 
                                Baby = true, 
                                Child = true, 
                                Elder = true, 
                                Infant = true, 
                                Teen = true, 
                                Toddler = true, 
                                YoungAdult = true, 
                                Female = true, 
                                Male = true};
                            } else if (AGFlag == "00003004") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = true, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = true, 
                                Male = true};
                            } else if (AGFlag == "00001078") {
                                agegenderset = new AgeGenderFlags{
                                Adult = true, 
                                Baby = false, 
                                Child = false, 
                                Elder = true, 
                                Infant = false, 
                                Teen = true, 
                                Toddler = false, 
                                YoungAdult = true, 
                                Female = false, 
                                Male = true};
                            } else if (AGFlag == "00003078") {
                                agegenderset = new AgeGenderFlags{
                                Adult = true, 
                                Baby = false, 
                                Child = false, 
                                Elder = true, 
                                Infant = false, 
                                Teen = true, 
                                Toddler = false, 
                                YoungAdult = true, 
                                Female = true, 
                                Male = true};
                            } else if (AGFlag == "000030BE") {
                                agegenderset = new AgeGenderFlags{
                                Adult = true, 
                                Baby = false, 
                                Child = true, 
                                Elder = false, 
                                Infant = true, 
                                Teen = true, 
                                Toddler = true, 
                                YoungAdult = true, 
                                Female = true, 
                                Male = true};
                            } else if (AGFlag == "00002002") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = true, 
                                YoungAdult = false, 
                                Female = true, 
                                Male = false};
                            }  else if (AGFlag == "00002004") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = true, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = true, 
                                Male = false};
                            }  else if (AGFlag == "00003002") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = true, 
                                YoungAdult = false, 
                                Female = true, 
                                Male = true};
                            }  else if (AGFlag == "00003004") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = true, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = true, 
                                Male = true};
                            }  else if (AGFlag == "00001002") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = true, 
                                YoungAdult = false, 
                                Female = false, 
                                Male = true};
                            }  else if (AGFlag == "00001004") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = true, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = false, 
                                Male = true};
                            } else if (AGFlag == "00100101") {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = true, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = false, 
                                Male = false};                            
                            } else if (AGFlag == "0000307E"){
                                agegenderset = new AgeGenderFlags{
                                Adult = true, 
                                Baby = false, 
                                Child = true, 
                                Elder = true, 
                                Infant = false, 
                                Teen = true, 
                                Toddler = true, 
                                YoungAdult = true, 
                                Female = true, 
                                Male = true};
                            } else if (AGFlag == "000030FE"){
                                agegenderset = new AgeGenderFlags{
                                Adult = true, 
                                Baby = false, 
                                Child = true, 
                                Elder = true, 
                                Infant = true, 
                                Teen = true, 
                                Toddler = true, 
                                YoungAdult = true, 
                                Female = true, 
                                Male = true};
                            } else {
                                agegenderset = new AgeGenderFlags{
                                Adult = false, 
                                Baby = false, 
                                Child = false, 
                                Elder = false, 
                                Infant = false, 
                                Teen = false, 
                                Toddler = false, 
                                YoungAdult = false, 
                                Female = false, 
                                Male = false};
                            }
                            
                            LogMessage = string.Format("P{0}/CASP{1} Ages:\n -- Adult: {0}, \n -- Baby: {1}, \n -- Child: {2}, \n -- Elder: {3}, \n -- Infant: {4}, \n -- Teen: {5}, \n -- Toddler: {6}, \n -- Young Adult: {7}\n\nGender: \n -- Male: {8}\n -- Female: {9}.", packageparsecount, e, agegenderset.Adult.ToString(), agegenderset.Baby.ToString(), agegenderset.Child.ToString(), agegenderset.Elder.ToString(), agegenderset.Infant.ToString(), agegenderset.Teen.ToString(), agegenderset.Toddler.ToString(), agegenderset.YoungAdult.ToString(), agegenderset.Female.ToString(), agegenderset.Male.ToString());
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

                            thisPackage.AgeGenderFlags = agegenderset;

                            


                            if (version >= 0x20)
                            {
                                uint species = readFile.ReadUInt32();
                            }
                            if (version >= 34)
                            {
                                int packID = readFile.ReadInt16();
                                int packFlags = readFile.ReadByte();
                                for(int p = 0; p < packFlags; p++)
                                {
                                    bool check = readFile.ReadBoolean();
                                    LogMessage = string.Format("P{0}/CASP{1} [Decompressed] - Packflag is: {2}", packageparsecount, e, check.ToString());
                                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                                } 
                                byte[] reserved2 = readFile.ReadBytes(9);
                            }
                            else
                            {
                                byte unused2 = readFile.ReadByte();
                                if (unused2 > 0) {
                                    int unused3 = readFile.ReadByte();
                                }
                            }

                            readFile.BaseStream.Position = tgioffset;
                            var tginum = readFile.ReadByte();
                            for (int t = 0; t < tginum; t++){
                                ulong iid = readFile.ReadUInt64();
                                uint gid = readFile.ReadUInt32();
                                uint tid = readFile.ReadUInt32();
                                string key = string.Format("{0}-{1}-{2}", tid.ToString("X8"), gid.ToString("X8"), iid.ToString("X16"));
                                if (key != "00000000-00000000-0000000000000000"){
                                    if (!thisPackage.CASPartKeys.Contains(key)){
                                        thisPackage.CASPartKeys.Add(key);
                                    } 
                                }                                
                            }
                    }

                    caspc++;
                    
                }                     

            }


            if (fileHas.Exists(x => x.TypeID == "COBJ")){

                var entryspots = (from has in fileHas
                        where has.TypeID == "COBJ"
                        select has.Location).ToList();

                int cobjc = 0;
                foreach (int e in entryspots){
                    
                    LogMessage = string.Format("P{0} - Opening COBJ #{1}", packageparsecount, cobjc);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                    if (indexData[e].compressionType == "5A42"){                                
                            readFile.BaseStream.Position = indexData[e].position;                  
                            int entryEnd = (int)readFile.BaseStream.Position + (int)indexData[e].memSize;

                            LogMessage = string.Format("P{0}/COBJ{1} - Position: {2}", packageparsecount, cobjc, indexData[e].position);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            LogMessage = string.Format("P{0}/COBJ{1} - File Size: {2}", packageparsecount, cobjc, indexData[e].fileSize);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            LogMessage = string.Format("P{0}/COBJ{1} - Memory Size: {2}", packageparsecount, cobjc, indexData[e].memSize);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            LogMessage = string.Format("P{0}/COBJ{1} - Entry Ends At: {2}", packageparsecount, cobjc, entryEnd);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            Stream decomps = S4Decryption.Decompress(Methods.ReadEntryBytes(readFile, (int)indexData[e].memSize));
                            
                            BinaryReader decompbr = new BinaryReader(decomps);                           
                            
                            ReadCOBJ rc = new ReadCOBJ(decompbr, packageparsecount, e, itemtags, LogFile);
                            if (!allInstanceIDs.Contains(rc.instanceid.ToString("X8"))){
                                allInstanceIDs.Add(rc.instanceid.ToString("X8"));  
                            }                                                      
                            if((!allGUIDS.Contains(rc.GUID)) && rc.GUID != "null"){
                                allGUIDS.Add(rc.GUID);
                            }

                            foreach (TagsList tag in rc.itemtags) {
                                if (!itemtags.Exists(x => x.TypeID == tag.TypeID) && !itemtags.Exists(x => x.Description == tag.Description)){
                                    itemtags.Add(tag);  
                                }                                    
                            }
                        decompbr.Dispose();
                        decomps.Dispose();
                            
                    } else if (indexData[e].compressionType == "00"){
                        readFile.BaseStream.Position = indexData[e].position;                           
                        int entryEnd = (int)readFile.BaseStream.Position + (int)indexData[e].memSize;
                        
                        LogMessage = string.Format("P{0}/COBJ{1} - Position: {2}", packageparsecount, cobjc, indexData[e].position);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        LogMessage = string.Format("P{0}/COBJ{1} - File Size: {2}", packageparsecount, cobjc, indexData[e].fileSize);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        LogMessage = string.Format("P{0}/COBJ{1} - Memory Size: {2}", packageparsecount, cobjc, indexData[e].memSize);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        LogMessage = string.Format("P{0}/COBJ{1} - Entry Ends At: {2}", packageparsecount, cobjc, entryEnd);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        ReadCOBJ rc = new ReadCOBJ(readFile, packageparsecount, e, itemtags, LogFile);                            
                        if (!allInstanceIDs.Contains(rc.instanceid.ToString("X8"))){
                            allInstanceIDs.Add(rc.instanceid.ToString("X8"));  
                        }                                                      
                        if((!allGUIDS.Contains(rc.GUID)) && rc.GUID != "null"){
                            allGUIDS.Add(rc.GUID);
                        }

                        foreach (TagsList tag in rc.itemtags) {
                            if (!itemtags.Exists(x => x.TypeID == tag.TypeID) && !itemtags.Exists(x => x.Description == tag.Description)){
                                itemtags.Add(tag);  
                            }                                    
                        }
                    }
                    cobjc++;
                }
            }

            if (fileHas.Exists(x => x.TypeID == "OBJD")){
                var entryspots = (from has in fileHas
                        where has.TypeID == "OBJD"
                        select has.Location).ToList();

                int objdc = 0;
                foreach (int e in entryspots){
                    LogMessage = string.Format("P{0} - Opening OBJD #{1}", packageparsecount, objdc);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                    if (indexData[e].compressionType == "5A42"){
                            readFile.BaseStream.Position = indexData[e].position;                       
                            int entryEnd = (int)readFile.BaseStream.Position + (int)indexData[e].memSize;
                            LogMessage = string.Format("P{0}/OBJD{1} - Position: {2}", packageparsecount, objdc, indexData[e].position);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            LogMessage = string.Format("P{0}/OBJD{1} - File Size: {2}", packageparsecount, objdc, indexData[e].fileSize);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            LogMessage = string.Format("P{0}/OBJD{1} - Memory Size: {2}", packageparsecount, objdc, indexData[e].memSize);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            LogMessage = string.Format("P{0}/OBJD{1} - Entry Ends At: {2}", packageparsecount, objdc, entryEnd);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            Stream decomps = S4Decryption.Decompress(Methods.ReadEntryBytes(readFile, (int)indexData[e].memSize));
                            
                            BinaryReader decompbr = new BinaryReader(decomps);

                            ReadOBJDIndex readOBJD = new ReadOBJDIndex(decompbr, packageparsecount, objdc, LogFile);
                            LogMessage = string.Format("P{0}/OBJD{1} - There are {2} entries to read.", packageparsecount, objdc, readOBJD.count);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            objdentries = new string[readOBJD.count];
                            objdpositions = new int[readOBJD.count];
                            for (int f = 0; f < readOBJD.count; f++){
                                LogMessage = string.Format("P{0}/OBJD{1} - Entry {2}: \n--- Type: {3}\n --- Position: {4}", packageparsecount, objdc, f, readOBJD.entrytype[f], readOBJD.position[f]);
                                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                                objdentries[f] = readOBJD.entrytype[f].ToString();
                                objdpositions[f] = (int)readOBJD.position[f];
                            }
                            decompbr.BaseStream.Position = readOBJD.position[0];
                            ReadOBJDEntry readobjdentry = new ReadOBJDEntry(decompbr, objdentries, objdpositions, packageparsecount, objdc, LogFile);
                            thisPackage.Title = readobjdentry.name;
                            thisPackage.Tuning = readobjdentry.tuningname;
                            thisPackage.TuningID = (int)readobjdentry.tuningid;
                            if(!allInstanceIDs.Contains(readobjdentry.instance.ToString("X8"))){
                                allInstanceIDs.Add(readobjdentry.instance.ToString("X8"));
                            }
                            
                            log.MakeLog("Adding components to package: ", true);
                            for (int c = 0; c < readobjdentry.componentcount; c++){
                                LogMessage = readobjdentry.components[c].ToString();
                                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                                LogMessage = readobjdentry.components[c].ToString("X8");
                                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                                thisPackage.Components.Add(readobjdentry.components[c].ToString("X8"));
                            }
                            foreach (string m in readobjdentry.model) {
                                if(!allGUIDS.Contains(m)){
                                    allGUIDS.Add(m);
                                }  
                            }
                            foreach (string r in readobjdentry.rig) {
                                if(!allGUIDS.Contains(r)){
                                    allGUIDS.Add(r);
                                }  
                            }
                            foreach (string s in readobjdentry.slot) {
                                if(!allGUIDS.Contains(s)){
                                    allGUIDS.Add(s);
                                }  
                            }
                            foreach (string f in readobjdentry.footprint) {
                                if(!allGUIDS.Contains(f)){
                                    allGUIDS.Add(f);
                                }
                            }                                                            
                            decompbr.Dispose();   
                            decomps.Dispose();
                    } else if (indexData[e].compressionType == "0000"){
                        readFile.BaseStream.Position = indexData[e].position;                       
                        int entryEnd = (int)readFile.BaseStream.Position + (int)indexData[e].memSize;
                        LogMessage = string.Format("P{0}/OBJD{1} - Position: {2}", packageparsecount, objdc, indexData[e].position);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        LogMessage = string.Format("P{0}/OBJD{1} - File Size: {2}", packageparsecount, objdc, indexData[e].fileSize);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        LogMessage = string.Format("P{0}/OBJD{1} - Memory Size: {2}", packageparsecount, objdc, indexData[e].memSize);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        LogMessage = string.Format("P{0}/OBJD{1} - Entry Ends At: {2}", packageparsecount, objdc, entryEnd);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        ReadOBJDIndex readOBJD = new ReadOBJDIndex(readFile, packageparsecount, objdc, LogFile);
                        LogMessage = string.Format("P{0}/OBJD{1} - There are {2} entries to read.", packageparsecount, objdc, readOBJD.count);
                        if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        objdentries = new string[readOBJD.count];
                        objdpositions = new int[readOBJD.count];
                        for (int f = 0; f < readOBJD.count; f++){
                            LogMessage = string.Format("P{0}/OBJD{1} - Entry {2}: \n--- Type: {3}\n --- Position: {4}", packageparsecount, objdc, f, readOBJD.entrytype[f], readOBJD.position[f]);
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            objdentries[f] = readOBJD.entrytype[f].ToString();
                            objdpositions[f] = (int)readOBJD.position[f];
                        }
                        readFile.BaseStream.Position = objdpositions[0];
                        ReadOBJDEntry readobjdentry = new ReadOBJDEntry(readFile, objdentries, objdpositions, packageparsecount, objdc, LogFile);
                        thisPackage.Title = readobjdentry.name;
                        thisPackage.Tuning = readobjdentry.tuningname;
                        thisPackage.TuningID = (int)readobjdentry.tuningid;
                        if(!allInstanceIDs.Contains(readobjdentry.instance.ToString("X8"))){
                            allInstanceIDs.Add(readobjdentry.instance.ToString("X8"));
                        }
                        
                        
                        log.MakeLog("Adding components to package: ", true);
                        for (int c = 0; c < readobjdentry.componentcount; c++){
                            LogMessage = readobjdentry.components[c].ToString();
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            LogMessage = readobjdentry.components[c].ToString("X8");
                            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                            thisPackage.Components.Add(readobjdentry.components[c].ToString("X8"));
                        }
                        foreach (string m in readobjdentry.model) {
                            if(!allGUIDS.Contains(m)){
                                allGUIDS.Add(m);
                            }  
                        }
                        foreach (string r in readobjdentry.rig) {
                            if(!allGUIDS.Contains(r)){
                                allGUIDS.Add(r);
                            }  
                        }
                        foreach (string s in readobjdentry.slot) {
                            if(!allGUIDS.Contains(s)){
                                allGUIDS.Add(s);
                            }  
                        }
                        foreach (string f in readobjdentry.footprint) {
                            if(!allGUIDS.Contains(f)){
                                allGUIDS.Add(f);
                            }
                        }
                    }                
                    objdc++;
                }
                
            }

            if (fileHas.Exists(x => x.TypeID == "GEOM")){
                var entryspots = (from has in fileHas
                        where has.TypeID == "GEOM"
                        select has.Location).ToList();                
                
                thisPackage.Mesh = true;

                foreach (int e in entryspots){
                    string key = string.Format("{0}-{1}-{2}", indexData[e].typeID, indexData[e].groupID, indexData[e].instanceID);
                    if (key != "00000000-00000000-0000000000000000"){
                        if (!thisPackage.MeshKeys.Contains(key)){
                            thisPackage.MeshKeys.Add(key);
                        } 
                    }                                       
                }
            }


            LogMessage = string.Format("P{0} - All methods complete, moving on to getting info.", packageparsecount);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

            List<TypeCounter> typecount = new List<TypeCounter>();
            Dictionary<string,int> typeDict = new Dictionary<string, int>();

            LogMessage = string.Format("P{0} - Making dictionary.", packageparsecount);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

            List<typeList> typse = GlobalVariables.S4FunctionTypesConnection.Query<typeList>(string.Format("SELECT * FROM S4Types"));

            foreach (fileHasList item in fileHas){
                foreach (typeList type in typse){
                    if (type.desc == item.TypeID){
                        //LogMessage = string.Format("Added {0} to dictionary.", type.desc);
                        //if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                        //if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                        typeDict.Increment(type.desc);
                    }
                }
            }

            typse = new List<typeList>();

            foreach (KeyValuePair<string, int> type in typeDict){
                TypeCounter tc = new TypeCounter();
                tc.Type = type.Key;
                tc.Count = type.Value;
                LogMessage = string.Format("There are {0} of {1} in this package.", tc.Count, tc.Type);
                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                typecount.Add(tc);
            }
            typeDict = new Dictionary<string, int>();

            thisPackage.Entries.AddRange(typecount);
            thisPackage.FileHas.AddRange(fileHas);

            distinctFlags = allFlags.Distinct().ToList();
            thisPackage.Flags.AddRange(distinctFlags);

            distinctItemTags = itemtags.Distinct().ToList();
            thisPackage.CatalogTags.AddRange(distinctItemTags);

            distinctInstanceIDs = allInstanceIDs.Distinct().ToList();
            thisPackage.InstanceIDs.AddRange(distinctInstanceIDs);

            distinctGUIDS = allGUIDS.Distinct().ToList();
            thisPackage.GUIDs.AddRange(distinctGUIDS);
                     
            LogMessage = string.Format("P{0}: Checking {1} against override IDs.", packageparsecount,thisPackage.PackageName);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            
            LogMessage = string.Format("P{0}: Checking instances.", packageparsecount);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

            var overrides = GlobalVariables.S4OverridesList.Where(p => thisPackage.InstanceIDs.Any(l => p.InstanceID == l)).ToList();

            if(overrides.Count > 0){
                LogMessage = string.Format("P{0}: Found {1} overrides!", packageparsecount, overrides.Count);
                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                thisPackage.Override = true;
                thisPackage.Type = "OVERRIDE";
            }           

            var specoverrides = GlobalVariables.S4SpecificOverridesList.Where(p => overrides.Any(l => p.Instance == l.InstanceID)).ToList();

            List<SpecificOverrides> speco = new List<SpecificOverrides>(specoverrides.Count);

            List<string> InstanceOverrides = new List<string>(overrides.Count);
            List<string> ItemOverrides = new List<string>(overrides.Count);
            foreach (OverridesList ov in overrides) {
                SpecificOverrides sspo = new SpecificOverrides();
                if (ov.InstanceID != "0000000000000000"){
                    var specco = GlobalVariables.S4SpecificOverridesList.Where(p => p.Instance == ov.InstanceID).ToList();                    
                    InstanceOverrides.Add(ov.InstanceID);
                    ItemOverrides.Add(ov.Name);
                    sspo.Instance = ov.InstanceID;
                    if (specco.Any()){
                        sspo.Description = specco[0].Description;
                        thisPackage.Type = string.Format("OVERRIDE: {0}", sspo.Description);
                    }
                }
            }

            if (InstanceOverrides.Any()){
                thisPackage.OverriddenInstances.AddRange(InstanceOverrides);
            }
            if (ItemOverrides.Any()){
                thisPackage.OverriddenInstances.AddRange(ItemOverrides);
            }
            InstanceOverrides = new List<string>(0);
            ItemOverrides = new List<string>(0);
            
            int casp;int geom;int rle;int rmap;int thum;int img;int xml;int clhd;int clip;int stbl;int cobj;int ftpt;int lite;int thm;int mlod;int modl;int mtbl;int objd;int rslt;int tmlt;int ssm;int lrle;int bond;int cpre;int dmap;int smod;int bgeo;int hotc;

            if (thisPackage.Override != true){
                log.MakeLog("No overrides were found. Checking other options.", true);
                if ((typeDict.TryGetValue("S4SM", out ssm) && ssm >= 1)){
                thisPackage.Type = "Merged Package";
                LogMessage = string.Format("This is a {0}!!", thisPackage.Type);
                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else if (!String.IsNullOrWhiteSpace(thisPackage.Function)) {
                    thisPackage.Type = thisPackage.Function;
                } else if (!String.IsNullOrWhiteSpace(thisPackage.Tuning)) {
                    if (thisPackage.Tuning == "object_bassinetGEN_Empty_01"){
                        thisPackage.Type = "Bassinet";
                    }
                } else if ((typeDict.TryGetValue("BGEO", out bgeo) && bgeo >= 1) && (typeDict.TryGetValue("HOTC", out hotc) && hotc >= 1) && (typeDict.TryGetValue("SMOD", out smod) && smod >= 1)){
                    thisPackage.Type = "Slider";
                    LogMessage = string.Format("This is a {0}!!", thisPackage.Type);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else if ((typeDict.TryGetValue("BOND", out bond) && bond >= 1) && (typeDict.TryGetValue("CPRE", out cpre) && cpre >= 1) && (typeDict.TryGetValue("DMAP", out dmap) && dmap >= 1) && (typeDict.TryGetValue("SMOD", out smod) && smod >= 1)){
                    thisPackage.Type = "Preset";
                    LogMessage = string.Format("This is a {0}!!", thisPackage.Type);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else if ((typeDict.TryGetValue("RLE2", out rle) && rle >= 1) && (typeDict.TryGetValue("CASP", out casp) && casp >= 1) && (typeDict.TryGetValue("GEOM", out geom) && geom <= 0)){
                    thisPackage.Type = "CAS Recolor";
                    LogMessage = string.Format("This is a {0}!!", thisPackage.Type);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else if ((typeDict.TryGetValue("LRLE", out lrle) && lrle >= 1) && (typeDict.TryGetValue("CASP", out casp) && casp >= 1) && (typeDict.TryGetValue("GEOM", out geom) && geom <= 0)){
                    thisPackage.Type = "Makeup";
                    LogMessage = string.Format("This is a {0}!!", thisPackage.Type);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else if ((typeDict.TryGetValue("CASP", out casp) && casp <= 0) && (typeDict.TryGetValue("GEOM", out geom) && geom >= 1) && (typeDict.TryGetValue("RLE2", out rle) && rle >= 1) && (typeDict.TryGetValue("RMAP", out rmap) && rmap >= 1) && (typeDict.TryGetValue("_IMG", out img) && img <= 0)){
                    thisPackage.Type = "Hair Mesh";
                    thisPackage.Mesh = true;
                    thisPackage.Recolor = false;
                    LogMessage = string.Format("This is a {0}!!", thisPackage.Type);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else if ((typeDict.TryGetValue("CASP", out casp) && casp >= 1) && (typeDict.TryGetValue("GEOM", out geom) && geom <= 0) && (typeDict.TryGetValue("RLE2", out rle) && rle >= 1) && (typeDict.TryGetValue("RMAP", out rmap) && rmap >= 1) && (typeDict.TryGetValue("_IMG", out img) && img >= 1)){
                    thisPackage.Type = "Hair Recolor";
                    thisPackage.Mesh = false;
                    thisPackage.Recolor = true;
                    LogMessage = string.Format("This is a {0}!!", thisPackage.Type);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else if ((typeDict.TryGetValue("CASP", out casp) && casp >= 1) && (typeDict.TryGetValue("GEOM", out geom) && geom >= 1) && (typeDict.TryGetValue("RLE2", out rle) && rle >= 1) && (typeDict.TryGetValue("RMAP", out rmap) && rmap >= 1) && (typeDict.TryGetValue("_IMG", out img) && img >= 1)){
                    thisPackage.Type = "Hair";
                    thisPackage.Mesh = true;
                    thisPackage.Recolor = true;
                    LogMessage = string.Format("This is a {0}!!", thisPackage.Type);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else if ((typeDict.TryGetValue("_IMG", out img) && img >= 1) && (typeDict.TryGetValue("CLHD", out clhd) && clhd >= 1) && (typeDict.TryGetValue("STBL", out stbl) && stbl >= 1)){
                    thisPackage.Type = "Pose Pack";
                    LogMessage = string.Format("This is a {0}!!", thisPackage.Type);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else if ((typeDict.TryGetValue("_IMG", out img) && img >= 1) && (typeDict.TryGetValue("COBJ", out cobj) && cobj >= 1) && (typeDict.TryGetValue("FTPT", out ftpt) && ftpt >= 1) && (typeDict.TryGetValue("MLOD", out mlod) && mlod >= 1) && (typeDict.TryGetValue("MODL", out modl) && modl >= 1) && (typeDict.TryGetValue("MTBL", out mtbl) && mtbl <= 0)  && (typeDict.TryGetValue("_IMG", out img) && img <= 0) && (typeDict.TryGetValue("OBJD", out objd) && objd >= 1)){
                    thisPackage.Type = "Object Mesh";
                    thisPackage.Mesh = true;
                    thisPackage.Recolor = false;
                    LogMessage = string.Format("This is a {0}!!", thisPackage.Type);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else if ((typeDict.TryGetValue("_IMG", out img) && img >= 1) && (typeDict.TryGetValue("COBJ", out cobj) && cobj >= 1) && (typeDict.TryGetValue("FTPT", out ftpt) && ftpt >= 1) && (typeDict.TryGetValue("MLOD", out mlod) && mlod >= 1) && (typeDict.TryGetValue("MODL", out modl) && modl >= 1) && (typeDict.TryGetValue("MTBL", out mtbl) && mtbl >= 1) && (typeDict.TryGetValue("OBJD", out objd) && objd <= 0)){
                    thisPackage.Type = "Object Recolor";
                    thisPackage.Mesh = false;
                    thisPackage.Recolor = true;
                    LogMessage = string.Format("This is a {0}!!", thisPackage.Type);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else if ((typeDict.TryGetValue("_IMG", out img) && img >= 1) && (typeDict.TryGetValue("COBJ", out cobj) && cobj >= 1) && (typeDict.TryGetValue("FTPT", out ftpt) && ftpt >= 1) && (typeDict.TryGetValue("MLOD", out mlod) && mlod >= 1) && (typeDict.TryGetValue("MODL", out modl) && modl >= 1) && (typeDict.TryGetValue("MTBL", out mtbl) && mtbl >= 1) && (typeDict.TryGetValue("OBJD", out objd) && objd >= 1)){
                    thisPackage.Type = "Object";
                    thisPackage.Mesh = true;
                    thisPackage.Recolor = true;
                    LogMessage = string.Format("This is a {0}!!", thisPackage.Type);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else if ((typeDict.TryGetValue("RLE2", out rle) && rle >= 1) && (typeDict.TryGetValue("CASP", out casp) && casp >= 1) && (typeDict.TryGetValue("GEOM", out geom) && geom >= 1)){
                    thisPackage.Type = "CAS Part";
                    LogMessage = string.Format("This is a {0}!!", thisPackage.Type);
                    if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                    if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
                } else {
                    log.MakeLog("Unable to identify package.", true);
                    thisPackage.Type = "UNKNOWN";
                }
            }

            if (thisPackage.AgeGenderFlags.Any()){
                if ((thisPackage.AgeGenderFlags.Female == true) && (thisPackage.AgeGenderFlags.Male == true)){
                    thisPackage.Gender = "Both";
                } else if (thisPackage.AgeGenderFlags.Female == true){
                    thisPackage.Gender = "Female";
                } else if (thisPackage.AgeGenderFlags.Male == true){
                    thisPackage.Gender = "Male";
                }


                string age = string.Empty;
                if (thisPackage.AgeGenderFlags.Adult == true){
                    if (string.IsNullOrEmpty(age)){
                        age += "Adult";
                    } else {
                        age += string.Format(", Adult");
                    }
                } else if (thisPackage.AgeGenderFlags.Baby == true){
                    if (string.IsNullOrEmpty(age)){
                        age += "Baby";
                    } else {
                        age += string.Format(", Baby");
                    }
                } else if (thisPackage.AgeGenderFlags.Child == true){
                    if (string.IsNullOrEmpty(age)){
                        age += "Child";
                    } else {
                        age += string.Format(", Child");
                    }
                } else if (thisPackage.AgeGenderFlags.Elder == true){
                    if (string.IsNullOrEmpty(age)){
                        age += "Elder";
                    } else {
                        age += string.Format(", Elder");
                    }
                } else if (thisPackage.AgeGenderFlags.Infant == true){
                    if (string.IsNullOrEmpty(age)){
                        age += "Infant";
                    } else {
                        age += string.Format(", Infant");
                    }
                } else if (thisPackage.AgeGenderFlags.Teen == true){
                    if (string.IsNullOrEmpty(age)){
                        age += "Teen";
                    } else {
                        age += string.Format(", Teen");
                    }
                } else if (thisPackage.AgeGenderFlags.Toddler == true){
                    if (string.IsNullOrEmpty(age)){
                        age += "Toddler";
                    } else {
                        age += string.Format(", Toddler");
                    }
                } else if (thisPackage.AgeGenderFlags.YoungAdult == true){
                    if (string.IsNullOrEmpty(age)){
                        age += "Young Adult";
                    } else {
                        age += string.Format(", Young Adult");
                    }
                }
                thisPackage.Age = age;           
            }
            dbpfFile.Close();
            dbpfFile.Dispose();
            readFile.Close();
            readFile.Dispose();

            
            if (thisPackage.Mesh == false && thisPackage.Recolor == true){
                thisPackage.Orphan = true;  
            } else if (thisPackage.Mesh == true && thisPackage.Recolor == false && thisPackage.Override == false){
                thisPackage.Orphan = true;
            }
            
            /*try {
                GlobalVariables.DatabaseConnection.InsertWithChildren(thisPackage, true);
            } catch (SQLiteException ex){
                LogMessage = string.Format("Caught exception adding Sims 4 package to database. \n Exception: {0}", ex);
                if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
                if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            }
            LogMessage = string.Format("Added {0} to packages database successfully.", thisPackage.PackageName);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            txt = string.Format("SELECT * FROM Processing_Reader where Name='{0}'", Methods.FixApostrophesforSQL(packageinfo.Name));
            List<PackageFile> closingquery = GlobalVariables.DatabaseConnection.Query<PackageFile>(txt);            
            GlobalVariables.DatabaseConnection.Delete(closingquery[0]);
            closingquery = new List<PackageFile>();*/
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            string elapsedtime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                                ts.Hours, ts.Minutes, ts.Seconds,
                                ts.Milliseconds / 10);
            GlobalVariables.currentpackage = packageinfo.Name;
            GlobalVariables.packagesRead++;


            thisPackage = findOrphans.FindMatchesS4(thisPackage);
            if (GlobalVariables.sortonthego == true){
                thisPackage = filesort.SortPackage(thisPackage);
            }
            GlobalVariables.AddPackages.Enqueue(thisPackage);
            PackageFile packageFile = new PackageFile(){Name = thisPackage.PackageName, Location = thisPackage.Location, Game = thisPackage.Game};
            GlobalVariables.RemovePackages.Enqueue(packageFile);
            log.MakeLog(string.Format("Package Summary: {0}", thisPackage.SimsPackagetoString()), false);
            LogMessage = string.Format("Package Summary: {0}", thisPackage.SimsPackagetoString());
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            log.MakeLog(string.Format("Package Summary: {0}", thisPackage.SimsPackagetoString()), false);
            LogMessage = string.Format("Adding {0} to packages database.", thisPackage.PackageName);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));

            LogMessage = string.Format("Reading file {0} took {1}", thisPackage.PackageName, elapsedtime);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            log.MakeLog(string.Format("Reading file {0} took {1}", thisPackage.PackageName, elapsedtime), false);           

            thisPackage = new();
            LogMessage = string.Format("Closing package # {0}/{1}: {2}", packageparsecount, GlobalVariables.PackageCount, packageinfo.Name);
            if(GlobalVariables.highdebug == true) log.MakeLog(LogMessage, true);
            if(GlobalVariables.highdebug == false) LogFile.Append(string.Format("{0}\n", LogMessage));
            if(GlobalVariables.highdebug == false) log.MakeLog(string.Format("Log Dumped from StringBuilder: \n {0}", LogFile.ToString()), true);
            return;
        }

        public void FindS4ConflictsAndMatches(SimsPackage package){
            
        }

        public static String hexToASCII(String hex)
        {
            // initialize the ASCII code string as empty.
            String ascii = "";
    
            for (int i = 0; i < hex.Length; i += 2)
            {
    
                // extract two characters from hex string
                String part = hex.Substring(i, 2);
    
                // change it into base 16 and
                // typecast as the character
                char ch = (char)Convert.ToInt32(part, 16);;
    
                // add this char to final ASCII string
                ascii = ascii + ch;
            }
            return ascii;
        } 

        public void RetunePackage(SimsPackage package, string tuning){
            //add otg tags OR add specific tuning passed in. for example, cars for transport mod
        }
    }
}