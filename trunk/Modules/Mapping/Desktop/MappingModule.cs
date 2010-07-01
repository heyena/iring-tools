﻿using PrismContrib.Base;
using PrismContrib.Loggers;

using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;

using org.iringtools.modulelibrary.layerbll;
using org.iringtools.modulelibrary.layerdal;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;
#if SILVERLIGHT
using org.iringtools.modules.projectapplicationregion;
#endif
using org.iringtools.modules.medatasourceregion;
using org.iringtools.modules.memappingregion;
using org.iringtools.modules.mainregion;

namespace org.iringtools.modules
{
  public class MappingModule : ModuleBase
  {
    /// <summary>
    /// Registers the views and services.
    /// </summary>
    public override void RegisterViewsAndServices()
    {
      Container
          .RegisterType<ILoggerFacade, DebugLogger>()
          .RegisterType<IIMPresentationModel, IMPresentationModel>(new ContainerControlledLifetimeManager())
          .RegisterType<IAdapter, AdapterBLL>(new ContainerControlledLifetimeManager()) // Singleton
          .RegisterType<IAdapter, AdapterDAL>("AdapterProxyDAL", new ContainerControlledLifetimeManager()) // Singleton
          .RegisterType<IReferenceData, ReferenceDataBLL>(new ContainerControlledLifetimeManager()) // Singleton
          .RegisterType<IReferenceData, ReferenceDataDAL>("ReferenceDataDAL", new ContainerControlledLifetimeManager())  // Singleton
#if SILVERLIGHT
          .RegisterType<IProjectApplicationView, ProjectApplicationView>()
#endif
          .RegisterType<IDataSourceTreeView, DataSourceTreeView>()
          .RegisterType<IMappingView, MappingView>()
          .RegisterType<IMappingEditorView, MappingEditorView>()
      ;
    }

    /// <summary>
    /// The following blog discusses regions:
    /// http://www.global-webnet.net/blogengine/post/2008/12/18/Silverlight-CompositeWPF-Prism-regions.aspx
    /// /
    /// Registers the types for pull based composition.
    /// </summary>
    public override void RegisterTypesForPullBasedComposition()
    {
#if SILVERLIGHT
      RegionManager.RegisterViewWithRegion("ProjAppRegion", () => Container.Resolve<ProjectApplicationPresenter>().View);
#endif
      RegionManager.RegisterViewWithRegion("MEDataSourceRegion", () => Container.Resolve<DataSourceTreePresenter>().View);
      RegionManager.RegisterViewWithRegion("MEMappingRegion", () => Container.Resolve<MappingPresenter>().View);
      RegionManager.RegisterViewWithRegion("MainRegion", () => Container.Resolve<MappingEditorPresenter>().View);
    }
  }
}
