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

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public DarcsProcess()
        {
            base.StartInfo.FileName = "darcs";
            base.EnableRaisingEvents = false;
        }
        
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public DarcsProcess(string path, TextWriter logWriter)
        {
            _path = path;
            _logWriter = logWriter;
            this.StartInfo.FileName = "darcs";
            this.EnableRaisingEvents = false;
            
        }

        public TextWriter LogWriter { get { return _logWriter; } set { _logWriter = value; } }
        public string Folder { get { return _path; } set { _path = value; } }

        public void AddFileToDarcs(string fullPath)
        {
            this.StartInfo.Arguments="add --case-ok " + fullPath;
            this.Start();
            //this.WaitForExit();
            CommitFileToDarcs(fullPath);
        }
        
        public void CommitFileToDarcs(string fullPath)
        {
            this.StartInfo.Arguments = "record -a --skip-long-comment --patch-name=" + fullPath + ":" + Guid.NewGuid().ToString() + " " + fullPath;
            this.Start();
            //this.WaitForExit();
        }
        
        public void MoveFileInDarcs(string oldPath, string newPath)
        {
            AddFileToDarcs(newPath);
            RemoveFileFromDarcs(oldPath);
        }
        
        public void RemoveFileFromDarcs(string fullPath)
        {
            this.StartInfo.Arguments = "remove " + fullPath;
            this.Start();
            //this.WaitForExit();
            CommitFileToDarcs(fullPath);
        }
        
        public void KillProcess()
        { 
            this.Kill(); 
        }
    }
}