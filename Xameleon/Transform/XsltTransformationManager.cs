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
    Hashtable _xdmNodeETagIndex;
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
    bool _xmlInMemory;
    bool _xsltInMemory;

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
      _xdmNodeETagIndex = new Hashtable();
      //NOTE: TransformEngine enum PLACEHOLDER FOR FUTURE USE
      _transformEngine = TransformEngine.SAXON;
      _xmlInMemory = false;
      _xsltInMemory = false;
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
      XsltTransformer transformer = createNewTransformer(uri);
      _xsltHashtable[key] = (XsltTransformer)transformer;
      _namedXsltETagIndex[name] = (string)key; 
    }

    public void AddXmlSource(string name, Uri uri) {
      Stream xmlStream = createNewXmlStream(uri);
      _sourceHashtable[name] = (Stream)xmlStream;
    }

    public XdmNode GetXdmNode(string name, string xmlSource) {
      Uri xmlSourceUri = new Uri(xmlSource);
      return GetXdmNode(name, xmlSourceUri);
    }

    public XdmNode GetXdmNode(string name, Uri xmlSourceUri) {

      Uri xdmNodeUri = (Uri)_xdmNodeETagIndex[name];
      if (xdmNodeUri != null && xdmNodeUri == xmlSourceUri) {
        _xmlInMemory = true;
        return getXdmNode(name, xmlSourceUri, false);
      } else {
        _xdmNodeETagIndex[name] = xmlSourceUri;
        return getXdmNode(name, xmlSourceUri, true);
      }
    }

    private XdmNode getXdmNode(string key, Uri xmlSourceUri, bool replaceExistingXdmNode) {

      XdmNode node = (XdmNode)_xdmNodeHashtable[key];

      if (node != null && !replaceExistingXdmNode) {
        return node;
      } else {
        using (Stream stream = createNewXmlStream(xmlSourceUri)) {
          node = createNewXdmNode(stream);
        }
      }
      _xdmNodeHashtable[key] = node;
      return node;
    }

    private Stream createNewXmlStream(Uri xmlSourceUri) {
      return (Stream)_resolver.GetEntity(xmlSourceUri, null, typeof(Stream));
    }

    private XdmNode createNewXdmNode(Stream xmlSourceStream) {
      return (XdmNode)_builder.Build(xmlSourceStream);
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
      if (namedETag != null && namedETag == key) {
        _xsltInMemory = true;
        return getTransformer(namedETag, xsltUri, false);
      } else {
        _namedXsltETagIndex[xsltName] = key;
        return getTransformer(key, xsltUri, true);
      }
    }

    private XsltTransformer getTransformer(string key, Uri xsltUri, bool replaceExistingXsltTransformer) {
      XsltTransformer transformer;
      transformer = (XsltTransformer)_namedXsltHashtable[key];

      if(transformer != null && !replaceExistingXsltTransformer) {
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
      using (Stream stream = createNewXmlStream(xsltUri)) {
        return _processor.NewXsltCompiler().Compile(stream).Load();
      }
    }

    public bool XmlInMemory {
      get { return _xmlInMemory; }
      set { _xmlInMemory = value; }
    }
    public bool XsltInMemory {
      get { return _xsltInMemory; }
      set { _xsltInMemory = value; }
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
