using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using org.iringtools.utility;
using org.ids_adi.qmxf;
using VDS.RDF;
using System.IO;


namespace MappingMigration
{


    internal class Program
    {
        private static string _mappingFolderPath = String.Empty;
        private static string _refdataServiceUri = String.Empty;
        
        private static string _proxyHost = String.Empty;
        private static string _proxyPort = String.Empty;
        private static string _proxyCredentials = String.Empty;
        private static XNamespace _ns = String.Empty;

        private static WebHttpClient _refdataClient = null;
        private static NamespaceMapper _nsMap;

        static void Main(string[] args)
        {
            if (Initialize(args))
            {
                _refdataClient = new WebHttpClient(_refdataServiceUri);
                foreach (var path in Directory.GetFiles(_mappingFolderPath))
                {
                    string fileName = System.IO.Path.GetFileName(path);
                    
                    if (!fileName.ToUpper().StartsWith("MAPPING."))
                        continue;

                    try
                    {
                        Console.WriteLine("Processing file: {0}\n", fileName);
                        ProcessFile(path);
                    }
                    catch
                    {
                        Console.WriteLine("Error in processing file: {0}", fileName);
                    }
                }
            
            }

            Console.WriteLine("Completed....\n Press any key to continue.");
            Console.ReadLine();
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
                    var response = _refdataClient.Get<QMXF>("/templates/" + id[1]);

                    if (response.templateQualifications.Count > 0)
                    {
                        Console.WriteLine("Processing template: {0}", templateName);
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

                                role.Element(_ns + "id").Value = newRoleId;
                                Console.WriteLine("\tRole Name: {0}", rolename);
                                Console.WriteLine("\tRole id:     {0}", roleId);
                                Console.WriteLine("\tNew Role id: {0}\n", newRoleId);                                
                            }
                            else
                            {
                                Console.WriteLine("\tRole not found");
                            }
                        }
                    }

                }
            }
            document.Save(filePath);
        }
        private static bool Initialize(string[] args)
        {
            try
            {
                _mappingFolderPath = System.Configuration.ConfigurationManager.AppSettings["MappingFolderPath"];
                _refdataServiceUri = System.Configuration.ConfigurationManager.AppSettings["RefdataServiceUri"];

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
