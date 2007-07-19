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
using System.IO;
using System.Xml;
using System.Net;

namespace Xameleon.HttpApplication {

  public class Global : System.Web.HttpApplication {

    MemcachedClient _memcachedClient = null;
    bool _useMemCached = false;
    SockIOPool _pool = null;
    AppSettings _AppSettings = new AppSettings();
    AspNetXameleonConfiguration _XameleonConfiguration = AspNetXameleonConfiguration.GetConfig();
    AspNetAwsConfiguration _AwsConfiguration = AspNetAwsConfiguration.GetConfig();
    AspNetBungeeAppConfiguration _BungeeAppConfguration = AspNetBungeeAppConfiguration.GetConfig();
    AspNetMemcachedConfiguration _MemcachedConfiguration = AspNetMemcachedConfiguration.GetConfig();
    XsltTransformationManager _XslTransformationManager;
    Transform.Transform _Transform = new Transform.Transform();
    Processor _Processor = new Processor();
    Serializer _Serializer = new Serializer();
    XmlUrlResolver _Resolver = new XmlUrlResolver();
    Hashtable _GlobalXsltParams = new Hashtable();
    Hashtable _TransformContextHashtable = new Hashtable();
    Hashtable _RequestXsltParams = null;
    BaseXsltContext _BaseXsltContext;
    String _BaseUri;
    bool _DEBUG = false;

    protected void Application_Start(object sender, EventArgs e) {

      if (_XameleonConfiguration.UseMemcached == "yes") {
        _useMemCached = true;
        _memcachedClient = new MemcachedClient();
        _pool = SockIOPool.GetInstance();
        List<string> serverList = new List<string>();
        foreach (MemcachedServer server in _MemcachedConfiguration.MemcachedServerCollection) {
          serverList.Add(server.IP + ":" + server.Port);
        }
        _pool.SetServers(serverList.ToArray());

        if (_MemcachedConfiguration.UseCompression != null && _MemcachedConfiguration.UseCompression == "yes")
          _memcachedClient.EnableCompression = true;
        else
          _memcachedClient.EnableCompression = false;

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
      if (baseUri != String.Empty)
        baseUri = (string)_XameleonConfiguration.PreCompiledXslt.BaseUri;
      else
        baseUri = "~";

      _XslTransformationManager = new XsltTransformationManager(_Processor, _Transform, _Resolver, _Serializer);
      _Resolver.Credentials = CredentialCache.DefaultCredentials;

      foreach (PreCompiledXslt xslt in _XameleonConfiguration.PreCompiledXslt) {
        string localBaseUri = (string)_XameleonConfiguration.PreCompiledXslt.BaseUri;
        if (localBaseUri == String.Empty)
          localBaseUri = baseUri;
        Uri xsltUri = new Uri(HttpContext.Current.Server.MapPath(localBaseUri + xslt.Uri));
        _XslTransformationManager.AddTransformer(xslt.Name, xsltUri, _Resolver);
        if (xslt.UseAsBaseXslt == "yes") {
          _BaseXsltContext = new BaseXsltContext(xsltUri, xslt.Name + ":" + xsltUri.GetHashCode().ToString(), xslt.Name);
        }
      }

      _XslTransformationManager.SetBaseXsltContext(_BaseXsltContext);

      if (_useMemCached && _memcachedClient != null)
        Application["appStart_memcached"] = _memcachedClient;

      Application["appStart_usememcached"] = _useMemCached;
      Application["appStart_xslTransformationManager"] = _XslTransformationManager;
      Application["appStart_baseXsltContext"] = _BaseXsltContext;
      Application["foobar"] = "foobar";

      foreach (XsltParam xsltParam in _XameleonConfiguration.GlobalXsltParam) {
        _GlobalXsltParams[xsltParam.Name] = (string)xsltParam.Select;
      }
    }

    protected void Session_Start(object sender, EventArgs e) {

    }

    protected void Application_BeginRequest(object sender, EventArgs e) {

      _Processor = _XslTransformationManager.Processor;

      Hashtable xsltParams = (Hashtable)_GlobalXsltParams.Clone();

      _useMemCached = (bool)Application["appStart_usememcached"];
      Application["debug"] = _DEBUG;
      Application["xslTransformationManager"] = (XsltTransformationManager)Application["appStart_xslTransformationManager"];
      Application["baseXsltContext"] = (BaseXsltContext)Application["appStart_baseXsltContext"];
      Application["transformContextHashtable"] = _TransformContextHashtable;
      Application["xsltParams"] = xsltParams;
      Application["appSettings"] = _AppSettings;
      Application["usememcached"] = _useMemCached;
      if (_useMemCached)
        Application["memcached"] = (MemcachedClient)Application["appStart_memcached"];

      if (_DEBUG) {
        WriteDebugOutput();
      }
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

    protected void WriteDebugOutput() {
      HttpContext.Current.Response.Output.WriteLine("CompilerBaseUri: " + _XslTransformationManager.Compiler.BaseUri.ToString());
      HttpContext.Current.Response.Output.WriteLine("Compiler: " + _XslTransformationManager.Compiler.ToString());
      HttpContext.Current.Response.Output.WriteLine("Serializer: " + _XslTransformationManager.Serializer.ToString());
      HttpContext.Current.Response.Output.WriteLine("MemCachedClient: " + _memcachedClient.ToString());
      HttpContext.Current.Response.Output.WriteLine("BaseTemplate: " + _AppSettings.GetSetting("baseTemplate"));
      HttpContext.Current.Response.Output.WriteLine("UseMemcached?: " + _useMemCached.ToString());
      HttpContext.Current.Response.Output.WriteLine("Transform: " + _XslTransformationManager.Transform.ToString());
      HttpContext.Current.Response.Output.WriteLine("Resolver: " + _XslTransformationManager.Resolver.ToString());
      HttpContext.Current.Response.Output.WriteLine("XslTransformationManager: " + _XslTransformationManager.ToString());
      HttpContext.Current.Response.Output.WriteLine("GlobalXsltParms: " + _GlobalXsltParams.ToString());
      HttpContext.Current.Response.Output.WriteLine("AppSettings: " + _AppSettings.ToString());
      HttpContext.Current.Response.Output.WriteLine("Processor: " + _XslTransformationManager.Processor.ToString());
    }
  }
}
