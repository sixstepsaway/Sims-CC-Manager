using Godot;
using MoreLinq;
using MoreLinq.Extensions;
using SimsCCManager.Containers;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.PackageReaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class Snapshotter : Node3D
{
    public List<SimsPackage> Packages = new();
    [Export]
    SubViewport subviewport;
    [Export]
    public MeshInstance3D Box;
    [Export]
    public MeshInstance3D AabBox;
    [Export]
    public MeshInstance3D Af;
    [Export]
    public MeshInstance3D Am;
    [Export]
    public MeshInstance3D Pu;
    [Export]
    public MeshInstance3D Cu;
    Aabb AabSize;
    List<MeshInstance3D> Meshes = new();

    public bool RenderDone = false;

    public delegate void SnapCompletedEvent();
    public SnapCompletedEvent SnapCompleted;

    Sims2Data data;
    SimsPackage thisPackage;
    List<TextureMatObjectMatch> textureMats = new();

    int meshType = -1; // 0 = object, 1 = hair

    SimsPackage texturePackage = new();

    bool foundTextures = false;

    List<Meshes> hairTypes = new();
    List<Meshes> bodyTypes = new();

    List<BodyTypeOption> BodyOptions = new();
    private int SelectedBodyOption = -1;

    public delegate void MultipleBodyOptionsEvent();
    public MultipleBodyOptionsEvent MultipleBodyOptions;

    List<string> S2HairAges = new(){"af", "yf", "ef", "tf", "cf", "am", "ym", "em", "tm", "cm", "pu"};
    List<string> S2BodyAges = new(){"af", "yf", "ef", "tf", "cf", "am", "ym", "em", "tm", "cm", "pu"};

    public void NextBodyOption()
    {
        ChangeBodyOption(1);
    }
    public void PreviousBodyOption()
    {
        ChangeBodyOption(-1);
    }

    public bool MultipleOptions = false;

    private void ChangeBodyOption(int change)
    {
        BodyOptions[SelectedBodyOption].Node.Visible = false;
        if (change == 1)
        {
            SelectedBodyOption++;
        } else
        {
            SelectedBodyOption--;
        }
        BodyOptions[SelectedBodyOption].Node.Visible = true;
    }

    private void S2ClothingMesh()
    {
        foreach (string type in S2BodyAges)
        {
            Node3D node3D = new();
            foreach (Meshes age in bodyTypes.Where(x => x.ItemType.Contains(type)))
            {
                if (age.MeshName.StartsWith("body_"))
                {
                    age.Mesh.Visible = false;
                } else
                {
                   age.Mesh.Visible = true; 
                }
                
                node3D.AddChild(age.Mesh);
                /*if (type == "af" || type == "ef" || type == "yf")
                {
                    node3D.AddChild(Af.Duplicate());
                    node3D.Position = new(0, -1.6f, 0);
                } else if (type == "am" || type == "em" || type == "ym")
                {
                    node3D.AddChild(Am.Duplicate());
                    node3D.Position = new(0, -1.6f, 0);
                } else if (type == "tf")
                {
                    MeshInstance3D tf = Af.Duplicate() as MeshInstance3D;
                    tf.Scale = new(0.94f, 0.94f, 0.94f);
                    node3D.Position = new(0, -0.4f, 0);
                    node3D.AddChild(tf);
                } else if (type == "tm" || type == "tu")
                {
                    MeshInstance3D tm = Am.Duplicate() as MeshInstance3D;
                    tm.Scale = new(9.4f, 9.4f, 9.4f);
                    node3D.Position = new(0, -0.4f, 0);
                    node3D.AddChild(tm);
                } else if (type == "cf" || type == "cm" || type == "cu")
                {
                    node3D.AddChild(Cu.Duplicate());
                    node3D.Position = new(0, -0.25f, 0);
                } else if (type == "pu" || type == "pf" || type == "pm")
                {
                    node3D.AddChild(Pu.Duplicate());
                    node3D.Position = new(0, 0.075f, 0);
                }*/
                age.ParentNode = node3D;
            }
            
            node3D.RotationDegrees = new(0, -75f, 0);
            node3D.Scale = new(3, 3, 3);
            node3D.Visible = false;
            node3D.Name = type;

            if (node3D.GetChildCount() > 0)
            {
                BodyOptions.Add(new() { Node = node3D, Identifier = type});
                AddChild(node3D); 
            } 
        }

        

        if (foundTextures)
        {
            foreach (Meshes body in bodyTypes)
            {
                TXTRData txtr = new();
                if (!string.IsNullOrEmpty(body.TextureName))
                {
                    txtr = texturePackage.Sims2Data.TXTRDataBlock.First(x => x.FullTXTRName.StartsWith(body.TextureName));

                }
                else
                {
                    txtr = texturePackage.Sims2Data.TXTRDataBlock.First(x => x.FullTXTRName.StartsWith(body.TextureFileName.Split('!')[0]));
                }

                StandardMaterial3D material = new();
                Texture2D mattxt = new();
                mattxt = ImageTexture.CreateFromImage(txtr.Texture);
                material.AlbedoTexture = mattxt;
                if (body.Alpha)
                    material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                if (flipepdInd)
                {
                    material.CullMode = BaseMaterial3D.CullModeEnum.Back;
                } else
                {
                    material.CullMode = BaseMaterial3D.CullModeEnum.Front;
                }
                
                body.Mesh.Mesh.SurfaceSetMaterial(0, material);
            }                                    
        }

        if (BodyOptions.Any(x => x.Identifier == "af"))
        {
            BodyOptions.First(x => x.Identifier == "af").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "yf"))
        {
            BodyOptions.First(x => x.Identifier == "yf").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "ef"))
        {
            BodyOptions.First(x => x.Identifier == "ef").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "tf"))
        {
            BodyOptions.First(x => x.Identifier == "tf").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "cf"))
        {
            BodyOptions.First(x => x.Identifier == "cf").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "am"))
        {
            BodyOptions.First(x => x.Identifier == "am").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "ym"))
        {
            BodyOptions.First(x => x.Identifier == "ym").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "em"))
        {
            BodyOptions.First(x => x.Identifier == "em").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "tm"))
        {
            BodyOptions.First(x => x.Identifier == "tm").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "cm"))
        {
            BodyOptions.First(x => x.Identifier == "cm").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "pu"))
        {
            BodyOptions.First(x => x.Identifier == "pu").Node.Visible = true;
        }

        

        SelectedBodyOption = BodyOptions.IndexOf(BodyOptions.First(x => x.Node.Visible));
        
        if (BodyOptions.Count > 1) MultipleOptions = true;
    }

    private void S2HairMesh()
    {
        foreach (string type in S2HairAges)
        {
            Node3D node3D = new();
            foreach (Meshes hair in hairTypes.Where(x => x.ItemType.Contains(type)))
            {
                node3D.AddChild(hair.Mesh);
                if (type == "af" || type == "ef" || type == "yf")
                {
                    node3D.AddChild(Af.Duplicate());
                    node3D.Position = new(0, -15f, 0);
                } else if (type == "am" || type == "em" || type == "ym")
                {
                    node3D.AddChild(Am.Duplicate());
                    node3D.Position = new(0, -15f, 0);
                } else if (type == "tf")
                {
                    MeshInstance3D tf = Af.Duplicate() as MeshInstance3D;
                    tf.Scale = new(0.94f, 0.94f, 0.94f);
                    node3D.Position = new(0, -13.7f, 0);
                    node3D.AddChild(tf);
                } else if (type == "tm" || type == "tu")
                {
                    MeshInstance3D tm = Am.Duplicate() as MeshInstance3D;
                    tm.Scale = new(9.4f, 9.4f, 9.4f);
                    node3D.Position = new(0, -13.7f, 0);
                    node3D.AddChild(tm);
                } else if (type == "cf" || type == "cm" || type == "cu")
                {
                    node3D.AddChild(Cu.Duplicate());
                    node3D.Position = new(0, -9.6f, 0);
                } else if (type == "pu" || type == "pf" || type == "pm")
                {
                    node3D.AddChild(Pu.Duplicate());
                    node3D.Position = new(0, -5f, 0);
                }
                hair.ParentNode = node3D;
            }
            
            node3D.RotationDegrees = new(0, -75f, 0);
            node3D.Scale = new(10, 10, 10);
            node3D.Visible = false;
            node3D.Name = type;

            if (node3D.GetChildCount() > 1)
            {
                BodyOptions.Add(new() { Node = node3D, Identifier = type});
                AddChild(node3D); 
            } 
        }

        

        if (foundTextures)
        {
            foreach (Meshes hair in hairTypes)
            {
                TXTRData txtr = new();
                if (!string.IsNullOrEmpty(hair.TextureName))
                {
                    txtr = texturePackage.Sims2Data.TXTRDataBlock.First(x => x.FullTXTRName.StartsWith(hair.TextureName));

                }
                else
                {
                    txtr = texturePackage.Sims2Data.TXTRDataBlock.First(x => x.FullTXTRName.StartsWith(hair.TextureFileName.Split('!')[0]));
                }

                StandardMaterial3D material = new();
                Texture2D mattxt = new();
                mattxt = ImageTexture.CreateFromImage(txtr.Texture);
                material.AlbedoTexture = mattxt;
                if (hair.Alpha)
                    material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                material.CullMode = BaseMaterial3D.CullModeEnum.Back;
                hair.Mesh.Mesh.SurfaceSetMaterial(0, material);
            }                                    
        }

        if (BodyOptions.Any(x => x.Identifier == "af"))
        {
            BodyOptions.First(x => x.Identifier == "af").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "yf"))
        {
            BodyOptions.First(x => x.Identifier == "yf").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "ef"))
        {
            BodyOptions.First(x => x.Identifier == "ef").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "tf"))
        {
            BodyOptions.First(x => x.Identifier == "tf").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "cf"))
        {
            BodyOptions.First(x => x.Identifier == "cf").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "am"))
        {
            BodyOptions.First(x => x.Identifier == "am").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "ym"))
        {
            BodyOptions.First(x => x.Identifier == "ym").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "em"))
        {
            BodyOptions.First(x => x.Identifier == "em").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "tm"))
        {
            BodyOptions.First(x => x.Identifier == "tm").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "cm"))
        {
            BodyOptions.First(x => x.Identifier == "cm").Node.Visible = true;
        } else if (BodyOptions.Any(x => x.Identifier == "pu"))
        {
            BodyOptions.First(x => x.Identifier == "pu").Node.Visible = true;
        }

        

        SelectedBodyOption = BodyOptions.IndexOf(BodyOptions.First(x => x.Node.Visible));
        
        if (BodyOptions.Count > 1) MultipleOptions = true;
    }

    bool flipepdInd = true;

    public void BuildSims2Mesh(SimsPackage d)
    {
        string type = d.Type;
        if (type.Contains("Hair"))
        {
            meshType = 1;
        }
        else if (type.Contains("Clothing"))
        {
            meshType = 2;
        } else
        {
            meshType = 0;
        }

        thisPackage = d;
        data = d.PackageData as Sims2Data;

        if (Packages.Any(x => x.Sims2Data.EIDRDataBlock.Any(e => e.ResourceKeys.Any(r => thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.FullKey == r.FullKey)))))
        {
            texturePackage = Packages.First(x => x.Sims2Data.EIDRDataBlock.Any(e => e.ResourceKeys.Any(r => thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.FullKey == r.FullKey))));
            foundTextures = true;
        }
      

        if (data.GMDCDataBlock.Count > 0)
        {
            Vector3 scale = new();
            Vector3 rotation = new(-90, 0, 0);   
  

            foreach (GMDCData gmdc in data.GMDCDataBlock)
            {
                int count = 0;

                string ref3 = gmdc.FileName.Replace("_tslocator_gmdc", "");

                foreach (GMDCGroup group in gmdc.Groups)
                {
                    string texturefilename = "";
                    string texturename = "";
                    string mattype = "";
                    bool alpha = false;
                    ArrayMesh newObject = new();
                    Godot.Collections.Array array = new();
                    array.Resize((int)Mesh.ArrayType.Max);
                    List<Vector3> Verts = new();
                    List<int> Indices = new();
                    List<Vector3> Normals = new();
                    List<Vector2> UVs = new();
                    List<float> Weights = new();
                    List<int> Bones = new();

                    string MeshName = group.ObjectName;
                    GMDCLinkage link = gmdc.Linkages[(int)group.LinkIndex];
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh {0} is {1}", count, MeshName));
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Original data:"));
                    IList<IGMDCElement> AllElements = gmdc.GetAllElements();

                    


                    IList<IGMDCElement> Elements = [];
                    foreach (IGMDCElement element in AllElements)
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Element index:{0}", element.Index));
                    }
                    foreach (uint indexvalue in link.IndexValues)
                    {
                        
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Finding element matching {0}", indexvalue));
                        if (AllElements.Any(x => x.Index == indexvalue))
                        {
                            Elements.Add(AllElements.First(x => x.Index == indexvalue));
                        }
                    }

                    foreach (IGMDCElement element in Elements)
                    {
                        if (element.Identity == "5B830781")
                        {
                            GMDCElementVertices vertices = (GMDCElementVertices)element;
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Vertices: {0}", vertices.Vertices.Count));
                            foreach (Vector3 vert in vertices.Vertices)
                            {
                                Verts.Add(new Vector3(-vert.X, -vert.Y, vert.Z));
                            }
                        }
                        else if (element.Identity == "3B83078B")
                        {
                            GMDCElementNormals norms = (GMDCElementNormals)element;
                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Normals: {0}", norms.Normals.Count));
                            foreach (Vector3 norm in norms.Normals)
                            {
                                Vector3 normalized = new(norm.X, norm.Y, norm.Z);
                                //if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Normals: {0}, Adjusted/Flipped: {1}, Normalized:{2}", norm, normalized, normalized.Normalized()));
                                //normalized = normalized.Normalized();
                                Normals.Add(normalized);
                            }
                        }
                        else if (element.Identity == "BB8307AB")
                        {
                            GMDCElementUVCoordinates UV = (GMDCElementUVCoordinates)element;
                            foreach (Vector2 uv in UV.UVCoordinates)
                            {
                                UVs.Add(uv);
                            }
                        }
                        else if (element.Identity == "3BD70105")
                        {
                            if (element.Length == 1)
                            {
                                GMDCElementSkinV1 skin = (GMDCElementSkinV1)element;
                                foreach (uint s in skin.SkinV1)
                                {
                                    Weights.Add(s);
                                }
                            }
                            else if (element.Length == 2)
                            {
                                GMDCElementSkinV2 skin = (GMDCElementSkinV2)element;
                                foreach (Vector2 s in skin.SkinV2)
                                {
                                    Weights.Add(s.X);
                                    Weights.Add(s.Y);
                                }
                            }
                            else if (element.Length == 3)
                            {
                                GMDCElementSkinV3 skin = (GMDCElementSkinV3)element;
                                foreach (Vector3 s in skin.SkinV3)
                                {
                                    Weights.Add(s.X);
                                    Weights.Add(s.Y);
                                    Weights.Add(s.Z);
                                }
                            }

                        }
                        else if (element.Identity == "FBD70111")
                        {
                            GMDCElementBoneAssignments bones = (GMDCElementBoneAssignments)element;
                            foreach (uint bone in bones.BoneAssignments)
                            {
                                Bones.Add((int)bone);
                            }
                        }
                    }

                    List<Vector3> NormalsIndexed = new();
                    foreach (Vector3 face in group.Faces)
                    {
                        Indices.Add((int)face.X);
                        Indices.Add((int)face.Y);
                        Indices.Add((int)face.Z);
                    }


                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("File {0}: Indices: {1}, Verts: {2}", d.Location, Indices.Count, Verts.Count));

                    Indices.Reverse();

                    MeshInstance3D newmesh = new();
                    Mesh meshinstance = new();

                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("File {0}, Mesh {1} has {2} Indices, {3} Verts, {4} UVs, {5} Normals", d.Location, Indices.Count, count, Verts.Count, UVs.Count, Normals.Count));

                    try
                    {
                        if (Indices.Count != 0)
                        {
                            array[(int)Mesh.ArrayType.Index] = Indices.ToArray();
                        } else
                        {
                           flipepdInd = false; 
                        }               
                        array[(int)Mesh.ArrayType.Vertex] = Verts.ToArray();
                        array[(int)Mesh.ArrayType.TexUV] = UVs.ToArray();
                        array[(int)Mesh.ArrayType.Normal] = Normals.ToArray();
                        newObject.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array);
                    }
                    catch (Exception e)
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Couldn't make mesh for {0}: {1}", d.Location, e.Message));
                    }

                    newmesh.Mesh = newObject;


                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh {0} has {1} faces.", count, newmesh.Mesh.GetFaces().Length));
                    newmesh.Visible = true;


                    newmesh.RotationDegrees = rotation;
                    newmesh.Name = MeshName;

                    //if (meshType != 1) AddChild(newmesh);
                    Meshes.Add(newmesh);







                    if (meshType == 1)
                    {
                        //use the gzps!!
                        string hairType = "";
                        if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("afhair"))))
                        {
                            //adult hair female
                            hairType = "afhair";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("yfhair"))))
                        {
                            //ya hair female
                            hairType = "yfhair";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("efhair"))))
                        {
                            //elder hair female
                            hairType = "efhair";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("tfhair"))))
                        {
                            //teen hair female
                            hairType = "tfhair";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("cfhair"))))
                        {
                            //child hair female
                            hairType = "cfhair";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("amhair"))))
                        {
                            //adult hair male
                            hairType = "amhair";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("emhair"))))
                        {
                            //elder hair male
                            hairType = "emhair";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("ymhair"))))
                        {
                            //ya hair male
                            hairType = "ymhair";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("tmhair"))))
                        {
                            //teen hair male
                            hairType = "tmhair";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("cmhair"))))
                        {
                            //child hair male
                            hairType = "cmfhair";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("puhair"))))
                        {
                            //toddler hair
                            hairType = "puhair";
                        }


                        if (texturePackage.Sims2Data.TXMTDataBlock.Any(x => x.FileName.Contains(hairType) && x.FileName.Contains(MeshName)))
                        {
                            foundTextures = true;
                            List<TXMTData> txmts = [.. texturePackage.Sims2Data.TXMTDataBlock.Where(x => x.FileName.Contains(hairType) && x.FileName.Contains(MeshName))];

                            XHTNData xhtn = texturePackage.Sims2Data.XHTNDataBlock.First(x => x.Name.Equals("blond", StringComparison.CurrentCultureIgnoreCase));

                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found XHTN: {0}", xhtn.FullKey));

                            EIDRData edir = texturePackage.Sims2Data.EIDRDataBlock.First(e => e.ResourceKeys.Any(r => r.FullKey == xhtn.FullKey));

                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found 3IDR: {0}", edir.FullKey));

                            StringBuilder sb = new();
                            foreach (ResourceKey key in edir.ResourceKeys)
                            {
                                sb.AppendLine(key.FullKey);
                            }

                            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Looking for any TXMT that matches any of: \n{0}", sb.ToString()));

                            TXMTData txmt = txmts.First(m => edir.ResourceKeys.Any(r => r.GroupID == m.GroupID));



                            texturefilename = txmt.FileName;
                            mattype = txmt.MaterialType;
                            if (txmt.MaterialProperties.Any(m => m.PropertyName == "stdMatBaseTextureName"))
                            {
                                texturename = txmt.MaterialProperties.First(m => m.PropertyName == "stdMatBaseTextureName").PropertyValue;

                            }
                            if (txmt.MaterialProperties.Any(m => m.PropertyName == "stdMatAlphaBlendMode"))
                            {
                                if (txmt.MaterialProperties.First(m => m.PropertyName == "stdMatAlphaBlendMode").PropertyValue == "blend")
                                {
                                    alpha = true;
                                }
                            }
                        }

                        if (S2HairAges.Any(x => ref3.Contains(x)))
                        {
                            hairType = S2HairAges.First(x => ref3.Contains(x));
                        }

                        if (hairType.StartsWith("af") || hairType.StartsWith("am") || hairType.StartsWith("ef") || hairType.StartsWith("em") || hairType.StartsWith("yf") || hairType.StartsWith("ym"))
                        {
                            newmesh.Name = string.Format("{0}_{1}", newmesh.Name, ref3);
                        }
                        else if (hairType.StartsWith("tf") || hairType.StartsWith("tm"))
                        {
                            newmesh.Name = string.Format("{0}_{1}", newmesh.Name, ref3);
                        }
                        else if (hairType.StartsWith("pu"))
                        {
                            newmesh.Name = string.Format("{0}_{1}", newmesh.Name, ref3);
                        }

                        newmesh.RotationDegrees = new(-90f, 90f, 0);

                        hairTypes.Add(new() { Mesh = newmesh, ItemType = hairType, MeshName = MeshName, FaceCount = newmesh.Mesh.GetFaces().Length, MeshFileName = ref3, TextureName = texturename, TextureFileName = texturefilename, Alpha = alpha });

                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh {0} is {1}", MeshName, hairType));


                        //newmesh.RotationDegrees += new Vector3(0, 15f, 0);

                    } else if (meshType == 2)
                    {
                        //use the gzps!!
                        string bodyType = "";
                        if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("af"))))
                        {
                            //adult body female
                            bodyType = "afbody";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("yf"))))
                        {
                            //ya body female
                            bodyType = "yfbody";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("ef"))))
                        {
                            //elder body female
                            bodyType = "efbody";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("tf"))))
                        {
                            //teen body female
                            bodyType = "tfbody";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("cf"))))
                        {
                            //child body female
                            bodyType = "cfbody";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("am"))))
                        {
                            //adult body male
                            bodyType = "ambody";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("em"))))
                        {
                            //elder body male
                            bodyType = "embody";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("ym"))))
                        {
                            //ya body male
                            bodyType = "ymbody";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("tm"))))
                        {
                            //teen body male
                            bodyType = "tmbody";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("cm"))))
                        {
                            //child body male
                            bodyType = "cmfbody";
                        }
                        else if (thisPackage.Sims2Data.SHPEDataBlock.Any(s => s.Materials.Any(m => m.MaterialDefinition.Contains("pu"))))
                        {
                            //toddler body
                            bodyType = "pubody";
                        }


                        if (texturePackage.Sims2Data.TXMTDataBlock.Any(x => x.FileName.Contains(bodyType) && x.FileName.Contains(MeshName)))
                        {
                            foundTextures = true;
                            List<TXMTData> txmts = [.. texturePackage.Sims2Data.TXMTDataBlock.Where(x => x.FileName.Contains(bodyType) && x.FileName.Contains(MeshName))];

                            foreach (TXMTData txmt in txmts)
                            {
                               texturefilename = txmt.FileName;
                                mattype = txmt.MaterialType;
                                if (txmt.MaterialProperties.Any(m => m.PropertyName == "stdMatBaseTextureName"))
                                {
                                    texturename = txmt.MaterialProperties.First(m => m.PropertyName == "stdMatBaseTextureName").PropertyValue;
                                }
                                if (txmt.MaterialProperties.Any(m => m.PropertyName == "stdMatAlphaBlendMode"))
                                {
                                    if (txmt.MaterialProperties.First(m => m.PropertyName == "stdMatAlphaBlendMode").PropertyValue == "blend")
                                    {
                                        alpha = true;
                                    }
                                } 
                            }
                            
                        }

                        if (S2BodyAges.Any(x => ref3.Contains(x)))
                        {
                            bodyType = S2BodyAges.First(x => ref3.Contains(x));
                        }

                        if (bodyType.StartsWith("af") || bodyType.StartsWith("am") || bodyType.StartsWith("ef") || bodyType.StartsWith("em") || bodyType.StartsWith("yf") || bodyType.StartsWith("ym"))
                        {
                            newmesh.Name = string.Format("{0}_{1}", newmesh.Name, ref3);
                        }
                        else if (bodyType.StartsWith("tf") || bodyType.StartsWith("tm"))
                        {
                            newmesh.Name = string.Format("{0}_{1}", newmesh.Name, ref3);
                        }
                        else if (bodyType.StartsWith("pu"))
                        {
                            newmesh.Name = string.Format("{0}_{1}", newmesh.Name, ref3);
                        }

                        newmesh.RotationDegrees = new(-90f, 90f, 0);

                        bodyTypes.Add(new() { Mesh = newmesh, ItemType = bodyType, MeshName = MeshName, FaceCount = newmesh.Mesh.GetFaces().Length, MeshFileName = ref3, TextureName = texturename, TextureFileName = texturefilename, Alpha = alpha });

                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh {0} is {1}", MeshName, bodyType));
                    }

                    if (meshType != 1)
                    {
                        if (foundTextures)
                        {
                            TXTRData txtr = new();
                            if (!string.IsNullOrEmpty(texturename))
                            {
                                txtr = texturePackage.Sims2Data.TXTRDataBlock.First(x => x.FullTXTRName.StartsWith(texturename));

                            }
                            else
                            {
                                txtr = texturePackage.Sims2Data.TXTRDataBlock.First(x => x.FullTXTRName.StartsWith(texturefilename.Split('!')[0]));
                            }

                            StandardMaterial3D material = new();
                            Texture2D mattxt = new();
                            mattxt = ImageTexture.CreateFromImage(txtr.Texture);
                            material.AlbedoTexture = mattxt;
                            if (alpha)
                                material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                            material.CullMode = BaseMaterial3D.CullModeEnum.Front;
                            newmesh.Mesh.SurfaceSetMaterial(0, material);
                            newmesh.Visible = false;                        
                        }
                    }
                    count++;
                }
            }

            if (meshType == 1) { 
                S2HairMesh(); 
            } else if (meshType == 2)
            {
                S2ClothingMesh();
            } else {
                SetMeshScale();
            }            
        }
    }

    public void ApplyS2Textures(SimsPackage texturePackage)
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Getting textures for {0}, using GUID {1}", data.FileLocation, data.GUID));

        MMATData mmat = null;
        XNGBData xngb = null;
        if ((texturePackage.PackageData as Sims2Data).MMATDataBlock.Count > 0) mmat = (texturePackage.PackageData as Sims2Data).MMATDataBlock[0];
        if ((texturePackage.PackageData as Sims2Data).XNGBDataBlock != null) xngb = (texturePackage.PackageData as Sims2Data).XNGBDataBlock;

        if (mmat != null)
        {
            string meshname = mmat.SubsetName;
            string TextureName = mmat.Name;
            if (TextureName.Contains('!')) TextureName = TextureName.Split('!')[^1];
            if (TextureName.Contains('_')) TextureName = TextureName.Split('_')[0];
            if (TextureName.Contains('-')) TextureName = TextureName.Split('-')[0];
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Got MMAT for {0}. Looking for texture matching {1}", mmat.SubsetName, TextureName));

            if ((texturePackage.PackageData as Sims2Data).TXTRDataBlock.Any(t => t.FullTXTRName.StartsWith(TextureName)))
            {
                TXTRData txtr = (texturePackage.PackageData as Sims2Data).TXTRDataBlock.First(t => t.FullTXTRName.StartsWith(TextureName));
                if (Meshes.Any(x => x.Name == meshname))
                {
                    StandardMaterial3D material = new();
                    Texture2D mattxt = new();
                    mattxt = ImageTexture.CreateFromImage(txtr.Texture);
                    material.AlbedoTexture = mattxt;
                    Meshes.First(x => x.Name == meshname).Mesh.SurfaceSetMaterial(0, material);
                }

            }
        }
        else if (xngb != null)
        {
            string meshname = xngb.ModelName.Split('-')[^1];
            string TextureName = xngb.ModelName;
            if (TextureName.Contains('!')) TextureName = TextureName.Split('!')[^1];
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Got XNGB for {0}. Looking for texture matching {1}", xngb.ModelName, TextureName));

            if ((texturePackage.PackageData as Sims2Data).TXTRDataBlock.Any(t => t.FullTXTRName.StartsWith(TextureName)))
            {
                TXTRData txtr = (texturePackage.PackageData as Sims2Data).TXTRDataBlock.First(t => t.FullTXTRName.StartsWith(TextureName));
                StandardMaterial3D material = new();
                Texture2D mattxt = new();
                mattxt = ImageTexture.CreateFromImage(txtr.Texture);
                material.AlbedoTexture = mattxt;
                if (Meshes.Any(x => x.Name == meshname))
                {
                    Meshes.First(x => x.Name == meshname).Mesh.SurfaceSetMaterial(0, material);
                }
                else if (Meshes.Any(x => x.Name.ToString().Contains(meshname)))
                {
                    Meshes.First(x => x.Name.ToString().Contains(meshname)).Mesh.SurfaceSetMaterial(0, material);
                }
            }
        }
        else
        {
            return;
        }

    }

    public void Snapshot(Window window, string ImageFolder, string PackageName)
    {
        if (!data.Recolor && !data.Mesh)
        {
            if (data.FunctionSort.Count > 0)
            {
                if (data.FunctionSort[0].Category.Equals("wall", StringComparison.CurrentCultureIgnoreCase) || data.FunctionSort[0].Category.Equals("floor", StringComparison.CurrentCultureIgnoreCase) || data.FunctionSort[0].Category.Equals("terrainpaint", StringComparison.CurrentCultureIgnoreCase))
                {
                    List<TXTRData> txtrs = data.TXTRDataBlock;
                    int c = 0;
                    foreach (TXTRData txtr in txtrs)
                    {
                        string output = Path.Combine(ImageFolder, string.Format("{0}_{1}.jpg", PackageName, c));
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saving texture image {0}", output));
                        txtr.Texture.SaveJpg(output);
                        c++;
                    }
                }
            }
        }
        else if (textureMats.Count > 0)
        {
            int count = 0;
            foreach (TextureMatObjectMatch match in textureMats)
            {
                if (match.RecolorCount == count)
                {
                    StandardMaterial3D material = new();
                    if (match.Texture != null)
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Creating material from image: {0} (Data size: {1})", match.TextureName, match.Texture.GetDataSize()));
                        Texture2D mattxt = new();
                        mattxt = ImageTexture.CreateFromImage(match.Texture);
                        material.AlbedoTexture = mattxt;
                        match.Mesh.MaterialOverride = material;
                    }
                    else
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Material image {0} is null.", match.TextureName));
                    }

                }
                else
                {
                    if (PackageName != match.PackageName)
                    {
                        string output = Path.Combine(ImageFolder, string.Format("{0}_{1}_{2}.png", PackageName, match.PackageName, count));
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saving screenshot {0}", output));
                        Task ready = ScreenshotAsync(window, output);
                    }
                    else
                    {
                        string output = Path.Combine(ImageFolder, string.Format("{0}_{1}.png", PackageName, count));
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saving screenshot {0}", output));
                        Task ready = ScreenshotAsync(window, output);
                    }
                    count++;
                }
            }
        }
        else
        {
            string output = Path.Combine(ImageFolder, string.Format("{0}.png", PackageName));
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saving screenshot {0}", output));
            Task ready = ScreenshotAsync(window, output);
            //ScreenSnap(window, output);
        }
        RenderDone = true;
        SnapCompleted.Invoke();
    }


    private void ApplyS2Textures(SimsPackage package, List<TXTRData> txtrData)
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Getting textures for {0}, using GUID {1}", data.FileLocation, data.GUID));

        string textureName = "";
        MMATData mmat = null;
        XNGBData xngb = null;
        if ((package.PackageData as Sims2Data).MMATDataBlock.Count > 0) mmat = (package.PackageData as Sims2Data).MMATDataBlock[0];
        if ((package.PackageData as Sims2Data).XNGBDataBlock != null) xngb = (package.PackageData as Sims2Data).XNGBDataBlock;

        if (mmat != null)
        {
            string meshname = mmat.SubsetName;
            string TextureName = mmat.Name;
            if (TextureName.Contains('!')) TextureName = TextureName.Split('!')[^1];
            if (TextureName.Contains('_')) TextureName = TextureName.Split('_')[0];
            if (TextureName.Contains('-')) TextureName = TextureName.Split('-')[0];
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Got MMAT for {0}. Looking for texture matching {1}", mmat.SubsetName, TextureName));

            if (txtrData.Any(t => t.FullTXTRName.StartsWith(TextureName)))
            {
                TXTRData txtr = txtrData.First(t => t.FullTXTRName.StartsWith(TextureName));
                if (Meshes.Any(x => x.Name == meshname))
                {
                    StandardMaterial3D material = new();
                    Texture2D mattxt = new();
                    mattxt = ImageTexture.CreateFromImage(txtr.Texture);
                    material.AlbedoTexture = mattxt;
                    if (Meshes.Any(x => x.Name == meshname))
                    {
                        Meshes.First(x => x.Name == meshname).Mesh.SurfaceSetMaterial(0, material);
                    }
                    else if (Meshes.Any(x => x.Name.ToString().Contains(meshname)))
                    {
                        Meshes.First(x => x.Name.ToString().Contains(meshname)).Mesh.SurfaceSetMaterial(0, material);
                    }
                }

            }
        }
        else if (xngb != null)
        {
            string meshname = xngb.ModelName.Split('-')[^1];
            string TextureName = xngb.ModelName;
            if (TextureName.Contains('!')) TextureName = TextureName.Split('!')[^1];
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Got XNGB for {0}. Looking for texture matching {1}", xngb.ModelName, TextureName));

            if (txtrData.Any(t => t.FullTXTRName.StartsWith(TextureName)))
            {
                TXTRData txtr = txtrData.First(t => t.FullTXTRName.StartsWith(TextureName));
                StandardMaterial3D material = new();
                Texture2D mattxt = new();
                mattxt = ImageTexture.CreateFromImage(txtr.Texture);
                material.AlbedoTexture = mattxt;
                if (Meshes.Any(x => x.Name == meshname))
                {
                    Meshes.First(x => x.Name == meshname).Mesh.SurfaceSetMaterial(0, material);
                }
                else if (Meshes.Any(x => x.Name.ToString().Contains(meshname)))
                {
                    Meshes.First(x => x.Name.ToString().Contains(meshname)).Mesh.SurfaceSetMaterial(0, material);
                }
            }
        }
        else
        {
            return;
        }
    }


    public void GetTexturesForS2Meshes(SimsPackage package)
    {
        if ((package.PackageData as Sims2Data).MMATDataBlock.Count != 0)
        {
            if (package.Sims2Data.TXTRDataBlock.Count != 0)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh package has textures."));
                ApplyS2Textures(package);
            }
            else
            {
                string TextureName = package.Sims2Data.MMATDataBlock[0].Name;
                if (TextureName.Contains('!')) TextureName = TextureName.Split('!')[^1];
                if (TextureName.Contains('_')) TextureName = TextureName.Split('_')[0];
                if (TextureName.Contains('-')) TextureName = TextureName.Split('-')[0];

                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh package has no textures. Searching for {0}", TextureName));

                if (Packages.Any(x => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName.Contains(TextureName, StringComparison.CurrentCultureIgnoreCase))))
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found texture matching {0}", TextureName));
                    ApplyS2Textures(package, Packages.First(x => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName.Contains(TextureName, StringComparison.CurrentCultureIgnoreCase))).Sims2Data.TXTRDataBlock);
                }
            }

        }
        else if ((package.PackageData as Sims2Data).XNGBDataBlock != null)
        {
            if (package.Sims2Data.TXTRDataBlock.Count != 0)
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh package has textures."));
                ApplyS2Textures(package);
            }
            else
            {
                string TextureName = package.Sims2Data.XNGBDataBlock.ModelName;
                if (TextureName.Contains('!')) TextureName = TextureName.Split('!')[^1];
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh package has no textures. Searching for {0}", TextureName));

                if (Packages.Any(x => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName.Contains(TextureName, StringComparison.CurrentCultureIgnoreCase))))
                {
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found texture matching {0}", TextureName));
                    ApplyS2Textures(package, Packages.First(x => x.Sims2Data.TXTRDataBlock.Any(t => t.FullTXTRName.Contains(TextureName, StringComparison.CurrentCultureIgnoreCase))).Sims2Data.TXTRDataBlock);
                }
            }
        }
        else if (Packages.Any(x => x.Sims2Data.EIDRDataBlock.Any(e => e.ResourceKeys.Any(r => package.Sims2Data.IndexEntries.Any(i => i.CompleteID == r.FullKey)))))
        {
            ApplyS2BodyTextures(package, Packages.First(x => x.Sims2Data.EIDRDataBlock.Any(e => e.ResourceKeys.Any(r => package.Sims2Data.IndexEntries.Any(i => i.CompleteID == r.FullKey)))));
        }
        else
        {
            if (Packages.Where(x => x.ObjectGUID == package.ObjectGUID).Any(p => p.Recolor))
            {
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Found a package matching {0}", package.ObjectGUID));
                SimsPackage matchingMesh = Packages.Where(x => x.ObjectGUID == package.ObjectGUID).First(p => p.Recolor);
                ApplyS2Textures(matchingMesh);
            }
        }
    }

    private void ApplyS2BodyTextures(SimsPackage meshPackage, SimsPackage texturePackage)
    {
        if (meshPackage.Sims2Data.SHPEDataBlock.Count > 0)
        {
            foreach (EIDRData edir in meshPackage.Sims2Data.EIDRDataBlock)
            {
                SHPEData shpe = new();
                List<TXMTData> txmt = new();


                if (meshPackage.Sims2Data.SHPEDataBlock.Any(x => edir.ResourceKeys.Any(r => x.FullKey == r.FullKey)))
                {
                    shpe = meshPackage.Sims2Data.SHPEDataBlock.First(x => edir.ResourceKeys.Any(r => x.FullKey == r.FullKey));

                }

                if (texturePackage.Sims2Data.TXMTDataBlock.Any(x => edir.ResourceKeys.Any(r => x.FullKey == r.FullKey)))
                {
                    txmt.Add(meshPackage.Sims2Data.TXMTDataBlock.First(x => edir.ResourceKeys.Any(r => x.FullKey == r.FullKey)));
                }


            }
        }

    }

    private void ScreenSnap(Window window, string ImageLoc)
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Loaded! Snapshotting."));
        Image image = window.GetTexture().GetImage();
        image.SavePng(ImageLoc);
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Done!"));
    }

    public async Task ScreenshotAsync(Window window, string ImageLoc)
    {
        /*if (this.IsNodeReady())
        {
            await WaitLoad(window, ImageLoc);
        } else
        {
            await ScreenshotAsync(window, ImageLoc);
        }*/
        await WaitLoad(window, ImageLoc);
    }

    private async Task<Task> WaitLoad(Window window, string ImageLoc)
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Waiting..."));
        if (this.IsNodeReady())
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Node is already ready. Awaiting frame."));
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        }
        else
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Node isn't ready! Awaiting postdraw."));
            await ToSignal(RenderingServer.Singleton, RenderingServerInstance.SignalName.FramePostDraw);
        }
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Loaded! Snapshotting."));
        Image image = window.GetTexture().GetImage();
        image.SavePng(ImageLoc);
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Done!"));
        return Task.CompletedTask;
    }

    private void SetMeshScale()
    {
        Node3D ContainerNode = new();
        Aabb boxAab = AabBox.GetAabb();
        Vector3 smallest = new(0, 0, 0);
        MeshInstance3D smallestMesh = new();

        foreach (MeshInstance3D mesh in Meshes)
        {
            ContainerNode.AddChild(mesh);
            Aabb ab = mesh.GetAabb();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh {0} size is {1}", mesh.Name, ab.Size));
            if (ab.Size < smallest)
            {
                smallestMesh = mesh;
                smallest = ab.Size;
            }
        }

        AddChild(ContainerNode);

        Vector3 scale = new(0, 0, 0);
        Aabb meshaab = smallestMesh.GetAabb();
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("MeshAABB: {0}, BoxAABB: {1}", meshaab.Size, boxAab.Size));
        if (meshaab.Size == Vector3.Zero)
        {
            scale = boxAab.Size;
        } else if (meshaab.Size > boxAab.Size)
        {
            scale = meshaab.Size/boxAab.Size;
        } else
        {
            scale = boxAab.Size/meshaab.Size;
        }

        ContainerNode.Scale = scale;
    }
}

public class TextureMatObjectMatch
{
    public int RecolorCount { get; set; }
    public string PackageName { get; set; }
    public string ObjectGuid { get; set; }
    public MeshInstance3D Mesh { get; set; }
    public string MeshName { get; set; }
    public string TextureName { get; set; }
    public Image Texture { get; set; }
}

public class Meshes
{
    public Node3D ParentNode {get; set;}
    public MeshInstance3D Mesh {get; set;}
    public string ItemType {get; set;}
    public string MeshName {get; set;}
    public string MeshFileName {get; set;}
    public string TextureName {get; set;}
    public string TextureFileName {get; set;}
    public int FaceCount {get; set;}
    public bool Alpha {get; set;}
}

public class BodyTypeOption {
    public string Identifier {get; set;}
    public Node3D Node {get; set;}
}