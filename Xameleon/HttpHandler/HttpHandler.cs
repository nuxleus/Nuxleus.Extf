using System;
using System.Web;

namespace Xameleon {

    class HttpHandler : IHttpHandler {

        public void ProcessRequest (HttpContext context) {
            Transform myTransformer = new Transform();
            myTransformer.Process(context, context.Response.Output);
        }

        public bool IsReusable {
            get { return true; }
        }

    }
}