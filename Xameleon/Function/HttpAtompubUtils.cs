using System;
using System.Web;
using System.Xml;
using Xameleon.Llup;

namespace Xameleon.Function {
  public class HttpAtompubUtils {

      public HttpAtompubUtils() { }

    public static string GenerateEntry(HttpContext context) {

      string s = "<qs xmlns='http://purl.oclc.org/DEFUZE/qs' resp='1e582a22b3f82a7c9acc227b0ccb6fdec0d84e6b74dccbcb542e8d11e1b379dd' type='response'><op><push-message /></op><qid>test</qid><mid>3664f5666c2564f7fb6c16d45b1e5f7f951ebff6d7d34792fe4badb63b566ed2</mid><payload>PGxsdXA6bm90aWZpY2F0aW9uIHhtbG5zOmxsdXA9Imh0dHA6Ly93d3cueDJ4Mngub3JnL2xsdXAiIGFjdGlvbj0iIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwNS9BdG9tIj48bGx1cDpyZWNpcGllbnQgaHJlZj0iIiAvPjxsaW5rIGhyZWY9Imh0dHA6Ly9hdG9tcHViLmRlZnV6ZS5vcmcvc3RhcnRlci9zYWxhZC9OYXBvbGl0YW4uYXRvbSIgcmVsPSJzZWxmIiB0eXBlPSJhcHBsaWNhdGlvbi9hdG9tK3htbDt0eXBlPWVudHJ5IiAvPjxjYXRlZ29yeSB0ZXJtPSJpbmRpZSIgLz48Y2F0ZWdvcnkgdGVybT0icm9jayIgLz48Y2F0ZWdvcnkgdGVybT0iamF6eiIgLz48Y2F0ZWdvcnkgdGVybT0iYmx1ZXMiIC8+PC9sbHVwOm5vdGlmaWNhdGlvbj4=</payload></qs>";

      Notification n = Notification.Parse(s);
      return "";
    }
  }
}
