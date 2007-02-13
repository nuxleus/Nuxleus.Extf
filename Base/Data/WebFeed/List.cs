// Portions of this codebase is derived from the RSS ScreenSaver sample FeedList.cs file 
// which contains the following licensing and copyright information:
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) 2006 Microsoft Corporation. All rights reserved.

using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Microsoft.Feeds.Interop;

namespace Extf.Net.Data {

    public class List {
        private List<WebFeed> webFeeds;
        private int currentFeedIndex;
        private int currentItemIndex;
        private DateTime lastRefresh;

        internal readonly static List<string> imageExtensions = new List<string>(
            new string[] { "*.BMP", "*.GIF", "*.PNG", "*.JPG", "*.JPEG" });

        public List () {
            lastRefresh = DateTime.MinValue;
            currentFeedIndex = -1;
            currentItemIndex = -1;
        }

        public void Refresh () {
            if (lastRefresh == DateTime.MinValue) {
                // we never successfully loaded the CFL, so let's do it again
                Load();
                return;
            }

            IFeedsManager fs = new FeedsManagerClass();

            try {
                IFeedFolder folder = (IFeedFolder)fs.GetFolder(Properties.Settings.Default.ImagePathOverride);
                foreach (IFeed feed in FeedUtility.LastWriteSince(folder, lastRefresh)) {
                    WebFeed rssFeed = null;
                    try {
                        // This feed was updated or is new, let's get it.
                        rssFeed = WebFeed.FromApi(feed);
                    } catch (System.Runtime.InteropServices.COMException ex) {
                        System.Diagnostics.Debug.Print("Failed to get RSS feed '{0}' from API; skipping feed. Error: {1} ", feed.Name, ex.ToString());
                        continue;  // Skip this feed.
                    }

                    // If the feed has no items with picture enclosures then skip it.
                    if (rssFeed == null || rssFeed.Items.Count == 0) {
                        System.Diagnostics.Debug.Print("Feed '{0}' does not have any picture enclosures; skipping feed.", feed.Name);
                        continue;
                    }

                    // Before we add it let's see if we have an old version of the feed.
                    int index = webFeeds.FindIndex(delegate(WebFeed f) { return (f.Path == rssFeed.Path); });
                    if (index == -1) {
                        // This must be a new feed, let's append it to the list.
                        webFeeds.Add(rssFeed);
                    } else {
                        // We have an existing feed with the same path. Let's insert it 
                        // where the previous feed is at.
                        webFeeds.Insert(index, rssFeed);

                        // Remove previous feed.
                        webFeeds.RemoveAt(index + 1);

                        // Assure that current indexes are not out of bounds.
                        ValidateIndexes();
                    }
                }
            } finally {
                GC.Collect(); // Release all COM objects and their file handles.
                lastRefresh = DateTime.Now;
            }
        }


        public void Load () {
            DateTime loadstart = DateTime.Now;
            List<WebFeed> newRssFeeds = new List<WebFeed>();

            try {
                IFeedsManager fs = new FeedsManagerClass();
                IFeedFolder folder = (IFeedFolder)fs.GetFolder(Properties.Settings.Default.ImagePathOverride);
                foreach (IFeed feed in FeedUtility.CommonFeedList(folder)) {
                    System.Diagnostics.Debug.Print("Found feed {0} with {1} items.",
                        feed.Name, ((IFeedsEnum)feed.Items).Count);
                    try {
                        WebFeed rssFeed = WebFeed.FromApi(feed);

                        // Only add this feed if it contains items
                        if (rssFeed != null) {
                            System.Diagnostics.Debug.Print("Feed has {0} items with enclosures.", rssFeed.Items.Count);
                            if (rssFeed.Items.Count > 0)
                                newRssFeeds.Add(rssFeed);
                        } else
                            System.Diagnostics.Debug.Print("Feed is null.");
                    } catch (System.Runtime.InteropServices.COMException ex) {
                        System.Diagnostics.Debug.Print("Failed to get RSS feed '{0}' from API; skipping feed. Error: {1} ", feed.Name, ex.ToString());
                        // Ignore exception, meaning ignore this feed and continue with next feed.
                    }
                }
            } finally {
                // Collect garbage so that all the COM objects are released which 
                // closes the backing structured storage files.
                GC.Collect();
            }

            if (newRssFeeds.Count == 0) {
                // There were no suitable feeds, hence get default feeds from resources.
                System.Diagnostics.Debug.Print("There were no suitable feeds, hence get default feeds from resources.");
                WebFeed rssFeed = WebFeed.FromText(Properties.Resource.DefaultRSSText);
                newRssFeeds.Add(rssFeed);
            }

            this.webFeeds = newRssFeeds;
            // reset current indexes
            currentFeedIndex = -1;
            currentItemIndex = -1;
            MoveNext();
            lastRefresh = loadstart;
        }

        public int Count { get { return webFeeds.Count; } }

        public WebFeed GetFeed (int index) {
            return webFeeds[index];
        }

        public WebFeed CurrentFeed { get { return webFeeds[currentFeedIndex]; } }
        public int CurrentFeedIndex { get { return currentFeedIndex; } }
        public Item CurrentItem { get { return webFeeds[currentFeedIndex].Items[currentItemIndex]; } }
        public int CurrentItemIndex { get { return currentItemIndex; } }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method is time consuming since it accesses the file system.")]
        //public System.Drawing.Image CurrentImage {
        //    get {
        //        if (currentImage == null) {
        //            System.IO.FileInfo file = rssFeeds[currentFeedIndex].Items[currentItemIndex].Enclosure;
        //            if (file != null) {
        //                // load from resource if resource url
        //                if (ScreenSaverForm.IsResourcePictureFilename(file.Name)) {
        //                    switch (new Random().Next(2)) {
        //                        case 0:
        //                            currentImage = Properties.Resources.SSaverBackground;
        //                            break;
        //                        case 1:
        //                            currentImage = Properties.Resources.SSaverBackground2;
        //                            break;
        //                    }
        //                } else {
        //                    // verify file's existence, else keep current image
        //                    file.Refresh();
        //                    if (file.Exists)
        //                        currentImage = System.Drawing.Image.FromFile(file.FullName);
        //                }
        //            }
        //        }

        //        return currentImage;
        //    }
        //}

        public void MoveNext () {
            if (webFeeds == null || webFeeds.Count == 0)
                return;

            // Clear the cached current image 
            //currentImage = null;

            // If we don't have a currentFeed then pick a feed and image
            if (currentFeedIndex == -1) {
                // Pick random feed & image
                //currentFeedIndex = new Random().Next(rssFeeds.Count);
                //currentItemIndex = new Random().Next(rssFeeds[currentFeedIndex].Items.Count);

                // Pick the first feed & image
                currentFeedIndex = 0;
                currentItemIndex = 0;
            } else {
                // Increment next item
                currentItemIndex++;

                ValidateIndexes();
            }
        }

        public void MovePrevious () {
            if (webFeeds == null || webFeeds.Count == 0)
                return;

            // Clear the cached current image 
            //currentImage = null;

            currentItemIndex--;

            if (currentItemIndex < 0) {
                // Return to the previous feed
                currentFeedIndex--;

                if (currentFeedIndex < 0)
                    currentFeedIndex = webFeeds.Count - 1;

                currentItemIndex = webFeeds[currentFeedIndex].Items.Count - 1;
            }
        }

        private void ValidateIndexes () {
            // If we are at the end of the feed jump to the next feed
            if (currentItemIndex >= webFeeds[currentFeedIndex].Items.Count) {
                currentItemIndex = 0;

                // We also need to go to the next feed
                currentFeedIndex++;

                // If we are at the end of the feeds jump to the first feed
                if (currentFeedIndex >= webFeeds.Count) {
                    currentFeedIndex = 0;
                }
            }
        }
    }
}
