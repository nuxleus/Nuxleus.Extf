using System;
using System.IO;
using System.Net;
using System.Web;
using Extf.Net;

namespace Extf.Net.Handler.AspNet {

    public class Agent : IHttpHandler, IAgent {

        public Agent () {
        }

        public void ProcessRequest (HttpContext context) {

            //Transform myTransformer = new Transform();
            //myTransformer.Process(context, context.Response.Output);
        }

        public bool IsReusable {
            get { return true; }
        }


        public Stream Request {
            get {
                throw new Exception("The method or operation is not implemented.");
            }
            set {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public Stream Response {
            get {
                throw new Exception("The method or operation is not implemented.");
            }
            set {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public ICredentials Credentials {
            get {
                throw new Exception("The method or operation is not implemented.");
            }
            set {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        Context.IContext IAgent.Context {
            get {
                throw new Exception("The method or operation is not implemented.");
            }
            set {
                throw new Exception("The method or operation is not implemented.");
            }
        }
    }

}