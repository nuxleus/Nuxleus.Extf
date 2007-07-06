using System;
using System.Xml;
using System.IO;
using Saxon.Api;
using System.Collections.Specialized;

namespace Xameleon {

    public partial class Transform {

        public struct Context {
            Uri _BaseUri;
            Uri _Xml;
            Uri _Xslt;
            XmlDocument _Doc;
            XmlUrlResolver _Resolver;
            Processor _Processor;
            XsltCompiler _Compiler;
            Stream _SourceXml;
            XsltExecutable _Template;
            Stream _TemplateStream;
            String _xsltParamKey;
            NameValueCollection _XsltParams;
            String _Backup;

            public Context(String paramKey) {
                _BaseUri = null;
                _Xml = null;
                _Xslt = null;
                _Doc = null;
                _Resolver = null;
                _Processor = null;
                _Compiler = null;
                _SourceXml = null;
                _Template = null;
                _TemplateStream = null;
                _XsltParams = null;
                _xsltParamKey = "xsltParam_";
                _Backup = @"<system>
                            <message>
                              Something very very bad has happened. Run while you still can!
                            </message>
                           </system>";
            }

            public XsltCompiler Compiler {
                get { return _Compiler; }
                set { _Compiler = value; }
            }
            public Processor Processor {
                get { return _Processor; }
                set { _Processor = value; }
            }
            public Stream TemplateStream {
                get { return _TemplateStream; }
                set { _TemplateStream = value; }
            }
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
