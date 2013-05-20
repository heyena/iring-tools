using PrismContrib.Base;
using PrismContrib.Loggers;

using Microsoft.Practices.Composite.Modularity;
using Microsoft.Practices.Composite.Logging;
using Microsoft.Practices.Composite.Regions;
using Microsoft.Practices.Unity;

using org.iringtools.modulelibrary.layerbll;
using org.iringtools.modulelibrary.layerdal;

using org.iringtools.ontologyservice.presentation;
using org.iringtools.ontologyservice.presentation.presentationmodels;

namespace org.iringtools.modules.spinner
{
    public class SpinnerModule : ModuleBase
    {
        /// <summary>
        /// Registers the views and services.
        /// </summary>
        public override void RegisterViewsAndServices()
        {
            Container
                .RegisterType<IWorkingSpinner, WorkingSpinner>(new ContainerControlledLifetimeManager());
            ;
        }

        public override void RegisterTypesForPullBasedComposition()
        {
            RegionManager.RegisterViewWithRegion("SpinnerRegion", () => Container.Resolve<WorkingSpinner>());
        }
    }
}
