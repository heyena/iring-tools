using MappingEditor.Views.Main;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using ModuleLibrary.LayerBLL;
using ModuleLibrary.LayerDAL;
using OntologyService.Interface;
using OntologyService.Interface.PresentationModels;
using PrismContrib.Base;
using PrismContrib.Loggers;
using InformationModel.Views.MainRegion.Main;
using InformationModel.Views.MainRegion.RefDataBrowser;
using InformationModel.Views.MenuRegion;
using InformationModel.Views.StatusRegion;
using InformationModel.Views.MenuRegionRight;
using InformationModel.Views.MESearchRegion;
using InformationModel.Views.MEDataSourceRegion;
using InformationModel.Views.MENavigationRegion;
using InformationModel.Views.MEPinnedRegion;
using InformationModel.Views.ClassDetails;
using InformationModel.Views.MEDataDetailRegion;
using InformationModel.UserControls;
using org.ids_adi.iring.referenceData;
//using InformationModel.Views._MappingEditor.MESearchRightRegion;

namespace InformationModel
{
  public class InformationModelModule : ModuleBase
  {
    /// <summary>
    /// Registers the views and services.
    /// </summary>
    public override void RegisterViewsAndServices()
    {
      Container
          .RegisterType<IWorkingSpinner, WorkingSpinner>(
              new ContainerControlledLifetimeManager())
          .RegisterType<ILoggerFacade, DebugLogger>()
          .RegisterType<IIMPresentationModel, IMPresentationModel>(
              new ContainerControlledLifetimeManager())

          .RegisterType<IAdapter, AdapterBLL>(
              new ContainerControlledLifetimeManager()) // Singleton
          .RegisterType<IAdapter, AdapterDAL>("AdapterProxyDAL",
              new ContainerControlledLifetimeManager()) // Singleton

          .RegisterType<IReferenceData, ReferenceDataBLL>(
              new ContainerControlledLifetimeManager()) // Singleton
          .RegisterType<IReferenceData, ReferenceDataDAL>("ReferenceDataDAL",
              new ContainerControlledLifetimeManager())  // Singleton

          .RegisterType<IMappingEditorView, MappingEditorView>()
          .RegisterType<IRefDataBrowserView, RefDataBrowserView>()

          .RegisterType<IMainView, MainView>()
          .RegisterType<IMenuView, MenuView>()
          .RegisterType<ILoginView, LoginView>()
          .RegisterType<IStatusView, StatusView>()

          .RegisterType<IDataSourceTreeView,DataSourceTreeView>()
          .RegisterType<IMESearchView, MESearchView>()
          .RegisterType<INavigationTreeView,NavigationTreeView>()
          .RegisterType<IMappingView, MappingView>()
          .RegisterType<IPinnedView, PinnedView>()
          .RegisterType<IDetailView, DetailView>()

          // Concrete classes for the InformationModelTreeFactory
          // Note: each string corresponds to the CompletedEventType
          //       that it applies to in ModuleLibrary.Silverlight\Types
          //.RegisterType<IInformationModelConcrete, IMSearchResults>("Search")
          //.RegisterType<IInformationModelConcrete, IMGetClass>("GetClass")
          //.RegisterType<IInformationModelConcrete, IMGetTemplate>("GetTemplate")
          //.RegisterType<IInformationModelConcrete, IMGetClassTemplate>("GetClassTemplates")
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
      RegionManager.RegisterViewWithRegion("MenuRegion",
          () => Container.Resolve<MenuPresenter>().View);

      RegionManager.RegisterViewWithRegion("MenuRegionRight",
          () => Container.Resolve<LoginViewPresenter>().View);

      RegionManager.RegisterViewWithRegion("MainRegion",
          () => Container.Resolve<MappingEditorPresenter>().View);

      RegionManager.RegisterViewWithRegion("MainRegion",
          () => Container.Resolve<MainPresenter>().View);

      RegionManager.RegisterViewWithRegion("MainRegion",
          () => Container.Resolve<RefDataBrowserPresenter>().View);

      RegionManager.RegisterViewWithRegion("StatusRegion",
        () => Container.Resolve<StatusPresenter>().View);

      RegionManager.RegisterViewWithRegion("MEDataSourceRegion",
          () => Container.Resolve<DataSourceTreePresenter>().View);

      RegionManager.RegisterViewWithRegion("MESearchRegion",
          () => Container.Resolve<MESearchPresenter>().View);

      //RegionManager.RegisterViewWithRegion("MESearchRightRegion",
      //    () => Container.Resolve<ToolButtonPresenter>().View);

      RegionManager.RegisterViewWithRegion("MENavigationRegion",
          () => Container.Resolve<NavigationTreePresenter>().View);

      RegionManager.RegisterViewWithRegion("MEMappingRegion",
          () => Container.Resolve<MappingPresenter>().View);

      RegionManager.RegisterViewWithRegion("MEPinnedRegion",
    () => Container.Resolve<PinnedPresenter>().View);

      RegionManager.RegisterViewWithRegion("MEDetailRegion",
          () => Container.Resolve<DetailPresenter>().View);

    }
  }
}
