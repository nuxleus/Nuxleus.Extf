using System;
using Extf.Net;
using Extf.Net.Configuration;
using System.IO;


namespace Extf.Net.Process {

    public class Agent : IAgent {

        public Agent () { }

        private void Init() {
            
        }

        public System.IO.Stream Request {
            get {
                throw new Exception("The method or operation is not implemented.");
            }
            set {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public System.IO.Stream Response {
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
