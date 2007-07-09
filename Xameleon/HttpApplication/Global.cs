using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Xameleon.Transform;
using Xameleon.Configuration;
using Memcached.ClientLibrary;
using System.Collections.Generic;
using Saxon.Api;
using IronPython.Hosting;
using IronPython.Runtime;
using System.IO;

namespace Xameleon.HttpApplication {

  public class Global : System.Web.HttpApplication {

    MemcachedClient _MemcachedClient = null;
    SockIOPool _pool = null;
    AppSettings _AppSettings = new AppSettings();
    AspNetXameleonConfiguration _XameleonConfiguration = AspNetXameleonConfiguration.GetConfig();
    AspNetAwsConfiguration _AwsConfiguration = AspNetAwsConfiguration.GetConfig();
    AspNetBungeeAppConfiguration _BungeeAppConfguration = AspNetBungeeAppConfiguration.GetConfig();
    AspNetMemcachedConfiguration _MemcachedConfiguration = AspNetMemcachedConfiguration.GetConfig();
    XsltCompiledHashtable _XsltCompiledHashtable = new XsltCompiledHashtable();
    Processor _Processor = new Processor();
    XsltCompiler _Compiler = null;
    String _BaseUri = null;
    Uri _BaseTemplateUri = null;
    static PythonEngine _PythonEngine = PythonEngine.CurrentEngine;

    protected void Application_Start(object sender, EventArgs e) {
      string useMemcached = _XameleonConfiguration.UseMemcached;
      if (useMemcached != null && useMemcached == "yes") {
        _MemcachedClient = new MemcachedClient();
        Application["memcached"] = _MemcachedClient;
        Application["usememcached"] = true;

        if (_MemcachedConfiguration.UseCompression != null && _MemcachedConfiguration.UseCompression == "yes")
          _MemcachedClient.EnableCompression = true;
        else
          _MemcachedClient.EnableCompression = false;

        List<string> serverList = new List<string>();

        foreach (MemcachedServer server in _MemcachedConfiguration.MemcachedServerCollection) {
          serverList.Add(server.IP + ":" + server.Port);
        }

        _pool = SockIOPool.GetInstance();

        _pool.SetServers(serverList.ToArray());

        MemcachedPoolConfig poolConfig = (MemcachedPoolConfig)_MemcachedConfiguration.PoolConfig;

        _pool.InitConnections = (int)poolConfig.InitConnections;
        _pool.MinConnections = (int)poolConfig.MinConnections;
        _pool.MaxConnections = (int)poolConfig.MaxConnections;
        _pool.SocketConnectTimeout = (int)poolConfig.SocketConnectTimeout;
        _pool.SocketTimeout = (int)poolConfig.SocketConnect;
        _pool.MaintenanceSleep = (int)poolConfig.MaintenanceSleep;
        _pool.Failover = (bool)poolConfig.Failover;
        _pool.Nagle = (bool)poolConfig.Nagle;
        _pool.Initialize();

      } else
        Application["usememcached"] = false;

      string baseUri = (string)_XameleonConfiguration.PreCompiledXslt.BaseUri;

      foreach (PreCompiledXslt xslt in _XameleonConfiguration.PreCompiledXslt) {
        if ((string)xslt.BaseUri != String.Empty) {
          //HttpContext.Current.Response.Output.WriteLine("uri: " + (string)xslt.BaseUri);
          baseUri = (string)xslt.BaseUri;
        }
        Uri uri = new Uri(baseUri, UriKind.Absolute);
        _XsltCompiledHashtable.GetTransformer(xslt.Name, (string)xslt.Uri, uri, _Processor);
      }
      _PythonEngine.AddToPath(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
      Application["pythonEngine"] = _PythonEngine;
      string baseTemplate = _AppSettings.GetSetting("baseTemplate");

      if (baseTemplate != null) {
        _BaseUri = baseTemplate;
      } else {
        _BaseUri = "http://localhost/";
      }

      _Compiler = _Processor.NewXsltCompiler();
      _Compiler.BaseUri = new Uri(HttpContext.Current.Server.MapPath(_BaseUri));

      Application["processor"] = _Processor;
      Application["compiler"] = _Compiler;
      Application["xsltCompiledHashtable"] = _XsltCompiledHashtable;
      Application["appSettings"] = _AppSettings;
    }

    protected void Session_Start(object sender, EventArgs e) {
      Application["sessionid"] = HttpContext.Current.Session.SessionID;
    }

    protected void Application_BeginRequest(object sender, EventArgs e) {

    }

    protected void Application_AuthenticateRequest(object sender, EventArgs e) {

    }

    protected void Application_Error(object sender, EventArgs e) {

    }

    protected void Session_End(object sender, EventArgs e) {

    }

    protected void Application_End(object sender, EventArgs e) {
      SockIOPool.GetInstance().Shutdown();
    }
  }
}
