using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using MoreLinq;

namespace MapParser.Common
{
    public class Brush
    {
        public List<BrushPlane> Planes = new List<BrushPlane>();

        public Vector3 GetCenter()
        {
            /*var points = new List<Vector3>();
            foreach (var plane in Planes)
            {
                points.Add(plane.Point1);
                points.Add(plane.Point2);
                points.Add(plane.Point3);
            }

            var max    = points.MaxBy(x => x.LengthSquared()).First();
            var min    = points.MinBy(x => x.LengthSquared()).First();
            var center = min - max;

            return center;*/

            var center = Vector3.Zero;
            foreach (var plane in Planes)
            {
                center += plane.GetCenter();
            }
            center /= Planes.Count;

            return center;
        }

        // Move center to 0,0,0 and return the old center location
        public Vector3 MakeLocalSpace()
        {
            var center = GetCenter();

            foreach (var plane in Planes)
            {
                plane.Point1 -= center;
                plane.Point2 -= center;
                plane.Point3 -= center;

                plane.ResetPlane();
            }

            return center;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("---- Brush ----");
            foreach (var plane in Planes)
            {
                sb.AppendLine($"{plane.Point1}, {plane.Point2}, {plane.Point3}");
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
