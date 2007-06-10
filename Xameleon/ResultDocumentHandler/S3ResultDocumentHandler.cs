using System;
using System.Collections.Generic;
using System.Text;
using Saxon.Api;
using javax.xml.transform;
using Xameleon.Document;

namespace Xameleon.ResultDocumentHandler {

    public class S3ResultDocumentHandler : IResultDocumentHandler {

        public XmlDestination HandleResultDocument(string href, Uri baseUri) {
            XmlDestination destDoc = new S3Document();
            return destDoc;
        }
    }
}
