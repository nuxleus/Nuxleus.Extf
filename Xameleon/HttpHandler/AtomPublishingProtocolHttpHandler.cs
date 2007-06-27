// Copyright (c) 2006 by M. David Peterson
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.

using System;
using System.Web;
using System.IO;

namespace Xameleon {

    class AtomPublishingProtocolHttpHandler : IHttpHandler {

        private TextWriter _writer;
        private HttpContext _context;
        private String _requestMethod;

        public void ProcessRequest(HttpContext context) {

            _requestMethod = context.Request.HttpMethod;
            _writer = context.Response.Output;
            _context = context;

            switch (_requestMethod) {

                case "GET": {
                        new Transform().Process(_context, _writer, false);
                        break;
                    }
                case "PUT": {
                        new Transform().Process(_context, _writer, false);
                        break;
                    }
                case "POST": {
                        new Transform().Process(_context, _writer, false);
                        break;
                    }
                case "DELETE": {
                        new Transform().Process(_context, _writer, false);
                        break;
                    }
                default: {
                        new Transform().Process(_context, _writer, false);
                        break;
                    }
            }
        }

        public bool IsReusable {
            get { return true; }
        }
    }
}