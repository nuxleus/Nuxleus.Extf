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

namespace Xameleon.Transform {

    public struct Context {
      AppSettings _AppSettings;
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
      NameValueCollection _XsltParams;
      String _Backup;
      TextWriter _TextWriter;
      DocumentBuilder _Builder;
      XdmNode _Node;
      Serializer _Destination;
      Hashtable _XsltObjectParams;

      public Context(HttpContext context, TextWriter writer, bool addHttpContextParams) {
        _AppSettings = new AppSettings();
        _TextWriter = writer;

        string paramPrefix = _AppSettings.GetSetting("xsltParamKeyPrefix");
        string baseTemplate = _AppSettings.GetSetting("baseTemplate");

        if (paramPrefix != null) {
          _xsltParamKey = paramPrefix;
        } else {
          _xsltParamKey = "xsltParam_";
        }

        if (baseTemplate != null) {
          _BaseUri = baseTemplate;
        } else {
          _BaseUri = "http://localhost/";
        }
        _BaseTemplateUri = new Uri(context.Server.MapPath(_BaseUri));
        _XmlSource = new Uri(context.Request.MapPath(context.Request.CurrentExecutionFilePath));
        _XsltSource = new Uri(Resources.SourceXslt);
        _Resolver = new XmlUrlResolver();
        _Resolver.Credentials = CredentialCache.DefaultCredentials;
        _Processor = new Processor();
        _Compiler = _Processor.NewXsltCompiler();
        _Compiler.BaseUri = _BaseTemplateUri;
        _SourceXml = (Stream)_Resolver.GetEntity(_XmlSource, null, typeof(Stream));
        _TemplateStream = (Stream)_Resolver.GetEntity(_BaseTemplateUri, null, typeof(Stream));
        _TransformExecutable = _Compiler.Compile(_TemplateStream);
        _XsltParams = new NameValueCollection();
        _Builder = _Processor.NewDocumentBuilder();
        _Builder.BaseUri = _BaseTemplateUri;
        _Node = _Builder.Build(_SourceXml);
        _Destination = new Serializer();
        _Destination.SetOutputWriter(_TextWriter);
        _XsltObjectParams = new Hashtable();
        if (addHttpContextParams) {
          _XsltObjectParams["response"] = context.Response;
          _XsltObjectParams["request"] = context.Request;
          _XsltObjectParams["server"] = context.Server;
          _XsltObjectParams["session"] = context.Session;
          _XsltObjectParams["timestamp"] = context.Timestamp;
        }
        _Backup = @"<system>
                      <message>
                        Something very very bad has happened. Run while you still can!
                      </message>
                    </system>";
      }

      public Hashtable XsltObjectParams {
        get { return _XsltObjectParams; }
        set { _XsltObjectParams = value; }
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
      public TextWriter Writer {
        get { return _TextWriter; }
        set { _TextWriter = value; }
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
      public NameValueCollection XsltParams {
        get { return _XsltParams; }
        set { _XsltParams = value; }
      }
    }
  }
