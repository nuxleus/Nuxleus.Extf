using System;
using System.Xml;
using System.Xml.Xsl;
using System.IO;

namespace  Xameleon {

  public partial class Transform {

    bool _IS_INITIALIZED = false;

    public Transform () {}

    internal Context Create() {
      Context context = new Context();
      return context;
    }

    internal XmlDocument Go(Context context) {
      if (!this._IS_INITIALIZED) return Process(Init(context));
      else return Process(context);
    }

  }
}
