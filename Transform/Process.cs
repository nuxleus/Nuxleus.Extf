using System;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using Saxon.Api;
using System.Collections;

namespace Xameleon
{

    public partial class Transform
    {

        public void Process(HttpContext context, TextWriter writer)
        {

            HttpRequest Request = context.Request;
            HttpResponse Response = context.Response;
            String sourceUri = context.Server.MapPath(Request.RawUrl);
            Uri sUri = new Uri(sourceUri);

            if (!this._IS_INITIALIZED) Init(context);

            using (Stream sXml = (Stream)this._Resolver.GetEntity(sUri, null, typeof(Stream)))
            {

                using (_TemplateStream)
                {
                    DocumentBuilder builder = _Processor.NewDocumentBuilder();
                    builder.BaseUri = sUri;
                    XdmNode input = builder.Build(sXml);

                    Serializer mySerializer = new Serializer();
                    mySerializer.SetOutputWriter(writer);

                    XsltTransformer myTransformer = this._Template.Load();

                    if (this._XsltParams.Count > 0)
                    {
                        IEnumerator xsltParamsEnum = this._XsltParams.GetEnumerator();

                        int i = 0;
                        while (xsltParamsEnum.MoveNext())
                        {
                            string key = this._XsltParams.AllKeys[i].ToString();
                            myTransformer.SetParameter(
                                new QName("", "", key),
                                new XdmAtomicValue(this._XsltParams[key])
                                );
                            i += 1;
                        }
                    }
                    myTransformer.InputXmlResolver = this._Resolver;
                    myTransformer.InitialContextNode = input;
                    myTransformer.Run(mySerializer);
                }
            }

        }
    }
}