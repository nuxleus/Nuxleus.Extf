using System;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using Saxon.Api;
using Extf.Net.Configuration;
using System.Collections.Specialized;

namespace Xameleon {

  public partial class Transform {

        private XmlUrlResolver _Resolver;
        private XsltExecutable _Template;
        private Stream _TemplateStream;
        private Processor _Processor;
        private XsltCompiler _Compiler;
        private NameValueCollection _XsltParams;
        private string _xsltParamKey = "xsltParam_";

        private void Init (HttpContext context) {
            AppSettings baseConfig = new AppSettings();
            string tempXsltParamKey = baseConfig.GetSetting("xsltParamKeyPrefix");
            if (tempXsltParamKey != null) this._xsltParamKey = tempXsltParamKey;

            String xsltUri = context.Server.MapPath(baseConfig.GetSetting("baseTemplate"));
            Uri xUri = new Uri(xsltUri);

            this._XsltParams = baseConfig.GetSettingArray(this._xsltParamKey);
            this._Resolver = new XmlUrlResolver();
            this._Resolver.Credentials = CredentialCache.DefaultCredentials;

            this._TemplateStream = (Stream)this._Resolver.GetEntity(xUri, null, typeof(Stream));
            this._Processor = new Processor();
            this._Compiler = _Processor.NewXsltCompiler();
            this._Compiler.BaseUri = xUri;
            this._Template = this._Compiler.Compile(_TemplateStream);
            this._IS_INITIALIZED = true;
        }
    }
}
