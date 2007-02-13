using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Extf.Net;

namespace Extf.Net.Transformer {

    class Agent : IAgent {

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
