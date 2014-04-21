using com.pingidentity.Security.STS.Client;
using com.pingidentity.Security.Tokens;
using org.iringtools.utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            string pingFederateStsURL = "https://sts.becpsn.com";
            string pingFederateAlternateURL = "https://sso.mypsn.com";
            string pingFederateTokenProcessor = "OAuthTokenSwap";
            string pingFederateAppliesTo = " DevonWayProd";

            // set IdP STS endpoint configuration
            STSClientConfiguration stsClientConfiguration = new STSClientConfiguration();

            // Note that it's possible to send the RST to either sts or sso server by name, and both will respond
            stsClientConfiguration.stsEndpoint = pingFederateStsURL;
            stsClientConfiguration.ignoreSSLTrustErrors = true;
            stsClientConfiguration.systemAuthType = SystemAuthType.NONE;
            stsClientConfiguration.embeddedToken = true;
            stsClientConfiguration.appliesTo = pingFederateAppliesTo;

            STSClient client;
            // instantiate the STS client 
            try {
                client = new STSClient(stsClientConfiguration);
            } catch (Exception e) {
                // in case of a hardcoded endpoint this never happens
                throw new Exception("Malformed Ping Federate STS URL", e);
            }
            // Base64 encode the access_token value
            string oauthRequest = ""; //access token goes here! 
            string encodedToken = Utility.EncodeTo64(oauthRequest);

            BinaryToken   accessToken = new BinaryToken(encodedToken, "urn:pingidentity.com:oauth2:grant_type:validate_bearer");
                                                                    
            // Send in a token and receive the issued SAML assertion
            SecurityToken token;
            try {
                token = client.IssueToken(SecurityToken accessToken);

            } catch (STSClientException e) {
                // deal with the exception
                throw new RuntimeException(e);
            }
                                                                    
            Document document = token.getOwnerDocument();
            DOMImplementationLS domImplLS = (DOMImplementationLS) document.getImplementation();
            LSSerializer serializer = domImplLS.createLSSerializer();
            String str = serializer.writeToString(token);
                                                                    
            System.out.print(str);
                                                                                
        } catch (ParseException e) {
                        e.printStackTrace();
        }

        }
    }
}
