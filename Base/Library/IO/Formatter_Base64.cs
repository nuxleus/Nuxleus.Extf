using System;
using System.Text;

namespace Extf.Net.IO {

    public partial class Formatter_Base64 : ICustomFormatter, IFormatProvider {

        public Formatter_Base64() { }

        Encoding encode = new UTF8Encoding();

        public object GetFormat (Type formatType) {
            if (formatType == typeof(ICustomFormatter))
                return this;
            else
                return null;
        }

        public string Format (string format, object arg, IFormatProvider formatProvider) {
            if (format != null)
                return Convert.ToBase64String(encode.GetBytes(format.ToCharArray()));
            else if (arg != null)
                return ((IFormattable)arg).ToString(format, formatProvider);
            else return null;
        }
    }
}
