using System;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using Extf.Net.Configuration;

namespace Xameleon {

  public partial class Transform {

    public class Context {

      Uri _BaseUri;
      Uri _Xml;
      Uri _Xslt;
      XmlDocument _Doc;
      XmlUrlResolver _Resolver;
      string _Backup = @"
      <system>
        <message>
          Something very very bad has happened. Run while you still can!
        </message>
      </system>";

      public Context () { }

      public Uri BaseUri {
        get { return _BaseUri; }
        set { _BaseUri = value; }
      }
      public Uri XmlSource {
        get { return _Xml; }
        set { _Xml = value; }
      }
      public Uri XsltSource {
        get { return _Xslt; }
        set { _Xslt = value; }
      }
      public XmlDocument ResultDocument {
        get { return _Doc; }
        set { _Doc = value; }
      }
      public XmlUrlResolver Resolver {
        get { return _Resolver; }
        set { _Resolver = value; }
      }
    }
  }
}
