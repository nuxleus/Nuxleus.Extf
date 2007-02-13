using System;
using System.Collections.Generic;
using Extf.Net.Data;

namespace Extf.Net {
    public interface IWorkspace : IStorage {
        String title {get; set;}
        List<IAtomCollection> collection {get; set;}
    }
}
