using System;
using System.Data;
using System.Web;
using System.Collections;
using System.Web.Services;
using System.Web.Services.Protocols;
using Xameleon.Function;
using Extf.Net.S3;
using System.Collections.Specialized;
using System.Web.Configuration;

namespace Xameleon.Service {
    /// <summary>
    /// Summary description for Authenticate service
    /// </summary>
    [WebService(Namespace = "http://xameleon.org/service")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Authenticate : WebService {

        static string pubKey = GetSetting("xsltParam_aws-public-key");
        static string privKey = GetSetting("xsltParam_aws-private-key");
        AWSAuthConnection conn = new AWSAuthConnection(pubKey, privKey, false);

        [WebMethod(EnableSession = true)]
        public bool CheckAuthentication(string nonce) {
            return false;
        }

        private string GenerateNonce(string sessionKey) {
            return "foo";
        }


        public static String GetSetting(String keyName) {

            NameValueCollection appSettings = WebConfigurationManager.AppSettings as NameValueCollection;

            IEnumerator appSettingsEnum = appSettings.GetEnumerator();

            try {
                for (int i = 0; appSettingsEnum.MoveNext(); i++) {
                    string key = appSettings.AllKeys[i].ToString();
                    if (key == keyName) {
                        return appSettings[key];
                    }
                }
            } catch (Exception e) {
                return e.Message;
            }

            return null;
        }
    }
}

