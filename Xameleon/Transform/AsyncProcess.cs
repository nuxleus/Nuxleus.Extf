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

      using (context.XmlStream) {

        XsltTransformer transformer = context.XsltCompiledCache.GetTransformer(context.BaseXsltUriHash, context.BaseXsltUri);

        if (context.XsltParams.Count > 0) {
          foreach (DictionaryEntry param in context.XsltParams) {
            string name = (string)param.Key;
            transformer.SetParameter(new QName("", "", name), new XdmValue((XdmItem)XdmAtomicValue.wrapExternalObject(param.Value)));
          }
        }

        transformer.InputXmlResolver = context.Resolver;
        transformer.InitialContextNode = context.Node;

        lock (transformer) {
          transformer.Run(context.Destination);
        }
      }
    }

    public void EndAysncProcess(IAsyncResult result) {

    }
  }
}

