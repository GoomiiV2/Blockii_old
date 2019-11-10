using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MapParser.Common
{
    public static class Utils
    {
        public static Vector3 Vector3FromStrings(string X, string Y, string Z)
        {
            var vec = new Vector3()
            {
                X = float.Parse(X),
                Y = float.Parse(Y),
                Z = float.Parse(Z)
            };

            return vec;
        }

        public static Vector2 Vector2FromStrings(string X, string Y)
        {
            var vec = new Vector2()
            {
                X = float.Parse(X),
                Y = float.Parse(Y)
            };

            return vec;
        }
    }
}
