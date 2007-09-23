﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Permissions;

namespace Nuxleus.FileSystem
{
    public class Watcher : FileSystemWatcher
    {
        string _path;
        TextWriter _logWriter;
        NotifyFilters _notifyFilters;
        string _filter;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public Watcher(string path, string filter, TextWriter logWriter)
        {
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

        public void Watch(bool watchSubDirectories)
        {
            base.Path = _path;
            base.NotifyFilter = _notifyFilters;

            base.IncludeSubdirectories = watchSubDirectories;
            base.Filter = _filter;

            this.Changed += new FileSystemEventHandler(OnChanged);
            this.Created += new FileSystemEventHandler(OnChanged);
            this.Deleted += new FileSystemEventHandler(OnChanged);
            this.Renamed += new RenamedEventHandler(OnRenamed);

            this.EnableRaisingEvents = true;
        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
	 
            Watcher watcher = (Watcher)source;
            watcher.LogWriter.WriteLine("File: " + e.FullPath + " " + e.ChangeType);

	    if(e.ChangeType == WatcherChangeTypes.Created) {
	       HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://localhost:8080/service/atom/build-atom-entry/");
	       req.Headers.Add("Slug", e.Name);
	       
	       HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
	       resp.Close();
	    }
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            Watcher watcher = (Watcher)source;
            watcher.LogWriter.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }
    }
}
