using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Blockii
{
    public class ProjectManager
    {
        public string ProjectDir { get; private set; }
        public string TexturesDir  => Path.Combine(ProjectDir, "textures");
        public string MapsSrcDir   => Path.Combine(ProjectDir, "maps");
        public string MeshesSrcDir => Path.Combine(ProjectDir, "meshes");
        public string Settings     => Path.Combine(ProjectDir, "settings.json");

        private FileSystemWatcher MapsDirWatcher = null;
        private ProjectBase ProjectHandler       = null;

        public ProjectManager(string Dir)
        {
            ProjectDir = Dir;
            WatchMaps();
            ProjectHandler = GetProjectHander();
        }

        private void WatchMaps()
        {
            MapsDirWatcher      = new FileSystemWatcher();
            MapsDirWatcher.Path = MapsSrcDir;

            MapsDirWatcher.NotifyFilter          = NotifyFilters.LastWrite;
            MapsDirWatcher.Filter                = "*.map";
            MapsDirWatcher.IncludeSubdirectories = true;
            MapsDirWatcher.Changed              += OnMapCHanged;
            MapsDirWatcher.EnableRaisingEvents   = true;
        }

        private void OnMapCHanged(object sender, FileSystemEventArgs e)
        {
            MapsDirWatcher.EnableRaisingEvents = false;
            var task = Task.Factory.StartNew(async () =>
            {
                // TODO: Fix better, its for the file not being ready to read yet
                await Task.Delay(2000);
                var relPath = Path.GetRelativePath(MapsSrcDir, e.FullPath);
                Log.Information($"Map changed: {relPath}");
                ProjectHandler.OnMapEdited(e.FullPath);
                MapsDirWatcher.EnableRaisingEvents = true;
            });  
        }

        private ProjectBase GetProjectHander()
        {
            switch (Config.ProjectTarget)
            {
                case ProjectTargets.UE4:
                    return new UE4Project(this);

                default:
                    return null;
            }
        }
    }
}
