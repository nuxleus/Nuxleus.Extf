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

        MemcachedPoolConfigCollection poolConfig = _MemcachedConfiguration.PoolConfig;

        if (poolConfig.InitConnections.Value != null)
          _pool.InitConnections = (int)poolConfig.InitConnections.Value;
        else
          _pool.InitConnections = 3;

        if (poolConfig.MinConnections.Value != null)
          _pool.MinConnections = (int)poolConfig.MinConnections.Value;
        else
          _pool.MinConnections = 3;

        if (poolConfig.MaxConnections.Value != null)
          _pool.MaxConnections = (int)poolConfig.MaxConnections.Value;
        else
          _pool.MaxConnections = 5;

        if (poolConfig.SocketConnectTimeout.Value != null)
          _pool.SocketConnectTimeout = (int)poolConfig.SocketConnectTimeout.Value;
        else
          _pool.SocketConnectTimeout = 1000;

        if (poolConfig.SocketConnect.Value != null)
          _pool.SocketTimeout = (int)poolConfig.SocketConnect.Value;
        else
          _pool.SocketTimeout = 3000;

        if (poolConfig.MaintenanceSleep.Value != null)
          _pool.MaintenanceSleep = (int)poolConfig.MaintenanceSleep.Value;
        else
          _pool.MaintenanceSleep = 30;

        if (poolConfig.Failover.BoolValue != null && (bool)poolConfig.Failover.BoolValue == false)
          _pool.Failover = false;
        else
          _pool.Failover = true;

        if (poolConfig.Nagle.BoolValue != null && (bool)poolConfig.Nagle.BoolValue == true)
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
