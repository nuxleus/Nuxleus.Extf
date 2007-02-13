using System;
using System.Collections.Generic;
using System.Text;
using Extf.Net;
using System.IO;
using System.Xml;
using Microsoft.Feeds.Interop;
using System.Text.RegularExpressions;

namespace Extf.Net.Data {
    public class Item : IMember {
        private readonly string title;
        private readonly string description;
        private readonly string link;
        private readonly FileInfo enclosure;

        public string Title { get { return title; } }
        public string Description { get { return description; } }
        public string Link { get { return link; } }
        public FileInfo Enclosure { get { return enclosure; } }

        /// <summary>
        /// Build an RSSItem from an XmlNode representing an Item element inside an RSS 2.0 XML document.
        /// </summary>
        /// <param name="itemNode"></param>
        internal Item (XmlNode itemNode) {
            XmlNode selected;
            selected = itemNode.SelectSingleNode("title");
            if (selected != null) {
                title = selected.InnerText;
            }

            selected = itemNode.SelectSingleNode("description");
            if (selected != null)
                description = selected.InnerText;

            selected = itemNode.SelectSingleNode("link");
            if (selected != null)
                link = selected.InnerText;

            selected = itemNode.SelectSingleNode("enclosure/@url");
            if (selected != null) {
                string filename = selected.InnerText;

                enclosure = new FileInfo(filename);
            }
        }

        internal Item (IFeedItem entry) {
            title = StripHTMLTags(entry.Title);
            link = entry.Link;

            try {
                description = StripHTMLTags(entry.Description);
            } catch (ArgumentException) {
                description = "<<<Item text too long.>>>";
            }

            IFeedEnclosure enclosure = (IFeedEnclosure)entry.Enclosure;
            if (enclosure != null) {
                string filename = GetLocalPath(enclosure);

                try {
                    // Let's make sure we only add pictures
                    string extension = "*" + Path.GetExtension(filename);
                    extension = extension.ToUpperInvariant();
                    if (List.imageExtensions.Contains(extension)) {
                        // Set enclosure without checking for existence, since enclosure might
                        // not be on disk yet (BITS still getting it)
                        this.enclosure = new FileInfo(filename);
                    }
                } catch (ArgumentException) {
                    System.Diagnostics.Debug.Print("Illegal character in filename.");
                    // 'Illegal character in filename' is one possible exception.
                    // Ignore exception; since we'll skip rssItems with this.enclosure == null
                    // higher up in the call chain.
                } catch (NotSupportedException) {
                    System.Diagnostics.Debug.Print("The given path's format is not supported.");
                    // 'Message="The given path's format is not supported."' is another possible exception.
                    // Ignore exception; since we'll skip rssItems with this.enclosure == null
                    // higher up in the call chain.
                }
            }
        }

        private IFeedEnclosure GetEnclosure(IFeedItem item)
        {
            try
            {
                System.Diagnostics.Debug.Print("Enclosure is null={0}",item.Enclosure == null);

                return (IFeedEnclosure)item.Enclosure;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                if (ex.ErrorCode == -2147023728)    
                {
                    // "Element not found. (Exception from HRESULT: 0x80070490)"
                    // ignore exception
                    System.Diagnostics.Debug.Print("Enclosure element not found exception.");
                    return null;
                }
                else
                    throw;
            }
        }

        private static string GetLocalPath (IFeedEnclosure encl) {
            try {
                return encl.LocalPath;
            } catch (System.Runtime.InteropServices.COMException ex) {
                if (ex.ErrorCode == -2147024735) {
                    System.Diagnostics.Debug.Print("Enclosure localPath is invalid.");
                    // "The specified path is invalid. (Exception from HRESULT: 0x800700A1)"
                    // ignore exception
                } else
                    throw;
            } catch (FileNotFoundException) {
                System.Diagnostics.Debug.Print("Enclosure localPath element not found exception.");
                // ignore
            }
            return null;
        }

        private static string StripHTMLTags (string input) {
            if (null != input) {
                Regex rgx = new Regex("<[^>]*>", RegexOptions.Multiline);
                return rgx.Replace(input, "");
            }

            return null;
        }
    }
}
