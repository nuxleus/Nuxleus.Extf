using System;
using System.Collections;
using Saxon.Api;
using System.Xml;

namespace Xameleon.Transform {

  public class XsltCompiledHashtable {

    Hashtable _results;
    Processor _processor;

    public XsltCompiledHashtable() {
      _results = new Hashtable();
      _processor = new Processor();
    }
    public XsltCompiledHashtable(Hashtable table) {
      _results = table;
      _processor = new Processor();
    }

    public void AddTransformer(string name, Uri uri, XmlUrlResolver resolver) {
      string xsltUriHash = uri.GetHashCode().ToString();
      string key = name + ":" + xsltUriHash;
      XsltCompiler compiler = _processor.NewXsltCompiler();
      compiler.BaseUri = uri;
      compiler.XmlResolver = resolver;
      
      XsltTransformer transformer = compiler.Compile(uri).Load();
      _results[key] = (XsltTransformer)transformer;
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
      foreach (DictionaryEntry entry in _results) {
        string uri = (string)entry.Key;
        if (uri == key) {
          return (XsltTransformer)_results[uri];
        }
      }

      XsltTransformer transformer = _processor.NewXsltCompiler().Compile(xsltUri).Load();
      _results[key] = (XsltTransformer)transformer;
      return transformer;
    }

    public Hashtable GetHashtable() {
      return this._results;
    }

    public Processor GetProcessor() {
      return this._processor;
    }
  }
}
