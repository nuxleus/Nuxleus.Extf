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

class PluralizerAttributesInjector : IAttributesInjector {
    class CallablePluralizerHelper : ICallable {
        string _word;
        bool _pluralize;

        public CallablePluralizerHelper(string word, bool pluralize) {
            _word = word;
            _pluralize = pluralize;
        }

        object ICallable.Call(params object[] args) {
            if (args == null || args.Length == 0) {
                if (_pluralize) {
                    return DynamicWebServiceHelpers.Pluralizer.ToPlural(_word);
                }
                else {
                    return DynamicWebServiceHelpers.Pluralizer.ToSingular(_word);
                }
            }
            else if (_pluralize && args.Length == 1 && (args[0] is int)) {
                int count = (int)args[0];

                if (count == 1) {
                    return string.Format("{0} {1}", count, _word);
                }
                else {
                    return string.Format("{0} {1}", count, DynamicWebServiceHelpers.Pluralizer.ToPlural(_word));
                }
            }

            throw new ArgumentException();
        }
    }

    #region IAttributesInjector Members

    List IAttributesInjector.GetAttrNames(object obj) {
        List list = List.MakeEmptyList(0);
        list.Add("ToPlural");
        list.Add("ToSingular");
        return list;
    }

    bool IAttributesInjector.TryGetAttr(object obj, SymbolId nameSymbol, out object value) {
        string name = nameSymbol.ToString();

        switch (name) {
            case "ToPlural":
                value = new CallablePluralizerHelper(obj.ToString(), true);
                return true;

            case "ToSingular":
                value = new CallablePluralizerHelper(obj.ToString(), false);
                return true;

            default:
                value = null;
                return false;
        }
    }

    #endregion
}
