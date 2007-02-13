using System;
using System.IO;
using Extf.Net;
using Extf.Net.IO;

namespace Extf.Net {

    public class Agent : IAgent {

        // This will be our only way in and out of the Extf.Net system.
        // All requests for data, operations, etc... must first come through 
        // the agent.  The agent will view the credentials contained in the
        // Request stream and then use these credentials to determine if the 
        // the requestor has the proper accreditation to make such a request.
        // If yes, the operation will be processed and the resulting message 
        // returned.
        //
        // If no, a return message suggesting in no uncertain terms that the 
        // credentials provided do not validate for the request being made.
        //

        public Agent() {}

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
