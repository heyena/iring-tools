using System;
using System.Text;
using System.Configuration;
using org.iringtools.utility;
using System.DirectoryServices;
using System.IO;
using System.DirectoryServices.Protocols;
using System.Collections;
using System.Net;
using log4net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Data;


namespace org.iringtools.utility
{
    public static class ConfigurationRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ConfigurationRepository));
        public static string server = string.Empty;
        public static int portNumber = 0;
        public static string userName = string.Empty;
        public static string password = string.Empty;
        public static string groupDN = "ou=configurations,o=iringtools,dc=iringug,dc=org";
        private static object _lockObj = new Object();
        public static List<Type> configTypes = new List<Type>(); //It will contain the type of objects which you want to
        // keep in LDAP.  


        public static void GetAppSettings()
        {
            server = ConfigurationManager.AppSettings["ldapserver"];
            portNumber = int.Parse(ConfigurationManager.AppSettings["ldapportnumber"]);
            userName = ConfigurationManager.AppSettings["ldapusername"];
            string tmpPassword = ConfigurationManager.AppSettings["ldappassword"];
            string keyFile = ConfigurationManager.AppSettings["keyfile"];
            password = EncryptionUtility.Decrypt(tmpPassword, keyFile);
        }

        /// <summary>
        /// Save Binary File in Ldap.
        /// </summary>
        /// <param name="binaryStream">Memory stream of Object and filePath</param>
        public static bool SaveFile<T>(Stream stream, string filePath)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);
                if (fileName.ToLower().StartsWith("configuration")) // Configuration is of dictionary type so to avoid that.
                {
                    return false;
                }

                lock (_lockObj)
                {
                    bool isFileExist = FileExists(fileName);
                    if (isFileExist) // If file exist, delete it and save a new one. 
                    {
                        DeleteEntry(filePath);
                    }

                    string path = "LDAP://" + server + ":" + portNumber.ToString() + "/" + groupDN;
                    DirectoryEntry container = new DirectoryEntry(path);
                    container.AuthenticationType = AuthenticationTypes.None;
                    container.Username = userName;
                    container.Password = password;

                    using (container)
                    {
                        byte[] byteArray = ((System.IO.MemoryStream)stream).ToArray();

                        DirectoryEntry entry = container.Children.Add("cn=" + fileName, "javacontainer");
                        entry.Properties["objectclass"].Add("javaserializedobject");
                        entry.Properties["objectclass"].Add("javacontainer");
                        entry.Properties["objectclass"].Add("javaobject");
                        entry.Properties["javaclassname"].Add(typeof(T).FullName);
                        entry.Properties["javaserializeddata"].Add(byteArray);
                        entry.CommitChanges();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in saving file to LDAP server: " + ex);
                throw ex;
            }
        }


        /// <summary>
        /// Retrive the binary file from Ldap
        /// </summary>
        public static Stream GetFile<T>(string filePath)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);
                if (fileName.ToLower().StartsWith("configuration")) // Configuration is of dictionary type so to avoid that.
                {
                    return null;
                }

                string filter = "(cn=" + fileName + ")";
                SearchRequest request = new SearchRequest
                {
                    DistinguishedName = groupDN,
                    Filter = filter,
                    Scope = System.DirectoryServices.Protocols.SearchScope.Subtree,
                };

                LdapConnection ldapConnection = Connect();

                if (ldapConnection != null)
                {
                    SearchResponse response = (SearchResponse)ldapConnection.SendRequest(request);
                    UTF8Encoding utf8 = new UTF8Encoding(false, true);

                    if (response.Entries.Count > 0)
                    {
                        SearchResultEntryCollection entries = response.Entries;
                        for (int i = 0; i < entries.Count; i++)
                        {
                            SearchResultEntry entry = entries[i];
                            IDictionaryEnumerator attribEnum = entry.Attributes.GetEnumerator();
                            while (attribEnum.MoveNext())
                            {
                                DirectoryAttribute subAttrib = (DirectoryAttribute)attribEnum.Value;
                                for (int ic = 0; ic < subAttrib.Count; ic++)
                                {
                                    attribEnum.Key.ToString();
                                    if (attribEnum.Key.ToString().ToLower() == "javaserializeddata")
                                    {
                                        if (subAttrib[ic] is byte[])
                                        {
                                            MemoryStream memStream = new MemoryStream();
                                            BinaryFormatter binForm = new BinaryFormatter();
                                            memStream.Write(subAttrib[ic] as byte[], 0, (subAttrib[ic] as byte[]).Length);
                                            memStream.Seek(0, SeekOrigin.Begin);
                                            return memStream;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in retrieving file from LDAP server: " + ex);
                throw ex;
            }
        }

        /// <summary>
        /// Delete the file from Ldap.
        /// </summary>
        public static void DeleteEntry(string filePath)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);
                if (fileName.ToLower().StartsWith("configuration")) // Configuration is of ditionary type so to avoid that.
                {
                    return;
                }

                lock (_lockObj)
                {
                    bool isFileExist = FileExists(fileName);
                    if (isFileExist) // Make sure file should exist before deleting.
                    {
                        string filter = "cn=" + fileName;
                        string parentPath = "LDAP://" + server + ":" + portNumber.ToString() + "/" + groupDN;
                        string childPath = "LDAP://" + server + ":" + portNumber.ToString() + "/" + filter + "," + groupDN;

                        //System.DirectoryServices.DirectoryEntry entry = new System.DirectoryServices.DirectoryEntry("LDAP://irtsvc-directory-staging-chi.becpsn.com:389/ou=configurations,o=iringtools,dc=iringug,dc=org");
                        DirectoryEntry entry = new DirectoryEntry(parentPath);
                        entry.AuthenticationType = AuthenticationTypes.None;
                        entry.Username = userName;
                        entry.Password = password;

                        //System.DirectoryServices.DirectoryEntry entryToRemove = new System.DirectoryServices.DirectoryEntry("LDAP://irtsvc-directory-staging-chi.becpsn.com:389/CN=test87.xml,ou=configurations,o=iringtools,dc=iringug,dc=org");
                        DirectoryEntry entryToRemove = new DirectoryEntry(childPath);
                        entryToRemove.AuthenticationType = AuthenticationTypes.None;
                        entryToRemove.Username = userName;
                        entryToRemove.Password = password;

                        entry.Children.Remove(entryToRemove);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error in deleting file from LDAP server: " + ex);
                throw ex;
            }
        }

        public static LdapConnection Connect()
        {
            LdapConnection ldapConnection = null;

            try
            {
                LdapDirectoryIdentifier ldapIdentifier = new LdapDirectoryIdentifier(server, portNumber, true, false);
                ldapConnection = new LdapConnection(ldapIdentifier);
                ldapConnection.Credential = new NetworkCredential(userName, password);
                ldapConnection.AuthType = AuthType.Basic;
                LdapSessionOptions options = ldapConnection.SessionOptions;
                options.ProtocolVersion = 3;
                ldapConnection.Bind();
                return ldapConnection;
            }
            catch (Exception e)
            {
                _logger.Error("Error connecting to LDAP server: " + e);
                throw e;
            }
        }

        /// <summary>
        /// To Check whether file exists in LDAP or Not. 
        /// </summary>
        /// <returns></returns>
        public static bool FileExists(string fileName)
        {
            try
            {
                fileName = Path.GetFileName(fileName);
                string filter = "(cn=" + fileName + ")";
                SearchRequest request = new SearchRequest
                {
                    DistinguishedName = groupDN,
                    Filter = filter,
                    Scope = System.DirectoryServices.Protocols.SearchScope.Subtree,
                };

                LdapConnection ldapConnection = Connect();

                if (ldapConnection != null)
                {
                    SearchResponse response = (SearchResponse)ldapConnection.SendRequest(request);
                    UTF8Encoding utf8 = new UTF8Encoding(false, true);

                    if (response.Entries.Count > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                _logger.Error("Error checking file in LDAP server: " + e);
                throw e;
            }
        }

        public static List<string> GetAllGroups(string userName)
        {
            List<string> lstgroups = new List<string>();
            try
            {
                GetAppSettings();
                SearchRequest request = new SearchRequest
                {
                    DistinguishedName = "ou=groups,o=iringtools,dc=iringug,dc=org",
                    // Filter = filter,
                    Scope = System.DirectoryServices.Protocols.SearchScope.Subtree,
                };

                LdapConnection ldapConnection = Connect();

                if (ldapConnection != null)
                {
                    SearchResponse response = (SearchResponse)ldapConnection.SendRequest(request);
                    UTF8Encoding utf8 = new UTF8Encoding(false, true);

                    if (response.Entries.Count > 0)
                    {
                        SearchResultEntryCollection entries = response.Entries;
                        for (int i = 0; i < entries.Count; i++)
                        {
                            SearchResultEntry entry = entries[i];

                            if (entry.Attributes["member"] != null)
                            {
                                DirectoryAttribute attrib = entry.Attributes["member"];
                                string[] names = null;
                                for (int ic = 0; ic < attrib.Count; ic++)
                                {
                                    if ((attrib[ic] as string).Contains(userName))
                                    {
                                        if (entry.DistinguishedName.Contains(","))
                                            names = entry.DistinguishedName.Split(',');
                                        else
                                        {
                                            _logger.Error("Please check the group name in LDAP:" + entry.DistinguishedName);
                                            return null;
                                        }
                                        string groupName = names.First().Substring(names.First().IndexOf("=") + 1);
                                        lstgroups.Add(groupName);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                return lstgroups;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in getting groups from LDAP server for the current user: " + ex);
                throw ex;
            }
        }
    }
}
    
