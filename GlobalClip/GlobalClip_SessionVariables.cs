using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using com.amazon.s3;

namespace Extf.Net
{
    public partial class GlobalClip
    {
        private AWSAuthConnection _Connect;
        private string _BaseHost;
        private string _StorageBase;
        private string _FilePrefix;
        private string _Guid;
        private ClipboardCollection<ClipItem> _ClipCopy;
        private ClipboardCollection<ClipItem> _ClipPaste;
        public string debugPublicKey;
        public string debugPrivateKey;

        NameValueCollection appSet_Private_Public_Keys = ConfigurationManager.AppSettings;

    }
}
