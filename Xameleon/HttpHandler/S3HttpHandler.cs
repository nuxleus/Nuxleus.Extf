// Copyright (c) 2006 by M. David Peterson
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.

using System;
using System.IO;
using System.Web;
using Saxon.Api;

namespace Xameleon.Transform {

  class S3HttpHandler : IHttpHandler {

    TextWriter _writer;
    HttpContext _context;
    String _requestMethod;
    Context _transformContext;
    Processor _processor;
    XsltCompiler _compiler;

    public void ProcessRequest(HttpContext context) {

      _requestMethod = context.Request.HttpMethod;
      _writer = context.Response.Output;
      _context = context;
      _processor = (Processor)context.Application["processor"];
      _compiler = (XsltCompiler)context.Application["compiler"];
      _transformContext = new Context(context, _writer, _processor, _compiler, true);

      switch (_requestMethod) {

        case "GET": {
            new Transform().Process(_transformContext);
            break;
          }
        case "PUT": {
            new Transform().Process(_transformContext);
            break;
          }
        case "POST": {
            new Transform().Process(_transformContext);
            break;
          }
        case "DELETE": {
            new Transform().Process(_transformContext);
            break;
          }
        default: {
            new Transform().Process(_transformContext);
            break;
          }
      }
    }

    public bool IsReusable {
      get { return true; }
    }
  }
}