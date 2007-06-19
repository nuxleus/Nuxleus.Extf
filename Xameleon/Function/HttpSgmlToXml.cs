using System;
using Saxon.Api;
using Sgml;
using System.Xml;
using Saxon.Api;
using net.sf.saxon.value;
using System.IO;

namespace Xameleon.Function {

    public class HttpSgmlToXml {

        public HttpSgmlToXml() { }

        public static Value GetDocXml(String uri) {
            return getDocXml(uri, "/html");
        }

        public static Value GetDocXml(String uri, String path) {
            return getDocXml(uri, path);
        }

        public static String GetDocXml(String uri, String path, bool stripNS) {
            try {
                return getXdmNode(uri, path).OuterXml;
            } catch (Exception e) {
                throw;
            }
        }

        private static Value getDocXml(String uri, String path) {
            try {
                return Value.asValue(getXdmNode(uri, path).Unwrap());
            } catch (Exception e) {
                throw;
            }
        }

        private static XdmNode getXdmNode(String uri, String path) {
            try {
                SgmlReader sr = new SgmlReader();
                sr.Href = uri;

                XmlDocument htmlDoc = new XmlDocument();

                try {
                    htmlDoc.Load(sr);
                } catch (Exception e) {
                    throw;
                }

                XmlNode html = htmlDoc.SelectSingleNode(path);
                Processor processor = new Processor();
                return processor.NewDocumentBuilder().Build(html);

            } catch (Exception e) {
                throw;
            }
        }
    }
}
