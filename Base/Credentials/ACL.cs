using System;
using System.Collections.Generic;
using System.Xml;
using System.Collections.Specialized;

namespace Extf.Net {

    public class ACL : XmlDocument {

        /// <summary>
        /// Create a new Generics-based Dictionary to hold our group of ACL XML files.
        /// </summary>
        /// 
        // TODO: Create a hash table to hold the ACL's and use this instead of using 
        // a generic Dictionary.

        public ACL() {}

        Dictionary<string, ACL> myACL_List = new Dictionary<string, ACL>();

        
    }
}
