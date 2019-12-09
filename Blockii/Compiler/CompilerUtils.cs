using Blockii.DataTypes;
using Blockii.Extensions;
using MapParser.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Blockii.Compiler
{
    public static class CompilerUtils
    {
        // Borrowed from Sledge https://github.com/LogicAndTrick/sledge/blob/0a772be5cd8a9aefb9e3902633c8ce36c954f96d/Sledge.DataStructures/Geometric/Plane.cs
        public static Vector3? PlaneIntersection(Plane P1, Plane P2, Plane P3)
        {
            var c1 = Vector3.Cross(P2.Normal, P3.Normal);
            var c2 = Vector3.Cross(P3.Normal, P1.Normal);
            var c3 = Vector3.Cross(P1.Normal, P2.Normal);

            var denom = Vector3.Dot(P1.Normal, c1);
            if (denom < Config.General.Epsilon) return null; // No intersection, planes must be parallel

            var numer = (-P1.D * c1) + (-P2.D * c2) + (-P3.D * c3);
            return numer / denom;
        }

        public static Vector3 NormalFromVerts(List<Vertex> Verts)
        {
            if (Verts.Count >= 3)
            {
                var dir  = Vector3.Cross(Verts[1].Pos - Verts[0].Pos, Verts[2].Pos - Verts[0].Pos);
                var norm = Vector3.Normalize(dir);
                return norm;
            }
            else
            {
                return Vector3.Zero;
            }
        }

        public static Vector2 GetUVs(BrushPlane BrushPlane, Vector3 Pos, TextureInfo TexInfo)
        {
            var uvs = new Vector2();

            uvs.X  = Vector3.Dot(BrushPlane.TextureXAxis.Axis, Pos);
            uvs.X /= ((float)TexInfo.Width) * BrushPlane.TextureScale.X;
            uvs.X += BrushPlane.TextureXAxis.Offset / TexInfo.Width;

            uvs.Y = Vector3.Dot(BrushPlane.TextureYAxis.Axis, Pos);
            uvs.Y /= ((float)TexInfo.Height) * BrushPlane.TextureScale.Y;
            uvs.Y += BrushPlane.TextureYAxis.Offset / TexInfo.Height;

            /*var axis = new Vector2(Vector3.Dot(Pos, BrushPlane.TextureXAxis.Axis) * BrushPlane.TextureScale.X, Vector3.Dot(Pos, BrushPlane.TextureYAxis.Axis) * BrushPlane.TextureScale.Y);
            uvs.X = BrushPlane.TextureXAxis.Offset + (axis.X / TexInfo.Width);
            uvs.Y = BrushPlane.TextureYAxis.Offset + (axis.Y / TexInfo.Height);*/

            //Log.Information($"X: {BrushPlane.TextureXAxis.Axis}, Y: {BrushPlane.TextureYAxis.Axis}");
            //Log.Information($"XYZ: {Pos}, U: {uvs.X}, V: {uvs.Y}");

            return uvs;
        }

        public static string PrintMapInfo(string Path, World World)
        {
            var numEntities = World.Entitys.Count;
            var numBrushes  = World.Entitys.Select(x => x.Brushes.Count).Sum();

            var sb = new StringBuilder();
            sb.AppendLine("--------[ Map Info ]--------");
            sb.AppendLine($"Map File: {Path}");
            sb.AppendLine($"Map Type: {World.GameType}");
            sb.AppendLine($"Num Entitys: {numEntities}");
            sb.AppendLine($"Num Brushes: {numBrushes}");
            sb.AppendLine("----------------------------");
            sb.AppendLine();

            return sb.ToString();
        }

        // Check to see if this is a face that shouldn't be in the visable mesh
        public static bool ShouldExcludeFace(string TexName)
        {
            return Config.Conversion.ExcludeTextureNames.Any(x => TexName.Contains(x, StringComparison.OrdinalIgnoreCase));
        }

        // Get the image sizes for textures used in the map
        public static Dictionary<string, TextureInfo> BuildTextureInfoMap(World World)
        {
            var map         = new Dictionary<string, TextureInfo>();
            ushort texCount = 0;
            foreach (var entity in World.Entitys)
            {
                foreach (var brush in entity.Brushes)
                {
                    foreach (var plane in brush.Planes)
                    {
                        if (!map.ContainsKey(plane.TextureRef))
                        {
                            var texInfo = GetTextureInfo(World, plane.TextureRef);
                            if (texInfo != null)
                            {
                                texInfo.Id = texCount++;
                                map.Add(plane.TextureRef, texInfo);
                            }
                            else
                            {
                                Log.Warning($"Couldn't find texture for {plane.TextureRef}");
                                map.Add(plane.TextureRef, new TextureInfo()
                                {
                                    Width    = 128,
                                    Height   = 128,
                                    FileName = "Null",
                                    Id       = texCount++
                                });
                            }
                        }
                    }
                }
            }

            return map;
        }

        // Search the texture paths, for the texture folder for supported images, whew
        public static TextureInfo GetTextureInfo(World World, string MatRef)
        {
            var wadDirs    = World.Wads.Select(x => Path.GetFileNameWithoutExtension(x));
            var searchDirs = Config.General.TextureRoots.SelectMany(x => wadDirs.Select(y => Path.Combine(x, y))).ToArray();
            var dirs       = searchDirs.Count() >= 1 ? searchDirs.ToList() : Config.General.TextureRoots; // If no wads use the base dirs
            foreach (var dir in dirs)
            {
                foreach (var ext in Config.ImageExtensions)
                {
                    var path = Path.Combine(dir, $"{MatRef}.{ext}");
                    if (File.Exists(path))
                    {
                        var img = new Bitmap(System.Drawing.Image.FromFile(path));
                        var texRef = new TextureInfo()
                        {
                            Width    = img.Width,
                            Height   = img.Height,
                            FileName = path,
                            Exclude  = ShouldExcludeFace(path)
                        };

                        return texRef;
                    }
                }
            }
            return null;
        }
    }
}
