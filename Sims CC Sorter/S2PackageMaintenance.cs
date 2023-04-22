using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;
using System.Xml;
using System.Security.Cryptography;
using DBPFReading;
using SSAGlobals;

namespace S2PackageMaintenance {

    public class SimsPackage {
        
        public string Name {get; set;}
        public string Description {get; set;}
        public string Location {get; set;}
        public string DBPF {get; set;}
        public int Game {get; set;}
        public uint Major {get; set;}
        public uint Minor {get; set;}
        public uint DateCreated {get; set;}
        public uint DateModified {get; set;}
        public uint IndexMajorVersion {get; set;}
        public uint IndexCount {get; set;}
        public uint IndexOffset {get; set;}
        public uint IndexSize {get; set;}
        public uint HolesCount {get; set;}
        public uint HolesOffset {get; set;}
        public uint HolesSize {get; set;}
    }

    
    class S2Packages {
        private uint chunkOffset = 0;
        public string[] packageTypes = { };
		public string[] xmlCatalogSortTypes = { };
		public string[] xmlSubtypes = { };
		public string[] xmlCategoryTypes = { };
		private uint majorVersion;
		private uint minorVersion;
		private string reserved;
		private uint dateCreated;
		private uint dateModified;
		private uint indexMajorVersion;
		private uint indexMinorVersion;
		private uint indexCount;
		private uint indexOffset;
		private uint indexSize;
		private uint holesCount;
		private uint holesOffset;
		private uint holesSize;
		public string title = "";
		public string description = "";
		public string pkgType = "";
		public uint pkgTypeInt = 0;
		public string xmlType = "";
		public string xmlCategory = "";
		public string xmlSubtype = "";
		public string xmlModelName = "";
		public string xmlAge = "";
		public string xmlGender = "";
		public string xmlCatalog = "";
		public ArrayList objectGUID = new ArrayList();
		public string xmlCreator = "";
		public bool isBroken = false;
		public string md5hash = "";
		// majorType is the main type in which this package falls, used for
		// orphan stuff
		// 0 = No type
		// 1 = Mesh
		// 2 = Recolour
		// 3 = BodyShop Mesh
		public uint majorType = 0;
		private string reserved2;
		public ArrayList indexData = new ArrayList();
		public scanTypeList scanList = new scanTypeList();
		public fileHasList fileHas = new fileHasList();
		private ArrayList shpeData = new ArrayList();
		public ArrayList linkData = new ArrayList();
		public ArrayList guidData = new ArrayList();

		private string filename = "";

        //ReadByteStream readByteStream = new ReadByteStream();
        DBPFTypeRead dbpfTypeRead = new DBPFTypeRead();




        public void s2Packages(){
            
        }

        public void setParams(string filename, uint chunkOffset, string[] packageTypes, string[] xmlCatalogSortTypes, string[] xmlSubTypes, string[] xmlCategoryTypes){
            this.filename = filename;
            this.chunkOffset = chunkOffset;
            this.packageTypes = packageTypes;
            this.xmlCatalogSortTypes = xmlCatalogSortTypes;
            this.xmlSubtypes = xmlSubTypes;
            this.xmlCategoryTypes = xmlCategoryTypes;
        }

        public void s2GetLabel(String file){
            LoggingGlobals logGlobals = new LoggingGlobals();
            var statement = "";
            var IncomingInformation = new ExtractedItems();
            var AllSimsPackages = new List<SimsPackage>();
            FileStream dbpfFile = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReader readFile = new BinaryReader(dbpfFile);
            var temp = new SimsPackage();
            temp.Game = 2;
            string dbpf = Encoding.ASCII.GetString(readFile.ReadBytes(4));
            temp.DBPF = dbpf;
            uint major = readFile.ReadUInt32();
            temp.Major = major;
            uint minor = readFile.ReadUInt32();
            temp.Minor = minor;
            string reserved = Encoding.UTF8.GetString(readFile.ReadBytes(12));
            uint dateCreated = readFile.ReadUInt32();
            temp.DateCreated = dateCreated;
            uint dateModified = readFile.ReadUInt32();
            temp.DateModified = dateModified;
            uint indexMajorVersion = readFile.ReadUInt32();
            temp.IndexMajorVersion = indexMajorVersion;
            uint indexCount = readFile.ReadUInt32();
            temp.IndexCount = indexCount;
            uint indexOffset = readFile.ReadUInt32();
            temp.IndexOffset = indexOffset;
            uint indexSize = readFile.ReadUInt32();
            temp.IndexSize = indexSize;
            uint holesCount = readFile.ReadUInt32();
            temp.HolesCount = holesCount;
            uint holesOffset = readFile.ReadUInt32();
            temp.HolesOffset = holesOffset;
            uint holesSize = readFile.ReadUInt32();
            temp.HolesSize = holesSize;
            string reserved2 = Encoding.UTF8.GetString(readFile.ReadBytes(32));
            dbpfFile.Seek(this.chunkOffset + indexOffset, SeekOrigin.Begin);
            for (int i = 0; i < indexCount; i++)
            {
                indexEntry myEntry = new indexEntry();
                myEntry.typeID = readFile.ReadUInt32().ToString("X8");
                myEntry.groupID = readFile.ReadUInt32().ToString("X8");
                myEntry.instanceID = readFile.ReadUInt32().ToString("X8");
                myEntry.instanceID2 = "00000000";
                if ((this.indexMajorVersion == 7) && (this.indexMinorVersion == 1)) 
                {
                    myEntry.instanceID2 = readFile.ReadUInt32().ToString("X8");
                }

                myEntry.offset = readFile.ReadUInt32();
                myEntry.filesize = readFile.ReadUInt32();
                myEntry.truesize = 0;
                myEntry.compressed = false;

                indexData.Add(myEntry);
                myEntry = null;   
            }
            var entrynum = 0;
            foreach (indexEntry iEntry in indexData)
            {
                uint numRecords;
                string typeID;
                string groupID;
                string instanceID;
                string instanceID2 = "";
                uint myFilesize;

                switch (iEntry.typeID.ToLower())
                {
                    case "fc6e1f7": fileHas.shpe++; linkData.Add(iEntry); break;
                }

                if (iEntry.typeID == "E86B1EEF")  // DIR Resource
                {
                    dbpfFile.Seek(this.chunkOffset + iEntry.offset, SeekOrigin.Begin);
                    if (indexMajorVersion == 7 && indexMinorVersion == 1)
                    {
                        numRecords = iEntry.filesize / 20;
                    }
                    else 
                    {
                        numRecords = iEntry.filesize / 16;
                    }

                    // Loop through getting all the compressed entries
                    for (int i = 0; i < numRecords; i++)
                    {
                        typeID = readFile.ReadUInt32().ToString("X8");
                        groupID = readFile.ReadUInt32().ToString("X8");
                        instanceID = readFile.ReadUInt32().ToString("X8");
                        if (indexMajorVersion == 7 && indexMinorVersion == 1) instanceID2 = readFile.ReadUInt32().ToString("X8");
                        myFilesize = readFile.ReadUInt32();
                        
                        foreach (indexEntry idx in indexData) {

                            int cFileSize = 0;
                            string cTypeID = "";

                            switch (idx.typeID)
                            {
                                case "43545353": // Catalog Description - CTSS
                                    //Console.WriteLine("    Catalog Description file");
                                    dbpfFile.Seek(this.chunkOffset + idx.offset, SeekOrigin.Begin);

                                        cFileSize = readFile.ReadInt32();
                                        cTypeID = readFile.ReadUInt16().ToString("X4");
                                        // check for the proper QFS type
                                        if (cTypeID == "FB10") 
                                        {
                                            byte[] tempBytes = readFile.ReadBytes(3);
                                            uint cFullSize = DBPFTypeRead.QFSLengthToInt(tempBytes);

                                            ReadByteStream decompressed = new ReadByteStream(DBPFTypeRead.Uncompress(readFile.ReadBytes(cFileSize), cFullSize, 0));                                           

                                            IncomingInformation = dbpfTypeRead.readCTSSchunk(decompressed);
                                        } 
                                        else 
                                        {
                                            dbpfFile.Seek(this.chunkOffset + idx.offset, SeekOrigin.Begin);
                                            IncomingInformation = dbpfTypeRead.readCTSSchunk(readFile);
                                        }
                                    break;
                                case "2CB230B8": // XFNC - Fence XML
                                case "4DCADB7E": // XFLR - Floor XML
                                case "CCA8E925": // XOBJ - Object XML
                                case "0C1FE246": // XMOL - Mesh Overlay XML
                                case "ACA8EA06": // XROF - Roof XML
                                case "2C1FD8A1": // Texture Overlay XML
                                case "4C697E5A": // Material Override - MMAT
                                case "8C1580B5": // HairTone XML
                                    //Console.WriteLine("    " + idx.typeID + " file");
                                    dbpfFile.Seek(this.chunkOffset + idx.offset, SeekOrigin.Begin);

                                    //if (idx.compressed == true)
                                    //{
                                        // Is this always in XML format?
                                    cFileSize = readFile.ReadInt32();
                                    cTypeID = readFile.ReadUInt16().ToString("X4");
                                    // check for the proper QFS type
                                    if (cTypeID == "FB10") 
                                    {
                                        byte[] tempBytes = readFile.ReadBytes(3);
                                        uint cFullSize = DBPFTypeRead.QFSLengthToInt(tempBytes);

                                        // Check for CPF type
                                        string cpfTypeID = readFile.ReadUInt32().ToString("X8");
                                        if ((cpfTypeID == "CBE7505E") || (cpfTypeID == "CBE750E0"))
                                        {
                                            // Is an actual CPF file, so we have to decompress it...
                                            IncomingInformation = dbpfTypeRead.readCPFchunk(readFile);
                                        } 
                                        else 
                                        {
                                            dbpfFile.Seek(this.chunkOffset + idx.offset + 9, SeekOrigin.Begin);
                                            ReadByteStream decompressed = new ReadByteStream(DBPFTypeRead.Uncompress(readFile.ReadBytes(cFileSize), cFullSize, 0));

                                            if (cpfTypeID == "E750E0E2") 
                                            {

                                                // Read first four bytes
                                                cpfTypeID = decompressed.ReadUInt32().ToString("X8");

                                                //Console.WriteLine("CPF type id: " + cpfTypeID);
                                                if ((cpfTypeID == "CBE7505E") || (cpfTypeID == "CBE750E0")) 
                                                {
                                                    // Is an actual CPF file, so we have to decompress it...
                                                    IncomingInformation = dbpfTypeRead.readCPFchunk(decompressed);
                                                }

                                            } 
                                            else 
                                            {
                                                IncomingInformation = dbpfTypeRead.readXMLchunk(decompressed);
                                            }
                                        }
                                    } 
                                    else 
                                    {
                                        dbpfFile.Seek(this.chunkOffset + idx.offset, SeekOrigin.Begin);

                                        string cpfTypeID = readFile.ReadUInt32().ToString("X8");
                                        //Console.WriteLine("CPF type id: " + cpfTypeID);
                                        if ((cpfTypeID == "CBE7505E") || (cpfTypeID == "CBE750E0"))
                                        {
                                            // Is an actual CPF file, so we have to decompress it...
                                            IncomingInformation = dbpfTypeRead.readCPFchunk(readFile);
                                        }

                                        // Actually an uncompressed XML file, so we can use the xmlChunk to 
                                        // process
                                        if  (cpfTypeID == "6D783F3C")
                                        {
                                            // Backtrack 4 bytes
                                            dbpfFile.Seek(this.chunkOffset + idx.offset, SeekOrigin.Begin);

                                            // Read entire XML as a normal string
                                            string xmlData = Encoding.UTF8.GetString(readFile.ReadBytes((int)idx.filesize));
                                            IncomingInformation = dbpfTypeRead.readXMLchunk(xmlData);

                                        }
                                    }
                                    break;
                                case "EBCF3E27":
                                    // Property Set - only read if no other XML/CPF resources
                                    if ((fileHas.xhtn == 0) && (fileHas.xobj == 0) && (fileHas.xflr == 0) && (fileHas.xfnc == 0) && (fileHas.xmol == 0) && (fileHas.xngb == 0) && (fileHas.xobj == 0) && (fileHas.xrof == 0) && (fileHas.xstn == 0) && (fileHas.xtol == 0) && (fileHas.aged == 0) )
                                    {
                                        //Console.WriteLine("    Property Set GZPS");
                                        dbpfFile.Seek(this.chunkOffset + idx.offset, SeekOrigin.Begin);

                                        cFileSize = readFile.ReadInt32();
                                        cTypeID = readFile.ReadUInt16().ToString("X4");
                                        // check for the proper QFS type
                                        if (cTypeID == "FB10") 
                                        {
                                            byte[] tempBytes = readFile.ReadBytes(3);
                                            uint cFullSize = DBPFTypeRead.QFSLengthToInt(tempBytes);

                                            ReadByteStream decompressed = new ReadByteStream(DBPFTypeRead.Uncompress(readFile.ReadBytes(cFileSize), cFullSize, 0));

                                            // Read first four bytes
                                            string cpfTypeID = decompressed.ReadUInt32().ToString("X8");

                                            //Console.WriteLine("CPF type id: " + cpfTypeID);
                                            if ((cpfTypeID == "CBE7505E") || (cpfTypeID == "CBE750E0")) 
                                            {
                                                // Is an actual CPF file, so we have to decompress it...
                                                IncomingInformation = dbpfTypeRead.readCPFchunk(decompressed);
                                            }

                                        } 
                                        else 
                                        {
                                            dbpfFile.Seek(this.chunkOffset + idx.offset, SeekOrigin.Begin);
                                            string cpfTypeID = readFile.ReadUInt32().ToString("X8");
                                            //Console.WriteLine("CPF type id: " + cpfTypeID);
                                            if ((cpfTypeID == "CBE7505E") || (cpfTypeID == "CBE750E0")) 
                                            {
                                                // Is an actual CPF file, so we have to decompress it...
                                                IncomingInformation = dbpfTypeRead.readCPFchunk(readFile);
                                            }

                                        }
                                    }
                                    break;
                            }

                            if ((this.title != "") || (this.xmlType != "")) break;

                                        }
                                    }
                                }
                            }

                            if (IncomingInformation.Type is "Title") {
                                temp.Name = IncomingInformation.Content;
                            } else if (IncomingInformation.Type is "Description") {
                                temp.Description = IncomingInformation.Content;
                            } else {
                                //
                            }

                            temp.Location = file;
                            
                            AllSimsPackages.Add(temp);

                            foreach (SimsPackage package in AllSimsPackages) {
                                statement = package.Location;
                                logGlobals.MakeLog(statement, false, false);
                                statement = package.Name;
                                logGlobals.MakeLog(statement, false, false);
                                statement = package.Description;
                                logGlobals.MakeLog(statement, false, false);
                            }     





        }











    } 
}