using System;
using Saxon.Api;
using Sgml;
using System.Xml;
using Saxon.Api;

namespace Xameleon.Function {

    public static class HttpSgmlToXml {

        public static XdmValue GetDocXml(String uri) {
            return getDocXml(uri, "/html/*");
        }

        public static XdmValue GetDocXml(String uri, String path) {
            return getDocXml(uri, path);
        }

        private static XdmValue getDocXml(String uri, String path) {
            SgmlReader sr = new SgmlReader();
            sr.Href = uri;

            XmlDocument htmlDoc = new XmlDocument();
            htmlDoc.Load(sr);

            XmlNode html = htmlDoc.SelectSingleNode(path);

            Processor processor = new Processor();
            return (XdmValue)processor.NewDocumentBuilder().Build(html);
        }
    }
}
