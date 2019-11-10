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

            sw.Stop();
            Log.Information($"Parsed Quake1 Map in {sw.Elapsed.TotalSeconds} second");
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
                    entity.KeyValues.Add(key, value);
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
                plane.TextureXAxis = new TextureAxisInfo()
                {
                    Axis   = Vector3.UnitX,
                    Offset = float.Parse(segments[segmentIdx++])
                };
                plane.TextureYAxis = new TextureAxisInfo()
                {
                    Axis = Vector3.UnitY,
                    Offset = float.Parse(segments[segmentIdx++])
                };
                plane.TextureRotation = float.Parse(segments[segmentIdx++]);
                plane.TextureScale    = Utils.Vector2FromStrings(segments[segmentIdx++], segments[segmentIdx++]);
            }

            return plane;
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

            return (key, value);
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
