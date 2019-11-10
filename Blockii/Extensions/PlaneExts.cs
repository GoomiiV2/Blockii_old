using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Blockii.Extensions
{
    public static class PlaneExts
    {
        public enum PlanePos
        {
            InFront,
            Behind,
            On
        }

        public static PlanePos IsPointInfront(this Plane Plane, Vector3 Point)
        {
            var dot = Vector3.Dot(Plane.Normal, Point) + Plane.D;
            if (dot < -Config.General.Epsilon)
            {
                return PlanePos.InFront;
            }
            else if (dot > Config.General.Epsilon)
            {
                return PlanePos.Behind;
            }
            else
            {
                return PlanePos.On;
            }
        }
    }
}
