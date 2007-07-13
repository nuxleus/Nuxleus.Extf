using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web;
using System.IO;
using Memcached.ClientLibrary;

namespace Xameleon.Transform {

  public class TransformServiceAsyncResult : IAsyncResult {
    AsyncCallback _callback;
    ManualResetEvent _event;
    HttpContext _context;
    bool _completed = false;
    object _lock = new object();
    object _state;

    public TransformServiceAsyncResult(AsyncCallback callback, object state) {
      _callback = callback;
      _state = state;
      _completed = false;
    }

    public Object AsyncState { get { return _state; } }

    public bool CompletedSynchronously { get { return false; } }

    public bool IsCompleted { get { return _completed; } }

    public WaitHandle AsyncWaitHandle {
      get {
        lock (_lock) {
          if (_event == null)
            _event = new ManualResetEvent(IsCompleted);
          return _event;
        }
      }
    }

    public void CompleteCall() {
      lock (_lock) {
        _completed = true;
        if (_event != null)
          _event.Set();
      }

      if (_callback != null)
        _callback(this);
    }
  }
}
