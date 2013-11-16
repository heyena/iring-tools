using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;
using log4net;
using org.iringtools.adapter.datalayer;
using org.iringtools.library;
using System.IO;
using org.iringtools.utility;

namespace org.iringtools.adapter
{
    public class Global : HttpApplication
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Global));

        void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes();
            log4net.Config.XmlConfigurator.Configure();

            BuildManager.GetReferencedAssemblies(); // make sure assemblies are loaded even though methods may not have been called yet        
            
            DataLayers dataLayers = GetDataLayers();
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\App_Data\\DataLayers.xml";
            Utility.Write<DataLayers>(dataLayers, path);
        }

        private void RegisterRoutes()
        {
            RouteTable.Routes.Add(new ServiceRoute("refdata", new WebServiceHostFactory(), typeof(org.iringtools.services.ReferenceDataService)));

            RouteTable.Routes.Add(new ServiceRoute("hibernate", new WebServiceHostFactory(), typeof(org.iringtools.services.HibernateService)));

            RouteTable.Routes.Add(new ServiceRoute("adapter", new WebServiceHostFactory(), typeof(org.iringtools.services.AdapterService)));

            RouteTable.Routes.Add(new ServiceRoute("data", new RawServiceHostFactory(), typeof(org.iringtools.services.DataService)));

            RouteTable.Routes.Add(new ServiceRoute("dxfr", new WebServiceHostFactory(), typeof(org.iringtools.services.DataTransferService)));

            RouteTable.Routes.Add(new ServiceRoute("adata", new RawServiceHostFactory(), typeof(org.iringtools.services.AdapterDataService)));

            RouteTable.Routes.Add(new ServiceRoute("abstract", new RawServiceHostFactory(), typeof(org.iringtools.services.AbstractDataService)));
        }

        private DataLayers GetDataLayers()
        {
            DataLayers dataLayers = new DataLayers();

            // Load NHibernate data layer
            Type nhType = typeof(NHibernateDataLayer);
            string nhLibrary = nhType.Assembly.GetName().Name;
            string nhAssembly = string.Format("{0}, {1}", nhType.FullName, nhLibrary);
            DataLayer nhDataLayer = new DataLayer { Assembly = nhAssembly, Name = nhLibrary, Configurable = true };
            dataLayers.Add(nhDataLayer);

            // Load Spreadsheet data layer
            Type ssType = typeof(SpreadsheetDatalayer);
            string ssLibrary = ssType.Assembly.GetName().Name;
            string ssAssembly = string.Format("{0}, {1}", ssType.FullName, ssLibrary);
            DataLayer ssDataLayer = new DataLayer { Assembly = ssAssembly, Name = ssLibrary, Configurable = true };
            dataLayers.Add(ssDataLayer);

            try
            {
                Assembly[] domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                GetDataLayerTypes(ref dataLayers, domainAssemblies);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error loading data layer: {0}" + ex));
            }

            return dataLayers;
        }

        private void GetDataLayerTypes(ref DataLayers dataLayers, Assembly[] domainAssemblies)
        {
            foreach (Assembly asm in domainAssemblies)
            {
                Type[] asmTypes = null;
                try
                {
                    asmTypes = asm.GetTypes().Where(a => a != null && (
                      (typeof(IDataLayer).IsAssignableFrom(a) && !(a.IsInterface || a.IsAbstract)) ||
                      (typeof(ILightweightDataLayer).IsAssignableFrom(a) && !(a.IsInterface || a.IsAbstract)))
                    ).ToArray();
                }
                catch (ReflectionTypeLoadException e)
                {
                    // if we are running the the iRing site under Anonymous authentication with the DefaultApplicationPool Identity we
                    // can run into ReflectionPermission issues but as our datalayer assemblies are in our web site's bin folder we
                    // should be able to ignore the exceptions and work with the accessibe types loaded in e.Types.
                    asmTypes = e.Types.Where(a => a != null && (
                      (typeof(IDataLayer).IsAssignableFrom(a) && !(a.IsInterface || a.IsAbstract)) ||
                      (typeof(ILightweightDataLayer).IsAssignableFrom(a) && !(a.IsInterface || a.IsAbstract)))
                    ).ToArray();
                    _logger.Warn("GetTypes() for " + asm.FullName + " cannot access all types, but datalayer loading is continuing: " + e);
                }

                try
                {
                    if (asmTypes.Any())
                    {
                        _logger.Debug("assembly:" + asm.FullName);
                        foreach (System.Type asmType in asmTypes)
                        {
                            _logger.Debug("asmType:" + asmType.ToString());

                            bool isLW = typeof(ILightweightDataLayer).IsAssignableFrom(asmType) && !(asmType.IsInterface || asmType.IsAbstract);
                            bool configurable = asmType.BaseType.Equals(typeof(BaseConfigurableDataLayer));
                            string name = asm.FullName.Split(',')[0];

                            if (name.ToLower() == "NHibernateExtension".ToLower())
                                continue;

                            if (!dataLayers.Exists(x => x.Name.ToLower() == name.ToLower()))
                            {
                                string assembly = string.Format("{0}, {1}", asmType.FullName, name);

                                DataLayer dataLayer = new DataLayer
                                {
                                    Assembly = assembly,
                                    Name = name,
                                    IsLightweight = isLW,
                                    Configurable = configurable
                                };

                                dataLayers.Add(dataLayer);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("Error loading data layer (while getting assemblies): " + e);
                }
            }
        }
    }
}
