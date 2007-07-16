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
    bool _useMemCached = false;
    MemcachedClient _memcachedClient = null;
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

        foreach (XsltParam xsltParam in _xameleonConfiguration.GlobalXsltParam) {
            _globalXsltParams[xsltParam.Name] = (string)xsltParam.Select;
        }

        if (_memcachedClient != null)
            Application["appStart_memcached"] = _memcachedClient;
        Application["appStart_usememcached"] = _useMemCached;
        Application["appStart_xslTransformationManager"] = _xslTransformationManager;
        Application["appStart_globalXsltParams"] = _globalXsltParams;
    }

    protected void Session_Start(object sender, EventArgs e) {

    }

    protected void Application_BeginRequest(object sender, EventArgs e) {

        Hashtable xsltParams = (Hashtable)Application["appStart_globalXsltParams"];
        Context context = new Context(HttpContext.Current, (Hashtable)xsltParams.Clone());
        StringBuilder builder = new StringBuilder();
        TextWriter writer = new StringWriter(builder);
        XslTransformationManager xslTransformationManager = (XslTransformationManager)Application["appStart_xslTransformationManager"];
        bool CONTENT_IS_MEMCACHED = false;
        bool useMemCached = (bool)Application["appStart_usememcached"];
        MemcachedClient memcachedClient = (MemcachedClient)Application["appStart_memcached"];
        Application["memcached"] = memcachedClient;

        if (useMemCached) {
            string obj = (string)memcachedClient.Get(context.RequestUriHash);
            if (obj != null) {
                builder.Append(obj);
                CONTENT_IS_MEMCACHED = true;
            } else {
                writer = new StringWriter(builder);
                CONTENT_IS_MEMCACHED = false;
            }
        } else {
            writer = new StringWriter(builder);
        }

        Application["debug"] = _DEBUG;
        Application["textWriter"] = writer;
        Application["stringBuilder"] = builder;
        Application["CONTENT_IS_MEMCACHED"] = CONTENT_IS_MEMCACHED;
        Application["xslTransformationManager"] = xslTransformationManager;
        Application["transformContext"] = context;
        //Application["transformContextHashtable"] = _transformContextHashtable;
        if (_DEBUG) {
            HttpContext.Current.Response.Write(WriteDebugOutput(context, xslTransformationManager, new StringBuilder(), CONTENT_IS_MEMCACHED).ToString());
        }

    }

    protected void Application_AuthenticateRequest(object sender, EventArgs e) {

    }

    protected void Application_Error(object sender, EventArgs e) {

    }
    protected void Application_EndRequest(object sender, EventArgs e) {
        //Context context = (Context)Application["transformContext"];
        //StringBuilder builder = (StringBuilder)Application["stringBuilder"];
        //using (TextWriter writer = HttpContext.Current.Response.Output) {
        //    string output = builder.ToString();
        //    writer.Write(output);
        //}
        //context.Clear();
        //builder = null;
    }
    protected void Session_End(object sender, EventArgs e) {

    }

    protected void Application_End(object sender, EventArgs e) {
        SockIOPool.GetInstance().Shutdown();
    }

    protected StringBuilder WriteDebugOutput(Context context, XslTransformationManager xslTransformationManager, StringBuilder builder, bool CONTENT_IS_MEMCACHED) {
        builder.Append("CompilerBaseUri: " + xslTransformationManager.Compiler.BaseUri + "<br/>");
        builder.Append("Compiler: " + xslTransformationManager.Compiler.GetHashCode() + "<br/>");
        builder.Append("Serializer: " + xslTransformationManager.Serializer.GetHashCode() + "<br/>");
        builder.Append("BaseXsltName: " + xslTransformationManager.BaseXsltName + "<br/>");
        builder.Append("BaseXsltUri: " + xslTransformationManager.BaseXsltUri + "<br/>");
        builder.Append("BaseXsltUriHash: " + xslTransformationManager.BaseXsltUriHash + "<br/>");
        builder.Append("UseMemcached?: " + (bool)Application["appStart_usememcached"] + "<br/>");
        builder.Append("Transform: " + xslTransformationManager.Transform.GetHashCode() + "<br/>");
        builder.Append("Resolver: " + xslTransformationManager.Resolver.GetHashCode() + "<br/>");
        builder.Append("XslTransformationManager: " + xslTransformationManager.GetHashCode() + "<br/>");
        builder.Append("GlobalXsltParms: " + _globalXsltParams.GetHashCode()+ "<br/>");
        builder.Append("Processor: " + xslTransformationManager.Processor.GetHashCode() + "<br/>");
        builder.Append("Request XmlSource Execution File Path: " + HttpContext.Current.Request.MapPath(HttpContext.Current.Request.CurrentExecutionFilePath) + "<br/>");
        builder.Append("Request Url: " + context.RequestUri + "<br/>");
        builder.Append("Request is Memcached? " + CONTENT_IS_MEMCACHED + "<br/>");
        builder.Append("Request WeakHashcode: " + context.GetWeakHashcode(true) + "<br/>");
        builder.Append("Request StrongHashcode: " + context.GetStrongHashcode(true, false) + "<br/>");
        builder.Append("Request ReallyStronghashcode: " + context.GetStrongHashcode(false, true) + "<br/>");
        builder.Append("Context Hashcode: " + context.GetHashCode() + "<br/>");
        builder.Append("Context Uri: " + context.RequestUri + "<br/>");
        builder.Append("Context UriHashCode: " + context.RequestUriHash + "<br/>");
        builder.Append("Context HttpParams Count: " + context.HttpParams.Count + "<br/>");
        IEnumerator httpParamsEnum = context.HttpParams.GetEnumerator();
        int i = 0;
        while (httpParamsEnum.MoveNext()) {
            string key = context.HttpParams.AllKeys[i].ToString();
            builder.Append("ParamName: " + key + "<br/>");
            builder.Append("ParamValue: " + context.HttpParams[key] + "<br/>");
            i += 1;
        }
        builder.Append("Context XsltParams Count:" + context.XsltParams.Count + "<br/>");
        foreach (DictionaryEntry entry in context.XsltParams) {
            builder.Append("XsltParam Name:" + (string)entry.Key + "<br/>");
            builder.Append("XsltParam Value:" + (string)entry.Value + "<br/>");
        }
        return builder;
    }
</script>

