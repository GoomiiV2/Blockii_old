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

            ProjectTest();
            //Test2();
        }

        public static void ProjectTest()
        {
            var projManager = new ProjectManager("C://NonWindows//Projects//Blockii//TestMaps//UE4");
            Console.Read();
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
