using System;
using System.Xml;
using System.IO;
using Saxon.Api;
using System.Collections.Specialized;
using System.Web;
using Xameleon.Configuration;
using System.Net;
using Xameleon.Properties;
using System.Collections;
using Memcached.ClientLibrary;
using System.Text;

namespace Xameleon.Transform {

  public struct Context : IDisposable {
    AppSettings _AppSettings;
    String _RequestUriHash;
    String _BaseUri;
    Uri _BaseTemplateUri;
    Uri _XmlSource;
    Uri _XsltSource;
    XmlUrlResolver _Resolver;
    Processor _Processor;
    XsltCompiler _Compiler;
    Stream _SourceXml;
    Stream _TemplateStream;
    XsltExecutable _TransformExecutable;
    String _xsltParamKey;
    Hashtable _XsltParams;
    Hashtable _HttpContextParams;
    String _Backup;
    TextWriter _Writer;
    TextWriter _ResponseOutput;
    DocumentBuilder _Builder;
    XdmNode _Node;
    Serializer _Destination;
    MemcachedClient _MemcachedClient;
    StringBuilder _StringBuilder;

    public Context(HttpContext context, Processor processor, XsltCompiler compiler, Serializer serializer, XmlUrlResolver resolver, bool addHttpContextParams, params string[] httpContextParamList) {
      _AppSettings = (AppSettings)context.Application["appSettings"];
      _ResponseOutput = context.Response.Output;
      _RequestUriHash = context.Request.Url.GetHashCode().ToString();

      string paramPrefix = _AppSettings.GetSetting("xsltParamKeyPrefix");

      if (paramPrefix != null) {
        _xsltParamKey = paramPrefix;
      } else {
        _xsltParamKey = "xsltParam_";
      }

      _BaseUri = compiler.BaseUri.ToString();
      _BaseTemplateUri = compiler.BaseUri;
      _XmlSource = new Uri(context.Request.MapPath(context.Request.CurrentExecutionFilePath));
      _XsltSource = new Uri(Resources.SourceXslt);
      _Resolver = resolver;
      _Processor = processor;
      _Compiler = compiler;
      _SourceXml = (Stream)_Resolver.GetEntity(_XmlSource, null, typeof(Stream));
      _TemplateStream = (Stream)_Resolver.GetEntity(_BaseTemplateUri, null, typeof(Stream));
      _TransformExecutable = _Compiler.Compile(_TemplateStream);
      _HttpContextParams = new Hashtable();
      _Destination = serializer;
      _MemcachedClient = null;
      _StringBuilder = null;

      _StringBuilder = new StringBuilder();
      _Writer = new StringWriter(_StringBuilder);
      _Destination.SetOutputWriter(_Writer);

      _Builder = _Processor.NewDocumentBuilder();
      _Builder.BaseUri = _BaseTemplateUri;
      _Node = _Builder.Build(_SourceXml);
      
      Hashtable xsltParamsHashtable = new Hashtable();

      Hashtable httpContextHashtable = null;
      if (_HttpContextParams.Count == 0) {
        httpContextHashtable = new Hashtable();
        httpContextHashtable["request"] = context.Request;
        httpContextHashtable["response"] = context.Response;
        httpContextHashtable["server"] = context.Server;
        httpContextHashtable["timestamp"] = context.Timestamp;
        httpContextHashtable["session"] = context.Session;
        httpContextHashtable["errors"] = context.AllErrors;
        httpContextHashtable["cache"] = context.Cache;
        httpContextHashtable["user"] = context.User;
      } else
        httpContextHashtable = _HttpContextParams;

      if (addHttpContextParams) {
        //default set of HttpContext object params
        string[] paramList = { "response", "request", "server", "session", "timestamp", "errors", "cache", "user" };
        if (httpContextParamList.Length > 0) paramList = httpContextParamList;
        foreach (string name in paramList) {
          xsltParamsHashtable[name] = httpContextHashtable[name];
        }
      };

      _XsltParams = _AppSettings.GetSettingArray(xsltParamsHashtable, _xsltParamKey);
      _Backup = @"<system>
                    <message>
                      Something very very bad has happened. Run while you still can!
                    </message>
                  </system>";
    }

    public String RequestUriHash {
      get { return _RequestUriHash; }
      set { _RequestUriHash = value; }
    }
    public DocumentBuilder Builder {
      get { return _Builder; }
      set { _Builder = value; }
    }
    public XdmNode Node {
      get { return _Node; }
      set { _Node = value; }
    }
    public Serializer Destination {
      get { return _Destination; }
      set { _Destination = value; }
    }
    public AppSettings Settings {
      get { return _AppSettings; }
      set { _AppSettings = value; }
    }
    public Uri BaseTemplateUri {
      get { return _BaseTemplateUri; }
      set { _BaseTemplateUri = value; }
    }
    public String XmlBackup {
      get { return _Backup; }
      set { _Backup = value; }
    }
    public String XsltParamKey {
      get { return _xsltParamKey; }
      set { _xsltParamKey = value; }
    }
    public XsltExecutable XsltExecutable {
      get { return _TransformExecutable; }
      set { _TransformExecutable = value; }
    }
    public XsltCompiler Compiler {
      get { return _Compiler; }
      set { _Compiler = value; }
    }
    public Processor Processor {
      get { return _Processor; }
      set { _Processor = value; }
    }
    public Stream XmlStream {
      get { return _SourceXml; }
      set { _SourceXml = value; }
    }
    public Stream TemplateStream {
      get { return _TemplateStream; }
      set { _TemplateStream = value; }
    }
    public String BaseUri {
      get { return _BaseUri; }
      set { _BaseUri = value; }
    }
    public Uri XmlSource {
      get { return _XmlSource; }
      set { _XmlSource = value; }
    }
    public Uri XsltSource {
      get { return _XsltSource; }
      set { _XsltSource = value; }
    }
    public XmlUrlResolver Resolver {
      get { return _Resolver; }
      set { _Resolver = value; }
    }
    public Hashtable XsltParams {
      get { return _XsltParams; }
      set { _XsltParams = value; }
    }
    public Hashtable HttpContextParams {
      get { return _HttpContextParams; }
      set { _HttpContextParams = value; }
    }
    public MemcachedClient MemcachedClient {
      get { return _MemcachedClient; }
      set { _MemcachedClient = value; }
    }
    public StringBuilder StringBuilder {
      get { return _StringBuilder; }
      set { _StringBuilder = value; }
    }
    public TextWriter Writer {
      get { return _Writer; }
      set { _Writer = value; }
    }
    public TextWriter ResponseOutput {
      get { return _ResponseOutput; }
      set { _ResponseOutput = value; }
    }

    #region IDisposable Members

    public void Dispose() {
      _XmlSource = null;
      _XsltSource = null;
    }

    #endregion
  }
}
