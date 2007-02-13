using System;
using System.Collections.Generic;
using System.Text;

namespace Extf.Net.Operations {

    public interface IOperation {

        bool Create { get; set; }
        bool Edit { get; set; }
        bool Delete { get; set; }
        bool Retrieve { get; }
        bool Update { get; set; }
        bool Copy { get; set; }
        bool Paste { get; set; }
    }
}
