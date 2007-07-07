// Copyright (c) 2006 by M. David Peterson
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.

using System;
using System.IO;
using System.Threading;
using System.Web;
using System.Collections;
using Memcached.ClientLibrary;

namespace Xameleon.Transform {

  class AsyncSaxonHttpHandler : IHttpAsyncHandler {

    MemcachedClient _memcachedClient;
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
                _memcachedClient = new MemcachedClient();
                //TODO: Finish this out
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
      _context.Response.Write(_exception.Message);
      _context.Response.Write(_exception.Source);
      _context.Response.Write(_exception.StackTrace);
    }

    #endregion

    


  }
}