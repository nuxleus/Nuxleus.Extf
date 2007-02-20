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

using IronPython.Runtime;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Operations;

class WebServiceHelperAttributesInjector : IAttributesInjector {
    class CallableLoadHelper : ICallable {
        string _providerName;

        public CallableLoadHelper(string providerName) {
            _providerName = providerName;
        }

        object ICallable.Call(params object[] args) {
            if (args != null && args.Length == 1 && (args[0] is string)) {
                return DynamicWebServiceHelpers.WebService.Load(_providerName, (string)args[0]);
            }

            throw new ArgumentException();
        }
    }

    #region IAttributesInjector Members

    List IAttributesInjector.GetAttrNames(object obj) {
        List<string> providerNames = DynamicWebServiceHelpers.WebService.GetProviderNames();
        List attrNames = List.MakeEmptyList(providerNames.Count);

        foreach (string providerName in providerNames) {
            attrNames.Add("Load" + providerName);
        }

        return attrNames;
    }

    bool IAttributesInjector.TryGetAttr(object obj, SymbolId nameSymbol, out object value) {
        string name = nameSymbol.ToString();

        if (name.StartsWith("Load", StringComparison.Ordinal)) {
            name = name.Substring("Load".Length);

            if (DynamicWebServiceHelpers.WebService.IsValidProviderName(name)) {
                value = new CallableLoadHelper(name);
                return true;
            }
        }

        value = null;
        return false;
    }

    #endregion
}
