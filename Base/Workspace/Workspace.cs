using System;
using System.Collections.Generic;


namespace Extf.Net {

  public class Workspace : IWorkspace {
    #region IWorkspace Members

    public string title {
      get {
        throw new System.Exception("The method or operation is not implemented.");
      }
      set {
        throw new System.Exception("The method or operation is not implemented.");
      }
    }

    public List<Extf.Net.Data.IAtomCollection> collection {
      get {
        throw new System.Exception("The method or operation is not implemented.");
      }
      set {
        throw new System.Exception("The method or operation is not implemented.");
      }
    }

    #endregion
  }

}
