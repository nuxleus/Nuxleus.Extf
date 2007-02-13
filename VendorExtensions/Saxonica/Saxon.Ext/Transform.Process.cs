using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Saxon.Api;
using System.IO;

namespace Saxon.Ext {

  public partial class Transform {

    private XmlDocument Process (Context context) {

      using (this._SourceXml) {

        using (this._TemplateStream) {

          XmlDocument doc = new XmlDocument();
          doc.Load(this._SourceXml);
          XdmNode input = this._Processor.NewDocumentBuilder().Wrap(doc);

          XsltTransformer transformer = this._Template.Load();
          transformer.InitialContextNode = input;

          DomDestination result = new DomDestination();
          transformer.Run(result);
          return result.XmlDocument;

        }
      }
    }
  }
}