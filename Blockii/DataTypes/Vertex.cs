using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Blockii.DataTypes
{
    public struct Vertex
    {
        public Vector3 Pos;
        public Vector2 Uv;

        public override bool Equals(object obj)
        {
            var other = (Vertex)obj;
            return Math.Abs(Vector3.DistanceSquared(Pos, other.Pos)) <= Config.General.Epsilon && Math.Abs(Vector2.DistanceSquared(Uv, other.Uv)) <= Config.General.Epsilon;
        }

        // Not great
        
    }
}
