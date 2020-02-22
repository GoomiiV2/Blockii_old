using MapParser.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace MapParser.Quake1
{
    public class Q1MapImporterV2
    {
        public const string WORLDSPAWN_STR = "WORLDSPAWN";

        private World World = new World();
        private bool IsValveFormat = false;

        public World Import(string Filepath)
        {
            World.GameType   = Game.Quake1;
            var mapFileLines = File.ReadAllLines(Filepath).AsSpan();
            ParseMap(mapFileLines);
            mapFileLines.Clear();

            return World;
        }

        private void ParseMap(ReadOnlySpan<string> Lines)
        {
            var sw              = Stopwatch.StartNew();
            int blockOpenCount  = 0;
            int entityStartLine = 0;
            for (int i = 0; i < Lines.Length; i++)
            {
                var newBlockOpenCount = blockOpenCount;
                var line          = Lines[i];

                if (IsBlockStart(line)) newBlockOpenCount++;
                if (IsBlockEnd(line)) newBlockOpenCount--;

                if (blockOpenCount == 0 && newBlockOpenCount == 1) // Entity start
                {
                    entityStartLine = i;
                }
                else if (blockOpenCount == 1 && newBlockOpenCount == 0) // Entity end
                {
                    var len         = i - entityStartLine;
                    var entitySlice = Lines.Slice(entityStartLine + 1, len);

                    var entity = ParseEntity(entitySlice);
                    World.Entitys.Add(entity);
                }

                blockOpenCount = newBlockOpenCount;
            }

            World.GameType = IsValveFormat ? Game.Halflife : Game.Quake1;

            sw.Stop();
            var mapTypeName = IsValveFormat ? "Half-Life 1" : "Quake 1";
            Log.Information($"Parsed {mapTypeName} Map in {sw.Elapsed.TotalSeconds} second");
        }

        private Entity ParseEntity(ReadOnlySpan<string> Lines)
        {
            //Log.Information("🧰 Reading Entity");

            var entity = new Entity();

            var isReadingBrush = false;
            int brushLineStart = 0;
            for (int i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i];

                if (IsBlockStart(line))
                {
                    isReadingBrush = true;
                    brushLineStart = i + 1; // Skip the first {
                }
                else if (isReadingBrush && IsBlockEnd(line))
                {
                    isReadingBrush = false;
                    var len        = i - brushLineStart;
                    var brushLines = Lines.Slice(brushLineStart, len);
                    var brush      = ParseBrush(brushLines);

                    entity.Brushes.Add(brush);
                }
                else if (!isReadingBrush && !IsLineComment(line) && !IsBlockEnd(line)) // Key value
                {
                    var (key, value) = ParseKeyValue(line);
                    entity.KeyValues.TryAdd(key, value);
                }
            }

            // Worldspawn
            if (entity.KeyValues.ContainsKey("CLASSNAME") && entity.KeyValues["CLASSNAME"].ToUpper() == WORLDSPAWN_STR)
            {
                if (entity.KeyValues.TryGetValue("WAD", out string wadsList))
                {
                    var wads = wadsList.Split(';')?.Where(x => x != "");
                    World.Wads.AddRange(wads);
                }
            }

            return entity;
        }

        private Brush ParseBrush(ReadOnlySpan<string> Lines)
        {
            //Log.Information("🧱 Reading Brush");

            var brush = new Brush();

            for (int i = 0; i < Lines.Length; i++)
            {
                var line  = Lines[i];
                var plane = ParsePlane(line);
                brush.Planes.Add(plane);
            }

            return brush;
        }

        private BrushPlane ParsePlane(string Line)
        {
            var stripedLine = Line.Replace("(", "").Replace(")", "");
            var segments    = stripedLine.Split(' ').Where(x => x != "").ToArray();
            int segmentIdx = 0;

            var plane = new BrushPlane()
            {
                Point1       = Utils.Vector3FromStrings(segments[segmentIdx++], segments[segmentIdx++], segments[segmentIdx++]),
                Point2       = Utils.Vector3FromStrings(segments[segmentIdx++], segments[segmentIdx++], segments[segmentIdx++]),
                Point3       = Utils.Vector3FromStrings(segments[segmentIdx++], segments[segmentIdx++], segments[segmentIdx++]),
                TextureRef   = segments[segmentIdx++]
            };

            if (IsValveFormat)
            {
                segmentIdx++; // skip over the [ bracket
                plane.TextureXAxis = new TextureAxisInfo()
                {
                    Axis   = Utils.Vector3FromStrings(segments[segmentIdx++], segments[segmentIdx++], segments[segmentIdx++]),
                    Offset = float.Parse(segments[segmentIdx++])
                };
                segmentIdx++;

                segmentIdx++;
                plane.TextureYAxis = new TextureAxisInfo()
                {
                    Axis   = Utils.Vector3FromStrings(segments[segmentIdx++], segments[segmentIdx++], segments[segmentIdx++]),
                    Offset = float.Parse(segments[segmentIdx++])
                };
                segmentIdx++;

                plane.TextureRotation = float.Parse(segments[segmentIdx++]);
                plane.TextureScale = Utils.Vector2FromStrings(segments[segmentIdx++], segments[segmentIdx++]);
            }
            else
            {
                // Need to convert to texture axis
                var qAxis = GetQuakeAxis(plane);
                plane.TextureXAxis = new TextureAxisInfo()
                {
                    Axis   = qAxis.TextureAxi.XAxis,
                    Offset = float.Parse(segments[segmentIdx++])
                };
                plane.TextureYAxis = new TextureAxisInfo()
                {
                    Axis   = qAxis.TextureAxi.YAxis,
                    Offset = float.Parse(segments[segmentIdx++])
                };

                plane.TextureRotation = float.Parse(segments[segmentIdx++]);
                plane.TextureScale    = Utils.Vector2FromStrings(segments[segmentIdx++], segments[segmentIdx++]);

                // Rotate the axis by Quake rotation
                var rotMatrix           = Matrix4x4.CreateFromAxisAngle(qAxis.QuakeAxis, (float)(Math.PI * plane.TextureRotation / 180.0));
                plane.TextureXAxis.Axis = Vector3.Transform(qAxis.TextureAxi.XAxis, rotMatrix);
                plane.TextureYAxis.Axis = Vector3.Transform(qAxis.TextureAxi.YAxis, rotMatrix);
            }

            return plane;
        }

        private static List<(Vector3 QuakeAxis, (Vector3, Vector3) TextureAxi)> anglesToAxis = new List<(Vector3, (Vector3, Vector3))> {
                ( Vector3.UnitZ,    (Vector3.UnitX, -Vector3.UnitY)), // Bottom
                (-Vector3.UnitZ,    (Vector3.UnitX, -Vector3.UnitY)), // Top
                ( Vector3.UnitX,    (Vector3.UnitY, -Vector3.UnitZ)), // Left
                (-Vector3.UnitX,    (Vector3.UnitY, -Vector3.UnitZ)), // Right
                ( Vector3.UnitY,    (Vector3.UnitX, -Vector3.UnitZ)), // Front
                (-Vector3.UnitY,    (Vector3.UnitX, -Vector3.UnitZ)), // Back
        };

        private (Vector3 QuakeAxis, (Vector3 XAxis, Vector3 YAxis) TextureAxi) GetQuakeAxis(BrushPlane Plane)
        {
            (int idx, float angle) best = (0, 100000);
            for (int i = 0; i < anglesToAxis.Count; i++)
            {
                var angleAxis = anglesToAxis[i];
                var dot       = 1 - Vector3.Dot(Plane.GetPlane().Normal, angleAxis.QuakeAxis);
                if (dot < best.angle)
                {
                    best = (i, dot);
                }
            }

            //Log.Information($"Face: {Plane.GetPlane().Normal}, Best Axis Normal: {anglesToAxis[best.idx].QuakeAxis}, Best Angle: {best.angle}");
            return anglesToAxis[best.idx];
        }

        private (string Key, string Value) ParseKeyValue(string Line)
        {
            //Log.Information("🔑 Reading Key value pair");
            var splitIdx = Line.IndexOf("\" \"");
            var key      = Line.Substring(1, splitIdx - 1);
            var value    = Line.Substring(splitIdx + 3, Line.Length - (splitIdx + 4));

            // Sorta hacky but we need to know if this is a Valve style map or not before we parse any planes
            if (IsValveMapFormat(key, value))
            {
                IsValveFormat = true;
            }

            //Log.Information($"{key}, {value}");

            return (key.ToUpper(), value);
        }

        #region Misc Utils
        bool IsLineComment(string Line) => Line.StartsWith("//", StringComparison.OrdinalIgnoreCase);
        bool IsBlockStart(string Line)  => Line.StartsWith("{", StringComparison.OrdinalIgnoreCase);
        bool IsBlockEnd(string Line)    => Line.StartsWith("}", StringComparison.OrdinalIgnoreCase);

        bool IsValveMapFormat(string Key, string Value) => Key.Equals("mapversion", StringComparison.InvariantCultureIgnoreCase)
                                                                && Value.Equals("220", StringComparison.InvariantCultureIgnoreCase);
        #endregion
    }
}
