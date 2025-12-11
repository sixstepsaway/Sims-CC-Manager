using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 1, Category = "Door", Subcategory = ""},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 4, Category = "Window", Subcategory = ""},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 100, Category = "Two Story Door", Subcategory = ""},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 2, Category = "Two Story Window", Subcategory = ""},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 10, Category = "Arch", Subcategory = ""},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 20, Category = "Staircase", Subcategory = ""},
            new FunctionSortList(){FlagNum = 0, FunctionSubsortNum = 0, Category = "Fireplaces (?)", Subcategory = ""},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 400, Category = "Garage", Subcategory = ""},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 1, Category = "Trees", Subcategory = ""},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 4, Category = "Flowers", Subcategory = ""},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 10, Category = "Gardening", Subcategory = ""},
            new FunctionSortList(){FlagNum = 4, FunctionSubsortNum = 2, Category = "Shrubs", Subcategory = ""},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 1000, Category = "Architecture", Subcategory = ""},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 8, Category = "Column", Subcategory = ""},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 100, Category = "Two Story Column", Subcategory = ""},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 200, Category = "Connecting Column", Subcategory = ""},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 40, Category = "Pools", Subcategory = ""},
            new FunctionSortList(){FlagNum = 8, FunctionSubsortNum = 8, Category = "Gates", Subcategory = ""},
            new FunctionSortList(){FlagNum = 1, FunctionSubsortNum = 800, Category = "Elevator", Subcategory = ""}
        };
    }
    public class SimsPackageReader
    {        
        private string _dbpf;
        public string DBPF
        {
            get { return _dbpf; }
            set { _dbpf = value;
            if (value == "DBPF")
                {
                    IsBroken = false;
                } else
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
            set { _minorversion = value; 
            switch (value)
                {
                    case 1: //minor is 1
                        if (MajorVersion == 1) GameVer = OptionLists.SimsGames.Sims2;
                        if (MajorVersion == 2) GameVer = OptionLists.SimsGames.Sims4;
                    break;

                    case 2: // minor is 2
                        if (MajorVersion == 1) GameVer = OptionLists.SimsGames.Sims2;
                    break;
                    
                    case 0: //minor is 0
                        switch (MajorVersion)
                        {
                            case 2:
                                GameVer = OptionLists.SimsGames.Sims3;
                            break;
                            case 3:
                                GameVer = OptionLists.SimsGames.SimCity5;
                            break;
                        }
                    break;

                }
            }
        }


        public Sims2Data Sims2Data = new();
        public Sims3Data Sims3Data = new();
        public Sims4Data Sims4Data = new();

        public SimsGames GameVer = new();
        public bool IsBroken = false;
        MemoryStream msPackage;
        BinaryReader packagereader;
        FileInfo fileinfo;


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

            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("File {0} is for game {1}", fileinfo.Name, GameVer.ToString()));

            switch (GameVer)
            {
                case SimsGames.Sims2:
                    ReadSims2Package();
                return;

                case SimsGames.Sims3:
                    ReadSims3Package();
                    //run package reader
                return;

                case SimsGames.Sims4:
                    ReadSims4Package();
                    //run package reader
                return;

            }
            return;
        }



        long IndexMajorLocation = 24;
        long IndexMinorLocation = 60;

        uint IndexMajorVersion;
        uint IndexCount;
        uint IndexOffset;
        uint IndexSize;
        uint IndexMinorVersion;
        uint ChunkOffset = 0;

        List<string> InstanceIDs = new();

        List<IndexEntry> IndexData = new();


        public void ReadSims2Package()
        {            
            if (MinorVersion == 2) IndexMajorLocation = 24;
            if (MinorVersion == 0) IndexMajorLocation = 32;
            //packagereader.BaseStream.Position = IndexMajorLocation;
            Encoding.UTF8.GetString(packagereader.ReadBytes(12));
            packagereader.ReadUInt32();
            packagereader.ReadUInt32();
            IndexMajorVersion = packagereader.ReadUInt32();
            
            IndexCount = packagereader.ReadUInt32();
            
            IndexOffset = packagereader.ReadUInt32();
            
            IndexSize = packagereader.ReadUInt32();
            //packagereader.BaseStream.Position = IndexMinorLocation;
            packagereader.ReadUInt32();
            packagereader.ReadUInt32();
            packagereader.ReadUInt32();

            IndexMinorVersion = packagereader.ReadUInt32() -1;

            //move to the index location
            packagereader.BaseStream.Position = ChunkOffset + IndexOffset;
            if (IndexCount == 0) 
            {
                IsBroken = true;
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("File {0} has 0 index entries. Broken. Returning.", fileinfo.Name));
                return;
            }
            for (int i = 0; i < IndexCount; i++)
            {
                
                IndexEntry holderEntry = new IndexEntry();
                holderEntry.TypeID = packagereader.ReadUInt32().ToString("X8");
                
                holderEntry.GroupID = packagereader.ReadUInt32().ToString("X8");
                holderEntry.InstanceID = packagereader.ReadUInt32().ToString("X8");
                
                InstanceIDs.Add(holderEntry.InstanceID.ToString());
                
                if ((IndexMajorVersion == 7) && (IndexMinorVersion == 1)) {
                    holderEntry.InstanceID2 = packagereader.ReadUInt32().ToString("X8");
                } else {
                    holderEntry.InstanceID2 = "00000000";
                }
                holderEntry.Offset = packagereader.ReadUInt32();
                
                holderEntry.FileSize = packagereader.ReadUInt32();
                
                holderEntry.TrueSize = 0;
                
                holderEntry.IsCompressed = false;

                holderEntry.EntryIDX = i;

                IndexData.Add(holderEntry);

                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("{0}", holderEntry.ToString()));
                
            }



        }
        public void ReadSims3Package()
        {
            
        }
        public void ReadSims4Package()
        {
            
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

    public class EntryType {
        /// <summary>
        /// For "types", for example Cas Parts or Geometry.
        /// </summary>
        public string Tag {get; set;}
        public string TypeID {get; set;}
        public string Description {get; set;}
    }

    public class FunctionSortList {
        /// <summary>
        /// Used for Sims 2 (I believe) function categorization. 
        /// </summary>
        public int FlagNum {get; set;}
        public int FunctionSubsortNum {get; set;}
        public string Category {get; set;}
        public string Subcategory {get; set;}
    } 

    public class IndexEntry
    {
        public string TypeID;
        public string GroupID;
        public string InstanceID;
        public string CompleteID
        {
            get { return string.Format("{0}-{1}-{2}", TypeID, GroupID, InstanceID); }
        }
        public string InstanceID2;
        public uint Offset;
        public uint FileSize;
        public uint TrueSize;
        public bool IsCompressed;        
        public string Unused;
        public int EntryIDX;

        public string EntryType
        {
            get { try
                {
                    return Sims2PackageStatics.Sims2EntryTypes.First(x => x.TypeID == TypeID).Tag; 
                } catch
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("TypeID doesn't have a matching entry: {0}", TypeID));
                    return "UNKNOWN";
                } 
            }
        }

        public override string ToString()
        {
            return string.Format("Index Entry: {0} - Location: {1}, FileSize: {2}, TrueSize: {3}, Compressed: {4}. Type: {5}", CompleteID, Offset, FileSize, TrueSize, IsCompressed, EntryType);
        }
    }	

    public class ByteReaders {
        /// <summary>
        /// Repeatedly called methods.
        /// </summary>

        public static MemoryStream ReadBytesToFile(string file){
            FileInfo f = new FileInfo(file);
            byte[] bit = new byte[f.Length];
            int bytes;
			using (FileStream fsSource = new FileStream(file, FileMode.Open, FileAccess.Read))
			{
				for (int w = 0; w < f.Length; w++){
                    bit[w] = (byte)fsSource.ReadByte();
                }                
				MemoryStream stream = new MemoryStream(bit);
				return stream;
			}
		}

		public static MemoryStream ReadBytesToFile(string file, int bytestoread){
			byte[] bit = new byte[bytestoread];
            int bytes;
			using (FileStream fsSource = new FileStream(file, FileMode.Open, FileAccess.Read))
			{
				for (int w = 0; w < bytestoread; w++){
                    bit[w] = (byte)fsSource.ReadByte();
                }
				MemoryStream stream = new MemoryStream(bit);
				return stream;
			}
		}

        public static Byte[] ReadEntryBytes(BinaryReader reader, int memSize){
            byte[] bit = new byte[memSize];
            for (int w = 0; w < memSize; w++){
                bit[w] = reader.ReadByte();
            }
            return bit;
        }
    }

}