<%--<%@ Application Inherits="Xameleon.HttpApplication.Global" Language="C#" %>--%>
<%@ Application Language="C#" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Configuration" %>
<%@ Import Namespace="System.Collections" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Web.Security" %>
<%@ Import Namespace="System.Web.SessionState" %>
<%@ Import Namespace="Xameleon.Transform" %>
<%@ Import Namespace="Xameleon.Configuration" %>
<%@ Import Namespace="Memcached.ClientLibrary" %>
<%@ Import Namespace="Saxon.Api" %>
<%@ Import Namespace="IronPython.Hosting" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Net" %>
<%@ Import Namespace="System.Xml" %>

<script RunAt="server">
    MemcachedClient _MemcachedClient = new MemcachedClient();
    bool _UseMemCached = false;
    SockIOPool _pool = null;
    AppSettings _AppSettings = new AppSettings();
    AspNetXameleonConfiguration _XameleonConfiguration = AspNetXameleonConfiguration.GetConfig();
    AspNetAwsConfiguration _AwsConfiguration = AspNetAwsConfiguration.GetConfig();
    AspNetBungeeAppConfiguration _BungeeAppConfguration = AspNetBungeeAppConfiguration.GetConfig();
    AspNetMemcachedConfiguration _MemcachedConfiguration = AspNetMemcachedConfiguration.GetConfig();
    XsltCompiledHashtable _XsltCompiledHashtable = new XsltCompiledHashtable();
    Transform _Transform = new Transform();
    Processor _Processor = new Processor();
    XsltCompiler _Compiler = null;
    Serializer _Serializer = new Serializer();
    XmlUrlResolver _Resolver = new XmlUrlResolver();
    Hashtable _GlobalXsltParams = new Hashtable();
    Hashtable _SessionXsltParams = null;
    Hashtable _RequestXsltParams = null;
    String _BaseUri = null;
    Uri _BaseTemplateUri = null;

    protected void Application_Start(object sender, EventArgs e) {

      if (_XameleonConfiguration.UseMemcached == "yes") {
        _MemcachedClient = new MemcachedClient();
        _pool = SockIOPool.GetInstance();
        List<string> serverList = new List<string>();
        foreach (MemcachedServer server in _MemcachedConfiguration.MemcachedServerCollection) {
          serverList.Add(server.IP + ":" + server.Port);
        }
        _pool.SetServers(serverList.ToArray());

        if (_MemcachedConfiguration.UseCompression != null && _MemcachedConfiguration.UseCompression == "yes")
          _MemcachedClient.EnableCompression = true;
        else
          _MemcachedClient.EnableCompression = false;

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

    }

    protected void Application_BeginRequest(object sender, EventArgs e) {

      HttpContext.Current.Response.Output.WriteLine(_XameleonConfiguration.UseMemcached);
      HttpContext.Current.Response.Output.WriteLine("CompilerBaseUri: " + _Compiler.BaseUri.ToString());
      HttpContext.Current.Response.Output.WriteLine("Compiler: " + _Compiler.ToString());
      HttpContext.Current.Response.Output.WriteLine("Serializer: " + _Serializer.ToString());
      HttpContext.Current.Response.Output.WriteLine("MemCachedClient: " + _MemcachedClient.ToString());
      HttpContext.Current.Response.Output.WriteLine("BaseTemplate: " + _AppSettings.GetSetting("baseTemplate"));
      HttpContext.Current.Response.Output.WriteLine("UseMemcached?: " + _UseMemCached.ToString());
      HttpContext.Current.Response.Output.WriteLine("Transform: " + _Transform.ToString());
      HttpContext.Current.Response.Output.WriteLine("Resolver: " + _Resolver.ToString());
      HttpContext.Current.Response.Output.WriteLine("XsltCompiledHashtable: " + _XsltCompiledHashtable.ToString());
      HttpContext.Current.Response.Output.WriteLine("GlobalXsltParms: " + _GlobalXsltParams.ToString());
      HttpContext.Current.Response.Output.WriteLine("AppSettings: " + _AppSettings.ToString());
      HttpContext.Current.Response.Output.WriteLine("Processor: " + _Processor.ToString());

      if (_XameleonConfiguration.UseMemcached == "yes")
        _UseMemCached = true;
      Application["processor"] = _Processor;
      Application["compiler"] = _Compiler;
      Application["serializer"] = _Serializer;
      Application["resolver"] = _Resolver;
      Application["transform"] = _Transform;
      Application["xsltCompiledHashtable"] = _XsltCompiledHashtable;
      Application["globalXsltParams"] = _GlobalXsltParams;
      //Application["requestXsltParams"] = _GlobalXsltParams;
      Application["appSettings"] = _AppSettings;
      Application["usememcached"] = _UseMemCached;
      Application["memcached"] = _MemcachedClient;
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
</script>
