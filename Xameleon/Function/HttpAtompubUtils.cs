using System;
using System.Web;
using System.Xml;
using net.sf.saxon.value;
using Saxon.Api;

namespace Xameleon.Function {
  public class HttpAtompubUtils {

      public HttpAtompubUtils() { }

    public static string GenerateEntry(HttpContext context) {
      return "<b>hey there</b>";
    }
  }
}
