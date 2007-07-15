using System;
using System.Xml;
using System.IO;
using Saxon.Api;
using System.Collections.Specialized;
using System.Web;
using Xameleon.Configuration;
using System.Net;
using Xameleon.Properties;
using System.Collections;
using Memcached.ClientLibrary;
using System.Text;

namespace Xameleon.Transform {

  public struct Context : IDisposable {

    String _RequestUriHash;
    Hashtable _XsltParams;

    public Context(HttpContext context, Hashtable xsltParams, params string[] httpContextParamList) {
      _RequestUriHash = context.Request.Url.GetHashCode().ToString();
      _XsltParams = xsltParams;
    }
    public String RequestUriHash {
      get { return _RequestUriHash; }
      set { _RequestUriHash = value; }
    }
    public Hashtable XsltParams {
      get { return _XsltParams; }
      set { _XsltParams = value; }
    }
    #region IDisposable Members

    public void Dispose() {
      _RequestUriHash = null;
      _XsltParams.Clear();
    }

    #endregion
  }
}
