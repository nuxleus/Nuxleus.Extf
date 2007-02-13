using System;
using System.Collections.Generic;
using System.Text;

namespace Extf.Net {
    public interface ICredentials {
        String publicKey {get; set;}
        String privateKey {get; set;}
        Uri location {get; set;}
        ACL Acl {get; set;}
    }
}
