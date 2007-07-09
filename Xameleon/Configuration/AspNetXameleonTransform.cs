﻿using System;
using System.Configuration;

namespace Xameleon.Configuration {

  public class AspNetXameleonConfiguration : ConfigurationSection {


    public static AspNetXameleonConfiguration GetConfig() {
      return (AspNetXameleonConfiguration)ConfigurationManager.GetSection("Xameleon.WebApp/xameleon");
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

    [ConfigurationProperty("baseSettings", IsRequired = false)]
    public BaseSettingCollection BaseSettings {
      get {
        return this["baseSettings"] as BaseSettingCollection;
      }
    }

    [ConfigurationProperty("xsltParams", IsRequired = false)]
    public XsltParamCollection XsltParams {
      get {
        return this["xsltParams"] as XsltParamCollection;
      }
    }

    [ConfigurationProperty("globalXsltParams", IsRequired = false)]
    public XsltParamCollection GlobalXsltParam {
      get {
        return this["globalXsltParams"] as XsltParamCollection;
      }
    }

    [ConfigurationProperty("sessionXsltParams", IsRequired = false)]
    public XsltParamCollection SessionXsltParam {
      get {
        return this["sessionXsltParams"] as XsltParamCollection;
      }
    }

    [ConfigurationProperty("httpContextXsltParams", IsRequired = false)]
    public XsltParamCollection HttpRequestXsltParams {
      get {
        return this["httpContextXsltParams"] as XsltParamCollection;
      }
    }
  }
}
