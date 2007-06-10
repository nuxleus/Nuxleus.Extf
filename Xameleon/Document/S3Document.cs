using System;
using System.Collections.Generic;
using System.Text;
using Saxon.Api;
using javax.xml.transform;

namespace Xameleon.Document {


    public class S3Document : XmlDestination {

        Result _result;

        public S3Document() { }

        public override Result GetResult() {
            this._result = new DocumentResult();
            return _result;
        }
    }

}