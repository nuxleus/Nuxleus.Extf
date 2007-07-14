// Copyright (c) 2006 by M. David Peterson
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.

using System;
using System.IO;
using System.Data;
using System.Configuration;
using System.Threading;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using System.Web.SessionState;
using System.Collections;
using Memcached.ClientLibrary;
using System.Text;
using Saxon.Api;
using IronPython.Hosting;
using System.Xml;
using Xameleon.Configuration;
using System.Collections.Generic;

namespace Xameleon.Transform {

  class AsyncSaxonHttpHandler : IHttpAsyncHandler {

    MemcachedClient _memcachedClient;
    bool _useMemcachedClient = false;
    XsltCompiledHashtable _xsltCompiledHashtable;
    Uri _baseXsltUri;
    String _baseXsltUriHash;
    Transform _transform;
    TextWriter _writer;
    StringBuilder _builder;
    HttpContext _context;
    TransformServiceAsyncResult _transformAsyncResult;
    String _httpMethod;
    Exception _exception;
    Context _transformContext;
    Processor _processor;
    XsltCompiler _compiler;
    Serializer _serializer;
    PythonEngine _pythonEngine;
    XmlUrlResolver _resolver;
    Hashtable _xsltParams;
    String _output;
    BaseXsltContext _baseXsltContext;

    public void ProcessRequest(HttpContext context) {
      //not called
    }

    public bool IsReusable {
      get { return true; }
    }

    #region IHttpAsyncHandler Members

    public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {

      _context = context;
      _writer = _context.Response.Output;
      _httpMethod = _context.Request.HttpMethod;
      _transform = (Transform)context.Application["transform"];
      _transformAsyncResult = new TransformServiceAsyncResult(cb, extraData);
      _processor = (Processor)context.Application["processor"];
      _compiler = (XsltCompiler)context.Application["compiler"];
      _serializer = (Serializer)context.Application["serializer"];
      _xsltCompiledHashtable = (XsltCompiledHashtable)context.Application["xsltCompiledHashtable"];
      _resolver = (XmlUrlResolver)context.Application["resolver"];
      _xsltParams = (Hashtable)context.Application["xsltParams"];
      _useMemcachedClient = (bool)context.Application["usememcached"];
      _baseXsltContext = (BaseXsltContext)context.Application["baseXsltContext"];
      _baseXsltUri = _baseXsltContext.BaseXsltUri;
      _baseXsltUriHash = _baseXsltContext.UriHash;

      if (_useMemcachedClient) {
        _memcachedClient = (MemcachedClient)context.Application["memcached"];
      }
      _transformAsyncResult._context = context;
      
      try {

        switch (_httpMethod) {

          case "GET": {

              //XsltTransformer transform = _xsltCompiledHashtable.GetTransformer("baseTemplate", "/transform/base.xslt", new Uri("http://localhost:9999/", UriKind.Absolute), _processor);

              //foreach (DictionaryEntry entry in (Hashtable)_xsltCompiledHashtable.GetHashtable()) {
              //  XsltTransformer transformer = (XsltTransformer)entry.Value;
              //  _context.Response.Output.WriteLine("Key: " + entry.Key);
              //  _context.Response.Output.WriteLine("Value: " + transformer.GetHashCode().ToString());
              //  _context.Response.Output.WriteLine("Value2: " + transform.GetHashCode().ToString());
              //}
              
              if (_useMemcachedClient) {
                _output = "memcached is true";
                  string key = _context.Request.Url.GetHashCode().ToString();
                  string obj = (string)_memcachedClient.Get(key);
                if (obj != null) {
                  _output = obj;
                  _transformAsyncResult.CompleteCall();
                  return _transformAsyncResult;
                } else {
                  
                  try {
                    _transform.BeginAsyncProcess(GetContext());
                    _output = _transformContext.StringBuilder.ToString();
                    _transformAsyncResult.CompleteCall();
                    return _transformAsyncResult;
                  } catch (Exception e) {
                    _exception = e;
                    WriteError();
                    _transformAsyncResult.CompleteCall();
                    return _transformAsyncResult;
                  }
                }
              } else {
                _transform.BeginAsyncProcess(GetContext());
                _output = _transformContext.StringBuilder.ToString();
                _transformAsyncResult.CompleteCall();
                return _transformAsyncResult;
              }
              break;
            }
          case "PUT": {
              _transform.BeginAsyncProcess(GetContext());
              _output = _transformContext.StringBuilder.ToString();
              _transformAsyncResult.CompleteCall();
              return _transformAsyncResult;
            }
          case "POST": {
              _transform.BeginAsyncProcess(GetContext());
              _output = _transformContext.StringBuilder.ToString();
              _transformAsyncResult.CompleteCall();
              return _transformAsyncResult;
            }
          case "DELETE": {
              _transform.BeginAsyncProcess(GetContext());
              _output = _transformContext.StringBuilder.ToString();
              _transformAsyncResult.CompleteCall();
              return _transformAsyncResult;
            }
          default: {
              _transform.BeginAsyncProcess(GetContext());
              _output = _transformContext.StringBuilder.ToString();
              _transformAsyncResult.CompleteCall();
              return _transformAsyncResult;
            }
        }

      } catch (Exception ex) {
        _exception = ex;
        WriteError();
        _transformAsyncResult.CompleteCall();
        return _transformAsyncResult;
      }
    }

    public void EndProcessRequest(IAsyncResult result) {
      TransformServiceAsyncResult async = result as TransformServiceAsyncResult;
      async._context.Response.Write(_output);
      if (_useMemcachedClient)
        _memcachedClient.Set(_transformContext.RequestUriHash, _output);
      _writer.Dispose();
    }

    private Context GetContext() {
      _transformContext = new Context(_context, _processor, _compiler, _serializer, _resolver, _xsltParams, _xsltCompiledHashtable, _baseXsltUri, _baseXsltUriHash);
      return _transformContext;
    }

    private void WriteError() {
      _context.Response.Output.WriteLine("<html>");
      _context.Response.Output.WriteLine("<head>");
      _context.Response.Output.WriteLine("<title>Xameleon Transformation Error</title>");
      _context.Response.Output.WriteLine("</head>");
      _context.Response.Output.WriteLine("<body>");
      _context.Response.Output.WriteLine("<h3>Error Message</h3>");
      _context.Response.Output.WriteLine("<p>" + _exception.Message + "</p>");
      _context.Response.Output.WriteLine("<h3>Error Source</h3>");
      _context.Response.Output.WriteLine("<p>" + _exception.Source + "</p>");
      _context.Response.Output.WriteLine("<h3>Error StackTrace</h3>");
      _context.Response.Output.WriteLine("<p>" + _exception.StackTrace + "</p>");
      _context.Response.Output.WriteLine("</body>");
      _context.Response.Output.WriteLine("</html>");
    }

    #endregion
  }
}