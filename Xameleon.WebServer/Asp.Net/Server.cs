// Copyright (c) 2006 by M. David Peterson
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.

using System;
using System.Net;
using System.Web;
using Mono.WebServer;
using System.IO;

namespace Xameleon {

  public class WebServer {

    public WebServer () { }

    private string App = "/:.";
    private int Port = 9999;
    private string Path = "/";
    private string RootDirectory = ".\\public_web";
    private bool Verbose = true;
    private XSPWebSource websource;
    private ApplicationServer WebAppServer;

    public void Start () {
      Environment.CurrentDirectory = RootDirectory;
      this.websource = new XSPWebSource(IPAddress.Any, this.Port);
      this.WebAppServer = new ApplicationServer(websource);
      this.WebAppServer.AddApplication(this.App, this.Port, this.Path, Directory.GetCurrentDirectory());
      this.WebAppServer.Verbose = this.Verbose;
      this.WebAppServer.Start(false);
    }

    public void Stop () {
      this.WebAppServer.UnloadAll();
      this.WebAppServer.Stop();
    }

    public void SetApp (string app) {
      this.App = app;
    }

    public void SetVerbose (bool verbose) {
      this.Verbose = verbose;
    }

    public void SetPort (int port) {
      this.Port = port;
    }

    public void SetPath (string path) {
      this.Path = path;
    }

    public void SetRoot (string root) {
      this.RootDirectory = root;
    }

    public void SetSystemRoot (string sys) {
      Environment.CurrentDirectory = sys;
    }

    public void GetCurDir () {
      System.Console.WriteLine(Directory.GetCurrentDirectory());
    }

    public void AddApp (String app, String path) {
      this.WebAppServer.AddApplication(app, this.Port, path, Directory.GetCurrentDirectory());
    }
  }
}
