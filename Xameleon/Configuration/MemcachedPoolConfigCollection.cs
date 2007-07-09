using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Xameleon.Configuration {

  public class MemcachedPoolConfigCollection : ConfigurationElementCollection {

    public MemcachedPoolConfig this[int index] {
      get {
        return base.BaseGet(index) as MemcachedPoolConfig;
      }
      set {
        if (base.BaseGet(index) != null) {
          base.BaseRemoveAt(index);
        }
        this.BaseAdd(index, value);
      }
    }

    protected override ConfigurationElement CreateNewElement() {
      return new MemcachedPoolConfig();
    }

    protected override object GetElementKey(ConfigurationElement element) {
      return ((MemcachedPoolConfig)element).Name;
    }

    [ConfigurationProperty("initConnections", IsRequired = false)]
    public MemcachedPoolConfig InitConnections {
      get {
        return this["initConnections"] as MemcachedPoolConfig;
      }
    }

    [ConfigurationProperty("minConnections", IsRequired = false)]
    public MemcachedPoolConfig MinConnections {
      get {
        return this["minConnections"] as MemcachedPoolConfig;
      }
    }

    [ConfigurationProperty("maxConnections", IsRequired = false)]
    public MemcachedPoolConfig MaxConnections {
      get {
        return this["maxConnections"] as MemcachedPoolConfig;
      }
    }

    [ConfigurationProperty("socketConnectTimeout", IsRequired = false)]
    public MemcachedPoolConfig SocketConnectTimeout {
      get {
        return this["socketConnectTimeout"] as MemcachedPoolConfig;
      }
    }

    [ConfigurationProperty("socketConnect", IsRequired = false)]
    public MemcachedPoolConfig SocketConnect {
      get {
        return this["socketConnect"] as MemcachedPoolConfig;
      }
    }

    [ConfigurationProperty("maintenanceSleep", IsRequired = false)]
    public MemcachedPoolConfig MaintenanceSleep {
      get {
        return this["maintenanceSleep"] as MemcachedPoolConfig;
      }
    }

    [ConfigurationProperty("failover", IsRequired = false)]
    public MemcachedPoolConfig Failover {
      get {
        return this["initConnections"] as MemcachedPoolConfig;
      }
    }

    [ConfigurationProperty("nagle", IsRequired = false)]
    public MemcachedPoolConfig Nagle {
      get {
        return this["nagle"] as MemcachedPoolConfig;
      }
    }
  }
}
