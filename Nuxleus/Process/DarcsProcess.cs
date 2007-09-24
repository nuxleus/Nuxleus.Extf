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
        DateTime _lastTransaction;
        static long _tickMultiplier = 10;
        long _tickBuffer = 100 * 100 * 100 *  _tickMultiplier;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public DarcsProcess()
        {
            base.StartInfo.FileName = "darcs";
            base.EnableRaisingEvents = false;
            _lastTransaction = DateTime.Now;
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
            DateTime now = DateTime.Now;
            long diff = now.Subtract(_lastTransaction).Ticks;
            _logWriter.WriteLine("Ticks since last transactions: {0}", diff);
            _lastTransaction = now;
            
            if(diff > _tickBuffer)
            {
                this.StartInfo.Arguments="add --case-ok " + fullPath;
                this.Start();
                now = DateTime.Now;
                diff = now.Subtract(_lastTransaction).Ticks;
                _logWriter.WriteLine("Ticks since last transactions: {0}", diff);
                if(diff > _tickBuffer)
                    CommitFileToDarcs(fullPath);
                else
                    _logWriter.WriteLine("Too many transactions...");
            }
            
            
        }
        
        public void CommitFileToDarcs(string fullPath)
        {
            this.StartInfo.Arguments = "record -a --skip-long-comment --patch-name=" + fullPath + ":" + Guid.NewGuid().ToString();
            this.Start();
        }

        public void MoveFileInDarcs(string oldPath, string newPath)
        {
            AddFileToDarcs(newPath);
            RemoveFileFromDarcs(oldPath);
        }
        
        public void RemoveFileFromDarcs(string fullPath)
        {
            //this.StartInfo.Arguments = "remove " + fullPath;
            //this.Start();
            CommitFileToDarcs(fullPath);
        }
        
        public void KillProcess()
        { 
            this.Kill(); 
        }
    }
}