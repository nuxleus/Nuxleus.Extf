using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Xml;
using Saxon.Api;
using Xameleon.ResultDocumentHandler;
using System.Text;
using System.Web.SessionState;

namespace Xameleon.Transform {

  ///<summary>
  ///</summary>
  public partial class Transform {

    public void BeginAsyncProcess(Context context) {

      using (context.TemplateStream) {

        using (context.XmlStream) {

          XsltTransformer transformer = context.XsltExecutable.Load();

          if (context.XsltParams.Count > 0) {
            foreach (DictionaryEntry param in context.XsltParams) {
              string name = (string)param.Key;
              transformer.SetParameter(new QName("", "", name), new XdmValue((XdmItem)XdmAtomicValue.wrapExternalObject(param.Value)));
            }
          }
          transformer.SetParameter(new QName("", "", "request"), new XdmValue((XdmItem)XdmAtomicValue.wrapExternalObject((HttpRequest)context.HttpContext.Request)));
          transformer.SetParameter(new QName("", "", "response"), new XdmValue((XdmItem)XdmAtomicValue.wrapExternalObject((HttpResponse)context.HttpContext.Response)));
          transformer.SetParameter(new QName("", "", "timestamp"), new XdmValue((XdmItem)XdmAtomicValue.wrapExternalObject((DateTime)context.HttpContext.Timestamp)));
          transformer.SetParameter(new QName("", "", "session"), new XdmValue((XdmItem)XdmAtomicValue.wrapExternalObject((HttpSessionState)context.HttpContext.Session)));
          transformer.SetParameter(new QName("", "", "server"), new XdmValue((XdmItem)XdmAtomicValue.wrapExternalObject((HttpServerUtility)context.HttpContext.Server)));

          transformer.InputXmlResolver = context.Resolver;
          transformer.InitialContextNode = context.Node;

          lock (transformer) {
            transformer.Run(context.Destination);
          }
        }
      }
    }

    public void EndAysncProcess(IAsyncResult result) {

    }
  }
}

