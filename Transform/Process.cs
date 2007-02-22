// Copyright (c) 2006 by M. David Peterson
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.

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