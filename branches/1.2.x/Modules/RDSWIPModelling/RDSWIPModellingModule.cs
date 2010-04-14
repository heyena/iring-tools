//using MappingEditor.Views.Main;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using ModuleLibrary.LayerBLL;
using ModuleLibrary.LayerDAL;
using OntologyService.Interface;
using OntologyService.Interface.PresentationModels;
using PrismContrib.Base;
using PrismContrib.Loggers;
//using InformationModel.Views.MEDataSourceRegion;
//using InformationModel.Views.MEDataDetailRegion;
//using SpinnerModule;
using Microsoft.Practices.Composite.Modularity;
//using org.ids_adi.iring.referenceData;
using RDSWIPModelling.MainRegion.RDSWIPEditor;
using Search.Views.MESearchRegion;
using Detail.Views.DetailsRegion;
using Menu.Views.MenuRegion;
//using RDSWIPModelling.MENavigationRegion;

namespace RDSWIPModelling
{    
    public class RDSWIPModellingModule : ModuleBase
    {
        /// <summary>
        /// Registers the views and services.
        /// </summary>
        /// 
        public override void RegisterViewsAndServices()
        {
            Container

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

          .RegisterType<IRDSWIPEditorView, RDSWIPEditorView>()
          .RegisterType<IMenuView, MenuView>();

          //.RegisterType<IMESearchView, MESearchView>()
          //.RegisterType<IDetailView, DetailView>()
          //.RegisterType<INavigationTreeView, NavigationTreeView>()
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
            RegionManager.RegisterViewWithRegion("MainRegion",
                () => Container.Resolve<RDSWIPEditorPresenter>().View);

            RegionManager.RegisterViewWithRegion("MenuRegion",
                () => Container.Resolve<MenuPresenter>().View);

            //RegionManager.RegisterViewWithRegion("MESearchRegion",
            //    () => Container.Resolve<MESearchPresenter>().View);

            //RegionManager.RegisterViewWithRegion("MEDetailRegion",
            //    () => Container.Resolve<DetailPresenter>().View);

            //RegionManager.RegisterViewWithRegion("MENavigationRegion",
            //    () => Container.Resolve<NavigationTreePresenter>().View);
        }
    }
}
