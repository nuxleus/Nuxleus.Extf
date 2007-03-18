using System;
using System.Collections.Generic;
using System.Text;

class AtomProvider : IWebServiceProvider {
    #region IWebServiceProvider Members

    string IWebServiceProvider.Name {
        get { return "Atom"; }
    }

    bool IWebServiceProvider.MatchUrl(string url) {
        return url.EndsWith(".atom", StringComparison.OrdinalIgnoreCase) ||
               url.EndsWith(".feed", StringComparison.OrdinalIgnoreCase) ||
               url.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)  ||
               url.IndexOf("/collection/", StringComparison.OrdinalIgnoreCase) >= 0 ||
               url.IndexOf("/feed.", StringComparison.OrdinalIgnoreCase) >= 0 ||
               url.IndexOf("/atom.", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    object IWebServiceProvider.LoadWebService(string url) {
        byte[] atomBytes = DynamicWebServiceHelpers.GetBytesForUrl(url);
        return DynamicWebServiceHelpers.LoadXmlFromBytes(atomBytes);
    }

    #endregion
}
