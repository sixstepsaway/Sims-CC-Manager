using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using MoreLinq.Extensions;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.OptionLists;

namespace SimsCCManager.PackageReaders
{
    public class Sims2PackageStatics
    {
        public static List<EntryType> Sims2EntryTypes = new()
        {
            new EntryType(){ Tag = "2ARY", TypeID = "6B943B43", Description = "2D Array" },
            new EntryType(){ Tag = "3ARY", TypeID = "2A51171B", Description = "3D Array" },
            new EntryType(){ Tag = "5DS", TypeID = "AC06A676", Description = "Lighting (Draw State Light)" },
            new EntryType(){ Tag = "5EL", TypeID = "6A97042F", Description = "Lighting (Environment Cube Light)" },
            new EntryType(){ Tag = "5LF", TypeID = "AC06A66F", Description = "Lighting (Linear Fog Light)" },
            new EntryType(){ Tag = "5SC", TypeID = "25232B11", Description = "Scene Node" },
            new EntryType(){ Tag = "ANIM", TypeID = "FB00791E", Description = "Animation Resource" },
            new EntryType(){ Tag = "BCON", TypeID = "42434F4E", Description = "Behaviour Constant" },
            new EntryType(){ Tag = "BHAV", TypeID = "42484156", Description = "Behaviour Function" },
            new EntryType(){ Tag = "BINX", TypeID = "0C560F39", Description = "Binary Index" },
            new EntryType(){ Tag = "BMP", TypeID = "424D505F", Description = "Bitmaps" },
            new EntryType(){ Tag = "BMP", TypeID = "856DDBAC", Description = "Bitmaps" },
            new EntryType(){ Tag = "CATS", TypeID = "43415453", Description = "Catalog String" },
            new EntryType(){ Tag = "CIGE", TypeID = "43494745", Description = "Image Link" },
            new EntryType(){ Tag = "CINE", TypeID = "4D51F042", Description = "Cinematic Scenes" },
            new EntryType(){ Tag = "CREG", TypeID = "CDB467B8", Description = "Content Registry" },
            new EntryType(){ Tag = "CRES", TypeID = "E519C933", Description = "Resource Node" },
            new EntryType(){ Tag = "CTSS", TypeID = "43545353", Description = "Catalog Description" },
            new EntryType(){ Tag = "DGRP", TypeID = "44475250", Description = "Drawgroup" },
            new EntryType(){ Tag = "DIR", TypeID = "E86B1EEF", Description = "Directory of Compressed Files" },
            new EntryType(){ Tag = "FACE", TypeID = "46414345", Description = "Face Properties" },
            new EntryType(){ Tag = "FAMh", TypeID = "46414D68", Description = "Family Data" },
            new EntryType(){ Tag = "FAMI", TypeID = "46414D49", Description = "Family Information" },
            new EntryType(){ Tag = "FAMt", TypeID = "8C870743", Description = "Family Ties" },
            new EntryType(){ Tag = "FCNS", TypeID = "46434E53", Description = "Global Tuning Values" },
            new EntryType(){ Tag = "FPL", TypeID = "AB4BA572", Description = "Fence Post Layer" },
            new EntryType(){ Tag = "FWAV", TypeID = "46574156", Description = "Audio Reference" },
            new EntryType(){ Tag = "FX", TypeID = "EA5118B0", Description = "Effects Resource Tree" },
            new EntryType(){ Tag = "GLOB", TypeID = "474C4F42", Description = "Glabal Data" },
            new EntryType(){ Tag = "GMDC", TypeID = "AC4F8687", Description = "Geometric Data Container" },
            new EntryType(){ Tag = "GMND", TypeID = "7BA3838C", Description = "Geometric Node" },
            new EntryType(){ Tag = "GZPS", TypeID = "EBCF3E27", Description = "Property Set" },
            new EntryType(){ Tag = "HLS", TypeID = "7B1ACFCD", Description = "Hitlist (TS2 format)" },
            new EntryType(){ Tag = "HOUS", TypeID = "484F5553", Description = "House Data" },
            new EntryType(){ Tag = "JFIF", TypeID = "4D533EDD", Description = "JPEG/JFIF Image" },
            new EntryType(){ Tag = "JFIF", TypeID = "856DDBAC", Description = "JPEG/JFIF Image" },
            new EntryType(){ Tag = "JFIF", TypeID = "8C3CE95A", Description = "JPEG/JFIF Image" },
            new EntryType(){ Tag = "JFIF", TypeID = "0C7E9A76", Description = "JPEG/JFIF Image" },
            new EntryType(){ Tag = "LDEF", TypeID = "0BF999E7", Description = "Lot or Tutorial Description" },
            new EntryType(){ Tag = "LGHT", TypeID = "C9C81B9B", Description = "Lighting (Ambient Light)" },
            new EntryType(){ Tag = "LGHT", TypeID = "C9C81BA3", Description = "Lighting (Directional Light)" },
            new EntryType(){ Tag = "LGHT", TypeID = "C9C81BA9", Description = "Lighting (Point Light)" },
            new EntryType(){ Tag = "LGHT", TypeID = "C9C81BAD", Description = "Lighting (Spot Light)" },
            new EntryType(){ Tag = "LIFO", TypeID = "ED534136", Description = "Level Information" },
            new EntryType(){ Tag = "LOT", TypeID = "6C589723", Description = "Lot Definition" },
            new EntryType(){ Tag = "LTTX", TypeID = "4B58975B", Description = "Lot Texture" },
            new EntryType(){ Tag = "LxNR", TypeID = "CCCEF852", Description = "Facial Structure" },
            new EntryType(){ Tag = "MATSHAD", TypeID = "CD7FE87A", Description = "Maxis Material Shader" },
            new EntryType(){ Tag = "MMAT", TypeID = "4C697E5A", Description = "Material Override" },
            new EntryType(){ Tag = "MOBJT", TypeID = "6F626A74", Description = "Main Lot Objects" },
            new EntryType(){ Tag = "MP3", TypeID = "2026960B", Description = "MP3 Audio" },
            new EntryType(){ Tag = "NGBH", TypeID = "4E474248", Description = "Neighborhood Data" },
            new EntryType(){ Tag = "NHTG", TypeID = "ABCB5DA4", Description = "Neighbourhood Terrain Geometry" },
            new EntryType(){ Tag = "NHTR", TypeID = "ABD0DC63", Description = "Neighborhood Terrain" },
            new EntryType(){ Tag = "NHVW", TypeID = "EC44BDDC", Description = "Neighborhood View" },
            new EntryType(){ Tag = "NID", TypeID = "AC8A7A2E", Description = "Neighbourhood ID" },
            new EntryType(){ Tag = "NMAP", TypeID = "4E6D6150", Description = "Name Map" },
            new EntryType(){ Tag = "NREF", TypeID = "4E524546", Description = "Name Reference" },
            new EntryType(){ Tag = "OBJD", TypeID = "4F424A44", Description = "Object Data" },
            new EntryType(){ Tag = "OBJf", TypeID = "4F424A66", Description = "Object Functions" },
            new EntryType(){ Tag = "ObJM", TypeID = "4F626A4D", Description = "Object Metadata" },
            new EntryType(){ Tag = "OBJT", TypeID = "FA1C39F7", Description = "Singular Lot Object" },
            new EntryType(){ Tag = "OBMI", TypeID = "4F626A4D", Description = "Object Metadata Imposter" },
            new EntryType(){ Tag = "PALT", TypeID = "50414C54", Description = "Image Color Palette" },
            new EntryType(){ Tag = "PDAT", TypeID = "AACE2EFB", Description = "Person Data (Formerly SDSC/SINF/SDAT)" },
            new EntryType(){ Tag = "PERS", TypeID = "50455253", Description = "Person Status" },
            new EntryType(){ Tag = "PMAP", TypeID = "8CC0A14B", Description = "Predictive Map" },
            new EntryType(){ Tag = "PNG", TypeID = "856DDBAC", Description = "PNG Image" },
            new EntryType(){ Tag = "POOL", TypeID = "0C900FDB", Description = "Pool Surface" },
            new EntryType(){ Tag = "Popups", TypeID = "2C310F46", Description = "Unknown" },
            new EntryType(){ Tag = "POSI", TypeID = "504F5349", Description = "Edith Positional Information (deprecated)" },
            new EntryType(){ Tag = "XFLR", TypeID = "4DCADB7E", Description = "Terrain Texture" },
            new EntryType(){ Tag = "PTBP", TypeID = "50544250", Description = "Package Toolkit" },
            new EntryType(){ Tag = "ROOF", TypeID = "AB9406AA", Description = "Roof" },
            new EntryType(){ Tag = "SFX", TypeID = "8DB5E4C2", Description = "Sound Effects" },
            new EntryType(){ Tag = "SHPE", TypeID = "FC6EB1F7", Description = "Shape" },
            new EntryType(){ Tag = "SIMI", TypeID = "53494D49", Description = "Sim Information" },
            new EntryType(){ Tag = "SKIN", TypeID = "AC506764", Description = "Sim Outfits" },
            new EntryType(){ Tag = "SLOT", TypeID = "534C4F54", Description = "Object Slot" },
            new EntryType(){ Tag = "SMAP", TypeID = "CAC4FC40", Description = "String Map" },
            new EntryType(){ Tag = "SPR2", TypeID = "53505232", Description = "Sprites" },
            new EntryType(){ Tag = "SPX1", TypeID = "2026960B", Description = "SPX Speech" },
            new EntryType(){ Tag = "SREL", TypeID = "CC364C2A", Description = "Sim Relations" },
            new EntryType(){ Tag = "STR#", TypeID = "53545223", Description = "Text String" },
            new EntryType(){ Tag = "STXR", TypeID = "ACE46235", Description = "Surface Texture" },
            new EntryType(){ Tag = "SWAF", TypeID = "CD95548E", Description = "Sim Wants and Fears" },
            new EntryType(){ Tag = "TATT", TypeID = "54415454", Description = "Tree Attributes" },
            new EntryType(){ Tag = "TGA", TypeID = "856DDBAC", Description = "Targa Image" },
            new EntryType(){ Tag = "TMAP", TypeID = "4B58975B", Description = "Lot or Terrain Texture Map" },
            new EntryType(){ Tag = "TPRP", TypeID = "54505250", Description = "Edith SimAntics Behavior Labels" },
            new EntryType(){ Tag = "TRCN", TypeID = "5452434E", Description = "Behavior Constant Labels" },
            new EntryType(){ Tag = "TREE", TypeID = "54524545", Description = "Tree Data" },
            new EntryType(){ Tag = "TSSG", TypeID = "BA353CE1", Description = "The Sims SG System" },
            new EntryType(){ Tag = "TTAB", TypeID = "54544142", Description = "Pie Menu Functions" },
            new EntryType(){ Tag = "TTAs", TypeID = "54544173", Description = "Pie Menu Strings" },
            new EntryType(){ Tag = "TXMT", TypeID = "49596978", Description = "Material Definitions" },
            new EntryType(){ Tag = "TXTR", TypeID = "1C4A276C", Description = "Texture" },
            new EntryType(){ Tag = "UI", TypeID = "00000000", Description = "User Interface" },
            new EntryType(){ Tag = "VERT", TypeID = "CB4387A1", Description = "Vertex Layer" },
            new EntryType(){ Tag = "WFR", TypeID = "CD95548E", Description = "Wants and Fears" },
            new EntryType(){ Tag = "WGRA", TypeID = "0A284D0B", Description = "Wall Graph" },
            new EntryType(){ Tag = "WLL", TypeID = "8A84D7B0", Description = "Wall Layer" },
            new EntryType(){ Tag = "WRLD", TypeID = "49FF7D76", Description = "World Database" },
            new EntryType(){ Tag = "WTHR", TypeID = "B21BE28B", Description = "Weather Info" },
            new EntryType(){ Tag = "XA", TypeID = "2026960B", Description = "XA Audio" },
            new EntryType(){ Tag = "XHTN", TypeID = "8C1580B5", Description = "Hairtone XML" },
            new EntryType(){ Tag = "XMTO", TypeID = "584D544F", Description = "Material Object Class Dump" },
            new EntryType(){ Tag = "XOBJ", TypeID = "CCA8E925", Description = "Object Class Dump" },
            new EntryType(){ Tag = "XTOL", TypeID = "2C1FD8A1", Description = "Texture Overlay XML" },
            new EntryType(){ Tag = "UNK", TypeID = "0F9F0C21", Description = "Unknown (from Nightlife)" },
            new EntryType(){ Tag = "UNK", TypeID = "8B0C79D6", Description = "Unknown" },
            new EntryType(){ Tag = "UNK", TypeID = "9D796DB4", Description = "Unknown" },
            new EntryType(){ Tag = "UNK", TypeID = "CC2A6A34", Description = "Unknown" },
            new EntryType(){ Tag = "UNK", TypeID = "CC8A6A69", Description = "Unknown" },
            new EntryType(){ Tag = "COLL", TypeID = "6C4F359D", Description = "Collection" }
        };

        public static List<FunctionSortList> Sims2BuyFunctionSortList = new(){
                        //seating   
            new FunctionSortList(){FlagNum = 0, FunctionSubsortNum = 1, Category = "Seating", Subcategory = "Dining Room"},
            new FunctionSortList(){FlagNum = 0, FunctionSubsortNum = 2, Category = "Seating", Subcategory = "Living Room"},
            new FunctionSortList(){FlagNum = 0, FunctionSubsortNum = 4, Category = "Seating", Subcategory = "Sofas"},
            new FunctionSortList(){FlagNum = 0, FunctionSubsortNum = 8, Category = "Seating", Subcategory = "Beds"},
            new FunctionSortList(){FlagNum = 0, FunctionSubsortNum = 16, Category = "Seating", Subcategory = "Recreation"},
            new FunctionSortList(){FlagNum = 0, FunctionSubsortNum = 32, Category = "Seating", Subcategory = "Unknown I"},
            new FunctionSortList(){FlagNum = 0, FunctionSubsortNum = 64, Category = "Seating", Subcategory = "Unknown II"},
            new FunctionSortList(){FlagNum = 0, FunctionSubsortNum = 128, Category = "Seating", Subcategory = "Misc"},            
                        //surfaces
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 1, Category = "Surfaces", Subcategory = "Counters"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 2, Category = "Surfaces", Subcategory = "Tables"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 4, Category = "Surfaces", Subcategory = "End Tables"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 8, Category = "Surfaces", Subcategory = "Desks"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 16, Category = "Surfaces", Subcategory = "Coffee Tables"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 32, Category = "Surfaces", Subcategory = "Shelves"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 64, Category = "Surfaces", Subcategory = "Unknown II"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 128, Category = "Surfaces", Subcategory = "Misc"},            
                        //Appliances
            new FunctionSortList(){FlagNum = 2, FunctionSubsortNum = 1, Category = "Appliances", Subcategory = "Cooking"},
            new FunctionSortList(){FlagNum = 2, FunctionSubsortNum = 2, Category = "Appliances", Subcategory = "Fridges"},
            new FunctionSortList(){FlagNum = 2, FunctionSubsortNum = 4, Category = "Appliances", Subcategory = "Small"},
            new FunctionSortList(){FlagNum = 2, FunctionSubsortNum = 8, Category = "Appliances", Subcategory = "Large"},
            new FunctionSortList(){FlagNum = 2, FunctionSubsortNum = 16, Category = "Appliances", Subcategory = "Unknown I"},
            new FunctionSortList(){FlagNum = 2, FunctionSubsortNum = 32, Category = "Appliances", Subcategory = "Unknown II"},
            new FunctionSortList(){FlagNum = 2, FunctionSubsortNum = 64, Category = "Appliances", Subcategory = "Unknown III"},
            new FunctionSortList(){FlagNum = 2, FunctionSubsortNum = 128, Category = "Appliances", Subcategory = "Misc"},
                        //Electronics
            new FunctionSortList(){FlagNum = 3, FunctionSubsortNum = 1, Category = "Electronics", Subcategory = "Entertainment"},
            new FunctionSortList(){FlagNum = 3, FunctionSubsortNum = 2, Category = "Electronics", Subcategory = "TV/Computer"},
            new FunctionSortList(){FlagNum = 3, FunctionSubsortNum = 4, Category = "Electronics", Subcategory = "Audio"},
            new FunctionSortList(){FlagNum = 3, FunctionSubsortNum = 8, Category = "Electronics", Subcategory = "Small"},
            new FunctionSortList(){FlagNum = 3, FunctionSubsortNum = 16, Category = "Electronics", Subcategory = "Unknown I"},
            new FunctionSortList(){FlagNum = 3, FunctionSubsortNum = 32, Category = "Electronics", Subcategory = "Unknown II"},
            new FunctionSortList(){FlagNum = 3, FunctionSubsortNum = 64, Category = "Electronics", Subcategory = "Unknown III"},
            new FunctionSortList(){FlagNum = 3, FunctionSubsortNum = 128, Category = "Electronics", Subcategory = "Misc"},
                        //Plumbing
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 1, Category = "Plumbing", Subcategory = "Toilets"},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 2, Category = "Plumbing", Subcategory = "Showers"},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 4, Category = "Plumbing", Subcategory = "Sinks"},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 8, Category = "Plumbing", Subcategory = "Hot Tubs"},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 16, Category = "Plumbing", Subcategory = "Unknown I"},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 32, Category = "Plumbing", Subcategory = "Unknown II"},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 64, Category = "Plumbing", Subcategory = "Unknown III"},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 128, Category = "Plumbing", Subcategory = "Misc"},
                        //Decorative
            new FunctionSortList(){FlagNum = 5, FunctionSubsortNum = 1, Category = "Decorative", Subcategory = "Wall Decorations"},
            new FunctionSortList(){FlagNum = 5, FunctionSubsortNum = 2, Category = "Decorative", Subcategory = "Sculptures"},
            new FunctionSortList(){FlagNum = 5, FunctionSubsortNum = 4, Category = "Decorative", Subcategory = "Rugs"},
            new FunctionSortList(){FlagNum = 5, FunctionSubsortNum = 8, Category = "Decorative", Subcategory = "Plants"},
            new FunctionSortList(){FlagNum = 5, FunctionSubsortNum = 16, Category = "Decorative", Subcategory = "Mirrors"},
            new FunctionSortList(){FlagNum = 5, FunctionSubsortNum = 32, Category = "Decorative", Subcategory = "Curtains"},
            new FunctionSortList(){FlagNum = 5, FunctionSubsortNum = 64, Category = "Decorative", Subcategory = "Unknown II"},
            new FunctionSortList(){FlagNum = 5, FunctionSubsortNum = 128, Category = "Decorative", Subcategory = "Misc"},
                        //General
            new FunctionSortList(){FlagNum = 6, FunctionSubsortNum = 1, Category = "Misc", Subcategory = "Unknown I"},
            new FunctionSortList(){FlagNum = 6, FunctionSubsortNum = 2, Category = "Misc", Subcategory = "Dressers"},
            new FunctionSortList(){FlagNum = 6, FunctionSubsortNum = 4, Category = "Misc", Subcategory = "Unknown II"},
            new FunctionSortList(){FlagNum = 6, FunctionSubsortNum = 8, Category = "Misc", Subcategory = "Party"},
            new FunctionSortList(){FlagNum = 6, FunctionSubsortNum = 16, Category = "Misc", Subcategory = "Child"},
            new FunctionSortList(){FlagNum = 6, FunctionSubsortNum = 32, Category = "Misc", Subcategory = "Cars"},
            new FunctionSortList(){FlagNum = 6, FunctionSubsortNum = 64, Category = "Misc", Subcategory = "Pets"},
            new FunctionSortList(){FlagNum = 6, FunctionSubsortNum = 128, Category = "Misc", Subcategory = "Misc"},
                        //Lighting
            new FunctionSortList(){FlagNum = 7, FunctionSubsortNum = 1, Category = "Lighting", Subcategory = "Table Lamps"},
            new FunctionSortList(){FlagNum = 7, FunctionSubsortNum = 2, Category = "Lighting", Subcategory = "Floor Lamps"},
            new FunctionSortList(){FlagNum = 7, FunctionSubsortNum = 4, Category = "Lighting", Subcategory = "Wall Lamps"},
            new FunctionSortList(){FlagNum = 7, FunctionSubsortNum = 8, Category = "Lighting", Subcategory = "Ceiling Lamps"},
            new FunctionSortList(){FlagNum = 7, FunctionSubsortNum = 16, Category = "Lighting", Subcategory = "Outdoor"},
            new FunctionSortList(){FlagNum = 7, FunctionSubsortNum = 32, Category = "Lighting", Subcategory = "Unknown I"},
            new FunctionSortList(){FlagNum = 7, FunctionSubsortNum = 64, Category = "Lighting", Subcategory = "Unknown II"},
            new FunctionSortList(){FlagNum = 7, FunctionSubsortNum = 128, Category = "Lighting", Subcategory = "Misc"},
                        //Hobbies
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 1, Category = "Hobbies", Subcategory = "Creative"},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 2, Category = "Hobbies", Subcategory = "Knowledge"},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 4, Category = "Hobbies", Subcategory = "Exercise"},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 8, Category = "Hobbies", Subcategory = "Recreation"},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 16, Category = "Hobbies", Subcategory = "Unknown I"},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 32, Category = "Hobbies", Subcategory = "Unknown II"},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 64, Category = "Hobbies", Subcategory = "Unknown III"},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 128, Category = "Hobbies", Subcategory = "Misc"},
                        //Aspiration Rewards
            new FunctionSortList(){FlagNum = 9, FunctionSubsortNum = 1, Category = "Aspiration Rewards", Subcategory = "Unknown I"},
            new FunctionSortList(){FlagNum = 9, FunctionSubsortNum = 2, Category = "Aspiration Rewards", Subcategory = "Unknown II"},
            new FunctionSortList(){FlagNum = 9, FunctionSubsortNum = 4, Category = "Aspiration Rewards", Subcategory = "Unknown III"},
            new FunctionSortList(){FlagNum = 9, FunctionSubsortNum = 8, Category = "Aspiration Rewards", Subcategory = "Unknown IV"},
            new FunctionSortList(){FlagNum = 9, FunctionSubsortNum = 16, Category = "Aspiration Rewards", Subcategory = "Unknown V"},
            new FunctionSortList(){FlagNum = 9, FunctionSubsortNum = 32, Category = "Aspiration Rewards", Subcategory = "Unknown VI"},
            new FunctionSortList(){FlagNum = 9, FunctionSubsortNum = 64, Category = "Aspiration Rewards", Subcategory = "Unknown VII"},
            new FunctionSortList(){FlagNum = 9, FunctionSubsortNum = 128, Category = "Aspiration Rewards", Subcategory = "Unknown VIII"},
                        //Career Rewards
            new FunctionSortList(){FlagNum = 10, FunctionSubsortNum = 1, Category = "Career Rewards", Subcategory = "Unknown I"},
            new FunctionSortList(){FlagNum = 10, FunctionSubsortNum = 2, Category = "Career Rewards", Subcategory = "Unknown II"},
            new FunctionSortList(){FlagNum = 10, FunctionSubsortNum = 4, Category = "Career Rewards", Subcategory = "Unknown III"},
            new FunctionSortList(){FlagNum = 10, FunctionSubsortNum = 8, Category = "Career Rewards", Subcategory = "Unknown IV"},
            new FunctionSortList(){FlagNum = 10, FunctionSubsortNum = 16, Category = "Career Rewards", Subcategory = "Unknown V"},
            new FunctionSortList(){FlagNum = 10, FunctionSubsortNum = 32, Category = "Career Rewards", Subcategory = "Unknown VI"},
            new FunctionSortList(){FlagNum = 10, FunctionSubsortNum = 64, Category = "Career Rewards", Subcategory = "Unknown VII"},
            new FunctionSortList(){FlagNum = 10, FunctionSubsortNum = 128, Category = "Career Rewards", Subcategory = "Unknown VIII"}
                        /*//seating
            new FunctionSortList(){FlagNum = 11, FunctionSubsortNum = 1, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 11, FunctionSubsortNum = 2, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 11, FunctionSubsortNum = 4, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 11, FunctionSubsortNum = 8, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 11, FunctionSubsortNum = 16, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 11, FunctionSubsortNum = 32, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 11, FunctionSubsortNum = 64, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 11, FunctionSubsortNum = 128, Category = "Seating", Subcategory = ""},
                        //seating
            new FunctionSortList(){FlagNum = 12, FunctionSubsortNum = 1, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 12, FunctionSubsortNum = 2, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 12, FunctionSubsortNum = 4, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 12, FunctionSubsortNum = 8, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 12, FunctionSubsortNum = 16, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 12, FunctionSubsortNum = 32, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 12, FunctionSubsortNum = 64, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 12, FunctionSubsortNum = 128, Category = "Seating", Subcategory = ""},
                        //seating
            new FunctionSortList(){FlagNum = 13, FunctionSubsortNum = 1, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 13, FunctionSubsortNum = 2, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 13, FunctionSubsortNum = 4, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 13, FunctionSubsortNum = 8, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 13, FunctionSubsortNum = 16, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 13, FunctionSubsortNum = 32, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 13, FunctionSubsortNum = 64, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 13, FunctionSubsortNum = 128, Category = "Seating", Subcategory = ""},
                        //seating
            new FunctionSortList(){FlagNum = 14, FunctionSubsortNum = 1, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 14, FunctionSubsortNum = 2, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 14, FunctionSubsortNum = 4, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 14, FunctionSubsortNum = 8, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 14, FunctionSubsortNum = 16, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 14, FunctionSubsortNum = 32, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 14, FunctionSubsortNum = 64, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 14, FunctionSubsortNum = 128, Category = "Seating", Subcategory = ""},
                        //seating
            new FunctionSortList(){FlagNum = 15, FunctionSubsortNum = 1, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 15, FunctionSubsortNum = 2, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 15, FunctionSubsortNum = 4, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 15, FunctionSubsortNum = 8, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 15, FunctionSubsortNum = 16, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 15, FunctionSubsortNum = 32, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 15, FunctionSubsortNum = 64, Category = "Seating", Subcategory = ""},
            new FunctionSortList(){FlagNum = 15, FunctionSubsortNum = 128, Category = "Seating", Subcategory = ""},
            */
        };

        public static List<FunctionSortList> Sims2BuildFunctionSortList = new(){
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 1, Category = "Door"},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 4, Category = "Window"},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 100, Category = "Two Story Door"},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 2, Category = "Two Story Window"},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 10, Category = "Arch"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 20, Category = "Staircase"},
            new FunctionSortList(){FlagNum = 0, FunctionSubsortNum = 0, Category = "Fireplaces (?)"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 400, Category = "Garage"},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 1, Category = "Trees"},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 4, Category = "Flowers"},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 10, Category = "Gardening"},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 2, Category = "Shrubs"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 1000, Category = "Architecture"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 8, Category = "Column"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 100, Category = "Two Story Column"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 200, Category = "Connecting Column"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 40, Category = "Pools"},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 8, Category = "Gates"},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 800, Category = "Elevator"},
            new FunctionSortList(){Category = "Wall", Subcategory = "Wallpaper"},
            new FunctionSortList(){Category = "Wall", Subcategory = "Paneling"},
            new FunctionSortList(){Category = "Wall", Subcategory = "Brick"},
            new FunctionSortList(){Category = "Wall", Subcategory = "Masonry"},
            new FunctionSortList(){Category = "Wall", Subcategory = "Siding"},
            new FunctionSortList(){Category = "Wall", Subcategory = "Poured"},
            new FunctionSortList(){Category = "Wall", Subcategory = "Paint"},
            new FunctionSortList(){Category = "Wall", Subcategory = "Tile"},
            new FunctionSortList(){Category = "Wall", Subcategory = "Poured"},
            new FunctionSortList(){Category = "Wall", Subcategory = "Other"},
            new FunctionSortList(){Category = "Wall"},
            new FunctionSortList(){Category = "Floor", Subcategory = "Tile"},
            new FunctionSortList(){Category = "Floor", Subcategory = "Lino"},
            new FunctionSortList(){Category = "Floor", Subcategory = "Carpet"},
            new FunctionSortList(){Category = "Floor", Subcategory = "Wood"},
            new FunctionSortList(){Category = "Floor", Subcategory = "Poured"},
            new FunctionSortList(){Category = "Floor", Subcategory = "Stone"},
            new FunctionSortList(){Category = "Floor", Subcategory = "Brick"},
            new FunctionSortList(){Category = "Floor", Subcategory = "Other"},
            new FunctionSortList(){Category = "Floor", Subcategory = "0"},
            new FunctionSortList(){Category = "Floor"},
            new FunctionSortList(){Category = "TerrainPaint", Subcategory = "Dirt"},
            new FunctionSortList(){Category = "TerrainPaint", Subcategory = "Gravel"},
            new FunctionSortList(){Category = "TerrainPaint", Subcategory = "Grass"},
            new FunctionSortList(){Category = "TerrainPaint"}
        };


    }
    public class SimsPackageReader : IDisposable
    {
        public ISimsData SimsData
        {
            set
            {
                if (PackageGame == SimsGames.Sims2)
                {
                    Sims2Data = value as Sims2Data;
                }
                else if (PackageGame == SimsGames.Sims3)
                {
                    Sims3Data = value as Sims3Data;
                }
                else if (PackageGame == SimsGames.Sims4)
                {
                    Sims4Data = value as Sims4Data;
                }
                else
                {
                    return;
                }
            }
            get
            {
                if (PackageGame == SimsGames.Sims2)
                {
                    return Sims2Data;
                }
                else if (PackageGame == SimsGames.Sims3)
                {
                    return Sims3Data;
                }
                else
                {
                    return Sims4Data;
                }
            }
        }

        public Sims2Data Sims2Data;
        public Sims3Data Sims3Data;
        public Sims4Data Sims4Data;

        public SimsGames PackageGame;

        private string _dbpf;
        public string DBPF
        {
            get { return _dbpf; }
            set
            {
                _dbpf = value;
                if (value == "DBPF")
                {
                    IsBroken = false;
                }
                else
                {
                    IsBroken = true;
                }
            }
        }
        public uint MajorVersion;
        private uint _minorversion;
        public uint MinorVersion
        {
            get { return _minorversion; }
            set
            {
                _minorversion = value;
                switch (value)
                {
                    case 1: //minor is 1
                        if (MajorVersion == 1) PackageGame = SimsGames.Sims2;
                        if (MajorVersion == 2) PackageGame = SimsGames.Sims4;
                        break;

                    case 2: // minor is 2
                        if (MajorVersion == 1) PackageGame = SimsGames.Sims2;
                        break;

                    case 0: //minor is 0
                        switch (MajorVersion)
                        {
                            case 2:
                                PackageGame = SimsGames.Sims3;
                                break;
                            case 3:
                                PackageGame = SimsGames.SimCity5;
                                break;
                        }
                        break;

                }
            }
        }


        public bool IsBroken = false;
        MemoryStream msPackage;
        BinaryReader packagereader;
        public FileInfo fileinfo;

        uint IndexMajorVersion;
        uint IndexMinorVersion;

        public void ReadPackage(string file)
        {
            fileinfo = new(file);
            msPackage = ByteReaders.ReadBytesToFile(file);
            packagereader = new BinaryReader(msPackage);
            DBPF = Encoding.ASCII.GetString(packagereader.ReadBytes(4));
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Package {0} test string: {1}", fileinfo.Name, DBPF));
            if (IsBroken) return;

            MajorVersion = packagereader.ReadUInt32();
            MinorVersion = packagereader.ReadUInt32();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("File {0} is for game {1}", fileinfo.Name, PackageGame.ToString()));

            switch (PackageGame)
            {
                case SimsGames.Sims2:
                    SimsData = new Sims2Data() { FileLocation = file };
                    ReadSims2Package();
                    break;

                case SimsGames.Sims3:
                    SimsData = new Sims3Data() { FileLocation = file };

                    ReadSims3Package();
                    //run package reader
                    break;

                case SimsGames.Sims4:
                    SimsData = new Sims4Data() { FileLocation = file };
                    ReadSims4Package();
                    //run package reader
                    break;

            }

        }



        long IndexMajorLocation = 24;
        long IndexMinorLocation = 60;

        uint IndexCount;
        uint IndexOffset;
        uint IndexSize;
        uint ChunkOffset = 0;
        uint DateCreated;
        uint DateModified;
        uint HolesCount;
        uint HolesOffset;
        uint HolesSize;

        List<string> InstanceIDs = new();

        List<IndexEntry> IndexData = new();

        public void ReadSims2Package()
        {
            if (MinorVersion == 2) IndexMajorLocation = 24;
            if (MinorVersion == 0) IndexMajorLocation = 32;
            //packagereader.BaseStream.Position = IndexMajorLocation;
            Encoding.UTF8.GetString(packagereader.ReadBytes(12));
            DateCreated = packagereader.ReadUInt32();
            DateModified = packagereader.ReadUInt32();
            IndexMajorVersion = packagereader.ReadUInt32();

            IndexCount = packagereader.ReadUInt32();

            IndexOffset = packagereader.ReadUInt32();

            IndexSize = packagereader.ReadUInt32();
            //packagereader.BaseStream.Position = IndexMinorLocation;
            HolesCount = packagereader.ReadUInt32();
            HolesOffset = packagereader.ReadUInt32();
            HolesSize = packagereader.ReadUInt32();

            IndexMinorVersion = packagereader.ReadUInt32() - 1;

            Encoding.UTF8.GetString(packagereader.ReadBytes(32));

            //move to the index location
            packagereader.BaseStream.Position = ChunkOffset + IndexOffset;
            if (IndexCount == 0)
            {
                IsBroken = true;
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("File {0} has 0 index entries. Broken. Returning.", fileinfo.Name));
                return;
            }

            //get which types are here

            for (int i = 0; i < IndexCount; i++)
            {

                IndexEntry holderEntry = new IndexEntry();
                holderEntry.TypeID = packagereader.ReadUInt32().ToString("X8");

                holderEntry.GroupID = packagereader.ReadUInt32().ToString("X8");
                holderEntry.InstanceID = packagereader.ReadUInt32().ToString("X8");

                InstanceIDs.Add(holderEntry.InstanceID.ToString());

                if ((IndexMajorVersion == 7) && (IndexMinorVersion == 1))
                {
                    holderEntry.InstanceID2 = packagereader.ReadUInt32().ToString("X8");
                }
                else
                {
                    holderEntry.InstanceID2 = "00000000";
                }
                holderEntry.Offset = packagereader.ReadUInt32();

                holderEntry.FileSize = packagereader.ReadUInt32();

                holderEntry.UncompressedSize = 0;

                holderEntry.IsCompressed = false;

                holderEntry.EntryIDX = i;

                IndexData.Add(holderEntry);

                //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}", holderEntry.ToString()));

            }



            //read types
            if (IndexData.Exists(x => x.EntryType == "DIR"))
            {
                List<IndexEntry> dirs = IndexData.Where(x => x.EntryType == "DIR").ToList();
                foreach (IndexEntry dir in dirs)
                {
                    uint myFilesize;
                    uint NumRecords;
                    packagereader.BaseStream.Position = ChunkOffset + dir.Offset;

                    if (IndexMajorVersion == 7 && IndexMinorVersion == 1)
                    {
                        NumRecords = dir.FileSize / 20;
                    }
                    else
                    {
                        NumRecords = dir.FileSize / 16;
                    }

                    for (int i = 0; i < NumRecords; i++)
                    {
                        // read compressed records
                        IndexEntry holderEntry = new IndexEntry();
                        holderEntry.TypeID = packagereader.ReadUInt32().ToString("X8");
                        holderEntry.GroupID = packagereader.ReadUInt32().ToString("X8");
                        //packagereader.ReadUInt32().ToString("X8");
                        holderEntry.InstanceID = packagereader.ReadUInt32().ToString("X8");
                        InstanceIDs.Add(holderEntry.InstanceID.ToString());
                        if (IndexMajorVersion == 7 && IndexMinorVersion == 1) holderEntry.InstanceID2 = packagereader.ReadUInt32().ToString("X8");
                        myFilesize = packagereader.ReadUInt32();

                        if (IndexData.Any(x => x.CompleteID == holderEntry.CompleteID))
                        {
                            IndexEntry entry = IndexData.First(x => x.CompleteID == holderEntry.CompleteID);
                            entry.IsCompressed = true;
                            entry.UncompressedSize = myFilesize;
                        }
                    }
                }
            }

            ListEntries(IndexData);


            if (IndexData.Exists(x => x.EntryType == "CTSS"))
            {
                S2ReadCTSS(IndexData.First(x => x.EntryType == "CTSS"));

            }
            else if (IndexData.Exists(x => x.EntryType == "XOBJ"))
            {
                S2ReadXOBJ(IndexData.First(x => x.EntryType == "XOBJ"));
            }
            if (IndexData.Exists(x => x.EntryType == "OBJD"))
            {
                S2ReadOBJD(IndexData.First(x => x.EntryType == "OBJD"));
            }
            if (IndexData.Exists(x => x.EntryType == "GMDC"))
            {
                int c = 0;
                foreach (IndexEntry entry in IndexData.Where(x => x.EntryType == "GMDC"))
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Reading {0} GMDC #{1}", fileinfo.Name, c));
                    S2ReadGMDC(entry);
                    c++;
                }

            }
            if (IndexData.Exists(x => x.EntryType == "MMAT"))
            {
                foreach (IndexEntry entry in IndexData.Where(x => x.EntryType == "MMAT"))
                {
                    S2ReadMMAT(entry);
                }
            }
            if (IndexData.Exists(x => x.EntryType == "XFLR"))
            {
                foreach (IndexEntry entry in IndexData.Where(x => x.EntryType == "XFLR"))
                {
                    S2ReadXFLR(entry);
                }
            }

            if (IndexData.Exists(x => x.EntryType == "TXTR"))
            {
                int c = 0;
                foreach (IndexEntry entry in IndexData.Where(x => x.EntryType == "TXTR"))
                {
                    S2ReadTXTR(entry, c);
                    c++;
                }
            }

            if (IndexData.Any(x => x.EntryType == "MMAT") && !IndexData.Any(x => x.EntryType == "GMDC"))
            {
                if (SimsData.FunctionSort.Count != 0)
                {
                    if (SimsData.FunctionSort[0].Category.Equals("wall", StringComparison.CurrentCultureIgnoreCase) || SimsData.FunctionSort[0].Category.Equals("floor", StringComparison.CurrentCultureIgnoreCase) || SimsData.FunctionSort[0].Category.Equals("terrainpaint", StringComparison.CurrentCultureIgnoreCase))
                    {
                        //no
                    }
                    else
                    {
                        SimsData.Recolor = true;
                    }
                }
                else
                {
                    SimsData.Recolor = true;
                }

            }
            else if (IndexData.Any(x => x.EntryType == "GMDC"))
            {
                SimsData.Mesh = true;
            }

            if (Sims2Data.MMATDataBlock.Count != 0)
            {
                //if (string.IsNullOrEmpty(Sims2Data.GUID)) Sims2Data.GUID = Sims2Data.MMATDataBlock[0].ObjectGUID;
                if (!string.IsNullOrEmpty(Sims2Data.MMATDataBlock[0]?.ObjectGUID) && (string.IsNullOrEmpty(Sims2Data.GUID) || Sims2Data.GUID == "N/a")) Sims2Data.GUID = Sims2Data.MMATDataBlock[0].ObjectGUID;
            }

            if (IndexData.Any(x => x.EntryType == "BHAV") && IndexData.Any(x => x.EntryType == "BCON") && IndexData.Any(x => x.EntryType == "TTAB") && IndexData.Any(x => x.EntryType == "OBJf"))
            {
                SimsData.Mesh = false;
                SimsData.Recolor = false;
                SimsData.GameMod = true;
            }

            SimsData.IndexEntries = IndexData;


            //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("SimsData: {0}", SimsData.ToString()));
            //SimsData.Serialize();
        }



        public void S2ReadTXTR(IndexEntry entry, int txtrc)
        {
            S2ReadTXTRChunk txtr;

            packagereader.BaseStream.Position = ChunkOffset + entry.Offset;
            int cFileSize = packagereader.ReadInt32();
            string cTypeID = packagereader.ReadUInt16().ToString("X4");
            if (cTypeID == "FB10")
            {
                byte[] tempBytes = packagereader.ReadBytes(3);
                uint cFullSize = Sims2EntryReaders.QFSLengthToInt(tempBytes);
                DecryptByteStream decompressed = new DecryptByteStream(Sims2EntryReaders.Uncompress(packagereader.ReadBytes(cFileSize), cFullSize, 0));
                txtr = new(decompressed, fileinfo, txtrc);

            }
            else
            {
                packagereader.BaseStream.Position = ChunkOffset + entry.Offset;
                txtr = new(packagereader, fileinfo, txtrc);
            }
            (SimsData as Sims2Data).TXTRDataBlock.Add(txtr.TXTRData);
            if (!string.IsNullOrEmpty(txtr.TXTRData.GUID) && (string.IsNullOrEmpty(Sims2Data.GUID) || Sims2Data.GUID == "N/a")) Sims2Data.GUID = txtr.TXTRData.GUID;

        }
        public void S2ReadXFLR(IndexEntry entry)
        {
            S2ReadXFLRChunk xflr = new();

            packagereader.BaseStream.Position = ChunkOffset + entry.Offset;
            int cFileSize = packagereader.ReadInt32();
            string cTypeID = packagereader.ReadUInt16().ToString("X4");
            if (cTypeID == "FB10")
            {
                byte[] tempBytes = packagereader.ReadBytes(3);
                uint cFullSize = Sims2EntryReaders.QFSLengthToInt(tempBytes);
                string cpfTypeID = packagereader.ReadUInt32().ToString("X8");
                if ((cpfTypeID == "CBE7505E") || (cpfTypeID == "CBE750E0"))
                {
                    xflr = new(packagereader);
                }
                else
                {
                    packagereader.BaseStream.Position = ChunkOffset + entry.Offset + 9;
                    DecryptByteStream decompressed = new DecryptByteStream(Sims2EntryReaders.Uncompress(packagereader.ReadBytes(cFileSize), cFullSize, 0));
                    if (cpfTypeID == "E750E0E2")
                    {
                        // Read first four bytes
                        cpfTypeID = decompressed.ReadUInt32().ToString("X8");
                        if ((cpfTypeID == "CBE7505E") || (cpfTypeID == "CBE750E0"))
                        {
                            xflr = new(decompressed);
                        }
                    }
                    else
                    {
                        xflr = new(decompressed, true);
                    }
                }
            }

            (SimsData as Sims2Data).XFLRDataBlock = xflr.XFLRData;
            if (xflr.XFLRData.Type == "terrainPaint")
            {
                if (Sims2PackageStatics.Sims2BuildFunctionSortList.Any(x => x.Category.Equals(xflr.XFLRData.Type, StringComparison.CurrentCultureIgnoreCase) && x.Subcategory.Equals(xflr.XFLRData.SoundSuffix, StringComparison.CurrentCultureIgnoreCase)))
                {
                    (SimsData as Sims2Data).FunctionSort.Add(Sims2PackageStatics.Sims2BuildFunctionSortList.First(x => x.Category.Equals(xflr.XFLRData.Type, StringComparison.CurrentCultureIgnoreCase) && x.Subcategory.Equals(xflr.XFLRData.SoundSuffix, StringComparison.CurrentCultureIgnoreCase)));
                }
                else
                {
                    (SimsData as Sims2Data).FunctionSort.Add(new() { Category = "Terrain" });
                }
            }
        }

        public void S2ReadMMAT(IndexEntry entry)
        {
            S2ReadMMATChunk mmat = new();

            packagereader.BaseStream.Position = ChunkOffset + entry.Offset;
            int cFileSize = packagereader.ReadInt32();
            string cTypeID = packagereader.ReadUInt16().ToString("X4");
            if (cTypeID == "FB10")
            {
                byte[] tempBytes = packagereader.ReadBytes(3);
                uint cFullSize = Sims2EntryReaders.QFSLengthToInt(tempBytes);
                string cpfTypeID = packagereader.ReadUInt32().ToString("X8");
                if ((cpfTypeID == "CBE7505E") || (cpfTypeID == "CBE750E0"))
                {
                    mmat = new(packagereader);
                }
                else
                {
                    packagereader.BaseStream.Position = ChunkOffset + entry.Offset + 9;
                    DecryptByteStream decompressed = new DecryptByteStream(Sims2EntryReaders.Uncompress(packagereader.ReadBytes(cFileSize), cFullSize, 0));
                    if (cpfTypeID == "E750E0E2")
                    {
                        // Read first four bytes
                        cpfTypeID = decompressed.ReadUInt32().ToString("X8");
                        if ((cpfTypeID == "CBE7505E") || (cpfTypeID == "CBE750E0"))
                        {
                            mmat = new(decompressed);
                        }
                    }
                    else
                    {
                        mmat = new(decompressed, true);
                    }
                }
            }

            (SimsData as Sims2Data).MMATDataBlock.Add(mmat.MMATData);
        }


        public void S2ReadGMDC(IndexEntry entry)
        {
            S2ReadGMDCChunk gmdc;

            packagereader.BaseStream.Position = ChunkOffset + entry.Offset;
            if (entry.IsCompressed)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0} GMDC is compressed!", fileinfo.Name));

                List<byte> bytes = new();
                while (packagereader.BaseStream.Length > packagereader.BaseStream.Position)
                {
                    bytes.Add(packagereader.ReadByte());
                }


                byte[] decompressedByte = Sims2Tools.DBPF.Utils.Decompressor.Decompress([.. bytes], entry.UncompressedSize * 5);

                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Normal byte length: {0}, decompressed byte length: {1}", bytes.ToArray().Length, decompressedByte.Length));


                BinaryReader binaryReader = new(new MemoryStream(decompressedByte));
                gmdc = new(binaryReader);
            }
            else
            {
                gmdc = new(packagereader);
            }
            (SimsData as Sims2Data).GMDCDataBlock.Add(gmdc.GMDCData);
        }

        public void S2ReadCTSS(IndexEntry entry)
        {
            S2ReadCTSSChunk cts;
            packagereader.BaseStream.Position = ChunkOffset + entry.Offset;
            int cFileSize = packagereader.ReadInt32();
            string cTypeID = packagereader.ReadUInt16().ToString("X4");
            if (cTypeID == "FB10")
            {
                byte[] tempBytes = packagereader.ReadBytes(3);
                uint cFullSize = Sims2EntryReaders.QFSLengthToInt(tempBytes);
                DecryptByteStream decompressed = new DecryptByteStream(Sims2EntryReaders.Uncompress(packagereader.ReadBytes(cFileSize), cFullSize, 0));
                cts = new(decompressed);
            }
            else
            {
                packagereader.BaseStream.Position = ChunkOffset + entry.Offset;
                cts = new(packagereader);
            }
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("File {0}. CTSS: {1}", fileinfo.Name, cts.ToString()));
            SimsData.Title = cts.Title;
            SimsData.Description = cts.Description;
        }

        public void S2ReadOBJD(IndexEntry entry)
        {
            S2ReadOBJDChunk objd;
            packagereader.BaseStream.Position = ChunkOffset + entry.Offset;
            int cFileSize = packagereader.ReadInt32();
            string cTypeID = packagereader.ReadUInt16().ToString("X4");
            if (cTypeID == "FB10")
            {
                byte[] tempBytes = packagereader.ReadBytes(3);
                uint cFullSize = Sims2EntryReaders.QFSLengthToInt(tempBytes);
                DecryptByteStream decompressed = new DecryptByteStream(Sims2EntryReaders.Uncompress(packagereader.ReadBytes(cFileSize), cFullSize, 0));
                objd = new(decompressed);

            }
            else
            {
                packagereader.BaseStream.Position = ChunkOffset + entry.Offset;
                objd = new(packagereader);
            }
            //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("File {0}. OBJD: {1}", fileinfo.Name, objd.ToString()));
            SimsData.FunctionSort = objd.FunctionSort;
            SimsData.GUID = objd.ObjectGUID;
        }
        public void S2ReadXOBJ(IndexEntry entry)
        {
            S2ReadXOBJChunk xobj = new();

            packagereader.BaseStream.Position = ChunkOffset + entry.Offset;
            int cFileSize = packagereader.ReadInt32();
            string cTypeID = packagereader.ReadUInt16().ToString("X4");
            if (cTypeID == "FB10")
            {
                byte[] tempBytes = packagereader.ReadBytes(3);
                uint cFullSize = Sims2EntryReaders.QFSLengthToInt(tempBytes);
                string cpfTypeID = packagereader.ReadUInt32().ToString("X8");
                if ((cpfTypeID == "CBE7505E") || (cpfTypeID == "CBE750E0"))
                {
                    xobj = new(packagereader);
                }
                else
                {
                    packagereader.BaseStream.Position = ChunkOffset + entry.Offset + 9;
                    DecryptByteStream decompressed = new DecryptByteStream(Sims2EntryReaders.Uncompress(packagereader.ReadBytes(cFileSize), cFullSize, 0));
                    if (cpfTypeID == "E750E0E2")
                    {
                        // Read first four bytes
                        cpfTypeID = decompressed.ReadUInt32().ToString("X8");
                        if ((cpfTypeID == "CBE7505E") || (cpfTypeID == "CBE750E0"))
                        {
                            xobj = new(decompressed);
                        }
                    }
                    else
                    {
                        xobj = new(decompressed, true);
                    }
                }
            }

            if (xobj.Category != null)
            {
                if (Sims2PackageStatics.Sims2BuildFunctionSortList.Any(x => x.Category.Equals(xobj.Type, StringComparison.CurrentCultureIgnoreCase)))
                {
                    SimsData.FunctionSort.Add(Sims2PackageStatics.Sims2BuildFunctionSortList.First(x => x.Category.Equals(xobj.Type, StringComparison.CurrentCultureIgnoreCase)));
                }
            }
            if (xobj.Title != null) SimsData.Title = xobj.Title;
            if (xobj.Description != null) SimsData.Description = xobj.Description;
            if (xobj.Type != null)
            {
                if (Sims2PackageStatics.Sims2BuildFunctionSortList.Any(x => x.Category.Equals(xobj.Type, StringComparison.CurrentCultureIgnoreCase) && (x.Subcategory.Equals(xobj.Subsort, StringComparison.CurrentCultureIgnoreCase) || x.Subcategory.Equals(xobj.Subtype, StringComparison.CurrentCultureIgnoreCase))))
                {
                    if (!Sims2Data.FunctionSort.Contains(Sims2PackageStatics.Sims2BuildFunctionSortList.First(x => x.Category.Equals(xobj.Type, StringComparison.CurrentCultureIgnoreCase))))
                    {
                        SimsData.FunctionSort.Add(Sims2PackageStatics.Sims2BuildFunctionSortList.First(x => x.Category.Equals(xobj.Type, StringComparison.CurrentCultureIgnoreCase)));
                    }
                }
            }
            if (xobj.ObjectGUID != null) SimsData.GUID = xobj.ObjectGUID;










            //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("File: {0}, {1}", fileinfo.Name, xobj.ToString()));            
        }







        public void ReadSims3Package()
        {
            return;
        }
        public void ReadSims4Package()
        {
            return;
        }










        private void ListEntries(List<IndexEntry> entries)
        {
            StringBuilder sb = new();
            int i = 0;
            foreach (IndexEntry enrty in entries)
            {
                sb.AppendLine(string.Format("{0} - {1}", i, enrty.ToString()));
                i++;
            }
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0} Entries: {1}", fileinfo.Name, sb.ToString()));

        }

        public override string ToString()
        {
            return string.Format("{0} is for game {1}", fileinfo.Name, PackageGame);
        }

        public void Dispose()
        {
           msPackage.Dispose();
           packagereader.Dispose();           
           Sims2Data = new();
           Sims3Data = new();
           Sims4Data = new();
           SimsData = Sims2Data;
           
        }
    }

    public struct S2ReadTXTRChunk
    {
        public TXTRData TXTRData = new();
        public FileInfo file;
        private List<uint> Mips = new();
        public S2ReadTXTRChunk(BinaryReader readFile, FileInfo fileInfo, int txtrc)
        {
            file = fileInfo;
            uint CreatorID;
            uint FormatFlag;
            readFile.ReadBytes(16);
            byte namelength = readFile.ReadByte();
            string blockName = CleanInput(Encoding.UTF8.GetString(readFile.ReadBytes(namelength)));
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("BlockName: {0}", blockName));
            string BlockID = readFile.ReadUInt32().ToString("X8");
            uint BlockVersion = readFile.ReadUInt32(); // 7, 8, or 9
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("TXTR BlockName: {0}, Block ID: {1}, Block Version: {2}", blockName, BlockID, BlockVersion));
            byte resourceIDlength = readFile.ReadByte();
            string resourceID = CleanInput(Encoding.UTF8.GetString(readFile.ReadBytes(resourceIDlength)));
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("ResourceID: {0}", resourceID));
            readFile.ReadBytes(8);
            byte filenamelength = readFile.ReadByte();
            TXTRData.FullTXTRName = Encoding.UTF8.GetString(readFile.ReadBytes(filenamelength));
            if (!string.IsNullOrEmpty(TXTRData.TextureName)) if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Full Filename: {0}, Guid: {1}, Texture Name: {2}", TXTRData.FullTXTRName, TXTRData.GUID, TXTRData.TextureName));
            TXTRData.TextureWidth = readFile.ReadUInt32();
            TXTRData.TextureHeight = readFile.ReadUInt32();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Height: {0}, Width: {1}", TXTRData.TextureHeight, TXTRData.TextureWidth));
            TXTRData.FormatCode = readFile.ReadUInt32();
            TXTRData.MipMapLevels = readFile.ReadUInt32();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Image stored as Format {0}, Mipmaps: {1}", TXTRData.FormatCode, TXTRData.MipMapLevels));
            uint Purpose = readFile.ReadUInt16();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Purpose: {0}", Purpose));
            uint unknown2 = readFile.ReadUInt16();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Unknown2 that should be 3f80,4040,4000: {0}", unknown2.ToString("X4")));
            uint OuterLoopCount = readFile.ReadUInt32();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Outer loops: {0}", OuterLoopCount));
            uint unknown3 = readFile.ReadUInt32();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("This should be 0: {0}", unknown3));
            uint InnerLoopCount = 0;
            if (BlockVersion == 9)
            {
                byte repfilename = readFile.ReadByte();
                string repeatFileName = Encoding.UTF8.GetString(readFile.ReadBytes(repfilename));
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Repeat File Name: {0}", repeatFileName));
            }
            for (int o = 0; o < OuterLoopCount; o++)
            {
                if (BlockVersion != 9)
                {
                    InnerLoopCount = TXTRData.MipMapLevels;
                }
                else if (BlockVersion == 9)
                {
                    InnerLoopCount = readFile.ReadUInt32();
                }
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Inner Loops: {0}", InnerLoopCount));
                for (int i = 0; i < InnerLoopCount; i++)
                {
                    byte DataType = readFile.ReadByte();
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("DataType: {0}", DataType));
                    uint ImageDataSize = 0;
                    if (DataType == 1)
                    {
                        byte len = readFile.ReadByte();
                        string LIFOfile = Encoding.UTF8.GetString(readFile.ReadBytes(len));
                    }
                    else
                    {
                        bool mipmaps = false;
                        //if (TXTRData.MipMapLevels > 1) mipmaps = true;
                        ImageDataSize = readFile.ReadUInt32();
                        InterpretMipSize(TXTRData.MipMapLevels);
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Image Data Size: {0}", ImageDataSize));
                        if (ImageDataSize >= Mips[0])
                        {
                            if (TXTRData.FormatCode == 1)
                            {
                                TXTRData.Texture = Godot.Image.CreateFromData((int)TXTRData.TextureWidth, (int)TXTRData.TextureHeight, mipmaps, Godot.Image.Format.Rgba8, readFile.ReadBytes((int)ImageDataSize));
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saved Image (Type: {0}, Name: {1}) to TXTRData. Size: {2}", TXTRData.FormatCode, TXTRData.FullTXTRName, TXTRData.Texture.GetDataSize()));
                                //texture.SavePng(string.Format("{0}_{1}.dds", fileInfo.FullName, txtrc));                                
                            }
                            else if (TXTRData.FormatCode == 2)
                            {
                                TXTRData.Texture = Godot.Image.CreateFromData((int)TXTRData.TextureWidth, (int)TXTRData.TextureHeight, mipmaps, Godot.Image.Format.Rgb8, readFile.ReadBytes((int)ImageDataSize));
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saved Image (Type: {0}, Name: {1}) to TXTRData. Size: {2}", TXTRData.FormatCode, TXTRData.FullTXTRName, TXTRData.Texture.GetDataSize()));
                                //texture.SavePng(string.Format("{0}_{1}.png", fileInfo.FullName, txtrc));
                            }
                            else if (TXTRData.FormatCode == 4)
                            {
                                TXTRData.Texture = Godot.Image.CreateFromData((int)TXTRData.TextureWidth, (int)TXTRData.TextureHeight, mipmaps, Godot.Image.Format.Dxt1, readFile.ReadBytes((int)ImageDataSize));
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saved Image (Type: {0}, Name: {1}) to TXTRData. Size: {2}", TXTRData.FormatCode, TXTRData.FullTXTRName, TXTRData.Texture.GetDataSize()));
                                //texture.SavePng(string.Format("{0}_{1}.png", fileInfo.FullName, txtrc));
                            }
                            else if (TXTRData.FormatCode == 5)
                            {
                                TXTRData.Texture = Godot.Image.CreateFromData((int)TXTRData.TextureWidth, (int)TXTRData.TextureHeight, mipmaps, Godot.Image.Format.Dxt3, readFile.ReadBytes((int)ImageDataSize));
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saved Image (Type: {0}, Name: {1}) to TXTRData. Size: {2}", TXTRData.FormatCode, TXTRData.FullTXTRName, TXTRData.Texture.GetDataSize()));
                                //texture.SavePng(string.Format("{0}_{1}.png", fileInfo.FullName, txtrc));
                            }
                            else if (TXTRData.FormatCode == 6)
                            {
                                TXTRData.Texture = Godot.Image.CreateFromData((int)TXTRData.TextureWidth, (int)TXTRData.TextureHeight, mipmaps, Godot.Image.Format.R8, readFile.ReadBytes((int)ImageDataSize));
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saved Image (Type: {0}, Name: {1}) to TXTRData. Size: {2}", TXTRData.FormatCode, TXTRData.FullTXTRName, TXTRData.Texture.GetDataSize()));
                                //texture.SavePng(string.Format("{0}_{1}.png", fileInfo.FullName, txtrc));
                            }
                            else if (TXTRData.FormatCode == 8)
                            {
                                TXTRData.Texture = Godot.Image.CreateFromData((int)TXTRData.TextureWidth, (int)TXTRData.TextureHeight, mipmaps, Godot.Image.Format.Dxt5, readFile.ReadBytes((int)ImageDataSize));
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saved Image (Type: {0}, Name: {1}) to TXTRData. Size: {2}", TXTRData.FormatCode, TXTRData.FullTXTRName, TXTRData.Texture.GetDataSize()));
                                //texture.SavePng(string.Format("{0}_{1}.png", fileInfo.FullName, txtrc));
                            }
                        }
                        else
                        {
                            readFile.ReadBytes((int)ImageDataSize);
                        }
                    }
                }
                if (BlockVersion == 7)
                {
                    CreatorID = readFile.ReadUInt32();
                }
                else
                {
                    CreatorID = readFile.ReadUInt32();
                    FormatFlag = readFile.ReadUInt32();
                }
            }
        }
        public S2ReadTXTRChunk(DecryptByteStream readFile, FileInfo fileInfo, int txtrc)
        {
            file = fileInfo;
            uint CreatorID;
            uint FormatFlag;
            readFile.ReadBytes(16);
            byte namelength = readFile.ReadByte();
            string blockName = CleanInput(Encoding.UTF8.GetString(readFile.ReadBytes(namelength)));
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("BlockName: {0}", blockName));
            string BlockID = readFile.ReadUInt32().ToString("X8");
            uint BlockVersion = readFile.ReadUInt32(); // 7, 8, or 9
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("TXTR BlockName: {0}, Block ID: {1}, Block Version: {2}", blockName, BlockID, BlockVersion));
            byte resourceIDlength = readFile.ReadByte();
            string resourceID = CleanInput(Encoding.UTF8.GetString(readFile.ReadBytes(resourceIDlength)));
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("ResourceID: {0}", resourceID));
            readFile.ReadBytes(8);
            byte filenamelength = readFile.ReadByte();
            TXTRData.FullTXTRName = Encoding.UTF8.GetString(readFile.ReadBytes(filenamelength));
            if (!string.IsNullOrEmpty(TXTRData.TextureName)) if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Full Filename: {0}, Guid: {1}, Texture Name: {2}", TXTRData.FullTXTRName, TXTRData.GUID, TXTRData.TextureName));
            TXTRData.TextureWidth = readFile.ReadUInt32();
            TXTRData.TextureHeight = readFile.ReadUInt32();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Height: {0}, Width: {1}", TXTRData.TextureHeight, TXTRData.TextureWidth));
            TXTRData.FormatCode = readFile.ReadUInt32();
            TXTRData.MipMapLevels = readFile.ReadUInt32();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Image stored as Format {0}, Mipmaps: {1}", TXTRData.FormatCode, TXTRData.MipMapLevels));
            uint Purpose = readFile.ReadUInt16();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Purpose: {0}", Purpose));
            uint unknown2 = readFile.ReadUInt16();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Unknown2 that should be 3f80,4040,4000: {0}", unknown2.ToString("X4")));
            uint OuterLoopCount = readFile.ReadUInt32();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Outer loops: {0}", OuterLoopCount));
            uint unknown3 = readFile.ReadUInt32();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("This should be 0: {0}", unknown3));
            uint InnerLoopCount = 0;
            if (BlockVersion == 9)
            {
                byte repfilename = readFile.ReadByte();
                string repeatFileName = Encoding.UTF8.GetString(readFile.ReadBytes(repfilename));
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Repeat File Name: {0}", repeatFileName));
            }
            for (int o = 0; o < OuterLoopCount; o++)
            {
                if (BlockVersion != 9)
                {
                    InnerLoopCount = TXTRData.MipMapLevels;
                }
                else if (BlockVersion == 9)
                {
                    InnerLoopCount = readFile.ReadUInt32();
                }
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Inner Loops: {0}", InnerLoopCount));
                for (int i = 0; i < InnerLoopCount; i++)
                {
                    byte DataType = readFile.ReadByte();
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("DataType: {0}", DataType));
                    uint ImageDataSize = 0;
                    if (DataType == 1)
                    {
                        byte len = readFile.ReadByte();
                        string LIFOfile = Encoding.UTF8.GetString(readFile.ReadBytes(len));
                    }
                    else
                    {
                        bool mipmaps = false;
                        //if (TXTRData.MipMapLevels > 1) mipmaps = true;
                        ImageDataSize = readFile.ReadUInt32();
                        InterpretMipSize(TXTRData.MipMapLevels);
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Image Data Size: {0}", ImageDataSize));
                        if (ImageDataSize >= Mips[0])
                        {
                            if (TXTRData.FormatCode == 1)
                            {
                                TXTRData.Texture = Godot.Image.CreateFromData((int)TXTRData.TextureWidth, (int)TXTRData.TextureHeight, mipmaps, Godot.Image.Format.Rgba8, readFile.ReadBytes((uint)ImageDataSize));
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saved Image (Type: {0}, Name: {1}) to TXTRData. Size: {2}", TXTRData.FormatCode, TXTRData.FullTXTRName, TXTRData.Texture.GetDataSize()));
                                //texture.SavePng(string.Format("{0}_{1}.dds", fileInfo.FullName, txtrc));                                
                            }
                            else if (TXTRData.FormatCode == 2)
                            {
                                TXTRData.Texture = Godot.Image.CreateFromData((int)TXTRData.TextureWidth, (int)TXTRData.TextureHeight, mipmaps, Godot.Image.Format.Rgb8, readFile.ReadBytes((uint)ImageDataSize));
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saved Image (Type: {0}, Name: {1}) to TXTRData. Size: {2}", TXTRData.FormatCode, TXTRData.FullTXTRName, TXTRData.Texture.GetDataSize()));
                                //texture.SavePng(string.Format("{0}_{1}.png", fileInfo.FullName, txtrc));
                            }
                            else if (TXTRData.FormatCode == 4)
                            {
                                TXTRData.Texture = Godot.Image.CreateFromData((int)TXTRData.TextureWidth, (int)TXTRData.TextureHeight, mipmaps, Godot.Image.Format.Dxt1, readFile.ReadBytes((uint)ImageDataSize));
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saved Image (Type: {0}, Name: {1}) to TXTRData. Size: {2}", TXTRData.FormatCode, TXTRData.FullTXTRName, TXTRData.Texture.GetDataSize()));
                                //texture.SavePng(string.Format("{0}_{1}.png", fileInfo.FullName, txtrc));
                            }
                            else if (TXTRData.FormatCode == 5)
                            {
                                TXTRData.Texture = Godot.Image.CreateFromData((int)TXTRData.TextureWidth, (int)TXTRData.TextureHeight, mipmaps, Godot.Image.Format.Dxt3, readFile.ReadBytes((uint)ImageDataSize));
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saved Image (Type: {0}, Name: {1}) to TXTRData. Size: {2}", TXTRData.FormatCode, TXTRData.FullTXTRName, TXTRData.Texture.GetDataSize()));
                                //texture.SavePng(string.Format("{0}_{1}.png", fileInfo.FullName, txtrc));
                            }
                            else if (TXTRData.FormatCode == 6)
                            {
                                TXTRData.Texture = Godot.Image.CreateFromData((int)TXTRData.TextureWidth, (int)TXTRData.TextureHeight, mipmaps, Godot.Image.Format.R8, readFile.ReadBytes((uint)ImageDataSize));
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saved Image (Type: {0}, Name: {1}) to TXTRData. Size: {2}", TXTRData.FormatCode, TXTRData.FullTXTRName, TXTRData.Texture.GetDataSize()));
                                //texture.SavePng(string.Format("{0}_{1}.png", fileInfo.FullName, txtrc));
                            }
                            else if (TXTRData.FormatCode == 8)
                            {
                                TXTRData.Texture = Godot.Image.CreateFromData((int)TXTRData.TextureWidth, (int)TXTRData.TextureHeight, mipmaps, Godot.Image.Format.Dxt5, readFile.ReadBytes((uint)ImageDataSize));
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saved Image (Type: {0}, Name: {1}) to TXTRData. Size: {2}", TXTRData.FormatCode, TXTRData.FullTXTRName, TXTRData.Texture.GetDataSize()));
                                //texture.SavePng(string.Format("{0}_{1}.png", fileInfo.FullName, txtrc));
                            }
                        }
                        else
                        {
                            readFile.ReadBytes((uint)ImageDataSize);
                        }

                    }
                }
                if (BlockVersion == 7)
                {
                    CreatorID = readFile.ReadUInt32();
                }
                else
                {
                    CreatorID = readFile.ReadUInt32();
                    FormatFlag = readFile.ReadUInt32();
                }
            }
        }


        public void InterpretMipSize(uint mips)
        {
            Mips = new();
            //uint size = 0;
            uint fullSize = 0;
            //int division = 0;

            if (TXTRData.FormatCode == 1)
            {
                fullSize = TXTRData.TextureWidth * TXTRData.TextureHeight * 4;
            }
            else if (TXTRData.FormatCode == 2)
            {
                fullSize = TXTRData.TextureWidth * TXTRData.TextureHeight * 3;
            }
            else if (TXTRData.FormatCode == 4)
            {
                fullSize = TXTRData.TextureWidth * TXTRData.TextureHeight / 2;
            }
            else if (TXTRData.FormatCode == 5)
            {
                fullSize = TXTRData.TextureWidth * TXTRData.TextureHeight;
            }
            else if (TXTRData.FormatCode == 6)
            {
                fullSize = TXTRData.TextureWidth * TXTRData.TextureHeight;
            }
            else
            {
                fullSize = TXTRData.TextureWidth * TXTRData.TextureHeight;
            }
            //size += fullSize;

            //uint prev = fullSize;

            Mips.Add(fullSize);

            for (uint i = 1; i < mips; i++)
            {
                uint width = TXTRData.TextureWidth;
                uint height = TXTRData.TextureHeight;
                if (i > 1)
                {
                    for (int s = 1; s < i; s++)
                    {
                        width /= 2;
                        height /= 2;
                    }
                    uint sum = 0;
                    if (TXTRData.FormatCode == 1)
                    {
                        sum = width * height * 4;
                    }
                    else if (TXTRData.FormatCode == 2)
                    {
                        sum = width * height * 3;
                    }
                    else if (TXTRData.FormatCode == 4)
                    {
                        sum = width * height / 2;
                    }
                    else if (TXTRData.FormatCode == 5)
                    {
                        sum = width * height;
                    }
                    else
                    {
                        sum = width * height;
                    }
                    Mips.Add(sum);
                }
            }

            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mips:"));

            foreach (uint m in Mips)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}", m));
            }

        }








        static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"[^\s\w\.@-]", "",
                                        RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters,
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }
    }

    public struct S2ReadXFLRChunk
    {
        public XFLRData XFLRData = new();

        private void DebugFinish()
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Type: {0}, Sound: {1}", XFLRData.Type, XFLRData.SoundSuffix));
        }

        public S2ReadXFLRChunk(BinaryReader readFile)
        {
            uint NumItems = readFile.ReadUInt32();
            // Read the items
            for (int i = 0; i < NumItems; i++)
            {
                // Get type of the item
                string dataType = readFile.ReadUInt32().ToString("X8");
                uint nameLength = readFile.ReadUInt32();
                string fieldName = Encoding.UTF8.GetString(readFile.ReadBytes((int)nameLength));

                uint fieldValueInt = 0;
                uint fieldValueFloat = 0;
                string fieldValueString = "";
                bool fieldValueBool = false;

                switch (dataType)
                {
                    // Int
                    case "EB61E4F7":
                        fieldValueInt = readFile.ReadUInt32();
                        break;
                    // Int #2 - Not Used
                    case "0C264712":
                        fieldValueInt = readFile.ReadUInt32();
                        break;
                    // String
                    case "0B8BEA18":
                        uint stringLength = readFile.ReadUInt32();
                        fieldValueString = Encoding.UTF8.GetString(readFile.ReadBytes((int)stringLength));
                        break;
                    // Float
                    case "ABC78708":
                        // Ignore for now
                        fieldValueFloat = readFile.ReadUInt32();
                        break;
                    // Boolean
                    case "CBA908E1":
                        fieldValueBool = readFile.ReadBoolean();
                        break;
                }

                switch (fieldName.ToLower())
                {
                    case "brushwidth":
                        XFLRData.BrushWidth = fieldValueInt;
                        break;
                    case "cost":
                        XFLRData.Cost = fieldValueInt;
                        break;
                    case "crapscore":
                        XFLRData.CrapScore = fieldValueFloat;
                        break;
                    case "depreciated":
                        XFLRData.Depreciated = (int)fieldValueInt;
                        break;
                    case "description":
                        XFLRData.Description = fieldValueString;
                        break;
                    case "guid":
                        XFLRData.Guid = fieldValueInt.ToString("X8");
                        break;
                    case "name":
                        XFLRData.Name = fieldValueString;
                        break;
                    case "nicenessmultiplier":
                        XFLRData.NicenessMultiplier = fieldValueFloat;
                        break;
                    case "resourcegroupid":
                        XFLRData.ResourceGroupID = fieldValueInt;
                        break;
                    case "resourceid":
                        XFLRData.ResourceID = fieldValueInt;
                        break;
                    case "resourcerestypeid":
                        XFLRData.ResourceResTypeID = fieldValueInt;
                        break;
                    case "showincatalog":
                        XFLRData.ShowInCatalog = fieldValueInt.ToString("X8");
                        break;
                    case "soundsuffix":
                        XFLRData.SoundSuffix = fieldValueString;
                        break;
                    case "stringsetgroupid":
                        XFLRData.StringSetGroupID = fieldValueInt;
                        break;
                    case "stringsetid":
                        XFLRData.StringSetID = fieldValueInt;
                        break;
                    case "stringsetrestypeid":
                        XFLRData.StringSetResTypeID = fieldValueInt;
                        break;
                    case "texturetname":
                        XFLRData.TextureTName = fieldValueString;
                        break;
                    case "type":
                        XFLRData.Type = fieldValueString;
                        break;
                    case "version":
                        XFLRData.Version = fieldValueInt;
                        break;

                }
            }
            DebugFinish();
        }

        public S2ReadXFLRChunk(DecryptByteStream readFile)
        {
            readFile.ReadUInt16();
            uint NumItems = readFile.ReadUInt32();
            // Read the items
            for (int i = 0; i < NumItems; i++)
            {
                // Get type of the item
                string dataType = readFile.ReadUInt32().ToString("X8");
                uint nameLength = readFile.ReadUInt32();
                string fieldName = Encoding.UTF8.GetString(readFile.ReadBytes(nameLength));

                uint fieldValueInt = 0;
                string fieldValueString = "";
                uint fieldValueFloat = 0;
                bool fieldValueBool = false;

                switch (dataType)
                {
                    // Int
                    case "EB61E4F7":
                        fieldValueInt = readFile.ReadUInt32();
                        break;
                    // Int #2 - Not Used
                    case "0C264712":
                        fieldValueInt = readFile.ReadUInt32();
                        break;
                    // String
                    case "0B8BEA18":
                        uint stringLength = readFile.ReadUInt32();
                        fieldValueString = Encoding.UTF8.GetString(readFile.ReadBytes(stringLength));
                        break;
                    // Float
                    case "ABC78708":
                        // Ignore for now
                        fieldValueFloat = readFile.ReadUInt32();
                        break;
                    // Boolean
                    case "CBA908E1":
                        fieldValueBool = readFile.ReadBoolean();
                        break;
                }

                switch (fieldName.ToLower())
                {
                    case "brushwidth":
                        XFLRData.BrushWidth = fieldValueInt;
                        break;
                    case "cost":
                        XFLRData.Cost = fieldValueInt;
                        break;
                    case "crapscore":
                        XFLRData.CrapScore = fieldValueFloat;
                        break;
                    case "depreciated":
                        XFLRData.Depreciated = (int)fieldValueInt;
                        break;
                    case "description":
                        XFLRData.Description = fieldValueString;
                        break;
                    case "guid":
                        XFLRData.Guid = fieldValueInt.ToString("X8");
                        break;
                    case "name":
                        XFLRData.Name = fieldValueString;
                        break;
                    case "nicenessmultiplier":
                        XFLRData.NicenessMultiplier = fieldValueFloat;
                        break;
                    case "resourcegroupid":
                        XFLRData.ResourceGroupID = fieldValueInt;
                        break;
                    case "resourceid":
                        XFLRData.ResourceID = fieldValueInt;
                        break;
                    case "resourcerestypeid":
                        XFLRData.ResourceResTypeID = fieldValueInt;
                        break;
                    case "showincatalog":
                        XFLRData.ShowInCatalog = fieldValueInt.ToString("X8");
                        break;
                    case "soundsuffix":
                        XFLRData.SoundSuffix = fieldValueString;
                        break;
                    case "stringsetgroupid":
                        XFLRData.StringSetGroupID = fieldValueInt;
                        break;
                    case "stringsetid":
                        XFLRData.StringSetID = fieldValueInt;
                        break;
                    case "stringsetrestypeid":
                        XFLRData.StringSetResTypeID = fieldValueInt;
                        break;
                    case "texturetname":
                        XFLRData.TextureTName = fieldValueString;
                        break;
                    case "type":
                        XFLRData.Type = fieldValueString;
                        break;
                    case "version":
                        XFLRData.Version = fieldValueInt;
                        break;
                }
            }
            DebugFinish();
        }

        public S2ReadXFLRChunk(DecryptByteStream readFile, bool xml)
        {
            XmlTextReader xmlDoc = new XmlTextReader(new StringReader(Encoding.UTF8.GetString(readFile.GetEntireStream())));
            bool inDesc = false;
            string inAttrDesc = "";
            while (xmlDoc.Read())
            {
                if (xmlDoc.NodeType == XmlNodeType.Element)
                {
                    if (xmlDoc.Name == "AnyString") inDesc = true;
                    if (xmlDoc.Name == "AnyUint32") inDesc = true;
                }
                if (xmlDoc.NodeType == XmlNodeType.EndElement)
                {
                    inDesc = false;
                    inAttrDesc = "";
                }
                if (inDesc == true)
                {
                    if (xmlDoc.AttributeCount > 0)
                    {
                        while (xmlDoc.MoveToNextAttribute())
                        {
                            switch (xmlDoc.Value.ToLower())
                            {
                                case "brushwidth":
                                case "cost":
                                case "crapscore":
                                case "depreciated":
                                case "description":
                                case "guid":
                                case "name":
                                case "nicenessmultiplier":
                                case "resourcegroupid":
                                case "resourceid":
                                case "resourcerestypeid":
                                case "showincatalog":
                                case "soundsuffix":
                                case "stringsetgroupid":
                                case "stringsetid":
                                case "stringsetrestypeid":
                                case "texturetname":
                                case "type":
                                case "version":
                                    inAttrDesc = xmlDoc.Value;
                                    break;
                            }
                        }
                    }
                }
                if (xmlDoc.NodeType == XmlNodeType.Text)
                {
                    if (inAttrDesc != "")
                    {
                        switch (inAttrDesc.ToLower())
                        {
                            case "brushwidth":
                                XFLRData.BrushWidth = uint.Parse(xmlDoc.Value);
                                break;
                            case "cost":
                                XFLRData.Cost = uint.Parse(xmlDoc.Value);
                                break;
                            case "crapscore":
                                XFLRData.CrapScore = float.Parse(xmlDoc.Value);
                                break;
                            case "depreciated":
                                XFLRData.Depreciated = int.Parse(xmlDoc.Value);
                                break;
                            case "description":
                                XFLRData.Description = xmlDoc.Value;
                                break;
                            case "guid":
                                XFLRData.Guid = xmlDoc.Value;
                                break;
                            case "name":
                                XFLRData.Name = xmlDoc.Value;
                                break;
                            case "nicenessmultiplier":
                                XFLRData.NicenessMultiplier = float.Parse(xmlDoc.Value);
                                break;
                            case "resourcegroupid":
                                XFLRData.ResourceGroupID = uint.Parse(xmlDoc.Value);
                                break;
                            case "resourceid":
                                XFLRData.ResourceID = uint.Parse(xmlDoc.Value);
                                break;
                            case "resourcerestypeid":
                                XFLRData.ResourceResTypeID = uint.Parse(xmlDoc.Value);
                                break;
                            case "showincatalog":
                                XFLRData.ShowInCatalog = xmlDoc.Value;
                                break;
                            case "soundsuffix":
                                XFLRData.SoundSuffix = xmlDoc.Value;
                                break;
                            case "stringsetgroupid":
                                XFLRData.StringSetGroupID = uint.Parse(xmlDoc.Value);
                                break;
                            case "stringsetid":
                                XFLRData.StringSetID = uint.Parse(xmlDoc.Value);
                                break;
                            case "stringsetrestypeid":
                                XFLRData.StringSetResTypeID = uint.Parse(xmlDoc.Value);
                                break;
                            case "texturetname":
                                XFLRData.TextureTName = xmlDoc.Value;
                                break;
                            case "type":
                                XFLRData.Type = xmlDoc.Value;
                                break;
                        }
                    }
                }
            }
            DebugFinish();
        }
    }

    public struct S2ReadMMATChunk
    {
        public MMATData MMATData = new();


        public S2ReadMMATChunk(BinaryReader readFile)
        {
            uint NumItems = readFile.ReadUInt32();
            // Read the items
            for (int i = 0; i < NumItems; i++)
            {
                // Get type of the item
                string dataType = readFile.ReadUInt32().ToString("X8");
                uint nameLength = readFile.ReadUInt32();
                string fieldName = Encoding.UTF8.GetString(readFile.ReadBytes((int)nameLength));

                uint fieldValueInt = 0;
                uint fieldValueFloat = 0;
                string fieldValueString = "";
                bool fieldValueBool = false;

                switch (dataType)
                {
                    // Int
                    case "EB61E4F7":
                        fieldValueInt = readFile.ReadUInt32();
                        break;
                    // Int #2 - Not Used
                    case "0C264712":
                        fieldValueInt = readFile.ReadUInt32();
                        break;
                    // String
                    case "0B8BEA18":
                        uint stringLength = readFile.ReadUInt32();
                        fieldValueString = Encoding.UTF8.GetString(readFile.ReadBytes((int)stringLength));
                        break;
                    // Float
                    case "ABC78708":
                        // Ignore for now
                        fieldValueFloat = readFile.ReadUInt32();
                        break;
                    // Boolean
                    case "CBA908E1":
                        fieldValueBool = readFile.ReadBoolean();
                        break;
                }

                switch (fieldName.ToLower())
                {
                    case "creator":
                        MMATData.Creator = fieldValueString;
                        break;
                    case "defaultmaterial":
                        MMATData.DefaultMaterial = fieldValueBool;
                        break;
                    case "family":
                        MMATData.Family = fieldValueString;
                        break;
                    case "flags":
                        MMATData.Flags = fieldValueInt.ToString();
                        break;
                    case "materialstateflags":
                        MMATData.MaterialStateFlags = fieldValueInt.ToString();
                        break;
                    case "modelname":
                        MMATData.ModelName = fieldValueString;
                        break;
                    case "name":
                        MMATData.Name = fieldValueString;
                        break;
                    case "objectguid":
                        MMATData.ObjectGUID = fieldValueInt.ToString("X8");
                        break;
                    case "objectstateindex":
                        MMATData.ObjectStateIndex = fieldValueInt.ToString();
                        break;
                    case "subsetname":
                        MMATData.SubsetName = fieldValueString;
                        break;
                    case "type":
                        MMATData.Type = fieldValueString;
                        break;
                }
            }
        }

        public S2ReadMMATChunk(DecryptByteStream readFile)
        {
            readFile.ReadUInt16();
            uint NumItems = readFile.ReadUInt32();
            // Read the items
            for (int i = 0; i < NumItems; i++)
            {
                // Get type of the item
                string dataType = readFile.ReadUInt32().ToString("X8");
                uint nameLength = readFile.ReadUInt32();
                string fieldName = Encoding.UTF8.GetString(readFile.ReadBytes(nameLength));

                uint fieldValueInt = 0;
                string fieldValueString = "";
                uint fieldValueFloat = 0;
                bool fieldValueBool = false;

                switch (dataType)
                {
                    // Int
                    case "EB61E4F7":
                        fieldValueInt = readFile.ReadUInt32();
                        break;
                    // Int #2 - Not Used
                    case "0C264712":
                        fieldValueInt = readFile.ReadUInt32();
                        break;
                    // String
                    case "0B8BEA18":
                        uint stringLength = readFile.ReadUInt32();
                        fieldValueString = Encoding.UTF8.GetString(readFile.ReadBytes(stringLength));
                        break;
                    // Float
                    case "ABC78708":
                        // Ignore for now
                        fieldValueFloat = readFile.ReadUInt32();
                        break;
                    // Boolean
                    case "CBA908E1":
                        fieldValueBool = readFile.ReadBoolean();
                        break;
                }

                switch (fieldName.ToLower())
                {
                    case "creator":
                        MMATData.Creator = fieldValueString;
                        break;
                    case "defaultmaterial":
                        MMATData.DefaultMaterial = fieldValueBool;
                        break;
                    case "family":
                        MMATData.Family = fieldValueString;
                        break;
                    case "flags":
                        MMATData.Flags = fieldValueInt.ToString();
                        break;
                    case "materialstateflags":
                        MMATData.MaterialStateFlags = fieldValueInt.ToString();
                        break;
                    case "modelname":
                        MMATData.ModelName = fieldValueString;
                        break;
                    case "name":
                        MMATData.Name = fieldValueString;
                        break;
                    case "objectguid":
                        MMATData.ObjectGUID = fieldValueInt.ToString("X8");
                        break;
                    case "objectstateindex":
                        MMATData.ObjectStateIndex = fieldValueInt.ToString();
                        break;
                    case "subsetname":
                        MMATData.SubsetName = fieldValueString;
                        break;
                    case "type":
                        MMATData.Type = fieldValueString;
                        break;
                }
            }
        }

        public S2ReadMMATChunk(DecryptByteStream readFile, bool xml)
        {
            XmlTextReader xmlDoc = new XmlTextReader(new StringReader(Encoding.UTF8.GetString(readFile.GetEntireStream())));
            bool inDesc = false;
            string inAttrDesc = "";
            while (xmlDoc.Read())
            {
                if (xmlDoc.NodeType == XmlNodeType.Element)
                {
                    if (xmlDoc.Name == "AnyString") inDesc = true;
                    if (xmlDoc.Name == "AnyUint32") inDesc = true;
                }
                if (xmlDoc.NodeType == XmlNodeType.EndElement)
                {
                    inDesc = false;
                    inAttrDesc = "";
                }
                if (inDesc == true)
                {
                    if (xmlDoc.AttributeCount > 0)
                    {
                        while (xmlDoc.MoveToNextAttribute())
                        {
                            switch (xmlDoc.Value.ToLower())
                            {
                                case "creator":
                                case "defaultmaterial":
                                case "family":
                                case "flags":
                                case "materialstateflags":
                                case "modelname":
                                case "name":
                                case "objectguid":
                                case "objectstateindex":
                                case "subsetname":
                                case "type":
                                    inAttrDesc = xmlDoc.Value;
                                    break;
                            }
                        }
                    }
                }
                if (xmlDoc.NodeType == XmlNodeType.Text)
                {
                    if (inAttrDesc != "")
                    {
                        switch (inAttrDesc.ToLower())
                        {
                            case "creator":
                                MMATData.Creator = xmlDoc.Value;
                                break;
                            case "defaultmaterial":
                                MMATData.SetDefaultMaterial(xmlDoc.Value);
                                break;
                            case "family":
                                MMATData.Family = xmlDoc.Value;
                                break;
                            case "flags":
                                MMATData.Flags = xmlDoc.Value;
                                break;
                            case "materialstateflags":
                                MMATData.MaterialStateFlags = xmlDoc.Value;
                                break;
                            case "modelname":
                                MMATData.ModelName = xmlDoc.Value;
                                break;
                            case "name":
                                MMATData.Name = xmlDoc.Value;
                                break;
                            case "objectguid":
                                MMATData.ObjectGUID = xmlDoc.Value;
                                break;
                            case "objectstateindex":
                                MMATData.ObjectStateIndex = xmlDoc.Value;
                                break;
                            case "subsetname":
                                MMATData.SubsetName = xmlDoc.Value;
                                break;
                            case "type":
                                MMATData.Type = xmlDoc.Value;
                                break;
                        }
                    }
                }
            }
        }
    }

    public struct S2ReadGMDCChunk
    {
        public List<string> Identities = new()
        {
            "1C4AFC56",
            "5C4AFC5C",
            "7C4DEE82",
            "CB6F3A6A",
            "CB7206A1",
            "EB720693",
            "3B83078B",
            "5B830781",
            "BB8307AB",
            "DB830795",
            "9BB38AFB",
            "3BD70105",
            "FBD70111",
            "89D92BA0",
            "69D92B93",
            "5CF2CFE1",
            "DCF2CFDC",
            "114113C3",
            "114113CD"
        };
        public Dictionary<string, string> Types = new()
        {
            { "1C4AFC56", "Blend Indices" },
            { "5C4AFC5C", "Blend Weights" },
            { "7C4DEE82", "Target Indices" },
            { "CB6F3A6A", "Morph Normal Deltas" },
            { "CB7206A1", "Colour" },
            { "EB720693", "Colour Deltas" },
            { "3B83078B", "Normals List" },
            { "5B830781", "Vertices" },
            { "BB8307AB", "UV Coordinates" },
            { "DB830795", "UV Coordinate Deltas" },
            { "9BB38AFB", "Binormals" },
            { "3BD70105", "Skin (Bone) Weights" },
            { "FBD70111", "Bone Assignments" },
            { "89D92BA0", "Bump Map Normals" },
            { "69D92B93", "Bump Map Normal Deltas" },
            { "5CF2CFE1", "Morph Vertex Deltas" },
            { "DCF2CFDC", "Morph Vertex Map" },
            { "114113C3", "(EP4) VertexID" },
            { "114113CD", "(EP4) RegionMask" }
        };
        public List<uint> Verts = new();
        public List<IGMDCElement> IElements = new();
        public List<GMDCLinkage> Linkages = new();
        public List<GMDCGroup> Groups = new();
        public GMDCModel Model = new();
        public List<GMDCSubset> Subsets = new();
        public GMDCData GMDCData = new();

        uint Version;

        BinaryReader readFile;

        public S2ReadGMDCChunk(BinaryReader reader)
        {
            readFile = reader;
            uint language = readFile.ReadUInt16();
            uint stringstyle = readFile.ReadUInt16();
            uint repeatvalue = readFile.ReadUInt32();
            uint indexvalue = readFile.ReadUInt32();
            uint filetype = readFile.ReadUInt32();
            byte namelength = readFile.ReadByte();
            string gmdcn = Encoding.UTF8.GetString(readFile.ReadBytes(namelength));
            uint blockID = readFile.ReadUInt32();
            Version = readFile.ReadUInt32();
            byte resourcenamelength = readFile.ReadByte();
            string resourceName = Encoding.UTF8.GetString(readFile.ReadBytes(resourcenamelength));
            uint resourceID = readFile.ReadUInt32();
            uint resourceversion = readFile.ReadUInt32();
            byte filenamelength = readFile.ReadByte();
            string filename = Encoding.UTF8.GetString(readFile.ReadBytes(filenamelength));



            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Version: {0}. FileName: {1}", Version, filename));
            // all above works
            //most meshses use version 4, apparently


            uint NumRecs = readFile.ReadUInt32();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("NumRecs:{0}", NumRecs));

            for (int i = 0; i < NumRecs; i++)
            {
                GMDCElement element = new();
                element.Index = i;
                //uint number = readFile.ReadUInt32();
                element.Number = readFile.ReadUInt32();
                element.Identity = readFile.ReadUInt32().ToString("X8");
                if (Identities.Contains(element.Identity))
                {
                    Types.TryGetValue(element.Identity, out string Type);
                    element.IdentityName = Type;
                }
                uint RepeatCount = readFile.ReadUInt32();
                //readFile.ReadUInt32();
                element.BlockFormat = readFile.ReadUInt32();
                element.SetFormat = readFile.ReadUInt32();



                element.BlockSize = readFile.ReadUInt32();
                element.ElementLocation = readFile.BaseStream.Position;

                if (element.BlockSize != 0)
                {
                    var e = ReadElement(element);
                    IElements.Add(e);
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(e.ToString());
                }
                else
                {
                    element.ItemCount = readFile.ReadUInt32();
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(element.ToString());
                    IElements.Add(element);
                }


                //readFile.ReadBytes((int)element.BlockSize);

                //

                //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Identity: {0}, Block Format: {1}, Set Format: {2}, BlockSize: {3}, ItemCount: {4}", element.Identity, element.BlockFormat, element.SetFormat, element.BlockSize, element.ItemCount));


            }

            //get linkages

            uint NumLinkages = readFile.ReadUInt32();
            for (int i = 0; i < NumLinkages; i++)
            {
                GMDCLinkage linkage = new();
                linkage.IndexCount = readFile.ReadUInt32();
                //linkage.BlockStart = readFile.BaseStream.Position;

                for (int c = 0; c < linkage.IndexCount; c++)
                {
                    if (Version == 4)
                    {
                        linkage.IndexValues.Add(readFile.ReadUInt16());
                    }
                    else
                    {
                        linkage.IndexValues.Add(readFile.ReadUInt32());
                    }
                }

                linkage.ArraySize = readFile.ReadUInt32();
                linkage.ActiveElements = readFile.ReadUInt32();
                linkage.SubmodelVertexCount = readFile.ReadUInt32();

                for (int c = 0; c < linkage.SubmodelVertexCount; c++)
                {
                    if (Version == 4)
                    {
                        linkage.SubmodelIndexValues.Add(readFile.ReadUInt16());
                    }
                    else
                    {
                        linkage.SubmodelIndexValues.Add(readFile.ReadUInt32());
                    }
                }

                linkage.SubmodelNormalsCount = readFile.ReadUInt32();

                for (int c = 0; c < linkage.SubmodelNormalsCount; c++)
                {
                    if (Version == 4)
                    {
                        linkage.NormalsIndexValues.Add(readFile.ReadUInt16());
                    }
                    else
                    {
                        linkage.NormalsIndexValues.Add(readFile.ReadUInt32());
                    }
                }
                linkage.SubmodelUVCount = readFile.ReadUInt32();

                for (int c = 0; c < linkage.SubmodelNormalsCount; c++)
                {
                    if (Version == 4)
                    {
                        linkage.UVIndexValues.Add(readFile.ReadUInt16());
                    }
                    else
                    {
                        linkage.UVIndexValues.Add(readFile.ReadUInt32());
                    }
                }
                Linkages.Add(linkage);
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}", linkage.ToString()));

            }

            //get groups

            uint groupCount = readFile.ReadUInt32();

            for (int g = 0; g < groupCount; g++)
            {
                GMDCGroup group = new();
                group.PrimitiveType = readFile.ReadUInt32();
                group.LinkIndex = readFile.ReadUInt32();
                byte objectNameLength = readFile.ReadByte();
                group.ObjectName = Encoding.UTF8.GetString(readFile.ReadBytes(objectNameLength));
                group.FaceCount = readFile.ReadUInt32();
                int gg = 0;
                Godot.Vector3 face = new();
                for (int fc = 0; fc < group.FaceCount; fc++)
                {
                    if (Version == 4)
                    {
                        if (gg == 0)
                        {
                            face.X = readFile.ReadUInt16();
                        }
                        else if (gg == 1)
                        {
                            face.Y = readFile.ReadUInt16();
                        }
                        else if (gg == 2)
                        {
                            face.Z = readFile.ReadUInt16();
                        }
                    }
                    else
                    {
                        if (gg == 0)
                        {
                            face.X = readFile.ReadUInt32();
                        }
                        else if (gg == 1)
                        {
                            face.Y = readFile.ReadUInt32();
                        }
                        else if (gg == 2)
                        {
                            face.Z = readFile.ReadUInt32();
                        }
                    }
                    if (gg == 2)
                    {
                        group.Faces.Add(face);
                        gg = 0;
                    }
                    else
                    {
                        gg++;
                    }
                }

                group.OpacityAmount = readFile.ReadUInt32().ToString("X8");
                if (Version != 1)
                {
                    uint subsetCount = readFile.ReadUInt32();
                    for (int s = 0; s < subsetCount; s++)
                    {
                        if (Version == 4)
                        {
                            group.ModelSubsetReference.Add(readFile.ReadUInt16());
                        }
                        else
                        {
                            group.ModelSubsetReference.Add(readFile.ReadUInt32());
                        }
                    }
                }
                Groups.Add(group);
            }

            /*uint blockcount = readFile.ReadUInt32();
            for (int i = 0; i < blockcount; i++)
            {
                TransformMatrix matrix = new();
                matrix.QuaternionX = readFile.ReadUInt32();
                matrix.QuaternionY = readFile.ReadUInt32();
                matrix.QuaternionZ = readFile.ReadUInt32();
                matrix.QuaternionW = readFile.ReadUInt32();
                matrix.TransformX = readFile.ReadUInt32();
                matrix.TransformY = readFile.ReadUInt32();
                matrix.TransformZ = readFile.ReadUInt32();
                Model.TransformMatrices.Add(matrix);
            }

            uint namepaircount = readFile.ReadUInt32();
            for (int i = 0; i < namepaircount; i++)
            {
                NamePair np = new();
                byte length = readFile.ReadByte();
                np.BlendGroupName = Encoding.UTF8.GetString(readFile.ReadBytes(length)); 
                length = readFile.ReadByte();
                np.AssignedElementName = Encoding.UTF8.GetString(readFile.ReadBytes(length)); 
                Model.NamedPairs.Add(np);
            }

            
            uint vertcount = readFile.ReadUInt32();
            if (vertcount > 0)
            {
                uint facecount = readFile.ReadUInt32();
                for (int i = 0; i < vertcount; i++)                
                {
                    Godot.Vector3 v3 = new();
                    v3.X = readFile.ReadUInt32();
                    v3.Y = readFile.ReadUInt32();
                    v3.Z = readFile.ReadUInt32();
                    Model.Verts.Add(v3);                    
                }
                for (int i = 0; i < facecount; i++)
                {
                    if (Version == 4)
                    {
                        Model.Faces.Add(readFile.ReadUInt16());
                    } else
                    {
                        Model.Faces.Add(readFile.ReadUInt32());
                    }
                    
                }
            }

            uint subsetcount = readFile.ReadUInt32();

            for (int i = 0; i < subsetcount; i++)
            {
                GMDCSubset subset = new();
                uint vertexcount = readFile.ReadUInt32();
                if (vertexcount > 0)
                {
                    uint facecount = readFile.ReadUInt32();
                    for (int v = 0; v < vertcount; v++)                
                    {
                        Godot.Vector3 v3 = new();
                        v3.X = readFile.ReadUInt32();
                        v3.Y = readFile.ReadUInt32();
                        v3.Z = readFile.ReadUInt32();
                        subset.Verts.Add(v3);                    
                    }
                    for (int f = 0; f < facecount; f++)
                    {
                        if (Version == 4)
                        {
                            subset.Faces.Add(readFile.ReadUInt16());
                        } else
                        {
                            subset.Faces.Add(readFile.ReadUInt32());
                        }
                        
                    }
                }
                Subsets.Add(subset);
            }*/

            GMDCData.Groups = Groups;
            GMDCData.ToElements(IElements);
            GMDCData.Linkages = Linkages;
            GMDCData.Model = Model;
            GMDCData.Subsets = Subsets;
        }

        private IGMDCElement ReadItems(IGMDCElement element)
        {
            if (element.ItemCount > 0)
            {
                for (int i = 0; i < element.ItemCount; i++)
                {
                    if (Version == 4)
                    {
                        readFile.ReadUInt16();
                    }
                    else
                    {
                        readFile.ReadUInt32();
                    }
                }
            }
            return element;
        }

        private IGMDCElement ReadElement(GMDCElement element)
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Identity: {0}, Block Format: {1}, Set Format: {2}, BlockSize: {3}, ItemCount: {4}, List Length: {5}, Length: {6}", element.Identity, element.BlockFormat, element.SetFormat, element.BlockSize, element.ItemCount, element.ListLength, element.Length));
            if (element.BlockSize != 0)
            {
                switch (element.Identity)
                {
                    case "5B830781":
                        GMDCElementVertices element00 = new();
                        element00 = element.ConvertTo(element00);
                        element00.Vertices = ReadVector3Chunk(element);
                        element00.ItemCount = readFile.ReadUInt32();
                        return ReadItems(element00);
                    case "3B83078B":

                        GMDCElementNormals element0 = new();
                        element0 = element.ConvertTo(element0);
                        element0.Normals = ReadVector3Chunk(element);
                        element0.ItemCount = readFile.ReadUInt32();
                        return ReadItems(element0);
                    case "BB8307AB":
                        GMDCElementUVCoordinates element1 = new();
                        element1 = element.ConvertTo(element1);
                        element1.UVCoordinates = ReadVector2Chunk(element);
                        element1.ItemCount = readFile.ReadUInt32();
                        return ReadItems(element1);
                    case "DB830795":
                        //ignore
                        break;
                    case "7C4DEE82":
                        GMDCElementTargetIndices element2 = new();
                        element2 = element.ConvertTo(element2);
                        element2.TargetIndices = ReadVector3Chunk(element);
                        element2.ItemCount = readFile.ReadUInt32();
                        return ReadItems(element2);
                    case "FBD70111":
                        GMDCElementBoneAssignments element3 = new();
                        element3 = element.ConvertTo(element3);
                        element3.BoneAssignments = ReadUIntChunk(element);
                        element3.ItemCount = readFile.ReadUInt32();
                        return ReadItems(element3);
                    case "3BD70105":
                        switch (element.Length)
                        {
                            case 1:
                                GMDCElementSkinV1 element4 = new();
                                element4 = element.ConvertTo(element4);
                                element4.SkinV1 = ReadUIntChunk(element);
                                element4.ItemCount = readFile.ReadUInt32();
                                return ReadItems(element4);
                            case 2:
                                GMDCElementSkinV2 element5 = new();
                                element5 = element.ConvertTo(element5);
                                element5.SkinV2 = ReadVector2Chunk(element);
                                element5.ItemCount = readFile.ReadUInt32();
                                return ReadItems(element5);
                            case 3:
                                GMDCElementSkinV3 element6 = new();
                                element6 = element.ConvertTo(element6);
                                element6.SkinV3 = ReadVector3Chunk(element);
                                element6.ItemCount = readFile.ReadUInt32();
                                return ReadItems(element6);
                        }
                        break;
                    case "5CF2CFE1":
                        GMDCElementMorphVertexDeltas element7 = new();
                        element7 = element.ConvertTo(element7);
                        element7.MorphVertexDeltas = ReadVector3Chunk(element);
                        element7.ItemCount = readFile.ReadUInt32();
                        return ReadItems(element7);
                    case "CB6F3A6A":
                        GMDCElementMorphNormalDeltas element8 = new();
                        element8 = element.ConvertTo(element8);
                        element8.MorphNormalDeltas = ReadVector3Chunk(element);
                        element8.ItemCount = readFile.ReadUInt32();
                        return ReadItems(element8);
                    case "DCF2CFDC":
                        GMDCElementMorphVertexMap element9 = new();
                        element9 = element.ConvertTo(element9);
                        element9.MorphVertexMap = ReadUIntChunk(element);
                        element9.ItemCount = readFile.ReadUInt32();
                        return ReadItems(element9);
                    case "89D92BA0":
                        GMDCElementBumpMapNormals element10 = new();
                        element10 = element.ConvertTo(element10);
                        element10.BumpMapNormals = ReadVector3Chunk(element);
                        element10.ItemCount = readFile.ReadUInt32();
                        return ReadItems(element10);
                }
            }
            readFile.ReadUInt32();
            return new GMDCElement();
        }


        private List<Godot.Vector3> ReadVector3Chunk(IGMDCElement element)
        {
            List<Godot.Vector3> vector3list = new();
            for (int l = 0; l < element.ListLength; l++)
            {
                Godot.Vector3 vector3 = new();
                for (int j = 0; j < element.Length; j++)
                {
                    if (element.BlockFormat == 4)
                    {
                        switch (j)
                        {
                            case 0:
                                vector3.X = readFile.ReadInt32();
                                break;
                            case 1:
                                vector3.Y = readFile.ReadInt32();
                                break;
                            case 2:
                                vector3.Z = readFile.ReadInt32();
                                break;
                        }

                    }
                    else
                    {
                        switch (j)
                        {
                            case 0:
                                //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pos before reading Single: {0}", readFile.BaseStream.Position));
                                vector3.X = float.Parse(readFile.ReadSingle().ToString("0.######"));
                                //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Pos after reading Single: {0}", readFile.BaseStream.Position));

                                break;
                            case 1:
                                vector3.Y = float.Parse(readFile.ReadSingle().ToString("0.######"));
                                break;
                            case 2:
                                vector3.Z = float.Parse(readFile.ReadSingle().ToString("0.######"));
                                break;
                        }
                    }
                }
                vector3list.Add(vector3);
            }
            return vector3list;
        }

        private List<Godot.Vector2> ReadVector2Chunk(IGMDCElement element)
        {
            List<Godot.Vector2> vector2list = new();
            for (int l = 0; l < element.ListLength; l++)
            {
                Godot.Vector2 vector2 = new();
                for (int j = 0; j < element.Length; j++)
                {
                    if (element.BlockFormat == 4)
                    {
                        switch (j)
                        {
                            case 0:
                                vector2.X = readFile.ReadInt32();
                                break;
                            case 1:
                                vector2.Y = readFile.ReadInt32();
                                break;
                        }

                    }
                    else
                    {
                        switch (j)
                        {
                            case 0:
                                vector2.X = readFile.ReadSingle();
                                break;
                            case 1:
                                vector2.Y = readFile.ReadSingle();
                                break;
                        }
                    }
                }
                vector2list.Add(vector2);
            }
            return vector2list;
        }

        private List<float> ReadUIntChunk(IGMDCElement element)
        {
            List<float> intlist = new();
            for (int l = 0; l < element.ListLength; l++)
            {
                float intdata = new();
                for (int j = 0; j < element.Length; j++)
                {
                    if (element.BlockFormat == 4)
                    {
                        intdata = readFile.ReadInt32();
                    }
                    else
                    {

                        intdata = readFile.ReadSingle();
                    }
                }
                intlist.Add(intdata);
            }
            return intlist;
        }
    }

    public struct S2ReadXOBJChunk
    {
        public string Title;
        public string Description;
        public string Type = "";
        public string Subtype = "";
        public string Category = "";
        public string Subsort = "";
        public string ModelName;
        public string ObjectGUID;
        public string Creator;
        public string Age;
        public string Gender;

        public void DebugFinish()
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Type: {0}, Category: {1}, Subtype: {2}, Subsort: {3}", Type, Category, Subtype, Subsort));
        }
        public S2ReadXOBJChunk(BinaryReader readFile)
        {
            uint NumItems = readFile.ReadUInt32();
            // Read the items
            for (int i = 0; i < NumItems; i++)
            {
                // Get type of the item
                string dataType = readFile.ReadUInt32().ToString("X8");
                uint nameLength = readFile.ReadUInt32();
                string fieldName = Encoding.UTF8.GetString(readFile.ReadBytes((int)nameLength));

                uint fieldValueInt = 0;
                string fieldValueString = "";


                switch (dataType)
                {
                    // Int
                    case "EB61E4F7":
                        fieldValueInt = readFile.ReadUInt32();
                        break;
                    // Int #2 - Not Used
                    case "0C264712":
                        fieldValueInt = readFile.ReadUInt32();
                        break;
                    // String
                    case "0B8BEA18":
                        uint stringLength = readFile.ReadUInt32();
                        fieldValueString = Encoding.UTF8.GetString(readFile.ReadBytes((int)stringLength));
                        break;
                    // Float
                    case "ABC78708":
                        // Ignore for now
                        uint fieldValueFloat = readFile.ReadUInt32();
                        break;
                    // Boolean
                    case "CBA908E1":
                        bool fieldValueBool = readFile.ReadBoolean();
                        break;
                }

                switch (fieldName)
                {
                    case "name":
                        Title = fieldValueString;
                        break;
                    case "description":
                        Description = fieldValueString;
                        break;
                    case "type":
                        Type = fieldValueString;
                        break;
                    case "subtype":
                        Subtype = fieldValueInt.ToString();
                        break;
                    case "subsort":
                        Subtype = fieldValueInt.ToString();
                        break;
                    case "category":
                        Category = fieldValueInt.ToString();
                        break;
                    case "modelName":
                        ModelName = fieldValueString;
                        break;
                    case "objectGUID":
                        ObjectGUID = fieldValueInt.ToString("X8");
                        break;
                    case "creator":
                        Creator = fieldValueString;
                        break;
                    case "age":
                        Age = fieldValueInt.ToString();
                        break;
                    case "gender":
                        Gender = fieldValueInt.ToString();
                        break;
                }
            }
            DebugFinish();
        }

        public S2ReadXOBJChunk(DecryptByteStream readFile)
        {
            readFile.ReadUInt16();
            uint NumItems = readFile.ReadUInt32();
            // Read the items
            for (int i = 0; i < NumItems; i++)
            {
                // Get type of the item
                string dataType = readFile.ReadUInt32().ToString("X8");
                uint nameLength = readFile.ReadUInt32();
                string fieldName = Encoding.UTF8.GetString(readFile.ReadBytes(nameLength));

                uint fieldValueInt = 0;
                string fieldValueString = "";


                switch (dataType)
                {
                    // Int
                    case "EB61E4F7":
                        fieldValueInt = readFile.ReadUInt32();
                        break;
                    // Int #2 - Not Used
                    case "0C264712":
                        fieldValueInt = readFile.ReadUInt32();
                        break;
                    // String
                    case "0B8BEA18":
                        uint stringLength = readFile.ReadUInt32();
                        fieldValueString = Encoding.UTF8.GetString(readFile.ReadBytes(stringLength));
                        break;
                    // Float
                    case "ABC78708":
                        // Ignore for now
                        uint fieldValueFloat = readFile.ReadUInt32();
                        break;
                    // Boolean
                    case "CBA908E1":
                        bool fieldValueBool = readFile.ReadBoolean();
                        break;
                }

                switch (fieldName)
                {
                    case "name":
                        Title = fieldValueString;
                        break;
                    case "description":
                        Description = fieldValueString;
                        break;
                    case "type":
                        Type = fieldValueString;
                        break;
                    case "subtype":
                        Subtype = fieldValueInt.ToString();
                        break;
                    case "subsort":
                        Subtype = fieldValueInt.ToString();
                        break;
                    case "category":
                        Category = fieldValueInt.ToString();
                        break;
                    case "modelName":
                        ModelName = fieldValueString;
                        break;
                    case "objectGUID":
                        ObjectGUID = fieldValueInt.ToString("X8");
                        break;
                    case "creator":
                        Creator = fieldValueString;
                        break;
                    case "age":
                        Age = fieldValueInt.ToString();
                        break;
                    case "gender":
                        Gender = fieldValueInt.ToString();
                        break;
                }
            }
            DebugFinish();
        }

        public S2ReadXOBJChunk(DecryptByteStream readFile, bool xml)
        {
            XmlTextReader xmlDoc = new XmlTextReader(new StringReader(Encoding.UTF8.GetString(readFile.GetEntireStream())));
            bool inDesc = false;
            string inAttrDesc = "";
            while (xmlDoc.Read())
            {
                if (xmlDoc.NodeType == XmlNodeType.Element)
                {
                    if (xmlDoc.Name == "AnyString") inDesc = true;
                    if (xmlDoc.Name == "AnyUint32") inDesc = true;
                }
                if (xmlDoc.NodeType == XmlNodeType.EndElement)
                {
                    inDesc = false;
                    inAttrDesc = "";
                }
                if (inDesc == true)
                {
                    if (xmlDoc.AttributeCount > 0)
                    {
                        while (xmlDoc.MoveToNextAttribute())
                        {
                            switch (xmlDoc.Value)
                            {
                                case "description":
                                case "type":
                                case "name":
                                case "subsort":
                                case "subtype":
                                case "category":
                                    inAttrDesc = xmlDoc.Value;
                                    break;
                            }
                        }
                    }
                }
                if (xmlDoc.NodeType == XmlNodeType.Text)
                {
                    if (inAttrDesc != "")
                    {
                        switch (inAttrDesc)
                        {
                            case "subtype":
                                Subtype = xmlDoc.Value;
                                break;
                            case "subsort":
                                Subsort = xmlDoc.Value;
                                break;
                            case "category":
                                Category = xmlDoc.Value;
                                break;
                            case "name":
                                Title = xmlDoc.Value;
                                break;
                            case "type":
                                Type = xmlDoc.Value;
                                break;
                            case "description":
                                Description = xmlDoc.Value.Replace("\n", " ");
                                break;
                        }
                    }
                }

            }
            DebugFinish();
        }

        public override string ToString()
        {
            return string.Format("Title: {0}, Description: {1}, Type: {2}, Subtype: {3}, Category: {4}, Subsort: {5}, ModelName: {6}, ObjectGUID: {7}, Creator: {8}, Age: {9}, Gender: {10}", Title, Description, Type, Subtype, Category, Subsort, ModelName, ObjectGUID, Creator, Age, Gender);
        }
    }

    public struct S2ReadOBJDChunk
    {
        public string ObjectGUID;
        public byte[] FileName;

        public List<FunctionSortList> FunctionSort = new();

        public uint Version;
        public uint InitialStackSize;
        public uint DefaultWallAdjacentFlags;
        public uint DefaultPlacementFlags;
        public uint DefaultWallPlacementFlags;
        public uint DefaultAllowedHeightFlags;
        public uint InteractionTableID;
        public uint InteractionGroup;
        public uint ObjectType;
        public uint MasterTileMasterId;
        public uint MasterTileSubIndex;
        public uint UseDefaultPlacementFlags;
        public uint LookAtScore;
        public uint RoomSortFlag;
        public uint ExpansionFlag;
        public string OriginalGUID;
        public string ObjectModelGUID;


        public S2ReadOBJDChunk(BinaryReader readFile)
        {
            FileName = readFile.ReadBytes(64);
            Version = readFile.ReadUInt32();
            InitialStackSize = readFile.ReadUInt16();
            DefaultWallAdjacentFlags = readFile.ReadUInt16();
            DefaultPlacementFlags = readFile.ReadUInt16();
            DefaultWallPlacementFlags = readFile.ReadUInt16();
            DefaultAllowedHeightFlags = readFile.ReadUInt16();
            InteractionTableID = readFile.ReadUInt16();
            InteractionGroup = readFile.ReadUInt16();
            ObjectType = readFile.ReadUInt16();
            MasterTileMasterId = readFile.ReadUInt16();
            MasterTileSubIndex = readFile.ReadUInt16();

            // Only check further if this is a Master ID or single id
            if ((MasterTileSubIndex == 65535) || (MasterTileMasterId == 0))
            {
                UseDefaultPlacementFlags = readFile.ReadUInt16();
                LookAtScore = readFile.ReadUInt16();
                ObjectGUID = readFile.ReadUInt32().ToString("X8");
                // Skip stuff we don't need
                readFile.ReadBytes(46);
                RoomSortFlag = readFile.ReadUInt16();
                int[] functionSortFlag = new int[1];
                functionSortFlag[0] = (int)readFile.ReadUInt16();
                BitArray functionSortFlags = new BitArray(functionSortFlag);

                int fsfn = 0;
                foreach (var fsf in functionSortFlags)
                {
                    fsfn++;
                }
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Function sort flag: {0}", fsfn));


                // No function sort, check Build Mode Sort
                if (functionSortFlag[0] == 0)
                {
                    // Skip until we hit the Build Mode sort and EP
                    readFile.ReadBytes(46);
                    ExpansionFlag = readFile.ReadUInt16();

                    readFile.ReadBytes(8);
                    uint BuildModeType = readFile.ReadUInt16();
                    OriginalGUID = readFile.ReadUInt32().ToString("X8");
                    ObjectModelGUID = readFile.ReadUInt32().ToString("X8");
                    uint BuildModeSubsort = readFile.ReadUInt16();

                    if (Sims2PackageStatics.Sims2BuildFunctionSortList.Any(x => x.FlagNum == BuildModeType && x.FunctionSubsortNum == BuildModeSubsort))
                    {
                        FunctionSort.Add(Sims2PackageStatics.Sims2BuildFunctionSortList.First(x => x.FlagNum == BuildModeType && x.FunctionSubsortNum == BuildModeSubsort));
                    }
                    else
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("NOT RECOGNIZED: BUILD MODE - Type: {0}, Subsort: {1}", BuildModeType, BuildModeSubsort));
                    }

                }
                else
                {
                    readFile.ReadBytes(46);
                    uint ExpansionFlag = readFile.ReadUInt16();

                    readFile.ReadBytes(8);
                    uint BuildModeType = readFile.ReadUInt16();
                    OriginalGUID = readFile.ReadUInt32().ToString("X8");
                    ObjectModelGUID = readFile.ReadUInt32().ToString("X8");
                    uint BuildModeSubsort = readFile.ReadUInt16();
                    readFile.ReadBytes(38);
                    uint FunctionSubsort = readFile.ReadUInt16();


                    for (int f = 0; f < functionSortFlags.Length; f++)
                    {
                        if (functionSortFlags[f]) // bitarray might have 100 entries but not all of them are "true". we only want the "true" one. eg; number 5 is true, so the flagnum is 5.
                        {
                            if (Sims2PackageStatics.Sims2BuyFunctionSortList.Any(x => x.FlagNum == f && x.FunctionSubsortNum == FunctionSubsort))
                            {
                                FunctionSort.Add(Sims2PackageStatics.Sims2BuyFunctionSortList.First(x => x.FlagNum == f && x.FunctionSubsortNum == FunctionSubsort));
                            }
                            else
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("NOT RECOGNIZED: BUY MODE - Flag Num: {0}, Subsort: {1}", f, FunctionSubsort));
                            }
                        }
                    }
                }
            }
        }
        public S2ReadOBJDChunk(DecryptByteStream readFile)
        {
            FileName = readFile.ReadBytes(64);
            Version = readFile.ReadUInt32();
            InitialStackSize = readFile.ReadUInt16();
            DefaultWallAdjacentFlags = readFile.ReadUInt16();
            DefaultPlacementFlags = readFile.ReadUInt16();
            DefaultWallPlacementFlags = readFile.ReadUInt16();
            DefaultAllowedHeightFlags = readFile.ReadUInt16();
            InteractionTableID = readFile.ReadUInt16();
            InteractionGroup = readFile.ReadUInt16();
            ObjectType = readFile.ReadUInt16();
            MasterTileMasterId = readFile.ReadUInt16();
            MasterTileSubIndex = readFile.ReadUInt16();

            // Only check further if this is a Master ID or single id
            if ((MasterTileSubIndex == 65535) || (MasterTileMasterId == 0))
            {
                UseDefaultPlacementFlags = readFile.ReadUInt16();
                LookAtScore = readFile.ReadUInt16();
                ObjectGUID = readFile.ReadUInt32().ToString("X8");
                // Skip stuff we don't need
                readFile.ReadBytes(46);
                RoomSortFlag = readFile.ReadUInt16();
                int[] functionSortFlag = new int[1];
                functionSortFlag[0] = (int)readFile.ReadUInt16();
                BitArray functionSortFlags = new BitArray(functionSortFlag);

                int fsfn = 0;
                foreach (var fsf in functionSortFlags)
                {
                    fsfn++;
                }
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Function sort flag: {0}", fsfn));


                // No function sort, check Build Mode Sort
                if (functionSortFlag[0] == 0)
                {
                    // Skip until we hit the Build Mode sort and EP
                    readFile.ReadBytes(46);
                    ExpansionFlag = readFile.ReadUInt16();

                    readFile.ReadBytes(8);
                    uint BuildModeType = readFile.ReadUInt16();
                    OriginalGUID = readFile.ReadUInt32().ToString("X8");
                    ObjectModelGUID = readFile.ReadUInt32().ToString("X8");
                    uint BuildModeSubsort = readFile.ReadUInt16();

                    if (Sims2PackageStatics.Sims2BuildFunctionSortList.Any(x => x.FlagNum == BuildModeType && x.FunctionSubsortNum == BuildModeSubsort))
                    {
                        FunctionSort.Add(Sims2PackageStatics.Sims2BuildFunctionSortList.First(x => x.FlagNum == BuildModeType && x.FunctionSubsortNum == BuildModeSubsort));
                    }
                    else
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("NOT RECOGNIZED: BUILD MODE - Type: {0}, Subsort: {1}", BuildModeType, BuildModeSubsort));
                    }

                }
                else
                {
                    readFile.ReadBytes(46);
                    uint ExpansionFlag = readFile.ReadUInt16();

                    readFile.ReadBytes(8);
                    uint BuildModeType = readFile.ReadUInt16();
                    OriginalGUID = readFile.ReadUInt32().ToString("X8");
                    ObjectModelGUID = readFile.ReadUInt32().ToString("X8");
                    uint BuildModeSubsort = readFile.ReadUInt16();
                    readFile.ReadBytes(38);
                    uint FunctionSubsort = readFile.ReadUInt16();

                    for (int f = 0; f < functionSortFlags.Length; f++)
                    {
                        if (functionSortFlags[f]) // bitarray might have 100 entries but not all of them are "true". we only want the "true" one. eg; number 5 is true, so the flagnum is 5.
                        {
                            if (Sims2PackageStatics.Sims2BuyFunctionSortList.Any(x => x.FlagNum == f && x.FunctionSubsortNum == FunctionSubsort))
                            {
                                FunctionSort.Add(Sims2PackageStatics.Sims2BuyFunctionSortList.First(x => x.FlagNum == f && x.FunctionSubsortNum == FunctionSubsort));
                            }
                            else
                            {
                                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("NOT RECOGNIZED: BUY MODE - Flag Num: {0}, Subsort: {1}", f, FunctionSubsort));
                            }
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            string name = CleanInput(Encoding.UTF8.GetString(FileName));
            StringBuilder sorts = new();
            sorts.Append(string.Format("OBJD Name: {0}, ", name));
            sorts.Append(string.Format("ObjectGUID: {0}, ", ObjectGUID));
            sorts.Append(string.Format("Function: "));
            foreach (FunctionSortList sort in FunctionSort)
            {
                if (string.IsNullOrEmpty(sort.Subcategory))
                {
                    sorts.AppendLine(string.Format("{0}", sort.Category));
                }
                else
                {
                    sorts.AppendLine(string.Format("{0}/{1}", sort.Category, sort.Subcategory));
                }

            }
            return sorts.ToString();
        }

        static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"[^\s\w\.@-]", "",
                                        RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters,
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }
    }

    public struct S2ReadCTSSChunk
    {
        public string Description;
        public string Title;

        public S2ReadCTSSChunk(BinaryReader readFile)
        {
            readFile.ReadBytes(64);
            readFile.ReadUInt16();

            uint numStrings = readFile.ReadUInt16();
            bool foundLang = false;

            for (int k = 0; k < numStrings; k++)
            {
                int langCode = Convert.ToInt32(readFile.ReadByte().ToString());

                string blah = Sims2EntryReaders.ReadNullString(readFile);
                string meep = Sims2EntryReaders.ReadNullString(readFile);

                if (langCode == 1)
                {
                    if (foundLang == true && !string.IsNullOrEmpty(blah)) { Description = blah.Replace("\n", " "); break; }
                    if (foundLang == false && !string.IsNullOrEmpty(blah)) { Title = blah.Replace("\n", " "); foundLang = true; }
                }

            }
        }

        public S2ReadCTSSChunk(DecryptByteStream readFile)
        {
            readFile.SkipAhead(66);

            uint numStrings = readFile.ReadUInt16();
            bool foundLang = false;

            for (int k = 0; k < numStrings; k++)
            {
                byte[] langCode = readFile.ReadBytes(1);

                string blah = readFile.GetNullString();
                string meep = readFile.GetNullString();

                if (langCode[0] == 1)
                {
                    if (foundLang == true && !string.IsNullOrEmpty(blah)) { Description = blah.Replace("\n", " "); break; }
                    if (foundLang == false && !string.IsNullOrEmpty(blah)) { Title = blah.Replace("\n", " "); foundLang = true; }
                }

            }
        }

        public override string ToString()
        {
            return string.Format("Title: {0}, Description: {1}", Title, Description);
        }
    }

    public class DecryptByteStream
    {
        private int currOffset = 0;
        private byte[] byteStream;
        public BinaryReader reader;

        public DecryptByteStream(byte[] inputBytes)
        {
            byteStream = inputBytes;

            reader = new(new MemoryStream(inputBytes));
        }

        public int Offset
        {
            get { return currOffset; }
            set { currOffset = value; }
        }

        public void SkipAhead(int numToSkip)
        {
            this.Offset += numToSkip;
        }

        public string GetNullString()
        {
            string result = "";
            char c;
            for (int i = 0; i < byteStream.Length; i++)
            {
                if ((c = (char)byteStream[currOffset]) == 0) { currOffset++; break; }
                result += c.ToString();
                currOffset++;
            }

            return result;
        }

        public byte ReadByte()
        {
            byte result = new byte();
            if (currOffset > byteStream.Length) return result;
            result = byteStream[currOffset];
            currOffset++;
            return result;
        }

        public byte[] ReadBytes(uint count)
        {
            byte[] result = new byte[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = byteStream[currOffset];
                currOffset++;
                if (currOffset > byteStream.Length) return result;
            }
            return result;
        }

        public uint ReadUInt32()
        {
            uint power = 1;
            uint result = 0;

            for (int i = 0; i < 4; i++)
            {
                if (currOffset > byteStream.Length) return result;
                result += (byteStream[currOffset] * power);
                power = power * 256;
                currOffset++;
            }
            //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("UInt32 Result: {0}", result));
            return result;
        }

        public uint ReadUInt16()
        {
            uint power = 1;
            uint result = 0;

            for (int i = 0; i < 2; i++)
            {
                if (currOffset > byteStream.Length) return result;
                result += (byteStream[currOffset] * power);
                power = power * 256;
                currOffset++;
            }
            return result;
        }

        public bool ReadBoolean()
        {
            bool result = false;
            if (currOffset > byteStream.Length) return result;

            byte temp = byteStream[currOffset];
            currOffset++;

            if (temp == 1) { result = true; }
            else { result = false; }

            return result;
        }

        public string ReadString()
        {
            string result = "";
            //Get the first byte containing the string length
            if (currOffset > byteStream.Length) return result;

            byte stringLength = byteStream[currOffset];
            currOffset++;

            //Does the string start with 01?
            if (byteStream[currOffset] == 1) currOffset++;

            //get the length of the string
            result = Encoding.UTF8.GetString(ReadBytes((uint)stringLength));
            return result;
        }

        public byte[] GetEntireStream()
        {
            return byteStream;
        }
    }

    public class Sims2EntryReaders
    {
        public static uint QFSLengthToInt(byte[] data)
        {
            // Converts a 3 byte length to a uint
            uint power = 1;
            uint result = 0;
            for (int i = data.Length; i > 0; i--)
            {
                result += (data[i - 1] * power);
                power = power * 256;
            }

            return result;
        }

        public static byte[] Uncompress(byte[] data, uint targetSize, int offset)
        {
            byte[] uncdata = null;
            int index = offset;

            try
            {
                uncdata = new Byte[targetSize];
            }
            catch (Exception)
            {
                uncdata = new Byte[0];
            }

            int uncindex = 0;
            int plaincount = 0;
            int copycount = 0;
            int copyoffset = 0;
            Byte cc = 0;
            Byte cc1 = 0;
            Byte cc2 = 0;
            Byte cc3 = 0;
            int source;

            try
            {
                while ((index < data.Length) && (data[index] < 0xfc))
                {
                    cc = data[index++];

                    if ((cc & 0x80) == 0)
                    {
                        cc1 = data[index++];
                        plaincount = (cc & 0x03);
                        copycount = ((cc & 0x1C) >> 2) + 3;
                        copyoffset = ((cc & 0x60) << 3) + cc1 + 1;
                    }
                    else if ((cc & 0x40) == 0)
                    {
                        cc1 = data[index++];
                        cc2 = data[index++];
                        plaincount = (cc1 & 0xC0) >> 6;
                        copycount = (cc & 0x3F) + 4;
                        copyoffset = ((cc1 & 0x3F) << 8) + cc2 + 1;
                    }
                    else if ((cc & 0x20) == 0)
                    {
                        cc1 = data[index++];
                        cc2 = data[index++];
                        cc3 = data[index++];
                        plaincount = (cc & 0x03);
                        copycount = ((cc & 0x0C) << 6) + cc3 + 5;
                        copyoffset = ((cc & 0x10) << 12) + (cc1 << 8) + cc2 + 1;
                    }
                    else
                    {
                        plaincount = (cc - 0xDF) << 2;
                        copycount = 0;
                        copyoffset = 0;
                    }

                    for (int i = 0; i < plaincount; i++) uncdata[uncindex++] = data[index++];

                    source = uncindex - copyoffset;
                    for (int i = 0; i < copycount; i++) uncdata[uncindex++] = uncdata[source++];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            if (index < data.Length)
            {
                plaincount = (data[index++] & 0x03);
                for (int i = 0; i < plaincount; i++)
                {
                    if (uncindex >= uncdata.Length) break;
                    uncdata[uncindex++] = data[index++];
                }
            }
            return uncdata;
        }

        public static string ReadNullString(BinaryReader reader)
        {
            string result = "";
            char c;
            for (int i = 0; i < reader.BaseStream.Length; i++)
            {
                if ((c = (char)reader.ReadByte()) == 0)
                {
                    break;
                }
                result += c.ToString();
            }
            return result;
        }
    }



    public class PackageReaderException : Exception
    {
        /// <summary>
        /// Custom exception reporting for the data grid.
        /// </summary>
        public PackageReaderException()
        {

        }

        public PackageReaderException(string message)
            : base(message)
        {
        }

        public PackageReaderException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }

    public class EntryType
    {
        /// <summary>
        /// For "types", for example Cas Parts or Geometry.
        /// </summary>
        public string Tag { get; set; }
        public string TypeID { get; set; }
        public string Description { get; set; }
    }

    public class FunctionSortList
    {
        /// <summary>
        /// Used for Sims 2 (I believe) function categorization. 
        /// </summary>
        public int FlagNum { get; set; }
        public int FunctionSubsortNum { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
    }

    public class IndexEntry
    {
        public string TypeID { get; set; }
        public string GroupID { get; set; }
        public string InstanceID { get; set; }
        private string _completeiddummy;
        public string CompleteID
        {
            get { return string.Format("{0}-{1}-{2}", TypeID, GroupID, InstanceID); }
            set { _completeiddummy = value; }
        }
        public string InstanceID2 { get; set; }
        public uint Offset { get; set; }
        public uint FileSize { get; set; }
        public uint UncompressedSize { get; set; }
        public bool IsCompressed { get; set; }
        public string Unused { get; set; }
        public int EntryIDX { get; set; }
        private string _entrytypedummy;

        public string EntryType
        {
            get
            {
                try
                {
                    return Sims2PackageStatics.Sims2EntryTypes.First(x => x.TypeID == TypeID).Tag;
                }
                catch
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("TypeID doesn't have a matching entry: {0}", TypeID));
                    return "UNKNOWN";
                }
            }
            set
            {
                _entrytypedummy = value;
            }
        }

        public override string ToString()
        {
            return string.Format("Index Entry: {0} - Location: {1}, FileSize: {2}, UncompressedSize: {3}, Compressed: {4}. Type: {5}", CompleteID, Offset, FileSize, UncompressedSize, IsCompressed, EntryType);
        }
    }

    public class ByteReaders
    {
        /// <summary>
        /// Repeatedly called methods.
        /// </summary>

        public static MemoryStream ReadBytesToFile(string file)
        {
            FileInfo f = new FileInfo(file);
            byte[] bit = new byte[f.Length];
            int bytes;
            using (FileStream fsSource = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                for (int w = 0; w < f.Length; w++)
                {
                    bit[w] = (byte)fsSource.ReadByte();
                }
                MemoryStream stream = new MemoryStream(bit);
                return stream;
            }
        }

        public static MemoryStream ReadBytesToFile(string file, int bytestoread)
        {
            byte[] bit = new byte[bytestoread];
            int bytes;
            using (FileStream fsSource = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                for (int w = 0; w < bytestoread; w++)
                {
                    bit[w] = (byte)fsSource.ReadByte();
                }
                MemoryStream stream = new MemoryStream(bit);
                return stream;
            }
        }

        public static Byte[] ReadEntryBytes(BinaryReader reader, int memSize)
        {
            byte[] bit = new byte[memSize];
            for (int w = 0; w < memSize; w++)
            {
                bit[w] = reader.ReadByte();
            }
            return bit;
        }
    }

    public class MMATData
    {
        public string Creator { get; set; }
        public bool DefaultMaterial { get; set; }
        public string Family { get; set; }
        public string Flags { get; set; }
        public string MaterialStateFlags { get; set; }
        public string ModelName { get; set; }
        public string Name { get; set; }
        public string ObjectGUID { get; set; } = "";
        public string ObjectStateIndex { get; set; }
        public string SubsetName { get; set; }
        public string Type { get; set; }

        public void SetDefaultMaterial(string boolString)
        {
            switch (boolString.ToLower())
            {
                case "true":
                    DefaultMaterial = true;
                    break;
                default:
                    DefaultMaterial = false;
                    break;
            }
        }
    }

    public class TXTRData
    {
        public string FullTXTRName { get; set; }
        public string GUID
        {
            get
            {
                if (FullTXTRName.Contains('!')) return FullTXTRName.Split('!')[0].TrimStart('#'); else return "N/a";
            }
        }
        public string TextureName
        {
            get
            {
                if (FullTXTRName.Contains('!')) return FullTXTRName.Split('!')[1]; else return "N/a";
            }
        }

        public uint TextureWidth { get; set; }
        public uint TextureHeight { get; set; }
        /*Image format codes: 
        0=??? 
        1=Raw A8 R8 G8 B8, total data size = width*height*4 
        2=Raw R8 G8 B8, total data size = width*height*3 
        3=??? 
        4=DXT1 RGB (no alpha bit) total data size = width*height/2 
        5=DXT3 ARGB, total data size = width*height 
        6=Raw Grayscale, total data size = width*height 
        7... = ???
        8 - DXT5*/

        public uint FormatCode { get; set; }
        public uint MipMapLevels { get; set; }
        private byte[] _imagedata;
        public byte[] ImageData
        {
            get { if (Texture != null) return Texture.SavePngToBuffer(); else return []; }
            set { if (value.Length != 0) Texture = DeserializeTexture(value); else _imagedata = value; }
        }
        [XmlIgnore]
        private Godot.Image _texture;
        [XmlIgnore]
        public Godot.Image Texture
        {
            get { return _texture; }
            set { _texture = ConvertTexture(value); }
        }

        private Godot.Image ConvertTexture(Godot.Image image)
        {
            if (image.GetFormat() != Godot.Image.Format.Rgb8)
            {
                if (image.IsCompressed())
                {
                    image.Decompress();
                }
                image.Convert(Godot.Image.Format.Rgb8);
            }
            else
            {
                if (!image.IsCompressed())
                {
                    image.Compress(Godot.Image.CompressMode.Astc);
                }
            }
            return image;
        }
        private Godot.Image DeserializeTexture(byte[] data)
        {
            Godot.Image image = new();
            image.LoadPngFromBuffer(data);
            return image;
        }
    }

    public class XFLRData
    {
        public uint BrushWidth { get; set; }
        public uint Cost { get; set; }
        public float CrapScore { get; set; }
        public int Depreciated { get; set; }
        public string Description { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public float NicenessMultiplier { get; set; }
        public uint ResourceGroupID { get; set; }
        public uint ResourceID { get; set; }
        public uint ResourceResTypeID { get; set; }
        public string ShowInCatalog { get; set; }
        public string SoundSuffix { get; set; } = "";
        public uint StringSetGroupID { get; set; }
        public uint StringSetID { get; set; }
        public uint StringSetResTypeID { get; set; }
        public string TextureTName { get; set; }
        public string Type { get; set; } = "";
        public uint Version { get; set; }

    }


    public interface IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                return BlockFormat switch
                {
                    4 => 1,//single float (4 byte length)
                    1 => 2,//paired floats (8 total bytes)
                    2 => 3,//triple floats (12 bytes total)
                    _ => 1,
                };
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public long ElementLocation { get; set; }
    }

    public class GMDCElement : IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                switch (BlockFormat)
                {
                    case 4:
                        //single float (4 byte length)
                        return 1;
                    case 1:
                        //paired floats (8 total bytes)
                        return 2;
                    case 2:
                        //triple floats (12 bytes total)
                        return 3;
                    default:
                        return 1;
                }
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public long ElementLocation { get; set; }

        public dynamic ConvertTo(dynamic type)
        {
            switch (type.GetType())
            {
                case Type VarGMDCElementVertices when VarGMDCElementVertices == typeof(GMDCElementVertices):
                    return new GMDCElementVertices()
                    {
                        Identity = this.Identity,
                        BlockFormat = this.BlockFormat,
                        SetFormat = this.SetFormat,
                        BlockSize = this.BlockSize,
                        ItemCount = this.ItemCount,
                        Index = this.Index,
                        ElementLocation = this.ElementLocation,
                        IdentityName = this.IdentityName,
                        Number = this.Number

                    };
                case Type VarGMDCElementNormals when VarGMDCElementNormals == typeof(GMDCElementNormals):
                    return new GMDCElementNormals()
                    {
                        Identity = this.Identity,
                        BlockFormat = this.BlockFormat,
                        SetFormat = this.SetFormat,
                        BlockSize = this.BlockSize,
                        ItemCount = this.ItemCount,
                        Index = this.Index,
                        ElementLocation = this.ElementLocation,
                        IdentityName = this.IdentityName,
                        Number = this.Number
                    };
                case Type VarGMDCElementUVCoordinates when VarGMDCElementUVCoordinates == typeof(GMDCElementUVCoordinates):
                    return new GMDCElementUVCoordinates()
                    {
                        Identity = this.Identity,
                        BlockFormat = this.BlockFormat,
                        SetFormat = this.SetFormat,
                        BlockSize = this.BlockSize,
                        ItemCount = this.ItemCount,
                        Index = this.Index,
                        ElementLocation = this.ElementLocation,
                        IdentityName = this.IdentityName,
                        Number = this.Number
                    };
                case Type VarGMDCElementTargetIndices when VarGMDCElementTargetIndices == typeof(GMDCElementTargetIndices):
                    return new GMDCElementTargetIndices()
                    {
                        Identity = this.Identity,
                        BlockFormat = this.BlockFormat,
                        SetFormat = this.SetFormat,
                        BlockSize = this.BlockSize,
                        ItemCount = this.ItemCount,
                        Index = this.Index,
                        ElementLocation = this.ElementLocation,
                        IdentityName = this.IdentityName,
                        Number = this.Number
                    };
                case Type VarGMDCElementBoneAssignments when VarGMDCElementBoneAssignments == typeof(GMDCElementBoneAssignments):
                    return new GMDCElementBoneAssignments()
                    {
                        Identity = this.Identity,
                        BlockFormat = this.BlockFormat,
                        SetFormat = this.SetFormat,
                        BlockSize = this.BlockSize,
                        ItemCount = this.ItemCount,
                        Index = this.Index,
                        ElementLocation = this.ElementLocation,
                        IdentityName = this.IdentityName,
                        Number = this.Number
                    };
                case Type VarGMDCElementSkinV1 when VarGMDCElementSkinV1 == typeof(GMDCElementSkinV1):
                    return new GMDCElementSkinV1()
                    {
                        Identity = this.Identity,
                        BlockFormat = this.BlockFormat,
                        SetFormat = this.SetFormat,
                        BlockSize = this.BlockSize,
                        ItemCount = this.ItemCount,
                        Index = this.Index,
                        ElementLocation = this.ElementLocation,
                        IdentityName = this.IdentityName,
                        Number = this.Number
                    };
                case Type VarGMDCElementSkinV2 when VarGMDCElementSkinV2 == typeof(GMDCElementSkinV2):
                    return new GMDCElementSkinV2()
                    {
                        Identity = this.Identity,
                        BlockFormat = this.BlockFormat,
                        SetFormat = this.SetFormat,
                        BlockSize = this.BlockSize,
                        ItemCount = this.ItemCount,
                        Index = this.Index,
                        ElementLocation = this.ElementLocation,
                        IdentityName = this.IdentityName,
                        Number = this.Number
                    };
                case Type VarGMDCElementSkinV3 when VarGMDCElementSkinV3 == typeof(GMDCElementSkinV3):
                    return new GMDCElementSkinV3()
                    {
                        Identity = this.Identity,
                        BlockFormat = this.BlockFormat,
                        SetFormat = this.SetFormat,
                        BlockSize = this.BlockSize,
                        ItemCount = this.ItemCount,
                        Index = this.Index,
                        ElementLocation = this.ElementLocation,
                        IdentityName = this.IdentityName,
                        Number = this.Number
                    };
                case Type VarGMDCElementMorphVertexDeltas when VarGMDCElementMorphVertexDeltas == typeof(GMDCElementMorphVertexDeltas):
                    return new GMDCElementMorphVertexDeltas()
                    {
                        Identity = this.Identity,
                        BlockFormat = this.BlockFormat,
                        SetFormat = this.SetFormat,
                        BlockSize = this.BlockSize,
                        ItemCount = this.ItemCount,
                        Index = this.Index,
                        ElementLocation = this.ElementLocation,
                        IdentityName = this.IdentityName,
                        Number = this.Number
                    };
                case Type VarGMDCElementMorphNormalDeltas when VarGMDCElementMorphNormalDeltas == typeof(GMDCElementMorphNormalDeltas):
                    return new GMDCElementMorphNormalDeltas()
                    {
                        Identity = this.Identity,
                        BlockFormat = this.BlockFormat,
                        SetFormat = this.SetFormat,
                        BlockSize = this.BlockSize,
                        ItemCount = this.ItemCount,
                        Index = this.Index,
                        ElementLocation = this.ElementLocation,
                        IdentityName = this.IdentityName,
                        Number = this.Number
                    };
                case Type VarGMDCElementMorphVertexMap when VarGMDCElementMorphVertexMap == typeof(GMDCElementMorphVertexMap):
                    return new GMDCElementMorphVertexMap()
                    {
                        Identity = this.Identity,
                        BlockFormat = this.BlockFormat,
                        SetFormat = this.SetFormat,
                        BlockSize = this.BlockSize,
                        ItemCount = this.ItemCount,
                        Index = this.Index,
                        ElementLocation = this.ElementLocation,
                        IdentityName = this.IdentityName,
                        Number = this.Number
                    };
                case Type VarGMDCElementBumpNorms when VarGMDCElementBumpNorms == typeof(GMDCElementBumpMapNormals):
                    return new GMDCElementBumpMapNormals()
                    {
                        Identity = this.Identity,
                        BlockFormat = this.BlockFormat,
                        SetFormat = this.SetFormat,
                        BlockSize = this.BlockSize,
                        ItemCount = this.ItemCount,
                        Index = this.Index,
                        ElementLocation = this.ElementLocation,
                        IdentityName = this.IdentityName,
                        Number = this.Number
                    };
                default:
                    return new GMDCElement();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(string.Format("Element Index: {0}, ", Index));
            sb.Append(string.Format("Number: {0}, ", Number));
            sb.Append(string.Format("Identity: {0} ({1}), ", Identity, IdentityName));
            sb.Append(string.Format("BlockFormat: {0}, ", BlockFormat));
            sb.Append(string.Format("SetFormat: {0}, ", SetFormat));
            sb.Append(string.Format("BlockSize: {0}, ", BlockSize));
            sb.Append(string.Format("ItemCount: {0}", ItemCount));
            return sb.ToString();
        }
    }

    public class GMDCElementVertices : IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                switch (BlockFormat)
                {
                    case 4:
                        //single float (4 byte length)
                        return 1;
                    case 1:
                        //paired floats (8 total bytes)
                        return 2;
                    case 2:
                        //triple floats (12 bytes total)
                        return 3;
                    default:
                        return 1;
                }
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public List<Godot.Vector3> Vertices { get; set; } = new();
        public int VerticesCount { get; set; }
        public long ElementLocation { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(string.Format("Element Index: {0}, ", Index));
            sb.Append(string.Format("Number: {0}, ", Number));
            sb.Append(string.Format("Identity: {0} ({1}), ", Identity, IdentityName));
            sb.Append(string.Format("BlockFormat: {0}, ", BlockFormat));
            sb.Append(string.Format("SetFormat: {0}, ", SetFormat));
            sb.Append(string.Format("BlockSize: {0}, ", BlockSize));
            sb.Append(string.Format("ItemCount: {0}", ItemCount));
            return sb.ToString();
        }
    }
    public class GMDCElementNormals : IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                switch (BlockFormat)
                {
                    case 4:
                        //single float (4 byte length)
                        return 1;
                    case 1:
                        //paired floats (8 total bytes)
                        return 2;
                    case 2:
                        //triple floats (12 bytes total)
                        return 3;
                    default:
                        return 1;
                }
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public List<Godot.Vector3> Normals { get; set; } = new();
        public long ElementLocation { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(string.Format("Element Index: {0}, ", Index));
            sb.Append(string.Format("Number: {0}, ", Number));
            sb.Append(string.Format("Identity: {0} ({1}), ", Identity, IdentityName));
            sb.Append(string.Format("BlockFormat: {0}, ", BlockFormat));
            sb.Append(string.Format("SetFormat: {0}, ", SetFormat));
            sb.Append(string.Format("BlockSize: {0}, ", BlockSize));
            sb.Append(string.Format("ItemCount: {0}", ItemCount));
            return sb.ToString();
        }
    }
    public class GMDCElementUVCoordinates : IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                switch (BlockFormat)
                {
                    case 4:
                        //single float (4 byte length)
                        return 1;
                    case 1:
                        //paired floats (8 total bytes)
                        return 2;
                    case 2:
                        //triple floats (12 bytes total)
                        return 3;
                    default:
                        return 1;
                }
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public List<Godot.Vector2> UVCoordinates { get; set; } = new();
        public long ElementLocation { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(string.Format("Element Index: {0}, ", Index));
            sb.Append(string.Format("Number: {0}, ", Number));
            sb.Append(string.Format("Identity: {0} ({1}), ", Identity, IdentityName));
            sb.Append(string.Format("BlockFormat: {0}, ", BlockFormat));
            sb.Append(string.Format("SetFormat: {0}, ", SetFormat));
            sb.Append(string.Format("BlockSize: {0}, ", BlockSize));
            sb.Append(string.Format("ItemCount: {0}", ItemCount));
            return sb.ToString();
        }
    }
    public class GMDCElementUVCoordinateDeltas : IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                switch (BlockFormat)
                {
                    case 4:
                        //single float (4 byte length)
                        return 1;
                    case 1:
                        //paired floats (8 total bytes)
                        return 2;
                    case 2:
                        //triple floats (12 bytes total)
                        return 3;
                    default:
                        return 1;
                }
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public List<uint> UVCoordinateDeltas { get; set; } = new();
        public long ElementLocation { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(string.Format("Element Index: {0}, ", Index));
            sb.Append(string.Format("Number: {0}, ", Number));
            sb.Append(string.Format("Identity: {0} ({1}), ", Identity, IdentityName));
            sb.Append(string.Format("BlockFormat: {0}, ", BlockFormat));
            sb.Append(string.Format("SetFormat: {0}, ", SetFormat));
            sb.Append(string.Format("BlockSize: {0}, ", BlockSize));
            sb.Append(string.Format("ItemCount: {0}", ItemCount));
            return sb.ToString();
        }
    }
    public class GMDCElementTargetIndices : IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                switch (BlockFormat)
                {
                    case 4:
                        //single float (4 byte length)
                        return 1;
                    case 1:
                        //paired floats (8 total bytes)
                        return 2;
                    case 2:
                        //triple floats (12 bytes total)
                        return 3;
                    default:
                        return 1;
                }
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public List<Godot.Vector3> TargetIndices { get; set; } = new();
        public long ElementLocation { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(string.Format("Element Index: {0}, ", Index));
            sb.Append(string.Format("Number: {0}, ", Number));
            sb.Append(string.Format("Identity: {0} ({1}), ", Identity, IdentityName));
            sb.Append(string.Format("BlockFormat: {0}, ", BlockFormat));
            sb.Append(string.Format("SetFormat: {0}, ", SetFormat));
            sb.Append(string.Format("BlockSize: {0}, ", BlockSize));
            sb.Append(string.Format("ItemCount: {0}", ItemCount));
            return sb.ToString();
        }
    }
    public class GMDCElementBoneAssignments : IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                switch (BlockFormat)
                {
                    case 4:
                        //single float (4 byte length)
                        return 1;
                    case 1:
                        //paired floats (8 total bytes)
                        return 2;
                    case 2:
                        //triple floats (12 bytes total)
                        return 3;
                    default:
                        return 1;
                }
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public List<float> BoneAssignments { get; set; } = new();
        public long ElementLocation { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(string.Format("Element Index: {0}, ", Index));
            sb.Append(string.Format("Number: {0}, ", Number));
            sb.Append(string.Format("Identity: {0} ({1}), ", Identity, IdentityName));
            sb.Append(string.Format("BlockFormat: {0}, ", BlockFormat));
            sb.Append(string.Format("SetFormat: {0}, ", SetFormat));
            sb.Append(string.Format("BlockSize: {0}, ", BlockSize));
            sb.Append(string.Format("ItemCount: {0}", ItemCount));
            return sb.ToString();
        }
    }
    public class GMDCElementSkinV1 : IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                switch (BlockFormat)
                {
                    case 4:
                        //single float (4 byte length)
                        return 1;
                    case 1:
                        //paired floats (8 total bytes)
                        return 2;
                    case 2:
                        //triple floats (12 bytes total)
                        return 3;
                    default:
                        return 1;
                }
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public List<float> SkinV1 { get; set; } = new(); // can be any of the three formats. ugh.  
        public long ElementLocation { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(string.Format("Element Index: {0}, ", Index));
            sb.Append(string.Format("Number: {0}, ", Number));
            sb.Append(string.Format("Identity: {0} ({1}), ", Identity, IdentityName));
            sb.Append(string.Format("BlockFormat: {0}, ", BlockFormat));
            sb.Append(string.Format("SetFormat: {0}, ", SetFormat));
            sb.Append(string.Format("BlockSize: {0}, ", BlockSize));
            sb.Append(string.Format("ItemCount: {0}", ItemCount));
            return sb.ToString();
        }
    }
    public class GMDCElementSkinV2 : IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                switch (BlockFormat)
                {
                    case 4:
                        //single float (4 byte length)
                        return 1;
                    case 1:
                        //paired floats (8 total bytes)
                        return 2;
                    case 2:
                        //triple floats (12 bytes total)
                        return 3;
                    default:
                        return 1;
                }
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public List<Godot.Vector2> SkinV2 { get; set; } = new(); // can be any of the three formats. ugh.  
        public long ElementLocation { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(string.Format("Element Index: {0}, ", Index));
            sb.Append(string.Format("Number: {0}, ", Number));
            sb.Append(string.Format("Identity: {0} ({1}), ", Identity, IdentityName));
            sb.Append(string.Format("BlockFormat: {0}, ", BlockFormat));
            sb.Append(string.Format("SetFormat: {0}, ", SetFormat));
            sb.Append(string.Format("BlockSize: {0}, ", BlockSize));
            sb.Append(string.Format("ItemCount: {0}", ItemCount));
            return sb.ToString();
        }
    }
    public class GMDCElementSkinV3 : IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                switch (BlockFormat)
                {
                    case 4:
                        //single float (4 byte length)
                        return 1;
                    case 1:
                        //paired floats (8 total bytes)
                        return 2;
                    case 2:
                        //triple floats (12 bytes total)
                        return 3;
                    default:
                        return 1;
                }
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public List<Godot.Vector3> SkinV3 { get; set; } = new(); // can be any of the three formats. ugh.
        public long ElementLocation { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(string.Format("Element Index: {0}, ", Index));
            sb.Append(string.Format("Number: {0}, ", Number));
            sb.Append(string.Format("Identity: {0} ({1}), ", Identity, IdentityName));
            sb.Append(string.Format("BlockFormat: {0}, ", BlockFormat));
            sb.Append(string.Format("SetFormat: {0}, ", SetFormat));
            sb.Append(string.Format("BlockSize: {0}, ", BlockSize));
            sb.Append(string.Format("ItemCount: {0}", ItemCount));
            return sb.ToString();
        }
    }
    public class GMDCElementMorphVertexDeltas : IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                switch (BlockFormat)
                {
                    case 4:
                        //single float (4 byte length)
                        return 1;
                    case 1:
                        //paired floats (8 total bytes)
                        return 2;
                    case 2:
                        //triple floats (12 bytes total)
                        return 3;
                    default:
                        return 1;
                }
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public List<Godot.Vector3> MorphVertexDeltas { get; set; } = new();
        public long ElementLocation { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(string.Format("Element Index: {0}, ", Index));
            sb.Append(string.Format("Number: {0}, ", Number));
            sb.Append(string.Format("Identity: {0} ({1}), ", Identity, IdentityName));
            sb.Append(string.Format("BlockFormat: {0}, ", BlockFormat));
            sb.Append(string.Format("SetFormat: {0}, ", SetFormat));
            sb.Append(string.Format("BlockSize: {0}, ", BlockSize));
            sb.Append(string.Format("ItemCount: {0}", ItemCount));
            return sb.ToString();
        }
    }
    public class GMDCElementMorphNormalDeltas : IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                switch (BlockFormat)
                {
                    case 4:
                        //single float (4 byte length)
                        return 1;
                    case 1:
                        //paired floats (8 total bytes)
                        return 2;
                    case 2:
                        //triple floats (12 bytes total)
                        return 3;
                    default:
                        return 1;
                }
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public List<Godot.Vector3> MorphNormalDeltas { get; set; } = new();
        public long ElementLocation { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(string.Format("Element Index: {0}, ", Index));
            sb.Append(string.Format("Number: {0}, ", Number));
            sb.Append(string.Format("Identity: {0} ({1}), ", Identity, IdentityName));
            sb.Append(string.Format("BlockFormat: {0}, ", BlockFormat));
            sb.Append(string.Format("SetFormat: {0}, ", SetFormat));
            sb.Append(string.Format("BlockSize: {0}, ", BlockSize));
            sb.Append(string.Format("ItemCount: {0}", ItemCount));
            return sb.ToString();
        }
    }
    public class GMDCElementMorphVertexMap : IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                switch (BlockFormat)
                {
                    case 4:
                        //single float (4 byte length)
                        return 1;
                    case 1:
                        //paired floats (8 total bytes)
                        return 2;
                    case 2:
                        //triple floats (12 bytes total)
                        return 3;
                    default:
                        return 1;
                }
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public List<float> MorphVertexMap { get; set; } = new();
        public long ElementLocation { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(string.Format("Element Index: {0}, ", Index));
            sb.Append(string.Format("Number: {0}, ", Number));
            sb.Append(string.Format("Identity: {0} ({1}), ", Identity, IdentityName));
            sb.Append(string.Format("BlockFormat: {0}, ", BlockFormat));
            sb.Append(string.Format("SetFormat: {0}, ", SetFormat));
            sb.Append(string.Format("BlockSize: {0}, ", BlockSize));
            sb.Append(string.Format("ItemCount: {0}", ItemCount));
            return sb.ToString();
        }
    }
    public class GMDCElementBumpMapNormals : IGMDCElement
    {
        public int Index { get; set; }
        public string Identity { get; set; }
        public string IdentityName { get; set; }
        public uint Number { get; set; }
        public uint BlockFormat { get; set; }
        public uint SetFormat { get; set; }
        public uint BlockSize { get; set; }
        public int Length
        {
            get
            {
                switch (BlockFormat)
                {
                    case 4:
                        //single float (4 byte length)
                        return 1;
                    case 1:
                        //paired floats (8 total bytes)
                        return 2;
                    case 2:
                        //triple floats (12 bytes total)
                        return 3;
                    default:
                        return 1;
                }
            }
        }
        public int ListLength { get { return (int)(BlockSize / Length / 4); } }
        public uint ItemCount { get; set; }
        public List<Godot.Vector3> BumpMapNormals { get; set; } = new();
        public long ElementLocation { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new();
            sb.Append(string.Format("Element Index: {0}, ", Index));
            sb.Append(string.Format("Number: {0}, ", Number));
            sb.Append(string.Format("Identity: {0} ({1}), ", Identity, IdentityName));
            sb.Append(string.Format("BlockFormat: {0}, ", BlockFormat));
            sb.Append(string.Format("SetFormat: {0}, ", SetFormat));
            sb.Append(string.Format("BlockSize: {0}, ", BlockSize));
            sb.Append(string.Format("ItemCount: {0}", ItemCount));
            return sb.ToString();
        }
    }


    public class GMDCLinkage
    {
        public uint IndexCount { get; set; }
        public List<uint> IndexValues { get; set; } = new();
        public uint ArraySize { get; set; }
        public uint ActiveElements { get; set; }
        public uint SubmodelVertexCount { get; set; }
        public uint SubmodelNormalsCount { get; set; }
        public uint SubmodelUVCount { get; set; }
        public List<uint> SubmodelIndexValues { get; set; } = new();
        public List<uint> NormalsIndexValues { get; set; } = new();
        public List<uint> UVIndexValues { get; set; } = new();
    }

    public class GMDCGroup
    {
        public uint PrimitiveType { get; set; }
        public uint LinkIndex { get; set; }
        public string ObjectName { get; set; }
        public uint FaceCount { get; set; }
        public List<Godot.Vector3> Faces { get; set; } = new();

        public string OpacityAmount { get; set; } //0xFFFFFFFF = full opacity, 0x0 = full transparency, < 16 for shadows
        public List<uint> ModelSubsetReference { get; set; } = new();

        public override string ToString()
        {
            return string.Format("{0}: Primitive Type: {1} Link Index: {2}, Face Count: {3}", ObjectName, PrimitiveType, LinkIndex, FaceCount);
        }
    }

    public class TransformMatrix
    {
        public uint QuaternionX { get; set; }
        public uint QuaternionY { get; set; }
        public uint QuaternionZ { get; set; }
        public uint QuaternionW { get; set; }
        public uint TransformX { get; set; }
        public uint TransformY { get; set; }
        public uint TransformZ { get; set; }
    }

    public class GMDCModel
    {
        public List<TransformMatrix> TransformMatrices { get; set; } = new();
        public List<NamePair> NamedPairs { get; set; } = new();
        public List<Godot.Vector3> Verts { get; set; } = new();
        public List<uint> Faces { get; set; } = new();
    }

    public class NamePair
    {
        public string BlendGroupName { get; set; }
        public string AssignedElementName { get; set; }
    }

    public class GMDCSubset
    {
        public List<Godot.Vector3> Verts { get; set; } = new();
        public List<uint> Faces { get; set; } = new();
    }
    public class GMDCData
    {
        private List<GMDCElementVertices> _GMDCElementVertices;
        public List<GMDCElementVertices> GMDCElementVertices
        {
            get
            {                
                return _GMDCElementVertices;
            }
            set
            {
                _GMDCElementVertices = value;
            }
        }
        private List<GMDCElementNormals> _GMDCElementNormals;
        public List<GMDCElementNormals> GMDCElementNormals
        {
            get
            {
                return _GMDCElementNormals;
            }
            set
            {
                _GMDCElementNormals = value;
            }
        }
        private List<GMDCElementUVCoordinates> _GMDCElementUVCoordinates;
        public List<GMDCElementUVCoordinates> GMDCElementUVCoordinates
        {
            get
            {
                
                return _GMDCElementUVCoordinates;
                
            }
            set
            {
                _GMDCElementUVCoordinates = value;
            }
        }
        private List<GMDCElementTargetIndices> _GMDCElementTargetIndices;
        public List<GMDCElementTargetIndices> GMDCElementTargetIndices
        {
            get
            {
                return _GMDCElementTargetIndices;                
            }
            set
            {
                _GMDCElementTargetIndices = value;
            }
        }
        private List<GMDCElementBoneAssignments> _GMDCElementBoneAssignments;
        public List<GMDCElementBoneAssignments> GMDCElementBoneAssignments
        {
            get
            {
                
                    return _GMDCElementBoneAssignments;
                
            }
            set
            {
                _GMDCElementBoneAssignments = value;
            }
        }
        private List<GMDCElementSkinV1> _GMDCElementSkinV1;
        public List<GMDCElementSkinV1> GMDCElementSkinV1
        {
            get
            {
                
                    return _GMDCElementSkinV1;
                
            }
            set
            {
                _GMDCElementSkinV1 = value;
            }
        }
        private List<GMDCElementSkinV2> _GMDCElementSkinV2;
        public List<GMDCElementSkinV2> GMDCElementSkinV2
        {
            get
            {
                
                
                    return _GMDCElementSkinV2;
                
            }
            set
            {
                _GMDCElementSkinV2 = value;
            }
        }
        private List<GMDCElementSkinV3> _GMDCElementSkinV3;
        public List<GMDCElementSkinV3> GMDCElementSkinV3
        {
            get
            {
                
                    return _GMDCElementSkinV3;
                
            }
            set
            {
                _GMDCElementSkinV3 = value;
            }
        }
        private List<GMDCElementMorphVertexDeltas> _GMDCElementMorphVertexDeltas;
        public List<GMDCElementMorphVertexDeltas> GMDCElementMorphVertexDeltas
        {
            get
            {
                
                    return _GMDCElementMorphVertexDeltas;
                
            }
            set
            {
                _GMDCElementMorphVertexDeltas = value;
            }
        }
        private List<GMDCElementMorphNormalDeltas> _GMDCElementMorphNormalDeltas;
        public List<GMDCElementMorphNormalDeltas> GMDCElementMorphNormalDeltas
        {
            get
            {
                
                    return _GMDCElementMorphNormalDeltas;
                
            }
            set
            {
                _GMDCElementMorphNormalDeltas = value;
            }
        }
        private List<GMDCElementMorphVertexMap> _GMDCElementMorphVertexMap;
        public List<GMDCElementMorphVertexMap> GMDCElementMorphVertexMap
        {
            get
            {
                
                
                    return _GMDCElementMorphVertexMap;
                
            }
            set
            {
                _GMDCElementMorphVertexMap = value;
            }
        }
        private List<GMDCElementBumpMapNormals> _GMDCElementBumpMapNormals;
        public List<GMDCElementBumpMapNormals> GMDCElementBumpMapNormals
        {
            get
            {
                
                
                    return _GMDCElementBumpMapNormals;
                
            }
            set
            {
                _GMDCElementBumpMapNormals = value;
            }
        }

        public List<GMDCLinkage> Linkages = new();
        public List<GMDCGroup> Groups = new();
        public GMDCModel Model = new();
        public List<GMDCSubset> Subsets = new();

        public void ToElements(IList<IGMDCElement> allElements)
        {
            GMDCElementVertices = allElements.OfType<GMDCElementVertices>().ToList();
            GMDCElementNormals = allElements.OfType<GMDCElementNormals>().ToList();
            GMDCElementUVCoordinates = allElements.OfType<GMDCElementUVCoordinates>().ToList();
            GMDCElementTargetIndices = allElements.OfType<GMDCElementTargetIndices>().ToList();
            GMDCElementBoneAssignments = allElements.OfType<GMDCElementBoneAssignments>().ToList();
            GMDCElementSkinV1 = allElements.OfType<GMDCElementSkinV1>().ToList();
            GMDCElementSkinV2 = allElements.OfType<GMDCElementSkinV2>().ToList();
            GMDCElementSkinV3 = allElements.OfType<GMDCElementSkinV3>().ToList();
            GMDCElementMorphVertexDeltas = allElements.OfType<GMDCElementMorphVertexDeltas>().ToList();
            GMDCElementMorphNormalDeltas = allElements.OfType<GMDCElementMorphNormalDeltas>().ToList();
            GMDCElementMorphVertexMap = allElements.OfType<GMDCElementMorphVertexMap>().ToList();
            GMDCElementBumpMapNormals = allElements.OfType<GMDCElementBumpMapNormals>().ToList();
        }

        public IList<IGMDCElement> GetAllElements()
        {
            IList<IGMDCElement> elements = [];
            if (GMDCElementVertices != null)
            {
                foreach (GMDCElementVertices e in GMDCElementVertices)
                {
                    elements.Add(e);
                }
            }
            if (GMDCElementNormals != null)
            {
                foreach (GMDCElementNormals e in GMDCElementNormals)
                {
                    elements.Add(e);
                }
            }
            if (GMDCElementUVCoordinates != null)
            {
                foreach (GMDCElementUVCoordinates e in GMDCElementUVCoordinates)
                {
                    elements.Add(e);
                }
            }
            if (GMDCElementTargetIndices != null)
            {
                foreach (GMDCElementTargetIndices e in GMDCElementTargetIndices)
                {
                    elements.Add(e);
                }
            }
            if (GMDCElementBoneAssignments != null)
            {
                foreach (GMDCElementBoneAssignments e in GMDCElementBoneAssignments)
                {
                    elements.Add(e);
                }
            }
            if (GMDCElementSkinV1 != null)
            {
                foreach (GMDCElementSkinV1 e in GMDCElementSkinV1)
                {
                    elements.Add(e);
                }
            }
            if (GMDCElementSkinV2 != null)
            {
                foreach (GMDCElementSkinV2 e in GMDCElementSkinV2)
                {
                    elements.Add(e);
                }
            }
            if (GMDCElementSkinV3 != null)
            {
                foreach (GMDCElementSkinV3 e in GMDCElementSkinV3)
                {
                    elements.Add(e);
                }
            }
            if (GMDCElementMorphVertexDeltas != null)
            {
                foreach (GMDCElementMorphVertexDeltas e in GMDCElementMorphVertexDeltas)
                {
                    elements.Add(e);
                }
            }
            if (GMDCElementMorphNormalDeltas != null)
            {
                foreach (GMDCElementMorphNormalDeltas e in GMDCElementMorphNormalDeltas)
                {
                    elements.Add(e);
                }
            }
            if (GMDCElementMorphVertexMap != null)
            {
                foreach (GMDCElementMorphVertexMap e in GMDCElementMorphVertexMap)
                {
                    elements.Add(e);
                }
            }
            if (GMDCElementBumpMapNormals != null)
            {
                foreach (GMDCElementBumpMapNormals e in GMDCElementBumpMapNormals)
                {
                    elements.Add(e);
                }
            }
            return elements;
        }
    }
}