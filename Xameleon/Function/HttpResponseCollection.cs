using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Web;

namespace Xameleon.Function {

    public class HttpResponseCollection {

        public static void SetValue(HttpResponse response, string type, string key, string value) {
            try {
                switch (type) {
                    case "cookie":
                        HttpCookie cookie = new HttpCookie(key, value);
                        response.Cookies.Set(cookie);
                        break;
                    case "headers":
                        response.Headers.Add(key, value);
                        break;
                    default:
                        break;
                }
            } catch (Exception e) {
                Debug.WriteLine("Error: " + e.Message);
            }
        }
    }
}


