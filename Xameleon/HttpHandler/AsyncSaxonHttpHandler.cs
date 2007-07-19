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

    XsltTransformationManager _xslTransformationManager;
    MemcachedClient _memcachedClient;
    Transform _transform;
    TextWriter _writer;
    StringBuilder _builder;
    HttpContext _context;
    TransformServiceAsyncResult _transformAsyncResult;
    AsyncCallback _callback;
    String _httpMethod;
    Exception _exception;
    Context _transformContext;
    bool _CONTENT_IS_MEMCACHED = false;

    public void ProcessRequest(HttpContext context) {
      //not called
    }

    public bool IsReusable {
      get { return false; }
    }

    #region IHttpAsyncHandler Members

    public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {

      _context = context;
      _httpMethod = _context.Request.HttpMethod;

      _xslTransformationManager = (XsltTransformationManager)context.Application["xsltTransformationManager"];
      _memcachedClient = (MemcachedClient)context.Application["memcached"];
      _transform = _xslTransformationManager.Transform;
      _transformContext = (Context)context.Application["transformContext"];
      _transformAsyncResult = new TransformServiceAsyncResult(cb, extraData);
      _callback = cb;
      _CONTENT_IS_MEMCACHED = (bool)context.Application["CONTENT_IS_MEMCACHED"];

      _transformAsyncResult._context = context;
      _writer = (TextWriter)context.Application["textWriter"];
      _builder = (StringBuilder)context.Application["stringBuilder"];

      try {

        switch (_httpMethod) {

          case "GET": {

              if (_CONTENT_IS_MEMCACHED) {
                _transformAsyncResult.CompleteCall();
                return _transformAsyncResult;
              } else {
                try {
                  _transform.BeginAsyncProcess(_transformContext, _xslTransformationManager, _writer, _callback, _transformAsyncResult);
                  return _transformAsyncResult;
                } catch (Exception e) {
                  _exception = e;
                  WriteError();
                  _transformAsyncResult.CompleteCall();
                  return _transformAsyncResult;
                }
              }
            }
          case "PUT": {
              _transform.BeginAsyncProcess(_transformContext, _xslTransformationManager, _writer, _callback, _transformAsyncResult);
              return _transformAsyncResult;
            }
          case "POST": {
              _transform.BeginAsyncProcess(_transformContext, _xslTransformationManager, _writer, _callback, _transformAsyncResult);
              return _transformAsyncResult;
            }
          case "DELETE": {
              _transform.BeginAsyncProcess(_transformContext, _xslTransformationManager, _writer, _callback, _transformAsyncResult);
              return _transformAsyncResult;
            }
          default: {
              _transform.BeginAsyncProcess(_transformContext, _xslTransformationManager, _writer, _callback, _transformAsyncResult);
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
      using (_writer) {
        string output = _builder.ToString();
        using (TextWriter writer = HttpContext.Current.Response.Output) {
          writer.Write(output);
        }
        _transformContext.Clear();
        if (!_CONTENT_IS_MEMCACHED)
          _memcachedClient.Set(_transformContext.GetWeakHashcode(false, true).ToString(), output);
      }
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