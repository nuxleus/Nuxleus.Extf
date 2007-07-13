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
    Transform _transform;
    Context _transformContext;
    TextWriter _writer;
    MemcachedClient _memcachedClient;
    bool _useMemcachedClient;
    bool _completed = false;
    object _lock = new object();
    object _state;

    public TransformServiceAsyncResult(AsyncCallback callback, HttpContext context, Transform transform, object state) {
      _callback = callback;
      _context = context;
      _state = state;
      _transform = transform;
      _completed = false;
    }

    public Object AsyncState { get { return _state; } }

    public bool CompletedSynchronously { get { return false; } }

    public bool IsCompleted { get { return _completed; } }

    public void StartAsyncTransformWork(Context context, MemcachedClient memcached, bool useMemcachedClient) {
      _transformContext = context;
      _memcachedClient = memcached;
      _useMemcachedClient = useMemcachedClient;
      ThreadPool.QueueUserWorkItem(new WaitCallback(StartAsyncTransformTask), null);
    }

    private void StartAsyncTransformTask(Object workItemState) {
      _transform.BeginAsyncProcess(_transformContext);
      if (_transformContext.IsInitialized) {
        string output = _transformContext.StringBuilder.ToString();
        _context.Response.Write(output);
        if (_useMemcachedClient) {
          _memcachedClient.Set(_transformContext.RequestUriHash, output);
        }
      }
      _completed = true;
      _callback(this);
    }

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
