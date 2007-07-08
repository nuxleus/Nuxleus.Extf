using System;
using System.Configuration;

namespace Xameleon.Configuration {

  public class AspNetMemcachedConfiguration : ConfigurationSection {


    public static AspNetMemcachedConfiguration GetConfig() {
      return ConfigurationManager.GetSection("aws") as AspNetMemcachedConfiguration;
    }

    [ConfigurationProperty("server", IsRequired = true)]
    public MemcachedServerCollection AwsKeyCollection {
      get {
        return this["server"] as MemcachedServerCollection;
      }
    }

    [ConfigurationProperty("poolConfig", IsRequired = true)]
    public MemcachedPoolConfigCollection PoolConfig {
      get {
        return this["poolConfig"] as MemcachedPoolConfigCollection;
      }
    }
  }
}
