using Assimp;
using Blockii.Compiler;
using Blockii.DataTypes;
using Blockii.Extensions;
using MapParser.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Blockii.Exporters
{
#if FALSE
    public class ModelExporter
    {
        public static async void ExportWorld(World World)
        {
            using (var asmpCntx = new AssimpContext())
            {
                var scene = new Scene();
                scene.RootNode = new Node("Root");
                scene.RootNode.Transform = Assimp.Matrix4x4.FromEulerAnglesXYZ((float)(Math.PI * 90 / 180.0f), 0, 0);

                int meshIdx = 0;
                //Parallel.ForEach(World.Entitys.Where(x => x.Brushes.Count > 0), async (x) =>
                foreach (var x in World.Entitys.Where(x => x.Brushes.Count > 0))
                {
                    //try
                    {
                        var mesh = await BuildEntityModel(x);
                        mesh.MaterialIndex = 0;

                        scene.Meshes.Add(mesh);
                        scene.RootNode.MeshIndices.Add(meshIdx++);
                    }
                    //catch { }
                }

                Material mat = new Material();
                mat.Name = "MyMaterial";
                scene.Materials.Add(mat);

                asmpCntx.ExportFile(scene, $"test.obj", "obj", PostProcessSteps.GenerateNormals);
            }
        }

        public static async Task<Mesh> BuildEntityModel(Entity Entity)
        {
            var mesh = new Mesh("EntityMesh");

            int faceIdx = 0;
            foreach (var brush in Entity.Brushes)
            {
                BrushToMeshSimple(brush, ref mesh, ref faceIdx);
            }

            return mesh;

        }

        // A simple brush to mesher, doesn't do csg
        public static void BrushToMeshSimple(Brush Brush, ref Mesh Mesh, ref int FaceIdx)
        {
            var polys = CompilerUtils.GetBrushPolys(Brush);
            foreach (var poly in polys/*.Skip(1).Take(1)*/)
            {
                poly.SortVerts();
                //var uniquieVerts = poly.Verts.Distinct();
                Mesh.Vertices.AddRange(poly.Verts.Select(x => x.Pos.ToAss()));

                int numFaces     = (poly.Verts.Count() - 3) + 1;
                int startFaceIdx = FaceIdx;
                //Mesh.Faces.Add(new Face(new int[] { startFaceIdx, startFaceIdx + 1, startFaceIdx + 2 }));
                for (int i = startFaceIdx + 1; i < startFaceIdx + numFaces + 1; i++)
                {
                    Mesh.Faces.Add(new Face(new int[] { startFaceIdx, i, i + 1 }));
                    //Log.Information($"Face: {startFaceIdx}, {i}, {i + 1} ");
                }

                FaceIdx += poly.Verts.Count();
            }
        }



    public static void ExportPointsTest(Vector3[] Verts)
        {
            using (var asmpCntx = new AssimpContext())
            {
                var scene      = new Scene();
                scene.RootNode = new Node("Root");
                var mesh       = new Mesh("Test Verts", PrimitiveType.Triangle);

                var verts = Verts.Select(x => x.ToAss()).Distinct();
                mesh.Vertices.AddRange(verts);

                var numFaces = (verts.Count() - 3) + 1;
                mesh.Faces.Add(new Face(new int[] { 0, 1, 2 }));
                for (int i = 2; i < numFaces + 1; i++)
                {
                    mesh.Faces.Add(new Face(new int[] { 0, i, i + 1 }));
                }

                mesh.MaterialIndex = 0;

                scene.Meshes.Add(mesh);
                scene.RootNode.MeshIndices.Add(0);

                Material mat = new Material();
                mat.Name = "MyMaterial";
                scene.Materials.Add(mat);

                asmpCntx.ExportFile(scene, "test.obj", "obj");
            }
        }
    }
#endif

    public class ModelExporter
    {
        private string Name;
        private string OutDir;
        private int ExportedMeshIdx = 0;
        private BrushConvData ConvData;

        public ModelExporter(string OutputDir, string Name)
        {
            this.Name = Name;
            OutDir    = OutputDir;

            Directory.CreateDirectory(OutputDir);
        }

        public void ExportWorld(World World)
        {
            var entities = World.Entitys.Where(x => x.Brushes.Count > 0);
            /*Parallel.ForEach(entities, (entity) =>
            {
                var mesh = EntityToMesh(entity);
                SaveMesh(mesh);
            });*/

            foreach (var entity in entities)
            {
                var mesh = EntityToMesh(entity);
                SaveMesh(mesh);
            }
        }

        private Mesh EntityToMesh(Entity Entity)
        {
            var mesh    = new Mesh("EntityMesh");
            int vertIdx = 0;

            foreach (var brush in Entity.Brushes)
            {
                var bmdl = brush.ToBrushModel(ref Entity, ref ConvData);
                foreach (var poly in bmdl.Polys)
                {
                    foreach (var vert in poly.Verts)
                    {
                        mesh.Vertices.Add(vert.Pos.ToAss());
                        mesh.Normals.Add(poly.BrushFacePlane.Normal.ToAss());
                    }

                    int numFaces = (poly.Verts.Count() - 3) + 1;
                    for (int i = vertIdx + 1; i < vertIdx + numFaces + 1; i++)
                    {
                        mesh.Faces.Add(new Face(new int[] { vertIdx, i, i + 1 }));
                    }

                    vertIdx += poly.Verts.Count;
                }
            }

            return mesh;
        }

        private void SaveMesh(Mesh Mesh)
        {
            using (var asmpCntx = new AssimpContext())
            {
                var idx                  = ExportedMeshIdx++;
                var name                 = $"{Name}_{idx}.obj";
                var exportPath           = Path.Combine(OutDir, name);
                var scene                = new Scene();
                scene.RootNode           = new Node("Root");
                scene.RootNode.Transform = Assimp.Matrix4x4.FromEulerAnglesXYZ((float)(Math.PI * 90 / 180.0f), 0, 0);


                Mesh.MaterialIndex = 0;
                scene.Meshes.Add(Mesh);
                scene.RootNode.MeshIndices.Add(0);

                Material mat = new Material();
                mat.Name = "MyMaterial";
                scene.Materials.Add(mat);

                asmpCntx.ExportFile(scene, exportPath, "obj", PostProcessSteps.GenerateUVCoords);
            }
        }
    }
}
