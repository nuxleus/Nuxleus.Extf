using System;
using System.Web;
using System.Xml;
using net.sf.saxon.value;
using Saxon.Api;

namespace Xameleon.Function {
  public class HttpAtompubUtils {

      public HttpAtompubUtils() { }

    public static XdmNode GenerateEntry(HttpContext context) {
      XmlDocument doc = new XmlDocument();
      doc.Load("<b>hey there</b>");
      
      Processor processor = new Processor();
      return processor.NewDocumentBuilder().Build(doc.DocumentElement);
    }
  }
}
