// Copyright (c) 2006 by M. David Peterson
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.

using System;
using System.Web;

namespace Xameleon {

    class HttpHandler : IHttpHandler {

        public void ProcessRequest(HttpContext context) {

            HttpRequest request = context.Request;

            switch (request.HttpMethod) {

                case "GET": {
                        // temp hack
                        context.Response.ContentType = "text/xml";
                        new Transform().Process(context);
                        break;
                    }
                case "PUT": {
                        // temp hack
                        context.Response.ContentType = "text/xml";
                        new Transform().Process(context);
                        break;
                    }
                case "POST": {
                        // temp hack
                        context.Response.ContentType = "text/xml";
                        new Transform().Process(context);
                        break;
                    }
                case "DELETE": {
                        // temp hack
                        context.Response.ContentType = "text/xml";
                        new Transform().Process(context);
                        break;
                    }
                default: {
                        // temp hack
                        context.Response.ContentType = "text/xml";
                        new Transform().Process(context);
                        break;
                    }
            }
        }

        public bool IsReusable {
            get { return true; }
        }

    }
}