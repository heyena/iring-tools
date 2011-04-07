using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using Ninject;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;


namespace iRINGTools.Web.Models
{
    public class AdapterRepository : IAdapterRepository
    {
        private NameValueCollection _settings = null;
        private WebHttpClient _client = null;
        private string _refDataServiceURI = string.Empty;

        [Inject]
        public AdapterRepository()
        {
            _settings = ConfigurationManager.AppSettings;
            _client = new WebHttpClient(_settings["AdapterServiceUri"]);
        }

        public ScopeProjects GetScopes()
        {
            ScopeProjects obj = null;

            try
            {
                obj = _client.Get<ScopeProjects>("/scopes");
            }
            catch (Exception ex)
            {
            }

            return obj;

        }

        public string PostScopes(ScopeProjects scopes)
        {
            string obj = null;

            try
            {
                obj = _client.Post<ScopeProjects>("/scopes", scopes, true);
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public DataLayers GetDataLayers()
        {
            DataLayers obj = null;

            try
            {
                obj = _client.Get<DataLayers>("/datalayers");
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public ScopeProject GetScope(string scopeName)
        {
            ScopeProjects scopes = GetScopes();

            return scopes.FirstOrDefault<ScopeProject>(o => o.Name == scopeName);
        }

        public ScopeApplication GetScopeApplication(string scopeName, string applicationName)
        {
            ScopeProject scope = GetScope(scopeName);

            ScopeApplication obj = null;

            try
            {
                obj = scope.Applications.FirstOrDefault<ScopeApplication>(o => o.Name == applicationName);
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public DataDictionary GetDictionary(string scopeName, string applicationName)
        {
            DataDictionary obj = null;

            try
            {
                obj = _client.Get<DataDictionary>(String.Format("/{0}/{1}/dictionary", scopeName, applicationName), true);
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public Mapping GetMapping(string scopeName, string applicationName)
        {
            Mapping obj = null;

            try
            {
                obj = _client.Get<Mapping>(String.Format("/{0}/{1}/mapping", scopeName, applicationName), true);
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public XElement GetBinding(string scope, string application)
        {
            XElement obj = null;

            try
            {
                obj = _client.Get<XElement>(String.Format("/{0}/{1}/binding", scope, application), true);
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public string UpdateBinding(string scope, string application, string dataLayer)
        {
            string obj = null;

            try
            {
                XElement binding = new XElement("module",
            new XAttribute("name", string.Format("{0}.{1}", scope, application)),
              new XElement("bind",
                new XAttribute("name", "DataLayer"),
                new XAttribute("service", "org.iringtools.library.IDataLayer2, iRINGLibrary"),
                new XAttribute("to", dataLayer)
              )
            );

                obj = _client.Post<XElement>(String.Format("/{0}/{1}/binding", scope, application), binding, true);

            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public DataLayer GetDataLayer(string scopeName, string applicationName)
        {
            XElement binding = GetBinding(scopeName, applicationName);

            DataLayer dataLayer = null;

            if (binding != null)
            {
                dataLayer = new DataLayer();
                dataLayer.Assembly = binding.Element("bind").Attribute("to").Value;
                dataLayer.Name = binding.Element("bind").Attribute("to").Value.Split(',')[1].Trim();
            }

            return dataLayer;
        }

        public string AddScope(ScopeProject scope)
        {
            ScopeProjects scopes = GetScopes();
            scopes.Add(scope);

            return PostScopes(scopes);
        }

        public string UpdateScope(string scopeName, string name, string description)
        {
            ScopeProjects scopes = GetScopes();
            ScopeProject scope = scopes.FirstOrDefault<ScopeProject>(o => o.Name == scopeName);

            if (scope == null)
            {
                scope = new ScopeProject()
                {
                    Name = name,
                    Description = description,
                    Applications = new ScopeApplications()
                };
                scopes.Add(scope);
            }
            else
            {
                scope.Name = name;
                scope.Description = description;
            }
            return PostScopes(scopes);
        }

        public string DeleteScope(string scopeName)
        {
            ScopeProjects scopes = GetScopes();
            ScopeProject scope = scopes.FirstOrDefault<ScopeProject>(o => o.Name == scopeName);

            scopes.Remove(scope);

            return PostScopes(scopes);
        }

        public string UpdateApplication(string scopeName, string applicationName, string name, string description, string assembly)
        {
            ScopeProjects scopes = GetScopes();
            ScopeProject scope = scopes.FirstOrDefault<ScopeProject>(o => o.Name == scopeName);
            ScopeApplication application = scope.Applications.FirstOrDefault<ScopeApplication>(o => o.Name == applicationName);

            if (scope != null)
            {
                if (application == null)
                {
                    application = new ScopeApplication()
                    {
                        Name = name,
                        Description = description
                    };

                    scope.Applications.Add(application);
                }
                else
                {
                    application.Name = name;
                    application.Description = description;
                }
            }
            //UpdateBinding--
            UpdateBinding(scopeName, applicationName, assembly);
            return PostScopes(scopes);
        }

        public string DeleteApplication(string scopeName, string applicationName)
        {
            ScopeProjects scopes = GetScopes();
            ScopeProject scope = scopes.FirstOrDefault<ScopeProject>(o => o.Name == scopeName);
            ScopeApplication application = scope.Applications.FirstOrDefault<ScopeApplication>(o => o.Name == applicationName);

            scope.Applications.Remove(application);

            return PostScopes(scopes);
        }

        public DataProviders GetDataProviders()
        {
            WebHttpClient client = new WebHttpClient(_settings["NHibernateServiceURI"]);
            return client.Get<DataProviders>("/providers", true);
        }
    }
}