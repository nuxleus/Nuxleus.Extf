using System;
using System.Configuration;
using System.Web.Configuration;
using System.Collections.Specialized;
using System.Collections;

namespace Extf.Net.Configuration
{

    public class AppSettings
    {

        public String GetSetting(String keyName)
        {
            NameValueCollection appSettings = WebConfigurationManager.AppSettings as NameValueCollection;
            IEnumerator appSettingsEnum = appSettings.GetEnumerator();
            int i = 0;

            try
            {
                while (appSettingsEnum.MoveNext())
                {
                    string key = appSettings.AllKeys[i].ToString();
                    if (key == keyName)
                    {
                        return appSettings[key];
                    }
                    i += 1;
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return null;
        }

        public NameValueCollection GetSettingArray(String keyName)
        {
            NameValueCollection appSettings = WebConfigurationManager.AppSettings as NameValueCollection;
            IEnumerator appSettingsEnum = appSettings.GetEnumerator();
            int i = 0;

            NameValueCollection keyArray = new NameValueCollection();

            try
            {
                while (appSettingsEnum.MoveNext())
                {
                    string key = appSettings.AllKeys[i].ToString();
                    if (key.StartsWith(keyName)){
                        keyArray.Add(key.Substring(keyName.Length), appSettings[key]);
                    }
                    i += 1;
                }
            }
            catch (Exception e)
            {
                keyArray.Add("ERROR", e.Message);
                return keyArray;
            }

            return keyArray;
        }
    }
}
