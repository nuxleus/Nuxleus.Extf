using System;
using Extf.Net.Data;
using System.Collections;

namespace Extf.Net.Data {
    public interface IAtomCollection : ICollection {
        String title { get; set; }
        Uri href {get; set; }
        Member.MemberType memberType {get; set;}  
    }
}
