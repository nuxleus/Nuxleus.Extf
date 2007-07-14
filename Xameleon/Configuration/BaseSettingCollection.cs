﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Xameleon.Configuration {

  public class BaseSettingCollection : ConfigurationElementCollection {

    public BaseSetting this[int index] {
      get {
        return base.BaseGet(index) as BaseSetting;
      }
      set {
        if (base.BaseGet(index) != null) {
          base.BaseRemoveAt(index);
        }
        this.BaseAdd(index, value);
      }
    }

    protected override ConfigurationElement CreateNewElement() {
      return new BaseSetting();
    }

    protected override object GetElementKey(ConfigurationElement element) {
      return ((BaseSetting)element).Key;
    }

    [ConfigurationProperty("baseXslt", IsRequired = true)]
    public PreCompiledXslt BaseXslt {
      get {
        return this["baseXslt"] as PreCompiledXslt;
      }
    }
  }
}
