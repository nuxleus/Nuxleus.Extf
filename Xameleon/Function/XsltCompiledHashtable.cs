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

namespace Xameleon {

    public class XsltCompiledHashtable {

        private Processor processor = new Processor();
        private Hashtable results;

        public XsltCompiledHashtable() {
            this.results = new Hashtable();
        }
        public XsltCompiledHashtable(Hashtable table) {
            this.results = table;
        }

        public XsltTransformer GetTransformer(string href, Uri baseUri) {

            foreach (DictionaryEntry entry in results) {
                string uri = (string)entry.Key;
                if (uri == href) {
                    return (XsltTransformer)results[uri];
                }
            }

            XsltTransformer transformer = processor.NewXsltCompiler().Compile(new Uri(href)).Load();
            results[href] = transformer;
            return transformer;
        }
    }
}
