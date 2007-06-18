using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Xameleon.Function {

    public static class HttpWebResponseStream {

        public static string GetResponseString(Stream stream) {
            StringBuilder builder = new StringBuilder();
            using (StringWriter writer = new StringWriter(builder)) {
                using (StreamReader reader = new StreamReader(stream)) {
                    builder.Append(reader.ReadToEnd());
                }
                return writer.ToString();
            }
        }
    }
}
