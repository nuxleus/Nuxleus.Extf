using System;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using Extf.Net.Configuration;

namespace Xameleon {

    public partial class Transform {

        internal class Context {

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

            internal Context() { }

            internal Uri BaseUri {
                get { return _BaseUri; }
                set { _BaseUri = value; }
            }
            internal Uri XmlSource {
                get { return _Xml; }
                set { _Xml = value; }
            }
            internal Uri XsltSource {
                get { return _Xslt; }
                set { _Xslt = value; }
            }
            internal XmlDocument ResultDocument {
                get { return _Doc; }
                set { _Doc = value; }
            }
            internal XmlUrlResolver Resolver {
                get { return _Resolver; }
                set { _Resolver = value; }
            }
        }
    }
}
