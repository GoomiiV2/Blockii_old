using MapParser.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Blockii.Compiler.SimpleMesher
{
    public class SimpleMesher
    {

        public Vector3[] BrushToMesh(Brush Brush)
        {
            /*var verts = new List<Vector3>();
            var planes = Brush.Planes.Select(x => x.GetPlane());
            foreach (var p1 in planes)
            {
                foreach (var p2 in planes)
                {
                    foreach (var p3 in planes)
                    {
                        var vert = CompilerUtils.PlaneIntersection(p1, p2, p3);
                        if (vert != null) { verts.Add(vert.Value); }
                    }
                }
            }

            return verts.ToArray();*/

            /*var verts = new List<Vector3>();
            var polys = CompilerUtils.GetBrushPolys(Brush);
            foreach (var poly in polys.Skip(5))
            {
                poly.SortVerts();
                verts.AddRange(poly.Verts.Select(x => x.Pos).ToArray());
            }

            return verts.ToArray();*/

            return null;
        }

        public Vector3[] TestPoints(Brush Brush)
        {
            /*var polys = CompilerUtils.GetBrushPolys(Brush);
            var verts = new List<Vector3>();

            foreach (var poly in polys)
            {
                poly.SortVerts();
                verts.AddRange(poly.Verts.Select(x => x.Pos));
            }

            return verts.ToArray();*/

            return null;
        }
    }
}
