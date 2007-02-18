using System;
using System.Web;

namespace Xameleon
{

    class HttpHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            Transform myTransformer = new Transform();

            switch (context.Request.RequestType)
            {
                case "GET":
                    myTransformer.Process(context, context.Response.Output);
                    break;

                case "POST":
                    context.Response.Write(context.Request.RequestType);
                    break;

                case "PUT":
                    context.Response.Write(context.Request.RequestType);
                    break;

                case "DELETE":
                    context.Response.Write(context.Request.RequestType);
                    break;

                default:
                    context.Response.Write(context.Request.RequestType);
                    break;

            }
        }

        public bool IsReusable
        {
            get { return true; }
        }

    }
}