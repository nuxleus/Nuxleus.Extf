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
    MemcachedClient _memcachedClient = null;
    bool _useMemCached = false;
    SockIOPool _pool = null;
    AppSettings _appSettings = new AppSettings();
    AspNetXameleonConfiguration _xameleonConfiguration = AspNetXameleonConfiguration.GetConfig();
    AspNetAwsConfiguration _awsConfiguration = AspNetAwsConfiguration.GetConfig();
    AspNetBungeeAppConfiguration _bungeeAppConfguration = AspNetBungeeAppConfiguration.GetConfig();
    AspNetMemcachedConfiguration _memcachedConfiguration = AspNetMemcachedConfiguration.GetConfig();
    XslTransformationManager _xslTransformationManager;
    Transform _transform = new Transform();
    Processor _processor = new Processor();
    Serializer _serializer = new Serializer();
    XmlUrlResolver _resolver = new XmlUrlResolver();
    Hashtable _globalXsltParams = new Hashtable();
    Hashtable _transformContextHashtable = new Hashtable();
    Hashtable _requestXsltParams = null;
    BaseXsltContext _baseXsltContext;
    String _baseUri;
    bool _DEBUG = true;

    protected void Application_Start(object sender, EventArgs e) {
        
        if (_xameleonConfiguration.UseMemcached == "yes") {
            _useMemCached = true;
            _memcachedClient = new MemcachedClient();
            _pool = SockIOPool.GetInstance();
            List<string> serverList = new List<string>();
            foreach (MemcachedServer server in _memcachedConfiguration.MemcachedServerCollection) {
                serverList.Add(server.IP + ":" + server.Port);
            }
            _pool.SetServers(serverList.ToArray());

            if (_memcachedConfiguration.UseCompression != null && _memcachedConfiguration.UseCompression == "yes")
                _memcachedClient.EnableCompression = true;
            else
                _memcachedClient.EnableCompression = false;

            MemcachedPoolConfig poolConfig = (MemcachedPoolConfig)_memcachedConfiguration.PoolConfig;
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

        string baseUri = (string)_xameleonConfiguration.PreCompiledXslt.BaseUri;
        if (baseUri != String.Empty)
            baseUri = (string)_xameleonConfiguration.PreCompiledXslt.BaseUri;
        else
            baseUri = "~";

        _xslTransformationManager = new XslTransformationManager(_processor, _transform, _resolver, _serializer);
        _resolver.Credentials = CredentialCache.DefaultCredentials;

        foreach (PreCompiledXslt xslt in _xameleonConfiguration.PreCompiledXslt) {
            string localBaseUri = (string)_xameleonConfiguration.PreCompiledXslt.BaseUri;
            if (localBaseUri == String.Empty)
                localBaseUri = baseUri;
            Uri xsltUri = new Uri(HttpContext.Current.Server.MapPath(localBaseUri + xslt.Uri));
            _xslTransformationManager.AddTransformer(xslt.Name, xsltUri, _resolver);
            if (xslt.UseAsBaseXslt == "yes") {
                _baseXsltContext = new BaseXsltContext(xsltUri, xslt.Name + ":" + xsltUri.GetHashCode().ToString(), xslt.Name);
            }
        }

        _xslTransformationManager.SetBaseXsltContext(_baseXsltContext);

        if (_useMemCached && _memcachedClient != null)
            Application["appStart_memcached"] = (MemcachedClient)_memcachedClient;

        foreach (XsltParam xsltParam in _xameleonConfiguration.GlobalXsltParam) {
            _globalXsltParams[xsltParam.Name] = (string)xsltParam.Select;
        }
        Application["appStart_usememcached"] = _useMemCached;
        Application["appStart_xslTransformationManager"] = _xslTransformationManager;
        Application["appStart_baseXsltContext"] = _baseXsltContext;
        Application["appStart_globalXsltParams"] = _globalXsltParams; 
    }

    protected void Session_Start(object sender, EventArgs e) {

    }

    protected void Application_BeginRequest(object sender, EventArgs e) {
        
        Hashtable xsltParams = (Hashtable)Application["appStart_globalXsltParams"];
        Context context = new Context(HttpContext.Current, (Hashtable)xsltParams.Clone());

        _useMemCached = (bool)Application["appStart_usememcached"];
        Application["debug"] = _DEBUG;
        Application["xslTransformationManager"] = (XslTransformationManager)Application["appStart_xslTransformationManager"];
        Application["transformContext"] = context;
        Application["baseXsltContext"] = (BaseXsltContext)Application["appStart_baseXsltContext"];
        Application["transformContextHashtable"] = _transformContextHashtable;
        Application["usememcached"] = _useMemCached;
        if (_useMemCached)
            Application["memcached"] = (MemcachedClient)Application["appStart_memcached"];
        if (_DEBUG) {
            WriteDebugOutput(context, (XslTransformationManager)Application["xslTransformationManager"]);
        }
    }

    protected void Application_AuthenticateRequest(object sender, EventArgs e) {

    }

    protected void Application_Error(object sender, EventArgs e) {

    }
    protected void Application_EndRequest(object sender, EventArgs e) {
        Context context = (Context)Application["transformContext"];
        context.Clear();
    }
    protected void Session_End(object sender, EventArgs e) {

    }

    protected void Application_End(object sender, EventArgs e) {
        SockIOPool.GetInstance().Shutdown();
    }

    protected void WriteDebugOutput(Context context, XslTransformationManager xslTransformationManager) {
        HttpContext.Current.Response.Output.WriteLine("CompilerBaseUri: " + xslTransformationManager.Compiler.BaseUri.ToString() + "<br/>");
        HttpContext.Current.Response.Output.WriteLine("Compiler: " + xslTransformationManager.Compiler.ToString() + "<br/>");
        HttpContext.Current.Response.Output.WriteLine("Serializer: " + xslTransformationManager.Serializer.ToString() + "<br/>");
        HttpContext.Current.Response.Output.WriteLine("BaseTemplate: " + _appSettings.GetSetting("baseTemplate") + "<br/>");
        HttpContext.Current.Response.Output.WriteLine("UseMemcached?: " + _useMemCached.ToString() + "<br/>");
        HttpContext.Current.Response.Output.WriteLine("Transform: " + xslTransformationManager.Transform.ToString() + "<br/>");
        HttpContext.Current.Response.Output.WriteLine("Resolver: " + xslTransformationManager.Resolver.ToString() + "<br/>");
        HttpContext.Current.Response.Output.WriteLine("XslTransformationManager: " + xslTransformationManager.ToString() + "<br/>");
        HttpContext.Current.Response.Output.WriteLine("GlobalXsltParms: " + _globalXsltParams.ToString() + "<br/>");
        HttpContext.Current.Response.Output.WriteLine("Processor: " + xslTransformationManager.Processor.ToString() + "<br/>");
        HttpContext.Current.Response.Write("Request Url: " + context.RequestUri.ToString() + "<br/>");
        HttpContext.Current.Response.Write("Request WeakHashcode: " + context.GetWeakHashcode(true) + "<br/>");
        HttpContext.Current.Response.Write("Request StrongHashcode: " + context.GetStrongHashcode(true, false) + "<br/>");
        HttpContext.Current.Response.Write("Request ReallyStronghashcode: " + context.GetStrongHashcode(false, true) + "<br/>");
        HttpContext.Current.Response.Write("Context Hashcode: " + context.GetHashCode().ToString() + "<br/>");
        HttpContext.Current.Response.Write("Context Uri: " + context.RequestUri.ToString() + "<br/>");
        HttpContext.Current.Response.Write("Context UriHashCode: " + context.RequestUriHash + "<br/>");
        HttpContext.Current.Response.Write("Context HttpParams Count: " + context.HttpParams.Count.ToString() + "<br/>");
        IEnumerator httpParamsEnum = context.HttpParams.GetEnumerator();
        int i = 0;
        while (httpParamsEnum.MoveNext()) {
            string key = context.HttpParams.AllKeys[i].ToString();
            HttpContext.Current.Response.Write("ParamName: " + key + "<br/>");
            HttpContext.Current.Response.Write("ParamValue: " + context.HttpParams[key] + "<br/>");
            i += 1;
        }
        HttpContext.Current.Response.Write("Context XsltParams Count:" + context.XsltParams.Count.ToString() + "<br/>");
        foreach (DictionaryEntry entry in context.XsltParams) {
            HttpContext.Current.Response.Write("XsltParam Name:" + (string)entry.Key + "<br/>");
            HttpContext.Current.Response.Write("XsltParam Value:" + (string)entry.Value + "<br/>");
        }
    }
</script>

