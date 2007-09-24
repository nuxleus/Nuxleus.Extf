using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Process _svnProc;

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
            _svnProc = new Process();
            _svnProc.EnableRaisingEvents = false;
            _svnProc.StartInfo.FileName = "svn";
            
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
            Process proc = watcher._svnProc;
            watcher.LogWriter.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            
            switch(e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://localhost:8080/service/atom/build-atom-entry/");
        	        req.Headers.Add("Slug", e.Name);
                     
        	        HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
        	        resp.Close();
                       
        	        AddFileToSVN(proc, e.FullPath);
        	        break;
                }
                case WatcherChangeTypes.Changed:
                {
                    CommitFileToSVN(proc, e.FullPath);
                    break;
                }
                case WatcherChangeTypes.Deleted:
                {
                    RemoveFileFromSVN(proc, e.FullPath);
                    break;
                }
                default:
                {
                    break;
                }

            }
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            Watcher watcher = (Watcher)source;
            watcher.LogWriter.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
            MoveFileInSVN(watcher._svnProc, e.OldFullPath, e.FullPath);
        }
        
        private static void AddFileToSVN(Process proc, string fullPath)
        {
            proc.StartInfo.Arguments="add " + fullPath;
            proc.Start();
            proc.WaitForExit();
            CommitFileToSVN(proc, fullPath);
        }
        
        private static void CommitFileToSVN(Process proc, string fullPath)
        {
            proc.StartInfo.Arguments="ci " + fullPath + " -m 'addition of '" + fullPath;
            proc.Start();
            proc.WaitForExit();
        }
        
        private static void MoveFileInSVN(Process proc, string oldPath, string newPath)
        {
            AddFileToSVN(proc, newPath);
            RemoveFileFromSVN(proc, oldPath);
        }
        
        private static void RemoveFileFromSVN(Process proc, string fullPath)
        {
            proc.StartInfo.Arguments="rm " + fullPath;
            proc.Start();
            proc.WaitForExit();
            CommitFileToSVN(proc, fullPath);
        }
    }
}
