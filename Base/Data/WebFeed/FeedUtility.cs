// Part of this code base was borrowed from the Microsoft RSS Screen Saveer Sample
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/feedsapi/rss/howto/samp_screensaver.asp
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) 2006 Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using Microsoft.Feeds.Interop;

namespace Extf.Net.Data {

    internal static class FeedUtility {

        public static IEnumerable<IFeed> CommonFeedList (IFeedFolder folder) {
            Queue<IFeedFolder> queue = new Queue<IFeedFolder>();
            queue.Enqueue(folder);
            while (queue.Count > 0) {
                IFeedFolder currentFolder = queue.Dequeue();
                foreach (IFeedFolder subfolder in (IFeedsEnum)currentFolder.Subfolders)
                    queue.Enqueue(subfolder);

                foreach (IFeed feed in (IFeedsEnum)currentFolder.Feeds) {
                    yield return feed;
                }
            }
        }

        public static IEnumerable<IFeed> LastWriteSince (IFeedFolder folder, System.DateTime lastWriteTime) {
            foreach (IFeed feed in CommonFeedList(folder)) {
                if (feed.LastWriteTime.ToLocalTime() > lastWriteTime)
                    yield return feed;
            }
        }

    }
}
