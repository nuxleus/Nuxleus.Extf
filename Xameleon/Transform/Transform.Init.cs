using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using Saxon.Api;
using Xameleon.Properties;
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
        

        private void Init(HttpContext context)
        {
            AppSettings settings = new AppSettings();
            string setting = settings.GetSetting("xsltParamKeyPrefix");
            if (setting != null)
            {
                _xsltParamKey = setting;
            }
            Uri absoluteUri = new Uri(context.Server.MapPath(settings.GetSetting("baseTemplate")));
            _XsltParams = settings.GetSettingArray(_xsltParamKey);
            _Resolver = new XmlUrlResolver();
            _Resolver.Credentials = CredentialCache.DefaultCredentials;
            _TemplateStream = (Stream)_Resolver.GetEntity(absoluteUri, null, typeof(Stream));
            _Processor = new Processor();
            _Compiler = _Processor.NewXsltCompiler();
            _Compiler.BaseUri = absoluteUri;
            _Template = _Compiler.Compile(_TemplateStream);
            _IS_INITIALIZED = true;
        }

        private Context Init(Context context)
        {
            context.BaseUri = new Uri("http://localhost/");
            context.XmlSource = new Uri(Resources.SourceXml);
            context.XsltSource = new Uri(Resources.SourceXslt);
            context.ResultDocument = new XmlDocument();
            context.Resolver = new XmlUrlResolver();
            context.Resolver.Credentials = CredentialCache.DefaultCredentials;
            if (PrepareTransform(context))
            {
                _IS_INITIALIZED = true;
                return context;
            }
            return null;
        }

    }
}
