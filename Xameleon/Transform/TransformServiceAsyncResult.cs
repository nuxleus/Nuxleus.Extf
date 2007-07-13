using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web;
using System.IO;
using Memcached.ClientLibrary;

namespace Xameleon.Transform {

  public class TransformServiceAsyncResult : IAsyncResult {

    internal TransformServiceAsyncResult(AsyncCallback cb, Object extraData) {
      this.cb = cb;
      asyncState = extraData;
      isCompleted = false;
    }

    private AsyncCallback cb = null;
    private Object asyncState;
    public object AsyncState {
      get {
        return asyncState;
      }
    }

    public bool CompletedSynchronously {
      get {
        return false;
      }
    }

    // If this object was not being used solely with ASP.Net this
    // method would need an implementation. ASP.Net never uses the
    // event, so it is not implemented here.
    public WaitHandle AsyncWaitHandle {
      get {
        throw new InvalidOperationException(
                  "ASP.Net should never use this property");
      }
    }

    private Boolean isCompleted;
    public bool IsCompleted {
      get {
        return isCompleted;
      }
    }

    internal void CompleteCall() {
      isCompleted = true;
      if (cb != null) {
        cb(this);
      }
    }

    // state internal fields
    internal HttpContext _context = null;
  }
}
