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
using System.Collections.Generic;

namespace Xameleon.HttpApplication {


  public class Global : System.Web.HttpApplication {

    MemcachedClient _MemcachedClient = null;
    SockIOPool _pool = null;
    AppSettings _AppSettings = new AppSettings();
    AspNetXameleonConfiguration _XameleonConfiguration = AspNetXameleonConfiguration.GetConfig();
    AspNetAwsConfiguration _AwsConfiguration = AspNetAwsConfiguration.GetConfig();
    AspNetBungeeAppConfiguration _BungeeAppConfguration = AspNetBungeeAppConfiguration.GetConfig();
    AspNetMemcachedConfiguration _MemcachedConfiguration = AspNetMemcachedConfiguration.GetConfig();
    XsltCompiledHashtable _XsltCompiledHashtable = new XsltCompiledHashtable();

    protected void Application_Start(object sender, EventArgs e) {
      string useMemcached = _XameleonConfiguration.UseMemcached;
      if (useMemcached != null && useMemcached == "yes") {
        _MemcachedClient = new MemcachedClient();
        Application["memcached"] = _MemcachedClient;
        Application["usememcached"] = true;

        if (_MemcachedConfiguration.UseCompression != null && _MemcachedConfiguration.UseCompression == "yes")
          _MemcachedClient.EnableCompression = true;
        else
          _MemcachedClient.EnableCompression = false;

        List<string> serverList = new List<string>();

        foreach (MemcachedServer server in _MemcachedConfiguration.MemcachedServerCollection) {
          serverList.Add(server.IP + ":" + server.Port);
        }

        _pool = SockIOPool.GetInstance();

        _pool.SetServers(serverList.ToArray());

        MemcachedPoolConfigCollection tempPoolConfig = _MemcachedConfiguration.PoolConfig;

        Hashtable poolConfig = new Hashtable();

        foreach(MemcachedPoolConfig item in tempPoolConfig)
          poolConfig[item.Property] = item.Value;

        if (poolConfig["initConnections"] != null)
          _pool.InitConnections = (int)poolConfig["initConnections"];
        else
          _pool.InitConnections = 3;

        if (poolConfig["minConnections"] != null)
          _pool.MinConnections = (int)poolConfig["minConnections"];
        else
          _pool.MinConnections = 3;

        if (poolConfig["maxConnections"] != null)
          _pool.MaxConnections = (int)poolConfig["maxConnections"];
        else
          _pool.MaxConnections = 5;

        if (poolConfig["socketConnectTimeout"] != null)
          _pool.SocketConnectTimeout = (int)poolConfig["socketConnectTimeout"];
        else
          _pool.SocketConnectTimeout = 1000;

        if (poolConfig["socketTimeout"] != null)
          _pool.SocketTimeout = (int)poolConfig["socketTimeout"];
        else
          _pool.SocketTimeout = 3000;

        if (poolConfig["maintenanceSleep"] != null)
          _pool.MaintenanceSleep = (int)poolConfig["maintenanceSleep"];
        else
          _pool.MaintenanceSleep = 30;

        if (poolConfig["failover"] != null && (int)poolConfig["failover"] == 0)
          _pool.Failover = false;
        else
          _pool.Failover = true;

        if (poolConfig["nagle"] != null && (int)poolConfig["nagle"] == 1)
          _pool.Nagle = true;
        else
          _pool.Nagle = false;

        _pool.Initialize();

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
      SockIOPool.GetInstance().Shutdown();
    }
  }
}
