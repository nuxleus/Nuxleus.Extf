using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Xameleon.Transform;
using Xameleon.Configuration;
using Memcached.ClientLibrary;

namespace Xameleon.HttpApplication {


  public class Global : System.Web.HttpApplication {

    MemcachedClient _MemcachedClient = null;
    AppSettings _AppSettings = new AppSettings();
    XsltCompiledHashtable _XsltCompiledHashtable = new XsltCompiledHashtable();

    protected void Application_Start(object sender, EventArgs e) {
      string useMemcached = _AppSettings.GetSetting("useMemcached");
      if (useMemcached != null && useMemcached == "yes") {
        _MemcachedClient = new MemcachedClient();
        Application["memcached"] = _MemcachedClient;
        Application["usememcached"] = true;
      } else
        Application["usememcached"] = false;
      Application["xsltCompiledHashtable"] = _XsltCompiledHashtable;
      Application["appSettings"] = _AppSettings;
    }

    protected void Session_Start(object sender, EventArgs e) {

    }

    protected void Application_BeginRequest(object sender, EventArgs e) {

    }

    protected void Application_AuthenticateRequest(object sender, EventArgs e) {

    }

    protected void Application_Error(object sender, EventArgs e) {

    }

    protected void Session_End(object sender, EventArgs e) {

    }

    protected void Application_End(object sender, EventArgs e) {

    }
  }
}
