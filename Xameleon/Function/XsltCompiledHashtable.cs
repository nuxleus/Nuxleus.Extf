using System;
using System.Collections;
using Saxon.Api;
using System.Xml;

namespace Xameleon.Transform {

  public class XsltCompiledHashtable {

    private Hashtable results;

    public XsltCompiledHashtable() {
      this.results = new Hashtable();
    }
    public XsltCompiledHashtable(Hashtable table) {
      this.results = table;
    }

    public XsltTransformer GetTransformer(string name, string href, Uri baseUri, Processor processor) {
      Uri xsltUri = new Uri(baseUri, new Uri(href, UriKind.Relative));
      string xsltUriHash = xsltUri.GetHashCode().ToString();
      string key = name + ":" + xsltUriHash;

      foreach (DictionaryEntry entry in results) {
        string uri = (string)entry.Key;
        if (uri == key) {
          return (XsltTransformer)results[uri];
        }
      }

      XsltTransformer transformer = processor.NewXsltCompiler().Compile(xsltUri).Load();
      results[key] = transformer;
      return transformer;
    }

    public Hashtable GetHashtable() {
      return this.results;
    }
  }
}
