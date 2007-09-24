using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Permissions;

namespace Nuxleus.Process
{
    public class DarcsProcess : System.Diagnostics.Process
    {
        string _path;
        TextWriter _logWriter;
        DarcsProcess _proc;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public DarcsProcess()
        {
            base.EnableRaisingEvents = false;
            base.StartInfo.FileName = "darcs";
            
        }
        
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public DarcsProcess(string path, TextWriter logWriter)
        {
            _path = path;
            _logWriter = logWriter;
            _proc = new DarcsProcess();
            _proc.EnableRaisingEvents = false;
            _proc.StartInfo.FileName = "darcs";
            
        }

        public TextWriter LogWriter { get { return _logWriter; } set { _logWriter = value; } }
        public string Folder { get { return _path; } set { _path = value; } }

        public void AddFileToDarcs(string fullPath)
        {
            this.StartInfo.Arguments="add " + fullPath;
            this.Start();
            this.WaitForExit();
            CommitFileToDarcs(fullPath);
        }
        
        public void CommitFileToDarcs(string fullPath)
        {
            this.StartInfo.Arguments="ci " + fullPath + " -m 'addition of '" + fullPath;
            this.Start();
            this.WaitForExit();
        }
        
        public void MoveFileInDarcs(string oldPath, string newPath)
        {
            AddFileToDarcs(newPath);
            RemoveFileFromDarcs(oldPath);
        }
        
        public void RemoveFileFromDarcs(string fullPath)
        {
            this.StartInfo.Arguments="rm " + fullPath;
            this.Start();
            this.WaitForExit();
            CommitFileToDarcs(fullPath);
        }
    }
}