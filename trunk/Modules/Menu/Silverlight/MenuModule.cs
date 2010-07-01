using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;
using org.iringtools.modulelibrary.layerbll;
using org.iringtools.modulelibrary.layerdal;
using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;
using PrismContrib.Base;
using PrismContrib.Loggers;
using Microsoft.Practices.Composite.Modularity;
using org.iringtools.modules.menu.menuregion;

namespace org.iringtools.modules.menu
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
