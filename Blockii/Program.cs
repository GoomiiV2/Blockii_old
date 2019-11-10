using Blockii.Compiler.SimpleMesher;
using Blockii.Exporters;
using MapParser.Quake1;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.IO;
using System.Linq;

namespace Blockii
{
    class Program
    {

        static void Main(string[] args)
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console()
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
            int mapIdx = 6;
            var maps = new string[]
            {
                "C://NonWindows//Projects//Blockii//TestMaps//TB_Quake1_Test1.map",
                "C://NonWindows//Projects//Blockii//TestMaps//TB_Quake1_TestBrushes.map",
                "C://NonWindows//Projects//Blockii//TestMaps//TB_Quake1_TroubleBrush.map",
                "C://NonWindows//Projects//Blockii//TestMaps//TB_Quake1_Valve_Box.map",
                "C://NonWindows//Projects//Meander//Map Src//AD//ad_sepulcher.map",
                "C://NonWindows//Projects//Meander//Map Src//AD//start.map",
                "C://NonWindows//Projects//Blockii//TestMaps//Quake//DM1.MAP"
            };

            var mapPath     = maps[mapIdx];
            var mapFileName = Path.GetFileNameWithoutExtension(mapPath);
            var importer    = new Q1MapImporterV2();
            var world       = importer.Import(mapPath);

            var exporter = new ModelExporter("C://NonWindows//Projects//Blockii//Export", mapFileName);
            exporter.ExportWorld(world);
        }
    }
}
