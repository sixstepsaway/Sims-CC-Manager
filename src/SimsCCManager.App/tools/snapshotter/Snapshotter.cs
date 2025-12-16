using Godot;
using MoreLinq;
using SimsCCManager.Debugging;
using SimsCCManager.Globals;
using SimsCCManager.PackageReaders;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Snapshotter : Node3D
{
    [Export]
    public MeshInstance3D MeshObject;
    [Export]
    public MeshInstance3D Box;



    public void BuildMesh(GMDCData gmdc)
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

        
        List<Vector3> FacesIdx = new();
        List<Vector3> NormalsIdx = new();
        List<Vector2> UVsIdx = new();

        int count = 0;

        foreach (GMDCGroup group in gmdc.Groups)
        {
            string Name = group.ObjectName;
            GMDCLinkage link = gmdc.Linkages[(int)group.LinkIndex];
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh {0} is {1}", count, Name));
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Original data:"));
            IList<IGMDCElement> Elements = [];
            foreach (uint indexvalue in link.IndexValues)
            {
                Elements.Add(gmdc.Elements[(int)indexvalue]);
            }

            foreach (IGMDCElement element in Elements)
            {
                if (element.Identity == "5B830781")
                {
                    GMDCElementVertices vertices = (GMDCElementVertices)element;
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Vertices: {0}", vertices.Vertices.Count));
                    foreach (Vector3 vert in vertices.Vertices)
                    {
                        Verts.Add(new Vector3(-vert.X, -vert.Y, vert.Z) * 0.05f);
                    }
                } else if (element.Identity == "3B83078B")
                {
                    GMDCElementNormals norms = (GMDCElementNormals)element;
                    if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Normals: {0}", norms.Normals.Count));
                    foreach (Vector3 norm in norms.Normals)
                    {
                        Normals.Add(norm);
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

            foreach (Vector3 face in group.Faces)
            {
                Indices.Add((int)face.X);
                Indices.Add((int)face.Y);
                Indices.Add((int)face.Z);
            }

            

            List<int> IndicesSorted = Indices.Distinct().Order().ToList();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("There are {0} Indices, {1} Normals, {2} UVs, {3} Verts", IndicesSorted.Count, Normals.Count, UVs.Count, Verts.Count));
            int indcount = IndicesSorted.Count;

            for (int i = 0; i < indcount; i++)
            {
                FacesIdx.Add(Verts[i]);
                UVsIdx.Add(UVs[i]);
                NormalsIdx.Add(Normals[i]);
            }

            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Faces list count: {0}", FacesIdx.Count));

            SurfaceTool surfaceTool = new();
            surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Group {0} has {1} faces.", count, group.Faces.Count));
            /*foreach (Vector3 face in group.Faces)
            {   
                surfaceTool.SetNormal(Normals[(int)face.X]);
                surfaceTool.SetUV(UVs[Indices[(int)face.X]]);
                surfaceTool.AddIndex((int)face.X);                
                surfaceTool.AddVertex(Verts[(int)face.X]);
                
                surfaceTool.SetNormal(Normals[(int)face.Y]);
                surfaceTool.SetUV(UVs[Indices[(int)face.Y]]);
                surfaceTool.AddIndex((int)face.Y);                
                surfaceTool.AddVertex(Verts[(int)face.Y]);

                
                surfaceTool.SetNormal(Normals[(int)face.Z]);
                surfaceTool.SetUV(UVs[Indices[(int)face.Z]]);
                surfaceTool.AddIndex((int)face.Z);                
                surfaceTool.AddVertex(Verts[(int)face.Z]);
            }*/
            int cc = 0;
            foreach (Vector3 face in group.Faces)
            {
                //surfaceTool.SetNormal(NormalsIdx[(int)face.X]);
                //surfaceTool.SetUV(UVsIdx[(int)face.X]);
                surfaceTool.AddIndex((int)face.X);
                surfaceTool.AddVertex(FacesIdx[(int)face.X]);
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Adding vertex 1 of face {0} at {1}", cc, FacesIdx[(int)face.X]));
                

                //surfaceTool.SetNormal(NormalsIdx[(int)face.Y]);
                //surfaceTool.SetUV(UVsIdx[(int)face.Y]);
                surfaceTool.AddIndex((int)face.Y);                           
                surfaceTool.AddVertex(FacesIdx[(int)face.Y]);
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Adding vertex 2 of face {0} at {1}", cc, FacesIdx[(int)face.Y]));
                
                
                //surfaceTool.SetNormal(NormalsIdx[(int)face.Z]);
                //surfaceTool.SetUV(UVsIdx[(int)face.Z]);
                surfaceTool.AddIndex((int)face.Z);                   
                surfaceTool.AddVertex(FacesIdx[(int)face.Z]);
                if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Adding vertex 3 of face {0} at {1}", cc, FacesIdx[(int)face.Z]));
                cc++;
            }

            //surfaceTool.Index();
            surfaceTool.GenerateNormals();
            
            MeshInstance3D newmesh = MeshObject.Duplicate() as MeshInstance3D; 
            newmesh.Mesh = surfaceTool.Commit();
            if (GlobalVariables.DebugMode) Logging.WriteDebugLog(string.Format("Mesh {0} has {1} faces.", count, newmesh.Mesh.GetFaces().Length)); 
            newmesh.Visible = true;
            AddChild(newmesh);
            ResourceSaver.Save(newmesh.Mesh, string.Format(@"res://{0}__MLCKFC.res", count));
            count++;
        }        
		
		
        
    }
}
