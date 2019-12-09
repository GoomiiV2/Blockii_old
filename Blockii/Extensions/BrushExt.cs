using Blockii.Compiler;
using Blockii.DataTypes;
using MapParser.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Blockii.Extensions
{
    public static class BrushExt
    {
        public static bool IsPointOutside(this Brush Brush, Vector3 Point)
        {
            var isOutsideBrush = false;
            for (int i = 0; i < Brush.Planes.Count; i++)
            {
                var plane = Brush.Planes[i].GetPlane();
                if (plane.IsPointInfront(Point) == PlaneExts.PlanePos.InFront)
                {
                    isOutsideBrush = true;
                }
            }

            return isOutsideBrush;
        }

        public static Poly[] GetBrushPolys(Brush Brush, ref Entity Entity, ref BrushConvData ConvData)
        {
            var polys = new Poly[Brush.Planes.Count];
            for (int i = 0; i < polys.Length; i++)
            {
                var texInfo = ConvData.TextureInfo[Brush.Planes[i].TextureRef];
                polys[i]    = new Poly()
                {
                    BrushFacePlane = Brush.Planes[i].GetPlane(),
                    TexID          = texInfo.Id,
                    Exclude        = texInfo.Exclude
                };
            }

            for (int i = 0; i < Brush.Planes.Count; i++)
            {
                for (int eye = 0; eye < Brush.Planes.Count; eye++)
                {
                    for (int aye = 0; aye < Brush.Planes.Count; aye++)
                    {
                        if (i != eye && eye != aye)
                        {
                            var interSection = CompilerUtils.PlaneIntersection(Brush.Planes[i].GetPlane(), Brush.Planes[eye].GetPlane(), Brush.Planes[aye].GetPlane());
                            if (interSection.HasValue)
                            {
                                var isOutside = Brush.IsPointOutside(interSection.Value);
                                if (!isOutside)
                                {
                                    var vert = new Vertex()
                                    {
                                        Pos = interSection.Value
                                    };

                                    polys[i].Verts.Add(vert);
                                    polys[eye].Verts.Add(vert);
                                    polys[aye].Verts.Add(vert);
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < Brush.Planes.Count; i++)
            {
                var poly     = polys[i];
                var plane    = Brush.Planes[i];
                var texInfo  = ConvData.TextureInfo[plane.TextureRef];
                for (int aye = 0; aye < poly.Verts.Count; aye++)
                {
                    var vert = poly.Verts[aye];
                    poly.Verts[aye] = new Vertex()
                    {
                        Pos = vert.Pos,
                        Uv  = CompilerUtils.GetUVs(plane, vert.Pos, texInfo)
                    };
                }
            }

            return polys;
        }

        public static BrushModel ToBrushModel(this Brush Brush, ref Entity Entity, ref BrushConvData ConvData)
        {
            var polys = GetBrushPolys(Brush, ref Entity, ref ConvData);
            var bMdl  = new BrushModel()
            {
                Polys = new List<Poly>(polys.Length)
            };

            foreach (var poly in polys)
            {
                // Check if this poly should be excluded
                if (!poly.Exclude)
                {
                    poly.SortVerts();
                    bMdl.Polys.Add(poly);
                }
            }

            bMdl.SetOrigin();

            return bMdl;
        }
    }
}
