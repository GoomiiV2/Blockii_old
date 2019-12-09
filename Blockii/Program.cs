using Blockii.Compiler;
using Blockii.Compiler.SimpleMesher;
using Blockii.Exporters;
using MapParser.Quake1;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Blockii
{
    class Program
    {

        static void Main(string[] args)
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console(theme: ConsoleTheme.None)
                //.WriteTo.File("log.txt")
                .CreateLogger();

            Log.Logger = log;

            //TestQuakeMapParse();
            Test2();
        }

        public static void TestQuakeMapParse()
        {
            //var map      = "C://NonWindows//Projects//Blockii//TestMaps//TB_Quake1_Test1.map";
            var map = "C://NonWindows//Projects//Blockii//TestMaps//TB_Quake1_TestBrushes.map";
            //var map      = "C://NonWindows//Projects//Blockii//TestMaps//TB_Quake1_Valve_Box.map";
            //var map      = "C://NonWindows//Projects//Meander//Map Src//AD//ad_sepulcher.map";
            //var map      = "C://NonWindows//Projects//Meander//Map Src//AD//start.map";
            var importer = new Q1MapImporterV2();
            var world    = importer.Import(map);

            /*var debugExp = new DebugExporter();
            debugExp.AddWorldBrushesAsPoints(world);
            //debugExp.AddBrushPlaneNormals(world);
            debugExp.Save("Debug.json");*/

            //var mesher    = new SimpleMesher();
            //var verts = mesher.BrushToMesh(world.Entitys[0].Brushes[0]); //.Distinct();
            //var verts = mesher.TestPoints(world.Entitys[0].Brushes[0]).Distinct();
            /*ModelExporter.ExportPointsTest(verts.ToArray());
            foreach (var vert in verts)
            {
                Console.WriteLine($"({vert.X}, {vert.Y}, {vert.Z})");
            }*/

            //ModelExporter.ExportWorld(world);
        }

        public static void Test2()
        {
            var sw = Stopwatch.StartNew();

            int mapIdx = 10;
            var maps = new string[]
            {
                "C://NonWindows//Projects//Blockii//TestMaps//TB_Quake1_Test1.map",
                "C://NonWindows//Projects//Blockii//TestMaps//TB_Quake1_TestBrushes.map",
                "C://NonWindows//Projects//Blockii//TestMaps//TB_Quake1_TroubleBrush.map",
                "C://NonWindows//Projects//Blockii//TestMaps//TB_Quake1_Valve_Box.map",
                "C://NonWindows//Projects//Blockii//TestMaps//TB_HalfLife_Test.map",
                "C://NonWindows//Projects//Meander//Map Src//AD//ad_sepulcher.map",
                "C://NonWindows//Projects//Meander//Map Src//AD//start.map",
                "C://NonWindows//Projects//Blockii//TestMaps//Quake//DM1.MAP",
                "C://NonWindows//Projects//Blockii//TestMaps//Half-Life/c1a0.map",
                "C://NonWindows//Projects//Blockii//TestMaps//TB_Quake1_Box.map",
                "C://NonWindows//Projects//Blockii//TestMaps//TB_Genric_Vavle220_box.map",
            };

            Config.General.TextureRoots.Add("C:/NonWindows/Projects/Blockii/TestMaps/textures");
            Config.Conversion.ExcludeTextureNames = new string[] { "CLIP", "TRIGGER", "SKIP" };
            var mapPath                           = maps[mapIdx];
            var mapFileName                       = Path.GetFileNameWithoutExtension(mapPath);
            var importer                          = new Q1MapImporterV2();
            var world                             = importer.Import(mapPath);
            var mapInfo                           = CompilerUtils.PrintMapInfo(mapPath, world);
            Log.Information(mapInfo);

            var exporter = new ModelExporter("C://NonWindows//Projects//Blockii//Export", mapFileName);
            exporter.ExportWorld(world);

            sw.Stop();
            Log.Information($"Took total: {sw.Elapsed}");
        }
    }
}
