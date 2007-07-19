using System;
using System.Security.Cryptography;
using System.Text;

namespace Xameleon.Cryptography {

  public enum HashAlgorithm { MD5, SHA1, SHA256 };

  public class HashcodeGenerator {

    static Encoding encoder = new UTF8Encoding();

    public static String GetHMACMD5Base64String(string key, params object[] hashArray) {
      StringBuilder builder = new StringBuilder();
      HMACMD5 hmacMD5 = new HMACMD5(encoder.GetBytes(key));
      foreach (object obj in hashArray) {
        builder.Append(obj);
      }
      return Convert.ToBase64String(hmacMD5.ComputeHash(encoder.GetBytes(builder.ToString())));
    }
    public static String GetHMACSHA1Base64String(string key, params object[] hashArray) {
      StringBuilder builder = new StringBuilder();
      HMACSHA1 hmacMD5 = new HMACSHA1(encoder.GetBytes(key));
      foreach (object obj in hashArray) {
        builder.Append(obj);
      }
      return Convert.ToBase64String(hmacMD5.ComputeHash(encoder.GetBytes(builder.ToString())));
    }
    public static String GetHMACSHA256Base64String(string key, params object[] hashArray) {
      StringBuilder builder = new StringBuilder();
      HMACSHA256 hmacMD5 = new HMACSHA256(encoder.GetBytes(key));
      foreach (object obj in hashArray) {
        builder.Append(obj);
      }
      return Convert.ToBase64String(hmacMD5.ComputeHash(encoder.GetBytes(builder.ToString())));
    }
  }
}
