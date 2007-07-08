using System;
using System.Configuration;

namespace Xameleon.Configuration {

  public class AspNetXameleonConfiguration : ConfigurationSection {


    public static AspNetXameleonConfiguration GetConfig() {
      return ConfigurationManager.GetSection("xameleon") as AspNetXameleonConfiguration;
    }

    [ConfigurationProperty("useMemcached", DefaultValue = "no", IsRequired = false)]
    public string UseMemcached {
      get {
        return this["useMemcached"] as string;
      }
    }

    [ConfigurationProperty("defaultEngine", DefaultValue="Saxon", IsRequired = false)]
    public string DefaultEngine {
      get {
        return this["defaultEngine"] as string;
      }
    }

    [ConfigurationProperty("xsltParams")]
    public XsltParamCollection XsltParams {
      get {
        return this["xsltParams"] as XsltParamCollection;
      }
    } 
  }
}
