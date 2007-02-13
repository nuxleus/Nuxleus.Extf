using System;
using System.IO;
using Extf.Net.Context;

namespace Extf.Net {

    public interface IAgent {

        Stream Request {get; set;}
        Stream Response {get; set;}
        ICredentials Credentials {get; set;}
        IContext Context {get; set;}

    }
}
