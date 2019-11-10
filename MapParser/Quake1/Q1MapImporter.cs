using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MapParser.Quake1
{
    public class Q1MapImporter
    {
        private ParseState ParserState = ParseState.FileStart;

        public void Import(string Filepath)
        {
            var mapFileLines = File.ReadAllLines(Filepath).AsSpan();

            ParseMap(mapFileLines);
        }

        private void ParseMap(Span<string> Lines)
        {
            for (int i = 0; i < Lines.Length; i++)
            {
                var line = Lines[i];
                var trimedLine = line.Trim();

                ChangeState(trimedLine);

                int entityStartLine = 0;
                switch (ParserState)
                {
                    case ParseState.EntityStart:
                        entityStartLine = i;
                        break;
                    case ParseState.EntityEnd:
                        var entityLines = Lines.Slice(entityStartLine, i);
                        ParseEntity(entityLines);
                        break;
                    default:
                        break;
                }
            }

            SetState(ParseState.FileEnd);
        }

        private void ParseEntity(Span<string> Lines)
        {
            Log.Information("Parsing entity");
        }

        private void ChangeState(string Line)
        {
            var isComment     = Line.StartsWith("//", StringComparison.OrdinalIgnoreCase);
            var isCurleyStart = Line.StartsWith("{", StringComparison.OrdinalIgnoreCase);
            var isCurleyEnd   = Line.StartsWith("}", StringComparison.OrdinalIgnoreCase);

            switch (ParserState)
            {
                case ParseState.FileStart:
                    if (isCurleyStart) { SetState(ParseState.EntityStart); }
                    break;
                case ParseState.FileEnd:
                    break;
                case ParseState.BrushEnd:
                case ParseState.EntityStart:
                    if (isCurleyStart) { SetState(ParseState.BrushStart); }
                    if (isCurleyEnd) { SetState(ParseState.EntityEnd); }
                    break;
                case ParseState.EntityEnd:
                    if (isCurleyStart) { SetState(ParseState.EntityStart); }
                    break;
                case ParseState.BrushStart:
                    if (isCurleyEnd) { SetState(ParseState.BrushEnd); }
                    break;
                default:
                    break;
            }
        }

        private void SetState(ParseState NewState)
        {
            Log.Information($"Changed state from {ParserState} to {NewState}");
            ParserState = NewState;
        }

        public enum ParseState
        {
            FileStart,
            FileEnd,
            EntityStart,
            EntityKeyValue,
            EntityEnd,
            BrushStart,
            BrushLine,
            BrushEnd
        }
    }
}
