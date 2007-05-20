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
            new Transform().Process(context, context.Response.Output);
        }

        public bool IsReusable
        {
            get { return true; }
        }

    }
}