using Blockii.Compiler;
using Blockii.Exporters;
using MapParser.Quake1;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Blockii
{
    public abstract class ProjectBase
    {
        public readonly ProjectManager ProjManager = null;

        public ProjectBase(ProjectManager Manager)
        {
            ProjManager = Manager;
        }

        public virtual void OnMapEdited(string MapPath)
        {
            var sw = Stopwatch.StartNew();
            var importer                 = new Q1MapImporterV2();
            MapParser.Common.World world = importer.Import(MapPath);
            Log.Information($"Map parsed in: {sw.Elapsed}");

            var mapInfo= CompilerUtils.PrintMapInfo(MapPath, world);
            Log.Information(mapInfo);

            var worldSpawn = world.WorldspawnGetter;
            if (worldSpawn == default) { Log.Warning($"Couldn't find worldspawn entity for map: {MapPath}"); return; }

            var mapName  = Path.GetFileNameWithoutExtension(MapPath);
            var outDir   = Path.Combine(ProjManager.MeshesSrcDir, mapName);
            var exporter = new ModelExporter(outDir, mapName);
            exporter.ExportWorld(world);

            sw.Stop();
            Log.Information($"Total conversion took: {sw.Elapsed}");

            OnMapCompiled(exporter.ExportedMeshPaths);

            // Assimp seems to be abit clingy to resources so i need to force a collect to not have it start to eat gigs of ram :/
            GC.Collect();
        }

        public virtual void OnMapCompiled(string[] OutPutMeshPaths)
        {

        }
    }
}
