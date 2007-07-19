using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Xameleon.Function {

  public class HashcodeGenerator {

    static Encoding encoder = new UTF8Encoding();

    public static String GetHMACMD5Base64String(string key, params int[] hashArray) {
      StringBuilder builder = new StringBuilder();
      HMACMD5 hmacMD5 = new HMACMD5(encoder.GetBytes(key));
      foreach (int value in hashArray) {
        builder.Append(value);
      }
      return Convert.ToBase64String(hmacMD5.ComputeHash(encoder.GetBytes(builder.ToString())));
    }
    public static String GetHMACSHA1Base64String(string key, params int[] hashArray) {
      StringBuilder builder = new StringBuilder();
      HMACSHA1 hmacMD5 = new HMACSHA1(encoder.GetBytes(key));
      foreach (int value in hashArray) {
        builder.Append(value);
      }
      return Convert.ToBase64String(hmacMD5.ComputeHash(encoder.GetBytes(builder.ToString())));
    }
    public static String GetHMACSHA256Base64String(string key, params int[] hashArray) {
      StringBuilder builder = new StringBuilder();
      HMACSHA256 hmacMD5 = new HMACSHA256(encoder.GetBytes(key));
      foreach (int value in hashArray) {
        builder.Append(value);
      }
      return Convert.ToBase64String(hmacMD5.ComputeHash(encoder.GetBytes(builder.ToString())));
    }
  }
}
