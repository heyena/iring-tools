
using System;
using System.Runtime.Serialization;
using System.Net;

namespace org.iringtools.utility
{
    [DataContract]
    public class SecurityConfigCredential
    {
        //protected static string _delimiter = "{`|";
        protected static string _delimiter = "_";
        protected static string[] _delimiterArray = { _delimiter };

        public SecurityConfigCredential()
        {
            isEncrypted = false;
        }

        public SecurityConfigCredential(string encryptedCredentials)
        {
            if (encryptedCredentials != String.Empty && encryptedCredentials != null)
            {
                encryptedToken = encryptedCredentials;
                isEncrypted = true;
            }
            else
            {
                isEncrypted = false;
            }
        }

        public virtual void Encrypt()
        {
            string conString = dataSource + _delimiter + initialCatalog + _delimiter + userId +_delimiter + password;
            
            //encryptedToken = Encryption.EncryptString(conString);
            encryptedToken = EncryptionUtility.Encrypt(conString);
            dataSource = null;
            initialCatalog = null;
            userId = null;
            password = null;

            isEncrypted = true;
        }

        public virtual void Decrypt()
        {
            string credentials = string.Empty;

            try
            {
                credentials = EncryptionUtility.Decrypt(encryptedToken);
            }
            catch
            {
                credentials = Encryption.DecryptString(encryptedToken);
            }

            string[] credentialsArray = credentials.Split(_delimiterArray, StringSplitOptions.None);

            dataSource = credentialsArray[0];
            initialCatalog = credentialsArray[1];
            userId = credentialsArray[2];
            password = credentialsArray[3];
            encryptedToken = null;
            isEncrypted = false;
        }

        public NetworkCredential GetNetworkCredential()
        {
            NetworkCredential credentials = null;

            //if (this.userName != string.Empty && this.userName != null)
            //{
            //    credentials = new NetworkCredential(userName, password, domain);
            //}
            //else
            //{
            //    credentials = CredentialCache.DefaultNetworkCredentials;
            //}

            return credentials;
        }

        [DataMember(EmitDefaultValue = false)]
        public bool isEncrypted { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string dataSource { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string initialCatalog { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string userId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string password { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string encryptedToken { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string domain { get; set; }
    }

    
}