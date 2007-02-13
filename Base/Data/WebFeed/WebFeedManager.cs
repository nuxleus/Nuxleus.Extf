using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Feeds.Interop;

namespace Extf.Net.Data {

    public partial class WebFeedManager {

        private bool _IS_INITIALIZED = false;
        private List _FeedList;
        private FeedsManager _FM;
        private IFeedFolder _RootFolder;

        public WebFeedManager () { }
    }
}
