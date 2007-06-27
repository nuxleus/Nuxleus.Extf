using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Xml;
using Saxon.Api;
using Xameleon.ResultDocumentHandler;

namespace Xameleon {

    ///<summary>
    ///</summary>
    public partial class Transform {

        private bool PrepareTransform(Context context) {
            _SourceXml = (Stream)context.Resolver.GetEntity(context.XmlSource, null, typeof(Stream));
            _TemplateStream = (Stream)context.Resolver.GetEntity(context.XsltSource, null, typeof(Stream));
            _Processor = new Processor();
            _Compiler = _Processor.NewXsltCompiler();
            if (_Compiler != null) {
                _Compiler.ErrorList = new ArrayList();
                _Template = _Compiler.Compile(_TemplateStream);
            }
            return true;
        }

        internal void Process(HttpContext context, TextWriter writer, bool outputS3) {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            Uri absoluteUri = new Uri(request.MapPath(request.CurrentExecutionFilePath));
            if (!_IS_INITIALIZED) {
                Init(context);
            }

            using (writer){
                using (Stream input = ((Stream)_Resolver.GetEntity(absoluteUri, null, typeof(Stream)))) {
                    if (_TemplateStream != null)
                        using (_TemplateStream)
                        {
                            DocumentBuilder builder = _Processor.NewDocumentBuilder();
                            builder.BaseUri = absoluteUri;
                            XdmNode node = builder.Build(input);
                            Serializer destination = new Serializer();
                            destination.SetOutputWriter(writer);
                            XsltTransformer transformer = _Template.Load();
                            if (_XsltParams.Count > 0) {
                                IEnumerator enumerator = _XsltParams.GetEnumerator();
                                for (int i = 0; enumerator.MoveNext(); i++) {
                                    string local = _XsltParams.AllKeys[i];
                                    transformer.SetParameter(new QName("", "", local), new XdmAtomicValue(_XsltParams[local]));
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

                            lock (transformer) {
                                transformer.Run(destination);
                            }

                            if (outputS3) {
                                //foreach (DictionaryEntry entry in results) {
                                //    //string uri = (string)entry.Key;
                                //    //DomDestination dom = (DomDestination)results[uri];
                                //    //TODO: Output results to S3
                                //}
                            }
                        }
                }
            }
        }

        internal XmlDocument Process(Context context) {
            XmlDocument xmlDocument = null;
            if (_SourceXml != null)
                using (_SourceXml)
                {
                    if (_TemplateStream != null)
                        using (_TemplateStream)
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(_SourceXml);
                            XdmNode node = _Processor.NewDocumentBuilder().Wrap(doc);
                            XsltTransformer transformer = _Template.Load();
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