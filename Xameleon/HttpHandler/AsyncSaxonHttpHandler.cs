// Copyright (c) 2006 by M. David Peterson
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.

using System;
using System.IO;
using System.Data;
using System.Configuration;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Collections;
using Memcached.ClientLibrary;
using System.Text;
using Saxon.Api;
using IronPython.Hosting;
using System.Xml;

namespace Xameleon.Transform {

  class AsyncSaxonHttpHandler : IHttpAsyncHandler {

    MemcachedClient _memcachedClient;
    bool _useMemcachedClient = false;
    XsltCompiledHashtable _xsltCompiledHashtable;
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
    Hashtable _globalXsltParams;
    Hashtable _sessionXsltParams;
    Hashtable _requestXsltParams;
    Hashtable _xsltParams;
    Context _requestContext;
    String _output;

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
      _transform = new Transform();
      _transformAsyncResult = new TransformServiceAsyncResult(cb, extraData);
      _processor = (Processor)context.Application["processor"];
      _compiler = (XsltCompiler)context.Application["compiler"];
      _serializer = (Serializer)context.Application["serializer"];
      _xsltCompiledHashtable = (XsltCompiledHashtable)context.Application["xsltCompiledHashtable"];
      _resolver = (XmlUrlResolver)context.Application["resolver"];
      _globalXsltParams = (Hashtable)context.Application["globalXsltParams"];
      _sessionXsltParams = (Hashtable)context.Application["sessionXsltParams"];
      _requestXsltParams = (Hashtable)context.Application["requestXsltParams"];
      //_pythonEngine = (PythonEngine)context.Application["pythonEngine"];
      //_useMemcachedClient = (bool)context.Application["usememcached"];

      //if (_useMemcachedClient) {
      //  _memcachedClient = (MemcachedClient)context.Application["memcached"];
      //  //_transformContext.MemcachedClient = _memcachedClient;
      //}
      _transformAsyncResult._context = context;
      _output = "foobar";
      _transformAsyncResult.CompleteCall();
      return _transformAsyncResult;

      //try {

      //  switch (_httpMethod) {

      //    case "GET": {

      //        XsltTransformer transform = _xsltCompiledHashtable.GetTransformer("baseTemplate", "/transform/base.xslt", new Uri("http://localhost/", UriKind.Absolute), _processor);

      //        foreach (DictionaryEntry entry in (Hashtable)_xsltCompiledHashtable.GetHashtable()) {
      //          XsltTransformer transformer = (XsltTransformer)entry.Value;
      //          _context.Response.Output.WriteLine("Key: " + entry.Key);
      //          _context.Response.Output.WriteLine("Value: " + transformer.GetHashCode().ToString());
      //          _context.Response.Output.WriteLine("Value2: " + transform.GetHashCode().ToString());
      //        }


      //        if (_useMemcachedClient) {
      //          string key = _context.Request.Url.GetHashCode().ToString();
      //          string obj = (string)_memcachedClient.Get(key);
      //          if (obj != null) {
      //            _output = obj;
      //            _transformAsyncResult.CompleteCall();
      //            return _transformAsyncResult;
      //          } else {
      //            _xsltParams = new Hashtable();

      //            if (_sessionXsltParams != null && _globalXsltParams.Count > 0) {
      //              foreach (DictionaryEntry param in _globalXsltParams) {
      //                _xsltParams[param.Key] = (string)param.Value;
      //              }
      //            }
      //            if (_sessionXsltParams != null && _sessionXsltParams.Count > 0) {
      //              foreach (DictionaryEntry param in _sessionXsltParams) {
      //                _xsltParams[param.Key] = (string)param.Value;
      //              }
      //            }

      //            _xsltParams["context"] = _context;
      //            _xsltParams["request"] = _context.Request;
      //            _xsltParams["response"] = _context.Response;
      //            _xsltParams["server"] = _context.Server;
      //            _xsltParams["timestamp"] = _context.Timestamp;
      //            _xsltParams["session"] = _context.Session;
      //            _xsltParams["errors"] = _context.AllErrors;
      //            _xsltParams["cache"] = _context.Cache;
      //            _xsltParams["user"] = _context.User;

      //            try {
      //              _requestContext = new Context(_context, _processor, _compiler, _serializer, _resolver, _xsltParams, true);
      //              _transform.BeginAsyncProcess(_requestContext);
      //              _output = _transformContext.StringBuilder.ToString();
      //              _transformAsyncResult.CompleteCall();
      //              return _transformAsyncResult;
      //            } catch (Exception e) {
      //              _exception = e;
      //              WriteError();
      //              _transformAsyncResult.CompleteCall();
      //              return _transformAsyncResult;
      //            }
      //          }
      //        } else {
      //          _transform.BeginAsyncProcess(_requestContext);
      //          _transformAsyncResult.CompleteCall();
      //          return _transformAsyncResult;
      //        }
      //        _transformAsyncResult.CompleteCall();
      //        break;
      //      }
      //    case "PUT": {
      //        _transform.BeginAsyncProcess(_requestContext);
      //        _transformAsyncResult.CompleteCall();
      //        return _transformAsyncResult;
      //        break;
      //      }
      //    case "POST": {
      //        _transform.BeginAsyncProcess(_requestContext);
      //        _transformAsyncResult.CompleteCall();
      //        return _transformAsyncResult;
      //        break;
      //      }
      //    case "DELETE": {
      //        _transform.BeginAsyncProcess(_requestContext);
      //        _transformAsyncResult.CompleteCall();
      //        return _transformAsyncResult;
      //        break;
      //      }
      //    default: {
      //        _transform.BeginAsyncProcess(_requestContext);
      //        _transformAsyncResult.CompleteCall();
      //        return _transformAsyncResult;
      //        break;
      //      }
      //  }

      //} catch (Exception ex) {
      //  _exception = ex;
      //  WriteError();
      //  _transformAsyncResult.CompleteCall();
      //  return _transformAsyncResult;
      //}
    }

    public void EndProcessRequest(IAsyncResult result) {
      TransformServiceAsyncResult async = result as TransformServiceAsyncResult;
      async._context.Response.Write(
        "<H1>This is an <i>Asynchronous</i> response!!</H1>");
      async._context.Response.Write(_output);
      //if (_useMemcachedClient)
      //  _memcachedClient.Set(_transformContext.RequestUriHash, _output);
      //_writer.Dispose();
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