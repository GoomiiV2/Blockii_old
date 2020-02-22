using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Blockii
{
    // What the target engine / thing is
    public enum ProjectTargets
    {
        UE4
    }

    public class General
    {
        public double Epsilon = 0.001d;
        public List<string> TextureRoots = new List<string>(); // Base folders to look for textures (wads should be extracted to folders of images)
    }

    public class Conversion
    {
        public ModelOriginPos ModelOrignPosition = ModelOriginPos.Center;
        public Vector3 UpAxis                    = Vector3.UnitZ;
        public bool AllOneFile                   = true;
        public string[] ExcludeTextureNames      = null; // Texture names to exclude from genrating faces, if the texture name contians this word it will be excluded
    }

    public static class Config
    {
        public static readonly string[] ImageExtensions = new string[] { "png", "jpg" };
        public static General General                   = new General();
        public static Conversion Conversion             = new Conversion();
        public static ProjectTargets ProjectTarget      = ProjectTargets.UE4;
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
