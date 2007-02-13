using System;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using Extf.Net.Configuration;
using Saxon.Api;

namespace Saxon.AspNet {

    public partial class Transform {

        public void Process (HttpContext context, TextWriter writer) {

            HttpRequest Request = context.Request;
            HttpResponse Response = context.Response;
            String sourceUri = context.Server.MapPath(Request.RawUrl);
            Uri sUri = new Uri(sourceUri);

            if (!this._IS_INITIALIZED) Init(context);

            using (Stream sXml = (Stream)this._Resolver.GetEntity(sUri, null, typeof(Stream))) {

                using (_TemplateStream) {
                    Processor processor = new Processor();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(new XmlTextReader(sXml));
                    XdmNode input = processor.NewDocumentBuilder().Wrap(doc);

                    Serializer mySerializer = new Serializer();
                    mySerializer.SetOutputWriter(writer);

                    XsltTransformer myTransformer = this._Template.Load();
                    myTransformer.InputXmlResolver = this._Resolver;
                    myTransformer.InitialContextNode = input;
                    myTransformer.Run(mySerializer);

                }
            }

        }
    }
}
