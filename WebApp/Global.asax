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
    MemcachedClient _MemcachedClient = null;
    bool _UseMemCached = false;
    SockIOPool _Pool = null;
    AppSettings _AppSettings = new AppSettings();
    AspNetXameleonConfiguration _XameleonConfiguration = AspNetXameleonConfiguration.GetConfig();
    AspNetAwsConfiguration _AwsConfiguration = AspNetAwsConfiguration.GetConfig();
    AspNetBungeeAppConfiguration _BungeeAppConfguration = AspNetBungeeAppConfiguration.GetConfig();
    AspNetMemcachedConfiguration _MemcachedConfiguration = AspNetMemcachedConfiguration.GetConfig();
    XslTransformationManager _XslTransformationManager;
    Transform _Transform = new Transform();
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
            _UseMemCached = true;
            _MemcachedClient = new MemcachedClient();
            _Pool = SockIOPool.GetInstance();
            List<string> serverList = new List<string>();
            foreach (MemcachedServer server in _MemcachedConfiguration.MemcachedServerCollection) {
                serverList.Add(server.IP + ":" + server.Port);
            }
            _Pool.SetServers(serverList.ToArray());

            if (_MemcachedConfiguration.UseCompression != null && _MemcachedConfiguration.UseCompression == "yes")
                _MemcachedClient.EnableCompression = true;
            else
                _MemcachedClient.EnableCompression = false;

            MemcachedPoolConfig poolConfig = (MemcachedPoolConfig)_MemcachedConfiguration.PoolConfig;
            _Pool.InitConnections = (int)poolConfig.InitConnections;
            _Pool.MinConnections = (int)poolConfig.MinConnections;
            _Pool.MaxConnections = (int)poolConfig.MaxConnections;
            _Pool.SocketConnectTimeout = (int)poolConfig.SocketConnectTimeout;
            _Pool.SocketTimeout = (int)poolConfig.SocketConnect;
            _Pool.MaintenanceSleep = (int)poolConfig.MaintenanceSleep;
            _Pool.Failover = (bool)poolConfig.Failover;
            _Pool.Nagle = (bool)poolConfig.Nagle;
            _Pool.Initialize();
        }

        string baseUri = (string)_XameleonConfiguration.PreCompiledXslt.BaseUri;
        if (baseUri != String.Empty)
            baseUri = (string)_XameleonConfiguration.PreCompiledXslt.BaseUri;
        else
            baseUri = "~";

        _XslTransformationManager = new XslTransformationManager(_Processor, _Transform, _Resolver, _Serializer);
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

        if (_UseMemCached && _MemcachedClient != null)
            Application["appStart_memcached"] = _MemcachedClient;
        
        Application["appStart_usememcached"] = _UseMemCached;
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

        _UseMemCached = (bool)Application["appStart_usememcached"];
        Application["debug"] = _DEBUG;
        Application["xslTransformationManager"] = (XslTransformationManager)Application["appStart_xslTransformationManager"];
        Application["baseXsltContext"] = (BaseXsltContext)Application["appStart_baseXsltContext"];
        Application["transformContextHashtable"] = _TransformContextHashtable;
        Application["xsltParams"] = xsltParams;
        Application["appSettings"] = _AppSettings;
        Application["usememcached"] = _UseMemCached;
        if (_UseMemCached)
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
        HttpContext.Current.Response.Output.WriteLine("MemCachedClient: " + _MemcachedClient.ToString());
        HttpContext.Current.Response.Output.WriteLine("BaseTemplate: " + _AppSettings.GetSetting("baseTemplate"));
        HttpContext.Current.Response.Output.WriteLine("UseMemcached?: " + _UseMemCached.ToString());
        HttpContext.Current.Response.Output.WriteLine("Transform: " + _XslTransformationManager.Transform.ToString());
        HttpContext.Current.Response.Output.WriteLine("Resolver: " + _XslTransformationManager.Resolver.ToString());
        HttpContext.Current.Response.Output.WriteLine("XslTransformationManager: " + _XslTransformationManager.ToString());
        HttpContext.Current.Response.Output.WriteLine("GlobalXsltParms: " + _GlobalXsltParams.ToString());
        HttpContext.Current.Response.Output.WriteLine("AppSettings: " + _AppSettings.ToString());
        HttpContext.Current.Response.Output.WriteLine("Processor: " + _XslTransformationManager.Processor.ToString());
    }
</script>

