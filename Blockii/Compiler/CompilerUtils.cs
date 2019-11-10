using Blockii.DataTypes;
using Blockii.Extensions;
using MapParser.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Blockii.Compiler
{
    public static class CompilerUtils
    {
        // Borrowed from Sledge https://github.com/LogicAndTrick/sledge/blob/0a772be5cd8a9aefb9e3902633c8ce36c954f96d/Sledge.DataStructures/Geometric/Plane.cs
        public static Vector3? PlaneIntersection(Plane P1, Plane P2, Plane P3)
        {
            var c1 = Vector3.Cross(P2.Normal, P3.Normal);
            var c2 = Vector3.Cross(P3.Normal, P1.Normal);
            var c3 = Vector3.Cross(P1.Normal, P2.Normal);

            var denom = Vector3.Dot(P1.Normal, c1);
            if (denom < Config.General.Epsilon) return null; // No intersection, planes must be parallel

            var numer = (-P1.D * c1) + (-P2.D * c2) + (-P3.D * c3);
            return numer / denom;
        }

        public static Vector3 NormalFromVerts(List<Vertex> Verts)
        {
            if (Verts.Count >= 3)
            {
                var dir  = Vector3.Cross(Verts[1].Pos - Verts[0].Pos, Verts[2].Pos - Verts[0].Pos);
                var norm = Vector3.Normalize(dir);
                return norm;
            }
            else
            {
                return Vector3.Zero;
            }
        }
    }
}
