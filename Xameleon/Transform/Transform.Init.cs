using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using Xameleon.Properties;
using System.Net;
using Saxon.Api;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using Extf.Net.Configuration;

namespace Xameleon
{

    public partial class Transform
    {

        // Fields
        private XsltCompiler _Compiler;
        private Processor _Processor;
        private XmlUrlResolver _Resolver;
        private Stream _SourceXml;
        private XsltExecutable _Template;
        private Stream _TemplateStream;
        private string _xsltParamKey = "xsltParam_";
        private NameValueCollection _XsltParams;
        private Uri _baseUri;

        private void Init(HttpContext context)
        {
            AppSettings settings = new AppSettings();
            string setting = settings.GetSetting("xsltParamKeyPrefix");
            if (setting != null)
            {
                this._xsltParamKey = setting;
            }
            this._baseUri = new Uri(context.Server.MapPath(settings.GetSetting("baseTemplate")));
            this._XsltParams = settings.GetSettingArray(this._xsltParamKey);
            this._Resolver = new XmlUrlResolver();
            this._Resolver.Credentials = CredentialCache.DefaultCredentials;
            this._TemplateStream = (Stream)this._Resolver.GetEntity(this._baseUri, null, typeof(Stream));
            this._Processor = new Processor();
            this._Compiler = this._Processor.NewXsltCompiler();
            this._Compiler.BaseUri = this._baseUri;
            this._Template = this._Compiler.Compile(this._TemplateStream);
            this._IS_INITIALIZED = true;
        }

        private Context Init(Context context)
        {
            try
            {
                context.BaseUri = new Uri("http://localhost/");
                context.XmlSource = new Uri(Resources.SourceXml);
                context.XsltSource = new Uri(Resources.SourceXslt);
                context.ResultDocument = new XmlDocument();
                context.Resolver = new XmlUrlResolver();
                context.Resolver.Credentials = CredentialCache.DefaultCredentials;
            }
            catch (Exception)
            {
                throw;
            }
            if (this.PrepareTransform(context))
            {
                this._IS_INITIALIZED = true;
                return context;
            }
            return null;
        }

    }
}
