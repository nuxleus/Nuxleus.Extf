// Copyright (c) 2006 by M. David Peterson
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.

using System;
using System.Web;
using System.Threading;
using System.IO;

namespace Xameleon {

    class AsyncSaxonHttpHandler : IHttpAsyncHandler {

        private Transform _transform;
        private TransformServiceAsyncResult _transformAsyncResult;
        private TextWriter _writer;
        private HttpContext _context;
        private String _httpMethod;
        private Exception _exception;


        public void ProcessRequest(HttpContext context) {
            //not called
        }

        public bool IsReusable {
            get { return true; }
        }

        #region IHttpAsyncHandler Members

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {
            _context = context;
            _writer = context.Response.Output;
            _transformAsyncResult = new TransformServiceAsyncResult(cb, extraData);
            _transform = new Transform();
            _httpMethod = context.Request.HttpMethod;

            try {
                DoTransform(cb);
                return _transformAsyncResult;
            } catch (Exception ex) {
                _exception = ex;
                return _transformAsyncResult;
            }
        }

        private void DoTransform(AsyncCallback cb) {

            try {

                switch (_httpMethod) {

                    case "GET": {
                            _transform.Process(_context, _writer, false);
                            _transformAsyncResult.CompleteCall();
                            break;
                        }
                    case "PUT": {
                            _transform.Process(_context, _writer, false);
                            _transformAsyncResult.CompleteCall();
                            break;
                        }
                    case "POST": {
                            _transform.Process(_context, _writer, false);
                            _transformAsyncResult.CompleteCall();
                            break;
                        }
                    case "DELETE": {
                            _transform.Process(_context, _writer, false);
                            _transformAsyncResult.CompleteCall();
                            break;
                        }
                    default: {
                            _transform.Process(_context, _writer, false);
                            _transformAsyncResult.CompleteCall();
                            break;
                        }
                }

            } catch (Exception ex) {
                _exception = ex;
                WriteError();
                _transformAsyncResult.CompleteCall();
            }
        }

        public void EndProcessRequest(IAsyncResult result) {
            _writer.Dispose();
        }

        private void WriteError() {
            _context.Response.Write(_exception.Message);
            _context.Response.Write(_exception.Source);
            _context.Response.Write(_exception.StackTrace);
        }

        #endregion

        class TransformServiceAsyncResult : IAsyncResult {
            private AsyncCallback _cb;
            private object _state;
            private ManualResetEvent _event;
            private bool _completed = false;
            private object _lock = new object();

            public TransformServiceAsyncResult(AsyncCallback cb, object state) {
                _cb = cb;
                _state = state;
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

                if (_cb != null)
                    _cb(this);
            }
        }


    }
}