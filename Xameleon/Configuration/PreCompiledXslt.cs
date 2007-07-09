﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Xameleon.Configuration {

  public class PreCompiledXslt : ConfigurationElement {

    [ConfigurationProperty("name", IsRequired = true)]
    public string Name {
      get {
        return this["name"] as string;
      }
    }

    [ConfigurationProperty("base-uri", IsRequired = false)]
    public string BaseUri {
      get {
        return this["base-uri"] as string;
      }
    }

    [ConfigurationProperty("uri", IsRequired = true)]
    public string Uri {
      get {
        return this["uri"] as string;
      }
    }
  }
}