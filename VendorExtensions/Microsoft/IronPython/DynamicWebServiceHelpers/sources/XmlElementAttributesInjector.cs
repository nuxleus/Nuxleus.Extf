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
using System.Text;
using System.Xml;

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

class XmlElementAttributesInjector : IAttributesInjector {
    #region IAttributesInjector Members

    List IAttributesInjector.GetAttrNames(object obj) {
        List list = List.MakeEmptyList(0);
        XmlElement xml = obj as XmlElement;

        if (xml != null) {
            foreach (XmlAttribute attr in xml.Attributes) {
                if (!list.Contains(attr.Name)) {
                    list.Add(attr.Name);
                }
            }

            for (XmlNode n = xml.FirstChild; n != null; n = n.NextSibling) {
                if (n is XmlElement) {
                    list.Add(n.Name);
                }
            }
        }

        return list;
    }

    bool IAttributesInjector.TryGetAttr(object obj, SymbolId nameSymbol, out object value) {
        string name = nameSymbol.ToString();
        XmlElement xml = obj as XmlElement;

        if (xml != null) {
            XmlAttribute attr = xml.Attributes[name];
            if (attr != null) {
                value = attr.Value;
                return true;
            }

            for (XmlNode n = xml.FirstChild; n != null; n = n.NextSibling) {
                if (n is XmlElement && string.CompareOrdinal(n.Name, name) == 0) {
                    if (n.HasChildNodes && n.FirstChild == n.LastChild &&
                        n.FirstChild is XmlText) {
                        value = n.InnerText;
                    }
                    else {
                        value = n;
                    }

                    return true;
                }
            }

            // see if they ask for pluralized element - return array in that case
            string singularName = null;
            List<XmlNode> elementList = null;

            for (XmlNode n = xml.FirstChild; n != null; n = n.NextSibling) {
                if (n is XmlElement) {
                    if (singularName == null) {
                        if (DynamicWebServiceHelpers.Pluralizer.IsNounPluralOfNoun(name, n.Name)) {
                            singularName = n.Name;
                            elementList = new List<XmlNode>();
                            elementList.Add(n);
                        }
                    }
                    else if (string.CompareOrdinal(n.Name, singularName) == 0) {
                        elementList.Add(n);
                    }
                }
            }

            if (elementList != null) {
                value = elementList.ToArray();
                return true;
            }
        }

        value = null;
        return false;
    }

    #endregion
}
