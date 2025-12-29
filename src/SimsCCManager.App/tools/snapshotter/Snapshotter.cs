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
using System.IO;
using System.Linq;
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
    Aabb AabSize;
    List<MeshInstance3D> Meshes = new();

    public bool RenderDone = false;

    public delegate void SnapCompletedEvent();
    public SnapCompletedEvent SnapCompleted;

    Sims2Data data;
    SimsPackage thisPackage;
    List<TextureMatObjectMatch> textureMats = new();
    

    public void BuildSims2Mesh(SimsPackage d)
    {
        thisPackage = d;
        data = d.PackageData as Sims2Data;
        if (data.GMDCDataBlock.Count > 0)
        {            
            Vector3 scale = new();
            Vector3 rotation = new(-90, 0, 0);
            foreach (GMDCData gmdc in data.GMDCDataBlock)
            {            
                int count = 0;

                foreach (GMDCGroup group in gmdc.Groups)
                {
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
                        Elements.Add(AllElements.First(x => x.Index == indexvalue));
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
                        } else if (element.Identity == "3B83078B")
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
                        } else if (element.Identity == "BB8307AB")
                        {
                            GMDCElementUVCoordinates UV = (GMDCElementUVCoordinates)element;
                            foreach (Vector2 uv in UV.UVCoordinates)
                            {
                                UVs.Add(uv);
                            }
                        } else if (element.Identity == "3BD70105")
                        {
                            if (element.Length == 1)
                            {
                                GMDCElementSkinV1 skin = (GMDCElementSkinV1)element;
                                foreach (uint s in skin.SkinV1)
                                {
                                    Weights.Add(s);
                                }
                            } else if (element.Length == 2)
                            {
                                GMDCElementSkinV2 skin = (GMDCElementSkinV2)element;
                                foreach (Vector2 s in skin.SkinV2)
                                {
                                    Weights.Add(s.X);
                                    Weights.Add(s.Y);
                                }
                            } else if (element.Length == 3)
                            {
                                GMDCElementSkinV3 skin = (GMDCElementSkinV3)element;
                                foreach (Vector3 s in skin.SkinV3)
                                {
                                    Weights.Add(s.X);
                                    Weights.Add(s.Y);
                                    Weights.Add(s.Z);
                                }
                            }
                            
                        } else if (element.Identity == "FBD70111")
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

                    try { 
                        array[(int)Mesh.ArrayType.Index] = Indices.ToArray();
                        array[(int)Mesh.ArrayType.Vertex] = Verts.ToArray();
                        array[(int)Mesh.ArrayType.TexUV] = UVs.ToArray();
                        array[(int)Mesh.ArrayType.Normal] = Normals.ToArray();
                        newObject.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, array);
                    } catch (Exception e) {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Couldn't make mesh for {0}: {1}", d.Location, e.Message ));
                    }
                    //newObject.RegenNormalMaps();
                    newmesh.Mesh = newObject;

                    //SurfaceTool surface = new();
                    //surface.Begin(Mesh.PrimitiveType.Triangles);
                    //surface.CreateFromArrays(array, Mesh.PrimitiveType.Triangles);
                    //surface.Index();
                    //newmesh.Mesh = surface.Commit();
                    
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh {0} has {1} faces.", count, newmesh.Mesh.GetFaces().Length)); 
                    newmesh.Visible = true;


                    newmesh.RotationDegrees = rotation;
                    newmesh.Name = MeshName;

                    
                    
                    
                    AddChild(newmesh);
                    Meshes.Add(newmesh);

                    
                    count++;
                }        
            }		
            
            SetMeshScale();
        }
    }   

    public void ApplyTextures(SimsPackage texturePackage)
    {
        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Getting textures for {0}, using GUID {1}", data.FileLocation, data.GUID));
        
        MMATData mmat = (texturePackage.PackageData as Sims2Data).MMATDataBlock[0];
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
        } else
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
        } else if (textureMats.Count > 0)
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
                    } else
                    {
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Material image {0} is null.", match.TextureName));
                    }
                                      
                } else
                {
                    if (PackageName != match.PackageName)
                    {
                        string output = Path.Combine(ImageFolder, string.Format("{0}_{1}_{2}.png", PackageName, match.PackageName, count));
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saving screenshot {0}", output));
                        Task ready = ScreenshotAsync(window, output);
                    } else
                    {
                        string output = Path.Combine(ImageFolder, string.Format("{0}_{1}.png", PackageName, count));
                        if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saving screenshot {0}", output));
                        Task ready = ScreenshotAsync(window, output); 
                    }                    
                    count++;
                }
            } 
        } else 
        {
            string output = Path.Combine(ImageFolder, string.Format("{0}.png", PackageName));
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Saving screenshot {0}", output));
            Task ready = ScreenshotAsync(window, output); 
            //ScreenSnap(window, output);
        }
        RenderDone = true;
        SnapCompleted.Invoke();        
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

    private async Task<Task> WaitLoad(Window window, string ImageLoc){
		if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Waiting..."));
        if (this.IsNodeReady())
        {
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Node is already ready. Awaiting frame."));
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		    await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        } else
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
        Aabb boxAab = AabBox.GetAabb();
        Vector3 biggest = new(0, 0, 0);
        MeshInstance3D biggestMesh = new();        
        foreach (MeshInstance3D mesh in Meshes)
        {
            Aabb ab = mesh.GetAabb();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh {0} size is {1}", mesh.Name, ab.Size));
            if (ab.Size > biggest)
            {
                biggestMesh = mesh;
                biggest = ab.Size;
            }
        }


        Aabb meshaab = biggestMesh.GetAabb();
        if (meshaab.Size < boxAab.Size)
        {
            biggestMesh.Scale *= (boxAab.Size / meshaab.Size);
        }

        Vector3 scale = biggestMesh.Scale;

        foreach (MeshInstance3D mesh in Meshes)
        {
            mesh.Scale = scale;
        }
    }
}

public class TextureMatObjectMatch
{
    public int RecolorCount {get; set;}
    public string PackageName {get; set;}
    public string ObjectGuid {get; set;}
    public MeshInstance3D Mesh {get; set;}
    public string MeshName {get; set;}
    public string TextureName {get; set;}    
    public Image Texture {get; set;}
}
