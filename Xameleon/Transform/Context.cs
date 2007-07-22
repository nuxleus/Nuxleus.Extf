using System;
using System.Xml;
using System.IO;
using Saxon.Api;
using System.Collections.Specialized;
using System.Web;
using Xameleon.Configuration;
using System.Net;
using Xameleon.Properties;
using Xameleon.Function;
using System.Collections;
using Memcached.ClientLibrary;
using System.Text;
using Xameleon.Cryptography;
using System.Reflection;

namespace Xameleon.Transform {

  public class Context {

    Uri _requestUri;
    //String _requestUriHash;
    FileInfo _requestXmlFileInfo;
    String _eTag;
    Hashtable _xsltParams;
    NameValueCollection _httpQueryString;
    NameValueCollection _httpForm;
    HttpCookieCollection _httpCookies;
    NameValueCollection _httpParams;

    public Context(HttpContext context, HashAlgorithm algorithm, String key, FileInfo fileInfo, Hashtable xsltParams, params object[] eTagArray) {
      _requestUri = context.Request.Url;
      _requestXmlFileInfo = fileInfo;
      _xsltParams = xsltParams;
      _httpQueryString = context.Request.QueryString;
      _httpForm = context.Request.Form;
      _httpCookies = context.Request.Cookies;
      _httpParams = context.Request.Params;
      _eTag = GenerateETag(key, algorithm, eTagArray);
    }

    public Uri RequestUri {
      get { return _requestUri; }
      set { _requestUri = value; }
    }
    //public String RequestUriHash {
    //  get { return _requestUriHash; }
    //  set { _requestUriHash = value; }
    //}
    public FileInfo RequestXmlFileInfo {
      get { return _requestXmlFileInfo; }
      set { _requestXmlFileInfo = value; }
    }
    public String ETag {
      get { return _eTag; }
      set { _eTag = value; }
    }
    public Hashtable XsltParams {
      get { return _xsltParams; }
      set { _xsltParams = value; }
    }
    public NameValueCollection HttpParams {
      get { return _httpParams; }
      set { _httpParams = value; }
    }
    public HttpCookieCollection HttpCookies {
      get { return _httpCookies; }
      set { _httpCookies = value; }
    }
    public NameValueCollection HttpForm {
      get { return _httpForm; }
      set { _httpForm = value; }
    }
    public NameValueCollection HttpQueryString {
      get { return _httpQueryString; }
      set { _httpQueryString = value; }
    }
    public static string GenerateETag(string key, HashAlgorithm algorithm, params object[] eTagArray) {
      return HashcodeGenerator.GetHMACHashBase64String(key, algorithm, eTagArray);
    }
    public int GetRequestHashcode(bool useQueryString, params object[] objectArray) {
      StringBuilder builder = new StringBuilder(_eTag);
      builder.Append(_xsltParams.ToString());
      if (useQueryString)
        builder.Append(_httpQueryString.ToString());
      builder.Append(_httpForm.ToString());
      return builder.ToString().GetHashCode();
    }
    public void Clear() {
      //FOR FUTURE USE
    }
  }
}

