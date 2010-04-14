using PrismContrib.Base;
using PrismContrib.Loggers;

using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;

using ModuleLibrary.LayerBLL;
using ModuleLibrary.LayerDAL;

using OntologyService.Interface;
using OntologyService.Interface.PresentationModels;

using org.ids_adi.iring.referenceData;

using Modules.Popup.PopupRegion;

namespace Modules.Popup
{
    public class PopupModule : ModuleBase
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
                .RegisterType<IReferenceData, ReferenceDataDAL>("ReferenceDataDAL",new ContainerControlledLifetimeManager())  // Singleton                
                .RegisterType<IPopupView, PopupView>()
            ;
        }
      
        public override void RegisterTypesForPullBasedComposition()
        {
            RegionManager.RegisterViewWithRegion("OverlayRegion", () => Container.Resolve<PopupPresenter>().View);           
        }
    }
}
