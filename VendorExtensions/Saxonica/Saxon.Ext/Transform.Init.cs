using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using Saxon.Api;
using Saxon.Ext.Properties;
using System.Net;

namespace Saxon.Ext {

  public partial class Transform {

    private Context Init (Context context) {
      try {
        context.BaseUri = new Uri("http://localhost/");
        context.XmlSource = new Uri(Resources.SourceXml);
        context.XsltSource = new Uri(Resources.SourceXslt);
        context.ResultDocument = new XmlDocument();
        context.Resolver = new XmlUrlResolver();
        context.Resolver.Credentials = CredentialCache.DefaultCredentials;
      } catch (Exception) {
        throw;
      }
      if(PrepareTransform(context)) {
        this._IS_INITIALIZED = true;
        return context;
      }
      else {
        return null;
      }
    }

    private bool PrepareTransform (Context context) {
      this._SourceXml = (Stream)context.Resolver.GetEntity(context.XmlSource, null, typeof(Stream));
      this._TemplateStream = (Stream)context.Resolver.GetEntity(context.XsltSource, null, typeof(Stream));
      this._Processor = new Processor();
      this._Compiler = _Processor.NewXsltCompiler();
      this._Template = this._Compiler.Compile(_TemplateStream);
      //this is a horrible, lazy, good for not much hack...  FIXME!
      return true;
    }

    //public bool Init (string clipData) {
    //  if (clipData.Length > 5) {
    //    try {
    //      this.doc = XmlReader.Create(new StringReader(clipData));
    //    } catch (ArgumentNullException) {
    //      try {
    //        this.doc = this._init;
    //      } catch (Exception) {
    //        this.doc = XmlReader.Create(new StringReader(this._backup));
    //      }
    //    }
    //  } else doc = XmlReader.Create(new StringReader(this._backup));
    //  return DoProcess(this.doc, this._xslt, this._baseUri);
    //}

    //public XmlDocument Init (string xmlKey, string xsltKey) {
    //  try {
    //    this.doc = GetResource(xmlKey);
    //    try {
    //      this._xslt = GetResource(xsltKey);
    //    } catch (Exception) {
    //      this._xslt = this._xslt;
    //    }
    //  } catch (Exception) {
    //    this.doc = XmlReader.Create(new StringReader(this._backup));
    //  }
    //  return DoProcess(this.doc, this._xslt, this._baseUri);
    //}

    //private XmlReader GetResource (string key) {
    //  return this.rm.GetSetting(key);
    //}
  }
}
