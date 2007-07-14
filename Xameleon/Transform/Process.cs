using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Xml;
using Saxon.Api;
using Xameleon.ResultDocumentHandler;
using System.Text;

namespace Xameleon.Transform {

  ///<summary>
  ///</summary>
  public partial class Transform {

    public void Process(Context context) {

      StringBuilder _builder = new StringBuilder();
      TextWriter _writer = new StringWriter(_builder);

      using (_writer) {

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
    }
  }
}