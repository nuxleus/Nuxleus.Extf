using System;
using System.Data;
using System.Linq;
using System.Web;
using System.Collections;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using Xameleon.Function;
using Extf.Net.S3;
using Extf.Net.Configuration;
using System.Collections.Specialized;
using System.Web.Configuration;

namespace Xameleon.Service {
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://sonicradar.com/service/authenticate")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Authenticate : System.Web.Services.WebService {

        [WebMethod(EnableSession = true)]
        public string Authenticate() {
            AWSAuthConnection conn = new AWSAuthConnection(GetSetting("xsltParam_aws-public-key"), GetSetting("xsltParam_aws-private-key"), false);
            return "foo";
        }


        public String GetSetting(String keyName) {

            NameValueCollection appSettings = WebConfigurationManager.AppSettings as NameValueCollection;

            IEnumerator appSettingsEnum = appSettings.GetEnumerator();

            int i = 0;
            try {
                while (appSettingsEnum.MoveNext()) {
                    string key = appSettings.AllKeys[i].ToString();
                    if (key == keyName) {
                        return appSettings[key];
                    }
                    i += 1;
                }
            } catch (Exception e) {
                return e.Message;
            }

            return null;
        }
    }
}

