using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using ModuleLibrary.LayerBLL;
using ModuleLibrary.LayerDAL;
using OntologyService.Interface;
using OntologyService.Interface.PresentationModels;
using PrismContrib.Base;
using PrismContrib.Loggers;
using Microsoft.Practices.Composite.Modularity;
using org.ids_adi.iring.referenceData;
using Modules.Menu.MenuRegion;

namespace Modules.Menu
{
    public class MenuModule : ModuleBase
    {
        /// <summary>
        /// Registers the views and services.
        /// </summary>
        public override void RegisterViewsAndServices()
        {
            Container
                .RegisterType<IIMPresentationModel, IMPresentationModel>(new ContainerControlledLifetimeManager())
                .RegisterType<IMenuView, MenuView>();
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
        }
    }
}
