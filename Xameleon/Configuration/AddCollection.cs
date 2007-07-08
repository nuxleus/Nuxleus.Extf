using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Xameleon.Configuration {

  public class AddCollection : ConfigurationElementCollection {

    public Add this[int index] {
      get {
        return base.BaseGet(index) as Add;
      }
      set {
        if (base.BaseGet(index) != null) {
          base.BaseRemoveAt(index);
        }
        this.BaseAdd(index, value);
      }
    }

    protected override ConfigurationElement CreateNewElement() {
      return new Add();
    }

    protected override object GetElementKey(ConfigurationElement element) {
      return ((Add)element).Key;
    }
  }
}
