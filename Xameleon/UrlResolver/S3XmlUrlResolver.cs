using System;
using System.Xml;
using System.IO;
using System.Net;

namespace Xameleon.UrlResolver {
    public class S3XmlResolver : XmlUrlResolver {
        public override object GetEntity(Uri absoluteUri, String role, Type ofObjectToReturn) {
            String uri = absoluteUri.GetComponents(UriComponents.AbsoluteUri, UriFormat.Unescaped);
            WebResponse response = WebRequest.Create(uri).GetResponse();
            return (Stream)response.GetResponseStream();
        }
    }
}
