// Copyright (c) 2006 by M. David Peterson
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.

using System;
using System.IO;
using System.Web;

namespace Xameleon {

  class HttpHandler : IHttpHandler {

    private TextWriter _writer;
    private HttpContext _context;
    private String _requestMethod;
    Transform.Context _transformContext;

    public void ProcessRequest(HttpContext context) {

      _requestMethod = context.Request.HttpMethod;
      _writer = context.Response.Output;
      _context = context;
      _transformContext = new Transform.Context(_context, _writer, true);

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