using Blockii.Compiler;
using Blockii.Extensions;
using MoreLinq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using static Blockii.Extensions.PlaneExts;

namespace Blockii.DataTypes
{
    // A poly for a face of a brush
    public class Poly
    {
        public List<Vertex> Verts = new List<Vertex>();
        public Vector3 Center;
        public ushort TexID;
        public string TextureRef;
        public Plane BrushFacePlane;
        public bool Exclude; // If true this poly won't be exported

        public void GenerateCenter() 
        {
            for (int i = 0; i < Verts.Count; i++)
            {
                Center += Verts[i].Pos;
            }

            Center = Center / Verts.Count;
        }

        public void SortVerts()
        {
            Verts = Verts.Distinct().ToList();

            if (Verts.Count >= 3)
            {
                var facePlane = Plane.CreateFromVertices(Verts[0].Pos, Verts[1].Pos, Verts[2].Pos);
                GenerateCenter();

                /*for (int i = 0; i < Verts.Count; i++)
                {
                    Verts[i] = new Vertex()
                    {
                        Pos = Verts[i].Pos - Center,
                        Uv  = Verts[i].Uv
                    };
                }*/

                //GenerateCenter();

                for (int i = 0; i < Verts.Count - 2; i++)
                {
                    var vert = Verts[i];
                    var norm = Vector3.Normalize(vert.Pos - Center);
                    var plane = Plane.CreateFromVertices(vert.Pos, Center, Center + facePlane.Normal);
                    (double angle, int idx) smallest = (-1, -1);

                    for (int aye = i + 1; aye < Verts.Count; aye++)
                    {
                        var vert2 = Verts[aye];
                        var pointPos = plane.IsPointInfront(vert2.Pos);
                        if (pointPos != PlanePos.Behind)
                        {
                            var norm2 = Vector3.Normalize(vert2.Pos - Center);
                            var angle = Vector3.Dot(norm, norm2);

                            if (angle > smallest.angle || smallest.idx == -1)
                            {
                                smallest = (angle, aye);
                            }
                        }
                    }

                    if (smallest.idx != -1)
                    {
                        var tempVert = Verts[smallest.idx];
                        Verts[smallest.idx] = Verts[i + 1];
                        Verts[i + 1] = tempVert;
                    }
                }

                var middleIdx     = Math.Abs(Verts.Count / 2);
                var facePlane2    = Plane.CreateFromVertices(Verts[0].Pos, Verts[middleIdx].Pos, Verts[Verts.Count-1].Pos);
                var faceDot       = Vector3.Dot(BrushFacePlane.Normal, facePlane2.Normal);

                //Log.Information($"sort verts dot: {faceDot} faceDot > 0.001f {faceDot > 0.001f} ({faceDot - 0.001f})");

                if (faceDot > 0.001f)
                {
                    Verts.Reverse();
                }
            }
            else
            {
                //Log.Warning($"Got an invalid poly with {Verts.Count} verts");
            }
        }
    }
}
