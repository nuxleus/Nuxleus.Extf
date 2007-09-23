using System;
using System.Web;
using System.Xml;

namespace Xameleon.Function {
  public class HttpAtompubUtils {

      public HttpAtompubUtils() { }

    public static XmlNode GenerateEntry(HttpContext context) {
      XmlDocument doc = new XmlDocument();
      doc.Load("<b>hey there</b>");
      
      return doc.DocumentElement;
    }
  }
}
