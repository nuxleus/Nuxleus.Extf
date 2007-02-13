using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Saxon.Api;

namespace Saxon.Ext {
  public partial class Transform {
    private static XmlDocument Process (XmlReader funcSeq, XmlReader extTransform, Uri baseUri) {
      Processor processor = new Processor();
      XdmNode input = processor.NewDocumentBuilder().Build(funcSeq);
      XsltCompiler compiler = processor.NewXsltCompiler();
      compiler.BaseUri = baseUri;
      XsltTransformer transformer = compiler.Compile(extTransform).Load();
      transformer.InitialContextNode = input;
      DomDestination result = new DomDestination();
      transformer.Run(result);
      return result.XmlDocument;
    }
  }
}
