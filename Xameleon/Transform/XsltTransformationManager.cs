using System;
using System.Collections;
using Saxon.Api;
using System.Xml;
using System.IO;
using System.Web;
using Xameleon.Cryptography;

namespace Xameleon.Transform {

  //NOTE: TransformEngine enum PLACEHOLDER FOR FUTURE USE
  public enum TransformEngine { SAXON, MVPXML, NET1_1, NET2_0, NET3_0, NET3_5 }

  public struct XsltTransformationManager {

    Hashtable _xsltHashtable;
    Hashtable _sourceHashtable;
    Hashtable _xdmNodeHashtable;
    Hashtable _namedXsltHashtable;
    Hashtable _namedXsltETagIndex;
    Processor _processor;
    Serializer _serializer;
    DocumentBuilder _builder;
    XsltCompiler _compiler;
    XmlUrlResolver _resolver;
    Transform _transform;
    Uri _baseXsltUri;
    String _baseXsltUriHash;
    String _baseXsltName;
    static HashAlgorithm _hashAlgorithm;
    //NOTE: TransformEngine enum PLACEHOLDER FOR FUTURE USE
    static TransformEngine _transformEngine;

    public XsltTransformationManager(Processor processor)
      : this(processor, new Transform(), new XmlUrlResolver(), new Serializer(), new Hashtable(), new Hashtable(), new Hashtable(), new Hashtable(), null, null, null) {
    }
    public XsltTransformationManager(Processor processor, Transform transform)
      : this(processor, transform, new XmlUrlResolver(), new Serializer(), new Hashtable(), new Hashtable(), new Hashtable(), new Hashtable(), null, null, null) {
    }
    public XsltTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver)
      : this(processor, transform, resolver, new Serializer(), new Hashtable(), new Hashtable(), new Hashtable(), new Hashtable(), null, null, null) {
    }
    public XsltTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer)
      : this(processor, transform, resolver, serializer, new Hashtable(), new Hashtable(), new Hashtable(), new Hashtable(), null, null, null) {
    }
    public XsltTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Hashtable xsltHashtable)
      : this(processor, transform, resolver, serializer, xsltHashtable, new Hashtable(), new Hashtable(), new Hashtable(), null, null, null) {
    }
    public XsltTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Hashtable xsltHashtable, Hashtable namedXsltHashtable)
      : this(processor, transform, resolver, serializer, xsltHashtable, new Hashtable(), new Hashtable(), namedXsltHashtable, null, null, null) {
    }
    public XsltTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Hashtable xsltHashtable, Hashtable xmlSourceHashtable, Hashtable namedXsltHashtable)
      : this(processor, transform, resolver, serializer, xsltHashtable, xmlSourceHashtable, new Hashtable(), namedXsltHashtable, null, null, null) {
    }
    public XsltTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Hashtable xsltHashtable, Hashtable xmlSourceHashtable, Hashtable xdmNodeHashtable, Hashtable namedXsltHashtable)
      : this(processor, transform, resolver, serializer, xsltHashtable, xmlSourceHashtable, xdmNodeHashtable, namedXsltHashtable, null, null, null) {
    }
    public XsltTransformationManager(Processor processor, Transform transform, XmlUrlResolver resolver, Serializer serializer, Hashtable xsltHashtable, Hashtable xmlSourceHashtable, Hashtable xdmNodeHashtable, Hashtable namedXsltHashtable, Uri baseXsltUri, String baseXsltUriHash, String baseXsltName) {
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
      _namedXsltHashtable = namedXsltHashtable;
      _namedXsltETagIndex = new Hashtable();
      _hashAlgorithm = HashAlgorithm.SHA256;
      //NOTE: TransformEngine enum PLACEHOLDER FOR FUTURE USE
      _transformEngine = TransformEngine.SAXON;
    }

    public void SetBaseXsltContext(BaseXsltContext baseXsltContext) {
      _baseXsltUri = baseXsltContext.BaseXsltUri;
      _baseXsltName = baseXsltContext.Name;
      _baseXsltUriHash = baseXsltContext.UriHash;
      _compiler.BaseUri = _baseXsltUri;
      _builder.BaseUri = _baseXsltUri;
    }

    public void AddTransformer(string name, Uri uri, XmlUrlResolver resolver) {
      string key = GenerateNamedETagKey(name, uri);
      XsltTransformer transformer = _compiler.Compile(uri).Load();
      _xsltHashtable[key] = (XsltTransformer)transformer;
    }

    public void AddXmlSource(string name, Uri uri) {
      Stream xmlStream = (Stream)_resolver.GetEntity(uri, null, typeof(Stream));
      string key = GenerateNamedETagKey(name, uri, xmlStream);
      _sourceHashtable[key] = (Stream)xmlStream;
    }

    public XdmNode GetXdmNode(string name, string xmlSource) {
      Uri xmlSourceUri = new Uri(xmlSource);
      return GetXdmNode(name, xmlSourceUri);
    }

    public XdmNode GetXdmNode(string name, Uri xmlSourceUri) {
      Stream xmlStream = (Stream)_resolver.GetEntity(xmlSourceUri, null, typeof(Stream));
      string key = GenerateNamedETagKey(name, xmlSourceUri, xmlStream);
      return (XdmNode)getXdmNode(key, xmlStream);
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

    public XsltTransformer GetTransformer(string eTag, Uri xsltUri) {
      return getTransformer(eTag, xsltUri, true);
    }

    public XsltTransformer GetTransformer(string name, string href, Uri baseUri) {
      Uri xsltUri = new Uri(baseUri, href);
      return getTransformer(GenerateNamedETagKey(name, xsltUri), name, xsltUri);
    }

    public XsltTransformer GetTransformer(string name) {
      Uri xsltUri = (Uri)_namedXsltHashtable[name];
      return getTransformer(GenerateNamedETagKey(name, xsltUri), name, xsltUri);
    }

    private XsltTransformer getTransformer(string key, string xsltName, Uri xsltUri) {
      XsltTransformer transformer;
      string transformerKey;
      string namedETag = (string)_namedXsltETagIndex[xsltName];
      if (namedETag != null && namedETag == key)
        return getTransformer(namedETag, xsltUri, false);
      else {
        _namedXsltETagIndex[xsltName] = key;
        return getTransformer(namedETag, xsltUri, true);
      }
    }

    private XsltTransformer getTransformer(string key, Uri xsltUri, bool replaceExistingXsltTransformer) {
      XsltTransformer transformer;
      transformer = (XsltTransformer)_namedXsltHashtable[key];

      if (transformer != null && !replaceExistingXsltTransformer) {
        return transformer;
      } else
        transformer = createNewTransformer(xsltUri);

      _xsltHashtable[key] = (XsltTransformer)transformer;
      return transformer;
    }

    public static String GenerateNamedETagKey(String name, Uri sourceUri, params object[] objectParams) {
      FileInfo fileInfo = new FileInfo(sourceUri.LocalPath);
      return name + ":" + Context.GenerateETag((string)HttpContext.Current.Application["hashkey"], _hashAlgorithm, fileInfo.LastWriteTimeUtc, fileInfo.Length, sourceUri, objectParams);
    }

    private XsltTransformer createNewTransformer(Uri xsltUri) {
      return _processor.NewXsltCompiler().Compile(xsltUri).Load();
    }

    public Hashtable XsltHashtable {
      get { return _xsltHashtable; }
      set { _xsltHashtable = value; }
    }
    public Hashtable XmlSourceHashtable {
      get { return _sourceHashtable; }
      set { _sourceHashtable = value; }
    }
    public Hashtable NamedXsltHashtable {
      get { return _namedXsltHashtable; }
      set { _namedXsltHashtable = value; }
    }
    public HashAlgorithm HashAlgorithm {
      get { return _hashAlgorithm; }
      set { _hashAlgorithm = value; }
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
