using System;
using System.Collections;
using Saxon.Api;
using System.Xml;
using System.IO;

namespace Xameleon.Transform {

  public struct XslTransformationManager {

    Hashtable _xsltHashtable;
    Hashtable _sourceHashtable;
    Hashtable _xdmNodeHashtable;
    Processor _processor;
    Serializer _serializer;
    DocumentBuilder _builder;
    XsltCompiler _compiler;
    XmlUrlResolver _resolver;
    Transform _transform;
    Uri _baseXsltUri;
    String _baseXsltUriHash;
    String _baseXsltName;

    public XslTransformationManager(Processor processor)
      : this(processor, new Transform(), new XmlUrlResolver(), new Serializer(), new Hashtable(), new Hashtable(), new Hashtable(), null, null, null) {
    }
    public XslTransformationManager(Processor processor, Transform transform)
      : this(processor, transform, new XmlUrlResolver(), new Serializer(), new Hashtable(), new Hashtable(), new Hashtable(), null, null, null) {
    }
    public XslTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver)
      : this(processor, transform, resolver, new Serializer(), new Hashtable(), new Hashtable(), new Hashtable(), null, null, null) {
    }
    public XslTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer)
      : this(processor, transform, resolver, serializer, new Hashtable(), new Hashtable(), new Hashtable(), null, null, null) {
    }
    public XslTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Hashtable xsltHashtable)
      : this(processor, transform, resolver, serializer, xsltHashtable, new Hashtable(), new Hashtable(), null, null, null) {
    }
    public XslTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Hashtable xsltHashtable, Hashtable xmlSourceHashtable)
      : this(processor, transform, resolver, serializer, xsltHashtable, xmlSourceHashtable, new Hashtable(), null, null, null) {
    }
    public XslTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Hashtable xsltHashtable, Hashtable xmlSourceHashtable, Hashtable xdmNodeHashtable)
      : this(processor, transform, resolver, serializer, xsltHashtable, xmlSourceHashtable, xdmNodeHashtable, null, null, null) {
    }
    public XslTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Hashtable xsltHashtable, Hashtable xmlSourceHashtable, Hashtable xdmNodeHashtable, Uri baseXsltUri, String baseXsltUriHash, String baseXsltName) {
      _baseXsltUri = baseXsltUri;
      _baseXsltUriHash = baseXsltUriHash;
      _baseXsltName = baseXsltName;
      _transform = transform;
      _xsltHashtable = xsltHashtable;
      _processor = processor;
      _compiler = _processor.NewXsltCompiler();
      _compiler.BaseUri = _baseXsltUri;
      _sourceHashtable = xmlSourceHashtable;
      _resolver = resolver;
      _compiler.XmlResolver = _resolver;
      _builder = _processor.NewDocumentBuilder();
      _serializer = serializer;
      _xdmNodeHashtable = xdmNodeHashtable;
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

    public void AddXmlSource(string name, Uri uri) {
      string sourceUriHash = uri.GetHashCode().ToString();
      string key = name + ":" + sourceUriHash;
      Stream xmlStream = (Stream)_resolver.GetEntity(uri, null, typeof(Stream));
      _sourceHashtable[key] = (Stream)xmlStream;
    }

    public XdmNode GetXdmNode(string name, string xmlSource) {
      Uri xmlSourceUri = new Uri(xmlSource);
      return GetXdmNode(name, xmlSourceUri);
    }

    public XdmNode GetXdmNode(string name, Uri xmlSourceUri) {
      Stream xmlStream = (Stream)_resolver.GetEntity(xmlSourceUri, null, typeof(Stream));
      string xmlStreamHash = xmlStream.GetHashCode().ToString();
      string key = name + ":" + xmlStreamHash;
      return (XdmNode)getXdmNode(key, xmlStream);
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
    public XsltTransformer GetTransformer(string name) {
      //TODO: This is a weak temporary hack.
      return (XsltTransformer)_xsltHashtable[name];
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

    private XdmNode getXdmNode(string key, Stream xmlSourceStream) {
      foreach (DictionaryEntry entry in _sourceHashtable) {
        string uri = (string)entry.Key;
        if (uri == key) {
          return (XdmNode)_sourceHashtable[uri];
        }
      }
      XdmNode node = _builder.Build(xmlSourceStream);
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
    public String BaseXsltName {
      get { return _baseXsltName; }
      set { _baseXsltName = value; }
    }
  }
}
