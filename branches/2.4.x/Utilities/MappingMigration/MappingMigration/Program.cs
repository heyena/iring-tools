using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using org.iringtools.utility;
using org.ids_adi.qmxf;
using VDS.RDF;
using System.IO;
using log4net;


namespace MappingMigration
{


    internal class Program
    {
        private static Dictionary<string, QMXF> _templates = new Dictionary<string, QMXF>();
        private static Dictionary<string, int> _report = new Dictionary<string, int>();

        private static string _mappingFolderPath = String.Empty;
        private static string _refdataServiceUri = String.Empty;
        private static bool _fixErrors = false;
        
        private static string _proxyHost = String.Empty;
        private static string _proxyPort = String.Empty;
        private static string _proxyCredentials = String.Empty;
        private static XNamespace _ns = String.Empty;

        private static WebHttpClient _refdataClient = null;
        private static NamespaceMapper _nsMap;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            try
            {
                if (Initialize(args))
                {
                    string encryptedCredetials = System.Configuration.ConfigurationManager.AppSettings["ProxyCredentialToken"];
                    WebCredentials proxyCredentials = new WebCredentials(encryptedCredetials);
                    proxyCredentials.Decrypt();

                    string proxyHost = System.Configuration.ConfigurationManager.AppSettings["ProxyHost"];
                    
                    int proxyPort = 0;
                    string proxyPortString = System.Configuration.ConfigurationManager.AppSettings["ProxyPort"];
                    int.TryParse(proxyPortString, out proxyPort);
                    
                    _refdataClient = new WebHttpClient(_refdataServiceUri, proxyCredentials.userName, proxyCredentials.password, proxyCredentials.domain, proxyHost, proxyPort);
                    foreach (var path in Directory.GetFiles(_mappingFolderPath))
                    {
                        string fileName = System.IO.Path.GetFileName(path);

                        if (!fileName.ToUpper().StartsWith("MAPPING."))
                            continue;

                        try
                        {
                            _logger.InfoFormat("Processing file: {0}", fileName);
                            Console.WriteLine("\nProcessing file: {0}\n", fileName);
                            ProcessFile(path);
                        }
                        catch(Exception e)
                        {
                            _logger.ErrorFormat("Error in processing file: {0}", fileName);
                            Console.WriteLine("\nError in processing file: {0}", fileName);
                            _logger.Error(e.Message);
                            //Console.WriteLine(e.Message);
                            _logger.Info(e.StackTrace);
                            Console.WriteLine("\n Press any key to continue.");
                            Console.ReadLine();
                            
                        }
                    }

                    foreach (KeyValuePair<string, int> file in _report)
                    {
                        _logger.InfoFormat("File: {0}\t Bad Templates: {1}", file.Key, file.Value);
                        Console.WriteLine("File: {0}\t Bad Templates: {1}", file.Key, file.Value);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("\n {0}",ex.Message);
            }
            Console.WriteLine("\n Press any key to continue.");
            Console.ReadKey();
        }


        private static void ProcessFile(string filePath)
        {
            XDocument document = XDocument.Load(filePath);
            var items = from r in document.Descendants(_ns + "templateMap")
                        select r;

            foreach (var template in items)
            {
                var id = template.Element(_ns + "id").Value.Split(new char[] { ':' });
                var templateName = template.Element(_ns + "name").Value;

                if (id.Length == 2)
                {
                    QMXF response = null;

                    if (!_templates.ContainsKey(id[1].ToString()))
                    {
                        response = _refdataClient.Get<QMXF>("/templates/" + id[1]);

                        _templates.Add(id[1].ToString(), response);      
                    }
                    else
                    {
                        response = _templates[id[1].ToString()];
                    }

                    if (response.templateQualifications.Count > 0)
                    {
                        bool isTemplateLogged = false;

                        var roles = template.Element(_ns + "roleMaps").Elements(_ns + "roleMap");

                        foreach (var role in roles)
                        {
                            string roleId = role.Element(_ns + "id").Value;
                            string rolename = role.Element(_ns + "name").Value;
                            string newRoleId = String.Empty;

                            var roleList = (from i in response.templateQualifications[0].roleQualification
                                            where i.name.Exists(x => x.value.ToUpper().Trim() == rolename.ToUpper().Trim())
                                            select i).ToList();

                            if (roleList.Count > 0)
                            {
                                string qId = string.Empty;
                                bool qn = _nsMap.ReduceToQName(roleList[0].qualifies, out qId);

                                newRoleId = qId;

                                if (newRoleId != roleId)
                                {
                                    if (_report.ContainsKey(filePath))
                                    {
                                        _report[filePath]++;
                                    }
                                    else
                                    {
                                        _report.Add(filePath, 1);
                                    }

                                    if (!isTemplateLogged)
                                    {
                                        _logger.InfoFormat("Processing template: {0}", templateName);
                                        Console.WriteLine("\nProcessing template: {0}", templateName);
                                        isTemplateLogged = true;
                                    }
                                    role.Element(_ns + "id").Value = newRoleId;
                                    _logger.InfoFormat("Role Name: {0}", rolename);
                                    _logger.InfoFormat("Role id:     {0}", roleId);
                                    _logger.InfoFormat("New Role id: {0}", newRoleId);

                                    Console.WriteLine("\tRole Name: {0}", rolename);
                                    Console.WriteLine("\tRole id:     {0}", roleId);
                                    Console.WriteLine("\tNew Role id: {0}", newRoleId);

                                }
                            }
                            else
                            {
                                if (!isTemplateLogged)
                                {
                                    _logger.InfoFormat("Processing template: {0}", templateName);
                                    Console.WriteLine("\nProcessing template: {0}", templateName);
                                    isTemplateLogged = true;
                                }

                                _logger.InfoFormat("Role not found: {0}", rolename);
                                Console.WriteLine("\tRole not found: {0}", rolename);
                            }
                        }
                    }
                    else
                    {
                        _logger.InfoFormat("Template not found: {0}", templateName);
                        Console.WriteLine("\tTemplate not found: {0}", templateName);
                    }
                }
            }

            if (_fixErrors)
                document.Save(filePath);
        }
        private static bool Initialize(string[] args)
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();

                _mappingFolderPath = System.Configuration.ConfigurationManager.AppSettings["MappingFolderPath"];
                _refdataServiceUri = System.Configuration.ConfigurationManager.AppSettings["RefdataServiceUri"];
                Boolean.TryParse(System.Configuration.ConfigurationManager.AppSettings["FixErrors"].ToString(), out _fixErrors);

                _ns = @"http://www.iringtools.org/mapping";

                _nsMap = new NamespaceMapper();
                _nsMap.AddNamespace("eg", new Uri("http://example.org/data#"));
                _nsMap.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
                _nsMap.AddNamespace("rdl", new Uri("http://rdl.rdlfacade.org/data#"));
                _nsMap.AddNamespace("tpl", new Uri("http://tpl.rdlfacade.org/data#"));
                _nsMap.AddNamespace("dm", new Uri("http://dm.rdlfacade.org/data#"));
                _nsMap.AddNamespace("p8dm", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/data-model#"));
                _nsMap.AddNamespace("owl2xml", new Uri("http://www.w3.org/2006/12/owl2-xml#"));
                _nsMap.AddNamespace("p8", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/template-model#"));
                _nsMap.AddNamespace("templates", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/templates#"));

                return true;
            }
            catch
            {
                Console.WriteLine("Error in initializing...");
                return false;
            }
        }
    }

}
