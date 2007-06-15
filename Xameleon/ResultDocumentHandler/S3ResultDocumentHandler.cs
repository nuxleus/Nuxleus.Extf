using System;
using System.Collections.Generic;
using System.Text;
using Saxon.Api;
using javax.xml.transform;
using Xameleon.Document;
using System.Collections;

namespace Xameleon.ResultDocumentHandler {

    public class S3ResultDocumentHandler : IResultDocumentHandler {

        private Hashtable results;

        public S3ResultDocumentHandler(Hashtable table) {
            this.results = table;
        }

        public XmlDestination HandleResultDocument(string href, Uri baseUri) {
            DomDestination destination = new DomDestination();
            results[href] = destination;
            return destination;
        }
    }
}
