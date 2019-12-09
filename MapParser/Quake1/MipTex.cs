using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static Formatii.Quake1.Wad2.Wad2;

namespace Formatii.Quake1
{
    public class MipTex
    {
        public const int MAX_MIP_LEVELS = 4;

        public string Name;
        public int Width;
        public int Height;
        public int[] MipsOffsets;
        public byte[][] Mips;

        public static MipTex Read(BinaryReader R, Entry Entry)
        {
            var tex = new MipTex()
            {
                Name        = new string(R.ReadChars(NAME_LEN)).Trim(new char[] { '\0', (char)0 }),
                Width       = R.ReadInt32(),
                Height      = R.ReadInt32(),
                Mips        = new byte[MAX_MIP_LEVELS][],
                MipsOffsets = new int [MAX_MIP_LEVELS]
            };

            for (int i = 0; i < MAX_MIP_LEVELS; i++) { tex.MipsOffsets[i] = R.ReadInt32(); }

            int numPixels = tex.Width * tex.Height;
            tex.Mips[0]   = R.ReadBytes(numPixels);
            tex.Mips[1]   = R.ReadBytes(numPixels / 4);
            tex.Mips[2]   = R.ReadBytes(numPixels / 16);
            tex.Mips[3]   = R.ReadBytes(numPixels / 64);

            return tex;
        }
    }
}
