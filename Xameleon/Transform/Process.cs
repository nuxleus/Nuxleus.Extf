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

    public void Process(Context context) {

      using (context.Writer) {

        using (context.TemplateStream) {

          using (context.XmlStream) {

            XsltTransformer transformer = context.XsltExecutable.Load();

            if (context.XsltParams.Count > 0) {
              IEnumerator enumerator = context.XsltParams.GetEnumerator();
              for (int i = 0; enumerator.MoveNext(); i++) {
                string local = context.XsltParams.AllKeys[i];
                transformer.SetParameter(new QName("", "", local), new XdmAtomicValue(context.XsltParams[local]));
              }
            }

            if (context.XsltObjectParams.Count > 0) {
              foreach (DictionaryEntry param in context.XsltObjectParams) {
                string name = (string)param.Key;
                transformer.SetParameter(new QName("", "", name), new XdmValue((XdmItem)XdmAtomicValue.wrapExternalObject(context.XsltObjectParams[name])));
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
    }
  }
}