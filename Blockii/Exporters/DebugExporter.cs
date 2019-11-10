using Blockii.Compiler;
using MapParser.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Blockii.Exporters
{
    public class DebugExporter
    {
        private const float Scale  = 0.02f;
        DebugData DebugOut         = new DebugData();

        public void AddWorldBrushesAsPoints(World World)
        {
            foreach (var entity in World.Entitys)
            {
                foreach (var brush in entity.Brushes)
                {
                    AddBrushAsPoints(brush);
                }
            }
        }

        public void AddBrushPlaneNormals(World World)
        {
            foreach (var entity in World.Entitys)
            {
                foreach (var brush in entity.Brushes)
                {
                    foreach (var plane in brush.Planes)
                    {
                        DebugOut.Arrorws.Add(new Arrow()
                        {
                            Pos    = plane.GetPlane().Normal * plane.GetPlane().D,
                            Normal = plane.GetPlane().Normal
                        });
                    }
                }
            }
        }

        public void AddBrushAsPoints(Brush Brush)
        {
            /*var polys = CompilerUtils.GetBrushPolys(Brush);

            foreach (var poly in polys)
            {
                DebugOut.Points.AddRange(poly.Verts.Select(x => x.Pos * Scale));
            }*/
        }

        public void Save(string Filepath)
        {
            var json = JsonConvert.SerializeObject(DebugOut/*, Formatting.Indented*/);
            File.WriteAllText(Filepath, json);
        }
    }

    public class DebugData
    {
        public List<Vector3> Points = new List<Vector3>();
        public List<Arrow> Arrorws  = new List<Arrow>();
    }

    public struct Arrow
    {
        public Vector3 Pos;
        public Vector3 Normal;
    }
}
