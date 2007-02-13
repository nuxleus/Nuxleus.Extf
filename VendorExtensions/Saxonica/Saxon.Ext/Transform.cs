using System;
using System.Xml;
using Saxon.Api;
using System.IO;

namespace Saxon.Ext {

  public partial class Transform {

    XsltExecutable _Template;
    Stream _TemplateStream;
    Stream _SourceXml;
    Processor _Processor;
    XsltCompiler _Compiler;
    bool _IS_INITIALIZED = false;

    public Transform () {}

    public Context Create() {
      Context context = new Context();
      return context;
    }

    public XmlDocument Go(Context context) {
      if (!this._IS_INITIALIZED) return Process(Init(context));
      else return Process(context);
    }

  }
}
