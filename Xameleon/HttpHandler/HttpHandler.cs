// Copyright (c) 2006 by M. David Peterson
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.

using System;
using System.Web;

namespace Xameleon {

    class HttpHandler : IHttpHandler {

        public void ProcessRequest(HttpContext context) {

            HttpRequest request = context.Request;
            // temp hack
            context.Response.ContentType = "text/xml";

            switch (request.HttpMethod) {

                case "GET": {
                        new Transform().Process(context, true);
                        break;
                    }
                case "PUT": {
                        new Transform().Process(context, true);
                        break;
                    }
                case "POST": {
                        new Transform().Process(context, true);
                        break;
                    }
                case "DELETE": {
                        new Transform().Process(context, true);
                        break;
                    }
                default: {
                        new Transform().Process(context, true);
                        break;
                    }
            }
        }

        public bool IsReusable {
            get { return true; }
        }
    }
}