using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Xameleon.Function {

    public static class HttpWebResponseStream {

        public static string GetResponseString(Stream stream) {
            using (StreamReader reader = new StreamReader(stream)) {
                return reader.ReadToEnd();
            }
        }
    }
}
