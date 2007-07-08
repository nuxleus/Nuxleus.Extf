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

namespace Xameleon.Transform {

  class AsyncSaxonHttpHandler : IHttpAsyncHandler {

    MemcachedClient _memcachedClient;
    XsltCompiledHashtable _xsltCompiledHashtable;
    Transform _transform = new Transform();
    TextWriter _writer;
    HttpContext _context;
    TransformServiceAsyncResult _transformAsyncResult;
    String _httpMethod;
    Exception _exception;
    Context _transformContext;

    public void ProcessRequest(HttpContext context) {
      //not called
    }

    public bool IsReusable {
      get { return true; }
    }

    #region IHttpAsyncHandler Members

    public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {
      _context = context;
      _writer = context.Response.Output;
      _httpMethod = context.Request.HttpMethod;
      _transformAsyncResult = new TransformServiceAsyncResult(cb, extraData);
      _transformContext = new Context(context, _writer, true);
      context.Response.Output.WriteLine((bool)context.Application["useMemcached"]);
      if((bool)context.Application["useMemcached"] == true)
        _memcachedClient = (MemcachedClient)context.Application["memcached"];

      _xsltCompiledHashtable = (XsltCompiledHashtable)context.Application["xsltCompiledHashtable"];

      try {
        BeginTransform(cb);
        return _transformAsyncResult;
      } catch (Exception ex) {
        _exception = ex;
        return _transformAsyncResult;
      }
    }

    private void BeginTransform(AsyncCallback cb) {

      //TODO: Temp hack
      bool useMemcached = false;
      try {

        switch (_httpMethod) {

          case "GET": {
              if (useMemcached) {
                
                
              } else {
                _transform.BeginAsyncProcess(_transformContext, cb, _transformAsyncResult);
              }
              break;
            }
          case "PUT": {
              _transform.BeginAsyncProcess(_transformContext, cb, _transformAsyncResult);
              break;
            }
          case "POST": {
              _transform.BeginAsyncProcess(_transformContext, cb, _transformAsyncResult);
              break;
            }
          case "DELETE": {
              _transform.BeginAsyncProcess(_transformContext, cb, _transformAsyncResult);
              break;
            }
          default: {
              _transform.BeginAsyncProcess(_transformContext, cb, _transformAsyncResult);
              break;
            }
        }

      } catch (Exception ex) {
        _exception = ex;
        WriteError();
        _transformAsyncResult.CompleteCall();
      }
    }

    public void EndProcessRequest(IAsyncResult result) {
      _writer.Dispose();
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