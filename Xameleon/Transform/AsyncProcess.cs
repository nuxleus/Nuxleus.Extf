using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Xml;
using Saxon.Api;
using Xameleon.ResultDocumentHandler;

namespace Xameleon.Transform {

  ///<summary>
  ///</summary>
  public partial class Transform {

    public void BeginAsyncProcess(Context context, AsyncCallback cb, TransformServiceAsyncResult result) {

      using (context.Writer) {

        using (context.TemplateStream) {

          using (context.XmlStream) {

            XsltTransformer transformer = context.XsltExecutable.Load();

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
      }
      string output = context.StringBuilder.ToString();
      context.MemcachedClient.Set(context.XmlSource.GetHashCode().ToString(), output);
      context.ResponseOutput.Write(output);
      result.CompleteCall();
    }

    public void EndAysncProcess(IAsyncResult result) {
 
    }
  }
}

