using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Saxon.Api;

namespace Saxon.Ext {

  public class Test {

    private static string filter = "http://xslt.googlecode.com/svn//trunk/Modules/DataFilter/init.xml";
    private static string xslt = "http://xslt.googlecode.com/svn/test/extensionfunction.xsl";
  
    public Test() {}

    public XmlDocument Process() {
        Processor processor = new Processor();
        XmlDocument doc = new XmlDocument();
        doc.Load(new XmlTextReader(filter));
        XdmNode input = processor.NewDocumentBuilder().Wrap(doc);
        XsltCompiler compiler = processor.NewXsltCompiler();
        XsltTransformer transformer = compiler.Compile(new Uri(xslt)).Load();
        transformer.InitialContextNode = input;
        DomDestination result = new DomDestination();
        transformer.Run(result);
        return result.XmlDocument;
    }
  }
}

