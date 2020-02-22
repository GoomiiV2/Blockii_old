using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MapParser.Common
{
    public class BrushPlane
    {
        public Vector3 Point1;
        public Vector3 Point2;
        public Vector3 Point3;

        public string TextureRef;
        public Vector2 TextureScale;
        public float TextureRotation;
        public TextureAxisInfo TextureXAxis;
        public TextureAxisInfo TextureYAxis;

        private Plane? NumericPlane = null;
        public Plane GetPlane()     => NumericPlane ??= Plane.CreateFromVertices(Point1, Point2, Point3);
        public Vector3 GetCenter()  => (Point1 + Point2 + Point3) / 3;

        public void ResetPlane()
        {
            NumericPlane = Plane.CreateFromVertices(Point1, Point2, Point3);
        }
    }

    public struct TextureAxisInfo
    {
        public Vector3 Axis;
        public float Offset;
    }
}
