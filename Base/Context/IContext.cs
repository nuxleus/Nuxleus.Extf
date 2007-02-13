using System;
using System.Collections.Generic;
using System.Text;
using Extf.Net.Configuration;
using System.IO;

namespace Extf.Net.Context {
  public interface IContext {
    AppSettings Settings { get; set; }
    Stream Request { get; set; }
    Stream Response { get; set; }
  }
}
