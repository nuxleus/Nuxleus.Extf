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

    HttpContext _HttpContext;
    AppSettings _AppSettings;
    String _RequestUriHash;
    Processor _Processor;
    Hashtable _XsltParams;
    Hashtable _HttpContextParams;
    String _Backup;
    TextWriter _Writer;
    TextWriter _ResponseOutput;
    Serializer _Destination;
    MemcachedClient _MemcachedClient;
    StringBuilder _StringBuilder;
    XslTransformationManager _XsltTransformationManager;
    bool _INITIALIZED;

    public Context(HttpContext context, Hashtable xsltParams, XslTransformationManager xslTransformationManager, params string[] httpContextParamList) {
      _AppSettings = (AppSettings)context.Application["appSettings"];
      _ResponseOutput = context.Response.Output;
      _RequestUriHash = context.Request.Url.GetHashCode().ToString();
      _HttpContext = context;
      _Processor = xslTransformationManager.Processor;
      _HttpContextParams = new Hashtable();
      _Destination = xslTransformationManager.Serializer;
      _StringBuilder = new StringBuilder();
      _Writer = new StringWriter(_StringBuilder);
      _Destination.SetOutputWriter(_Writer);
      _MemcachedClient = null;
      _XsltTransformationManager = xslTransformationManager;

      _XsltParams = xsltParams;

      _Backup = @"<system>
                    <message>
                      Something very very bad has happened. Run while you still can!
                    </message>
                  </system>";
      _INITIALIZED = true;
    }

    public XslTransformationManager XslTransformationManager {
      get { return _XsltTransformationManager; }
      set { _XsltTransformationManager = value; }
    }

    public HttpContext HttpContext {
      get { return _HttpContext; }
      set { _HttpContext = value; }
    }

    public String RequestUriHash {
      get { return _RequestUriHash; }
      set { _RequestUriHash = value; }
    }
    public Serializer Destination {
      get { return _Destination; }
      set { _Destination = value; }
    }
    public AppSettings Settings {
      get { return _AppSettings; }
      set { _AppSettings = value; }
    }
    public String XmlBackup {
      get { return _Backup; }
      set { _Backup = value; }
    }
    public Processor Processor {
      get { return _Processor; }
      set { _Processor = value; }
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
    public bool IsInitialized {
      get { return _INITIALIZED; }
      set { _INITIALIZED = value; }
    }

    #region IDisposable Members

    public void Dispose() {

    }

    #endregion
  }
}
