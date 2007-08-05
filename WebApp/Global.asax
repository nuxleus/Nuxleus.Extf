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
<%@ Import Namespace="Xameleon.Cryptography" %>
<%@ Import Namespace="Xameleon.Configuration" %>
<%@ Import Namespace="Memcached.ClientLibrary" %>
<%@ Import Namespace="Saxon.Api" %>
<%@ Import Namespace="IronPython.Hosting" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Net" %>
<%@ Import Namespace="System.Xml" %>

<script RunAt="server">
    bool _useMemCached = false;
    bool _DEBUG = false;
    MemcachedClient _memcachedClient = null;
    SockIOPool _pool = null;
    AppSettings _appSettings = new AppSettings();
    AspNetXameleonConfiguration _xameleonConfiguration = AspNetXameleonConfiguration.GetConfig();
    AspNetAwsConfiguration _awsConfiguration = AspNetAwsConfiguration.GetConfig();
    AspNetBungeeAppConfiguration _bungeeAppConfguration = AspNetBungeeAppConfiguration.GetConfig();
    AspNetMemcachedConfiguration _memcachedConfiguration = AspNetMemcachedConfiguration.GetConfig();
    XsltTransformationManager _xsltTransformationManager;
    Transform _transform = new Transform();
    Processor _processor = new Processor();
    Serializer _serializer = new Serializer();
    XmlUrlResolver _resolver = new XmlUrlResolver();
    Hashtable _namedXsltHashtable = new Hashtable();
    Hashtable _globalXsltParams = new Hashtable();
    Hashtable _transformContextHashtable = new Hashtable();
    Hashtable _requestXsltParams = null;
    BaseXsltContext _baseXsltContext;
    String _baseUri;
    HashAlgorithm _hashAlgorithm = HashAlgorithm.SHA1;
    

    protected void Application_Start(object sender, EventArgs e)
    {
        if (_xameleonConfiguration.DebugMode == "yes") _DEBUG = true;

        if (_xameleonConfiguration.UseMemcached == "yes")
        {
            _useMemCached = true;
            _memcachedClient = new MemcachedClient();
            _pool = SockIOPool.GetInstance();
            List<string> serverList = new List<string>();
            foreach (MemcachedServer server in _memcachedConfiguration.MemcachedServerCollection)
            {
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

        _xsltTransformationManager = new XsltTransformationManager(_processor, _transform, _resolver, _serializer);
        _xsltTransformationManager.HashAlgorithm = _hashAlgorithm;
        _resolver.Credentials = CredentialCache.DefaultCredentials;
        _namedXsltHashtable = _xsltTransformationManager.NamedXsltHashtable;

        string hashkey = (string)_xameleonConfiguration.BaseSettings.ObjectHashKey;
        Application["hashkey"] = hashkey;

        foreach (PreCompiledXslt xslt in _xameleonConfiguration.PreCompiledXslt)
        {
            string localBaseUri = (string)_xameleonConfiguration.PreCompiledXslt.BaseUri;
            if (localBaseUri == String.Empty)
                localBaseUri = baseUri;
            Uri xsltUri = new Uri(HttpContext.Current.Server.MapPath(localBaseUri + xslt.Uri));
            _xsltTransformationManager.Compiler.BaseUri = xsltUri;
            _xsltTransformationManager.AddTransformer(xslt.Name, xsltUri, _resolver, xslt.InitialMode, xslt.InitialTemplate);
            _namedXsltHashtable.Add(xslt.Name, xsltUri);
            if (xslt.UseAsBaseXslt == "yes")
            {
                _baseXsltContext = new BaseXsltContext(xsltUri, XsltTransformationManager.GenerateNamedETagKey(xslt.Name, xsltUri), xslt.Name);
            }
        }

        _xsltTransformationManager.SetBaseXsltContext(_baseXsltContext);

        foreach (XsltParam xsltParam in _xameleonConfiguration.GlobalXsltParam)
        {
            _globalXsltParams[xsltParam.Name] = (string)xsltParam.Select;
        }

        if (_memcachedClient != null)
            Application["appStart_memcached"] = _memcachedClient;
        Application["appStart_usememcached"] = _useMemCached;
        Application["appStart_xslTransformationManager"] = _xsltTransformationManager;
        Application["appStart_namedXsltHashtable"] = _namedXsltHashtable;
        Application["appStart_globalXsltParams"] = _globalXsltParams;
        Application["debug"] = _DEBUG;

    }

    protected void Application_BeginRequest(object sender, EventArgs e)
    {

        Hashtable xsltParams = (Hashtable)Application["appStart_globalXsltParams"];
        FileInfo fileInfo = new FileInfo(HttpContext.Current.Request.MapPath(HttpContext.Current.Request.CurrentExecutionFilePath));
        Context context = new Context(HttpContext.Current, _hashAlgorithm, (string)Application["hashkey"], fileInfo, (Hashtable)xsltParams.Clone(), fileInfo.LastWriteTimeUtc, fileInfo.Length);
        StringBuilder builder = new StringBuilder();
        TextWriter writer = new StringWriter(builder);
        XsltTransformationManager xslTransformationManager = (XsltTransformationManager)Application["appStart_xslTransformationManager"];
        bool CONTENT_IS_MEMCACHED = false;
        bool useMemCached = (bool)Application["appStart_usememcached"];
        bool hasXmlSourceChanged = xslTransformationManager.HasXmlSourceChanged(context.RequestXmlETag);
        bool hasBaseXsltSourceChanged = xslTransformationManager.HasBaseXsltSourceChanged();

        MemcachedClient memcachedClient = (MemcachedClient)Application["appStart_memcached"];
        Application["memcached"] = memcachedClient;

        if (useMemCached)
        {
            string obj = (string)memcachedClient.Get(context.GetRequestHashcode(true));
            if (obj != null && !(hasXmlSourceChanged || hasBaseXsltSourceChanged))
            {
                builder.Append(obj);
                CONTENT_IS_MEMCACHED = true;
            }
            else
            {
                writer = new StringWriter(builder);
                CONTENT_IS_MEMCACHED = false;
            }
        }
        else
        {
            writer = new StringWriter(builder);
        }

        Application["textWriter"] = writer;
        Application["stringBuilder"] = builder;
        Application["CONTENT_IS_MEMCACHED"] = CONTENT_IS_MEMCACHED;
        Application["USE_MEMCACHED"] = useMemCached;
        Application["xsltTransformationManager"] = xslTransformationManager;
        Application["namedXsltHashtable"] = (Hashtable)Application["appStart_namedXsltHashtable"];
        Application["transformContext"] = context;
        if ((bool)Application["debug"])
        {
            HttpContext.Current.Response.Write("Has Xml Changed: " + hasXmlSourceChanged + ":" + context.RequestXmlETag + "<br/>");
            HttpContext.Current.Response.Write("Has Xslt Changed: " + hasBaseXsltSourceChanged + "<br/>");
            HttpContext.Current.Response.Write("Xml ETag: " + context.GetRequestHashcode(true) + "<br/>");
            HttpContext.Current.Response.Write("XdmNode Count: " + xslTransformationManager.GetXdmNodeHashtableCount() + "<br/>");
            Application["debugOutput"] = (string)("<DebugOutput>" + WriteDebugOutput(context, xslTransformationManager, new StringBuilder(), CONTENT_IS_MEMCACHED).ToString() + "</DebugOutput>");
        }

    }

    protected void Application_EndRequest(object sender, EventArgs e)
    {
        if ((bool)Application["debug"])
            HttpContext.Current.Response.Write((string)Application["debugOutput"]);
    }

    protected void Application_AuthenticateRequest(object sender, EventArgs e)
    {

    }

    protected void Application_Error(object sender, EventArgs e)
    {

    }

    protected void Application_End(object sender, EventArgs e)
    {
        SockIOPool.GetInstance().Shutdown();
    }

    protected StringBuilder WriteDebugOutput(Context context, XsltTransformationManager xsltTransformationManager, StringBuilder builder, bool CONTENT_IS_MEMCACHED)
    {
        builder.Append(CreateNode("Request_File_ETag", context.ETag));
        builder.Append(CreateNode("CompilerBaseUri", xsltTransformationManager.Compiler.BaseUri));
        builder.Append(CreateNode("Compiler", xsltTransformationManager.Compiler.GetHashCode()));
        //foreach(System.Reflection.PropertyInfo t in HttpContext.Current.GetType().GetProperties()){
        // 
        //}
        builder.Append(CreateNode("Serializer", xsltTransformationManager.Serializer.GetHashCode()));
        builder.Append(CreateNode("BaseXsltName", xsltTransformationManager.BaseXsltName));
        builder.Append(CreateNode("BaseXsltUri", xsltTransformationManager.BaseXsltUri));
        builder.Append(CreateNode("BaseXsltUriHash", xsltTransformationManager.BaseXsltUriHash));
        builder.Append(CreateNode("UseMemcached", (bool)Application["appStart_usememcached"]));
        builder.Append(CreateNode("Transform", xsltTransformationManager.Transform.GetHashCode()));
        builder.Append(CreateNode("Resolver", xsltTransformationManager.Resolver.GetHashCode()));
        builder.Append(CreateNode("XslTransformationManager", xsltTransformationManager.GetHashCode()));
        builder.Append(CreateNode("GlobalXsltParms", _globalXsltParams.GetHashCode()));
        builder.Append(CreateNode("Processor", xsltTransformationManager.Processor.GetHashCode()));
        builder.Append(CreateNode("RequestXmlSourceExecutionFilePath", HttpContext.Current.Request.MapPath(HttpContext.Current.Request.CurrentExecutionFilePath)));
        builder.Append(CreateNode("RequestUrl", context.RequestUri, true));
        builder.Append(CreateNode("RequestIsMemcached", CONTENT_IS_MEMCACHED));
        builder.Append(CreateNode("RequestHashcode", context.GetRequestHashcode(false)));
        builder.Append(CreateNode("ContextHashcode", context.GetHashCode()));
        builder.Append(CreateNode("ContextUri", context.RequestUri, true));
        builder.Append(CreateNode("ContextHttpParamsCount", context.HttpParams.Count));
        IEnumerator httpParamsEnum = context.HttpParams.GetEnumerator();
        int i = 0;
        while (httpParamsEnum.MoveNext())
        {
            string key = context.HttpParams.AllKeys[i].ToString();
            builder.Append("<Param>");
            builder.Append(CreateNode("Name", key));
            builder.Append(CreateNode("Value", context.HttpParams[key]));
            builder.Append("</Param>");
            i += 1;
        }
        MemcachedClient mc = (MemcachedClient)Application["appStart_memcached"];
        IDictionary stats = mc.Stats();

        foreach (string key1 in stats.Keys)
        {
            builder.Append("<Key>");
            builder.Append(CreateNode("Name", key1));
            Hashtable values = (Hashtable)stats[key1];
            foreach (string key2 in values.Keys)
            {
                builder.Append(CreateNode(key2, values[key2]));
            }
            builder.Append("</Key>");
        }
        builder.Append(CreateNode("ContextXsltParamsCount", context.XsltParams.Count));
        foreach (DictionaryEntry entry in context.XsltParams)
        {
            builder.Append(CreateNode("XsltParamName", (string)entry.Key));
            builder.Append(CreateNode("XsltParamValue", (string)entry.Value));
        }
        return builder;
    }
    protected String CreateNode(String name, object value, bool useCDATA)
    {
        return "<" + name + "><![CDATA[" + value.ToString() + "]]></" + name + ">";
    }
    protected String CreateNode(String name, object value)
    {
        return "<" + name + ">" + value.ToString() + "</" + name + ">";
    }
</script>

