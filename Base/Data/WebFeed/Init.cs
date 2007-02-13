using System;
using Microsoft.Feeds.Interop;

namespace Extf.Net.Data {

    public partial class WebFeedManager {

       private void Init() {
            this._FM = new FeedsManagerClass();
            this._RootFolder = (IFeedFolder)this._FM.RootFolder;
            this._IS_INITIALIZED = true;
        }

    }
}
