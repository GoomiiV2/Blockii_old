using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Blockii
{
    public class General
    {
        public double Epsilon = 0.0001d;
    }

    public class Conversion
    {
        public ModelOriginPos ModelOrignPosition = ModelOriginPos.Center;
        public Vector3 UpAxis                    = Vector3.UnitZ;
        public bool AllOneFile                   = true;
    }

    public static class Config
    {
        public static General General       = new General();
        public static Conversion Conversion = new Conversion();
    }

    public enum ModelOriginPos
    {
        Center,
        BottmLeft,
        BottomRight,
        TopLeft,
        TopRight
    }
}
