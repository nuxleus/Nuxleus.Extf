using System;
using Saxon.Api;
using Sgml;
using System.Xml;
using Saxon.Api;
using net.sf.saxon.om;

namespace Xameleon.Function {

    public static class HttpSgmlToXml {

        public static Item GetDocXml(String uri) {
            return getDocXml(uri, "/html");
        }

        public static Item GetDocXml(String uri, String path) {
            return getDocXml(uri, path);
        }

        private static Item getDocXml(String uri, String path) {
            SgmlReader sr = new SgmlReader();
            sr.Href = uri;

            XmlDocument htmlDoc = new XmlDocument();
            htmlDoc.Load(sr);

            XmlNode html = htmlDoc.SelectSingleNode(path);

            Processor processor = new Processor();

            return processor.NewDocumentBuilder().Build(html).Implementation.getTypedValue().current();
        }
    }
}
