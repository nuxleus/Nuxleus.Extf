using System;
using System.Collections;
using Saxon.Api;
using System.Xml;
using System.IO;

namespace Xameleon.Transform {

  public struct XslTransformationManager {

    Hashtable _xsltHashtable;
    Hashtable _sourceHashtable;
    Processor _processor;
    Serializer _serializer;
    DocumentBuilder _builder;
    XsltCompiler _compiler;
    XmlUrlResolver _resolver;
    Transform _transform;
    Uri _baseXsltUri;
    String _baseXsltUriHash;
    String _baseXsltName;

    public XslTransformationManager(Processor processor) {
      _baseXsltUri = null;
      _baseXsltUriHash = null;
      _baseXsltName = null;
      _transform = new Transform();
      _processor = processor;
      _xsltHashtable = new Hashtable();
      _sourceHashtable = new Hashtable();
      _compiler = _processor.NewXsltCompiler();
      _compiler.BaseUri = _baseXsltUri;
      _resolver = new XmlUrlResolver();
      _compiler.XmlResolver = _resolver;
      _builder = _processor.NewDocumentBuilder();
      _serializer = new Serializer();
    }

    public XslTransformationManager(Processor processor, Transform transform) {
      _baseXsltUri = null;
      _baseXsltUriHash = null;
      _baseXsltName = null;
      _transform = transform;
      _processor = processor;
      _xsltHashtable = new Hashtable();
      _sourceHashtable = new Hashtable();
      _compiler = _processor.NewXsltCompiler();
      _compiler.BaseUri = _baseXsltUri;
      _resolver = new XmlUrlResolver();
      _compiler.XmlResolver = _resolver;
      _builder = _processor.NewDocumentBuilder();
      _serializer = new Serializer();
    }
    public XslTransformationManager(Processor processor, XmlUrlResolver resolver, Transform transform) {
      _baseXsltUri = null;
      _baseXsltUriHash = null;
      _baseXsltName = null;
      _transform = transform;
      _xsltHashtable = new Hashtable();
      _processor = processor;
      _sourceHashtable = new Hashtable();
      _compiler = _processor.NewXsltCompiler();
      _compiler.BaseUri = _baseXsltUri;
      _resolver = resolver;
      _compiler.XmlResolver = _resolver;
      _builder = _processor.NewDocumentBuilder();
      _serializer = new Serializer();
    }
    public XslTransformationManager(Processor processor, Serializer serializer, XmlUrlResolver resolver, Transform transform) {
      _baseXsltUri = null;
      _baseXsltUriHash = null;
      _baseXsltName = null;
      _transform = transform;
      _xsltHashtable = new Hashtable();
      _processor = processor;
      _sourceHashtable = new Hashtable();
      _compiler = _processor.NewXsltCompiler();
      _compiler.BaseUri = _baseXsltUri;
      _resolver = resolver;
      _compiler.XmlResolver = _resolver;
      _builder = _processor.NewDocumentBuilder();
      _serializer = serializer;
    }
    public XslTransformationManager(Processor processor, Serializer serializer, XmlUrlResolver resolver, Hashtable table, Transform transform) {
      _baseXsltUri = null;
      _baseXsltUriHash = null;
      _baseXsltName = null;
      _transform = transform;
      _xsltHashtable = table;
      _processor = processor;
      _compiler = _processor.NewXsltCompiler();
      _compiler.BaseUri = _baseXsltUri;
      _sourceHashtable = new Hashtable();
      _resolver = resolver;
      _compiler.XmlResolver = _resolver;
      _builder = _processor.NewDocumentBuilder();
      _serializer = serializer;
    }
    public XslTransformationManager(Processor processor, Serializer serializer, XmlUrlResolver resolver, Hashtable table, Hashtable source, Transform transform) {
      _baseXsltUri = null;
      _baseXsltUriHash = null;
      _baseXsltName = null;
      _transform = transform;
      _xsltHashtable = table;
      _processor = processor;
      _compiler = _processor.NewXsltCompiler();
      _compiler.BaseUri = _baseXsltUri;
      _sourceHashtable = source;
      _resolver = resolver;
      _compiler.XmlResolver = _resolver;
      _builder = _processor.NewDocumentBuilder();
      _serializer = serializer;
    }
    public void SetBaseXsltContext(BaseXsltContext baseXsltContext) {
      _baseXsltUri = baseXsltContext.BaseXsltUri;
      _baseXsltName = baseXsltContext.Name;
      _baseXsltUriHash = baseXsltContext.UriHash;
      _compiler.BaseUri = _baseXsltUri;
      _builder.BaseUri = _baseXsltUri;
    }
    public void AddTransformer(string name, Uri uri, XmlUrlResolver resolver) {
      string xsltUriHash = uri.GetHashCode().ToString();
      string key = name + ":" + xsltUriHash;
      XsltTransformer transformer = _compiler.Compile(uri).Load();
      _xsltHashtable[key] = (XsltTransformer)transformer;
    }

    public void AddSource(string name, Uri uri) {
      string sourceUriHash = uri.GetHashCode().ToString();
      string key = name + ":" + sourceUriHash;
      Stream xmlStream = (Stream)_resolver.GetEntity(uri, null, typeof(Stream));
      _sourceHashtable[key] = (Stream)xmlStream;
    }

    public XdmNode GetXmlSourceStream(string name, string xmlSource) {
      Uri xmlSourceUri = new Uri(xmlSource);
      string xmlSourceUriHash = xmlSourceUri.GetHashCode().ToString();
      string key = name + ":" + xmlSourceUriHash;
      return (XdmNode)getXmlSourceStream(key, xmlSourceUri);
    }

    public XdmNode GetXmlSourceStream(string name, Uri xmlSourceUri) {
      string xmlSourceUriHash = xmlSourceUri.GetHashCode().ToString();
      string key = name + ":" + xmlSourceUriHash;
      return (XdmNode)getXmlSourceStream(key, xmlSourceUri);
    }

    public XsltTransformer GetTransformer(string name, string href, Uri baseUri) {

      Uri xsltUri = new Uri(baseUri, href);
      string xsltUriHash = xsltUri.GetHashCode().ToString();
      string key = name + ":" + xsltUriHash;

      return getTransformer(key, xsltUri);
    }

    public XsltTransformer GetTransformer(string xsltUriHash, Uri xsltUri) {
      return getTransformer(xsltUriHash, xsltUri);
    }

    private XsltTransformer getTransformer(string key, Uri xsltUri) {
      foreach (DictionaryEntry entry in _xsltHashtable) {
        string uri = (string)entry.Key;
        if (uri == key) {
          return (XsltTransformer)_xsltHashtable[uri];
        }
      }

      XsltTransformer transformer = _processor.NewXsltCompiler().Compile(xsltUri).Load();
      _xsltHashtable[key] = (XsltTransformer)transformer;
      return transformer;
    }

    private XdmNode getXmlSourceStream(string key, Uri sourceUri) {
      foreach (DictionaryEntry entry in _sourceHashtable) {
        string uri = (string)entry.Key;
        if (uri == key) {
          return (XdmNode)_sourceHashtable[uri];
        }
      }
      Stream xmlStream = (Stream)_resolver.GetEntity(sourceUri, null, typeof(Stream));
      XdmNode node = _builder.Build(xmlStream);
      _sourceHashtable[key] = (XdmNode)node;
      return (XdmNode)node;
    }

    public Hashtable XsltHashtable {
      get { return _xsltHashtable; }
      set { _xsltHashtable = value; }
    }
    public Hashtable XmlSourceHashtable {
      get { return _sourceHashtable; }
      set { _sourceHashtable = value; }
    }
    public Processor Processor {
      get { return _processor; }
      set { _processor = value; }
    }
    public DocumentBuilder DocumentBuilder {
      get { return _builder; }
      set { _builder = value; }
    }
    public XmlUrlResolver Resolver {
      get { return _resolver; }
      set { _resolver = value; }
    }
    public XsltCompiler Compiler {
      get { return _compiler; }
      set { _compiler = value; }
    }
    public Serializer Serializer {
      get { return _serializer; }
      set { _serializer = value; }
    }
    public Transform Transform {
      get { return _transform; }
      set { _transform = value; }
    }
    public Uri BaseXsltUri {
      get { return _baseXsltUri; }
      set { _baseXsltUri = value; }
    }
    public String BaseXsltUriHash {
      get { return _baseXsltUriHash; }
      set { _baseXsltUriHash = value; }
    }
  }
}
