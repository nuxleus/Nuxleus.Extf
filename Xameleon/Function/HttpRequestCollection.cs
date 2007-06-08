using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Web;

namespace Xameleon.Function {

    public class HttpRequestCollection {

        static string notSet = "not-set";

        public static string GetValue(HttpRequest request, string type, string key) {
            try {
                switch (type) {
                    case "cookie":
                        if (request.Cookies.Count > 0) {
                            IEnumerator enumerator = request.Cookies.GetEnumerator();
                            for (int i = 0; enumerator.MoveNext(); i++) {
                                string local = request.Cookies.AllKeys[i].ToString();
                                if (local == key) {
                                    return request.Cookies[local].Value;
                                    break;
                                }
                            }
                            return notSet;
                        }
                        return notSet;
                        break;

                    case "form":
                        if (request.Form.Count > 0) {
                            IEnumerator enumerator = request.Form.GetEnumerator();
                            for (int i = 0; enumerator.MoveNext(); i++) {
                                string local = request.Form.AllKeys[i].ToString();
                                if (local == key) {
                                    return request.Form[local];
                                    break;
                                }
                            }
                            return notSet;
                        }
                        return notSet;
                        break;

                    case "query-string":
                        if (request.QueryString.Count > 0) {
                            IEnumerator enumerator = request.QueryString.GetEnumerator();
                            for (int i = 0; enumerator.MoveNext(); i++) {
                                string local = request.QueryString.AllKeys[i].ToString();
                                if (local == key) {
                                    return request.QueryString[local];
                                    break;
                                }
                            }
                            return notSet;
                        }
                        return notSet;
                        break;

                    default:
                        return notSet;
                        break;
                }
                
            } catch (Exception e) {
                Debug.WriteLine("Error: " + e.Message);
                return e.Message;
            }

            return notSet;
        }
    }
}

