using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Blockii.Exporters
{
    // Convert to and from assimp types and numeric types
    public static class AssimpExtentions
    {
        public static Vector3 ToNumeric(this Assimp.Vector3D AssVec)
        {
            var vec = new Vector3(AssVec.X, AssVec.Y, AssVec.Z);
            return vec;
        }

        public static Assimp.Vector3D ToAss(this Vector3 NumVec)
        {
            var vec = new Assimp.Vector3D(NumVec.X, NumVec.Y, NumVec.Z);
            return vec;
        }

        public static Assimp.Vector3D ToAssV3(this Vector2 NumVec)
        {
            var vec = new Assimp.Vector3D(NumVec.X, NumVec.Y, 0);
            return vec;
        }
    }
}
