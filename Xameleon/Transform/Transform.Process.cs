using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Saxon.Api;
using System.IO;
using System.Web;
using System.Collections;
using System.Collections.Specialized;
using Xameleon.ResultDocumentHandler;

namespace Xameleon {

    public partial class Transform {

        private bool PrepareTransform(Context context) {
            this._SourceXml = (Stream)context.Resolver.GetEntity(context.XmlSource, null, typeof(Stream));
            this._TemplateStream = (Stream)context.Resolver.GetEntity(context.XsltSource, null, typeof(Stream));
            this._Processor = new Processor();
            this._Compiler = this._Processor.NewXsltCompiler();
            this._Compiler.ErrorList = new ArrayList();
            this._Template = this._Compiler.Compile(this._TemplateStream);
            return true;
        }

        internal void Process(HttpContext context, bool outputS3) {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            TextWriter writer = context.Response.Output;

            Uri absoluteUri = new Uri(context.Server.MapPath(request.RawUrl));
            if (!this._IS_INITIALIZED) {
                this.Init(context);
            }

            using (writer){
                using (Stream input = ((Stream)this._Resolver.GetEntity(absoluteUri, null, typeof(Stream)))) {
                    using (Stream transform = this._TemplateStream) {
                        DocumentBuilder builder = this._Processor.NewDocumentBuilder();
                        builder.BaseUri = absoluteUri;
                        XdmNode node = builder.Build(input);
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

                        Hashtable results = new Hashtable(); ;
                        if (outputS3) {
                            transformer.ResultDocumentHandler = new S3ResultDocumentHandler(results);
                        }
                        transformer.SetParameter(new QName("", "", "response"), new XdmValue((XdmItem)XdmAtomicValue.wrapExternalObject(response)));
                        transformer.SetParameter(new QName("", "", "request"), new XdmValue((XdmItem)XdmAtomicValue.wrapExternalObject(request)));
                        transformer.SetParameter(new QName("", "", "server"), new XdmValue((XdmItem)XdmAtomicValue.wrapExternalObject(context.Server)));
                        transformer.SetParameter(new QName("", "", "session"), new XdmValue((XdmItem)XdmAtomicValue.wrapExternalObject(context.Session)));
                        transformer.SetParameter(new QName("", "", "timestamp"), new XdmValue((XdmItem)XdmAtomicValue.wrapExternalObject(context.Timestamp)));
                        transformer.InputXmlResolver = this._Resolver;
                        transformer.InitialContextNode = node;
                        transformer.Run(destination);

                        if (outputS3) {
                            foreach (DictionaryEntry entry in results) {
                                string uri = (string)entry.Key;
                                DomDestination dom = (DomDestination)results[uri];
                                //TODO: Output results to S3
                            }
                        }
                    }
                }
            }
        }

        internal XmlDocument Process(Context context) {
            XmlDocument xmlDocument;
            using (Stream stream = this._SourceXml) {
                using (Stream transform = this._TemplateStream) {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(this._SourceXml);
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