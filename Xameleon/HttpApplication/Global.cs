using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Xameleon.Transform;
using Xameleon.Configuration;
using Xameleon.Cryptography;
using Memcached.ClientLibrary;
using System.Collections.Generic;
using Saxon.Api;
using IronPython.Hosting;
using System.IO;
using System.Xml;
using System.Net;
using System.Text;

namespace Xameleon.HttpApplication {

  public class Global : System.Web.HttpApplication {


    bool _useMemCached = false;
    MemcachedClient _memcachedClient = null;
    SockIOPool _pool = null;
    AspNetXameleonConfiguration _xameleonConfiguration = AspNetXameleonConfiguration.GetConfig();
    AspNetAwsConfiguration _awsConfiguration = AspNetAwsConfiguration.GetConfig();
    AspNetBungeeAppConfiguration _bungeeAppConfguration = AspNetBungeeAppConfiguration.GetConfig();
    AspNetMemcachedConfiguration _memcachedConfiguration = AspNetMemcachedConfiguration.GetConfig();
    XsltTransformationManager _xsltTransformationManager;
    Transform.Transform _transform = new Transform.Transform();
    Processor _processor = new Processor();
    Serializer _serializer = new Serializer();
    XmlUrlResolver _resolver = new XmlUrlResolver();
    Hashtable _namedXsltHashtable = new Hashtable();
    Hashtable _globalXsltParams = new Hashtable();
    BaseXsltContext _baseXsltContext;
    HashAlgorithm _hashAlgorithm = HashAlgorithm.SHA256;
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

        _xsltTransformationManager = new XsltTransformationManager(_processor, _transform, _resolver, _serializer);
        _resolver.Credentials = CredentialCache.DefaultCredentials;
        _namedXsltHashtable = _xsltTransformationManager.NamedXsltHashtable;
        
        string hashkey =  (string)_xameleonConfiguration.BaseSettings.ObjectHashKey;
        Application["hashkey"] = hashkey;
        
        foreach (PreCompiledXslt xslt in _xameleonConfiguration.PreCompiledXslt) {
            string localBaseUri = (string)_xameleonConfiguration.PreCompiledXslt.BaseUri;
            if (localBaseUri == String.Empty)
                localBaseUri = baseUri;
            Uri xsltUri = new Uri(HttpContext.Current.Server.MapPath(localBaseUri + xslt.Uri));
            _xsltTransformationManager.Compiler.BaseUri = xsltUri;
            _xsltTransformationManager.AddTransformer(xslt.Name, xsltUri, _resolver, xslt.InitialMode, xslt.InitialTemplate);
            _namedXsltHashtable.Add(xslt.Name, xsltUri);
            if (xslt.UseAsBaseXslt == "yes") {
                _baseXsltContext = new BaseXsltContext(xsltUri, XsltTransformationManager.GenerateNamedETagKey(xslt.Name, xsltUri), xslt.Name);
            }
        }

        _xsltTransformationManager.SetBaseXsltContext(_baseXsltContext);

        foreach (XsltParam xsltParam in _xameleonConfiguration.GlobalXsltParam) {
            _globalXsltParams[xsltParam.Name] = (string)xsltParam.Select;
        }

        if (_memcachedClient != null)
            Application["appStart_memcached"] = _memcachedClient;
        Application["appStart_usememcached"] = _useMemCached;
        Application["appStart_xslTransformationManager"] = _xsltTransformationManager;
        Application["appStart_namedXsltHashtable"] = _namedXsltHashtable;
        Application["appStart_globalXsltParams"] = _globalXsltParams;
        
    }

    protected void Session_Start(object sender, EventArgs e) {

    }

    protected void Application_BeginRequest(object sender, EventArgs e) {

        Hashtable xsltParams = (Hashtable)Application["appStart_globalXsltParams"];
        FileInfo fileInfo = new FileInfo(HttpContext.Current.Request.MapPath(HttpContext.Current.Request.CurrentExecutionFilePath));
        Context context = new Context(HttpContext.Current, HashAlgorithm.SHA256, (string)Application["hashkey"], fileInfo, (Hashtable)xsltParams.Clone(), fileInfo.LastWriteTimeUtc, fileInfo.Length);
        StringBuilder builder = new StringBuilder();
        TextWriter writer = new StringWriter(builder);
        XsltTransformationManager xslTransformationManager = (XsltTransformationManager)Application["appStart_xslTransformationManager"];
        bool CONTENT_IS_MEMCACHED = false;
        bool useMemCached = (bool)Application["appStart_usememcached"];
        bool hasXmlSourceChanged = xslTransformationManager.HasXmlSourceChanged(context.ETag);
        bool hasBaseXsltSourceChanged = xslTransformationManager.HasBaseXsltSourceChanged();
        HttpContext.Current.Response.Write("Has Xml Changed: " + hasXmlSourceChanged + "<br/>");
        HttpContext.Current.Response.Write("Has Xslt Changed: " + hasBaseXsltSourceChanged + "<br/>");
        MemcachedClient memcachedClient = (MemcachedClient)Application["appStart_memcached"];
        Application["memcached"] = memcachedClient;
        
        if (useMemCached) {
            string obj = (string)memcachedClient.Get(context.GetRequestHashcode(false).ToString());
            if (obj != null && !(hasXmlSourceChanged || hasBaseXsltSourceChanged)) {
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
        Application["USE_MEMCACHED"] = useMemCached;
        Application["xsltTransformationManager"] = xslTransformationManager;
        Application["namedXsltHashtable"] = (Hashtable)Application["appStart_namedXsltHashtable"];
        Application["transformContext"] = context;
        if (_DEBUG) {
            HttpContext.Current.Response.Write(WriteDebugOutput(context, xslTransformationManager, new StringBuilder(), CONTENT_IS_MEMCACHED).ToString());
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

    protected StringBuilder WriteDebugOutput(Context context, XsltTransformationManager xsltTransformationManager, StringBuilder builder, bool CONTENT_IS_MEMCACHED) {
        builder.Append(("Request File ETag: " + context.ETag + "<br/>"));
        builder.Append("CompilerBaseUri: " + xsltTransformationManager.Compiler.BaseUri + "<br/>");
        builder.Append("Compiler: " + xsltTransformationManager.Compiler.GetHashCode() + "<br/>");
        foreach(System.Reflection.PropertyInfo t in HttpContext.Current.GetType().GetProperties()){
            builder.Append("Number of HttpContext Properties: " + t.ToString() + "<br/>");
        }
        builder.Append("Serializer: " + xsltTransformationManager.Serializer.GetHashCode() + "<br/>");
        builder.Append("BaseXsltName: " + xsltTransformationManager.BaseXsltName + "<br/>");
        builder.Append("BaseXsltUri: " + xsltTransformationManager.BaseXsltUri + "<br/>");
        builder.Append("BaseXsltUriHash: " + xsltTransformationManager.BaseXsltUriHash + "<br/>");
        builder.Append("UseMemcached?: " + (bool)Application["appStart_usememcached"] + "<br/>");
        builder.Append("Transform: " + xsltTransformationManager.Transform.GetHashCode() + "<br/>");
        builder.Append("Resolver: " + xsltTransformationManager.Resolver.GetHashCode() + "<br/>");
        builder.Append("XslTransformationManager: " + xsltTransformationManager.GetHashCode() + "<br/>");
        builder.Append("GlobalXsltParms: " + _globalXsltParams.GetHashCode()+ "<br/>");
        builder.Append("Processor: " + xsltTransformationManager.Processor.GetHashCode() + "<br/>");
        builder.Append("Request XmlSource Execution File Path: " + HttpContext.Current.Request.MapPath(HttpContext.Current.Request.CurrentExecutionFilePath) + "<br/>");
        builder.Append("Request Url: " + context.RequestUri + "<br/>");
        builder.Append("Request is Memcached? " + CONTENT_IS_MEMCACHED + "<br/>");
        builder.Append("Request RequestHashcode: " + context.GetRequestHashcode(true) + "<br/>");
        builder.Append("Context Hashcode: " + context.GetHashCode() + "<br/>");
        builder.Append("Context Uri: " + context.RequestUri + "<br/>");
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

}
}