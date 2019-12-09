using Formatii.Common;
using Formatii.Quake1;
using Formatii.Quake1.Wad2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Wadii
{
    public static class Converter
    {
        public static void ExtractFromWad2(string WadPath, string PalettePath, string OutDir)
        {
            var outPath = Path.Combine(OutDir, Path.GetFileNameWithoutExtension(WadPath));
            Directory.CreateDirectory(outPath);
            var wad     = new Wad2(WadPath);
            var palette = LoadPalette(PalettePath);
            var listing = wad.GetFileList();
            Console.WriteLine(listing);

            for (int i = 0; i < wad.Entries.Length; i++)
            {
                var entry = wad.Entries[i];
                if (entry.Type == Wad2.EntryType.MipsTexture)
                {
                    var path   = Path.Combine(outPath, entry.Name);
                    var mipTex = wad.GetMipTex(entry);
                    SaveMipTex(mipTex, ref palette, path);
                }
            }
        }

        public static Color24[] LoadPalette(string Palettle)
        {
            var palColors = new Color24[256];
            var palette   = File.ReadAllBytes(Palettle);
            int idx = 0;

            for (int i = 0; i < palette.Length; i += 3)
            {
                palColors[idx++] = new Color24()
                {
                    R = palette[i],
                    G = palette[i + 1],
                    B = palette[i + 2]
                };
            }

            return palColors;
        }

        public static void SaveMipTex(MipTex MipTex, ref Color24[] Palette, string OutPath, ImageFormat ImgFormat = null, bool SaveMips = false)
        {
            ImgFormat = ImgFormat ?? ImageFormat.Png;

            if (!SaveMips)
            {
                SaveMipTexMipLevel(MipTex, ref Palette, OutPath, 0, ImgFormat);
            }
            else
            {
                for (int i = 0; i < MipTex.MAX_MIP_LEVELS; i++)
                {
                    SaveMipTexMipLevel(MipTex, ref Palette, OutPath, i, ImgFormat);
                }
            }
        }

        public static void SaveMipTexMipLevel(MipTex MipTex, ref Color24[] Palette, string OutPath, int MipLevel, ImageFormat ImgFormat)
        {
            var img       = new Bitmap(MipTex.Width, MipTex.Height);
            var mipPixels = MipTex.Mips[MipLevel];
            var imgData   = img.LockBits(new Rectangle(0, 0, MipTex.Width, MipTex.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var pixels    = new byte[mipPixels.Length * 4];

            int pixelIdx = 0;
            for (int i = 0; i < mipPixels.Length; i++)
            {
                var paletteIdx          = mipPixels[i];
                var color               = Palette[paletteIdx];
                pixels[pixelIdx++]      = color.B;
                pixels[pixelIdx++]      = color.G;
                pixels[pixelIdx++]      = color.R;
                pixels[pixelIdx++]      = (byte)(paletteIdx == 255 ? 0 : 255);
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, imgData.Scan0, pixels.Length);
            img.UnlockBits(imgData);

            // Need to strip out chars that windows won't like
            // TODO: Create a mapping for these to something else?
            var pathWithExt = $"{OutPath}.png".Replace('*', ' ');
            img.Save(pathWithExt, ImgFormat);
        }
    }
}
