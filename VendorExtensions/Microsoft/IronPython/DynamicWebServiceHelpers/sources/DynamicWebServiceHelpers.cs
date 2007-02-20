/* **********************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This source code is subject to terms and conditions of the Shared Source License
 * for IronPython. A copy of the license can be found in the License.html file
 * at the root of this distribution. If you can not locate the Shared Source License
 * for IronPython, please send an email to ironpy@microsoft.com.
 * By using this source code in any fashion, you are agreeing to be bound by
 * the terms of the Shared Source License for IronPython.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * **********************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

public static class DynamicWebServiceHelpers {
    static WebServiceHelper _webServiceHelper;
    static PluralizerHelper _pluralizerHelper;
    static SimpleXmlHelper _simpleXmlHelper;

    static DynamicWebServiceHelpers() {
        _webServiceHelper = new WebServiceHelper();
        _pluralizerHelper = new PluralizerHelper();
        _simpleXmlHelper = new SimpleXmlHelper();

        Ops.RegisterAttributesInjectorForType(typeof(XmlElement), new XmlElementAttributesInjector(), true);
        Ops.RegisterAttributesInjectorForType(typeof(WebServiceHelper), new WebServiceHelperAttributesInjector(), false);
        Ops.RegisterAttributesInjectorForType(typeof(string), new PluralizerAttributesInjector(), false);
    }

    public static WebServiceHelper WebService {
        get { return _webServiceHelper; }
    }

    public static PluralizerHelper Pluralizer {
        get { return _pluralizerHelper; }
    }

    public static SimpleXmlHelper SimpleXml {
        get { return _simpleXmlHelper; }
    }

    #region Misc Internal Helper Methods

    internal static byte[] GetBytesForUrl(string url) {
        return new WebClient().DownloadData(url);
    }

    internal static string GetStringForUrl(string url) {
        return new WebClient().DownloadString(url);
    }

    internal static XmlElement LoadXmlFromBytes(byte[] xml) {
        XmlDocument doc = new XmlDocument();
        doc.Load(new MemoryStream(xml));
        return doc.DocumentElement;
    }

    internal static XmlElement LoadXmlFromString(string text) {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(text);
        return doc.DocumentElement;
    }

    #endregion
}
