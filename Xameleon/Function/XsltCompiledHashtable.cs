using System;
using System.Collections;
using Saxon.Api;
using System.Xml;
using System.IO;

namespace Xameleon.Transform {

  public class XsltCompiledHashtable {

    Hashtable _xsltHashtable;
    Hashtable _sourceHashtable;
    Processor _processor;
    DocumentBuilder _builder;
    XsltCompiler _compiler = null;
    XmlUrlResolver _resolver = null;

    public XsltCompiledHashtable() {
      _xsltHashtable = new Hashtable();
      _processor = new Processor();
      _sourceHashtable = new Hashtable();
      _compiler = _processor.NewXsltCompiler();
      _resolver = new XmlUrlResolver();
      _compiler.XmlResolver = _resolver;
      _builder = _processor.NewDocumentBuilder();
    }
    public XsltCompiledHashtable(XmlUrlResolver resolver) {
      _xsltHashtable = new Hashtable();
      _processor = new Processor();
      _sourceHashtable = new Hashtable();
      _compiler = _processor.NewXsltCompiler();
      _resolver = resolver;
      _compiler.XmlResolver = _resolver;
      _builder = _processor.NewDocumentBuilder();
    }
    public XsltCompiledHashtable(Hashtable table) {
      _xsltHashtable = table;
      _processor = new Processor();
      _compiler = _processor.NewXsltCompiler();
      _sourceHashtable = new Hashtable();
      _resolver = new XmlUrlResolver();
      _compiler.XmlResolver = _resolver;
      _builder = _processor.NewDocumentBuilder();
    }
    public XsltCompiledHashtable(Hashtable table, Hashtable source) {
      _xsltHashtable = table;
      _processor = new Processor();
      _compiler = _processor.NewXsltCompiler();
      _sourceHashtable = source;
      _resolver = new XmlUrlResolver();
      _compiler.XmlResolver = _resolver;
      _builder = _processor.NewDocumentBuilder();
    }
    public void AddTransformer(string name, Uri uri, XmlUrlResolver resolver) {
      string xsltUriHash = uri.GetHashCode().ToString();
      string key = name + ":" + xsltUriHash;
      if (_compiler.BaseUri == null) {
        _compiler.BaseUri = uri;
      }
      if (_builder.BaseUri == null) {
        _builder.BaseUri = uri;
      }
      XsltTransformer transformer = _compiler.Compile(uri).Load();
      _xsltHashtable[key] = (XsltTransformer)transformer;
    }

    public void AddSource(string name, Uri uri) {
      string sourceUriHash = uri.GetHashCode().ToString();
      string key = name + ":" + sourceUriHash;
      Stream xmlStream = (Stream)_resolver.GetEntity(uri, null, typeof(Stream));
      _sourceHashtable[key] = (Stream)xmlStream;
      if (_builder.BaseUri == null) {
        _builder.BaseUri = uri;
      }
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

    public Hashtable GetHashtable() {
      return _xsltHashtable;
    }
    public Hashtable GetXmlSourceHashtable() {
      return _sourceHashtable;
    }
    public Processor GetProcessor() {
      return _processor;
    }
    public DocumentBuilder GetDocumentBuilder() {
      return _builder;
    }
    public XmlUrlResolver GetResolver() {
      return _resolver;
    }
    public XsltCompiler GetCompiler() {
      return _compiler;
    }
  }
}
