// Copyright (c) 2006 by M. David Peterson
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.

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
                //case "GET":
                //    context.Response.Write(myTransformer.Go(context).ToString());
                //    break;

                //case "POST":
                //    context.Response.Write(context.Request.RequestType);
                //    break;

                //case "PUT":
                //    context.Response.Write(context.Request.RequestType);
                //    break;

                //case "DELETE":
                //    context.Response.Write(context.Request.RequestType);
                //    break;

                //default:
                //    context.Response.Write(context.Request.RequestType);
                //    break;

            }
        }

        public bool IsReusable
        {
            get { return true; }
        }

    }
}