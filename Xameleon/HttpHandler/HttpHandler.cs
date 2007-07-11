// Copyright (c) 2006 by M. David Peterson
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.

using System;
using System.IO;
using System.Web;
using Saxon.Api;
using System.Xml;

namespace Xameleon.Transform {

  class HttpHandler : IHttpHandler {

    TextWriter _writer;
    HttpContext _context;
    String _requestMethod;
    Context _transformContext;
    Processor _processor;
    XsltCompiler _compiler;
    Serializer _serializer;
    XmlUrlResolver _resolver;

    public void ProcessRequest(HttpContext context) {

      _requestMethod = context.Request.HttpMethod;
      _writer = context.Response.Output;
      _context = context;
      _processor = (Processor)context.Application["processor"];
      _compiler = (XsltCompiler)context.Application["compiler"];
      _serializer = (Serializer)context.Application["serializer"];
      _resolver = (XmlUrlResolver)context.Application["resolver"];
      _transformContext = new Context(context, _processor, _compiler, _serializer, _resolver, true);

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