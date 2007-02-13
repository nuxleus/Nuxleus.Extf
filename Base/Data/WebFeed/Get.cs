using System;
using Microsoft.Feeds.Interop;

namespace Extf.Net.Data {

    public partial class WebFeedManager {

        // Borrowed from Sam Ruby in whom, I believe, derived his code base from Dave Johnson
        // See http://www.intertwingly.net/blog/2006/03/22/Feed-API-Web-Application for more detail.

        public String GetFeed (String uri) {

            if (!this._IS_INITIALIZED) Init();

            IFeed feed = null;
            if (!this._FM.IsSubscribed(uri)) {
                feed = (IFeed)this._RootFolder.CreateFeed(uri, uri);
            } else {
                feed = (IFeed)this._FM.GetFeedByUrl(uri);
            }

            feed.Download();

            String xml = feed.Xml(feed.ItemCount,
                    FEEDS_XML_SORT_PROPERTY.FXSP_PUBDATE,
                    FEEDS_XML_SORT_ORDER.FXSO_DESCENDING,
                    FEEDS_XML_FILTER_FLAGS.FXFF_ALL,
                    FEEDS_XML_INCLUDE_FLAGS.FXIF_CF_EXTENSIONS);

            return xml;
        }
    }
}