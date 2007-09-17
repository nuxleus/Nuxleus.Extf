﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Permissions;
using Nuxleus.FileSystem;

namespace Nuxleus
{
    public class FileSystemWatcher
    {

        public static void Main()
        {
            Run();
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void Run()
        {
            string[] args = System.Environment.GetCommandLineArgs();

            if (args.Length != 2)
            {
                Console.WriteLine("Usage: Watcher.exe (directory)");
                return;
            }
            Console.WriteLine(args[1]);
            Watcher fileSystemWatcher = new Watcher(args[1], "", Console.Out);
            fileSystemWatcher.Watch(true);

            Console.WriteLine("Press \'q\' to quit the sample.");
            while (Console.Read() != 'q') ;
        }

    }
}
