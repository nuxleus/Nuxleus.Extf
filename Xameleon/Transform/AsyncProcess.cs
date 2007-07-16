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

    public void BeginAsyncProcess(Context context, XslTransformationManager manager, TextWriter writer) {
      BeginAsyncProcess(context, manager, writer, manager.BaseXsltUriHash);
    }

    public void BeginAsyncProcess(Context context, XslTransformationManager manager, TextWriter writer, String xsltName) {

      XsltTransformer transformer = manager.GetTransformer(xsltName);

      if (context.XsltParams.Count > 0) {
        foreach (DictionaryEntry param in context.XsltParams) {
          string name = (string)param.Key;
          transformer.SetParameter(new QName("", "", name), new XdmValue((XdmItem)XdmAtomicValue.wrapExternalObject(param.Value)));
        }
      }

      Uri requestXmlUri = new Uri(HttpContext.Current.Request.MapPath(HttpContext.Current.Request.CurrentExecutionFilePath));

      transformer.InputXmlResolver = manager.Resolver;
      transformer.InitialContextNode = manager.GetXdmNode(requestXmlUri.GetHashCode().ToString(), requestXmlUri);

      Serializer destination = manager.Serializer;
      destination.SetOutputWriter(writer);

      lock (transformer) {
        transformer.Run(destination);
      }
    }

    public void EndAysncProcess(IAsyncResult result) {

    }
  }
}

