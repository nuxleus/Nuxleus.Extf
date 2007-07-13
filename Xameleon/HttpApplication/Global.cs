using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Xameleon;
using Xameleon.Transform;
using Xameleon.Configuration;
using Memcached.ClientLibrary;
using System.Collections.Generic;
using Saxon.Api;
using IronPython.Hosting;
using System.IO;
using System.Xml;
using System.Net;

namespace Xameleon.HttpApplication {

  public class Global : System.Web.HttpApplication {

    MemcachedClient _MemcachedClient = new MemcachedClient();
    bool _UseMemCached = false;
    SockIOPool _pool = null;
    AppSettings _AppSettings = new AppSettings();
    AspNetXameleonConfiguration _XameleonConfiguration = AspNetXameleonConfiguration.GetConfig();
    AspNetAwsConfiguration _AwsConfiguration = AspNetAwsConfiguration.GetConfig();
    AspNetBungeeAppConfiguration _BungeeAppConfguration = AspNetBungeeAppConfiguration.GetConfig();
    AspNetMemcachedConfiguration _MemcachedConfiguration = AspNetMemcachedConfiguration.GetConfig();
    XsltCompiledHashtable _XsltCompiledHashtable = new XsltCompiledHashtable();
    Transform.Transform _Transform = new Transform.Transform();
    Processor _Processor = new Processor();
    XsltCompiler _Compiler;
    Serializer _Serializer = new Serializer();
    XmlUrlResolver _Resolver = new XmlUrlResolver();
    Hashtable _GlobalXsltParams = new Hashtable();
    Hashtable _SessionXsltParams = null;
    Hashtable _RequestXsltParams = null;
    String _BaseUri = null;
    Uri _BaseTemplateUri = null;

    protected void Application_Start(object sender, EventArgs e) {
      string useMemcached = _XameleonConfiguration.UseMemcached;

      if (useMemcached == "yes") {
        _UseMemCached = true;
        _MemcachedClient = new MemcachedClient();
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

      }

      string baseUri = (string)_XameleonConfiguration.PreCompiledXslt.BaseUri;
      HttpContext.Current.Response.Output.WriteLine("baseUri: " + baseUri);

      foreach (PreCompiledXslt xslt in _XameleonConfiguration.PreCompiledXslt) {
        Uri xsltUri = new Uri(HttpContext.Current.Server.MapPath("~" + xslt.Uri));
        _XsltCompiledHashtable.AddTransformer(xslt.Name, xsltUri, _Processor);
      }

      foreach (XsltParam xsltParam in _XameleonConfiguration.GlobalXsltParam) {
        _GlobalXsltParams[xsltParam.Name] = (string)xsltParam.Select;
      }

      string baseTemplate = _AppSettings.GetSetting("baseTemplate");

      if (baseTemplate != null) {
        _BaseUri = baseTemplate;
      } else {
        _BaseUri = "http://localhost/";
      }

      _Compiler = _Processor.NewXsltCompiler();
      _Compiler.BaseUri = new Uri(HttpContext.Current.Server.MapPath("~" + baseTemplate));
      _Resolver.Credentials = CredentialCache.DefaultCredentials;
    }

    protected void Session_Start(object sender, EventArgs e) {
      _SessionXsltParams = new Hashtable();
      foreach (XsltParam xsltParam in _XameleonConfiguration.SessionXsltParam) {
        _SessionXsltParams[xsltParam.Name] = (string)xsltParam.Select;
      }
      Application["sessionid"] = HttpContext.Current.Session.SessionID;
      Application["sessionXsltParams"] = _SessionXsltParams;
    }

    protected void Application_BeginRequest(object sender, EventArgs e) {

      HttpContext.Current.Response.Output.WriteLine(_XameleonConfiguration.UseMemcached);
      Application["processor"] = _Processor;
      HttpContext.Current.Response.Output.WriteLine("Processor: " + _Processor.ToString());
      _Compiler = _Processor.NewXsltCompiler();
      _Compiler.BaseUri = new Uri(HttpContext.Current.Server.MapPath("~" + _AppSettings.GetSetting("baseTemplate")));
      HttpContext.Current.Response.Output.WriteLine("CompilerBaseUri: " + _Compiler.BaseUri.ToString());
      Application["compiler"] = _Compiler;
      HttpContext.Current.Response.Output.WriteLine("Compiler: " + _Compiler.ToString());
      Application["serializer"] = _Serializer;
      HttpContext.Current.Response.Output.WriteLine("Serializer: " + _Serializer.ToString());
      Application["resolver"] = _Resolver;
      HttpContext.Current.Response.Output.WriteLine("Resolver: " + _Resolver.ToString());
      Application["transform"] = _Transform;
      HttpContext.Current.Response.Output.WriteLine("Transform: " + _Transform.ToString());
      Application["xsltCompiledHashtable"] = _XsltCompiledHashtable;
      HttpContext.Current.Response.Output.WriteLine("XsltCompiledHashtable: " + _XsltCompiledHashtable.ToString());
      Application["globalXsltParams"] = _GlobalXsltParams;
      HttpContext.Current.Response.Output.WriteLine("GlobalXsltParms: " + _GlobalXsltParams.ToString());
      //Application["sessionXsltParams"] = _GlobalXsltParams;
      //Application["requestXsltParams"] = _GlobalXsltParams;
      Application["appSettings"] = _AppSettings;
      HttpContext.Current.Response.Output.WriteLine("AppSettings: " + _AppSettings.ToString());
      Application["usememcached"] = _UseMemCached;
      HttpContext.Current.Response.Output.WriteLine("UseMemcached?: " + _UseMemCached.ToString());
      Application["memcached"] = _MemcachedClient;
      HttpContext.Current.Response.Output.WriteLine("MemCachedClient: " + _MemcachedClient.ToString());
      HttpContext.Current.Response.Output.WriteLine("BaseTemplate: " + _AppSettings.GetSetting("baseTemplate"));
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
