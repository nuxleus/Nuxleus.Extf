using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Xameleon.Configuration {

  public class MemcachedPoolConfig : ConfigurationElement {

    [ConfigurationProperty("name", IsRequired = false)]
    public string Name {
      get {
        return this["name"] as string;
      }
    }

    [ConfigurationProperty("value", IsRequired = false)]
    public int Value {
      get {
        return (int)this["value"];
      }
    }

    [ConfigurationProperty("boolValue", IsRequired = false)]
    public bool BoolValue {
      get {
        return (bool)this["boolValue"];
      }
    }
  }
}
