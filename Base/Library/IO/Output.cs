using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Extf.Net.IO {

    public class Output : Stream {

        public Output() {}

        public override bool CanRead {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override bool CanSeek {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override bool CanWrite {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override void Flush () {
            throw new Exception("The method or operation is not implemented.");
        }

        public override long Length {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override long Position {
            get {
                throw new Exception("The method or operation is not implemented.");
            }
            set {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public override int Read (byte[] buffer, int offset, int count) {
            throw new Exception("The method or operation is not implemented.");
        }

        public override long Seek (long offset, SeekOrigin origin) {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void SetLength (long value) {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void Write (byte[] buffer, int offset, int count) {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
