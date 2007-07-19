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

namespace Xameleon.Transform {

  public enum HashAlgorithm { MD5, SHA1, SHA256 };

  public class Context : IDisposable {

    Uri _requestUri;
    String _requestUriHash;
    FileInfo _requestXmlFileInfo;
    String _eTag;
    Hashtable _xsltParams;
    NameValueCollection _httpQueryString;
    NameValueCollection _httpForm;
    HttpCookieCollection _httpCookies;
    NameValueCollection _httpParams;

    public Context(HttpContext context, HashAlgorithm algorithm, String key, FileInfo fileInfo, Hashtable xsltParams, params int[] eTagArray) {
      _requestUri = context.Request.Url;
      _requestUriHash = _requestUri.GetHashCode().ToString();
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
    public String RequestUriHash {
      get { return _requestUriHash; }
      set { _requestUriHash = value; }
    }
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
    public string GenerateETag(string key, HashAlgorithm algorithm, params int[] eTagArray) {
      switch (algorithm) {
        case HashAlgorithm.SHA1:
          return HashcodeGenerator.GetHMACSHA1Base64String(key, eTagArray);
        case HashAlgorithm.SHA256:
          return HashcodeGenerator.GetHMACSHA256Base64String(key, eTagArray);
        case HashAlgorithm.MD5:
        default:
          return HashcodeGenerator.GetHMACMD5Base64String(key, eTagArray);
      }
    }
    public int GetWeakHashcode(bool useQueryString, bool useETag) {
      StringBuilder builder = new StringBuilder(_requestUriHash);
      builder.Append(_xsltParams.ToString());
      if (useQueryString)
        builder.Append(_httpQueryString.ToString());
      if (useETag)
        builder.Append(_eTag);
      builder.Append(_httpForm.ToString());
      return builder.ToString().GetHashCode();
    }
    public int GetStrongHashcode(bool useQueryString, bool useServerVariables) {
      StringBuilder builder = new StringBuilder(_requestUriHash);
      builder.Append(_xsltParams.GetHashCode());
      if (useServerVariables)
        builder.Append(_httpParams.ToString());
      else {
        if (useQueryString)
          builder.Append(_httpQueryString.ToString());
        builder.Append(_httpForm.ToString());
        builder.Append(_httpCookies.ToString());
      }
      return builder.ToString().GetHashCode();
    }

    #region IDisposable Members

    public void Dispose() {

    }

    public void Clear() {
      _httpCookies = null;
      _httpForm = null;
      _httpParams = null;
      _httpQueryString = null;
      _requestUri = null;
      _requestUriHash = null;
      _xsltParams = null;
    }

    #endregion
  }
}
