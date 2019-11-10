using MapParser.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blockii.Exporters
{
    public abstract class Exporter
    {
        public Exporter(string OutDir) { }
        public void ExportWorld(World World) { }
        public void ExportEntity(Entity Entity) { }
    }
}
