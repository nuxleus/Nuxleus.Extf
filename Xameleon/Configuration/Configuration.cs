// Copyright (c) 2006 by M. David Peterson
// The code contained in this file is licensed under The MIT License
// Please see http://www.opensource.org/licenses/mit-license.php for specific detail.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web.Configuration;

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
