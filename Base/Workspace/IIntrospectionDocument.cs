using System;

namespace Extf.Net.Data {
    public interface IIntrospectionDocument {
        String workspace { get; }
        IAtomCollection collection { get; }
    }
}
