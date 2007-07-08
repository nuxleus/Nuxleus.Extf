using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Xameleon.Configuration {

  public class MemcachedPoolConfig : ConfigurationElement {

    [ConfigurationProperty("property", IsRequired = true)]
    public string Property {
      get {
        return this["key"] as string;
      }
    }

    [ConfigurationProperty("value", IsRequired = true)]
    public string Value {
      get {
        return this["value"] as string;
      }
    }
  }
}
