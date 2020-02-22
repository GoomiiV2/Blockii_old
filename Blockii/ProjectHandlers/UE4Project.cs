using Blockii.Compiler;
using Blockii.Exporters;
using MapParser.Quake1;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Blockii
{
    public class UE4Project : ProjectBase
    {

        public UE4Project(ProjectManager Manager) : base(Manager)
        {
            Config.General.TextureRoots.Add(Manager.TexturesDir);
            Config.Conversion.ExcludeTextureNames = new string[] { "CLIP", "TRIGGER", "SKIP" };
            Config.Conversion.UpAxis              = Vector3.UnitZ;
        }

        public override void OnMapEdited(string MapPath)
        {
            base.OnMapEdited(MapPath);
        }
    }
}
