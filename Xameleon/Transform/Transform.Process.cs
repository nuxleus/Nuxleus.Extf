using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Saxon.Api;
using System.IO;

namespace Xameleon
{

    public partial class Transform
    {

        XsltExecutable _Template;
        Stream _TemplateStream;
        Stream _SourceXml;
        Processor _Processor;
        XsltCompiler _Compiler;

        private bool PrepareTransform(Context context)
        {
            this._SourceXml = (Stream)context.Resolver.GetEntity(context.XmlSource, null, typeof(Stream));
            this._TemplateStream = (Stream)context.Resolver.GetEntity(context.XsltSource, null, typeof(Stream));
            this._Processor = new Processor();
            this._Compiler = _Processor.NewXsltCompiler();
            this._Template = this._Compiler.Compile(_TemplateStream);
            //this is a horrible, lazy, good for not much hack...  FIXME!
            return true;
        }

        private XmlDocument Process(Context context)
        {

            using (this._SourceXml)
            {

                using (this._TemplateStream)
                {

                    XmlDocument doc = new XmlDocument();
                    doc.Load(this._SourceXml);
                    XdmNode input = this._Processor.NewDocumentBuilder().Wrap(doc);

                    XsltTransformer transformer = this._Template.Load();
                    transformer.InitialContextNode = input;

                    DomDestination result = new DomDestination();
                    transformer.Run(result);
                    return result.XmlDocument;

                }
            }
        }
    }
}