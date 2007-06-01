using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Saxon.Api;
using System.IO;
using System.Web;
using System.Collections;

namespace Xameleon {

    public partial class Transform {

        private bool PrepareTransform(Context context, bool usePI) {
            try {
                this._SourceXml = (Stream)context.Resolver.GetEntity(context.XmlSource, null, typeof(Stream));
                this._TemplateStream = (Stream)context.Resolver.GetEntity(context.XsltSource, null, typeof(Stream));
                this._Processor = new Processor();
                this._Builder = this._Processor.NewDocumentBuilder();
                this._Compiler = this._Processor.NewXsltCompiler();
                this._Compiler.ErrorList = new ArrayList();
                this._Node = _Builder.Build(_SourceXml);
                this._usePI = usePI;
                if (this._usePI) {
                    this._Template = this._Compiler.CompileAssociatedStylesheet(_Node);
                } else {
                    this._Template = this._Compiler.Compile(this._TemplateStream);
                }
                return true;
            } catch (Exception e) {
                return false;
            }
        }

        internal void Process(HttpContext context, bool usePI) {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            TextWriter writer = context.Response.Output;

            Uri absoluteUri = new Uri(context.Server.MapPath(request.FilePath));
            if (!this._IS_INITIALIZED) {
                this.Init(context, usePI);
            }
            using (Stream xmlStream = this._SourceXml) {
                using (Stream xslStream = this._TemplateStream) {
                    Serializer destination = new Serializer();
                    destination.SetOutputWriter(writer);
                    XsltTransformer transformer = this._Template.Load();
                    if (this._XsltParams.Count > 0) {
                        IEnumerator enumerator = this._XsltParams.GetEnumerator();
                        for (int i = 0; enumerator.MoveNext(); i++) {
                            string local = this._XsltParams.AllKeys[i].ToString();
                            transformer.SetParameter(new QName("", "", local), new XdmAtomicValue(this._XsltParams[local]));
                        }
                    }
                    // TODO: Make this a bit more elegant/reusable.
                    if (request.QueryString.Count > 0) {
                        IEnumerator enumerator = request.QueryString.GetEnumerator();
                        for (int i = 0; enumerator.MoveNext(); i++) {
                            string local = request.QueryString.AllKeys[i].ToString();
                            transformer.SetParameter(new QName("", "", "qs_" + local), new XdmAtomicValue(request.QueryString[local]));
                        }
                    }
                    // TODO: Ditto.
                    if (request.Form.Count > 0) {
                        IEnumerator enumerator = request.Form.GetEnumerator();
                        for (int i = 0; enumerator.MoveNext(); i++) {
                            string local = request.Form.AllKeys[i].ToString();
                            transformer.SetParameter(new QName("", "", "form_" + local), new XdmAtomicValue(request.Form[local]));
                        }
                    }
                    // TODO: Ditto.
                    if (request.Cookies.Count > 0) {
                        IEnumerator enumerator = request.Cookies.GetEnumerator();
                        for (int i = 0; enumerator.MoveNext(); i++) {
                            string local = request.Cookies.AllKeys[i].ToString();
                            transformer.SetParameter(new QName("", "", "cookie_" + local), new XdmAtomicValue(request.Cookies[local].Value));
                        }
                    }
                    // temporary hack
                    transformer.SetParameter(new QName("", "", "request.ip"), new XdmAtomicValue(request.UserHostAddress));
                    //transformer.SetParameter(new QName("", "", "response"), new XdmValue((XdmItem)response.Headers.GetEnumerator()));
                    // end temporary hack
                    transformer.InputXmlResolver = this._Resolver;
                    transformer.InitialContextNode = this._Node;
                    transformer.Run(destination);
                }
            }
        }

        private XmlDocument Process(Context context) {
            XmlDocument xmlDocument;
            using (Stream xmlStream = this._SourceXml) {
                using (Stream xslStream = this._TemplateStream) {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(xmlStream);
                    XdmNode node = this._Processor.NewDocumentBuilder().Wrap(doc);
                    XsltTransformer transformer = this._Template.Load();
                    transformer.InitialContextNode = node;
                    DomDestination destination = new DomDestination();
                    transformer.Run(destination);
                    xmlDocument = destination.XmlDocument;
                }
            }
            return xmlDocument;
        }
    }
}