using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Composite.Modularity;
using PrismContrib.Base;
using Microsoft.Practices.Composite.Logging;
using PrismContrib.Loggers;
using OntologyService.Interface.PresentationModels;
using OntologyService.Interface;
using Microsoft.Practices.Unity;
using ModuleLibrary.LayerBLL;
using ModuleLibrary.LayerDAL;

namespace InformationModel
{
    /// <summary>
    /// Mock InformationModelModule
    /// </summary>
    public class InformationModelModule : ModuleBase
    {
        public override void RegisterViewsAndServices()
        {
            Container
                .RegisterType<ILoggerFacade, DebugLogger>()
                .RegisterType<IIMPresentationModel, IMPresentationModel>(
                    new ContainerControlledLifetimeManager())

                .RegisterType<IAdapter, AdapterBLL>(
                    new ContainerControlledLifetimeManager()) // Singleton
                .RegisterType<IAdapter, AdapterDAL>("AdapterProxyDAL",
                    new ContainerControlledLifetimeManager()) // Singleton

                .RegisterType<IReferenceData, ReferenceDataBLL>()
                .RegisterType<IReferenceData, ReferenceDataDAL>("ReferenceDataDAL");

        
        }


        public override void RegisterTypesForPullBasedComposition()
        {
        }
    }
}
