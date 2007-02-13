using System;
using System.Configuration;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Xml;
using System.IO;
using System.Collections;

namespace Saxon.Ext {

  internal class ResourceManager {

    internal XmlReader GetSetting (String keyName) {

      NameValueCollection appSettings = ConfigurationSettings.AppSettings as NameValueCollection;

      IEnumerator appSettingsEnum = appSettings.GetEnumerator();

      int i = 0;
      try {
        while (appSettingsEnum.MoveNext()) {
          string key = appSettings.AllKeys[i].ToString();
          if (key == keyName) {
            return XmlReader.Create(new StringReader(appSettings[key]));
          }
          i += 1;
        }
      } catch (Exception e) {
        return XmlReader.Create(new StringReader(@"<exception>" + e.Message + "</exception>"));
      }
      return null;
    }
  }
}

