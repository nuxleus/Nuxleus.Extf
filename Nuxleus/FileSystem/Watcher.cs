using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Permissions;

namespace Nuxleus.FileSystem
{
    public class Watcher : FileSystemWatcher
    {
      Entity _entity = null;
        string _path;
        TextWriter _logWriter;
        NotifyFilters _notifyFilters;
        string _filter;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	  public Watcher(Entity entity, string path, string filter, TextWriter logWriter)
        {
	  _entity = entity;
            _path = path;
            _logWriter = logWriter;
            _filter = filter;
            _notifyFilters =
                NotifyFilters.LastAccess    |
                NotifyFilters.LastWrite     |
                NotifyFilters.FileName      |
                NotifyFilters.DirectoryName |
                NotifyFilters.Size          |
                NotifyFilters.CreationTime  |
                NotifyFilters.Attributes;
        }

        public TextWriter LogWriter { get { return _logWriter; } set { _logWriter = value; } }
        public string Folder { get { return _path; } set { _path = value; } }
        public NotifyFilters NotifyFilters { get { return _notifyFilters; } set { _notifyFilters = value; } }
        public string FileFilter { get { return _filter; } set { _filter = value; } }
	public Entity Entity { get { return this._entity; } set { this._entity = value; } }

        public void Watch(bool watchSubDirectories)
        {
            base.Path = _path;
            base.NotifyFilter = _notifyFilters;

            base.IncludeSubdirectories = watchSubDirectories;
            base.Filter = _filter;

            this.Changed += new FileSystemEventHandler(OnChanged);
            this.Created += new FileSystemEventHandler(OnCreated);
            this.Deleted += new FileSystemEventHandler(OnChanged);
            this.Renamed += new RenamedEventHandler(OnRenamed);

            this.EnableRaisingEvents = true;
        }

	private static void OnCreated(object source, FileSystemEventArgs e)
        {
            Watcher watcher = (Watcher)source;
            watcher.LogWriter.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
	    watcher.Entity.Process(e.FullPath);
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            Watcher watcher = (Watcher)source;
            watcher.LogWriter.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
	    watcher.Entity.Process(e.FullPath);
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            Watcher watcher = (Watcher)source;
            watcher.LogWriter.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }
    }
}
