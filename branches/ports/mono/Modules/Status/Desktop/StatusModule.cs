﻿using PrismContrib.Base;
using PrismContrib.Loggers;

using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;

using org.iringtools.modulelibrary.layerbll;
using org.iringtools.modulelibrary.layerdal;

using org.iringtools.modules.status.statusregion;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;

using org.ids_adi.iring.referenceData;

namespace org.iringtools.modules.status
{
    public class StatusModule : ModuleBase
    {
        /// <summary>
        /// Registers the views and services.
        /// </summary>
        public override void RegisterViewsAndServices()
        {
            Container
              .RegisterType<IIMPresentationModel, IMPresentationModel>(new ContainerControlledLifetimeManager())
              .RegisterType<IStatusView, StatusView>()
              .RegisterType<IReferenceData, ReferenceDataBLL>(new ContainerControlledLifetimeManager()) // Singleton
              .RegisterType<IReferenceData, ReferenceDataDAL>("ReferenceDataDAL", new ContainerControlledLifetimeManager())  // Singleton
              .RegisterType<IAdapter, AdapterBLL>(new ContainerControlledLifetimeManager()) // Singleton
                .RegisterType<IAdapter, AdapterDAL>("AdapterProxyDAL", new ContainerControlledLifetimeManager()) // Singleton
            ;
        }

        /// <summary>
        /// The following blog discusses regions:
        /// http://www.global-webnet.net/blogengine/post/2008/12/18/Silverlight-CompositeWPF-Prism-regions.aspx
        /// 
        /// Registers the types for pull based composition.
        /// </summary>
        public override void RegisterTypesForPullBasedComposition()
        {
            RegionManager.RegisterViewWithRegion("StatusRegion", () => Container.Resolve<StatusPresenter>().View);
        }
    }
}
